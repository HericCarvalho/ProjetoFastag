using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float sensitivity = 0.1f;

    private InputSystem_Actions controls;
    private CinemachineCamera cinemachine;

    private float yaw;
    private float pitch;

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        cinemachine = FindFirstObjectByType<CinemachineCamera>();

        cinemachine.Follow = cameraTarget;
        cinemachine.LookAt = cameraTarget;

        controls.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        RotateCamera();
    }

    private void RotateCamera()
    {
        Vector2 mouse = controls.Player.Look.ReadValue<Vector2>();

        yaw += mouse.x * sensitivity;
        pitch -= mouse.y * sensitivity;

        pitch = Mathf.Clamp(pitch, -40f, 70f);

        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        controls.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
