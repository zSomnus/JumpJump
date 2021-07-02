using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDatabase : MonoBehaviour
{
    [SerializeField] LevelDictionary levelDictionary;

    public LevelData GetLevelData(string name)
    {
        return levelDictionary.GetLevel(name);
    }
}
