using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public GameObject serverCanvas, clientCanvas;
    public ClientHUD clientHudScript;
    public ServerHUD serverHUDScript;

    public void StartServer()
    {
        serverHUDScript.enabled = true;
        serverCanvas.SetActive(true);       
        SceneManager.LoadScene("ServerClientMenu");
    }

    public void StartClient()
    {
        clientHudScript.enabled = true;
        clientCanvas.SetActive(true);
        SceneManager.LoadScene("ServerClientMenu");
    }
}
