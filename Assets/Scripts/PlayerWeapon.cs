using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    ObjectPool objectPool;
    [SerializeField] int damage;
    float damageCD = 0.2f;
    [SerializeField] AudioClip whipAudio;

    // Start is called before the first frame update
    void Awake()
    {
        objectPool = D.Get<ObjectPool>();
    }

    private void OnEnable()
    {
        PlayWhipShound();
    }

    IEnumerator Attack(Collider2D collision)
    {
        collision.gameObject.GetComponent<Actor>().OnDamage(damage);
        yield return new WaitForSeconds(damageCD);
    }

    private void PlayWhipShound()
    {
        if (whipAudio != null && objectPool != null)
        {
            GameObject whipAudioObj = objectPool.GetFromPool("AudioSource");
            whipAudioObj.transform.position = transform.position;
            whipAudioObj.GetComponent<AudioPlayer>().SetAudioClip(whipAudio, 0.2f, 0.2f);
            whipAudioObj.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            StartCoroutine(Attack(collision));
        }
    }
}
