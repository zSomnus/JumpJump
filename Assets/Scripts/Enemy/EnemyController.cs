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
    [Header("Enemy Info")]
    [SerializeField] int damage;
    [SerializeField] bool isRanged;
    [SerializeField] bool isMelee;
    [SerializeField] Vector3 centerOffset;
    [SerializeField] float aggroRange = 8;
    [SerializeField] float attackRange = 3;

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
        canShoot = true;
        canSlash = true;
        player = D.Get<Player>();
        playerTransform = player.transform;
        rb = GetComponent<Rigidbody2D>();
        Init(GetComponent<Actor>(), D.Get<ObjectPool>());
        animator = owner.Animator;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
        }


        RangedAttack();
        MeleeAttack();

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
        if (isMelee && canSlash && dis < aggroRange)
        {
            if (dis > attackRange)
            {
                owner.isFacingMoveDirection = false;
                rb.velocity = new Vector2(owner.GetMoveSpeed() * transform.localScale.x, rb.velocity.y);
            }
            else
            {
                owner.isFacingMoveDirection = true;
                rb.velocity = Vector2.zero;
                StartCoroutine(Slash());
            }
        }
    }

    void RangedAttack()
    {
        if (isRanged && canShoot && dis < aggroRange)
        {
            if (dis > attackRange)
            {
                rb.velocity = ((playerTransform.position - transform.position).normalized * owner.GetMoveSpeed());
            }
            else
            {
                rb.velocity = Vector2.zero;
                StartCoroutine(Shoot());
            }
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

    private void OnDrawGizmosSelected()
    {
        // Attack range;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, attackRange);

        // Aggro range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, aggroRange);
    }
}
