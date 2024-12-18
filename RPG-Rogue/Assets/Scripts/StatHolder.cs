using UnityEngine;

public class StatHolder : MonoBehaviour
{
    public Statistics stats;

    public int health;
    public int maxHealth;
    public int mana;
    public int maxMana;
    public float stamina;
    public float maxStamina;
    public int attack;
    public int defense;
    public int mAttack;
    public int mDefense;
    public float speed;

    void Awake()
    {
        health = stats.health;
        mana = stats.mana;
        stamina = stats.stamina;
        attack = stats.attack;
        defense = stats.defense;
        mAttack = stats.mAttack;
        mDefense = stats.mDefense;
        speed = stats.speed;
        maxHealth = stats.health;
        maxMana = stats.mana;
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
        if (mana <= 0)
        {
            mana = 0;
        } 
        else if (mana >= maxMana)
        {
            mana = maxMana;
        }
    }

    public void TakeDamage(int rawDamage)
    {
        if (health > 0)
        {
            int hitDamage = rawDamage - defense;
            health -= hitDamage;
            Debug.Log(this.gameObject.name + " has taken " + hitDamage.ToString() + " damage and has " + health.ToString() + " health left.");
        }
    }

    public void RecoverHealth(int recoverAmount)
    {
        health += recoverAmount;
    }
    
    public void RecoverMana(int recoverAmount)
    {
        mana += recoverAmount;
    }

    public void SpendMana(int manaAmount)
    {
        mana -= manaAmount;
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

}
