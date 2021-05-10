using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Transform playerTransform;

    [Header("Enemy Info")]
    [SerializeField] int hp;
    [SerializeField] int damage;

    [Header("Ranged Attack")]
    [SerializeField] float shootingCD;
    [SerializeField] float bulletSpeed;
    [SerializeField] float rangedDistance = 100;
    bool canShoot;

    [Header("Enemy Component")]
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb;

    bool canTakeDamage;

    public bool CanTakeDamage { get => canTakeDamage; set => canTakeDamage = value; }
    public int Hp { get => hp; set => hp = value; }

    private void Start()
    {
        canTakeDamage = true;
        canShoot = true;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null && GameObject.FindGameObjectWithTag("Player"))
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (canShoot)
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

    protected virtual void OnPostDeath() { }

    protected virtual void OnDeath()
    {
        OnPostDeath();
        Destroy(gameObject);
    }

    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
}
