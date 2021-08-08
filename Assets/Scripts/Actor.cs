using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Actor : MonoBehaviour
{
    [FormerlySerializedAs("spriteObject")] [SerializeField] protected GameObject spriteContainer;
    protected ObjectPool objectPool;
    protected Rigidbody2D rb;
    protected Collider2D baseCollider;

    [Header("Actor Info")]
    [SerializeField] int baseHp;
    int currentHp;
    [SerializeField] float moveSpeed;
    public bool isFacingMoveDirection = true;
    [SerializeField] protected Transform[] weaponTransforms;
    [SerializeField] protected SpriteRenderer mainRenderer;
    [SerializeField] protected Material mainMaterial;
    [SerializeField] private Animator animator;
    const float AnimatorSpeedWhenTimePaused = 0;
    [SerializeField] Vector2 centerOffset;
    [SerializeField] CameraEffect mainCameraEffect;
    [SerializeField] protected bool canTakeSpikeDamage = true;
    [SerializeField] AudioClip hitAudio;
    [SerializeField] AudioClip deathAudio;
    [SerializeField] bool isHeavy;
    protected Controller2D controller2D;

    bool isDying;
    public EnemyController Enemy { get; set; }
    public bool IsEnemy => Enemy != null;

    public float MoveSpeed { get => GetMoveSpeed(); set => moveSpeed = value; }
    float shieldBlockTime;
    public bool IsFacingRight { get; private set; } = true;

    public float ShieldBlockTime
    {
        get => shieldBlockTime;
        set
        {
            shieldBlockTime = Mathf.Max(shieldBlockTime, value);
        }
    }

    public bool CountAsLevelEnemy => Enemy != null && Enemy.CountAsLevelEnemy;

    public bool IsHeavy
    {
        get => isHeavy;
        set => isHeavy = value;
    }

    public bool IsAlive() => GetCurrentHp() > 0;

    public Animator Animator { get => animator; set => animator = value; }
    public Vector2 CenterOffset { get => centerOffset; set => centerOffset = value; }

    public event Action<Actor> OnPostDeath = delegate { };
    public static Action<Actor> RaiseActorDamagedEvent = delegate { };
    public Action<Actor, int, DamageType, MeleeProperty> OnPostDamage = delegate { };
    public bool IsInvincible { get; set; }
    private int timeScalePercent = 100;
    public int TimeScalePercent
    {
        get
        {
            return timeScalePercent;
        }
        set
        {
            timeScalePercent = value;
            UpdateAnimatorSpeed();
        }
    }

    public float MyDeltaTime
    {
        get
        {

            return (TimeScalePercent / 100f) * GameTime.deltaTime;
        }
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator != null)
        {
            animator.speed = GameTime.IsGamePaused ? AnimatorSpeedWhenTimePaused : (timeScalePercent / 100f) * GameTime.GetSlowValue();
            animator.enabled = timeScalePercent > 0;
        }
    }

    protected virtual void OnEnable()
    {
        currentHp = baseHp;
    }

    private void Awake()
    {
        OnAwake();
        Init();
    }

    protected virtual void Init()
    {
        objectPool = D.Get<ObjectPool>();
        mainCameraEffect = D.Get<CameraEffect>();
        rb = GetComponent<Rigidbody2D>();
        mainRenderer = GetComponent<SpriteRenderer>();
        mainMaterial = mainRenderer.material;
        animator = GetComponent<Animator>();
        currentHp = baseHp;
        baseCollider = GetComponent<Collider2D>();
        mainMaterial.SetFloat("_FlashAmount", 0);
    }

    private void Start()
    {
        OnStart();
    }

    protected virtual void Update()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        FlipSprite();
    }

    internal void EndJuggle()
    {
        spriteContainer.transform.localEulerAngles = Vector3.zero;
    }

    protected virtual void FlipSprite()
    {
        if (isFacingMoveDirection)
        {
            bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon + 0.5f;

            if (playerHasHorizontalSpeed)
            {
                transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
            }
        }
    }

    internal virtual bool CanMoveForwardGrounded(int direction)
    {
        return false;
    }

    public virtual void DropThroughPlatform() { }

    public virtual Vector2 GetFacingDirection()
    {
        return new Vector2(transform.localScale.x, 0);
    }

    protected virtual void UpdateHp()
    {
        currentHp = Mathf.Clamp(currentHp, 0, GetBaseHp());
    }

    public int GetCurrentHp()
    {
        return currentHp;
    }

    public int GetBaseHp()
    {
        return baseHp;
    }

    public float GetHpRatio()
    {
        float ratio = (float)currentHp / GetBaseHp();

        if (ratio > 0)
        {
            return ratio;
        }
        else
        {
            return 0;
        }
    }

    public virtual void OnHeal(int heal)
    {
        if (currentHp + heal <= baseHp)
        {
            currentHp += heal;
        }
        else
        {
            currentHp = baseHp;
        }

        UpdateHp();
    }

    protected virtual Vector2 GetInputMove()
    {
        return Vector2.zero;
    }

    public virtual int OnDamage(int damage)
    {
        // Prepare and play hit audio
        GameObject hitAudioObj = objectPool.GetFromPool("AudioSource");
        hitAudioObj.GetComponent<AudioPlayer>().SetAudioClip(hitAudio);
        hitAudioObj.SetActive(true);

        StartCoroutine(HitFlash());
        currentHp -= OnPreDamageApplication(damage);

        if (currentHp <= 0)
        {
            OnDeath();
        }

        return damage;
    }

    public virtual int OnDamage(int damage, DamageType damageType, Actor damager, MeleeProperty meleeProperty = MeleeProperty.None)
    {
        Debug.Log($"{gameObject.name} got hit for {damage} dmg.");
        bool isNotSuicide = (damageType == DamageType.Dot || damager != this);

        if (IsInvincible)
        {
            return 0;
        }
        else if (ShieldBlockTime > 0)
        {
            bool blockFacing = !IsFacingRight;
            Vector3 blockPos = GetWeaponTransform().position;
            if (damager != null)
            {
                blockFacing = transform.position.x > damager.transform.position.x;
                if (blockFacing == IsFacingRight)
                {
                    blockPos = transform.position;
                }
            }

            return 0;
        }

        RaiseActorDamagedEvent(this);
        if (isDying)
        {
            return damage;
        }

        currentHp -= OnPreDamageApplication(damage);
        OnPostDamageApplication();
        UpdateHp();

        if (currentHp <= 0)
        {
            isDying = true;
            OnDeath();
        }

        OnPostDamage(this, damage, damageType, meleeProperty);
        return damage;
    }

    public virtual bool IsGrounded()
    {
        return true;
    }

    protected virtual Controller2D CreateController2D()
    {
        return new Controller2D();
    }

    public virtual int OnHpCost(int cost)
    {
        currentHp -= OnPreDamageApplication(cost);

        if (currentHp <= 0)
        {
            OnDeath();
        }

        return cost;
    }

    protected virtual void OnPostDamageApplication() { }

    protected virtual int OnPreDamageApplication(int damage)
    {
        return damage;
    }

    public virtual float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public virtual void OnDeath()
    {
        // Prepare and play death audio
        GameObject deathAudioObj = objectPool.GetFromPool("AudioSource");
        deathAudioObj.GetComponent<AudioPlayer>().SetAudioClip(deathAudio);
        deathAudioObj.SetActive(true);

        OnPostDeath(this);
        GameObject temp = objectPool.GetFromPool("DeathParticle");
        temp.transform.position = transform.position;
        temp.SetActive(true);
        gameObject.SetActive(false);
    }

    public CameraEffect GetCameraEffect()
    {
        return mainCameraEffect;
    }

    IEnumerator HitFlash()
    {
        mainMaterial.SetFloat("_FlashAmount", 1);
        yield return new WaitForSeconds(0.1f);
        mainMaterial.SetFloat("_FlashAmount", 0);
    }

    public virtual void Juggle()
    {
        // Play juggle sound effect;
    }

    public Transform GetWeaponTransform()
    {
        return weaponTransforms.Length > 0 ? weaponTransforms[0] : null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (canTakeSpikeDamage && collision.gameObject.layer == LayerMask.NameToLayer("Spike"))
        {
            OnDamage(currentHp);
        }
    }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
}
