using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb; // Reference to player rigidbody
    Transform attackCircle; // Empty gameobject used to create hitbox
    LayerMask enemyLayer; // Used to only look for enemies with the hitbox
    SpriteRenderer playerSprite; // Reference for the player sprite
    float attackRange = 0.5f; // Attack hitbox size
    float baseMoveSpeed = 5.0f; // Base move speed; Should never change while in game
    float staminaDrainSpeed = 0.25f; // The number of seconds that pass before the DrainStamina function is repeated
    int originalStamina; // The amount of stamina that the player starts out with
    [SerializeField] float moveSpeed = 5.0f; // Dynamic movespeed
    PlayerInputActions playerAction; // Reference the system inputs that were converted to script
    StatHolder playerStats; // Create an instance of the attributes attached to the player

    Vector2 moveDirection = Vector2.zero; // Start movement at 0
    InputAction move; // Instances of actions from the Input Manager
    InputAction attack;
    InputAction interact;
    InputAction sprint;

    void Awake()
    {
        playerAction = new PlayerInputActions();
        playerStats = GetComponent<StatHolder>();
        playerSprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        attackCircle = this.gameObject.transform.GetChild(0).GetComponent<Transform>();
        enemyLayer = LayerMask.GetMask("Enemies");
        originalStamina = playerStats.stats.stamina;
    }

    void Start()
    {
        
    }

    private void OnEnable()
    {
        // Sets references to action in the input manager
        move = playerAction.Player.Move;
        attack = playerAction.Player.Attack;
        interact = playerAction.Player.Interact;
        sprint = playerAction.Player.Sprint;

        // Subscribes the local functions to the events attached to the input system
        move.Enable();
        attack.Enable();
        attack.performed += Attacked;
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

        // Clamps speed back to the original when not sprinting
        if (!playerAction.Player.Sprint.IsPressed())
            moveSpeed = baseMoveSpeed;
        
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

    }

    // Draw hit box for testing
    void OnDrawGizmosSelected()
    {
        if (attackCircle != null)
            Gizmos.DrawWireSphere(attackCircle.position, attackRange);
    }

    #region Input Action Functions
    void Attacked(InputAction.CallbackContext context)
    {
        Debug.Log("ATTACK!");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCircle.position, attackRange, enemyLayer);

        foreach(Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit: " + enemy.name);
            enemy.GetComponent<StatHolder>().TakeDamage(playerStats.stats.attack);
        }
    }

    void Sprinted(InputAction.CallbackContext context)
    {
        if (playerStats.stats.stamina > 0)
        {
            Debug.Log("SPRINT!");
            moveSpeed = moveSpeed + 2.5f;
            
        }
    }

    void Interacted(InputAction.CallbackContext context)
    {
        Debug.Log("INTERACT!");
    }
    #endregion

}