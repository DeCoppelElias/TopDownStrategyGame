using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager: MonoBehaviour
{
    public List<RealPlayer> realPlayers = new List<RealPlayer>();
    public List<AiPlayer> aiPlayers = new List<AiPlayer>();
    public GameObject castlePrefab;

    private void Start()
    {
        GameObject castle = Instantiate(castlePrefab, new Vector3(-8, 0, 0), Quaternion.identity);
        RealPlayer player = new RealPlayer();
        player.playerName = "mainPlayer";
        player.castle = castle.GetComponent<Castle>();
        realPlayers.Add(player);
    }
    public void createTroopEvent(string playerName, string troopName)
    {
        foreach(RealPlayer player in realPlayers)
        {
            if(player.playerName == playerName)
            {
                player.createTroopEvent(troopName);
            }
        }
    }

    public void updateGameEvent()
    {
        foreach(RealPlayer player in realPlayers)
        {
            player.stateActions();
            player.updateCastle();
        }
    }
}
