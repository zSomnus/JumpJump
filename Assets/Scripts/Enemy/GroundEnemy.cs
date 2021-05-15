using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEnemy : GroundActor
{
    protected override void Update()
    {
        base.Update();
        Animator.SetFloat("H", Mathf.Abs(rb.velocity.x));
    }

    protected override void FlipSprite()
    {
        //base.FlipSprite();
    }

    public override void OnDeath()
    {
        GetCameraEffect().ShackCamera(6f, 0.1f);
        GameObject temp = objectPool.GetFromPool("DeathParticle");
        temp.transform.position = transform.position;
        temp.SetActive(true);
        base.OnDeath();
    }
}
