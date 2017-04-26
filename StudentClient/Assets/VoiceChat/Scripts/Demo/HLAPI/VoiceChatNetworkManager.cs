using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using VoiceChat.Networking;
using UnityEngine.UI;

namespace VoiceChat.Demo.HLAPI
{
    

    public class VoiceChatNetworkManager : NetworkManager
    {
        public FSM fsm;
        public InputField passwordField;
        void Start(){
			
		}

        public override void OnStartClient(NetworkClient client)
        {
            VoiceChatNetworkProxy.OnManagerStartClient(client);
            VoiceChatRecorder.Instance.AutoDetectSpeech = true;
        }

        public override void OnStopClient()
        {
            VoiceChatNetworkProxy.OnManagerStopClient();
            VoiceChatRecorder.Instance.AutoDetectSpeech = false;
            VoiceChatRecorder.Instance.StopRecording();
            fsm.StateTrans(FSM.State.Unconnected);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);

            VoiceChatNetworkProxy.OnManagerServerDisconnect(conn);
        }

        public override void OnStartServer()
        {
            VoiceChatNetworkProxy.OnManagerStartServer();

            gameObject.AddComponent<VoiceChatServerUi>();
        }

        public override void OnStopServer()
        {
            VoiceChatNetworkProxy.OnManagerStopServer();

            Destroy(GetComponent<VoiceChatServerUi>());
        }

        public override void OnClientConnect(NetworkConnection connection)
        {
            base.OnClientConnect(connection);
            VoiceChatNetworkProxy.OnManagerClientConnect(passwordField.text, connection);
        }
    }
}
