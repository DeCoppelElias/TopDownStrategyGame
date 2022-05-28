using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public abstract class Troop : AttackingEntity
{
    [SerializeField]
    private List<Vector2> _path = new List<Vector2>();
    [SyncVar]
    protected List<Entity> _targetsToFollow = new List<Entity>();
    public List<Vector2> Path
    {
        set => _path = value;
    }

    [SerializeField]
    private int _speed;

    [SerializeField]
    private int _cost;
    public int Cost { 
        get => _cost;
    }

    /// <summary>
    /// This will update a troop, it will make the troop attack, follow or continue on his path
    /// </summary>
    public void updateTroop()
    {
        if (_entityState.Equals(EntityState.Normal))
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
        }
        if (_entityState.Equals(EntityState.WalkingToTarget))
        {
            if (_currentTarget)
            {
                var step = _speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, _currentTarget.transform.position, step);
            }
            else
            {
                _entityState = EntityState.Normal;
            }
        }
        if (_entityState.Equals(EntityState.Attacking))
        {
            if(Time.time - _lastAttack > _cooldown)
            {
                attackEntity(_currentTarget);
            }
            if(_currentTarget == null || _currentTarget.Health <= 0)
            {
                killTarget();
            }
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
    protected override void updateOwnerClientEventSpecific(Player oldClient, Player newClient) { }

    /// <summary>
    /// This method is called when a new collision enters the Detect Ring
    /// </summary>
    /// <param name="collision"></param>
    public void onEnterDetect(Collider2D collision)
    {;
        Entity entity = collision.GetComponent<Entity>();
        if (entity && _serverClient && entity.ServerClient && _owner != entity.Owner)
        {
            this._targetsToFollow.Add(entity);
            if (_currentTarget == null)
            {
                _currentTarget = entity;
                _entityState = AttackingEntity.EntityState.WalkingToTarget;
                Debug.Log(this + " follows " + _currentTarget);
            }
        }
    }

    /// <summary>
    /// This method is called when a new collision exits the Detect Ring
    /// </summary>
    /// <param name="collision"></param>
    public void onExitDetect(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity && _serverClient && entity.ServerClient && _currentTarget == entity && entity.Owner != _owner)
        {
            _targetsToFollow.Remove(entity);
            if (_targetsToFollow.Count == 0)
            {
                _currentTarget = null;
                _entityState = AttackingEntity.EntityState.Normal;
                Debug.Log(this + " has lost sight of " + entity);
            }
            else if (_currentTarget == entity && _targetsToFollow.Count > 0)
            {
                _currentTarget = _targetsToFollow[0];
                _entityState = AttackingEntity.EntityState.WalkingToTarget;
                Debug.Log(this + " follows " + _currentTarget);
            }
        }
    }
}
