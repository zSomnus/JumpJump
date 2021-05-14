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
    [Header("Move")]
    [SerializeField] float wallSlideSpeed;
    bool onRightWall;
    bool onLeftWall;
    DIR movingDirection;
    STATE state;

    [Header("Jump")]
    [SerializeField] float jumpSpeed;
    [SerializeField] float minJumpSpeed;
    [SerializeField] float maxFallSpeed;
    [SerializeField] float fallMultiplier;
    [SerializeField] float lowJumpMultiplier;
    [SerializeField] int maxAirJumpCount;
    int airJumpCount;
    [SerializeField] Animator wingAnimator;
    [SerializeField] Vector2 wallJumpFource;

    [Header("Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCD;
    [SerializeField] float shadowCD;
    bool isDashing;
    bool canDash;
    bool isDashCD;

    [Header("Collision")]
    [SerializeField] private Vector2 bottomOffset;
    [SerializeField] private Vector2 rightOffset;
    [SerializeField] private Vector2 leftOffset;
    [SerializeField] private Vector2 boxSizeGround;
    [SerializeField] private Vector2 boxSizeWall;
    [SerializeField] LayerMask layerMask;

    [Header("Melee Attack")]
    [SerializeField] Animator weaponAnimator;

    [Header("Ranged Attack")]
    [SerializeField] float rangedCD;
    bool canShoot;

    protected override void OnStart()
    {
        base.OnStart();
        dashCD += dashDuration;
        canShoot = true;
        state = STATE.None;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
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
        }
        else
        {
            rb.gravityScale = 1;
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing && state != STATE.WallJumping)
        {
            Run();

            if (state != STATE.Grounding)
            {
                SimulatePhysics();
            }
        }

        ShowPlayerShadow();
    }

    void ShowPlayerShadow()
    {
        if (isDashing)
        {
            StartCoroutine(GetShadow());
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
                    //rb.velocity = Vector2.up * dashSpeed;
                    velocity = Vector2.up * dashSpeed;
                    //Dash(Vector2.up * dashSpeed, dashDuration);
                    break;
                case DIR.Down:
                    //rb.velocity = Vector2.down * dashSpeed;
                    velocity = Vector2.down * dashSpeed;
                    //Dash(Vector2.down * dashSpeed * 0.5f, dashDuration);
                    break;
                case DIR.Left:
                    //rb.velocity = Vector2.left * dashSpeed * 2f;
                    velocity = Vector2.left * dashSpeed;
                    //Dash(Vector2.left * dashSpeed, dashDuration);
                    break;
                case DIR.Right:
                    //rb.velocity = Vector2.right * dashSpeed * 2f;
                    velocity = Vector2.right * dashSpeed;
                    //Dash(Vector2.right * dashSpeed, dashDuration);
                    break;
                case DIR.UpRight:
                    //rb.velocity = Vector2.up * dashSpeed / 0.2f + Vector2.right * dashSpeed / 0.2f;
                    velocity = Vector2.up * dashSpeed * 0.7f + Vector2.right * dashSpeed * 0.7f;
                    //Dash(Vector2.up * dashSpeed * 0.5f + Vector2.right * dashSpeed * 0.5f, dashDuration);
                    break;
                case DIR.DownRight:
                    //rb.velocity = Vector2.down * dashSpeed / 0.2f + Vector2.right * dashSpeed / 0.2f;
                    velocity = Vector2.down * dashSpeed * 0.7f + Vector2.right * dashSpeed * 0.7f;
                    //Dash(Vector2.down * dashSpeed * 0.5f + Vector2.right * dashSpeed * 0.5f, dashDuration);
                    break;
                case DIR.UpLeft:
                    //rb.velocity = Vector2.up * dashSpeed / 0.2f + Vector2.left * dashSpeed / 0.2f;
                    velocity = Vector2.up * dashSpeed * 0.7f + Vector2.left * dashSpeed * 0.7f;
                    //Dash(Vector2.up * dashSpeed * 0.5f + Vector2.left * dashSpeed * 0.5f, dashDuration);
                    break;
                case DIR.DownLeft:
                    //rb.velocity = Vector2.down * dashSpeed / 0.2f + Vector2.left * dashSpeed / 0.2f;
                    velocity = Vector2.down * dashSpeed * 0.7f + Vector2.left * dashSpeed * 0.7f;
                    //Dash(Vector2.down * dashSpeed * 0.5f + Vector2.left * dashSpeed * 0.5f, dashDuration);
                    break;
                default:
                    velocity = new Vector2(transform.localScale.x * dashSpeed, 0f);
                    //Dash(new Vector2(transform.localScale.x * dashSpeed, 0f), dashDuration);
                    break;
            }

            //Dash(velocity, dashDuration);
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
        yield return new WaitForSeconds(shadowCD);
        objectPool.GetFromPool("PlayerShadow").SetActive(true);
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

        yield return new WaitForSeconds(duration);

        isDashing = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1;
    }

    void SimulatePhysics()
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

    bool IsOnGround()
    {
        return Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, boxSizeGround, 0f, layerMask);
    }

    bool IsOnWall()
    {
        onRightWall = Physics2D.OverlapBox((Vector2)transform.position + rightOffset, boxSizeWall, 0f, layerMask);
        onLeftWall = Physics2D.OverlapBox((Vector2)transform.position + leftOffset, boxSizeWall, 0f, layerMask);

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
            }
        }
        else if (state != STATE.WallJumping)
        {
            state = STATE.InAir;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position;
        Gizmos.DrawWireCube(new Vector3(center.x + bottomOffset.x, center.y + bottomOffset.y, center.z), boxSizeGround);
        Gizmos.DrawWireCube(new Vector3(center.x + rightOffset.x, center.y + rightOffset.y, center.z), boxSizeWall);
        Gizmos.DrawWireCube(new Vector3(center.x + leftOffset.x, center.y + leftOffset.y, center.z), boxSizeWall);
    }
}
