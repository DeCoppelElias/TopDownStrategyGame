using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    protected override void updateOwnerClientEventSpecific()
    {
        dyeAndNameTower(this.Owner);
    }

    /// <summary>
    /// This method will dye and name troops to visually reflect if they are owned by the player or enemies
    /// </summary>
    public void dyeAndNameTower(Player newOwner)
    {
        if (newOwner == null) return;
        if (newOwner.name == "LocalClient")
        {
            this.name = "Local" + this.name;
            float r = 88;  // red component
            float g = 222;  // green component
            float b = 255;  // blue component
            float a = this.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, a);
        }
        else if (newOwner is AiClient)
        {
            this.name = "Ai" + this.name;
            float r = 95;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            float a = this.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, a);
        }
        else if (newOwner != null)
        {
            this.name = "Enemy" + this.name;
            float r = 255;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            float a = this.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, a);
        }
        else if (newOwner == null)
        {
            this.name = "Lost" + this.name;
            float r = 255;  // red component
            float g = 255;  // green component
            float b = 255;  // blue component
            float a = this.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, a);
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

            this.attackRingOpacity = 1f;
        }
        else if (_currentEntityState.Equals(EntityState.Normal))
        {
            this.attackRingOpacity = 0.2f;
        }
    }

    public override void detectClick()
    {
        if (SceneManager.GetActiveScene().name == "Level")
        {
            Client localClient = GameObject.Find("LocalClient").GetComponent<Client>();
            if (localClient.getClientState() == "ViewingState")
            {
                Dictionary<string, object> info = getEntityInfo();
                localClient.displayInfo(info);
            }
        }
    }

    public override Dictionary<string, object> getEntityInfo()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add("Name", this.name);
        result.Add("Damage", this.Damage);
        result.Add("AttackCooldown", this.AttackCooldown);
        result.Add("Range", this.Range);
        result.Add("CurrentEntityState", this.CurrentEntityState.ToString());
        if (this.CurrentEntityState == EntityState.Attacking)
        {
            result.Add("CurrentTarget", this.CurrentTarget.ToString());
        }
        return result;
    }

    protected override void toAttackingState()
    {
        this._currentEntityState = EntityState.Attacking;
        this.attackRingOpacity = 1f;
    }

    protected override void toWalkingToTargetState()
    {
        this._currentEntityState = EntityState.WalkingToTarget;
        this.attackRingOpacity = 0.2f;
    }

    protected override void toNormalState()
    {
        this._currentEntityState = EntityState.Normal;
        this.attackRingOpacity = 0.2f;
    }
}
