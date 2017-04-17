using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Linq;

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
        // TODO: Get a real database for authentication instead of using this hardcoded map 
		private static Dictionary<string, string> accounts = new Dictionary<string, string>() {
            { "matt", "tutor" },
            { "sally", "tutor" },
            { "jonathan1337", "tutor" }
        };
		private static Dictionary<int, string> proxyIdToRole = new Dictionary<int, string>();

        // maps tutor ids to a list of all student ids that they are currently speaking with
        private static Dictionary<int, List<int>> currentCallMap = new Dictionary<int, List<int>>();

        [SyncVar]
        private int networkId;

        VoiceChatPlayer player = null;

        void Start() {
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

            if (proxyIdToRole.ContainsKey(id))
            {
                proxyIdToRole.Remove(id);
            }
            else
            {
                Debug.LogWarning("Attempt to delete proxy " + id + " but was not assigned a role");
            }

            if (currentCallMap.ContainsKey(id))
            {
                currentCallMap.Remove(id);
            }

            foreach (var entry in currentCallMap)
            {
                if (entry.Value.Contains(id))
                {
                    entry.Value.Remove(id);
                }
            }

            // tell tutors to remove disconnected user from list of connected users
            foreach (var entry in proxyIdToRole)
            {
                if ("tutor".Equals(entry.Value))
                {
                    foreach (var connection in NetworkServer.connections)
                    {
                        if (connection != null && connection.connectionId == entry.Key)
                        {
                            connection.Send(VoiceChatMsgType.RemoveUser, new IntegerMessage(id));
                        }
                     
                    }
                }
            }

        }

        public static void OnManagerStartServer(GameObject customPrefab = null)
        {
            if (customPrefab == null)
            {
                proxyPrefab = Resources.Load<GameObject>(ProxyPrefabPath);
            }
            else
            {
                proxyPrefab = customPrefab;
            }

            NetworkServer.RegisterHandler(VoiceChatMsgType.Packet, OnServerPacketReceived);
            NetworkServer.RegisterHandler(VoiceChatMsgType.RequestProxy, OnProxyRequested);
            NetworkServer.RegisterHandler(VoiceChatMsgType.RequestTutor, OnTutorRequested);
            NetworkServer.RegisterHandler(VoiceChatMsgType.RequestStudent, OnStudentRequested);
            NetworkServer.RegisterHandler(VoiceChatMsgType.RequestStudentDrop, OnStudentDropRequested);
            NetworkServer.RegisterHandler(VoiceChatMsgType.EndCall, OnEndCall);
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

        private static bool AuthPassword(string password, int proxyId)
        {
            // TODO: this is horribly insecure! Make sure that passwords are at least hashed first.
            string role = LookupRole(password);
            AssignProxyToRole(proxyId, role);

            // TODO: use a real authentication workflow that returns false if the client isn't registered in the database
            return true;
        }

        // get client password and get the role
        public static string LookupRole(string password)
        {
            string clientRole = "student";
            if (accounts.ContainsKey(password))
            {
                clientRole = accounts[password];
            }
            return clientRole;
        }

        // have password and role, and assign ip to role
        public static void AssignProxyToRole(int proxyId, string role)
        {
            proxyIdToRole.Add(proxyId, role);
        }

        #region Network Message Handlers

        private static void OnStudentRequested(NetworkMessage netMsg)
        {
            // TODO: check that proxyIdToRole has the id
            if ("tutor".Equals(proxyIdToRole[netMsg.conn.connectionId]))
            {
                var studentProxyId = netMsg.ReadMessage<IntegerMessage>().value;

                if (currentCallMap.ContainsKey(netMsg.conn.connectionId))
                {
                    // only add the student id if it isn't in the call already
                    if (!currentCallMap[netMsg.conn.connectionId].Contains(studentProxyId))
                    {
                        currentCallMap[netMsg.conn.connectionId].Add(studentProxyId);
                    }
                }
                else
                {
                    currentCallMap.Add(netMsg.conn.connectionId, new List<int>(studentProxyId));
                }
            }
            
        }

        private static void OnStudentDropRequested(NetworkMessage netMsg)
        {
            // TODO: check that proxyIdToRole has the id
            if ("tutor".Equals(proxyIdToRole[netMsg.conn.connectionId]))
            {
                var studentProxyId = netMsg.ReadMessage<IntegerMessage>().value;
                
                // short circuit evaluation yo
                if (currentCallMap.ContainsKey(netMsg.conn.connectionId) && currentCallMap[netMsg.conn.connectionId].Count > 0)
                {
                     currentCallMap[netMsg.conn.connectionId].Remove(studentProxyId);
                }
            
            }

        }

        private static void OnEndCall(NetworkMessage netMsg)
        {

            print("hello");

            // TODO: check that proxyIdToRole has the id
            /*if ("tutor".Equals(proxyIdToRole[netMsg.conn.connectionId]))
            {
                
                if (currentCallMap.ContainsKey(netMsg.conn.connectionId))
                {
                    currentCallMap.Remove(netMsg.conn.connectionId);
                }

            }*/
        }

        private static void OnTutorRequested(NetworkMessage netMsg)
        {
            int id = netMsg.conn.connectionId;

            var tutorProxyId = proxyIdToRole.FirstOrDefault(x => "tutor".Equals(x.Value)).Key;

            foreach (var connection in NetworkServer.connections)
            {
                if (connection != null && connection.connectionId == tutorProxyId)
                {
                    connection.Send(VoiceChatMsgType.StudentRequestTutor, new IntegerMessage(id));
                }
            }
        }

        private static void OnProxyRequested(NetworkMessage netMsg)
        {
            string password = netMsg.ReadMessage<StringMessage>().value;

            var id = netMsg.conn.connectionId;

            if (LogFilter.logDebug)
            {
                Debug.Log("Proxy Requested by " + id);
            }

            if (AuthPassword(password, id))
            {
                print(proxyIdToRole[id]);

                var proxy = Instantiate<GameObject>(proxyPrefab);
                proxy.SendMessage("SetNetworkId", id);

                proxies.Add(id, proxy);
                NetworkServer.Spawn(proxy);

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

                    

                    // tell tutors to add new connected user to list of connected users
                    foreach (var entry in proxyIdToRole)
                    {
                        print(entry.Key);
                        // make sure we are not sending our own id to ourselves
                        if ("tutor".Equals(entry.Value) && entry.Key != id)
                        {
                            foreach (var connection in NetworkServer.connections)
                            {
                                if (connection != null && connection.connectionId == entry.Key)
                                {
                                    connection.Send(VoiceChatMsgType.AddUser, new IntegerMessage(id));
                                }

                            }
                        }
                    }

                    // if it's a new tutor, add all of the existing connections to their list of connected users
                    if ("tutor".Equals(proxyIdToRole[id]))
                    {
                        foreach (var proxyId in proxyIdToRole.Keys)
                        {
                            if (proxyId != id)
                            {
                                netMsg.conn.Send(VoiceChatMsgType.AddUser, new IntegerMessage(proxyId));
                            }
                            
                        }
                        
                    }
                }

                
            }
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

            foreach (var entry in currentCallMap)
            {
                if (data.proxyId == entry.Key || entry.Value.Contains(data.proxyId))
                {
                    foreach (var connection in NetworkServer.connections)
                    {

                        if (connection == null || connection.connectionId == data.proxyId)
                            continue;

                        if (connection.connectionId == entry.Key || entry.Value.Contains(connection.connectionId))
                        {
                            connection.SendUnreliable(VoiceChatMsgType.Packet, data);
                        }
                        
                    }


                    foreach (var connection in NetworkServer.localConnections)
                    {
                        var connectionId = connection.connectionId;

                        if (connection == null || connectionId == data.proxyId)
                            continue;

                        if (connectionId == entry.Key || entry.Value.Contains(connectionId))
                        {
                            connection.SendUnreliable(VoiceChatMsgType.Packet, data);
                        }

                    }
                }
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
	
        
        
    }


}