using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientStateManager
{
    public List<ClientState> clientStates;
    public ClientState currentClientState;
    public Client client;

    public ClientStateManager(Client client)
    {
        clientStates = new List<ClientState>() { new ViewingState(this), new DrawPathState(this) };
        currentClientState = clientStates[0];
        this.client = client;
    }

    public void changeState(string newState)
    {
        if (newState.Equals("ViewingState"))
        {
            currentClientState = clientStates[0];
        }
        if (newState.Equals("DrawPathState"))
        {
            Debug.Log("DrawingState");
            currentClientState = clientStates[1];
        }
    }

    public void sendPathToPlayer(List<Vector2> path)
    {
        client.createSelectedTroop(path);
        changeState("ViewingState");
    }

    public void stateActions()
    {
        currentClientState.action();
    }

    public Vector2 getCastlePosition()
    {
        return this.client.getCastlePosition();
    }
}
