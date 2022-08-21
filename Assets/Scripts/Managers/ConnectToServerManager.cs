using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectToServerManager : MonoBehaviour
{
    public static string serverAdress = "localhost";
    public static ushort port = 7777;

    private float duration = 2;
    private Slider loadingBar;
    private float startTime;

    private void Start()
    {
        loadingBar = GameObject.Find("LoadingBar").GetComponent<Slider>();
        startTime = Time.time;
        Invoke("connectToServer", duration);
    }

    private void Update()
    {
        loadingBar.value = (Time.time - startTime) / duration;
    }

    private void connectToServer()
    {
        try
        {
            Invoke("automaticDisconnect", 10);
            NetworkManager.singleton.offlineScene = "BackToMainMenuScene";
            NetworkManager.singleton.onlineScene = "LevelSelectScene";

            NetworkManager.singleton.networkAddress = serverAdress;
            NetworkManager.singleton.GetComponent<KcpTransport>().Port = port;

            NetworkManager.singleton.StartClient();
        }
        catch
        {
            SceneManager.LoadScene("BackToMainMenuScene");
        }
    }

    private void automaticDisconnect()
    {
        SceneManager.LoadScene("BackToMainMenuScene");
    }
}
