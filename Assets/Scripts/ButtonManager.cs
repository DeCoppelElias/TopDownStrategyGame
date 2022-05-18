﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public Controller controller;
    public void createSwordManButton()
    {
        controller.createTroopEvent("mainPlayer", "swordMan");
    }

    public void pauseGameButton()
    {
        controller.pauseGameEvent();
    }

    public void unPauseGameButton()
    {
        controller.unPauseGameEvent();
    }
}
