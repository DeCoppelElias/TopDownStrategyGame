using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SwordManTroop : Troop
{
    public override Dictionary<string, object> getEntityInfo()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add("Name", "Sword Man");
        result.Add("Health", this.Health);
        result.Add("Cost", this.Cost);
        result.Add("Damage", this.Damage);
        result.Add("AttackCooldown", this.AttackCooldown);
        result.Add("Range", this.Range);
        return result;
    }
}
