using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private GameStateManager gameStateManager;
    private PlayerManager playerManager;
    public GameObject castlePrefab;

    public Controller()
    {
        this.playerManager = new PlayerManager();
        this.gameStateManager = new GameStateManager(this);
    }
    private void Start()
    {
        GameObject castle = Instantiate(castlePrefab, new Vector3(-8, 0, 0), Quaternion.identity);
        RealPlayer player = new RealPlayer();
        player.playerName = "mainPlayer";
        player.castle = castle.GetComponent<Castle>();
        playerManager.realPlayers.Add(player);
    }
    private void Update()
    {
        gameStateManager.gameStateUpdate();
    }

    public void createTroopEvent(string playerName, string name)
    {
        playerManager.createTroopEvent(playerName, name);
    }

    public void updateGameEvent()
    {
        playerManager.updateGameEvent();
    }

    public void pauseGameEvent()
    {
        this.gameStateManager.changeGameState("PauseGameState");
    }

    public void unPauseGameEvent()
    {
        this.gameStateManager.changeGameState("NormalGameState");
    }
}
