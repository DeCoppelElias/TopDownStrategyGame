using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalGameState : GameState
{
    public NormalGameState(GameStateManager gameStateManager) : base(gameStateManager) { }
    public override void gameStateUpdate()
    {
        gameStateManager.updateGame();
    }
}
