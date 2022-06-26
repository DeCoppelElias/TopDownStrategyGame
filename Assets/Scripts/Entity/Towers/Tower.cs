using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : AttackingEntity
{
    /// <summary>
    /// This method is called when a tower is destroyed. It does all the needed procedures before actually deleting the object
    /// </summary>
    public override void getKilled()
    {
        Castle castle = this.Owner.castle;
        if (castle != null)
        {
            castle.removeTower(this);
        }
        ServerClient.destoryObject(this.gameObject);
    }

    protected override void updateOwnerClientEventSpecific(Player oldClient, Player newClient) { }

    /// <summary>
    /// This method will dye and name troops to visually reflect if they are owned by the player or enemies
    /// </summary>
    public void dyeAndNameTower()
    {
        if (this.Owner.isLocalPlayer)
        {
            this.name = "Local" + this.name;
            float r = 88;  // red component
            float g = 222;  // green component
            float b = 255;  // blue component
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
        else if (this.Owner is AiClient)
        {
            this.name = "Ai" + this.name;
            float r = 95;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
        else if (this.Owner != null)
        {
            this.name = "Enemy" + this.name;
            float r = 255;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
        else if (this.Owner == null)
        {
            this.name = "Lost" + this.name;
            float r = 255;  // red component
            float g = 255;  // green component
            float b = 255;  // blue component
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
    }

    /// <summary>
    /// This method will update the tower, it will atack an enemy if in range
    /// </summary>
    public void updateTower()
    {
        if (_currentEntityState.Equals(EntityState.Attacking))
        {
            attackTarget();
        }
    }
}
