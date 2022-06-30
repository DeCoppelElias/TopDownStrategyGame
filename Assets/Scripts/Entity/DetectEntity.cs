using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DetectEntity : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Troop troop = this.transform.parent.GetComponent<Troop>();
        if(troop) troop.onEnterDetect(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Troop troop = this.transform.parent.GetComponent<Troop>();
        if (troop) troop.onExitDetect(collision);
    }
}
