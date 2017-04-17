using UnityEngine;
using UnityEngine.Networking;
using VoiceChat.Networking;
using UnityEngine.Networking.NetworkSystem;

public class CallUserButtonController : MonoBehaviour {

    public int userId;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnClick()
    {
        NetworkManager.singleton.client.Send(VoiceChatMsgType.RequestStudent, new IntegerMessage(userId));
    }
}
