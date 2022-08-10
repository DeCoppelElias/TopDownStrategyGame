using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System.Net.Sockets;
using System.Net;
using kcp2k;

public class MainSceneUi : NetworkBehaviour
{
    private GameObject selectLevelUi;
    private GameObject hostMultiplayerUi;
    private GameObject mainScreenUi;
    private Client client;
    private TMP_InputField inputField;

    private NetworkManager networkManager;

    
    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        mainScreenUi = GameObject.Find("MainScreenUi");
        hostMultiplayerUi = GameObject.Find("HostMultiplayerUi");
        inputField = GameObject.Find("AdressInputField").GetComponent<TMP_InputField>();

        hostMultiplayerUi.SetActive(false);

        if(NetworkServer.connections.Count == 0)
        {
            int port = 7777;
            KcpTransport transport = networkManager.GetComponent<KcpTransport>();

            bool found = false;
            int counter = 0;
            while (!found && counter < 10)
            {
                try
                {
                    transport.Port = (ushort)port;

                    networkManager.StartHost();
                    networkManager.maxConnections = 1;
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

    public void selectMultiplayer()
    {
        hostMultiplayerUi.SetActive(true);
        mainScreenUi.SetActive(false);
    }

    public void backToMainMenu()
    {
        hostMultiplayerUi.SetActive(false);
        mainScreenUi.SetActive(true);
    }

    public void selectCreateLevel()
    {
        networkManager.ServerChangeScene("CreateLevelScene");
        networkManager.maxConnections = 1;
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void host()
    {
        networkManager.maxConnections = 4;
        networkManager.ServerChangeScene("LevelSelectScene");
    }

    public void connect()
    {
        string[] s = inputField.text.Split(':');
        ConnectToServerManager.serverAdress = s[0];
        ConnectToServerManager.port = ushort.Parse(s[1]);

        networkManager.offlineScene = "ConnectToServerScene";

        networkManager.StopHost();
    }

    public void startSinglePlayer()
    {
        networkManager.maxConnections = 1;
        networkManager.ServerChangeScene("LevelSelectScene");
    }
}
