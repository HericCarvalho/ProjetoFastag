using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPowerUps : NetworkBehaviour
{
    [Header("Stun")]
    [SerializeField] private GameObject prefabProjetilStun;

    public NetworkVariable<int> slot1 = new NetworkVariable<int>((int)PowerUpTipo.Nenhum);
    public NetworkVariable<int> slot2 = new NetworkVariable<int>((int)PowerUpTipo.Nenhum);
    public NetworkVariable<bool> slot2Liberado = new NetworkVariable<bool>(false);

    private PlayerMovement movimento;
    private float bloqueioUsoLocal;

    private void Awake()
    {
        movimento = GetComponent<PlayerMovement>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        int rodada = LobbyManager.Singleton != null ? LobbyManager.Singleton.RodadaAtual : 1;
        slot2Liberado.Value = rodada >= 3;
    }

    public bool ServidorTentarPegar(PowerUpTipo tipo)
    {
        if (!IsServer)
            return false;

        if (slot1.Value == (int)PowerUpTipo.Nenhum)
        {
            slot1.Value = (int)tipo;
            return true;
        }

        if (slot2Liberado.Value && slot2.Value == (int)PowerUpTipo.Nenhum)
        {
            slot2.Value = (int)tipo;
            return true;
        }

        return false;
    }

    public bool TentarUsar(PowerUpTipo tipo)
    {
        if (!IsOwner || Time.time < bloqueioUsoLocal)
            return false;

        int slot;
        if (slot1.Value == (int)tipo) slot = 1;
        else if (slot2Liberado.Value && slot2.Value == (int)tipo) slot = 2;
        else return false;

        bloqueioUsoLocal = Time.time + 0.3f;
        ConsumirRpc(slot);
        return true;
    }

    private void Update()
    {
        if (!IsOwner || Mouse.current == null)
            return;

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            UsarPowerUpAtivo();
    }

    private static bool EhAtivo(int tipo)
    {
        return tipo == (int)PowerUpTipo.Dash || tipo == (int)PowerUpTipo.Stun;
    }

    private void UsarPowerUpAtivo()
    {
        if (movimento == null || movimento.foiCapturado.Value || movimento.atordoado.Value)
            return;

        if (Time.time < bloqueioUsoLocal)
            return;

        int slot;
        if (EhAtivo(slot1.Value)) slot = 1;
        else if (slot2Liberado.Value && EhAtivo(slot2.Value)) slot = 2;
        else return;

        PowerUpTipo tipo = (PowerUpTipo)(slot == 1 ? slot1.Value : slot2.Value);
        bloqueioUsoLocal = Time.time + 0.3f;

        switch (tipo)
        {
            case PowerUpTipo.Dash:
                movimento.IniciarDash();
                ConsumirRpc(slot);
                break;

            case PowerUpTipo.Stun:
                Transform cam = Camera.main != null ? Camera.main.transform : transform;
                Vector3 dir = cam.forward;
                Vector3 plano = new Vector3(dir.x, 0f, dir.z).normalized;
                Vector3 origem = transform.position + Vector3.up * 1.2f + plano * 0.8f;
                LancarStunRpc(slot, origem, dir);
                break;
        }
    }

    [Rpc(SendTo.Server)]
    private void ConsumirRpc(int slot)
    {
        if (slot == 1) slot1.Value = (int)PowerUpTipo.Nenhum;
        else if (slot == 2) slot2.Value = (int)PowerUpTipo.Nenhum;
    }

    [Rpc(SendTo.Server)]
    private void LancarStunRpc(int slot, Vector3 origem, Vector3 direcao)
    {
        if (slot == 2 && !slot2Liberado.Value)
            return;

        NetworkVariable<int> alvoSlot = slot == 1 ? slot1 : slot2;

        if (alvoSlot.Value != (int)PowerUpTipo.Stun || prefabProjetilStun == null)
            return;

        alvoSlot.Value = (int)PowerUpTipo.Nenhum;

        GameObject obj = Instantiate(prefabProjetilStun, origem, Quaternion.LookRotation(direcao));
        obj.GetComponent<ProjetilStun>().Configurar(direcao.normalized, NetworkObjectId);
        obj.GetComponent<NetworkObject>().Spawn();
    }
}