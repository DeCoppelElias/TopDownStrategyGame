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

    [SerializeField]
    [SyncVar(hook = nameof(updateOwnerClientEvent))]
    protected Player _owner;

    public Player Owner
    {
        get => _owner;
        set => _owner = value;
    }

    [SerializeField][SyncVar]
    protected Client _serverClient;
    public Client ServerClient
    {
        get => _serverClient;
        set => _serverClient = value;
    }

    private Castle castle;

    public abstract void detectClick();

    public abstract Dictionary<string, object> getEntityInfo();

    /// <summary>
    /// This method is called when an entity is killed. It does all the needed procedures before actually deleting the object
    /// </summary>
    public abstract void getKilled();

    /// <summary>
    /// Method for global operations when the owner client is changed
    /// </summary>
    /// <param name="oldClient"></param> The old owner client
    /// <param name="newClient"></param> The new owner client
    public void updateOwnerClientEvent(Player oldPlayer, Player newPlayer)
    {
        updateOwnerClientEventSpecific();
    }

    /// <summary>
    /// Abstract method for specific operations when the owner client is changed
    /// </summary>
    /// <param name="oldPlayer"></param> The old owner client, should always be null
    /// <param name="newPlayer"></param> The new owner client
    protected abstract void updateOwnerClientEventSpecific();

    public bool isVisible()
    {
        if (this is Troop troop)
        {
            if (troop._owner.isLocalPlayer || troop.GlobalVisibility) return true;
            return false;
        }
        else return true;
    }
}
