using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rb;

    [Header("Move")]
    [SerializeField] float runSpeed;
    DIR movingDirection;

    [Header("Jump")]
    [SerializeField] float jumpSpeed;
    [SerializeField] float minJumpSpeed;
    [SerializeField] float maxFallSpeed;
    [SerializeField] float fallMultiplier;
    [SerializeField] float lowJumpMultiplier;
    [SerializeField] int maxAirJumpCount;
    int airJumpCount;
    [SerializeField] Animator wingAnimator;

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
    [SerializeField] private Vector2 boxSizeGround;
    [SerializeField] LayerMask layerMask;

    [Header("Melee Attack")]
    [SerializeField] Animator weaponAnimator;

    [Header("Ranged Attack")]
    [SerializeField] float rangedCD;
    bool canShoot;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashCD += dashDuration;
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerState();
        movingDirection = GetMovingDirection();
        Dash();

        if (!isDashing)
        {
            SimulatePhysics();
            Run();
        }

        Jump();
        Shoot();
        DoubleJump();
        Attack();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            float runInput = Input.GetAxis("Horizontal");
            if (runInput > 0)
            {
                runInput = runSpeed;
            }
            else if (runInput < 0)
            {
                runInput = -runSpeed;
            }
            else
            {
                runInput = 0f;
            }
            rb.velocity = new Vector2(runInput, rb.velocity.y);
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

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon + 0.5f;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }
    }

    void Run()
    {
        float runInput = Input.GetAxis("Horizontal");
        if (runInput > 0)
        {
            runInput = runSpeed;
        }
        else if (runInput < 0)
        {
            runInput = -runSpeed;
        }
        else
        {
            runInput = 0f;
        }
        rb.velocity = new Vector2(runInput, rb.velocity.y);
    }

    void Jump()
    {
        if (IsOnGround() && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(0, jumpSpeed);
        }
    }

    void DoubleJump()
    {
        if (!IsOnGround() && airJumpCount < maxAirJumpCount && Input.GetButtonDown("Jump"))
        {
            wingAnimator.SetTrigger("DoubleJump");
            rb.velocity = new Vector2(0, jumpSpeed);
            airJumpCount++;
        }
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
        if (!isDashing && !isDashCD && canDash && (Input.GetButtonDown("Dash") || Input.GetAxisRaw("Dash") > 0.2f))
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
            GameObject bullet = ObjectPool.instance.GetFromPool("Bullet");
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
        ObjectPool.instance.GetFromPool("PlayerShadow").SetActive(true);
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
        rb.gravityScale = 0;

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

    void PlayerState()
    {
        if (IsOnGround())
        {
            airJumpCount = 0;

            if (Input.GetAxis("Dash") <= 0.1f)
            {
                canDash = true;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position;
        Gizmos.DrawWireCube(new Vector3(center.x + bottomOffset.x, center.y + bottomOffset.y, center.z), boxSizeGround);
    }
}
