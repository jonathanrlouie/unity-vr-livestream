using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DisconnectButtonController : MonoBehaviour {

    public NetworkManager mng;
    public FSM fsm;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        mng.StopClient();
        fsm.StateTrans(FSM.State.Unconnected);
    }


}
