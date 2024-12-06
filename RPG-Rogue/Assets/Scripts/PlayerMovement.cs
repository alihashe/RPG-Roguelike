using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    Rigidbody2D rb; // Reference to player rigidbody
    Transform attackCircle; // Empty gameobject used to create hitbox
    Vector2 moveDirection = Vector2.zero; // Start movement at 0
    LayerMask enemyLayer; // Used to only look for enemies with the hitbox
    SpriteRenderer playerSprite; // Reference for the player sprite
    PlayerInputActions playerAction; // Reference the system inputs that were converted to script
    StatHolder playerStats; // Create an instance of the attributes attached to the player
    // EnemyAIController enemyAI; // Create an instance of the EnemyAIController
    CharacterState currentState; // Current state of the player

    float attackRange = 0.5f; // Attack hitbox size
    float tiredCooldownTime = 3f; // The amount of time the player spends in the tired state
    float dodgeCooldownTime = 0.5f; // The amount of time before the player can press dodge again
    float dodgeDuration = 0.5f; // Invincibility frames per dodge roll
    float dodgeStaminaCost = 15f; // Stamina cost for each dodge roll
    float dodgeSpeed = 9.0f; // The speed the player will move while mid dodge
    float moveSpeed; // Dynamic movespeed - Set speed with stat sheet
    float baseMoveSpeed = 5.0f; // Base move speed; Should never change while in game
    float sprintSpeed = 7.0f; // The speed when sprinting
    float tiredSpeed = 4.0f; // The speed when under fatigue
    float staminaDrainSpeed = 10f; // The speed at which stamina will deplete when sprinting or rolling
    float staminaRecoverSpeed = 6f; // The speed at which stamina will recover when idle or moving
    float staminaRecoveryTemp; // Temp variable used to resume stamina recovery speed after temporarily halting it
    
    bool inDodgeCooldown { get; set; } // If the player just pressed dodge, this prevents spam and bugs
    bool inTiredCooldown { get; set; } // If the player runs out of stamina, this prevents them from sprinting or dodging by putting them in the tired state
    bool isMoving { get; set; } // Is the player moving
    bool isSprinting { get; set; } // Is the player sprinting
    bool isAttacking { get; set; } // Is the player attacking
    bool isDodging { get; set; } // Is the player dodging

    // Instances of actions from the Input Manager
    InputAction move;
    InputAction attack;
    InputAction dodge;
    InputAction interact;
    InputAction sprint;
    #endregion

    void Awake()
    {
        playerAction = new PlayerInputActions();
        playerStats = GetComponent<StatHolder>();
        playerSprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        attackCircle = this.gameObject.transform.GetChild(0).GetComponent<Transform>(); // !!! MAKE SURE THE ATTACK HITBOX GAMEOBJECT IS THE FIRST CHILD OF THE PLAYER !!!
        enemyLayer = LayerMask.GetMask("Enemies"); // Used to differentiate targets within the player hitbox
    }

    void Start()
    {
        currentState = CharacterState.Idle; // Start player in Idle
        moveSpeed = playerStats.speed; // Set the movespeed to the speed set through the stat instance
        staminaRecoveryTemp = staminaRecoverSpeed; // Set the temp variable to the correct original float
    }

    void OnEnable()
    {
        // Sets references to action in the input manager
        move = playerAction.Player.Move;
        attack = playerAction.Player.Attack;
        dodge = playerAction.Player.Dodge;
        interact = playerAction.Player.Interact;
        sprint = playerAction.Player.Sprint;

        // Subscribes the local functions to the events attached to the input system
        move.Enable();
        attack.Enable();
        attack.performed += Attacked;
        dodge.Enable();
        dodge.performed += Dodged;
        interact.Enable();
        interact.performed += Interacted;
        sprint.Enable();
        sprint.performed += Sprinted;
    }

    void OnDisable()
    {
        // Good practice: Cleans up code by unsubcribing when object is no longer needed
        move.Disable();
        attack.Disable();
        dodge.Disable();
        interact.Disable();
        sprint.Disable();
    }

    void FixedUpdate()
    {
        // Moves the rigidbody by multiplying the directional input by the speed variable
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }

    void Update()
    {
        // Used to calculate the movement and position of the player
        moveDirection = move.ReadValue<Vector2>();

        // Player Rotation based on movement
        #region Four Direction Movement
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            playerSprite.flipX = false;
            attackCircle.transform.localPosition = new Vector2(0.5f, 0);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            playerSprite.flipX = true;
            attackCircle.transform.localPosition = new Vector2(-0.5f, 0);
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            playerSprite.flipY = false;
            attackCircle.transform.localPosition = new Vector2(0, 0.5f);
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            playerSprite.flipY = true;
            attackCircle.transform.localPosition = new Vector2(0, -0.5f);
        }
        #endregion
        #region Diagonal Movement
        if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) && ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))))
        {
            playerSprite.flipX = false;
            playerSprite.flipY = false;
            rb.MoveRotation(-45f);
        }
        else if ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) && ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))))
        {
            playerSprite.flipX = false;
            playerSprite.flipY = true;
            rb.MoveRotation(45f);
        }
        else if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) && ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))))
        {
            playerSprite.flipX = true;
            playerSprite.flipY = true;
            rb.MoveRotation(-45f);
        }
        else if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) && ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))))
        {
            playerSprite.flipX = true;
            playerSprite.flipY = false;
            rb.MoveRotation(45f);
        }
        else
            transform.rotation = quaternion.identity;
        #endregion

        // Handle switching between states
        switch (currentState)
        {
            case CharacterState.Idle:
                HandleIdleState();
                break;
            case CharacterState.Moving:
                HandleMovingState();
                break;
            case CharacterState.Sprinting:
                HandleSprintingState();
                break;
            case CharacterState.Attacking:
                HandleAttackingState();
                break;
            case CharacterState.Dodging:
                StartCoroutine(HandleDodgingState(dodgeDuration));
                break;
            case CharacterState.Tired:
                StartCoroutine(HandleTiredState(tiredCooldownTime));
                break;
        }

    }

    #region Handle State Functions
    void HandleIdleState()
    {
        // SetAnimation("Idle"); // Play idle animation
        moveSpeed = baseMoveSpeed;
        playerStats.RecoverStamina(staminaRecoverSpeed); // When not sprinting or dodging, recover stamina
        isSprinting = false; // Solution to pressing sprint without moving bug - Always false when Idle
        if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && !playerAction.Player.Sprint.IsPressed()) // If there is movement input and sprint is not pressed...
        {
            currentState = CharacterState.Moving; // Switch to moving state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && playerAction.Player.Sprint.IsPressed()) // If there is movement input but sprint is pressed...
        {
            currentState = CharacterState.Sprinting; // Switch to sprinting state
        }
        else if (isAttacking)
        {
            currentState = CharacterState.Attacking; // Switch to attacking state
        }
    }

    void HandleMovingState()
    {
        isMoving = true;
        moveSpeed = baseMoveSpeed;
        playerStats.RecoverStamina(staminaRecoverSpeed);
        if (Mathf.Abs(rb.linearVelocity.magnitude) < 0.1f && !isSprinting)
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
            currentState = CharacterState.Idle; // Switch to idle state
        }
        else if (playerAction.Player.Sprint.IsPressed()) { 
            isMoving = false;
            currentState = CharacterState.Sprinting; // Switch to sprinting state
        }
        else if (isAttacking)
        {
            isMoving = false;
            currentState = CharacterState.Attacking; // Switch to attack state
        }
        else if (playerAction.Player.Dodge.IsPressed() && !inDodgeCooldown)
        {
            isMoving = false;
            playerStats.DodgeStamina(dodgeStaminaCost);
            currentState = CharacterState.Dodging; // Switch to dodging state
        }
    }

    void HandleSprintingState()
    {
        isSprinting = true;
        moveSpeed = sprintSpeed;
        playerStats.DrainStamina(staminaDrainSpeed);
        if (Mathf.Abs(rb.linearVelocity.magnitude) < 0.1f)
        {
            isSprinting = false;
            rb.linearVelocity = Vector2.zero;
            currentState = CharacterState.Idle; // Switch to idle state
        }
        else if (!playerAction.Player.Sprint.IsPressed())
        {
            isSprinting = false;
            currentState = CharacterState.Moving; // Switch to moving state
        }
        else if (isAttacking)
        {
            isSprinting = false;
            currentState = CharacterState.Attacking; // Switch to attacking state
        }
        else if (playerAction.Player.Dodge.IsPressed() && !inDodgeCooldown && !inTiredCooldown)
        {
            isSprinting = false;
            playerStats.DodgeStamina(dodgeStaminaCost);
            currentState = CharacterState.Dodging; // Switch to dodging state
        }
        else if (playerStats.stamina <= 0)
        {
            isSprinting = false;
            currentState = CharacterState.Tired; // Switch to tired state
        }
    }

    IEnumerator HandleDodgingState(float dodgeTiming)
    {
        isDodging = true;
        moveSpeed = dodgeSpeed;
        playerSprite.color = Color.red;
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(dodgeTiming);
        playerSprite.color = Color.blue;
        GetComponent<Collider2D>().enabled = true;
        isDodging = false;
        if (Mathf.Abs(rb.linearVelocity.magnitude) < 0.1f) // If there is no movement...
        {
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(DodgeCoolDown(dodgeCooldownTime));
            currentState = CharacterState.Idle; // Switch to idle state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && !playerAction.Player.Sprint.IsPressed()) // If there is movement input and sprint is not pressed...
        {
            StartCoroutine(DodgeCoolDown(dodgeCooldownTime));
            currentState = CharacterState.Moving; // Switch to moving state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && playerAction.Player.Sprint.IsPressed()) // If there is movement input but sprint is pressed...
        {
            StartCoroutine(DodgeCoolDown(dodgeCooldownTime));
            currentState = CharacterState.Sprinting; // Switch to sprinting state
        }
        else if (playerStats.stamina <= 0) // If stamina runs out...
        {
            currentState = CharacterState.Tired; // Switch to tired state
        }
    }

    IEnumerator DodgeCoolDown(float cooldownTime)
    {
        inDodgeCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        inDodgeCooldown = false;
    }

    IEnumerator HandleTiredState(float fatigueTime)
    {
        inTiredCooldown = true;
        moveSpeed = tiredSpeed;
        staminaRecoverSpeed = 0f;
        yield return new WaitForSeconds(fatigueTime);
        playerStats.stamina = 5f;
        staminaRecoverSpeed = staminaRecoveryTemp;
        inTiredCooldown = false;
        if (Mathf.Abs(rb.linearVelocity.magnitude) < 0.1f && !inTiredCooldown)
        {
            currentState = CharacterState.Idle; // Switch to idle state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && !playerAction.Player.Sprint.IsPressed() && !inTiredCooldown)
        {
            currentState = CharacterState.Moving; // Switch to moving state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && playerAction.Player.Sprint.IsPressed() && !inTiredCooldown)
        {
            currentState = CharacterState.Sprinting; // Switch to sprinting state
        }
    }

    void HandleAttackingState()
    {
        Debug.Log("Is Attacking State");
        // Set attacking animation
        //      SetAnimation("Attack");
        //      After the attack finishes, transition back to idle
        if (!isAttacking)
        {
            currentState = CharacterState.Idle;  // Switch to idle state
        }
    }

    // Set animation based on the state   KEEP FOR LATER
    /*private void SetAnimation(string animationName)
    {
        // Example of setting animation using an Animator component
        Animator animator = GetComponent<Animator>();
        animator.Play(animationName);
    }*/
    #endregion

    // Draw attack hit box
    void OnDrawGizmosSelected()
    {
        if (attackCircle != null)
            Gizmos.DrawWireSphere(attackCircle.position, attackRange);
    }

    #region Input Action Callbacks
    void Attacked(InputAction.CallbackContext context)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCircle.position, attackRange, enemyLayer); // Array that collects all gameobjects in the Enemy Layer that are touching the hitbox in front of the player

        foreach(Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit: " + enemy.name);
            enemy.GetComponent<StatHolder>().TakeDamage(playerStats.attack); // For each enemy in the hitbox at the time of an attack, use the TakeDamage function attached to the StatHolder script on them
        }
    }

    void Dodged(InputAction.CallbackContext context)
    {
        if (playerStats.stamina > dodgeStaminaCost && currentState != CharacterState.Idle)
            isDodging = true;
    }

    void Sprinted(InputAction.CallbackContext context)
    {
        if (playerStats.stamina > 0)
            isSprinting = true;
    }

    void Interacted(InputAction.CallbackContext context)
    {
        Debug.Log("INTERACT!");
    }
    #endregion

}