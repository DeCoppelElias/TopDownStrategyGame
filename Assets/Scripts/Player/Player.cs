using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public Castle castle;
    public string playerName;
    public PlayerStateManager playerStateManager;

    private string selectedTroop;

    public Player()
    {
        playerStateManager = new PlayerStateManager(this);
    }

    public void createTroop(List<Vector2> path)
    {
        castle.createTroop(selectedTroop, path);
    }

    public void createTroopEvent(string troopName)
    {
        this.selectedTroop = troopName;
        this.playerStateManager.changeState("DrawPathState");
    }

    public void stateActions()
    {
        playerStateManager.stateActions();
    }

    public void updateCastle()
    {
        castle.updateTroops();
    }

    public Vector2 getCastlePosition()
    {
        return this.castle.transform.position;
    }
}
