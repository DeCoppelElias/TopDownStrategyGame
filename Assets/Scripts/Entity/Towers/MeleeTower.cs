using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTower : Tower
{
    public override Dictionary<string, object> getEntityInfo()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add("Name", "Melee Tower");
        result.Add("Health", this.Health);
        result.Add("Cost", this.Cost);
        result.Add("Damage", this.Damage);
        result.Add("AttackCooldown", this.AttackCooldown);
        result.Add("Range", this.Range);
        return result;
    }
}
