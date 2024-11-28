using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb; // Reference to player rigidbody
    Transform attackCircle; // Empty gameobject used to create hitbox
    LayerMask enemyLayer; // Used to only look for enemies with the hitbox
    SpriteRenderer playerSprite; // Reference for the player sprite
    PlayerInputActions playerAction; // Reference the system inputs that were converted to script
    StatHolder playerStats; // Create an instance of the attributes attached to the player
    CharacterState currentState; // Current state of the player

    float attackRange = 0.5f; // Attack hitbox size
    float iFrameDuration = 0.5f; // Seconds of Iframes per dodge roll
    [SerializeField] float moveSpeed = 5.0f; // Dynamic movespeed
    float baseMoveSpeed = 5.0f; // Base move speed; Should never change while in game
    float sprintSpeed = 7.0f; // The speed when sprinting
    //float staminaDrainSpeed = 0.25f; // The number of seconds that pass before the DrainStamina function is repeated
    int originalStamina; // The amount of stamina that the player starts out with
    bool IsMoving { get; set; } // Is the player moving
    bool IsSprinting { get; set; } // Is the player sprinting
    bool IsAttacking { get; set; } // Is the player attacking
    bool IsDodging { get; set; } // Is the player dodging

    Vector2 moveDirection = Vector2.zero; // Start movement at 0
    // Instances of actions from the Input Manager
    InputAction move;
    InputAction attack;
    InputAction dodge;
    InputAction interact;
    InputAction sprint;

    void Awake()
    {
        playerAction = new PlayerInputActions();
        playerStats = GetComponent<StatHolder>();
        playerSprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        attackCircle = this.gameObject.transform.GetChild(0).GetComponent<Transform>(); // MAKE SURE THE ATTACK HITBOX GAMEOBJECT IS THE FIRST CHILD OF THE PLAYER
        enemyLayer = LayerMask.GetMask("Enemies"); // Used to differentiate targets within the player hitbox
    }

    void Start()
    {
        currentState = CharacterState.Idle; // Start player in Idle
        originalStamina = playerStats.stats.stamina; // Set the player stamina to the original amount when intialized
        iFrameDuration *= Time.deltaTime;
    }

    private void OnEnable()
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

    private void OnDisable()
    {
        // Good practice: Cleans up code by unsubcribing when object is no longer needed
        move.Disable();
        attack.Disable();
        dodge.Disable();
        interact.Disable();
        sprint.Disable();
    }

    private void FixedUpdate()
    {
        // Moves the rigidbody by multiplying the directional input by the speed variable
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }

    void Update()
    {
        // Used to calculate the movement and position of the player
        moveDirection = move.ReadValue<Vector2>();

        // Clamp stamina
        if (playerStats.stats.stamina < 0)
            playerStats.stats.stamina = 0;
        else if (playerStats.stats.stamina > originalStamina)
            playerStats.stats.stamina = originalStamina;

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
                HandleDodgingState();
                break;
        }

    }

    #region State Functions
    void HandleIdleState()
    {
        // SetAnimation("Idle"); // Play idle animation
        moveSpeed = baseMoveSpeed;
        IsSprinting = false; // Hack solution to pressing sprint without moving bug
        if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && !playerAction.Player.Sprint.IsPressed()) // If there is movement input and sprint is not pressed...
        {
            currentState = CharacterState.Moving; // Switch to moving state
        }
        else if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && playerAction.Player.Sprint.IsPressed()) // If there is movement input but sprint is pressed...
        {
            currentState = CharacterState.Sprinting; // Switch to sprinting state
        }
        else if (IsAttacking)
        {
            currentState = CharacterState.Attacking; // Switch to attacking state
        }
        else if (playerAction.Player.Dodge.IsPressed())
        {
            currentState = CharacterState.Dodging;
        }
    }

    void HandleMovingState()
    {
        IsMoving = true;
        moveSpeed = baseMoveSpeed;
        if (Mathf.Abs(rb.linearVelocity.magnitude) < 0.1f && !IsSprinting)
        {
            IsMoving = false;
            rb.linearVelocity = Vector2.zero;
            currentState = CharacterState.Idle; // Switch to idle state
        }
        else if (playerAction.Player.Sprint.IsPressed()) { 
            IsMoving = false;
            currentState = CharacterState.Sprinting; // Switch to sprinting state
        }
        else if (IsAttacking)
        {
            IsMoving = false;
            currentState = CharacterState.Attacking; // Switch to attack state
        }
        else if (playerAction.Player.Dodge.IsPressed())
        {
            currentState = CharacterState.Dodging;
        }
    }

    void HandleSprintingState()
    {
        IsSprinting = true;
        moveSpeed = sprintSpeed;
        if (Mathf.Abs(rb.linearVelocity.magnitude) < 0.1f)
        {
            IsSprinting = false;
            rb.linearVelocity = Vector2.zero;
            currentState = CharacterState.Idle; // Switch to idle state
        }
        else if (!playerAction.Player.Sprint.IsPressed())
        {
            IsSprinting = false;
            currentState = CharacterState.Moving; // Switch to moving state
        }
        else if (IsAttacking)
        {
            IsSprinting = false;
            currentState = CharacterState.Attacking; // Switch to attacking state
        }
        else if (playerAction.Player.Dodge.IsPressed())
        {
            currentState = CharacterState.Dodging;
        }
    }

    void HandleDodgingState()
    {
        IsDodging = true;


    }

    void HandleAttackingState()
    {
        Debug.Log("Is Attacking State");
        // Set attacking animation
        //      SetAnimation("Attack");
        //      After the attack finishes, transition back to idle
        if (!IsAttacking)
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

    // Draw hit box for testing
    void OnDrawGizmosSelected()
    {
        if (attackCircle != null)
            Gizmos.DrawWireSphere(attackCircle.position, attackRange);
    }

    IEnumerator DodgeIFrames(float dodgeTiming)
    {
        Debug.Log("Player is invincible... yay");
        yield return new WaitForSeconds(dodgeTiming);
        Debug.Log("Player is NOT invincible... booooooo");
    }



    #region Input Action Functions
    void Attacked(InputAction.CallbackContext context)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCircle.position, attackRange, enemyLayer);

        foreach(Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit: " + enemy.name);
            enemy.GetComponent<StatHolder>().TakeDamage(playerStats.stats.attack);
        }
    }

    void Dodged(InputAction.CallbackContext context)
    {

    }

    void Sprinted(InputAction.CallbackContext context)
    {
        if (playerStats.stats.stamina > 0)
        {
            IsSprinting = true;
        }
    }

    void Interacted(InputAction.CallbackContext context)
    {
        Debug.Log("INTERACT!");
    }
    #endregion

}