using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// The superclass for each type of Entity
/// </summary>
public abstract class Entity : NetworkBehaviour
{
    [SyncVar] [SerializeField]
    protected int _health = 100;
    public int Health
    {
        get => _health;
        set => _health = value;
    }

    [SyncVar(hook = nameof(updateOwnerClientEvent))][SerializeField]
    protected Player _owner;
    public Player Owner
    {
        get => _owner;
        set => _owner = value;
    }

    [SerializeField]
    protected Client _serverClient;
    public Client ServerClient
    {
        get => _serverClient;
        set => _serverClient = value;
    }

    /// <summary>
    /// This method is called when an entity is killed. It does all the needed procedures before actually deleting the object
    /// </summary>
    public abstract void getKilled();

    /// <summary>
    /// Method for global operations when the owner client is changed
    /// </summary>
    /// <param name="oldClient"></param> The old owner client, should always be null
    /// <param name="newClient"></param> The new owner client
    public void updateOwnerClientEvent(Player oldClient, Player newClient)
    {
        if(oldClient != null) throw new System.Exception("oldClient must be null, a client cannot be replaced");
        updateOwnerClientEventSpecific(oldClient,newClient);
    }

    /// <summary>
    /// Abstract method for specific operations when the owner client is changed
    /// </summary>
    /// <param name="oldClient"></param> The old owner client, should always be null
    /// <param name="newClient"></param> The new owner client
    protected abstract void updateOwnerClientEventSpecific(Player oldClient, Player newClient);
}
