using System;
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

    [SerializeField] BoxCollider2D feetCollider;
    protected BoxCollider2D boxCollider2D;
    Player playerCache;

    public bool IsGravityOn
    {
        get => controller2D.IsGravityOn;
        set => controller2D.IsGravityOn = value;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (feetCollider != null)
        {
            boxCollider2D = feetCollider;
        }
        else
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
        }
        controller2D = CreateController2D();
        controller2D.Init(boxCollider2D, this);
    }

    protected override void OnStart()
    {
        base.OnStart();
        playerCache = D.Get<Player>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        rb.gravityScale = 1;
    }
    protected override void Update()
    {
        base.Update();

        controller2D.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        SimulatePhysics();
    }

    protected virtual void SimulatePhysics()
    {
        if (IsOnGround())
        {
            rb.gravityScale = 0;

            if (landingAudio != null)
            {
                GameObject landingAudioObj = objectPool.GetFromPool("AudioSource");
                landingAudioObj.transform.position = transform.position;
                landingAudioObj.GetComponent<AudioPlayer>().SetAudioClip(landingAudio);
                landingAudioObj.SetActive(true);
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

    public override bool IsGrounded()
    {
        return controller2D?.IsGrounded() ?? false;
    }

    public bool IsFalling()
    {
        return controller2D?.IsFalling() ?? false;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position;
        Gizmos.DrawWireCube(new Vector3(center.x + bottomOffset.x, center.y + bottomOffset.y, center.z), boxSizeGround);
    }
}
