using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class GroundEnemy : GroundActor
{
    [SerializeField] bool isStartingAggroed = false;
    [SerializeField] bool isPatrolling = true;
    [SerializeField] bool canChasePlayerOffLand = true;
    protected EnemyController enemyController;

    public bool IsStartingAggroed { get => isStartingAggroed; set => isStartingAggroed = value; }
    public bool IsPatrolling { get => isPatrolling; set => isPatrolling = value; }

    protected override void OnAwake()
    {
        base.OnAwake();
        enemyController = GetComponent<EnemyController>();
        enemyController.Init(this, objectPool);
    }

    protected override void OnStart()
    {
        base.OnStart();
        InitStateMachine(enemyController.StateMachine);
        enemyController.StateMachine.StartMachine();
    }

    protected virtual void InitStateMachine(EnemyStateMachine stateMachine)
    {
        if (!isStartingAggroed)
        {
            stateMachine.AddState(new IdleEnemyState(isPatrolling));
        }
        stateMachine.AddState(new AggroEnemyState(canChasePlayerOffLand: canChasePlayerOffLand));

        if (enemyController.HasMultipleAttacks())
        {
            var attackInfos = enemyController.GetAttackInfos();

            foreach (var attackInfo in attackInfos)
            {
                stateMachine.AddAttackState(new AttackEnemyState(attackInfo));
            }
        }
        else
        {
            stateMachine.AddState(new AttackEnemyState());
        }
        stateMachine.AddState(new JuggleEnemyState());
    }

    protected override void Update()
    {
        base.Update();
        Animator.SetFloat("H", Mathf.Abs(rb.velocity.x));
    }

    protected override Vector2 GetInputMove()
    {
        return enemyController.GetInputMove();
    }

    public override int OnDamage(int damage)
    {
        return base.OnDamage(damage);
    }

    public override void OnDeath()
    {
        GetCameraEffect().ShackCamera(6f, 0.1f);
        base.OnDeath();
    }
}
