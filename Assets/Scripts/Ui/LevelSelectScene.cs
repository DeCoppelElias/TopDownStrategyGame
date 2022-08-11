using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using kcp2k;

public class LevelSelectScene : NetworkBehaviour
{
    [SerializeField]
    private NetworkManager networkManager;
    [SerializeField]
    private GameObject selectLevelUi;
    [SerializeField]
    private Client client;

    private TMP_Text amountOfClientsText;

    [SyncVar(hook = nameof(onDetectAmountOfPlayersChanged))]
    private int amountOfPlayers = 0;

    private TMP_Text serverAdressText;
    private TMP_Text serverPort;

    private void Start()
    {
        selectLevelUi = GameObject.Find("LevelScrollViews");
        selectLevelUi.SetActive(false);
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        amountOfClientsText = GameObject.Find("ClientAmount").GetComponent<TMP_Text>();
        serverAdressText = GameObject.Find("ServerAdressInput").GetComponent<TMP_Text>();
        serverPort = GameObject.Find("ServerPortInput").GetComponent<TMP_Text>();

        serverAdressText.text = networkManager.networkAddress;
        serverPort.text = networkManager.GetComponent<KcpTransport>().Port.ToString();

        networkManager.offlineScene = "BackToMainMenuScene";

        amountOfClientsText.text = amountOfPlayers.ToString();
    }

    private void Update()
    {
        Debug.Log(amountOfPlayers);
    }

    private void onDetectAmountOfPlayersChanged(int oldAmountOfPlayers, int newAmountOfPlayers)
    {
        TMP_Text amountOfClientsText = GameObject.Find("ClientAmount").GetComponent<TMP_Text>();
        amountOfClientsText.text = newAmountOfPlayers.ToString();
    }

    public void selectLevel(string level)
    {
        Level.levelName = level;
        networkManager.ServerChangeScene("Level");
    }

    public void activateSelectLevelUi()
    {
        selectLevelUi.SetActive(true);
    }

    public void leaveGame()
    {
        if (client.isServer)
        {
            networkManager.StopHost();
        }
        else
        {
            networkManager.StopClient();
        }
    }

    public void setClient(Client client)
    {
        this.client = client;
    }

    [Server]
    public void clientJoined()
    {
        amountOfPlayers += 1;
    }
}
