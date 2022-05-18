using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager
{
    public List<PlayerState> playerStates;
    public PlayerState currentPlayerState;
    public Player player;

    public PlayerStateManager(Player player)
    {
        playerStates = new List<PlayerState>() { new ViewingState(this), new DrawPathState(this) };
        currentPlayerState = playerStates[0];
        this.player = player;
    }

    public void changeState(string newState)
    {
        if (newState.Equals("ViewingState"))
        {
            currentPlayerState = playerStates[0];
        }
        if (newState.Equals("DrawPathState"))
        {
            Debug.Log("DrawingState");
            currentPlayerState = playerStates[1];
        }
    }

    public void sendPathToPlayer(List<Vector2> path)
    {
        player.createTroop(path);
        changeState("ViewingState");
    }

    public void stateActions()
    {
        currentPlayerState.action();
    }

    public Vector2 getCastlePosition()
    {
        return this.player.getCastlePosition();
    }
}
