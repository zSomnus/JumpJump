using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(LevelDictionary))]
public class LevelDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelDictionary levelDictionary = (LevelDictionary)target;

        if (GUILayout.Button("Update"))
        {
            levelDictionary.Create();
        }

        DrawDefaultInspector();
    }
}
#endif