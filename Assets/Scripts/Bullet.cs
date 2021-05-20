using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PoolableObject
{
    Transform playerTransform;
    Rigidbody2D rb;
    Vector2 direction;
    Player player;

    [SerializeField] float scale = 1;
    [SerializeField] float activeTime;
    [SerializeField] float activeStart;
    [SerializeField] float speed;
    [SerializeField] int damage = 1;
    [SerializeField] Vector2 startSpeed;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Vector3 size;

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
        player = D.Get<Player>();
        playerTransform = player.gameObject.transform;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (playerTransform != null)
        {
            if (player.Direction.y > 0.5f)
            {
                direction.y = 1;
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (player.Direction.y < -0.5f)
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

        Collider2D collision = Physics2D.OverlapBox(transform.position, size, 0f, layerMask);
        // Return to the Object Pool
        if (collision != null)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                collision.gameObject.GetComponent<Actor>().OnDamage(damage);
            }

            gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position;
        Gizmos.DrawWireCube(center, size);
    }
}
