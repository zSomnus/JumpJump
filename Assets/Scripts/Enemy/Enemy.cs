using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Transform playerTransform;
    Player player;

    [Header("Enemy Info")]
    [SerializeField] int hp;
    [SerializeField] int damage;
    [SerializeField] ParticleSystem deathParticle;
    [SerializeField] bool isRanged;
    [SerializeField] bool isMelee;
    [SerializeField] float moveSpeed;
    [SerializeField] float aggroRange = 8;
    [SerializeField] float attackRange = 3;
    [SerializeField] Vector2 centerOffset;

    [Header("Attack Info")]
    [SerializeField] float attackCD = 3;
    [SerializeField] float bulletSpeed;
    bool canShoot;
    bool canSlash;

    [Header("Enemy Component")]
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;

    bool canTakeDamage;
    SpriteRenderer spriteRenderer;
    Material material;
    float dis;

    Vector2 center;
    Vector2 facingDirection;

    public bool CanTakeDamage { get => canTakeDamage; set => canTakeDamage = value; }
    public int Hp { get => hp; set => hp = value; }

    private void Start()
    {
        canTakeDamage = true;
        canShoot = true;
        canSlash = true;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = D.Get<Player>();
        playerTransform = player.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;


        OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        dis = Vector2.Distance(transform.position, playerTransform.position);

        RangedAttack();

        MeleeAttack();

        animator.SetFloat("H", Mathf.Abs(rb.velocity.x));

        if (canSlash)
        {
            FacePlayer();
        }

        if (hp <= 0)
        {
            OnDeath();
        }
    }

    public void ApplyMeleeDamage()
    {
        center = new Vector2(transform.position.x + centerOffset.x, transform.position.y + centerOffset.y);
        facingDirection = new Vector2(transform.localScale.x, 0);

        RaycastHit2D hit = Physics2D.Raycast(center, facingDirection, attackRange, LayerMask.GetMask("Default"));

        if (hit.collider != null && hit.collider.tag == "Player")
        {
            player.TakeDamage(damage);
        }
    }


    void MeleeAttack()
    {
        if (isMelee && canSlash && dis < aggroRange)
        {
            if (dis > attackRange)
            {
                rb.velocity = new Vector2(moveSpeed * transform.localScale.x, rb.velocity.y);
            }
            else
            {
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
                rb.velocity = ((playerTransform.position - transform.position).normalized * moveSpeed);
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
        GameObject bullet = ObjectPool.instance.GetFromPool("RunBullet");
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

    public void TakeDamage(int damage)
    {
        if (hp > 0)
        {
            hp -= damage;
            animator.SetTrigger("TakeHit");

            // Material flash to white color
            //StartCoroutine(HitFlash());
        }
    }

    IEnumerator HitFlash()
    {
        material.SetFloat("_FlashAmount", 1);
        yield return new WaitForSeconds(0.1f);
        material.SetFloat("_FlashAmount", 0);
    }

    protected virtual void OnDeath()
    {
        OnPostDeath();
        D.Get<CameraEffect>().ShackCamera(6f, 0.1f);
        GameObject temp = Instantiate(deathParticle.gameObject);
        temp.transform.position = transform.position;
        Destroy(temp, 0.5f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Attack range;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Aggro range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Melee Attack ray
        //Gizmos.color = Color.cyan;
        //center = new Vector3(transform.position.x + centerOffset.x, transform.position.y + centerOffset.y, 0);
        //facingDirection = new Vector3(transform.localScale.x, 0, 0);
        //Gizmos.DrawRay(center, facingDirection * attackRange);
    }

    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnPostDeath() { }
}
