using System;
using UnityEngine;

public class EnemyState
{
    public EnemyStateMachine StateMachine { get; set; }
    public EnemyController Controller { get; set; }
    public Player Player { get; set; }
    public string CustomName { get; set; }

    protected EnemyState()
    {
        CustomName = GetType().Name;
    }

    protected EnemyState(string customName)
    {
        CustomName = customName;
    }

    public void TransitionTo<T>() where T : EnemyState
    {
        Debug.Log($"{Controller.enemyName} switching to state {typeof(T).Name}.");
        StateMachine.TransitionTo<T>();
    }

    public void TransitionTo(string stateName)
    {
        Debug.Log($"{Controller.enemyName} switching to state {stateName}");
        StateMachine.TransitionTo(stateName);
    }

    public virtual void OnInit() { }
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Update() { }
    public virtual void OnPreAttack() { }
    public virtual void OnAttack() { }
    public virtual void OnPostAttack() { }
    public virtual void OnCustomAnimEvent(string name) { }
    public virtual void OnDamage(int dame, Vector3? sourcePosition = null) { }
    public virtual void OnJuggleHit() { }
    public virtual void OnHitStun() { }
    public virtual Vector2 GetInputMove()
    {
        return Vector3.zero;
    }
    protected Vector2 GetDistanceToPlayer()
    {
        return GetAttackTarget().transform.position - Controller.transform.position;
    }

    public Actor GetAttackTarget()
    {
        return Player;
    }
}
