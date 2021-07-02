using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject cameraCollider;

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
            LoadLevel(CurrentLevel);
        }
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName, LoadSceneMode.Additive);
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

    public void SetCameraCollider()
    {
        GameObject currentCollider = GameObject.FindGameObjectWithTag("CameraCollider");
        cameraCollider.GetComponent<PolygonCollider2D>().offset = currentCollider.GetComponent<PolygonCollider2D>().offset;
        cameraCollider.GetComponent<PolygonCollider2D>().points = currentCollider.GetComponent<PolygonCollider2D>().points;
    }
}
