using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;

public class DecorationCollisions : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Troop troop = collision.GetComponent<Troop>();
        if (troop != null)
        {
            troop.onEnterDecoration();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Troop troop = collision.GetComponent<Troop>();
        if (troop != null)
        {
            troop.onExitDecoration();
        }
    }

    public bool isColliding(Collider2D col)
    {
        return GetComponent<TilemapCollider2D>().IsTouching(col);
    }
}
