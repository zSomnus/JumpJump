using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    Dictionary<string, ObjectPoolQueue> pools = new Dictionary<string, ObjectPoolQueue>();

    private void Awake()
    {
        foreach (Transform pool in transform)
        {
            var objectPoolQueue = new ObjectPoolQueue();

            foreach (Transform poolChild in pool)
            {
                var poolableObject = poolChild.GetComponent<PoolableObject>();
                poolableObject.Init();
                objectPoolQueue.Add(poolableObject);
            }

            pools.Add(pool.name, objectPoolQueue);
        }
    }

    public GameObject GetFromPool(string objectName)
    {
        if (pools.ContainsKey(objectName))
        {
            return pools[objectName].Get().gameObject;
        } else
        {
            Debug.LogError("Cannot find pool " + objectName, this);
            return null;
        }
    }

    public void Reset()
    {
        if (pools.Count > 0)
        {
            foreach (var pool in pools)
            {
                pool.Value.Inactivate();
            }
        }
    }

    public void Reset(string poolName)
    {
        pools[poolName].Inactivate();
    }
}
