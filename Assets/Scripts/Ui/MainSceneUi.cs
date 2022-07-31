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
        networkManager.StartHost();
        MultiplayerSceneManager.nextScene = "CreateLevelScene";
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void host()
    {
        networkManager.StartHost();
        MultiplayerSceneManager.nextScene = "LevelSelectScene";
    }

    public void connect()
    {
        networkManager.networkAddress = ip_inputField.text;
        networkManager.StartClient();
        MultiplayerSceneManager.nextScene = "LevelSelectScene";
    }

    public void startSinglePlayer()
    {
        networkManager.StartHost();
        networkManager.maxConnections = 1;
        MultiplayerSceneManager.nextScene = "LevelSelectScene";
    }
}
