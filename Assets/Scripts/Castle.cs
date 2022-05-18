using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castle : Entity
{
    public List<Troop> troops;
    public GameObject swordManPrefab;
    public Player player;
    public void createTroop(string troopName, List<Vector2> path)
    {
        if(troopName == "swordMan")
        {
            GameObject swordManGameObject = Instantiate(swordManPrefab, transform.position, Quaternion.identity);
            SwordManTroop swordManTroop = swordManGameObject.GetComponent<SwordManTroop>();
            swordManTroop.path = path;
            troops.Add(swordManTroop);
        }
    }

    public void updateTroops()
    {
        foreach(Troop troop in troops)
        {
            troop.updateTroop();
        }
    }
}
