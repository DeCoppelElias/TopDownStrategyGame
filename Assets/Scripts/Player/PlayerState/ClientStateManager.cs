using System;
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
        clientStates = new List<ClientState>() { new ViewingState(this), new DrawPathState(this), new SelectEntityState(this), new SelectPositionState(this) };
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
            currentClientState = clientStates[1];
        }
        if (newState.Equals("SelectEntityState"))
        {
            throw new System.Exception("SelectEntityState must have a target argument");
        }
        if (newState.Equals("SelectPositionState"))
        {
            currentClientState = clientStates[3];
        }
    }

    public void changeState(string newState, string target)
    {
        if (newState.Equals("ViewingState"))
        {
            currentClientState = clientStates[0];
        }
        if (newState.Equals("DrawPathState"))
        {
            currentClientState = clientStates[1];
        }
        else
        {
            currentClientState = clientStates[2];
            ((SelectEntityState)currentClientState).target = target;
        }
    }

    public void sendPathToPlayer(List<Vector2> path)
    {
        client.createSelectedTroop(path);
        changeState("ViewingState");
    }

    public void sendEntityToPlayer(Entity entity)
    {
        client.createSelectedTroop(entity);
        changeState("ViewingState");
    }

    public void sendPositionToPlayer(Vector2 position)
    {
        client.createSelectedTower(position);
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
