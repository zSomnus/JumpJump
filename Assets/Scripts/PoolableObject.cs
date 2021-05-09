using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolableObject : MonoBehaviour
{
    private Transform initialParent;

    public void Init()
    {
        initialParent = transform.parent;
    }

    public void ReturnToParent()
    {
        transform.SetParent(initialParent, false);
    }

    public abstract void Reset();
}
