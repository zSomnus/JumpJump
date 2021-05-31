using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject cameraCollider;

    string nextLevel;
    static int enemyCount;

    public int EnemyCount => enemyCount;

    private void Awake()
    {
        if (string.IsNullOrEmpty(nextLevel))
        {
            LoadLevel("Map1");
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
