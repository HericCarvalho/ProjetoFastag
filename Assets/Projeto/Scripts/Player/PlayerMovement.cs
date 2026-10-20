using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float velocidade = 5f;
    [SerializeField] private float gravidade = -20f;
    [SerializeField] private float velocidaderotação = 10f;
    [SerializeField] private float alturapulo = 1.5f;

    [Header("Interação")]
    [SerializeField] private float distanciaInteracao = 3f;

    [SerializeField] private Transform cameraTarget;
    private CharacterController controlador;
    private InputSystem_Actions controles;
    private Animator animator;

    private float verticalVelocity;

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();
        controles = new InputSystem_Actions();
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        controles.Enable();
        controles.Player.Jump.performed += Pular;
        controles.Player.Interact.performed += Interagir;

    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        controles.Player.Jump.performed -= Pular;
        controles.Player.Interact.performed -= Interagir;
        controles.Disable();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Movimentação();
        AtualizaçãoAnimação();
    }

    private void Pular(InputAction.CallbackContext context)
    {
        if (controlador.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(alturapulo * -2f * gravidade);

            if (animator != null)
            {
                animator.SetTrigger("Pular");
            }
        }
    }

    private void Interagir(InputAction.CallbackContext context)
    {
        if (animator != null)
        {
            animator.SetTrigger("Interagir");
        }

        Transform camTransform = Camera.main != null ? Camera.main.transform : (cameraTarget != null ? cameraTarget : transform);

        if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, distanciaInteracao))
        {
            NetworkObject objRede = hit.collider.GetComponentInParent<NetworkObject>();

            if (objRede != null && objRede.NetworkObjectId != NetworkObjectId)
            {
                SolicitarInteracaoServerRpc(objRede.NetworkObjectId);
            }
            else
            {
                Debug.Log($"Interagiu com o cenário: {hit.collider.name}");
            }
        }
    }

    [ServerRpc]
    private void SolicitarInteracaoServerRpc(ulong idObjetoAlvo)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(idObjetoAlvo, out NetworkObject objetoAlvo))
        {
            Debug.Log($"[Servidor] Jogador {OwnerClientId} interagiu com o objeto/jogador ID: {idObjetoAlvo}");
        }
    }

    private void Movimentação()
    {
        Vector2 moveInput = controles.Player.Move.ReadValue<Vector2>();

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
                velocidaderotação * Time.deltaTime
            );
        }

        movement *= velocidade;
        AplicarGravidade(ref movement);

        controlador.Move(movement * Time.deltaTime);
    }

    private void AtualizaçãoAnimação()
    {
        if (animator == null) return;

        float speed = new Vector3(controlador.velocity.x, 0f, controlador.velocity.z).magnitude;
        animator.SetFloat("Speed", speed);
    }

    private void AplicarGravidade(ref Vector3 movement)
    {
        if (controlador.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravidade * Time.deltaTime;
        movement.y = verticalVelocity;
    }
}