using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Transform playerTransform;
    Player player;
    Actor owner;
    Vector3 center;
    private ObjectPool objectPool;
    GameDirector gameDirector;

    [Header("Enemy Info")]
    [SerializeField] int damage;
    [SerializeField] bool isRanged;
    [SerializeField] bool isMelee;
    [SerializeField] Vector3 centerOffset;
    [SerializeField] float aggroRange = 8;
    [SerializeField] float attackRange = 3;
    [SerializeField] float safeHeight;

    [Header("Attack Info")]
    [SerializeField] float attackCD = 3;
    [SerializeField] float bulletSpeed;
    bool canShoot;
    bool canSlash;

    [Header("Enemy Component")]
    [SerializeField] Animator animator;

    float dis;
    private Rigidbody2D rb;

    private void OnValidate()
    {
        center = transform.position + centerOffset;
    }

    private void Awake()
    {
        Init(GetComponent<Actor>(), D.Get<ObjectPool>());

        canShoot = true;
        canSlash = true;
        rb = GetComponent<Rigidbody2D>();

        player = D.Get<Player>();

        if (player != null)
        {
            playerTransform = player.transform;
        }

        gameDirector = D.Get<GameDirector>();

        if (gameDirector != null)
        {
            gameDirector.EnemyCountIncrease();
            owner.OnPostDeath += gameDirector.EnemyCountDecrease;
        }

        animator = owner.Animator;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
        }

        if (isMelee)
        {
            MeleeAttack();
        }

        if (isRanged)
        {
            RangedAttack();
        }

        if (canSlash && owner.isFacingMoveDirection == false)
        {
            FacePlayer();
        }
    }

    private void FixedUpdate()
    {
        center = transform.position + centerOffset;

        if (player != null)
        {
            dis = Vector2.Distance(center, playerTransform.position);
        }
    }

    public void ApplyMeleeDamage()
    {
        RaycastHit2D hit = Physics2D.Raycast(center, owner.GetFacingDirection(), attackRange, LayerMask.GetMask("Default"));

        if (hit.collider != null && hit.collider.tag == "Player")
        {
            player.OnDamage(damage);
        }
    }

    public void Init(Actor owner, ObjectPool objectPool)
    {
        this.owner = owner;
        this.objectPool = objectPool;
    }

    void MeleeAttack()
    {
        if (canSlash && dis < aggroRange)
        {
            if (dis > attackRange)
            {
                owner.isFacingMoveDirection = false;
                rb.velocity = new Vector2(owner.GetMoveSpeed() * transform.localScale.x, rb.velocity.y);
            }
            else
            {
                owner.isFacingMoveDirection = true;
                rb.velocity = new Vector2(0, rb.velocity.y);
                StartCoroutine(Slash());
            }
        }
        else if (rb.velocity.x != 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void RangedAttack()
    {
        if (canShoot && dis < aggroRange)
        {
            if (dis > attackRange && IsHeightSafe())
            {
                rb.velocity = ((playerTransform.position - transform.position).normalized * owner.GetMoveSpeed());
            }
            else if (dis > attackRange)
            {
                rb.velocity = (new Vector3(playerTransform.position.x - transform.position.x, 0).normalized * owner.GetMoveSpeed());
            }
            else if (dis <= attackRange)
            {
                rb.velocity = Vector2.zero;
                StartCoroutine(Shoot());
            }
        }
        else if (rb.velocity != Vector2.zero)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void FacePlayer()
    {
        if (playerTransform != null && canSlash == true)
        {
            if (playerTransform.position.x - transform.position.x > 0)
            {
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    IEnumerator Shoot()
    {
        canShoot = false;
        GameObject bullet = objectPool.GetFromPool("RunBullet");
        bullet.transform.position = transform.position;
        bullet.GetComponent<EnemyBullet>().Damage = damage;

        bullet.GetComponent<EnemyBullet>().Velocity = ((playerTransform.position - transform.position).normalized * bulletSpeed);
        bullet.SetActive(true);

        yield return new WaitForSeconds(attackCD);
        canShoot = true;
    }

    IEnumerator Slash()
    {
        FacePlayer();
        canSlash = false;
        animator.SetTrigger("Attack1");

        yield return new WaitForSeconds(attackCD);
        canSlash = true;
    }

    // Bullet rotation
    float AngleToTarget()
    {
        float fireAngle = Vector2.Angle(playerTransform.position - transform.position, Vector2.up);

        if (playerTransform.position.x > this.transform.position.x)
        {
            fireAngle = -fireAngle;
        }

        return fireAngle;
    }

    bool IsHeightSafe()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, safeHeight, LayerMask.GetMask("Foreground"));

        if (hit.collider != null)
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        // Attack range;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + centerOffset, attackRange);

        // Aggro range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + centerOffset, aggroRange);

        // Save height ray
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * safeHeight);
    }
}
