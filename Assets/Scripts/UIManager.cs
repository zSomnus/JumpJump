using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    Player player;
    [SerializeField] GameObject hpBarFill;
    // Start is called before the first frame update
    void Start()
    {
        player = D.Get<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        hpBarFill.transform.localScale = new Vector3(player.HpRatio(), 1, 1);
    }
}
