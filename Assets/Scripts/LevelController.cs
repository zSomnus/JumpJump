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
    // Start is called before the first frame update
    void Start()
    {
        player = D.Get<Player>();
        playerObject = player.gameObject;

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
            Debug.Log("Reset");
            playerObject.transform.position = playerStartPosition;
            playerObject.SetActive(true);
            reseting = false;
        }
    }
}
