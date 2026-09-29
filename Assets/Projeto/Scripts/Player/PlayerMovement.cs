using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private Transform cameraTarget;
    private CharacterController controller;
    private InputSystem_Actions controls;
    private Animator animator;

    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new InputSystem_Actions();
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Player Spawned | Owner: {OwnerClientId} | LocalClient: {NetworkManager.Singleton.LocalClientId} | IsOwner: {IsOwner}");

        if (!IsOwner)
            return;

        controls.Enable();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        controls.Disable();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleMovement();
        UpdateAnimation();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();

        Transform camTransform = Camera.main != null ? Camera.main.transform : (cameraTarget != null ? cameraTarget : transform);

        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = forward * moveInput.y + right * moveInput.x;

        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        movement *= moveSpeed;
        ApplyGravity(ref movement);

        controller.Move(movement * Time.deltaTime);
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        float speed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
        animator.SetFloat("Speed", speed);
    }

    private void ApplyGravity(ref Vector3 movement)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;
    }
}