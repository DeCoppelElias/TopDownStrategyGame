using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class MainSceneUi : NetworkBehaviour
{
    private GameObject selectLevelUi;
    private GameObject hostMultiplayerUi;
    private GameObject mainScreenUi;
    private Client client;
    private TMP_InputField ip_inputField;

    private NetworkManager networkManager;
    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        mainScreenUi = GameObject.Find("MainScreenUi");
        hostMultiplayerUi = GameObject.Find("HostMultiplayerUi");
        hostMultiplayerUi.SetActive(false);

        if(NetworkServer.connections.Count == 0)
        {
            networkManager.onlineScene = "";
            networkManager.StartHost();
            networkManager.maxConnections = 1;
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
        networkManager.ServerChangeScene("LevelSelectScene");
        networkManager.maxConnections = 4;
    }

    public void connect()
    {
        networkManager.StopHost();

        networkManager.networkAddress = ip_inputField.text;
        networkManager.StartClient();
    }

    public void startSinglePlayer()
    {
        networkManager.ServerChangeScene("LevelSelectScene");
        networkManager.maxConnections = 1;
    }
}
