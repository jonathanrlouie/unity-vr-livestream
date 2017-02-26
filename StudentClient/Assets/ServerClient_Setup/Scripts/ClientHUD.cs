using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class ClientHUD : MonoBehaviour
{

    public GameObject connectToServer, disConnect, addressPanel, connecting, menuCam, disConnectMessage;
    public InputField portText, ipText, passwordText;
    public Text connectingText;

    private NetworkManager manager;
    private float connectingTimer, connectionFaileTimer;
    private bool connected;

    // Use this for initialization
    void Start()
    {
        if (!manager)
            manager = GetComponent<NetworkManager>();

        //checking if we have saved server info.
        if (PlayerPrefs.HasKey("nwPortC"))
        {
            manager.networkPort = Convert.ToInt32(PlayerPrefs.GetString("nwPortC"));
            portText.text = PlayerPrefs.GetString("nwPortC");
        }
        if (PlayerPrefs.HasKey("IPAddressC"))
        {
            manager.networkAddress = PlayerPrefs.GetString("IPAddressC");
            ipText.text = PlayerPrefs.GetString("IPAddressC");
        }
    }

    void Update()
    {
        if (!connected)
        {
            //shows the failed to connect message after a certain time waiting to connect.
            if (connectingTimer > 0)
                connectingTimer -= Time.deltaTime;
            else
            {
               // manager.StopClient();
                connectingText.text = "Failed To Connect !!";
                if (connectionFaileTimer > 0)
                    connectionFaileTimer -= Time.deltaTime;
                else connecting.SetActive(false);
            }
        }
    }

    public void ConnectToServer()
    {
        if (ipText.text != "" && portText.text != "")//is the information filled in ?.
        {
            connected = false;
            disConnectMessage.SetActive(false);
            connectingText.text = "Connecting !!";
            connecting.SetActive(true);
            connectingTimer = 8;//how long we try to connect until the fail message appears.
            connectionFaileTimer = 2;//how long the fail message is showing.
            manager.networkAddress = ipText.text;
            manager.networkPort = Convert.ToInt32(portText.text);
            PlayerPrefs.SetString("IPAddressC", ipText.text);//saving the filled in ip.
            PlayerPrefs.SetString("nwPortC", portText.text);//saving the filled in port.

            manager.StartClient();
        }
    }

    //called by the CustomNetworkManager.
    public void ConnectSuccses()
    {
        connected = true;
        connecting.SetActive(false);
        disConnect.SetActive(true);
        connectToServer.SetActive(false);
        addressPanel.SetActive(false);
        //menuCam.SetActive(false);   //if your player has a camera on him this one should be turned off when entering the game/lobby.
    }

    public void ButtonDisConnect()
    {
        DisConnect(false);
    }

    public void DisConnect(bool showMessage)
    {
        if (showMessage)
            disConnectMessage.SetActive(true);
        connectToServer.SetActive(true);
        disConnect.SetActive(false);
        addressPanel.SetActive(true);
        //menuCam.SetActive(true);  //turn the camera on again when returning to menu scene.
        manager.StopClient();
    }
}
