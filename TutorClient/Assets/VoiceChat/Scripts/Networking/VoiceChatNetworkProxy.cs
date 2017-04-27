using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

namespace VoiceChat.Networking
{
    public class VoiceChatNetworkProxy : NetworkBehaviour
    {
        public delegate void MessageHandler<T>(T data);
        public static event MessageHandler<VoiceChatPacketMessage> VoiceChatPacketReceived;
        public static event System.Action<VoiceChatNetworkProxy> ProxyStarted;

        private const string ProxyPrefabPath = "VoiceChat_NetworkProxy";
        private static GameObject proxyPrefab;
        private static int localProxyId;
        private static Dictionary<int, GameObject> proxies = new Dictionary<int, GameObject>();

        public static GameObject callUserButtonPrefab;
        public static GameObject canvas;
        private static Dictionary<int, User> connectedUsers = new Dictionary<int, User>();
        private static Dictionary<string, Vector2> userPosition = new Dictionary<string, Vector2>();
        private static GameObject[,] userGrid;

        //public bool isMine { get { return networkId != 0 && networkId == localProxyId; } }
        public bool isMine { get { return networkId == localProxyId; } }

        [SyncVar]
        private int networkId;

        VoiceChatPlayer player = null;

        void Start()
        {
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

        private static void OnAddUserId(NetworkMessage netMsg)
        {
            VoiceChatAddUserMessage addUserMessage = netMsg.ReadMessage<VoiceChatAddUserMessage>();

            int proxyId = addUserMessage.ProxyId;
            User user = addUserMessage.User;

            if (!connectedUsers.ContainsKey(proxyId) && proxyId != localProxyId)
            {
                connectedUsers.Add(proxyId, user);

                Vector2 gridPos = userPosition[user.Username];

                var button = Instantiate(callUserButtonPrefab, new Vector3(50 * gridPos.x + 50, 50 * gridPos.y + 50, 0), Quaternion.identity);
                button.transform.SetParent(canvas.transform);
                button.GetComponent<CallUserButtonController>().userId = proxyId;
                button.GetComponentInChildren<Text>().text = user.Username;

                userGrid[(int)gridPos.x, (int)gridPos.y] = button;
            }

        }

        private static void OnRemoveUserId(NetworkMessage netMsg)
        {
            int removeId = netMsg.ReadMessage<IntegerMessage>().value;
            var user = connectedUsers[removeId];
            if (user != null)
            {
                Vector2 pos = userPosition[user.Username];
                Destroy(userGrid[(int) pos.x, (int) pos.y]);
            }
            connectedUsers.Remove(removeId);

            
        }

        private static void OnPopulateUserList(NetworkMessage netMsg)
        {
            VoiceChatUserListMessage userListMessage = netMsg.ReadMessage<VoiceChatUserListMessage>();

            int extraRow = 0;
            if (userListMessage.Users.Count % 4 > 0)
            {
                extraRow = 1;
            }
            int numRows = userListMessage.Users.Count / 4 + extraRow;

            userGrid = new GameObject[4, numRows];

            for (int i = 0; i < userListMessage.Users.Count; i++)
            {
                userPosition.Add(userListMessage.Users[i].Username, new Vector2(i % 4, i / 4));
            }

            foreach (var onlineUser in userListMessage.OnlineUsers)
            {
                if (localProxyId != onlineUser.ProxyId)
                {
                    connectedUsers.Add(onlineUser.ProxyId, onlineUser.User);


                    Vector2 gridPos = userPosition[onlineUser.User.Username];

                    var button = Instantiate(callUserButtonPrefab, new Vector3(50 * gridPos.x + 50, 50 * gridPos.y + 50, 0), Quaternion.identity);
                    button.transform.SetParent(canvas.transform);
                    button.GetComponent<CallUserButtonController>().userId = onlineUser.ProxyId;
                    button.GetComponentInChildren<Text>().text = onlineUser.User.Username;

                    userGrid[(int)gridPos.x, (int)gridPos.y] = button;
                }
            }
            
        }

        public static void OnManagerStartClient(NetworkClient client, GameObject customPrefab = null)
        {
            client.RegisterHandler(VoiceChatMsgType.Packet, OnClientPacketReceived);
            client.RegisterHandler(VoiceChatMsgType.SpawnProxy, OnProxySpawned);
			client.RegisterHandler(VoiceChatMsgType.StudentRequestTutor, OnStudentRequestTutor);
            client.RegisterHandler(VoiceChatMsgType.AddUser, OnAddUserId);
            client.RegisterHandler(VoiceChatMsgType.RemoveUser, OnRemoveUserId);
            client.RegisterHandler(VoiceChatMsgType.PopulateUserList, OnPopulateUserList);


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


            if (userGrid != null)
            {
                foreach (var button in userGrid)
                {
                    Destroy(button);
                }
            }
            

            client.UnregisterHandler(VoiceChatMsgType.Packet);
            client.UnregisterHandler(VoiceChatMsgType.SpawnProxy);
			client.UnregisterHandler(VoiceChatMsgType.StudentRequestTutor);
            client.UnregisterHandler(VoiceChatMsgType.AddUser);
            client.UnregisterHandler(VoiceChatMsgType.RemoveUser);
            client.UnregisterHandler(VoiceChatMsgType.PopulateUserList);
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

        public static void OnManagerClientConnect(string password, NetworkConnection connection)
        {
            var client = NetworkManager.singleton.client;
            client.Send(VoiceChatMsgType.RequestProxy, new StringMessage(password));
        }
        
        #endregion

        #region Network Message Handlers
		
		private static void OnStudentRequestTutor(NetworkMessage netMsg)
		{
			var studentProxyId = netMsg.ReadMessage<IntegerMessage>().value;
            
            var user = connectedUsers[studentProxyId];
            Vector2 buttonPos = userPosition[user.Username];
            userGrid[(int)buttonPos.x, (int)buttonPos.y].GetComponent<Image>().color = Color.red;
            
		}

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
    }
}