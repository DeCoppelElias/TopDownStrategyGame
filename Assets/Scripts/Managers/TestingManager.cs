using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManager>().StartHost();
    }
}
