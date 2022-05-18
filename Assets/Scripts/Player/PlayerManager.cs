using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    public List<RealPlayer> realPlayers = new List<RealPlayer>();
    public List<AiPlayer> aiPlayers = new List<AiPlayer>();
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
