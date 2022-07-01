using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public abstract class Troop : AttackingEntity
{
    [SerializeField][SyncVar]
    private List<Vector2> _path = new List<Vector2>();

    [SerializeField]
    private float viewRange = 5;
    public List<Vector2> Path
    {
        set => _path = value;
    }
    [SyncVar]
    protected List<Entity> _targetsToFollow = new List<Entity>();

    [SerializeField]
    private float _speed;

    private LineRenderer lineRenderer;

    [SyncVar(hook = nameof(onDetectRingChangeScale))]
    private Vector3 detectRingScale = new Vector3(0, 0, 0);
    [SyncVar(hook = nameof(onDetectRingChangeOpacity))]
    private float detectRingOpacity = 0;

    public override void Start()
    {
        base.Start();

        this.lineRenderer = this.GetComponentInChildren<LineRenderer>();
        lineRenderer.positionCount = 0;

        GameObject troops = GameObject.Find("Troops");
        this.transform.SetParent(troops.transform);

        if (!isServer) return;

        Vector3 scale = new Vector3((viewRange * 2) + 1, (viewRange * 2) + 1, 0);
        this.detectRingOpacity = 0.2f;
        this.detectRingScale = scale;
    }

    /// <summary>
    /// This will update a troop, it will make the troop attack, follow or continue on his path
    /// </summary>
    public void updateTroop()
    {
        if (_currentEntityState.Equals(EntityState.Normal))
        {
            if (_path.Count > 0)
            {
                Vector2 currentPosition = transform.position;
                if (Vector2.Distance(currentPosition, _path[0]) < 0.3)
                {
                    _path.RemoveAt(0);
                }
                else
                {
                    var step = _speed * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, _path[0], step);
                }
            }
            else
            {
                // Searching for closest castle and attacking it
                Castle closestCastle = null;
                float smallestDistance = float.MaxValue;
                foreach(Castle castle in GameObject.Find("Castles").GetComponentsInChildren<Castle>())
                {
                    float distance = Vector3.Distance(castle.transform.position, this.transform.position);
                    if(distance < smallestDistance && castle.Owner != this.Owner)
                    {
                        smallestDistance = distance;
                        closestCastle = castle;
                    }
                }
                if(closestCastle == null) { return; }
                PathFinding pathFinding = GameObject.Find("PathFinding").GetComponent<PathFinding>();
                _path = pathFinding.findPath(Vector3Int.FloorToInt(this.transform.position), Vector3Int.FloorToInt(closestCastle.transform.position));
            }
        }
        if (_currentEntityState.Equals(EntityState.WalkingToTarget))
        {
            if (_currentTarget)
            {
                var step = _speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, _currentTarget.transform.position, step);
            }
            else
            {
                _currentEntityState = EntityState.Normal;
            }
        }
        if (_currentEntityState.Equals(EntityState.Attacking))
        {
            attackTarget();
        }
    }

    /// <summary>
    /// This method is called when a troop is killed. It does all the needed procedures before actually deleting the object
    /// </summary>
    protected override void killTarget()
    {
        _currentEntityState = EntityState.Normal;
        //Debug.Log("Killing target: " + this._currentTarget);
        this._targetsToAttack.Remove(this._currentTarget);
        this._targetsToFollow.Remove(this._currentTarget);
        this._currentTarget.getKilled();
        this._currentTarget = null;
    }

    /// <summary>
    /// This method will search for a new target to attack or follow for this troop
    /// </summary>
    protected override void searchNewTarget()
    {
        if (_targetsToFollow.Count == 0 && _targetsToAttack.Count == 0)
        {
            _currentTarget = null;
            _currentEntityState = AttackingEntity.EntityState.Normal;
            this.detectRingOpacity = 0.2f;
            this.attackRingOpacity = 0.2f;
        }
        else if (_targetsToAttack.Count > 0)
        {
            _currentTarget = _targetsToAttack[0];
            _currentEntityState = AttackingEntity.EntityState.Attacking;
            this.detectRingOpacity = 1f;
            this.attackRingOpacity = 1f;
        }
        else if (_targetsToFollow.Count > 0)
        {
            _currentTarget = _targetsToFollow[0];
            _currentEntityState = AttackingEntity.EntityState.WalkingToTarget;
            this.detectRingOpacity = 1f;
            this.attackRingOpacity = 0.2f;
        }
    }

    /// <summary>
    /// This method is called when an entity is killed. It does all the needed procedures before actually deleting the object
    /// </summary>
    public override void getKilled()
    {
        Castle castle = this.Owner.castle;
        if(castle != null)
        {
            castle.removeTroop(this);
        }
        ServerClient.destoryObject(this.gameObject);
    }

    /// <summary>
    /// Abstract method for specific operations when the owner client is changed
    /// </summary>
    /// <param name="oldClient"></param> The old owner client, should always be null
    /// <param name="newClient"></param> The new owner client
    protected override void updateOwnerClientEventSpecific()
    {
        //Debug.Log("changed owner client of " + this + " from " + oldClient + " to " + newClient);
        dyeAndNameTroop(this.Owner);
    }

    /// <summary>
    /// This method is called when a new collision enters the Detect Ring
    /// </summary>
    /// <param name="collision"></param>
    public void onEnterDetect(Collider2D collision)
    {
        if (this._owner is Client client && !client.isServer) return;
        Entity entity = collision.GetComponent<Entity>();
        if (entity && _serverClient && entity.ServerClient && _owner != entity.Owner)
        {
            //Debug.Log("adding to targets to follow: " + entity);
            this._targetsToFollow.Add(entity);
            if (_currentTarget == null)
            {
                _currentTarget = entity;
                _currentEntityState = AttackingEntity.EntityState.WalkingToTarget;
                this.detectRingOpacity = 1f;
                //Debug.Log(this + " follows " + _currentTarget);
            }
        }
    }

    /// <summary>
    /// This method is called when a new collision exits the Detect Ring
    /// </summary>
    /// <param name="collision"></param>
    public void onExitDetect(Collider2D collision)
    {
        if (this._owner is Client client && !client.isServer) return;
        Entity entity = collision.GetComponent<Entity>();
        if (entity && _serverClient && entity.ServerClient && entity.Owner != _owner)
        {
            //Debug.Log("removing from targets to follow: " + entity);
            _targetsToFollow.Remove(entity);
            if (_currentTarget == entity)
            {
                searchNewTarget();
            }
        }
    }

    /// <summary>
    /// This method will dye and name troops to visually reflect if they are owned by the player or enemies
    /// </summary>
    public void dyeAndNameTroop(Player newOwner)
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
    /// This method will visualize the path of the troop
    /// </summary>
    private void createPathLine()
    {
        if (this.Owner.isLocalPlayer)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.gameObject.SetActive(true);
            foreach (Vector2 position in this._path)
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            }
            lineRenderer.gameObject.GetComponent<LineRendererController>().decayOverTime(3);
        }
    }

    public override void detectClick()
    {
        Client localClient = GameObject.Find("LocalClient").GetComponent<Client>();
        if(localClient.getClientState() == "ViewingState")
        {
            createPathLine();
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
        if(this.CurrentEntityState == EntityState.Attacking)
        {
            result.Add("CurrentTarget", this.CurrentTarget.ToString());
        }
        return result;
    }

    public void onDetectRingChangeOpacity(float oldOpacity, float newOpacity)
    {
        Invoke("updateDetectRingOpacity", 0.1f);
    }

    public void onDetectRingChangeScale(Vector3 oldScale, Vector3 newScale)
    {
        Invoke("updateDetectRingScale", 0.1f);
    }

    private void updateDetectRingOpacity()
    {
        this.ServerClient.updateDetectRingOfGameObject(this.gameObject, this.detectRingOpacity);
    }

    private void updateDetectRingScale()
    {
        this.ServerClient.updateDetectRingOfGameObject(this.gameObject, this.detectRingScale);
    }
}
