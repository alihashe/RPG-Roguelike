using UnityEngine;

[CreateAssetMenu(fileName = "New Stats", menuName = "Stats")]
public class Statistics : ScriptableObject
{
    public int health;
    public int mana;
    public float stamina;
    public int attack;
    public int defense;
    public int mAttack;
    public int mDefense;
    public float speed;
}
