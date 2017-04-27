using UnityEngine;
using UnityEngine.Networking;
using VoiceChat.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class CallUserButtonController : MonoBehaviour {

    public int userId;
    private bool inCall = false;

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        if (inCall)
        {
            NetworkManager.singleton.client.Send(VoiceChatMsgType.RequestStudentDrop, new IntegerMessage(userId));
        }
        else
        {
            Debug.Log(userId);
            NetworkManager.singleton.client.Send(VoiceChatMsgType.RequestStudent, new IntegerMessage(userId));
            GetComponent<Image>().color = Color.white;
        }
        inCall = !inCall;
    }
}
