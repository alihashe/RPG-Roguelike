using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    #region Variables
    Rigidbody2D rb; // Reference to player rigidbody
    Transform attackCircle; // Empty gameobject used to create hitbox
    Vector2 moveDirection = Vector2.zero; // Start movement at 0
    LayerMask enemyLayer; // Used to only look for enemies with the hitbox
    SpriteRenderer playerSprite; // Reference for the player sprite
    PlayerInputActions playerAction; // Reference the system inputs that were converted to script
    StatHolder playerStats; // Create an instance of the attributes attached to the player
    CharacterState currentState; // Current state of the player

    float attackRange = 0.5f; // Attack hitbox size
    [SerializeField] float tiredCooldownTime = 3f; // The amount of time the player spends in the tired state
    float tiredTempTime;
    float dodgeCooldownTime = 1.5f; // The amount of time before the player can press dodge again
    float dodgeTemp; // The temp variable used to store the initial dodge cooldown time
    float dodgeDuration = 0.25f; // Invincibility frames per dodge roll
    float dodgeStaminaCost = 20f; // Stamina cost for each dodge roll
    float dodgeSpeed = 8.0f; // The speed the player will move while mid dodge
    float moveSpeed; // Dynamic movespeed - Set speed with stat sheet
    float tiredSpeed = 4.0f; // The speed when under fatigue
    float staminaDrainSpeed = 10f; // The speed at which stamina will deplete when sprinting or rolling
    float staminaRecoverSpeed = 8f; // The speed at which stamina will recover when idle or moving
    float staminaRecoveryTemp; // Temp variable used to resume stamina recovery speed after temporarily halting it

    bool inDodgeCooldown { get; set; } // If the player just pressed dodge, this prevents spam and bugs
    bool inTiredCooldown { get; set; } // If the player runs out of stamina, this prevents them from sprinting or dodging by putting them in the tired state
    bool isMoving { get; set; } // Is the player moving
    bool isSprinting { get; set; } // Is the player sprinting
    bool isAttacking { get; set; } // Is the player attacking
    bool isDodging { get; set; } // Is the player dodging
    bool isInvulnerable { get; set; } // Should the player be attackable
    public bool lowStamina { get; set; } // Used to change the stamina bar color when stamina is low

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
        moveSpeed = playerStats.GetSpeed; // Set the movespeed to the speed set through the stat instance
        staminaRecoveryTemp = staminaRecoverSpeed; // Set the temp variable to the correct original float
        dodgeTemp = dodgeCooldownTime;
        tiredTempTime = tiredCooldownTime;
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

        // Handles the force propulsion when dodging
        if (moveDirection != Vector2.zero && isDodging && !inDodgeCooldown)
            rb.AddForce((moveDirection * dodgeSpeed), ForceMode2D.Impulse);

    }

    void Update()
    {
        // Used to calculate the movement and position of the player
        moveDirection = move.ReadValue<Vector2>();

        // When the number is positive, the player can dodge again
        dodgeCooldownTime -= Time.deltaTime;
        if (dodgeCooldownTime > 0)
            inDodgeCooldown = true;
        else
            inDodgeCooldown = false;

        tiredCooldownTime -= Time.deltaTime;
        if (tiredCooldownTime > 0)
        {
            inTiredCooldown = true;
            staminaRecoverSpeed = 0;
        }
        else
        {
            inTiredCooldown = false;
            staminaRecoverSpeed = staminaRecoveryTemp;
        }

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

        // Use this bool to determine what color the stamina bar should be
        if (playerStats.GetStamina < dodgeStaminaCost)
            lowStamina = true;
        else lowStamina = false;

        // Determines when the player should be recovering stamina
        if (currentState == CharacterState.Idle || currentState == CharacterState.Moving)
        {
            playerStats.RecoverStamina(staminaRecoverSpeed);
        }

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
        }

    }

    #region Handle State Functions
    void HandleIdleState()
    {
        // SetAnimation("Idle"); // Play idle animation
        if (!inTiredCooldown)
            moveSpeed = playerStats.GetSpeed;
        else
            moveSpeed = tiredSpeed;
        isDodging = false;
        isSprinting = false; // Solution to pressing sprint without moving bug - Always false when Idle
        if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && !playerAction.Player.Sprint.IsPressed()) // If there is movement input and sprint is not pressed...
        {
            currentState = CharacterState.Moving; // Switch to moving state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && playerAction.Player.Sprint.IsPressed() && !inTiredCooldown) // If there is movement input but sprint is pressed...
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
        isDodging = false;
        moveSpeed = playerStats.GetSpeed;
        if (Mathf.Abs(rb.linearVelocity.magnitude) < 0.1f && !isSprinting)
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
            currentState = CharacterState.Idle; // Switch to idle state
        }
        else if (playerAction.Player.Sprint.IsPressed() && !inTiredCooldown)
        {
            isMoving = false;
            currentState = CharacterState.Sprinting; // Switch to sprinting state
        }
        else if (isAttacking)
        {
            isMoving = false;
            currentState = CharacterState.Attacking; // Switch to attack state
        }
        else if (playerAction.Player.Dodge.IsPressed() && !inDodgeCooldown && (playerStats.GetStamina > dodgeStaminaCost) && !inTiredCooldown)
        {
            isMoving = false;
            playerStats.DodgeStamina(dodgeStaminaCost);
            currentState = CharacterState.Dodging; // Switch to dodging state
        }
    }

    void HandleSprintingState()
    {
        isSprinting = true;
        isDodging = false;
        if (!inTiredCooldown)
            moveSpeed = playerStats.GetSprintSpeed;
        else
            moveSpeed = tiredSpeed;
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
        else if (playerAction.Player.Dodge.IsPressed() && !inDodgeCooldown && !inTiredCooldown && (playerStats.GetStamina > dodgeStaminaCost))
        {
            isSprinting = false;
            playerStats.DodgeStamina(dodgeStaminaCost);
            currentState = CharacterState.Dodging; // Switch to dodging state
        }
        else if (playerStats.GetStamina <= 0.5f)
        {
            isSprinting = false;
            tiredCooldownTime = tiredTempTime; // Reset tired cooldown timer
        }
    }

    IEnumerator HandleDodgingState(float dodgeTiming)
    {
        isDodging = true;
        playerSprite.color = Color.red;
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(dodgeTiming);
        playerSprite.color = Color.blue;
        GetComponent<Collider2D>().enabled = true;
        isDodging = false;
        if (Mathf.Abs(rb.linearVelocity.magnitude) < 0.1f) // If there is no movement...
        {
            rb.linearVelocity = Vector2.zero;
            dodgeCooldownTime = dodgeTemp;
            currentState = CharacterState.Idle; // Switch to idle state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && !playerAction.Player.Sprint.IsPressed()) // If there is movement input and sprint is not pressed...
        {
            dodgeCooldownTime = dodgeTemp;
            currentState = CharacterState.Moving; // Switch to moving state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && playerAction.Player.Sprint.IsPressed()) // If there is movement input but sprint is pressed...
        {
            dodgeCooldownTime = dodgeTemp;
            currentState = CharacterState.Sprinting; // Switch to sprinting state
        }
        else if (playerStats.GetStamina <= 0.5f) // If stamina runs out...
        {
            tiredCooldownTime = tiredTempTime; // Reset tired cooldown timer
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

    void Backstep()
    {
        // Handles the force propulsion when backstepping
        if (currentState == CharacterState.Idle && !inDodgeCooldown)
            rb.AddForce((-moveDirection * dodgeSpeed), ForceMode2D.Impulse);
        dodgeCooldownTime = dodgeTemp;
    }

    #region Input Action Callbacks
    void Attacked(InputAction.CallbackContext context)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCircle.position, attackRange, enemyLayer); // Array that collects all gameobjects in the Enemy Layer that are touching the hitbox in front of the player

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit: " + enemy.name);
            enemy.GetComponent<StatHolder>().TakeDamage(playerStats.GetAttack); // For each enemy in the hitbox at the time of an attack, use the TakeDamage function attached to the StatHolder script on them
        }
    }

    void Dodged(InputAction.CallbackContext context)
    {
        if (currentState != CharacterState.Idle)
            isDodging = true;
        else
            Backstep();
    }

    void Sprinted(InputAction.CallbackContext context)
    {
        if (playerStats.GetStamina > 0)
            isSprinting = true;
    }

    void Interacted(InputAction.CallbackContext context)
    {
        Debug.Log("INTERACT!");
    }
    #endregion

}