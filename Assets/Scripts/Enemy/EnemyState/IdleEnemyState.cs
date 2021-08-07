using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleEnemyState : EnemyState
{
    float initialAggroTimer;
    public bool IsPatrolling { get; set; }

    int patrolDirection;
    float patrolTime;
    const float PatrolMinTime = 0.5f;
    const float PatrolMaxTime = 2f;
    bool isPatrolPausing;
    float pauseTimer;
    const float PauseDuration = 2f;

    public IdleEnemyState(bool isPatrolling = false)
    {
        IsPatrolling = isPatrolling;
    }

    public override void OnEnter()
    {
        initialAggroTimer = Controller.InitialAggroTimer;
        patrolTime = CreatePatrolTime();
        pauseTimer = PauseDuration;
    }

    public override void Update()
    {
        if (initialAggroTimer > 0)
        {
            initialAggroTimer -= Time.deltaTime;
            if (initialAggroTimer <= 0)
            {
                OnAggro();
                return;
            }
        }
    }

    float CreatePatrolTime()
    {
        return Random.Range(PatrolMinTime, PatrolMaxTime);
    }

    protected virtual void OnAggro()
    {
        Controller.OnAggro();
        TransitionTo<AggroEnemyState>();
    }
}
