using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D
{
    readonly static Dictionary<string, MonoBehaviour> scripts = new Dictionary<string, MonoBehaviour>();

    public static void AddBinding(MonoBehaviour script)
    {
        string key = script.GetType().Name;

        if (scripts.ContainsKey(key))
        {
            scripts[key] = script;
        }
        else
        {
            scripts.Add(key, script);
        }
    }

    public static void RemoveBinding(MonoBehaviour script)
    {
        scripts.Remove(script.GetType().Name);
    }

    public static T TryGet<T>() where T : MonoBehaviour
    {
        return Get<T>(true);
    }

    public static T Get<T>(bool canFail = false) where T : MonoBehaviour
    {
        if (!GameDirector.IsReady)
        {
            return null;
        }

        string key = typeof(T).Name;

        if (!scripts.ContainsKey(key))
        {
            SearchForBinder();
        }

        if (!scripts.ContainsKey(key) && !canFail)
        {
            Debug.LogError($"D cannot find class {key}.");
        }

        return scripts.ContainsKey(key) ? (T)scripts[key] : null;
    }

    private static void SearchForBinder()
    {
        GameObject.FindGameObjectWithTag("DBinder")?.GetComponent<DBinder>().AddBindings();
    }
}
