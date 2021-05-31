using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] Vector3 playerStartPosition;
    [SerializeField] float resetTime = 1f;
    bool reseting;
    Player player;
    GameObject playerObject;
    [SerializeField] GameObject levelDoor;
    [SerializeField] string nextLevel;
    GameDirector gameDirector;
    // Start is called before the first frame update
    void Start()
    {
        player = D.Get<Player>();
        playerObject = player.gameObject;
        levelDoor.SetActive(false);
        gameDirector = D.Get<GameDirector>();
        gameDirector.SetCameraCollider();

        if (playerObject != null)
        {
            playerObject.transform.position = playerStartPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerObject.activeInHierarchy == false && !reseting)
        {
            StartCoroutine(ResetPlayer());
        }

        if (gameDirector.EnemyCount <= 0 && levelDoor.activeInHierarchy == false)
        {
            levelDoor.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(playerStartPosition, 1f);
    }

    IEnumerator ResetPlayer()
    {
        reseting = true;

        yield return new WaitForSeconds(resetTime);

        if (player != null)
        {
            playerObject.transform.position = playerStartPosition;
            playerObject.SetActive(true);
            reseting = false;
        }
    }
}
