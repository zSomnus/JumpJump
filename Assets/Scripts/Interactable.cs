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
    private GameObject iconGameObject;
    public Action OnInteraction;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Interact()
    {
        OnInteraction?.Invoke();
    }
}
