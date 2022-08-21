using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using kcp2k;
using UnityEngine.UI;

public class BackToMainMenuManager : MonoBehaviour
{
    private float duration = 2;
    private Slider loadingBar;
    private float startTime;
    private void Start()
    {
        loadingBar = GameObject.Find("LoadingBar").GetComponent<Slider>();
        startTime = Time.time;
        Invoke("backToMainMenu", duration);
    }

    private void Update()
    {
        loadingBar.value = (Time.time - startTime) / duration;
    }

    private void backToMainMenu()
    {
        /*NetworkManager.singleton.onlineScene = "MainMenu";

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
        }*/

        NetworkServer.Shutdown();
        GameObject networkManager = GameObject.Find("NetworkManager");
        Destroy(networkManager);

        SceneManager.LoadScene("MainMenu");
    }
}
