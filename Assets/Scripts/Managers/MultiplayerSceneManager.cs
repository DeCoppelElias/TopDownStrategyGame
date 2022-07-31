using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerSceneManager : MonoBehaviour
{
    public static string nextScene = "";

    private NetworkManager networkManager;
    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        Invoke("goToNextScene", 1);
    }

    private void goToNextScene()
    {
        if (nextScene.Length == 0) return;
        networkManager.ServerChangeScene(nextScene);
    }
}
