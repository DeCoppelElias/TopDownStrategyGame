using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class HostConnect : NetworkBehaviour
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

    public void startSinglePlayer()
    {
        networkManager.StartHost();
        networkManager.maxConnections = 1;
    }
}
