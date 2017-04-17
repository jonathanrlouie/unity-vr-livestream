using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VoiceChat.Networking;
using UnityEngine.Networking.NetworkSystem;

public class EndCallButtonController : MonoBehaviour {

    public NetworkManager mng;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        NetworkManager.singleton.client.Send(VoiceChatMsgType.EndCall, new EmptyMessage());
    }
}
