using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using UnityEngine.SceneManagement;

public class ConnectToServerManager : MonoBehaviour
{
    public static string serverAdress = "localhost";
    public static ushort port = 7777;

    private void Start()
    {
        Invoke("connectToServer", 2);
    }

    private void connectToServer()
    {
        NetworkManager.singleton.offlineScene = "BackToMainMenuScene";
        NetworkManager.singleton.onlineScene = "LevelSelectScene";

        NetworkManager.singleton.networkAddress = serverAdress;
        NetworkManager.singleton.GetComponent<KcpTransport>().Port = port;

        NetworkManager.singleton.StartClient();
    }
}
