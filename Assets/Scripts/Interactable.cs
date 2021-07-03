using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionID
{
    LevelTransfer,
}

public class Interactable : MonoBehaviour
{
    [SerializeField] InteractionID interactionID;
    [SerializeField] Vector2 interactionOffset = Vector2.zero;
    [SerializeField] GameObject iconGameObject;
    [SerializeField] string nextLevel;
    public Action OnInteraction;

    // Temp value
    GameDirector gameDirector;

    // Start is called before the first frame update
    void Start()
    {
        iconGameObject.SetActive(false);
        gameDirector = D.Get<GameDirector>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Interact()
    {
        OnInteraction?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            iconGameObject.SetActive(true);

            if (!string.IsNullOrEmpty(nextLevel))
            {
                gameDirector.LoadLevel(nextLevel);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            iconGameObject.SetActive(false);
        }
    }
}
