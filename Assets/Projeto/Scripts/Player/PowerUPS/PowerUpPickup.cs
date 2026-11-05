using Unity.Netcode;
using UnityEngine;

public class PowerUpPickup : NetworkBehaviour
{
    [Header("Configuração")]
    [SerializeField]private PowerUpTipo[] tiposPossiveis =
    {
        PowerUpTipo.DoubleJump,
        PowerUpTipo.Dash,
        PowerUpTipo.Stun
    };
    [SerializeField] private float tempoRespawn = 15f;

    [Header("Aparência")]
    [SerializeField] private Material materialIndisponivel;

    private readonly NetworkVariable<bool> disponivel = new NetworkVariable<bool>(true);

    private float tempoParaReaparecer;
    private Collider gatilho;
    private Renderer[] renderers;
    private Material[][] materiaisDisponivel;
    private Material[][] materiaisIndisponivel;

    private void Awake()
    {
        gatilho = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>(true);

        materiaisDisponivel = new Material[renderers.Length][];
        materiaisIndisponivel = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] original = renderers[i].sharedMaterials;
            materiaisDisponivel[i] = original;

            if (materialIndisponivel != null)
            {
                Material[] fantasma = new Material[original.Length];
                for (int m = 0; m < fantasma.Length; m++)
                    fantasma[m] = materialIndisponivel;

                materiaisIndisponivel[i] = fantasma;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        disponivel.OnValueChanged += AoMudarDisponivel;
        AplicarEstado(disponivel.Value);
    }

    public override void OnNetworkDespawn()
    {
        disponivel.OnValueChanged -= AoMudarDisponivel;
    }

    private void AoMudarDisponivel(bool antigo, bool novo) => AplicarEstado(novo);

    private void AplicarEstado(bool ativo)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            if (ativo)
            {
                r.enabled = true;
                r.sharedMaterials = materiaisDisponivel[i];
            }
            else if (materiaisIndisponivel[i] != null)
            {
                r.sharedMaterials = materiaisIndisponivel[i];
            }
            else
            {
                r.enabled = false;
            }
        }

        if (gatilho != null)
            gatilho.enabled = IsServer && ativo;

        enabled = IsServer && !ativo;
    }

    private void Update()
    {
        if (Time.time >= tempoParaReaparecer)
            disponivel.Value = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer || !disponivel.Value || tiposPossiveis.Length == 0)
            return;

        PlayerPowerUps jogador = other.GetComponentInParent<PlayerPowerUps>();
        if (jogador == null)
            return;

        PowerUpTipo sorteado = tiposPossiveis[Random.Range(0, tiposPossiveis.Length)];

        if (jogador.ServidorTentarPegar(sorteado))
        {
            Debug.Log($"[POWERUP] {jogador.name} pegou {sorteado}");
            tempoParaReaparecer = Time.time + tempoRespawn;
            disponivel.Value = false;
        }
    }
}