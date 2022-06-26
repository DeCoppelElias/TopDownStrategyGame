using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectPositionState : ClientState
{
    public SelectPositionState(ClientStateManager p) : base(p) { }

    /// <summary>
    /// This method will check if the mouse has clicked a position and send that position the CLientStateManager
    /// </summary>
    public override void action()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            clientStateManager.sendPositionToPlayer(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }
}
