using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager
{
    private List<GameState> gameStates;
    private GameState currentGameState;
    private Controller controller;

    public GameStateManager(Controller controller)
    {
        this.controller = controller;
        this.gameStates = new List<GameState>() { new NormalGameState(this), new PauseGameState(this) };
        this.currentGameState = gameStates[0];
    }

    public void changeGameState(string gameState)
    {
        if (gameState.Equals("NormalGameState"))
        {
            this.currentGameState = gameStates[0];
        }
        if (gameState.Equals("PauseGameState"))
        {
            this.currentGameState = gameStates[1];
        }
    }

    public void gameStateUpdate()
    {
        this.currentGameState.gameStateUpdate();
    }

    public void updateGame()
    {
        this.controller.updateGameEvent();
    }
}
