using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MaskEntry
{
    public string name;
    public LayerMask value = default;
}

public class MaskHelper : MonoBehaviour
{
    [SerializeField] MaskEntry[] maskEntries = null;

    internal static int IgnoreObstacle = 14;
    internal static int GeneralActors = 17;
    static Dictionary<string, LayerMask> maskByName = new Dictionary<string, LayerMask>();

    private void Awake()
    {
        if (maskByName.Count == 0)
        {
            foreach (var entry in maskEntries)
            {
                maskByName.Add(entry.name, entry.value);
            }
        }
    }

    public static LayerMask GetLayerMask(string name)
    {
        return maskByName[name];
    }

    public static bool IsGameObjectInMask(LayerMask mask, GameObject gameObject)
    {
        return mask == (mask | 1 << gameObject.layer);
    }
}
