using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroEnemyState : EnemyState
{
    const float defaultChargeTime = 1f;
    float chargeTime;
    protected const float DistanceThreshold = 0.15f;
    readonly string soundWhenAggro;
    bool canChasePlayerOffLand;
    bool hasCrossedAttackThreshold;

    public AggroEnemyState(string soundWhenAggro = "", bool canChasePlayerOffLand = true)
    {
        this.soundWhenAggro = soundWhenAggro;
        this.canChasePlayerOffLand = canChasePlayerOffLand;

    }

    public override void OnEnter()
    {
        base.OnEnter();
        ResetChargeTime();

        Controller.SetColliderActive(true);
    }

    private void ResetChargeTime()
    {
        chargeTime = Mathf.Max(Controller.GetAttackChargeTime(), defaultChargeTime);
    }

    public override void Update()
    {
        if (!Player.IsAlive())
        {
            return;
        }

        Controller.DropIfPlayerUnder();

        if (chargeTime > 0)
        {
            chargeTime -= Time.deltaTime;
        }
        else if (CanAttackNow())
        {
            TransitionToAttack();
        }
    }

    protected virtual bool CanAttackNow() => Controller.CanAttackNow();

    protected virtual void TransitionToAttack()
    {
        if (StateMachine.HasMultipleAttacks)
        {
            StateMachine.TransitionToRandomAttackState();
        }
        else
        {
            TransitionTo<AttackEnemyState>();
        }
    }

    public override Vector2 GetInputMove()
    {
        if (chargeTime > 0 && hasCrossedAttackThreshold)
        {
            return Vector2.zero;
        }

        float distance = GetDistanceToPlayer().x;

        if (CanAttackNow() || Mathf.Abs(distance) < DistanceThreshold)
        {
            hasCrossedAttackThreshold = true;
            return Vector2.zero;
        }

        if (!canChasePlayerOffLand && !Controller.CanMoveForwardGrounded(distance > 0 ? 1 : -1))
        {
            return Vector2.zero;
        }

        return distance > 0 ? Vector2.right : Vector2.left;
    }
}
