using UnityEngine;

public class StatHolder : MonoBehaviour
{
    public Statistics stats;
    
    int health;
    int mana;
    int stamina;
    int attack;
    int defense;
    int mAttack;
    int mDefense;

    void Start()
    {
        health = stats.health;
        mana = stats.mana;
        stamina = stats.stamina;
        attack = stats.attack;
        defense = stats.defense;
        mAttack = stats.mAttack;
        mDefense = stats.mDefense;
    }

    void Update()
    {
        // Clamp health to never go below 0
        if (health <= 0) 
        {
            health = 0;
        }
    }

    public void TakeDamage(int rawDamage)
    {
        if (health > 0)
        {
            int hitDamage = rawDamage - defense;
            health -= hitDamage;
            Debug.Log(this.gameObject.name + " has taken " + hitDamage.ToString() + " damage and has " + health.ToString() + " health left.");
            if (health <= 0)
            {
                Death();
            }
        }
    }

    public void DrainStamina()
    {
        stamina -= 1;
    }

    public void Death()
    {
        Debug.Log(this.gameObject.name + " has died!");
        // Play a death animation

        // Disable collider
        GetComponent<CircleCollider2D>().enabled = false;
        // Disable gameObject
        this.enabled = false;
    }

}
