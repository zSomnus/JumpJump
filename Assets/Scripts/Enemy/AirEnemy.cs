using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirEnemy : Enemy
{
    [SerializeField] ParticleSystem deathParticle;
    protected override void OnStart()
    {
        base.OnStart();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    protected override void OnPostDeath()
    {
        base.OnPostDeath();
        deathParticle.Play();
    }
}
