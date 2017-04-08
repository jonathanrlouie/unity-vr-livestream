using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

public class StartScript : MonoBehaviour {
	
	public NetworkManager mng;
	// Use this for initialization
	void Start () {

		mng.StartHost();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
