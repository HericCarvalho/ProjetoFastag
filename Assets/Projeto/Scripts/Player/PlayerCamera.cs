using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera cinemachine;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float sensitivity = 0.1f;

    private InputSystem_Actions controls;
    private float yaw;
    private float pitch;

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    public override void OnNetworkSpawn() => AplicarCamera();
    public override void OnGainedOwnership() => AplicarCamera();
    public override void OnLostOwnership() => AplicarCamera();

    private void AplicarCamera()
    {
        Debug.Log($"[CAMERA-{(IsServer ? "HOST" : "CLIENT")}] {name} | Owner={OwnerClientId} | " +
                  $"Local={NetworkManager.Singleton.LocalClientId} | IsOwner={IsOwner} | " +
                  $"vcam={(cinemachine != null)} | target={(cameraTarget != null)}", this);

        if (cinemachine == null || cameraTarget == null)
            return;

        if (IsOwner)
        {
            cinemachine.gameObject.SetActive(true);
            cinemachine.Follow = cameraTarget;
            cinemachine.LookAt = cameraTarget;
            cinemachine.Priority = 20;

            yaw = transform.eulerAngles.y;
            controls.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            cinemachine.Priority = 0;
            cinemachine.gameObject.SetActive(false);
            controls.Disable();
        }
    }

    private void Update()
    {
        if (!IsOwner || cameraTarget == null)
            return;

        cameraTarget.position = transform.position + new Vector3(0f, 0f, 0f);

        Vector2 mouse = controls.Player.Look.ReadValue<Vector2>();
        yaw += mouse.x * sensitivity;
        pitch = Mathf.Clamp(pitch - mouse.y * sensitivity, -40f, 70f);
        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public override void OnNetworkDespawn()
    {
        controls?.Disable();
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}