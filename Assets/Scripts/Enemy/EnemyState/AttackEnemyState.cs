using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyState : EnemyState
{
    float currentCooldown;
    protected bool isAnimDone;
    public readonly EnemyAttackInfo enemyAttackInfo;
    bool canTurn;
    public Action<EnemyState> OnAttackCooldownAction = null;
    protected bool IsDamageResetAttack = true;

    public AttackEnemyState(bool canTurn = true)
    {
        this.canTurn = canTurn;
    }

    public AttackEnemyState(EnemyAttackInfo enemyAttackInfo, bool canTurn = true) : base(enemyAttackInfo.animTriggerName)
    {
        this.enemyAttackInfo = enemyAttackInfo;
        this.canTurn = canTurn;
    }

    public override void OnJuggleHit()
    {
        base.OnJuggleHit();
        if (IsDamageResetAttack)
        {
            if (Controller.ContainsAnimParam("reset"))
            {
                Controller.TriggerAnim("reset");
            }
            TransitionTo<JuggleEnemyState>();
        }
    }

    public override void OnPostAttack()
    {
        isAnimDone = true;
    }

    public override void OnEnter()
    {
        if (canTurn)
        {
            Controller.FacePlayer();
        }
        isAnimDone = false;
        currentCooldown = Controller.GetAttackCooldown();

        if (StateMachine.HasMultipleAttacks)
        {
            Controller.ActiveItemControllerIndex = enemyAttackInfo.itemControllerIndex;
        }
    }
}
