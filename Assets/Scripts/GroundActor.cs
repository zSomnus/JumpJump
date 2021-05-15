using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundActor : Actor
{
    [SerializeField] protected float jumpSpeed = 12;
    [SerializeField] protected float minJumpSpeed = 10;
    [SerializeField] protected int maxFallSpeed = 15;
    [SerializeField] protected int fallMultiplier = 8;
    [SerializeField] protected int lowJumpMultiplier = 30;

    protected override void Update()
    {
        base.Update();
        SimulatePhysics();
    }

    protected virtual void SimulatePhysics()
    {
        // simulate physics
        if (rb.velocity.y < minJumpSpeed && rb.velocity.y > -maxFallSpeed)
        {

            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;

        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
}
