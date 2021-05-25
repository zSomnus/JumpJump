using System.Collections;
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
    Vector2 direction;

    [Header("Jump")]
    [SerializeField] int maxAirJumpCount;
    int airJumpCount;
    [SerializeField] Animator wingAnimator;
    [SerializeField] Vector2 wallJumpFource;
    [SerializeField] ParticleSystem landingParticle;
    [SerializeField] AudioClip doubleJumpAudio;
    bool isLandingParticlePlayed;
    bool isWallSlideAudioPlayed;
    bool isJumpPressed;

    [Header("Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCD;
    [SerializeField] float shadowCD;
    [SerializeField] AudioClip dashAudio;
    bool isDashing;
    bool isDashCD;
    bool isShadowStart;
    bool isDashPressed;
    bool canDash;

    [Header("Collision")]
    [SerializeField] private Vector2 rightOffset;
    [SerializeField] private Vector2 leftOffset;
    [SerializeField] private Vector2 boxSizeWall;

    [Header("Melee Attack")]
    [SerializeField] Animator weaponAnimator;

    [Header("Ranged Attack")]
    [SerializeField] float rangedCD;
    bool canShoot;

    public Vector2 Direction { get => direction; set => direction = value; }
    public bool IsJumpPressed { get => isJumpPressed; set => isJumpPressed = value; }
    public bool IsDashPressed { get => isDashPressed; set => isDashPressed = value; }

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
        isShadowStart = false;
        direction = Vector2.zero;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        PlayerState();
        movingDirection = GetMovingDirection();

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
            bool playerHasHorizontalSpeed = direction.x != 0;

            if (playerHasHorizontalSpeed)
            {
                transform.localScale = new Vector2(Mathf.Sign(direction.x), 1f);
            }
        }
        else
        {
            base.FlipSprite();
        }
    }

    public void Dash()
    {
        bool dashState = !isDashPressed && state != STATE.WallSliding && !isDashing && !isDashCD && canDash;

        if (dashState)
        {
            canDash = false;
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

    IEnumerator Dash(Vector2 velocity, float duration)
    {
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

    IEnumerator DashCooldown(float cd)
    {
        isDashCD = true;
        yield return new WaitForSeconds(cd);
        isDashCD = false;
    }

    public void Run()
    {
        float runInput = direction.x;

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
        canDash = true;
    }

    public void RangedAttack()
    {
        if (canShoot)
        {
            StartCoroutine(ShootCooldownCount());
            OnHpCost(10);
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

    protected override void SimulatePhysics()
    {
        if (!isDashing && (state != STATE.WallJumping) && (state != STATE.Grounding))
        {
            // simulate physics
            if (rb.velocity.y < minJumpSpeed && rb.velocity.y > -maxFallSpeed)
            {

                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;

            }
            else if (rb.velocity.y > 0 && !isJumpPressed)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }

        if (isDashing && IsOnWall())
        {
            rb.velocity = Vector2.zero;
        }
    }

    private DIR GetMovingDirection()
    {
        if (direction.y > 0.5f)
        {
            if (direction.x > 0.5f)
            {
                return DIR.UpRight;
            }
            else if (direction.x < -0.5f)
            {
                return DIR.UpLeft;
            }
            else
            {
                return DIR.Up;
            }
        }
        else if (direction.y < -0.5f)
        {
            if (direction.x > 0.5f)
            {
                return DIR.DownRight;
            }
            else if (direction.x < -0.5f)
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
            if (direction.x > 0.5f)
            {
                return DIR.Right;
            }
            else if (direction.x < -0.5f)
            {
                return DIR.Left;
            }
            else
            {
                return DIR.Center;
            }
        }
    }

    bool IsOnWall()
    {
        onRightWall = Physics2D.OverlapBox((Vector2)transform.position + rightOffset, boxSizeWall, 0f, groundLayer);
        onLeftWall = Physics2D.OverlapBox((Vector2)transform.position + leftOffset, boxSizeWall, 0f, groundLayer);

        return onRightWall || onLeftWall;
    }

    void OnWallSlide(float slideSpeed)
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(0, -slideSpeed);
            airJumpCount = 0;
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

    public void MeleeAttack()
    {
        weaponAnimator.SetTrigger("Attack");
    }

    public void Jump()
    {
        if (state == STATE.Grounding)
        {
            rb.velocity = new Vector2(0, jumpSpeed);
        }
    }

    public void DoubleJump()
    {
        if (!IsOnWall() && state == STATE.InAir && airJumpCount < maxAirJumpCount)
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

    public void WallJump()
    {
        if (!isJumpPressed && !IsOnGround() && IsOnWall())
        {
            StartCoroutine(WallJumpTimer());
        }
    }
    void PlayerState()
    {
        if (IsOnGround())
        {
            airJumpCount = 0;
            state = STATE.Grounding;

            if (!isDashPressed)
            {
                canDash = true;
            }
        }
        else
        {
            if (state != STATE.WallJumping)
            {
                state = STATE.InAir;
            }

            if (IsOnWall())
            {
                canDash = false;
                OnWallSlide(wallSlideSpeed);
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.DrawWireCube(new Vector3(transform.position.x + rightOffset.x, transform.position.y + rightOffset.y, transform.position.z), boxSizeWall);
        Gizmos.DrawWireCube(new Vector3(transform.position.x + leftOffset.x, transform.position.y + leftOffset.y, transform.position.z), boxSizeWall);
    }
}
