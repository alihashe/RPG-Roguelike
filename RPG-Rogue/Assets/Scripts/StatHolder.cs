using UnityEngine;

public class StatHolder : MonoBehaviour
{
    public Statistics stats;

    int health;
    int mana;
    int attack;
    int defense;
    int mAttack;
    int mDefense;

    void Start()
    {
        health = stats.health;
        mana = stats.mana;
        attack = stats.attack;
        defense = stats.defense;
        mAttack = stats.mAttack;
        mDefense = stats.mDefense;
    }

}
