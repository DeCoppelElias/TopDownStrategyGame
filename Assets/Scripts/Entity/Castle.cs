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
    [SyncVar(hook = nameof(displayGold))][SerializeField]
    private int _gold = 1;
    [SerializeField]
    private int buildRange = 20;
    [SerializeField]
    private int _goldGain = 1;
    [SerializeField]
    private float _goldCooldown = 4;
    [SerializeField]
    private float _lastgoldGain = 0;

    private void Start()
    {
        _lastgoldGain = Time.time;
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

    public void removeTower(Tower tower)
    {
        if (tower.Owner != this.Owner) throw new System.Exception("You are trying to remove a troop from a castle it doesn't belong to");
        this._towers.Remove(tower);
    }

    public void createTower(string towerName, Vector2 spawnPosition)
    {
        if (Vector2.Distance(this.transform.position, spawnPosition) > buildRange)
        {
            Debug.Log("Building that far away from your castle is not allowed");
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
        GameObject gameObject = Instantiate(prefab, transform.position, Quaternion.identity);
        Troop troop = gameObject.GetComponent<Troop>();
        troop.Path = path;
        troop.Owner = this.Owner;
        troop.ServerClient = this.ServerClient;
        _troops.Add(troop);
        this._gold -= cost;
        troop.dyeAndNameTroop();
        NetworkServer.Spawn(gameObject);
    }

    private void createTower(GameObject prefab, int cost, Vector2 spawnPosition)
    {
        GameObject gameObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        Tower tower = gameObject.GetComponent<Tower>();
        tower.Owner = this.Owner;
        tower.ServerClient = this.ServerClient;
        _towers.Add(tower);
        this._gold -= cost;
        tower.dyeAndNameTower();
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
            client.displayGold(nextGold);
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
        if (_entityState.Equals(EntityState.Attacking))
        {
            attackTarget();
        }
    }

    /// <summary>
    /// This method is called to increase the gold of the castle by using the class parameters
    /// </summary>
    private void gainGold()
    {
        if (Time.time - this._lastgoldGain > this._goldCooldown)
        {
            int duration = (int)(Time.time - this._lastgoldGain);
            int goldGainAmount = (int)(duration / _goldCooldown);
            this._gold += (this._goldGain * goldGainAmount);
            this._lastgoldGain = Time.time;
        }
    }

    /// <summary>
    /// Method for global operations when the owner client is changed
    /// </summary>
    /// <param name="oldClient"></param> The old owner client
    /// <param name="newClient"></param> The new owner client
    protected override void updateOwnerClientEventSpecific(Player oldClient, Player newClient)
    {
        Debug.Log("changed owner client of " + this + " from " + oldClient + " to " + newClient);
        dyeAndNameCastle();
        if (this.Owner is Client client)
        {
            client.displayGold(this._gold);
        }
    }

    /// <summary>
    /// Will dye and name a castle depending on their owner
    /// </summary>
    /// <param name="this"></param>
    public void dyeAndNameCastle()
    {
        if (this.Owner == this.ServerClient)
        {
            this.gameObject.name = "LocalCastle";
            GameObject.Find("Canvas").GetComponent<LevelSceneUi>().activateInGameUi();
            float r = 88;  // red component
            float g = 222;  // green component
            float b = 255;  // blue component
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
        else if (this.Owner is AiClient)
        {
            this.gameObject.name = "AiCastle";
            float r = 95;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
        else if (this.Owner != null)
        {
            this.gameObject.name = "EnemyCastle";
            float r = 255;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
        else if (this.Owner == null)
        {
            this.gameObject.name = "EmptyCastle";
            float r = 255;  // red component
            float g = 255;  // green component
            float b = 255;  // blue component
            this.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
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
}
