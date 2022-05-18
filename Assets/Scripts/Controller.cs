using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public GameStateManager gameStateManager;
    public PlayerManager playerManager;

    public void createTroopEvent(string playerName, string name)
    {
        playerManager.createTroopEvent(playerName, name);
    }

    private void Update()
    {
        playerManager.updateGameEvent();
    }
}
