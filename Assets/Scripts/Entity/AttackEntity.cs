using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AttackEntity : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AttackingEntity attackingEntity = this.transform.parent.GetComponent<AttackingEntity>();
        if (attackingEntity)
        {
            //Debug.Log(collision.gameObject);
            attackingEntity.onEnterAttack(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        AttackingEntity attackingEntity = this.transform.parent.GetComponent<AttackingEntity>();
        if (attackingEntity) attackingEntity.onExitAttack(collision);
    }
}
