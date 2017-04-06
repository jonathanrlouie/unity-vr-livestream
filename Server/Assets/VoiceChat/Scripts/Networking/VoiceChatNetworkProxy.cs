using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

using System.Net;
using System.Net.Sockets;

namespace VoiceChat.Networking {
    public class VoiceChatNetworkProxy : NetworkBehaviour{
        public delegate void MessageHandler<T>(T data);
        public static event MessageHandler<VoiceChatPacketMessage> VoiceChatPacketReceived;
        public static event System.Action<VoiceChatNetworkProxy> ProxyStarted;

        private const string ProxyPrefabPath = "VoiceChat_NetworkProxy";
        private static GameObject proxyPrefab;
        private static int localProxyId;
        private static Dictionary<int, GameObject> proxies = new Dictionary<int, GameObject>();

        //public bool isMine { get { return networkId != 0 && networkId == localProxyId; } }
        public bool isMine { get { return networkId == localProxyId; } }

		// roles password to roles
		private Dictionary<string, string> accounts = new Dictionary<string, string>();
		private Dictionary<string, string> ipToRole = new Dictionary<string, string> ();

		private string clientIPAddress;
		private string clientPassword;
		private string clientRole;

        [SyncVar]
        private int networkId;

        VoiceChatPlayer player = null;

        void Start() {
			clientIPAddress = GetClientIPAddress ();
            if (isMine)
            {
                if (LogFilter.logDebug)
                {
                    Debug.Log("Setting VoiceChat recorder NetworkId.");
                }

                VoiceChatRecorder.Instance.NewSample += OnNewSample;
                VoiceChatRecorder.Instance.NetworkId = networkId;
            }
            else
            {
                VoiceChatPacketReceived += OnReceivePacket;
            }

            if (isClient && (!isMine || VoiceChatSettings.Instance.LocalDebug))
            {
                gameObject.AddComponent<AudioSource>();
                player = gameObject.AddComponent<VoiceChatPlayer>();
            }

            if (ProxyStarted != null)
            {
                ProxyStarted(this);
            }
        }

        void OnDestroy()
        {
            if (VoiceChatRecorder.Instance != null)
                VoiceChatRecorder.Instance.NewSample -= OnNewSample;
            VoiceChatPacketReceived -= OnReceivePacket;
        }

        private void OnReceivePacket(VoiceChatPacketMessage data)
        {

            //if (LogFilter.logDebug)
            //{
            //    Debug.Log("Received a new Voice Sample. Playing!");
            //}

            if (data.proxyId == networkId)
            {
                player.OnNewSample(data.packet);
            }
        }

        void OnNewSample(VoiceChatPacket packet)
        {
            var packetMessage = new VoiceChatPacketMessage {
                proxyId = (short)localProxyId,
                packet = packet,
            };

            //if (LogFilter.logDebug)
            //{
            //    Debug.Log("Got a new Voice Sample. Streaming!");
            //}

            NetworkManager.singleton.client.SendUnreliable(VoiceChatMsgType.Packet, packetMessage);
        }



        void SetNetworkId(int networkId)
        {
            var netIdentity = GetComponent<NetworkIdentity>();
            if (netIdentity.isServer || netIdentity.isClient)
            {
                Debug.LogWarning("Can only set NetworkId before spawning");
                return;
            }

            this.networkId = networkId;
            //VoiceChatRecorder.Instance.NetworkId = networkId;
        }

      
        #region NetworkManager Hooks

        public static void OnManagerStartClient(NetworkClient client, GameObject customPrefab = null)
        {
            client.RegisterHandler(VoiceChatMsgType.Packet, OnClientPacketReceived);
            client.RegisterHandler(VoiceChatMsgType.SpawnProxy, OnProxySpawned);


            if (customPrefab == null)
            {
                proxyPrefab = Resources.Load<GameObject>(ProxyPrefabPath);
            }
            else
            {
                proxyPrefab = customPrefab;
            }
            
            ClientScene.RegisterPrefab(proxyPrefab);
        }

        public static void OnManagerStopClient()
        {
            var client = NetworkManager.singleton.client;
            if (client == null) return;

            client.UnregisterHandler(VoiceChatMsgType.Packet);
            client.UnregisterHandler(VoiceChatMsgType.SpawnProxy);
        }

        public static void OnManagerServerDisconnect(NetworkConnection conn)
        {
            var id = conn.connectionId;

            if (!proxies.ContainsKey(id))
            {
                Debug.LogWarning("Proxy destruction requested for client " + id + " but proxy wasn't registered");
                return;
            }

            var proxy = proxies[id];
            NetworkServer.Destroy(proxy);

            proxies.Remove(id);
        }

        public static void OnManagerStartServer()
        {
            NetworkServer.RegisterHandler(VoiceChatMsgType.Packet, OnServerPacketReceived);
            NetworkServer.RegisterHandler(VoiceChatMsgType.RequestProxy, OnProxyRequested);
        }

        public static void OnManagerStopServer()
        {
            NetworkServer.UnregisterHandler(VoiceChatMsgType.Packet);
            NetworkServer.UnregisterHandler(VoiceChatMsgType.RequestProxy);
        }

        public static void OnManagerClientConnect(NetworkConnection connection)
        {
            var client = NetworkManager.singleton.client;
            client.Send(VoiceChatMsgType.RequestProxy, new EmptyMessage());
        }
        
        #endregion

        #region Network Message Handlers

        private static void OnProxyRequested(NetworkMessage netMsg)
        {
            var id = netMsg.conn.connectionId;

            if (LogFilter.logDebug)
            {
                Debug.Log("Proxy Requested by " + id);
            }

            // We need to set the "localProxyId" static variable on the client
            // before the "Start" method of the local proxy is called.
            // On Local Clients, the Start method of a spowned obect is faster than
            // Connection.Send() so we will set the "localProxyId" flag ourselves
            // since we are in the same instance of the game.
            if (id == -1)
            {
                if (LogFilter.logDebug)
                {
                    Debug.Log("Local proxy! Setting local proxy id by hand");
                }

                VoiceChatNetworkProxy.localProxyId = id;
            }
            else
            {
                netMsg.conn.Send(VoiceChatMsgType.SpawnProxy, new IntegerMessage(id));
            }

            var proxy = Instantiate<GameObject>(proxyPrefab);
            proxy.SendMessage("SetNetworkId", id);

            proxies.Add(id, proxy);
            NetworkServer.Spawn(proxy);

        }

        private static void OnProxySpawned(NetworkMessage netMsg)
        {
            localProxyId = netMsg.ReadMessage<IntegerMessage>().value;

            if (LogFilter.logDebug)
            {
                Debug.Log("Proxy spawned for " + localProxyId + ", setting local proxy id.");
            }

        }

        private static void OnServerPacketReceived(NetworkMessage netMsg)
        {
            var data = netMsg.ReadMessage<VoiceChatPacketMessage>();

            foreach (var connection in NetworkServer.connections)
            {
                if (connection == null || connection.connectionId == data.proxyId)
                    continue;

                connection.SendUnreliable(VoiceChatMsgType.Packet, data);
            }

            foreach (var connection in NetworkServer.localConnections)
            {
                if (connection == null || connection.connectionId == data.proxyId)
                    continue;

                connection.SendUnreliable(VoiceChatMsgType.Packet, data);
            }

        }

        private static void OnClientPacketReceived(NetworkMessage netMsg)
        {
            if (VoiceChatPacketReceived != null)
            {
                var data = netMsg.ReadMessage<VoiceChatPacketMessage>();
                VoiceChatPacketReceived(data);
            }
        }
        
        #endregion
	

		#region IP, Pw, and Role

		// get client ip address
		public string GetClientIPAddress() {

			IPHostEntry Host = default(IPHostEntry);
			string Hostname = null;
			Hostname = System.Environment.MachineName;
			Host = Dns.GetHostEntry(Hostname);
			foreach (IPAddress IP in Host.AddressList) {
				if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
					clientIPAddress = Host.AddressList [0].ToString ();
				}
			}
			return clientIPAddress;
		}
		// get client password and get the role
		public string matchPasswordToRole() {
			if (accounts.ContainsKey (clientPassword)) {
				clientRole = accounts [clientPassword];
			} else {
				accounts.Add (clientPassword, "student");
			}
			return clientRole;
		}
		// have password and role, and assign ip to role
		public void assignIpToRole() {
			// ["127.445.435.900", "student"]
			if (ipToRole.ContainsKey (clientIPAddress)) {
				clientRole = ipToRole [clientIPAddress];
			} else {
				ipToRole.Add (clientIPAddress, clientRole);
			}
		}

		#endregion
    }


}