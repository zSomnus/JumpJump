using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirEnemy : Enemy
{
    [SerializeField] ParticleSystem deathParticle;
    Animator animator;
    protected override void OnStart()
    {
        base.OnStart();
        animator = GetComponent<Animator>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    protected override void OnPostDeath()
    {
        base.OnPostDeath();
        GameObject temp = Instantiate(deathParticle.gameObject);
        temp.transform.position = transform.position;
        Destroy(temp, 0.5f);
        animator.SetTrigger("TakeHit");
    }
}
