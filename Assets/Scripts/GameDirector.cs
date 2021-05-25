using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    string nextLevel;
    static int enemyCount;

    public int EnemyCount => enemyCount;

    private void Awake()
    {
        if (string.IsNullOrEmpty(nextLevel))
        {
            LoadLevel("Map");
        }
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName, LoadSceneMode.Additive);
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
}
