using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;

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
        NetworkManager.singleton.offlineScene = "MainMenu";
        NetworkManager.singleton.StartClient();
    }
}
