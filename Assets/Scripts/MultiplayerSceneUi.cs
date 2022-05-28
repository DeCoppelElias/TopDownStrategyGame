using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MultiplayerSceneUi : NetworkBehaviour
{
    private NetworkManager networkManager;
    public GameObject selectLevelUi;

    private void Start()
    {
        selectLevelUi = GameObject.Find("SelectLevelUi");
        selectLevelUi.SetActive(false);
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }
    public void selectLevel(int level)
    {
        string levelString = "level-" + level;
        networkManager.ServerChangeScene(levelString);
    }
}
