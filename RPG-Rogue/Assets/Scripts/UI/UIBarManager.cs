using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIBarManager : MonoBehaviour
{
    StatHolder playerStats;
    Slider healthSlider;
    Slider magicSlider;
    Slider staminaSlider;

    void Awake()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<StatHolder>();
        // WARNING ---- The order of these in the hierarchy under the panel gameobject is important for assigning that right slider
        healthSlider = this.gameObject.transform.GetChild(0).GetComponent<Slider>(); // Get the slider from the first child gameobject attached to this one
        magicSlider = this.gameObject.transform.GetChild(1).GetComponent<Slider>(); // ...from the second child
        staminaSlider = this.gameObject.transform.GetChild(2).GetComponent<Slider>(); // ...from the third child
    }

    void Start()
    {
        healthSlider.maxValue = playerStats.maxHealth;
        magicSlider.maxValue = playerStats.maxMana;
        staminaSlider.maxValue = playerStats.maxStamina;
        healthSlider.minValue = 0;
        magicSlider.minValue = 0;
        staminaSlider.minValue = 0;
    }

    void Update()
    {
        healthSlider.value = playerStats.health;
        magicSlider.value = playerStats.mana;
        staminaSlider.value = playerStats.stamina;
    }
}
