﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainSceneUi : NetworkBehaviour
{
    private GameObject selectLevelUi;
    private GameObject hostMultiplayerUi;
    private GameObject mainScreenUi;
    private Client client;
    private void Start()
    {
        mainScreenUi = GameObject.Find("MainScreenUi");
        hostMultiplayerUi = GameObject.Find("HostMultiplayerUi");
        hostMultiplayerUi.SetActive(false);
    }

    public void selectMultiplayer()
    {
        hostMultiplayerUi.SetActive(true);
        mainScreenUi.SetActive(false);
    }
}
