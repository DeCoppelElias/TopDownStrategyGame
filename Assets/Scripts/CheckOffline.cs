using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckOffline : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("NetworkManager") == null)
        {
            Debug.Log("Offline");
        }
    }
}
