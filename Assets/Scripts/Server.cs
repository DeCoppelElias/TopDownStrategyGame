using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class Server : MonoBehaviour
{
    private NetworkManager networkManager;
    public TMP_InputField ip_inputField;
    private void Awake()
    {
        networkManager = this.GetComponent<NetworkManager>();
    }

    public void host()
    {
        networkManager.StartHost();
    }

    public void connect()
    {
        networkManager.networkAddress = ip_inputField.text;
        networkManager.StartClient();
    }
}
