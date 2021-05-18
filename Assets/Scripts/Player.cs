using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STATE
{
    None,
    Jumping,
    WallSliding,
    Attacking,
    WallJumping,
    Grounding,
    InAir
};
public enum DIR
{
    Center,
    Right,
    Left,
    Up,
    Down,
    UpRight,
    UpLeft,
    DownRight,
    DownLeft
};

public class Player : GroundActor
{
    [SerializeField] GameObject[] animationObjects;

    [Header("Move")]
    [SerializeField] float wallSlideSpeed;
    bool onRightWall;
    bool onLeftWall;
    DIR movingDirection;
    STATE state;

    [Header("Jump")]
    [SerializeField] int maxAirJumpCount;
    int airJumpCount;
    [SerializeField] Animator wingAnimator;
    [SerializeField] Vector2 wallJumpFource;
    [SerializeField] ParticleSystem landingParticle;
    [SerializeField] AudioClip doubleJumpAudio;
    bool isLandingParticlePlayed;
    bool isWallSlideAudioPlayed;

    [Header("Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCD;
    [SerializeField] float shadowCD;
    [SerializeField] AudioClip dashAudio;
    bool isDashing;
    bool canDash;
    bool isDashCD;
    bool isShadowStart;

    [Header("Collision")]
    [SerializeField] private Vector2 rightOffset;
    [SerializeField] private Vector2 leftOffset;
    [SerializeField] private Vector2 boxSizeWall;

    [Header("Melee Attack")]
    [SerializeField] Animator weaponAnimator;

    [Header("Ranged Attack")]
    [SerializeField] float rangedCD;
    bool canShoot;

    protected override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    protected override void Init()
    {
        base.Init();
        canShoot = true;
        canDash = true;
        isDashing = false;
        isDashCD = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        PlayerState();
        movingDirection = GetMovingDirection();
        Dash();
        Jump();
        Shoot();
        DoubleJump();
        Attack();

        if (state == STATE.Grounding || isDashing)
        {
            rb.gravityScale = 0;

            if (!isLandingParticlePlayed)
            {
                landingParticle.Play();
                isLandingParticlePlayed = true;

                // Play landing audio
                if (landingAudio != null && state == STATE.Grounding)
                {
                    GameObject audioSourceObj = objectPool.GetFromPool("AudioSource");
                    audioSourceObj.transform.position = transform.position;
                    audioSourceObj.GetComponent<AudioPlayer>().SetAudioClip(landingAudio);
                    audioSourceObj.SetActive(true);
                }
            }
        }
        else
        {
            rb.gravityScale = 1;
            isLandingParticlePlayed = false;
        }

        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isDashing && state != STATE.WallJumping)
        {
            Run();
        }

        ShowPlayerShadow();
    }

    void ShowPlayerShadow()
    {
        if (isDashing)
        {
            if (!isShadowStart)
            {
                StartCoroutine(GetShadow());
            }
        }
    }

    protected override void FlipSprite()
    {
        if (state == STATE.Grounding)
        {
            bool playerHasHorizontalSpeed = Input.GetAxis("Horizontal") != 0;

            if (playerHasHorizontalSpeed)
            {
                transform.localScale = new Vector2(Mathf.Sign(Input.GetAxis("Horizontal")), 1f);
            }
        }
        else
        {
            base.FlipSprite();
        }
    }

    void Run()
    {
        float runInput = Input.GetAxis("Horizontal");

        if (runInput > 0)
        {
            runInput = GetMoveSpeed();
        }
        else if (runInput < 0)
        {
            runInput = -GetMoveSpeed();
        }
        else
        {
            runInput = 0f;
        }
        rb.velocity = new Vector2(runInput, rb.velocity.y);
    }

    void Jump()
    {
        if (state == STATE.Grounding && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(0, jumpSpeed);
        }
    }

    void DoubleJump()
    {
        if (state == STATE.InAir && airJumpCount < maxAirJumpCount && Input.GetButtonDown("Jump"))
        {
            wingAnimator.SetTrigger("DoubleJump");
            rb.velocity = new Vector2(0, jumpSpeed);
            airJumpCount++;

            // play double jump audio
            if (doubleJumpAudio != null)
            {
                GameObject doubleJumpAudioObj = objectPool.GetFromPool("AudioSource");
                doubleJumpAudioObj.transform.position = transform.position;
                doubleJumpAudioObj.GetComponent<AudioPlayer>().SetAudioClip(doubleJumpAudio, 0.3f);
                doubleJumpAudioObj.SetActive(true);
            }
        }
    }

    void WallJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            StartCoroutine(WallJumpTimer());
        }
    }

    IEnumerator WallJumpTimer()
    {
        state = STATE.WallJumping;
        float x = 0;

        if (onLeftWall)
        {
            x = wallJumpFource.x;
        }
        else if (onRightWall)
        {
            x = -wallJumpFource.x;
        }

        rb.velocity = new Vector2(x, wallJumpFource.y);
        yield return new WaitForSeconds(0.2f);
        state = STATE.InAir;
    }

    void Attack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            weaponAnimator.SetTrigger("Attack");
        }
    }

    void Dash()
    {
        if (state != STATE.WallSliding && !isDashing && !isDashCD && canDash && (Input.GetButtonDown("Dash") || Input.GetAxisRaw("Dash") > 0.2f))
        {
            Vector2 velocity = Vector2.zero;

            switch (movingDirection)
            {
                case DIR.Up:
                    velocity = Vector2.up * dashSpeed * 0.7f;
                    break;
                case DIR.Down:
                    velocity = Vector2.down * dashSpeed * 0.7f;
                    break;
                case DIR.Left:
                    velocity = Vector2.left * dashSpeed;
                    break;
                case DIR.Right:
                    velocity = Vector2.right * dashSpeed;
                    break;
                case DIR.UpRight:
                    velocity = Vector2.up * dashSpeed * 0.7f + Vector2.right * dashSpeed * 0.7f;
                    break;
                case DIR.DownRight:
                    velocity = Vector2.down * dashSpeed * 0.7f + Vector2.right * dashSpeed * 0.7f;
                    break;
                case DIR.UpLeft:
                    velocity = Vector2.up * dashSpeed * 0.7f + Vector2.left * dashSpeed * 0.7f;
                    break;
                case DIR.DownLeft:
                    velocity = Vector2.down * dashSpeed * 0.7f + Vector2.left * dashSpeed * 0.7f;
                    break;
                default:
                    velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
                    break;
            }

            StartCoroutine(Dash(velocity, dashDuration));
            StartCoroutine(DashCooldown(dashCD));
        }

    }

    void Shoot()
    {
        if (Input.GetButton("Fire2") && canShoot)
        {
            StartCoroutine(ShootCooldownCount());
            GameObject bullet = objectPool.GetFromPool("Bullet");
            bullet.SetActive(true);
        }
    }

    IEnumerator ShootCooldownCount()
    {
        canShoot = false;
        yield return new WaitForSeconds(rangedCD);
        canShoot = true;
    }

    IEnumerator GetShadow()
    {
        isShadowStart = true;
        objectPool.GetFromPool("PlayerShadow").SetActive(true);
        yield return new WaitForSeconds(shadowCD);
        isShadowStart = false;
    }

    IEnumerator DashCooldown(float cd)
    {
        isDashCD = true;
        yield return new WaitForSeconds(cd);
        isDashCD = false;
    }

    IEnumerator Dash(Vector2 velocity, float duration)
    {
        canDash = false;
        rb.velocity = velocity;
        isDashing = true;

        // Play dash audio
        if (dashAudio != null)
        {
            GameObject dashAudioObj = objectPool.GetFromPool("AudioSource");
            dashAudioObj.transform.position = transform.position;
            dashAudioObj.GetComponent<AudioPlayer>().SetAudioClip(dashAudio, 0.3f, 0.5f);
            dashAudioObj.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        isDashing = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1;
    }

    protected override void SimulatePhysics()
    {
        if (!isDashing && (state != STATE.WallJumping) && (state != STATE.Grounding))
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

    private DIR GetMovingDirection()
    {
        if (Input.GetAxis("Vertical") > 0.2f)
        {
            if (Input.GetAxis("Horizontal") > 0.2f)
            {
                return DIR.UpRight;
            }
            else if (Input.GetAxis("Horizontal") < -0.2f)
            {
                return DIR.UpLeft;
            }
            else
            {
                return DIR.Up;
            }
        }
        else if (Input.GetAxis("Vertical") < -0.2f)
        {
            if (Input.GetAxis("Horizontal") > 0.2f)
            {
                return DIR.DownRight;
            }
            else if (Input.GetAxis("Horizontal") < -0.2f)
            {
                return DIR.DownLeft;
            }
            else
            {
                return DIR.Down;
            }
        }
        else
        {
            if (Input.GetAxis("Horizontal") > 0.2f)
            {
                return DIR.Right;
            }
            else if (Input.GetAxis("Horizontal") < -0.2f)
            {
                return DIR.Left;
            }
            else
            {
                return DIR.Center;
            }
        }
    }

    //protected override bool IsOnGround()
    //{
    //    return Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, boxSizeGround, 0f, layerMask);
    //}

    bool IsOnWall()
    {
        onRightWall = Physics2D.OverlapBox((Vector2)transform.position + rightOffset, boxSizeWall, 0f, groundLayer);
        onLeftWall = Physics2D.OverlapBox((Vector2)transform.position + leftOffset, boxSizeWall, 0f, groundLayer);

        return onRightWall || onLeftWall;
    }

    void OnWallSlide(float slideSpeed)
    {
        if (state != STATE.WallJumping)
        {
            rb.velocity = new Vector2(0, -slideSpeed);
            airJumpCount = 0;
        }
    }

    void PlayerState()
    {
        if (IsOnGround())
        {
            airJumpCount = 0;
            state = STATE.Grounding;

        }
        else if (IsOnWall() &&
                ((onRightWall && Input.GetAxis("Horizontal") > 0.2f) ||
                (onLeftWall && Input.GetAxis("Horizontal") < -0.2f)))
        {
            if (Input.GetButtonDown("Jump"))
            {
                WallJump();
            }
            else if (state != STATE.WallJumping)
            {
                OnWallSlide(wallSlideSpeed);
                state = STATE.WallSliding;

                // Play touch wall audio
                if (landingAudio != null && !isWallSlideAudioPlayed)
                {
                    GameObject touchWallAudioObj = objectPool.GetFromPool("AudioSource");
                    touchWallAudioObj.transform.position = transform.position;
                    touchWallAudioObj.GetComponent<AudioPlayer>().SetAudioClip(landingAudio);
                    touchWallAudioObj.SetActive(true);
                    isWallSlideAudioPlayed = true;
                }
            }
        }
        else if (state != STATE.WallJumping)
        {
            state = STATE.InAir;
            isWallSlideAudioPlayed = false;
        }

        if (state == STATE.Grounding || state == STATE.WallSliding)
        {
            if (Input.GetAxis("Dash") <= 0.1f)
            {
                canDash = true;
            }
        }

    }

    public override int OnDamage(int damage)
    {
        GetCameraEffect().ShackCamera(5f, 0.1f);
        return base.OnDamage(damage);
    }

    public override void OnDeath()
    {
        GetCameraEffect().ShackCamera(5f, 0.1f);

        foreach (var obj in animationObjects)
        {
            obj.SetActive(false);
        }

        base.OnDeath();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.DrawWireCube(new Vector3(transform.position.x + rightOffset.x, transform.position.y + rightOffset.y, transform.position.z), boxSizeWall);
        Gizmos.DrawWireCube(new Vector3(transform.position.x + leftOffset.x, transform.position.y + leftOffset.y, transform.position.z), boxSizeWall);
    }
}
