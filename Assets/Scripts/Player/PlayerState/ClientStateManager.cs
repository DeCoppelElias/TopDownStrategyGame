using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientStateManager
{
    private readonly List<ClientState> _clientStates;
    private ClientState _currentClientState;
    public Client Client { get; }

    /// <summary>
    /// Initializes the client state manager, creates the different client states and sets the current state as viewing state
    /// </summary>
    /// <param name="client"></param>
    public ClientStateManager(Client client)
    {
        _clientStates = new List<ClientState>() { new ViewingState(this), new DrawPathState(this), new SelectEntityState(this), new SelectPositionState(this) };
        _currentClientState = _clientStates[0];
        this.Client = client;
    }

    public string getClientState()
    {
        return this._currentClientState.ToString();
    }

    /// <summary>
    /// Changes the client state to viewing state
    /// </summary>
    public void toViewingState()
    {
        _currentClientState.onExitState();
        _currentClientState = _clientStates[0];
        this.Client.displayViewingUi();
    }

    /// <summary>
    /// Changes the client state to draw path state
    /// </summary>
    public void toDrawPathState()
    {
        _currentClientState.onExitState();
        _currentClientState = _clientStates[1];
        this.Client.displayDrawPathUi();
    }

    /// <summary>
    /// Changes the client state to select entity state
    /// </summary>
    /// <param name="target"></param> a string representing the type of target
    public void toSelectEntityState(string target)
    {
        _currentClientState.onExitState();
        _currentClientState = _clientStates[2];
        ((SelectEntityState)_currentClientState).target = target;
    }

    /// <summary>
    /// Changes the client state to select position state
    /// </summary>
    public void toSelectPositionState()
    {
        _currentClientState.onExitState();
        _currentClientState = _clientStates[3];
        this.Client.displaySelectPositionUi();
    }

    /// <summary>
    /// Will handle the finished path from the draw path state and revert to the viewing state
    /// </summary>
    /// <param name="path"></param>
    public void sendPathToPlayer(List<Vector2> path)
    {
        toViewingState();
        Client.createSelectedTroop(path);
    }

    /// <summary>
    /// Will handle the clicked entity from select entity state and revert to the viewing state
    /// </summary>
    /// <param name="entity"></param>
    public void sendEntityToPlayer(Entity entity)
    {
        //Client.createSelectedTroop(entity);
        toViewingState();
    }

    /// <summary>
    /// Will handle the clicked position from select position state and revert to the viewing state
    /// </summary>
    /// <param name="position"></param>
    public void sendPositionToPlayer(Vector2 position)
    {
        this.Client.createSelectedTower(position);
        toViewingState();
    }

    /// <summary>
    /// Will call the actions of the selected state
    /// </summary>
    public void stateActions()
    {
        _currentClientState.action();
    }

    /// <summary>
    /// Gets the position of the castle of the current client
    /// </summary>
    /// <returns></returns>
    public Vector2 getCastlePosition()
    {
        return this.Client.getCastlePosition();
    }
}
