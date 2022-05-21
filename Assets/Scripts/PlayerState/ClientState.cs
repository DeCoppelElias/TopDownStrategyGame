using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ClientState
{
    protected ClientStateManager clientStateManager;

    public ClientState(ClientStateManager clientStateManager)
    {
        this.clientStateManager = clientStateManager;
    }
    public abstract void action();
}
