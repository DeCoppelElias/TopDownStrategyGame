using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected PlayerStateManager playerStateManager;

    public PlayerState(PlayerStateManager playerStateManager)
    {
        this.playerStateManager = playerStateManager;
    }
    public abstract void action();
}
