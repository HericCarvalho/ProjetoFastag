
using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float velocidade = 5f;
    [SerializeField] private float gravidade = -20f;
    [SerializeField] private float velocidadeRotacao = 10f;
    [SerializeField] private float alturaPulo = 1.5f;
    [SerializeField] private float multiplicadorDoubleJump = 1.2f;

    [Header("Captura")]
    [SerializeField] private float distanciaInteracao = 3f;
    [SerializeField] private float distanciaCaptura = 2f;

    [Header("Câmera")]
    [SerializeField] private CinemachineCamera cameraDoPrefab;
    [SerializeField] private Transform cameraTarget;



    public NetworkVariable<bool> isPegador =
        new NetworkVariable<bool>(false);

    public NetworkVariable<bool> foiCapturado =
        new NetworkVariable<bool>(false);

    public static event Action<PlayerMovement> JogadorCapturadoNoServidor;

    private CharacterController controlador;
    private InputSystem_Actions controles;
    private Animator animator;
    private float velocidadeVertical;

    private PlayerPowerUps powerUps;


    private float tempoUltimoPulo;

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();
        controles = new InputSystem_Actions();
        animator = GetComponentInChildren<Animator>();
        powerUps = GetComponent<PlayerPowerUps>();
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
        if (IsOwner && controles != null)
        {
            controles.Player.Jump.performed -= Pular;
            controles.Player.Interact.performed -= Interagir;
            controles.Disable();
        }
    }


    private void Update()
    {
        if (!IsOwner || foiCapturado.Value)
            return;

        Movimentar();
        AtualizarAnimacao();
    }

    private void Pular(InputAction.CallbackContext context)
    {
        if (foiCapturado.Value)
            return;

        if (controlador.isGrounded)
        {
            velocidadeVertical = Mathf.Sqrt(alturaPulo * -2f * gravidade);
            tempoUltimoPulo = Time.time;

            if (animator != null)
                animator.SetTrigger("Pular");
            return;
        }

        if (Time.time - tempoUltimoPulo < 0.2f)
            return;

        if (powerUps != null && powerUps.TentarUsar(PowerUpTipo.DoubleJump))
        {
            velocidadeVertical = Mathf.Sqrt(alturaPulo * multiplicadorDoubleJump * -2f * gravidade);
            Debug.Log("[DOUBLEJUMP] usado");

            if (animator != null)
                animator.SetTrigger("Pular");
        }
    }

    private void Interagir(InputAction.CallbackContext context)
    {
        if (!isPegador.Value || foiCapturado.Value)
            return;

        Transform camTransform = ObterTransformCamera();
        if (camTransform == null)
            return;

        RaycastHit[] hits = Physics.RaycastAll(camTransform.position, camTransform.forward, distanciaInteracao);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        PlayerMovement alvo = null;

        foreach (var hit in hits)
        {
            PlayerMovement potentialTarget = hit.collider.GetComponentInParent<PlayerMovement>();

            if (potentialTarget != null)
            {
                if (potentialTarget == this || potentialTarget.isPegador.Value)
                {
                    continue;
                }

                alvo = potentialTarget;
                break;
            }
            else
            {
                break;
            }
        }

        if (alvo == null)
        {
            Debug.Log("[INTERAGIR] Nenhuma vítima válida no raio de visão (ou bloqueado por obstáculo).");
            return;
        }

        if (alvo.foiCapturado.Value)
        {
            Debug.Log("[INTERAGIR] O alvo já foi capturado anteriormente.");
            return;
        }

        float distancia = Vector3.Distance(transform.position, alvo.transform.position);
        if (distancia > distanciaCaptura)
        {
            Debug.Log($"[INTERAGIR] Alvo fora do alcance físico de captura. Distância: {distancia:F2}m");
            return;
        }

        Debug.Log($"[INTERAGIR] SUCESSO! A solicitar captura para a vítima ID: {alvo.NetworkObjectId}");
        SolicitarCapturaServerRpc(alvo.NetworkObjectId);
    }

    [ServerRpc]
    private void SolicitarCapturaServerRpc(ulong idObjetoAlvo)
    {
        if (!isPegador.Value || foiCapturado.Value)
            return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(idObjetoAlvo, out NetworkObject objetoAlvo))
        {
            return;
        }

        PlayerMovement alvo = objetoAlvo.GetComponent<PlayerMovement>();

        if (alvo == null ||
            alvo == this ||
            alvo.isPegador.Value ||
            alvo.foiCapturado.Value)
        {
            return;
        }

        float distancia = Vector3.Distance(
            transform.position,
            alvo.transform.position);

        if (distancia > distanciaCaptura)
            return;

        alvo.Morrer();
    }

    public void Morrer()
    {
        if (!IsServer || foiCapturado.Value)
            return;

        foiCapturado.Value = true;

        JogadorCapturadoNoServidor?.Invoke(this);

        MorrerClientRpc();
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void MorrerClientRpc()
    {
        if (animator != null)
            animator.SetTrigger("Morrer");

        if (controlador != null)
            controlador.enabled = false;

        if (IsServer)
            StartCoroutine(RemoverAposMorte());
    }

    private IEnumerator RemoverAposMorte()
    {
        yield return new WaitForSeconds(0.5f);

        if (IsServer && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
    [Rpc(SendTo.Owner)]
    public void DefinirPosicaoInicialRpc(Vector3 posicao, Quaternion rotacao)
    {
        if (controlador != null)
            controlador.enabled = false;

        transform.SetPositionAndRotation(posicao, rotacao);

        if (controlador != null)
            controlador.enabled = true;

        velocidadeVertical = 0f;
    }

    private Transform ObterTransformCamera()
    {
        if (Camera.main != null)
            return Camera.main.transform;

        if (cameraTarget != null)
            return cameraTarget;

        return transform;
    }

    private void Movimentar()
    {
        Vector2 entrada = controles.Player.Move.ReadValue<Vector2>();
        Transform cameraTransform = ObterTransformCamera();

        Vector3 frente = cameraTransform.forward;
        Vector3 direita = cameraTransform.right;

        frente.y = 0f;
        direita.y = 0f;

        frente.Normalize();
        direita.Normalize();

        Vector3 movimento = frente * entrada.y + direita * entrada.x;

        if (movimento.sqrMagnitude > 0.01f)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(movimento);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacaoAlvo,
                velocidadeRotacao * Time.deltaTime);
        }

        movimento *= velocidade;
        AplicarGravidade(ref movimento);

        controlador.Move(movimento * Time.deltaTime);
    }

    private void AtualizarAnimacao()
    {
        if (animator == null)
            return;

        Vector3 velocidadeHorizontal = controlador.velocity;
        velocidadeHorizontal.y = 0f;

        animator.SetFloat("Speed", velocidadeHorizontal.magnitude);
    }

    private void AplicarGravidade(ref Vector3 movimento)
    {
        if (controlador.isGrounded && velocidadeVertical < 0f)
            velocidadeVertical = -2f;

        velocidadeVertical += gravidade * Time.deltaTime;
        movimento.y = velocidadeVertical;
    }
}
