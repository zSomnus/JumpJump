using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct ScenePathPair
{
#if UNITY_EDITOR
    public SceneAsset scene;
    public string path;

    public ScenePathPair(SceneAsset scene, string path)
    {
        this.scene = scene;
        this.path = path;
    }
#endif
}

[CreateAssetMenu(menuName = "Level/LevelDictionary")]
public class LevelDictionary : ScriptableObject
{
    public List<LevelData> allData = new List<LevelData>();

    string[] sceneSearchFolder = new string[] { "Assets/Scenes/Levels" };
    string[] levelSearchFolder = new string[] { "Assets/ScriptableObjects/Levels" };

    Dictionary<string, LevelData> levelNameToData = new Dictionary<string, LevelData>();

    private void Init()
    {
        if (levelNameToData.Count > 0)
        {
            return;
        }

        foreach (var levelData in allData)
        {
            levelNameToData.Add(levelData.levelName, levelData);
        }
    }

    public void Create()
    {
#if UNITY_EDITOR
        string[] sceneGuids = AssetDatabase.FindAssets("t:SceneAsset", sceneSearchFolder);
        Debug.Log($"Found {sceneGuids.Length} scenes.", this);
        if (sceneGuids.Length == 0)
        {
            return;
        }

        string[] levelGuids = AssetDatabase.FindAssets("t:LevelData", levelSearchFolder);
        Debug.Log($"Found {levelGuids.Length} levels.", this);
        if (levelGuids.Length == 0)
        {
            return;
        }

        var sceneList = new List<ScenePathPair>();
        foreach (var sceneGuid in sceneGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            sceneList.Add(new ScenePathPair(AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(assetPath), assetPath));
        }
        var levelNames = new HashSet<string>();

        foreach (var data in allData)
        {
            if (data != null)
            {
                levelNames.Add(data.name);
            }
        }

        var sceneDictionary = new Dictionary<SceneAsset, LevelData>();
        foreach (var levelGuid in levelGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(levelGuid);
            var levelData = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
            if (levelData.scene == null)
            {
                Debug.LogError($"Level data named {levelData.name} has empty scene.");
            }
            else
            {
                if (levelData.isIgnoredForDatabase)
                {
                    continue;
                }

                levelData.FixName();

                if (!levelNames.Contains(levelData.name))
                {
                    allData.Add(levelData);
                }

                if (!sceneDictionary.ContainsKey(levelData.scene))
                {
                    sceneDictionary.Add(levelData.scene, levelData);
                }
                else
                {
                    Debug.LogWarning($"Scene named {levelData.scene.name} already exists in dictionary.", this);
                }
            }
        }
#endif
    }

    internal LevelData GetLevel(string name)
    {
        Init();

        if (levelNameToData.ContainsKey(name))
        {
            Debug.Log("$Successfully returning level data {name}.");
            return levelNameToData[name];
        }
        Debug.LogWarning($"Cannot find level named {name}.");
        return null;
    }
}
