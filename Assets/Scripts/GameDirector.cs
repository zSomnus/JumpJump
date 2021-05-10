using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    [System.Obsolete]
    private void Awake()
    {
        Application.LoadLevelAdditive("Map");
    }
}
