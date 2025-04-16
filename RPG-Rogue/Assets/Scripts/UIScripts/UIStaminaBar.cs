using UnityEngine;
using UnityEngine.UI;

public class UIStaminaBar : MonoBehaviour
{
    PlayerManager player;
    Image fillImage;
    Color origColor;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        fillImage = GetComponent<Image>();
        origColor = fillImage.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.LowStamina)
            fillImage.color = Color.yellow;
        else fillImage.color = origColor;
    }
}
