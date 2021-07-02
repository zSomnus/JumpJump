using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelInfo : MonoBehaviour
{
    GameDirector gameDirector;
    private void Awake()
    {
        gameDirector = D.Get<GameDirector>();

        if (gameDirector != null)
        {

        }
        else
        {
            GameDirector.debugInitialLevel = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene("Main");
        }
    }
}
