using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Transform playerTransform;

    [Header("Enemy Info")]
    [SerializeField] int hp;
    [SerializeField] int damage;
    [SerializeField] ParticleSystem deathParticle;
    [SerializeField] bool isRanged;
    [SerializeField] bool isMelee;

    [Header("Ranged Attack")]
    [SerializeField] float shootingCD;
    [SerializeField] float bulletSpeed;
    [SerializeField] float rangedDistance = 100;
    bool canShoot;

    [Header("Enemy Component")]
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;

    bool canTakeDamage;
    SpriteRenderer spriteRenderer;
    Material material;

    public bool CanTakeDamage { get => canTakeDamage; set => canTakeDamage = value; }
    public int Hp { get => hp; set => hp = value; }

    private void Start()
    {
        canTakeDamage = true;
        canShoot = true;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerTransform = D.Get<Player>().transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;

        OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRanged && canShoot)
        {
            float dis = (transform.position - playerTransform.position).sqrMagnitude;
            if (dis < rangedDistance)
            {
                StartCoroutine(ShootCooldown());
            }
        }

        if (playerTransform != null)
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

        if(hp <= 0)
        {
            OnDeath();
        }
    }


    void Attack()
    {

    }

    void FacePlayer()
    {

    }

    IEnumerator ShootCooldown()
    {
        canShoot = false;
        GameObject bullet = ObjectPool.instance.GetFromPool("RunBullet");
        bullet.transform.position = transform.position;
        bullet.GetComponent<EnemyBullet>().Damage = damage;

        if (playerTransform != null)
        {
            bullet.GetComponent<EnemyBullet>().Velocity = ((playerTransform.position - transform.position).normalized * bulletSpeed);
            //bullet.transform.eulerAngles = new Vector3(0, 0, ShootAngle());
        }

        bullet.SetActive(true);
        yield return new WaitForSeconds(shootingCD);
        canShoot = true;
    }

    // Bullet rotation
    float ShootAngle()
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
        if(hp > 0)
        {
            hp -= damage;
            Debug.Log(material.name);
            StartCoroutine(HitFlash());
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
        D.Get<CameraEffect>().ShackCamera(5f, 0.1f);
        GameObject temp = Instantiate(deathParticle.gameObject);
        temp.transform.position = transform.position;
        Destroy(temp, 0.5f);
        animator.SetTrigger("TakeHit");
        Destroy(gameObject);
    }

    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnPostDeath() { }
}
