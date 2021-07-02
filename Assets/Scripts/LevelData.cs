using UnityEngine;

[CreateAssetMenu(menuName = "Level/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName = "?";
    public bool isIgnoredForDatabase = false;

#if UNITY_EDITOR
    public UnityEditor.SceneAsset scene = null;

    public void SetScene(UnityEditor.SceneAsset scene)
    {
        this.scene = scene;
        levelName = scene.name;
    }

    void OnValidate()
    {
        FixName();
    }
#endif

    public string GetSceneName()
    {
        return levelName;
    }

    internal void FixName()
    {
#if UNITY_EDITOR
        if (scene == null)
        {
            return;
        }

        levelName = scene.name;
        if (name != levelName)
        {
            Debug.LogWarning($"Asset name {name} different from scene name {levelName}");
        }
#endif
    }
}
