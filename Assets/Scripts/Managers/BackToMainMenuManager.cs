using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using kcp2k;

public class BackToMainMenuManager : MonoBehaviour
{
    private void Start()
    {
        Invoke("backToMainMenu", 2);
    }

    private void backToMainMenu()
    {
        NetworkManager.singleton.onlineScene = "MainMenu";

        // Setting up server to simulate background
        if (NetworkServer.connections.Count == 0)
        {
            // Trying different ports to host
            int port = 7777;
            KcpTransport transport = NetworkManager.singleton.GetComponent<KcpTransport>();

            bool found = false;
            int counter = 0;
            while (!found && counter < 10)
            {
                try
                {
                    transport.Port = (ushort)port;

                    NetworkManager.singleton.StartHost();
                    NetworkManager.singleton.maxConnections = 1;
                    found = true;
                }
                catch (System.Exception e)
                {
                    port++;
                }
                counter++;
            }
        }
    }
}
