using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiClient : Player
{
    private enum AiClientState { Attacking, Defensive}
    [SerializeField]
    private AiClientState aiClientState = AiClientState.Attacking;
    [SerializeField]
    private int spawnCooldown;
    private float lastSpawn = 0;

    [SerializeField]
    private float defenseRange = 10;

    private List<(Troop, float)> nearbyTroops = new List<(Troop, float)>();
    public void aiUpdate()
    {
        if (this.castle == null) return;
        if(this.castle.Gold > 0 && Time.time > lastSpawn + spawnCooldown)
        {
            lastSpawn = Time.time;

            nearbyTroops = new List<(Troop, float)>();
            nearbyTroops = checkNearbyTroops();

            if(nearbyTroops.Count > 0)
            {
                aiClientState = AiClientState.Defensive;
                defensiveActions();
            }
            else
            {
                aiClientState = AiClientState.Attacking;
                attackingActions();
            }
        }
    }

    public List<(Troop, float)> checkNearbyTroops()
    {
        List<(Troop, float)> nearbyTroops = new List<(Troop, float)>();
        foreach (Troop troop in GameObject.Find("Troops").GetComponentsInChildren<Troop>())
        {
            float distance = Vector3.Distance(troop.transform.position, this.castle.transform.position);
            if (distance <= defenseRange && troop.Owner != this)
            {
                nearbyTroops.Add((troop, distance));
            }
        }
        return nearbyTroops;
    }

    public void defensiveActions()
    {
        float smallestDistance = float.MaxValue;
        Troop closestTroop = null;
        foreach((Troop, float) tuple in nearbyTroops)
        {
            Troop currentTroop = tuple.Item1;
            float distance = tuple.Item2;
            if(distance < smallestDistance)
            {
                smallestDistance = distance;
                closestTroop = currentTroop;
            }
        }
        if(closestTroop == null) { throw new System.Exception("Devense state is active but there are no attacking troops"); }
        this.castle.createTroop("SwordManTroop", closestTroop);
    }

    public void attackingActions()
    {
        List<(Castle, float)> castlesWithDistance = new List<(Castle, float)>();
        float totalValue = 0;
        foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
        {
            Castle currentCastle = castleGameObject.GetComponent<Castle>();
            if (currentCastle.Owner != this)
            {
                float value = 100 - Vector3.Distance(this.castle.transform.position, currentCastle.transform.position);
                if (value < 0) { value = 0; }
                totalValue += value;
                castlesWithDistance.Add((currentCastle, totalValue));
            }
        }

        float randomTargetNumber = Random.Range(0, totalValue);

        Castle target = null;
        foreach ((Castle, float) tuple in castlesWithDistance)
        {
            if (randomTargetNumber < tuple.Item2)
            {
                target = tuple.Item1;
                break;
            }
        }
        if (target == null) throw new System.Exception("Ai cannot find castle to target");

        this.castle.createTroop("SwordManTroop", target);
    }
}
