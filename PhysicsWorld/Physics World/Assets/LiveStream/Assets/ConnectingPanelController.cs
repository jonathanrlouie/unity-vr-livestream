﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VoiceChat;

public class ConnectingPanelController : MonoBehaviour {

    public NetworkManager mng;
    public FSM fsm;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        bool noConnection = (mng.client == null || mng.client.connection == null ||
                                 mng.client.connection.connectionId == -1);
        if (mng.IsClientConnected()) {
            Application.RequestUserAuthorization(UserAuthorization.Microphone);
            VoiceChatRecorder.Instance.StartRecording();
            fsm.StateTrans(FSM.State.Connected);
        } else if (!mng.IsClientConnected() && noConnection) {
            fsm.StateTrans(FSM.State.Unconnected);
        }
	}
}
