using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class ClientState
{
    protected ClientStateManager clientStateManager;

    public ClientState(ClientStateManager clientStateManager)
    {
        this.clientStateManager = clientStateManager;
    }

    /// <summary>
    /// Each client state must implement this method. This method will be called on the client every frame
    /// </summary>
    public abstract void action();
}
