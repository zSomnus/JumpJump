using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{
    Dictionary<string, EnemyState> stateByName = new Dictionary<string, EnemyState>();
    EnemyState currentState;
    EnemyController controller;
    Player player;
    int duplicationIndex = 1;

    public bool HasMultipleAttacks
    {
        get => attackStates.Count > 0;
    }

    public List<AttackEnemyState> attackStates = new List<AttackEnemyState>();

    public EnemyStateMachine(EnemyController enemyController, Player player)
    {
        controller = enemyController;
        controller.RaiseAttackEvent += () => { currentState?.OnAttack(); };
        controller.RaisePostAttackEvent += () => { currentState?.OnPostAttack(); };
        controller.RaisePreAttackEvent += () => { currentState?.OnPreAttack(); };
        controller.RaiseDamageEvent += (damageValue, damageType) =>
        {
            currentState?.OnDamage(damageValue, player.transform.position);
            //if (damageValue > 0 && !controller.IsHeavy && !controller.IsBoss() && damageType == DamageType.Melee)
            //{
            //    currentState?.OnHitStun();
            //}
        };
        controller.RaiseCustomAnimEvent += (s) => { currentState?.OnCustomAnimEvent(s); };
        this.player = player;
    }

    public void Update()
    {
        currentState?.Update();
    }

    public void StartMachine()
    {
        foreach (var state in stateByName.Values)
        {
            state.OnInit();
        }
        currentState?.OnEnter();
    }

    internal void TransitionToRandomAttackState()
    {
        TransitionTo(GetRandomAttackState());
    }

    public AttackEnemyState GetRandomAttackState()
    {
        return attackStates[UnityEngine.Random.Range(0, attackStates.Count)];
    }

    public void AddState(EnemyState state)
    {
        if (currentState == null)
        {
            currentState = state;
        }
        state.StateMachine = this;
        state.Controller = controller;
        state.Player = player;
        if (!stateByName.ContainsKey(state.CustomName))
        {
            stateByName.Add(state.CustomName, state);
        }
        else
        {
            state.CustomName += ++duplicationIndex;
            stateByName.Add(state.CustomName, state);
        }
    }

    public void AddAttackState(AttackEnemyState state)
    {
        attackStates.Add(state);
        AddState(state);
    }

    public void TransitionTo(EnemyState enemyState)
    {
        currentState?.OnExit();
        currentState = enemyState;
        currentState.OnEnter();
    }

    public void TransitionTo<T>() where T : EnemyState
    {
        string stateName = typeof(T).Name;
        TransitionTo(stateName);
    }

    public void TransitionTo(string stateName)
    {
        if (stateByName.ContainsKey(stateName))
        {
            TransitionTo(stateByName[stateName]);
        }
        else
        {
            Debug.Log($"Cannot find state {stateName}");
        }
    }

    internal Vector2 GetInputMove()
    {
        return currentState?.GetInputMove() ?? Vector2.zero;
    }

    internal bool IsIn<T>()
    {
        return currentState is T;
    }
}
