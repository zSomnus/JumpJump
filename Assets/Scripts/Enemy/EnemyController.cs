using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyRank
{
    Normal,
    Boss,
}

[Serializable]
public class EnemyAttackInfo
{
    public string animTriggerName;
    public int itemControllerIndex = -1;
    public Transform trackPoint;
    public float attackForwardPush = 0;
    public int landingItemControllerIndex = -1;
}

public class EnemyController : MonoBehaviour
{
    Transform playerTransform;
    Player player;
    Actor owner;
    Vector3 center;
    private ObjectPool objectPool;
    GameDirector gameDirector;

    [Header("Enemy Info")]
    public string enemyName;
    [SerializeField] int damage;
    [SerializeField] bool isRanged;
    [SerializeField] bool isMelee;
    [SerializeField] Vector3 centerOffset;
    [SerializeField] float aggroRange = 8;
    [SerializeField] float attackRange = 3;
    [SerializeField] float safeHeight;
    [SerializeField] EnemyRank enemyRank = EnemyRank.Normal;
    public bool IsBoss() => enemyRank == EnemyRank.Boss;
    public bool IsHeavy => owner.IsHeavy;
    public float attackForwardPush = 0;
    EnemyAttackInfo currentEnemyAttackInfo;
    [SerializeField] float AttackCooldownTime = 0;
    [SerializeField] float attackChargeTime = 0;
    const float DropThreshold = 1.0f;
    public bool CountAsLevelEnemy
    {
        get => CountAsLevelEnemy;
        set => CountAsLevelEnemy = value;
    }

    [Header("Attack Info")]
    [SerializeField] EnemyAttackInfo[] attackInfos = null;
    [SerializeField] float attackCD = 3;
    [SerializeField] float bulletSpeed;
    bool canShoot;
    bool canSlash;
    public bool IsAttacking { get; set; }
    public int ActiveItemControllerIndex { get; set; }

    [Header("Enemy Component")]
    [SerializeField] Collider2D[] collider2Ds = null;
    [SerializeField] Animator animator;
    HashSet<string> animatorParams = new HashSet<string>();
    public EnemyStateMachine StateMachine { get; set; }
    public float InitialAggroTimer { get; set; }
    public EnemyRank EnemyRank { get => enemyRank; set => enemyRank = value; }

    float dis;
    private Rigidbody2D rb;

    public event Action<int, DamageType> RaiseDamageEvent = delegate { };
    public event Action RaiseAttackEvent = delegate { };
    public event Action RaisePreAttackEvent = delegate { };
    public event Action RaiseAggroEvent = delegate { };
    public event Action RaisePostAttackEvent = delegate { };
    public event Action<string> RaiseCustomAnimEvent = delegate { };

    private void OnValidate()
    {
#if UNITY_EDITOR
        center = transform.position + centerOffset;
#endif
    }

    private void Awake()
    {
        Init(GetComponent<Actor>(), D.Get<ObjectPool>());

        canShoot = true;
        canSlash = true;
        rb = GetComponent<Rigidbody2D>();

        player = D.Get<Player>();
        StateMachine = new EnemyStateMachine(this, player);

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

    public bool ContainsAnimParam(string paramName)
    {
        return animatorParams.Contains(paramName);
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

    public void FacePlayer()
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

    public Vector2 GetInputMove()
    {
        if (StateMachine == null)
        {
            return Vector2.zero;
        }
        return StateMachine.GetInputMove();
    }

    public void DropIfPlayerUnder()
    {
        if (GetAttackTarget().transform.position.y < transform.position.y - DropThreshold)
        {
            owner.DropThroughPlatform();
        }
    }

    internal bool CanMoveForwardGrounded(int direction)
    {
        return owner.CanMoveForwardGrounded(direction);
    }

    internal bool HasMultipleAttacks()
    {
        return attackInfos != null && attackInfos.Length > 1;
    }

    internal void DoAttack(EnemyAttackInfo enemyAttackInfo)
    {
        attackForwardPush = enemyAttackInfo.attackForwardPush;
        currentEnemyAttackInfo = enemyAttackInfo;
        DoAttack(enemyAttackInfo.animTriggerName);
    }

    public void DoAttack(string triggerName = "attack")
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    public bool CanAttackNow()
    {
        return Vector3.Distance(transform.position, GetAttackTarget().transform.position) <= attackRange;
    }

    internal void OnAggro()
    {
        RaiseAggroEvent?.Invoke();
    }

    public bool IsAggroed()
    {
        return StateMachine.IsIn<AggroEnemyState>() || StateMachine.IsIn<AttackEnemyState>();
    }

    public void SetColliderActive(bool isActive)
    {
        if (collider2Ds != null)
        {
            foreach (var collider2D in collider2Ds)
            {
                collider2D.enabled = isActive;
            }
        }
        owner.IsInvincible = !isActive;
    }

    public void OnPreAttack()
    {
        IsAttacking = true;
        RaisePreAttackEvent();
        // Play pre attack sound
    }

    public void OnAttack(int index = -1)
    {
        IsAttacking = true;
        RaiseAttackEvent();
    }

    public void OnPostAttack()
    {
        IsAttacking = false;
        RaisePostAttackEvent();
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

    internal void Juggle()
    {
        if (IsHeavy)
        {
            return;
        }
        owner.Juggle();
    }

    internal void TriggerAnim(string name)
    {
        animator.SetTrigger(name);
    }

    public void SetAnimaBool(string name, bool value)
    {
        animator.SetBool(name, value);
    }

    public float GetAttackCooldown()
    {
        float cooldown = AttackCooldownTime;
        return cooldown;
    }

    public float GetAttackChargeTime()
    {
        return attackChargeTime;
    }

    internal EnemyAttackInfo[] GetAttackInfos()
    {
        return attackInfos;
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

    public Actor GetAttackTarget()
    {
        return player;
    }
}
