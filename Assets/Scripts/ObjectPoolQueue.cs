using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolQueue
{
    List<PoolableObject> poolables = new List<PoolableObject>();
    int index;

    public void Add(PoolableObject poolableObject)
    {
        poolables.Add(poolableObject);
    }

    public PoolableObject Get()
    {
        if (poolables.Count == 0)
        {
            Debug.LogError($"Pool is empty.");
            return null;
        }
        PoolableObject poolableObject = poolables[index++];
        index %= poolables.Count;
        poolableObject.Reset();
        return poolableObject;
    }

    public void Inactivate()
    {
        if (poolables.Count > 0)
        {
            foreach (var pool in poolables)
            {
                pool.gameObject.SetActive(false);
            }
        }
    }
}
