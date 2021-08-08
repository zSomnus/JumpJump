using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBehaviour : MonoBehaviour
{
    void Awake()
    {
        if (GameDirector.IsReady)
        {
            OnAwake();
        }
    }

    protected virtual void OnAwake() { }

    void Start()
    {
        if (GameDirector.IsReady)
        {
            OnStart();
        }
    }

    protected virtual void OnStart() { }
}
