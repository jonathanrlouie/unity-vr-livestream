using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ConnectButtonController : MonoBehaviour {

    public NetworkManager mng;
    public FSM fsm;
    public InputField ipField;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        mng.networkAddress = ipField.text;
        mng.StartClient();
        fsm.StateTrans(FSM.State.Connecting);
    }
}
