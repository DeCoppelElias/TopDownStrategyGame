using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AiClient : Player
{
    private enum AiClientState { Attacking, Defensive}
    [SerializeField]
    private AiClientState aiClientState = AiClientState.Attacking;
    [SerializeField]
    private enum AiAction { Troop, Tower}
    [SerializeField]
    private AiAction aiAction = AiAction.Troop;
    private int spawnCooldown;
    private float lastSpawn = 0;

    [SerializeField]
    private float defenseRange = 10;

    private List<(Troop, float)> nearbyTroops = new List<(Troop, float)>();

    private Dictionary<string,int> troopNamesAndCosts;
    private Dictionary<string, int> towerNamesAndCosts;

    private Action nextAction;

    private bool setup = false;

    private void Start()
    {
        this.transform.SetParent(GameObject.Find("AiClients").transform);

        troopNamesAndCosts = new Dictionary<string, int>();
        Object[] troopObjects = Resources.LoadAll("Prefabs/Entities/TroopPrefabs",typeof(GameObject));
        foreach(Object obj in troopObjects)
        {
            GameObject gameObject = (GameObject)obj;
            Troop troop = gameObject.GetComponent<Troop>();
            if(troop != null)
            {
                troopNamesAndCosts.Add(gameObject.name, troop.Cost);
            }
        }

        towerNamesAndCosts = new Dictionary<string, int>();
        Object[] towerObjects = Resources.LoadAll("Prefabs/Entities/TowerPrefabs", typeof(GameObject));
        foreach (Object obj in towerObjects)
        {
            GameObject gameObject = (GameObject)obj;
            Tower tower = gameObject.GetComponent<Tower>();
            if (tower != null)
            {
                towerNamesAndCosts.Add(gameObject.name, tower.Cost);
            }
        }
        setup = true;
    }
    public void aiUpdate()
    {
        if (!setup) return;
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
        if (nextAction == null)
        {
            determineNextDefensiveAction();
            return;
        }
        if (this.nextAction.checkDone())
        {
            determineNextDefensiveAction();
        }
        this.nextAction.actionStep();
    }

    public void attackingActions()
    {
        if (nextAction == null)
        {
            determineNextOffensiveAction();
            return;
        }
        if (this.nextAction.checkDone())
        {
            determineNextOffensiveAction();
        }
        this.nextAction.actionStep();
    }

    private Castle determineCastle()
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
        return target;
    }

    private void determineNextOffensiveAction()
    {
        int random = Random.Range(0, 10);
        if(random < 8)
        {
            // Create a troop to attack a random castle
            string nextTroop = determineNextTroop();
            Castle target = determineCastle();
            if (target == null) return;

            this.nextAction = new BuildTroopAction(nextTroop, troopNamesAndCosts[nextTroop], target, this.castle);
        }
        else
        {
            // Create a preventive tower
            if (this.castle.maximumAmountOfTowers()) return;
            string nextTower = determineNextTower();
            Castle target = determineCastle();
            if (target == null) return;
            Vector3 towerPosition = castle.generateRandomTowerPosition(target.transform.position);

            this.nextAction = new BuildTowerAction(nextTower, towerNamesAndCosts[nextTower], towerPosition, this.castle);
        }
    }

    private void determineNextDefensiveAction()
    {
        int random = Random.Range(0, 10);
        if(random < 8)
        {
            // Create Defensive Troop
            string nextTroop = determineNextTroop();
            Troop target = findClosestEnemyTroop();

            this.nextAction = new BuildTroopAction(nextTroop, troopNamesAndCosts[nextTroop], target, this.castle);
        }
        else
        {
            // Create Defensive Tower
            if (this.castle.maximumAmountOfTowers()) return;
            string nextTower = determineNextTower();
            Troop target = findClosestEnemyTroop();
            Vector3 towerPosition = castle.generateRandomTowerPosition(target.transform.position);

            this.nextAction = new BuildTowerAction(nextTower, towerNamesAndCosts[nextTower], towerPosition, this.castle);
        }
    }

    private Troop findClosestEnemyTroop()
    {
        float smallestDistance = float.MaxValue;
        Troop closestTroop = null;
        foreach ((Troop, float) tuple in nearbyTroops)
        {
            Troop currentTroop = tuple.Item1;
            float distance = tuple.Item2;
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestTroop = currentTroop;
            }
        }
        if (closestTroop == null) { throw new System.Exception("Devense state is active but there are no attacking troops"); }
        return closestTroop;
    }

    private string determineNextTower()
    {
        // Calculating total costs of towers
        int total = 0;
        foreach (KeyValuePair<string, int> keyValuePair in towerNamesAndCosts)
        {
            int towerCost = keyValuePair.Value;
            total += towerCost;
        }

        // Creating random number that will determine the next troop
        int random = Random.Range(0, total);
        foreach (KeyValuePair<string, int> keyValuePair in towerNamesAndCosts)
        {
            string towerName = keyValuePair.Key;
            int towerCost = keyValuePair.Value;
            if (random < towerCost)
            {
                return towerName;
            }
            else
            {
                random -= towerCost;
            }
        }
        throw new System.Exception("No troop found");
    }
    private string determineNextTroop()
    {
        // Calculating total costs of troops
        int total = 0;
        foreach(KeyValuePair<string, int> keyValuePair in troopNamesAndCosts)
        {
            int troopCost = keyValuePair.Value;
            total += troopCost;
        }

        // Creating random number that will determine the next troop
        int random = Random.Range(0, total);
        foreach (KeyValuePair<string, int> keyValuePair in troopNamesAndCosts)
        {
            string troopName = keyValuePair.Key;
            int troopCost = keyValuePair.Value;
            if(random < troopCost)
            {
                return troopName;
            }
            else
            {
                random -= troopCost;
            }
        }
        throw new System.Exception("No troop found");
    }

    private abstract class Action
    {
        protected bool done = false;
        public bool checkDone()
        {
            return this.done;
        }

        public abstract void actionStep();
    }

    private class BuildTroopAction : Action
    {
        private Castle owner;
        private Entity target;
        private string troopName;
        private int troopCost;
        public BuildTroopAction(string troopName, int troopCost, Entity target, Castle owner)
        {
            this.troopName = troopName;
            this.troopCost = troopCost;
            this.target = target;
            this.owner = owner;
        }
        public override void actionStep()
        {
            if (done) return;

            // Check if castle has enough money
            if (owner.Gold >= troopCost)
            {
                if(target is Castle castle)
                {
                    owner.createTroopWithPathVariation(troopName, castle);
                }
                else
                {
                    owner.createTroop(troopName, target);
                }
                done = true;
            }
        }
    }

    private class BuildTowerAction : Action
    {
        private Castle owner;
        private Vector3 position;
        private string towerName;
        private int towerCost;

        public BuildTowerAction(string towerName, int towerCost, Vector3 position, Castle owner)
        {
            this.towerName = towerName;
            this.towerCost = towerCost;
            this.position = position;
            this.owner = owner;
        }
        public override void actionStep()
        {
            if (done) return;

            // Check if castle has enough money
            if (owner.Gold >= towerCost)
            {
                owner.createTower(towerName, position);
                done = true;
            }
        }
    }
}
