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

    private GameObject detectRing;

    public override void Start()
    {
        base.Start();
        this.lineRenderer = this.GetComponentInChildren<LineRenderer>();
        lineRenderer.positionCount = 0;


        this.detectRing = this.transform.Find("DetectRing").gameObject;
        this.detectRing.transform.localScale = new Vector3((viewRange * 2) + 1, (viewRange * 2) + 1, 0);
        Color alphaColor = detectRing.GetComponent<SpriteRenderer>().color;
        alphaColor.a = 0.2f;
        detectRing.GetComponent<SpriteRenderer>().color = alphaColor;
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

            Color alphaColor = attackRing.GetComponent<SpriteRenderer>().color;
            alphaColor.a = 0.2f;
            attackRing.GetComponent<SpriteRenderer>().color = alphaColor;

            alphaColor = detectRing.GetComponent<SpriteRenderer>().color;
            alphaColor.a = 0.2f;
            detectRing.GetComponent<SpriteRenderer>().color = alphaColor;
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

            Color alphaColor = attackRing.GetComponent<SpriteRenderer>().color;
            alphaColor.a = 0.2f;
            attackRing.GetComponent<SpriteRenderer>().color = alphaColor;

            alphaColor = detectRing.GetComponent<SpriteRenderer>().color;
            alphaColor.a = 1f;
            detectRing.GetComponent<SpriteRenderer>().color = alphaColor;
        }
        if (_currentEntityState.Equals(EntityState.Attacking))
        {
            attackTarget();

            Color alphaColor = attackRing.GetComponent<SpriteRenderer>().color;
            alphaColor.a = 1;
            attackRing.GetComponent<SpriteRenderer>().color = alphaColor;

            alphaColor = detectRing.GetComponent<SpriteRenderer>().color;
            alphaColor.a = 1f;
            detectRing.GetComponent<SpriteRenderer>().color = alphaColor;
        }
    }

    /// <summary>
    /// This method is called when a troop is killed. It does all the needed procedures before actually deleting the object
    /// </summary>
    protected override void killTarget()
    {
        _currentEntityState = EntityState.Normal;
        Debug.Log("Killing target: " + this._currentTarget);
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
        }
        else if (_targetsToAttack.Count > 0)
        {
            _currentTarget = _targetsToAttack[0];
            _currentEntityState = AttackingEntity.EntityState.Attacking;
        }
        else if (_targetsToFollow.Count > 0)
        {
            _currentTarget = _targetsToFollow[0];
            _currentEntityState = AttackingEntity.EntityState.WalkingToTarget;
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
    protected override void updateOwnerClientEventSpecific(Player oldClient, Player newClient)
    {
        Debug.Log("changed owner client of " + this + " from " + oldClient + " to " + newClient);
        dyeAndNameTroop();
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
    public void dyeAndNameTroop()
    {
        if (GameObject.Find("LocalClient") && this.Owner == GameObject.Find("LocalClient").GetComponent<Client>())
        {
            this.name = "Local" + this.name;
            float r = 88;  // red component
            float g = 222;  // green component
            float b = 255;  // blue component
            float a = this.gameObject.GetComponent<SpriteRenderer>().color.a;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, a);
        }
        else if (this.Owner is AiClient)
        {
            this.name = "Ai" + this.name;
            float r = 95;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            float a = this.gameObject.GetComponent<SpriteRenderer>().color.a;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, a);
        }
        else if (this.Owner != null)
        {
            this.name = "Enemy" + this.name;
            float r = 255;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            float a = this.gameObject.GetComponent<SpriteRenderer>().color.a;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, a);
        }
        else if (this.Owner == null)
        {
            this.name = "Lost" + this.name;
            float r = 255;  // red component
            float g = 255;  // green component
            float b = 255;  // blue component
            float a = this.gameObject.GetComponent<SpriteRenderer>().color.a;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, a);
        }
    }

    /// <summary>
    /// This method will visualize the path of the troop
    /// </summary>
    public void createPathLine()
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

    public Dictionary<string, object> getInfo()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add("Name", this.name);
        result.Add("Damage", this.Damage);
        result.Add("AttackCooldown", this.AttackCooldown);
        result.Add("Range", this.Range);
        result.Add("CurrentEntityState", this.CurrentEntityState);
        result.Add("CurrentTarget", this.CurrentTarget);
        return result;
    }
}
