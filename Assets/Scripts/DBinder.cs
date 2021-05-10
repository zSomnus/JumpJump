using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBinder : MonoBehaviour
{
    [SerializeField] MonoBehaviour[] bindTargets = null;
    bool bindingsAdded;

    private void OnValidate()
    {
        foreach (var target in bindTargets)
        {
            if (target == null)
            {
                Debug.LogError("There is a null bind target.", this);
            }
        }
    }

    public void AddBindings()
    {
        if (bindingsAdded)
        {
            return;
        }

        bindingsAdded = true;

        foreach (var target in bindTargets)
        {
            D.AddBinding(target);
        }
    }

    private void OnDestroy()
    {
        foreach (var target in bindTargets)
        {
            D.RemoveBinding(target);
        }
    }
}
