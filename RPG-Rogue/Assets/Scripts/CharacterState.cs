using UnityEngine;

public enum CharacterState
{
    /* When adding another CharacterState make sure you update the following aspects of the PlayerMovement script
        - bool variable example -> "is_____" 
        - update the switch statement with another case
        - create a new function for handleing the state
        - update all of the state functions to transistion into the new state                                  */
    
    Idle,
    Moving,
    Sprinting,
    Attacking
}
