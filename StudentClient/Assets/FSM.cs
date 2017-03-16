using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour {
    public GameObject[] menuPanels;
    public GameObject unconnectedPanel;
    public GameObject connectingPanel;
    public GameObject connectedPanel;

    public enum State { Unconnected, Connecting, Connected };

    // Use this for initialization
    void Start()
    {
        menuPanels = GameObject.FindGameObjectsWithTag("VRLivestreamPanel");
		
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    public void StateTrans(State state)
    {
        foreach (GameObject panel in menuPanels)
        {
            panel.gameObject.SetActive(false);
        }
        
        switch (state)
        {
            case State.Unconnected:
                unconnectedPanel.SetActive(true);
                break;
            case State.Connecting:
                connectingPanel.SetActive(true);
                break;
            case State.Connected:
                connectedPanel.SetActive(true);
                break;
        } 
    }
}
