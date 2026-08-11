using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private InputSystem_Actions controls;

    private Vector2 moveInput;

    private Vector3 moveDirection;

    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new InputSystem_Actions();
    }
    private void Update()
    {
        if (!IsOwner)
            return;

        HandleMovement();
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        controls.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;

        controls.Disable();
    }
    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

    }
    private void HandleMovement()
    {
        ReadCameraDirection();
        CalculateMovement();
        ApplyGravity();
        MoveCharacter();
    }
    void ReadCameraDirection()
    {

    }

    void CalculateMovement()
    {

    }

    void ApplyGravity()
    {

    }

    void MoveCharacter()
    {

    }
}