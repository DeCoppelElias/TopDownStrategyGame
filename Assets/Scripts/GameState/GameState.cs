using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState
{
    public GameStateManager gameStateManager;

    public GameState(GameStateManager gameStateManager)
    {
        this.gameStateManager = gameStateManager;
    }
    public abstract void gameStateUpdate();
}
