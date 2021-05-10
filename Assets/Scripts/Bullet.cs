using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PoolableObject
{
    Transform playerTransform;
    Rigidbody2D rb;
    Vector2 direction;

    [SerializeField] float scale = 1;
    [SerializeField] float activeTime;
    [SerializeField] float activeStart;
    [SerializeField] float speed;
    [SerializeField] int damage = 1;
    Vector2 startSpeed;

    public override void Reset()
    {
        activeStart = 0;
        startSpeed = Vector2.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector2.one * scale;
        direction = Vector3.zero;
    }

    private void Awake()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void OnEnable()
    {
        if (playerTransform != null)
        {
            if (Input.GetAxis("Vertical") > 0.5f)
            {
                direction.y = 1;
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (Input.GetAxis("Vertical") < -0.5f)
            {
                direction.y = -1;
                transform.rotation = Quaternion.Euler(0, 0, -90);
            }
            else
            {
                direction.x = playerTransform.localScale.x;
                transform.localScale = new Vector3(direction.x * scale, transform.localScale.y * scale, 1);
            }

            startSpeed = direction * speed;
            transform.position = playerTransform.position;

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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().Hp -= damage;
        }

        gameObject.SetActive(false);
    }
}
