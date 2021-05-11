using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEnemy : Enemy
{
    Animator animator;
    Rigidbody2D rb;
    protected override void OnStart()
    {
        base.OnStart();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        animator.SetFloat("H", rb.velocity.x);
    }
}
