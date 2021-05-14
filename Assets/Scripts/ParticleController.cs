using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : PoolableObject
{
    [SerializeField] float lifetime;
    ParticleSystem particle;

    private void OnEnable()
    {
        particle = GetComponent<ParticleSystem>();

        if (lifetime == 0)
        {
            lifetime = particle.main.startLifetime.constantMax;
        }

        StartCoroutine(LifeTimer());
    }

    IEnumerator LifeTimer()
    {
        particle.Play();
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }

    public override void Reset()
    {
        
    }
}
