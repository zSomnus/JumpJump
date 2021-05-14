using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    protected ObjectPool objectPool;
    protected Rigidbody2D rb;

    [Header("Actor Info")]
    [SerializeField] int baseHp;
    int currentHp;
    [SerializeField] float moveSpeed;
    [SerializeField] protected SpriteRenderer mainRenderer;
    [SerializeField] protected Material mainMaterial;
    [SerializeField] private Animator animator;
    [SerializeField] Vector2 centerOffset;
    Vector2 center;
    [SerializeField] CameraEffect mainCameraEffect;

    public Vector2 Center { get => center; set => center = value; }
    public Animator Animator { get => animator; set => animator = value; }

    public event Action<Actor> OnPostDeath = delegate { };

    protected virtual void OnValidate()
    {
        center = new Vector2(transform.position.x + centerOffset.x, transform.position.y + centerOffset.y);
    }

    private void Awake()
    {
        objectPool = D.Get<ObjectPool>();
        mainCameraEffect = D.Get<CameraEffect>();
        rb = GetComponent<Rigidbody2D>();
        mainRenderer = GetComponent<SpriteRenderer>();
        mainMaterial = mainRenderer.material;
        animator = GetComponent<Animator>();
        currentHp = baseHp;

        OnAwake();
    }

    private void Start()
    {
        OnStart();
    }

    protected virtual void Update()
    {
        FlipSprite();
    }

    protected virtual void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon + 0.5f;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
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
        return (float)currentHp / GetBaseHp();
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
        Destroy(gameObject);
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

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(center, 0.1f);
    }
}
