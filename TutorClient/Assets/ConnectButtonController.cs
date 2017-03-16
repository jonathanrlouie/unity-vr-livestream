using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConnectButtonController : MonoBehaviour {

    public NetworkManager mng;
    public InputField ipAddrField;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    public void OnClickedButton() {
        mng.StartClient();
        mng.networkAddress = ipAddrField.text.ToString();
    }
}
