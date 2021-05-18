using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Player player;
    float playerHpRatio;
    [SerializeField] GameObject hpBarFill;
    [SerializeField] Image[] images;
    [SerializeField] Volume volume;
    Vignette vignette;

    // Start is called before the first frame update
    void Start()
    {
        player = D.Get<Player>();
        volume.profile.TryGet(out vignette);
    }

    // Update is called once per frame
    void Update()
    {
        playerHpRatio = player.GetHpRatio();
        hpBarFill.transform.localScale = new Vector3(playerHpRatio, 1, 1);

        if (playerHpRatio < 0.4f)
        {
            foreach (var image in images)
            {
                image.color = Color.Lerp(new Color(1, 0, 0, 0.2f), new Color(0, 0, 0, 0.5f), playerHpRatio);
            }

            vignette.intensity.value = Mathf.Lerp(0.6f, 0, playerHpRatio);
        }
        else if (images[0].color.r != 0)
        {
            foreach (var image in images)
            {
                image.color = new Color(0, 0, 0, 0.5f);
            }

            vignette.intensity.value = 0;
        }
    }
}
