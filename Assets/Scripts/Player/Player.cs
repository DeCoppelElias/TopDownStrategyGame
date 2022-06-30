using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public Castle castle;
    public enum GameState { Pause, Normal };
    [SyncVar]
    [SerializeField]
    protected GameState _currentGameState = GameState.Pause;
    public GameState CurrentGameState { get => _currentGameState; }

    /// <summary>
    /// Will create a troop on the server
    /// </summary>
    /// <param name="troopName"></param> a string representing the troop
    /// <param name="path"></param> the path that the troop will follow
    [Command]
    public void createTroop(string troopName, List<Vector2> path)
    {
        this.castle.createTroop(troopName, path);
    }

    [Command]
    public void createTower(string towerName, Vector2 position)
    {
        this.castle.createTower(towerName, position);
    }
}
