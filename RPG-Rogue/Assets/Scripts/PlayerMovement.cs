using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float moveSpeed = 5.0f; // Dynamic movespeed
    [SerializeField] float baseMoveSpeed = 5.0f; // Base move speed; Should never change while in game
    PlayerInputActions playerAction;

    Vector2 moveDirection = Vector2.zero;
    InputAction move;
    InputAction attack;
    InputAction interact;
    InputAction sprint;

    private void Awake()
    {
        playerAction = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
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
    }

    void Attacked(InputAction.CallbackContext context)
    {
        Debug.Log("ATTACK!");
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
}