using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : PoolableObject
{
    [SerializeField] float activeTime;
    [SerializeField] float activeStart;

    Rigidbody2D rb;

    Vector2 velocity;

    public Vector2 Velocity { get => velocity; set => velocity = value; }

    public override void Reset()
    {
        activeStart = 0;
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
        // Return to the Object Pool
        gameObject.SetActive(false);
    }
}
