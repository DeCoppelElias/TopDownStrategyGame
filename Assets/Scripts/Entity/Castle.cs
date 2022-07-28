using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Castle : AttackingEntity
{
    [SerializeField]
    private GameObject _archerTowerPrefab;
    [SerializeField]
    private GameObject _cannonTowerPrefab;
    [SerializeField]
    private GameObject _meleeTowerPrefab;
    [SerializeField]
    private GameObject _swordManPrefab;
    [SerializeField]
    private GameObject _horseRiderPrefab;
    [SerializeField]
    private GameObject _archerPrefab;
    [SerializeField]
    private List<Troop> _troops = new List<Troop>();
    [SerializeField]
    private List<Tower> _towers = new List<Tower>();
    [SerializeField]
    private int maxTowers = 3;
    [SyncVar(hook = nameof(displayGold))][SerializeField]
    private int _gold = 1;
    public int Gold { get => _gold; }
    [SerializeField]
    private int _maxGold = 10;
    [SerializeField]
    private int buildRange = 20;
    [SerializeField]
    private int _goldGain = 1;
    [SerializeField]
    private float _goldCooldown = 4;
    [SerializeField]
    private float _lastgoldGain = 0;

    public override void Start()
    {
        base.Start();
        _lastgoldGain = Time.time;
    }

    public Dictionary<string, object> getTowerInfo(string type)
    {
        GameObject prefab = null;
        if (type == "ArcherTower") prefab = _archerTowerPrefab;
        else if (type == "CannonTower") prefab = _cannonTowerPrefab;
        else if (type == "MeleeTower") prefab = _meleeTowerPrefab;
        if (prefab == null) throw new System.Exception("The tower " + type + " does not exist");
        return prefab.GetComponent<Tower>().getEntityInfo();
    }

    public Dictionary<string, object> getTroopInfo(string type)
    {
        GameObject prefab = null;
        if (type == "SwordManTroop") prefab = _swordManPrefab;
        else if (type == "ArcherTroop") prefab = _archerPrefab;
        else if (type == "HorseRiderTroop") prefab = _horseRiderPrefab;
        if (prefab == null) throw new System.Exception("The tower " + type + " does not exist");
        return prefab.GetComponent<Troop>().getEntityInfo();
    }
    /// <summary>
    /// This method removes a troop from his castle, this means that the troop will no longer be updated
    /// </summary>
    /// <param name="troop"></param> the troop that will be removed
    public void removeTroop(Troop troop)
    {
        if (troop.Owner != this.Owner) throw new System.Exception("You are trying to remove a troop from a castle it doesn't belong to");
        this._troops.Remove(troop);
    }

    /// <summary>
    /// This method removes a tower from his castle, this means that the troop will no longer be updated
    /// </summary>
    /// <param name="tower"></param>
    public void removeTower(Tower tower)
    {
        if (tower.Owner != this.Owner) throw new System.Exception("You are trying to remove a troop from a castle it doesn't belong to");
        this._towers.Remove(tower);
    }

    /// <summary>
    /// This method creates a new tower with the specified parameters
    /// </summary>
    /// <param name="towerName"></param>
    /// <param name="spawnPosition"></param>
    public void createTower(string towerName, Vector2 spawnPosition)
    {
        //Debug.Log("spawning at: " + spawnPosition);
        if(_towers.Count == maxTowers)
        {
            DebugPanel.displayDebugMessage("You have reached the maximum amount of towers");
            Debug.Log("You have reached the maximum amount of towers");
            return;
        }
        if (Vector2.Distance(this.transform.position, spawnPosition) > buildRange)
        {
            DebugPanel.displayDebugMessage("Building that far away from your castle is not allowed");
            Debug.Log("Building that far away from your castle is not allowed");
            return;
        }
        foreach (Tower tower in _towers)
        {
            float distance = Vector2.Distance(spawnPosition, tower.transform.position);
            float spriteSize = tower.transform.Find("AttackRing").GetComponent<SpriteRenderer>().bounds.size.x;
            if (distance <= spriteSize/2)
            {
                Debug.Log("Building cannot be that close to an other building");
                DebugPanel.displayDebugMessage("Building cannot be that close to an other building");
                return;
            }
        }
        float distanceToCastle = Vector2.Distance(spawnPosition, this.transform.position);
        float castleSpriteSize = this.transform.Find("AttackRing").GetComponent<SpriteRenderer>().bounds.size.x;
        if (distanceToCastle < castleSpriteSize / 2)
        {
            Debug.Log("Building cannot be that close to your castle");
            DebugPanel.displayDebugMessage("Building cannot be that close to your castle");
            return;
        }

        int cost = 0;
        GameObject prefab = null;
        if (towerName == "ArcherTower")
        {
            cost = _archerTowerPrefab.GetComponent<Tower>().Cost;
            prefab = _archerTowerPrefab;
        }
        else if (towerName == "CannonTower")
        {
            cost = _cannonTowerPrefab.GetComponent<Tower>().Cost;
            prefab = _cannonTowerPrefab;
        }
        else if (towerName == "MeleeTower")
        {
            cost = _meleeTowerPrefab.GetComponent<Tower>().Cost;
            prefab = _meleeTowerPrefab;
        }
        else
        {
            throw new System.Exception("Tower: " + towerName + " doesn't exist");
        }

        if (this._gold >= cost && prefab)
        {
            createTower(prefab, cost, spawnPosition);
        }
        else
        {
            Debug.Log("Not enough gold, this tower costs: " + cost);
            DebugPanel.displayDebugMessage("Not enough gold, this tower costs: " + cost);
        }
    }

    /// <summary>
    /// This method creates a new troop with the specified parameters
    /// </summary>
    /// <param name="troopName"></param> the name of the troop
    /// <param name="path"></param> the path that the troop will walk
    public void createTroop(string troopName, List<Vector2> path)
    {
        int cost = 0;
        GameObject prefab = null;
        if (troopName == "SwordManTroop")
        {
            cost = _swordManPrefab.GetComponent<SwordManTroop>().Cost;
            prefab = _swordManPrefab;
        }
        else if (troopName == "HorseRiderTroop")
        {
            cost = _horseRiderPrefab.GetComponent<HorseRiderTroop>().Cost;
            prefab = _horseRiderPrefab;
        }
        else if (troopName == "ArcherTroop")
        {
            cost = _archerPrefab.GetComponent<ArcherTroop>().Cost;
            prefab = _archerPrefab;
        }
        else
        {
            throw new System.Exception("Troop: " + troopName + " doesn't exist");
        }

        if (this._gold >= cost && prefab)
        {
            createTroop(path, prefab, cost);
        }
        else
        {
            Debug.Log("Not enough gold, this troop costs: " + cost);
        }
    }

    /// <summary>
    /// Method for creating a troop, creates the prefab and updates some values like owner and server
    /// </summary>
    /// <param name="path"></param> the path that the troop will folow
    /// <param name="prefab"></param> the prefab of the troop
    /// <param name="cost"></param> the cost of the troop
    private void createTroop(List<Vector2> path, GameObject prefab, int cost)
    {
        GameObject troops = GameObject.Find("Troops");
        GameObject gameObject = Instantiate(prefab, transform.position, Quaternion.identity, troops.transform);
        Troop troop = gameObject.GetComponent<Troop>();
        troop.Path = path;
        troop.Owner = this.Owner;
        troop.ServerClient = this.ServerClient;
        _troops.Add(troop);
        this._gold -= cost;
        NetworkServer.Spawn(gameObject);
    }

    /// <summary>
    /// Will create a troop that will attack the target
    /// </summary>
    /// <param name="path"></param> The path that the troop will take
    [Client]
    public void createTroop(string troop, Entity target)
    {
        if (target is Castle castle)
        {
            PathFinding pathFinding = GameObject.Find("PathFinding").GetComponent<PathFinding>();
            List<Vector2> path = pathFinding.findPath(Vector3Int.FloorToInt(this.transform.position), Vector3Int.FloorToInt(castle.transform.position));
            createTroop(troop, path);
        }
        if (target is Troop enemy)
        {
            PathFinding pathFinding = GameObject.Find("PathFinding").GetComponent<PathFinding>();
            List<Vector2> pathToEnemy = pathFinding.findPath(Vector3Int.FloorToInt(this.transform.position), Vector3Int.FloorToInt(enemy.transform.position));
            List<Vector2> pathFromEnemyToCastle = pathFinding.findPath(Vector3Int.FloorToInt(enemy.transform.position), Vector3Int.FloorToInt(enemy.Owner.castle.transform.position));

            List<Vector2> path = pathToEnemy;
            path.AddRange(pathFromEnemyToCastle);

            createTroop(troop, path);
        }
    }

    /// <summary>
    /// Method for creating a tower, creates the prefab and updates some values like owner and server
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="cost"></param>
    /// <param name="spawnPosition"></param>
    private void createTower(GameObject prefab, int cost, Vector2 spawnPosition)
    {
        GameObject towers = GameObject.Find("Towers");
        GameObject gameObject = Instantiate(prefab, spawnPosition, Quaternion.identity, towers.transform);
        Tower tower = gameObject.GetComponent<Tower>();
        tower.Owner = this.Owner;
        tower.ServerClient = this.ServerClient;
        _towers.Add(tower);
        this._gold -= cost;
        NetworkServer.Spawn(gameObject);
    }

    /// <summary>
    /// This method will display the gold of the castle on the computer, works only if this is the local castle
    /// </summary>
    /// <param name="previousGold"></param> the previous amount of gold
    /// <param name="nextGold"></param> the next amount of gold
    private void displayGold(int previousGold, int nextGold)
    {
        if (!this.Owner) return;
        if(this.Owner is Client client)
        {
            client.displayGold(nextGold,_maxGold);
        }
    }

    /// <summary>
    /// This method is called when an entity is killed. It does all the needed procedures before actually deleting the object
    /// </summary>
    public override void getKilled()
    {
        foreach(Troop troop in this._troops)
        {
            this.ServerClient.destoryObject(troop.gameObject);
        }
        GetComponent<BoxCollider2D>().isTrigger = false;
        if (this.Owner is Client client)
        {
            NetworkIdentity opponentIdentity = client.GetComponent<NetworkIdentity>();
            client.clientCastleDestroyed(opponentIdentity.connectionToClient);
        }
        this.ServerClient.destoryObject(this.gameObject);
        this.ServerClient.checkGameDoneAfterDelay();
    }

    /// <summary>
    /// This method will update the troops, gold and attack of this castle
    /// </summary>
    public void updateCastle()
    {
        updateTroops();
        updateTowers();
        gainGold();
        if (_currentEntityState.Equals(EntityState.Attacking))
        {
            attackTarget();

            this.attackRingOpacity = 1f;
        }
        else
        {
            this.attackRingOpacity = 0.2f;
        }
    }

    /// <summary>
    /// This method is called to increase the gold of the castle by using the class parameters
    /// </summary>
    private void gainGold()
    {
        if (Time.time - this._lastgoldGain > this._goldCooldown && this._gold < _maxGold)
        {
            this._gold += this._goldGain;
            this._lastgoldGain = Time.time;
        }
    }

    /// <summary>
    /// Method for global operations when the owner client is changed
    /// </summary>
    /// <param name="oldClient"></param> The old owner client
    /// <param name="newClient"></param> The new owner client
    protected override void updateOwnerClientEventSpecific()
    {
        //Debug.Log("changed owner client of " + this + " from " + oldClient + " to " + newClient);
        foreach(Troop troop in this._troops)
        {
            troop.Owner = this.Owner;
        }
        //Debug.Log(newClient);
        dyeAndNameCastle(this.Owner);
        if (this.Owner is Client client)
        {
            client.displayGold(this._gold, this._maxGold);
        }
    }

    /// <summary>
    /// Will dye and name a castle depending on their owner
    /// </summary>
    /// <param name="this"></param>
    public void dyeAndNameCastle(Player newOwner)
    {
        /*Debug.Log("///////////////////////////");
        Debug.Log(this);
        Debug.Log(newOwner);
        Debug.Log(this.ServerClient);*/

        if (newOwner == null)
        {
            this.gameObject.name = "EmptyCastle";
            float r = 255;  // red component
            float g = 255;  // green component
            float b = 255;  // blue component
            float mainOpacity = this.gameObject.GetComponent<SpriteRenderer>().color.a;
            float childOpacity = this.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, mainOpacity);
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, childOpacity);
        }
        else if (newOwner.name == "LocalClient")
        {
            this.gameObject.name = "LocalCastle";
            float r = 88;  // red component
            float g = 222;  // green component
            float b = 255;  // blue component
            float mainOpacity = this.gameObject.GetComponent<SpriteRenderer>().color.a;
            float childOpacity = this.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, mainOpacity);
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, childOpacity);
        }
        else if (newOwner is AiClient)
        {
            this.gameObject.name = "AiCastle";
            float r = 95;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            float mainOpacity = this.gameObject.GetComponent<SpriteRenderer>().color.a;
            float childOpacity = this.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, mainOpacity);
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, childOpacity);
        }
        else if (newOwner != null)
        {
            this.gameObject.name = "EnemyCastle";
            float r = 255;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            float mainOpacity = this.gameObject.GetComponent<SpriteRenderer>().color.a;
            float childOpacity = this.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a;
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, mainOpacity);
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, childOpacity);
        }
    }

    private void updateTowers()
    {
        foreach (Tower tower in _towers)
        {
            tower.updateTower();
        }
    }

    /// <summary>
    /// This method will update all the troops
    /// </summary>
    private void updateTroops()
    {
        foreach (Troop troop in _troops)
        {
            troop.updateTroop();
        }
    }

    public override void detectClick()
    {
        Client localClient = GameObject.Find("LocalClient").GetComponent<Client>();
        if (localClient.getClientState() == "ViewingState")
        {
            Dictionary<string, object> info = getEntityInfo();
            localClient.displayInfo(info);
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
