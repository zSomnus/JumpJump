using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShadow : PoolableObject
{
    Transform playerTransform;
    SpriteRenderer thisSprite;
    SpriteRenderer playerSprite;

    Color color;

    [Header("Sadow Setting")]
    [SerializeField] float activeTime;
    [SerializeField] float activeStart;

    [Header("Alpha Value")]
    [SerializeField] float alphaSet;
    [SerializeField] float alphaMultiplier;
    float alpha;

    public override void Reset()
    {
        activeStart = 0;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            thisSprite = GetComponent<SpriteRenderer>();
            playerSprite = playerTransform.GetComponent<SpriteRenderer>();

            alpha = alphaSet;
            thisSprite.sprite = playerSprite.sprite;

            transform.position = playerTransform.position;
            transform.localScale = playerTransform.localScale;
            transform.rotation = playerTransform.rotation;

            activeStart = Time.time;
            color = Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {
        alpha *= alphaMultiplier;
        color.a = alpha;
        thisSprite.color = color;

        if (Time.time >= activeStart + activeTime)
        {
            // Return to the Object Pool
            gameObject.SetActive(false);
        }
    }
}
