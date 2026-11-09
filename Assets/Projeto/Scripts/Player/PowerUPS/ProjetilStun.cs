using Unity.Netcode;
using UnityEngine;

public class ProjetilStun : NetworkBehaviour
{
    [SerializeField] private float velocidade = 20f;
    [SerializeField] private float duracaoStun = 1.5f;
    [SerializeField] private float tempoDeVida = 3f;

    private Vector3 direcao;
    private ulong idDono;
    private float tempoMorte;

    public void Configurar(Vector3 dir, ulong idObjetoDono)
    {
        direcao = dir;
        idDono = idObjetoDono;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }

        tempoMorte = Time.time + tempoDeVida;
    }

    private void Update()
    {
        transform.position += direcao * (velocidade * Time.deltaTime);

        if (Time.time >= tempoMorte)
            Destruir();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || other.isTrigger) 
            return;

        PlayerMovement alvo = other.GetComponentInParent<PlayerMovement>();

        if (alvo != null)
        {
            if (alvo.NetworkObjectId == idDono)
                return;

            alvo.ServidorAtordoar(duracaoStun);
        }

        Destruir();
    }

    private void Destruir()
    {
        if (IsSpawned)
            NetworkObject.Despawn();
    }
}