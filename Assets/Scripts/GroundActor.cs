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
    [SerializeField] protected Vector2 bottomOffset;
    [SerializeField] protected Vector2 boxSizeGround;
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected AudioClip landingAudio;

    protected override void Update()
    {
        base.Update();
        SimulatePhysics();
    }

    protected virtual void SimulatePhysics()
    {
        if (IsOnGround())
        {
            rb.gravityScale = 0;

            if (landingAudio != null)
            {
                GameObject audioSourceObj = objectPool.GetFromPool("AudioSource");
                audioSourceObj.transform.position = transform.position;
                audioSourceObj.SetActive(true);
                audioSourceObj.GetComponent<AudioSource>().PlayOneShot(landingAudio);
            }
        }
        else
        {
            rb.gravityScale = 1;
            // simulate physics
            if (rb.velocity.y < minJumpSpeed && rb.velocity.y > -maxFallSpeed)
            {

                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;

            }
            else if (rb.velocity.y > 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
    }

    protected virtual bool IsOnGround()
    {
        return Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, boxSizeGround, 0f, groundLayer);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position;
        Gizmos.DrawWireCube(new Vector3(center.x + bottomOffset.x, center.y + bottomOffset.y, center.z), boxSizeGround);
    }
}
