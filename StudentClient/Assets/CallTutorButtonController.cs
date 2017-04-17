using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VoiceChat;
using VoiceChat.Networking;
using UnityEngine.Networking.NetworkSystem;

public class CallTutorButtonController : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
        Application.RequestUserAuthorization(UserAuthorization.Microphone);
        VoiceChatRecorder.Instance.AutoDetectSpeech = true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        NetworkManager.singleton.client.Send(VoiceChatMsgType.StudentRequestTutor, new EmptyMessage());
    }
    
}
