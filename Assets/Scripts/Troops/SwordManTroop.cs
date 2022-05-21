using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SwordManTroop : Troop
{
    public static GameObject createTroop(List<Vector2> path, Vector2 spawnPosition, GameObject prefab)
    {
        GameObject swordManGameObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        SwordManTroop swordManTroop = swordManGameObject.GetComponent<SwordManTroop>();
        swordManTroop.setPath(path);
        swordManTroop.speed = 2;
        NetworkServer.Spawn(swordManGameObject);
        return swordManGameObject;
    }
}
