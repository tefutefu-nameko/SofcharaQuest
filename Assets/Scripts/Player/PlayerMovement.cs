using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Movement
    [HideInInspector]
    public Vector2 moveDir;
    [HideInInspector]
    public Vector2 mouseDir; // Vector2 for the direction of the mouse
    [HideInInspector]
    public float lastHorizontalVector;
    [HideInInspector]
    public float lastVerticalVector;
    [HideInInspector]
    public Vector2 lastMovedVector;

    //References
    Rigidbody2D rb;
    PlayerStats player;
    GameStateService gameStateService;

    public bool isMovementLocked = false;
    public void SetMovementLock(bool locked)
    {
        isMovementLocked = locked;
    }

    void Start()
    {
        player = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        gameStateService = FindObjectOfType<GameStateService>();
        lastMovedVector = new Vector2(1, 0f); //If we don't do this and game starts up and don't move, the projectile weapon will have no momentum
        
        // Remove Friction from Player so they slide along walls
        PhysicsMaterial2D slipperyMaterial = new PhysicsMaterial2D("PlayerSlippery");
        slipperyMaterial.friction = 0f;
        slipperyMaterial.bounciness = 0f;
        rb.sharedMaterial = slipperyMaterial;

        // Also apply to any attached Colliders (like CircleCollider2D)
        Collider2D[] cols = GetComponents<Collider2D>();
        foreach(var c in cols) c.sharedMaterial = slipperyMaterial;
    }

    void Update()
    {
        InputManagement();
    }

    void FixedUpdate()
    {
        Move();
    }

    void InputManagement()
    {
        if (isMovementLocked)
        {
            moveDir = Vector2.zero;
            return;
        }

        // Use service if available for robust check
        if (gameStateService != null && (gameStateService.IsGameOver || gameStateService.CurrentState != GameState.Gameplay))
        {
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(moveX, moveY).normalized;

        // Calculate Mouse Direction
        Vector3 mousePosition = Input.mousePosition;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
        Vector2 offset = new Vector2(mousePosition.x - screenPoint.x, mousePosition.y - screenPoint.y);
        mouseDir = offset.normalized;

        if (moveDir.x != 0)
        {
            lastHorizontalVector = moveDir.x;
            lastMovedVector = new Vector2(lastHorizontalVector, 0f);    //Last moved X
        }

        if (moveDir.y != 0)
        {
            lastVerticalVector = moveDir.y;
            lastMovedVector = new Vector2(0f, lastVerticalVector);  //Last moved Y
        }

        if (moveDir.x != 0 && moveDir.y != 0)
        {
            lastMovedVector = new Vector2(lastHorizontalVector, lastVerticalVector);    //While moving
        }
    }

    void Move()
    {
        if (isMovementLocked)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (gameStateService != null && (gameStateService.IsGameOver || gameStateService.CurrentState != GameState.Gameplay))
        {
            return;
        }

        rb.velocity = new Vector2(moveDir.x * player.CurrentMoveSpeed, moveDir.y * player.CurrentMoveSpeed);
    }
}
