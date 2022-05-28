using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class AttackingEntity : Entity
{
    [SyncVar]
    protected int _damage = 10;
    protected enum EntityState { Normal, WalkingToTarget, Attacking }
    [SyncVar]
    protected EntityState _entityState = EntityState.Normal;
    [SyncVar]
    protected float _cooldown = 1;
    [SyncVar]
    protected float _lastAttack;
    [SyncVar]
    protected Entity _currentTarget;
    [SyncVar]
    protected List<Entity> _targetsToAttack = new List<Entity>();

    /// <summary>
    /// This method is called when a new collision enters the Attack Ring
    /// </summary>
    /// <param name="collision"></param>
    public void onEnterAttack(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity && _serverClient && entity.ServerClient && _owner != entity.Owner)
        {
            _targetsToAttack.Add(entity);
            if (_currentTarget == entity)
            {
                _entityState = AttackingEntity.EntityState.Attacking;
                Debug.Log(this + " will now attack " + _currentTarget);
            }
            else if (_currentTarget == null)
            {
                _currentTarget = entity;
                _entityState = AttackingEntity.EntityState.Attacking;
                Debug.Log(this + " will now attack " + _currentTarget);
            }
        }
    }

    /// <summary>
    /// This method is called when a new collision exits the Attack Ring
    /// </summary>
    /// <param name="collision"></param>
    public void onExitAttack(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity && _serverClient && entity.ServerClient && _currentTarget == entity && _owner)
        {
            _targetsToAttack.Remove(entity);
            if (_targetsToAttack.Count == 0)
            {
                _currentTarget = entity;
                _entityState = AttackingEntity.EntityState.WalkingToTarget;
                Debug.Log(this + " is now following " + _currentTarget);
            }
            else if (entity == _currentTarget && _targetsToAttack.Count > 0)
            {
                _currentTarget = _targetsToAttack[0];
                _entityState = AttackingEntity.EntityState.Attacking;
                Debug.Log(this + " will now attack " + _currentTarget);
            }
        }
    }

    public void killTarget()
    {
        _entityState = EntityState.Normal;
        this._currentTarget.getKilled();
        this._currentTarget = null;
    }

    public void attackEntity(Entity entity)
    {
        entity.Health -= _damage;
        _lastAttack = Time.time;
    }
}
