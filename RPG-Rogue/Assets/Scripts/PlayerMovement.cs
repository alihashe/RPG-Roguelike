using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb; // Reference to player rigidbody
    Transform attackCircle; // Empty gameobject used to create hitbox
    LayerMask enemyLayer;
    [SerializeField] float attackRange = 0.5f; // Attack hitbox size
    [SerializeField] float moveSpeed = 5.0f; // Dynamic movespeed
    [SerializeField] float baseMoveSpeed = 5.0f; // Base move speed; Should never change while in game
    PlayerInputActions playerAction;
    StatHolder playerStats;

    Vector2 moveDirection = Vector2.zero;
    InputAction move;
    InputAction attack;
    InputAction interact;
    InputAction sprint;

    private void Awake()
    {
        playerAction = new PlayerInputActions();
        playerStats = GetComponent<StatHolder>();
        rb = GetComponent<Rigidbody2D>();
        attackCircle = this.gameObject.transform.GetChild(0).GetComponent<Transform>();
        enemyLayer = LayerMask.GetMask("Enemies");
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

        // HEEEEEEEEY This is fine for testing but will need fixing once the player sprite is introduced
        #region Player Rotation
        if (Input.GetKey(KeyCode.RightArrow))
            rb.MoveRotation(0f);
        if (Input.GetKey(KeyCode.LeftArrow))
            rb.MoveRotation(180f);
        if (Input.GetKey(KeyCode.UpArrow))
            rb.MoveRotation(90f);
        if (Input.GetKey(KeyCode.DownArrow))
            rb.MoveRotation(-90f);
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
        Debug.Log("SPRINT!");
        moveSpeed = moveSpeed + 2.5f;
    }

    void Interacted(InputAction.CallbackContext context)
    {
        Debug.Log("INTERACT!");
    }
    #endregion

}