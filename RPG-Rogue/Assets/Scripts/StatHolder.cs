using UnityEngine;

public class StatHolder : MonoBehaviour
{
    public Statistics stats;
    
    int health;
    int mana;
    float stamina;
    float maxStamina;
    int attack;
    int defense;
    int mAttack;
    int mDefense;
    float speed;

    void Start()
    {
        health = stats.health;
        mana = stats.mana;
        stamina = stats.stamina;
        attack = stats.attack;
        defense = stats.defense;
        mAttack = stats.mAttack;
        mDefense = stats.mDefense;
        speed = stats.speed;
        maxStamina = stats.stamina;
    }

    void Update()
    {
        // Clamp to never go below 0 or over the max
        if (stamina <= 0)
        {
            stamina = 0;
        }
        else if (stamina >= maxStamina)
        {
            stamina = maxStamina;
        }
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

    public void RecoverStamina(float recoverAmount)
    {
        stamina += (recoverAmount * Time.deltaTime);
    }

    public void DrainStamina(float drainAmount)
    {
        stamina -= (drainAmount * Time.deltaTime);
    }

    public void DodgeStamina(float dodgeDrainAmount)
    {
        stamina -= dodgeDrainAmount;
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
