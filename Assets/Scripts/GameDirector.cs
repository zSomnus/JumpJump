using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject cameraCollider;
    [SerializeField] CinemachineConfiner cinemachineConfinder;

    LevelDatabase levelDatabase;

    string nextLevel;
    static int enemyCount;

    public int EnemyCount => enemyCount;

    public static string debugInitialLevel = "";
    public string CurrentLevel { get; private set; }
    public LevelData CurrentLevelData { get; set; }
    public static bool IsReady { get; set; }

    private void Awake()
    {
        IsReady = true;

        if (debugInitialLevel.Length > 0)
        {
            CurrentLevel = debugInitialLevel;
        }

        levelDatabase = D.Get<LevelDatabase>();
        CurrentLevelData = levelDatabase.GetLevelData(CurrentLevel);

        if (string.IsNullOrEmpty(nextLevel))
        {
            SceneManager.LoadSceneAsync(CurrentLevel, LoadSceneMode.Additive);
        }
    }

    public void LoadLevel(string levelName)
    {
        if (CurrentLevelData == null || CurrentLevelData.levelName != levelName)
        {
            CurrentLevelData = levelDatabase.GetLevelData(levelName);
        }

        if (levelName == "Main")
        {
            Debug.LogError("Not main!!", this);
            return;
        }

        nextLevel = levelName;

        TransitionLevelNow();
    }

    public void LoadLevel(LevelData levelData)
    {
        CurrentLevelData = levelData;
        LoadLevel(levelData.GetSceneName());
    }

    public void UnloadLevel(string levelName)
    {
        SceneManager.UnloadSceneAsync(levelName);
    }

    public void EnemyCountIncrease()
    {
        enemyCount++;
    }

    public void EnemyCountDecrease(Actor actor)
    {
        actor.OnPostDeath -= EnemyCountDecrease;

        if (enemyCount > 0)
        {
            enemyCount--;
        }
    }

    public void SetCameraCollider(PolygonCollider2D shape)
    {
        //GameObject currentCollider = GameObject.FindGameObjectWithTag("CameraCollider");
        //cameraCollider.GetComponent<PolygonCollider2D>().offset = shape.offset;
        //cameraCollider.GetComponent<PolygonCollider2D>().points = shape.points;
        cinemachineConfinder.m_BoundingShape2D = shape;
    }

    void TransitionLevelNow()
    {
        UnloadLevel(CurrentLevel);
        CurrentLevel = nextLevel;
        SceneManager.LoadSceneAsync(CurrentLevel, LoadSceneMode.Additive);
    }
}
