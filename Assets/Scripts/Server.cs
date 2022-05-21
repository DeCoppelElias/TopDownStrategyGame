using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
public class Server : MonoBehaviour
{
    private NetworkManager networkManager;
    public InputField ip_inputField;
    public GameObject serverUi;
    private void Awake()
    {
        networkManager = this.GetComponent<NetworkManager>();
    }

    public void host()
    {
        networkManager.StartHost();

        serverUi.SetActive(false);
    }

    public void connect()
    {
        networkManager.networkAddress = ip_inputField.text;
        networkManager.StartClient();

        serverUi.SetActive(false);
    }
}
