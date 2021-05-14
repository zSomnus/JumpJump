using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirEnemy : AirActor
{
    public override int OnDamage(int damage)
    {
        Animator.SetTrigger("TakeHit");

        return base.OnDamage(damage);
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
