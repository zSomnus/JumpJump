using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PoolableObject
{
    Transform playerTransform;
    Rigidbody2D rb;
    Vector2 direction;

    [SerializeField] float activeTime;
    [SerializeField] float activeStart;
    [SerializeField] Vector2 speed;
    [SerializeField] int damage = 1;
    Vector2 startSpeed;

    public override void Reset()
    {
        activeStart = 0;
        startSpeed = Vector2.zero;
    }

    private void Awake()
    {
        if(GameObject.FindGameObjectWithTag("Player"))
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void OnEnable()
    {
        if (playerTransform != null)
        {
            direction = playerTransform.localScale;
            startSpeed = new Vector2(speed.x * direction.x, speed.y);

            transform.localScale = direction;
            transform.position = playerTransform.position;
            transform.rotation = playerTransform.rotation;

            activeStart = Time.time;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = startSpeed;

        if (Time.time >= activeStart + activeTime)
        {
            // Return to the Object Pool
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Return to the Object Pool
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().Hp -= damage;
        }

        gameObject.SetActive(false);
    }
}
