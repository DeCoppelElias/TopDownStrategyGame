using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LevelSelectScene : NetworkBehaviour
{
    [SerializeField]
    private NetworkManager networkManager;
    [SerializeField]
    private GameObject selectLevelUi;
    [SerializeField]
    private Client client;

    private void Start()
    {
        selectLevelUi = GameObject.Find("LevelScrollView");
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    public void selectLevel(string level)
    {
        string s = level.Split('.')[0];
        int levelNumber = int.Parse(s.Split('-')[1]);
        Level.level = levelNumber;
        networkManager.ServerChangeScene("Level");
    }
    public void selectLevel(int levelNumber)
    {
        Level.level = levelNumber;
        networkManager.ServerChangeScene("Level");
    }

    public void activateSelectLevelUi()
    {
        selectLevelUi.SetActive(true);
    }

    public void leaveGame()
    {
        client.leaveGame();
        if (client.isServer)
        {

        }
    }

    public void setClient(Client client)
    {
        this.client = client;
    }
}
