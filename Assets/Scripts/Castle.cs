using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castle : Entity
{
    private List<Troop> troops = new List<Troop>();
    public GameObject swordManPrefab;
    public Client client;
    public GameObject createTroop(string troopName, List<Vector2> path, Castle castle)
    {
        if(troopName == "SwordManTroop")
        {
            GameObject swordManGameObject = SwordManTroop.createTroop(path, transform.position, swordManPrefab);
            SwordManTroop swordMan = swordManGameObject.GetComponent<SwordManTroop>();
            swordMan.castle = castle;
            troops.Add(swordMan);
            return swordManGameObject;
        }
        return null;
    }

    public void updateTroops()
    {
        foreach(Troop troop in troops)
        {
            troop.updateTroop();
        }
    }
}
