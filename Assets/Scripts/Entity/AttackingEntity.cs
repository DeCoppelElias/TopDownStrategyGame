using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class AttackingEntity : Entity
{
    [SyncVar]
    [SerializeField]
    protected int _damage = 10;
    public int Damage { get => _damage; }

    [SerializeField]
    protected float _range = 2;
    public float Range { get => _range; }

    public enum EntityState { Normal, WalkingToTarget, Attacking }
    [SyncVar]
    [SerializeField]
    protected EntityState _currentEntityState = EntityState.Normal;
    public EntityState CurrentEntityState { get => _currentEntityState; }

    [SyncVar]
    [SerializeField]
    protected float _attackCooldown = 1;
    public float AttackCooldown { get => _attackCooldown; }

    [SyncVar]
    [SerializeField]
    protected float _lastAttack;

    [SyncVar]
    [SerializeField]
    protected Entity _currentTarget;
    public Entity CurrentTarget { get => _currentTarget; }

    [SyncVar]
    [SerializeField]
    protected List<Entity> _targetsToAttack = new List<Entity>();
    [SerializeField]
    private int _cost;
    public int Cost { get => _cost; }

    protected GameObject attackRing;

    public virtual void Start()
    {
        this.attackRing = this.transform.Find("AttackRing").gameObject;
        attackRing.transform.localScale = new Vector3((_range * 2)+1, (_range * 2) + 1, 0);
        Color alphaColor = attackRing.GetComponent<SpriteRenderer>().color;
        alphaColor.a = 0.2f;
        attackRing.GetComponent<SpriteRenderer>().color = alphaColor;
    }

    /// <summary>
    /// This method is called when a new collision enters the Attack Ring
    /// </summary>
    /// <param name="collision"></param>
    public void onEnterAttack(Collider2D collision)
    {
        if (this._owner is Client client && !client.isServer) return;
        Entity entity = collision.GetComponent<Entity>();
        if (entity && _serverClient && entity.ServerClient && _owner != entity.Owner)
        {
            //Debug.Log("adding new entity to targets to attack: " + entity);
            _targetsToAttack.Add(entity);
            if (_currentTarget == entity)
            {
                _currentEntityState = AttackingEntity.EntityState.Attacking;
                //Debug.Log(this + " will now attack " + _currentTarget);
            }
            else if (_currentTarget == null)
            {
                _currentTarget = entity;
                _currentEntityState = AttackingEntity.EntityState.Attacking;
                //Debug.Log(this + " will now attack " + _currentTarget);
            }
        }
    }

    /// <summary>
    /// This method is called when a new collision exits the Attack Ring
    /// </summary>
    /// <param name="collision"></param>
    public void onExitAttack(Collider2D collision)
    {
        if (this._owner is Client client && !client.isServer) return;
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && _serverClient != null && entity.ServerClient != null)
        {
            //Debug.Log("removing entity from targets to attack: " + entity);
            _targetsToAttack.Remove(entity);
            if (_currentTarget == entity)
            {
                searchNewTarget();
            }
        }
    }

    /// <summary>
    /// This method will kill the target object and reset the target parameter
    /// </summary>
    protected virtual void killTarget()
    {
        _currentEntityState = EntityState.Normal;
        Debug.Log("Killing target: " + this._currentTarget);
        this._targetsToAttack.Remove(this._currentTarget);
        this._currentTarget.getKilled();
        this._currentTarget = null;
    }

    /// <summary>
    /// This method will attack a given entity
    /// </summary>
    /// <param name="entity"></param>
    public void attackEntity(Entity entity)
    {
        entity.Health -= _damage;
        _lastAttack = Time.time;
    }

    /// <summary>
    /// This method is called to attack the current target
    /// </summary>
    protected void attackTarget()
    {
        if (!_currentTarget) return;
        if (Time.time - _lastAttack > _attackCooldown)
        {
            attackEntity(_currentTarget);
        }
        if (_currentTarget.Health <= 0)
        {
            killTarget();
            searchNewTarget();
        }
    }

    /// <summary>
    /// This method will search for a new target in the target to attack list
    /// </summary>
    protected virtual void searchNewTarget()
    {
        if(this._targetsToAttack.Count > 0)
        {
            this._currentTarget = this._targetsToAttack[0];
            this._currentEntityState = EntityState.Attacking;
        }
        else
        {
            this._currentTarget = null;
            this._currentEntityState = EntityState.Normal;
        }
    }
}
