using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoiceChat;

public class CallTutorButtonController : MonoBehaviour {

    enum ButtonState { StartCall, EndCall };

    ButtonState currentState = ButtonState.StartCall;

	// Use this for initialization
	void Start () {
        Application.RequestUserAuthorization(UserAuthorization.Microphone);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        switch (currentState)
        {
            case ButtonState.StartCall:
                currentState = ButtonState.EndCall;
                StartCall();
                break;
            case ButtonState.EndCall:
                currentState = ButtonState.StartCall;
                EndCall();
                break;
        }
    }

    void StartCall()
    {
        VoiceChatRecorder.Instance.AutoDetectSpeech = true;
    }

    void EndCall()
    {
        VoiceChatRecorder.Instance.AutoDetectSpeech = false;
    }
}
