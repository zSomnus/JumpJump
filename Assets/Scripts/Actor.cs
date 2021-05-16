using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    protected ObjectPool objectPool;
    protected Rigidbody2D rb;
    protected Collider2D baseCollider;

    [Header("Actor Info")]
    [SerializeField] int baseHp;
    int currentHp;
    [SerializeField] float moveSpeed;
    public bool isFacingMoveDirection = true;
    [SerializeField] protected SpriteRenderer mainRenderer;
    [SerializeField] protected Material mainMaterial;
    [SerializeField] private Animator animator;
    [SerializeField] Vector2 centerOffset;
    [SerializeField] CameraEffect mainCameraEffect;
    [SerializeField] protected bool canTakeSpikeDamage = true;

    public Animator Animator { get => animator; set => animator = value; }
    public Vector2 CenterOffset { get => centerOffset; set => centerOffset = value; }

    public event Action<Actor> OnPostDeath = delegate { };

    protected virtual void OnEnable()
    {
        rb.gravityScale = 1;
        currentHp = baseHp;
    }

    private void Awake()
    {
        Init();
        OnAwake();
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
    }

    private void Start()
    {
        OnStart();
    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {
        FlipSprite();
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
        currentHp += heal;
        UpdateHp();
    }

    public virtual int OnDamage(int damage)
    {

        currentHp -= OnPreDamageApplication(damage);

        if (currentHp <= 0)
        {
            OnDeath();
        }

        return damage;
    }

    protected virtual int OnPreDamageApplication(int damage)
    {
        return damage;
    }

    public virtual float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public bool IsAlive() => GetCurrentHp() > 0;

    public virtual void OnDeath()
    {
        OnPostDeath(this);
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
