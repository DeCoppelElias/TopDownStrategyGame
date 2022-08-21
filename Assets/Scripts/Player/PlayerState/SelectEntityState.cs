using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectEntityState : ClientState
{
    public string target;
    public SelectEntityState(ClientStateManager p) : base(p) { }

    /// <summary>
    /// Will check with every mouseclick if it was on an entity specified by target. If the clicked entity is correct it will send the entity to ClientStateManager
    /// </summary>
    public override void action()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(mousePosition, 1))
            {
                if(target == "Castle")
                {
                    Castle castle = collider.GetComponent<Castle>();
                    if (castle && castle.Owner != this.clientStateManager.Client)
                    {
                        Debug.Log("Found entity " + castle + " at location " + mousePosition);
                        clientStateManager.sendEntityToPlayer(castle);
                        break;
                    }
                }
                else if (target == "Troop")
                {
                    Troop troop = collider.GetComponent<Troop>();
                    if (troop && troop.Owner != this.clientStateManager.Client)
                    {
                        Debug.Log("Found entity " + troop + " at location " + mousePosition);
                        clientStateManager.sendEntityToPlayer(troop);
                        break;
                    }
                }
            }
            Debug.Log("No entity found, please try again");
        }
    }

    public override void onExitState()
    {
        
    }
}
