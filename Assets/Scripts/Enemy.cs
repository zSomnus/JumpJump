using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Transform playerTransform;

    [Header("Ranged Attack")]
    [SerializeField] float shootingCD;
    [SerializeField] float bulletSpeed;
    bool canShoot;

    private void Start()
    {
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (canShoot)
        {
            StartCoroutine(ShootCooldown());
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
    }

    IEnumerator ShootCooldown()
    {
        canShoot = false;
        GameObject bullet = ObjectPool.instance.GetFromPool("RunBullet");
        bullet.transform.position = transform.position;

        if(playerTransform != null)
        {
            bullet.GetComponent<EnemyBullet>().Velocity = ((playerTransform.position - transform.position).normalized * bulletSpeed);
            //bullet.transform.eulerAngles = new Vector3(0, 0, ShootAngle());
        }

        bullet.SetActive(true);
        yield return new WaitForSeconds(shootingCD);
        canShoot = true;
    }

    float ShootAngle()
    {
        float fireAngle = Vector2.Angle(playerTransform.position - transform.position, Vector2.up);

        if (playerTransform.position.x > this.transform.position.x)
        {
            fireAngle = -fireAngle;
        }

        return fireAngle;
    }
}
