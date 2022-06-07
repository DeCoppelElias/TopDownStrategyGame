using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPositionState : ClientState
{
    public SelectPositionState(ClientStateManager p) : base(p) { }
    public override void action()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            clientStateManager.sendPositionToPlayer(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }
}
