using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] int damage;
    Collider2D collider;
    float damageCD = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Attack(Collider2D collision)
    {
        collision.GetComponent<Enemy>().Hp -= damage;
        yield return new WaitForSeconds(damageCD);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            StartCoroutine(Attack(collision));
        }
    }
}
