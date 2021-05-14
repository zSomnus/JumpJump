using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : PoolableObject
{
    [SerializeField] float activeTime;
    [SerializeField] float activeStart;
    int damage;

    Rigidbody2D rb;

    Vector2 velocity;

    public Vector2 Velocity { get => velocity; set => velocity = value; }
    public int Damage { get => damage; set => damage = value; }

    public override void Reset()
    {
        activeStart = 0;
        damage = 0;
    }

    private void OnEnable()
    {
        activeStart = Time.time;
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb.velocity = velocity;

        if (Time.time >= activeStart + activeTime)
        {
            // Return to the Object Pool
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        gameObject.SetActive(false);

        // Return to the Object Pool
        if (collision.gameObject.tag == "Player")
        {
            D.Get<Player>().OnDamage(damage);
        }

    }
}
