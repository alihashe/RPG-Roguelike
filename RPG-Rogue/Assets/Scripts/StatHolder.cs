using UnityEngine;

public class StatHolder : MonoBehaviour
{
    public Statistics stats;

    int health;
    int maxHealth;
    int mana;
    int maxMana;
    float stamina;
    float maxStamina;
    int attack;
    int defense;
    int mAttack;
    int mDefense;
    float speed;

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

    #region Get/Set Methods
    public int GetHealth
    {
        get { return health; }
        private set { health = value; }
    }

    public int GetMana
    {
        get { return mana; }
        private set { mana = value; }
    }

    public float GetStamina
    {
        get { return stamina; }
        private set { stamina = value; }
    }

    public int GetMaxHealth
    {
        get { return maxHealth; }
        private set { maxHealth = value; }
    }

    public int GetMaxMana
    {
        get { return maxMana; }
        private set { maxMana = value; }
    }

    public float GetMaxStamina
    {
        get { return maxStamina; }
        private set { maxStamina = value; }
    }

    public int GetAttack
    {
        get { return attack; }
        private set { attack = value; }
    }

    public int GetDefense
    {
        get { return defense; }
        private set { defense = value; }
    }

    public int GetMAttack
    {
        get { return mAttack; }
        private set { mAttack = value; }
    }

    public int GetMDefense
    {
        get { return mDefense; }
        private set { mDefense = value; }
    }

    public float GetSpeed
    {
        get { return speed; }
        private set { speed = value; }
    }
    #endregion

    #region Increase/Decrease Methods
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
    #endregion

}