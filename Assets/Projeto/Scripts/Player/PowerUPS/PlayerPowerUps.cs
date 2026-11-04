using Unity.Netcode;
using UnityEngine;

public class PlayerPowerUps : NetworkBehaviour
{
    public NetworkVariable<int> slot1 = new NetworkVariable<int>((int)PowerUpTipo.Nenhum);
    public NetworkVariable<int> slot2 = new NetworkVariable<int>((int)PowerUpTipo.Nenhum);
    public NetworkVariable<bool> slot2Liberado = new NetworkVariable<bool>(false);

    private float bloqueioUsoLocal;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        // O spawn acontece depois de rodadaAtual ser atualizada, então já vale para esta rodada
        int rodada = LobbyManager.Singleton != null ? LobbyManager.Singleton.RodadaAtual : 1;
        slot2Liberado.Value = rodada >= 3;
    }

    // ---------- SERVIDOR: pegar ----------
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

    // ---------- DONO: usar ----------
    public bool TentarUsar(PowerUpTipo tipo)
    {
        if (!IsOwner || Time.time < bloqueioUsoLocal)
            return false;

        int slot;
        if (slot1.Value == (int)tipo) slot = 1;
        else if (slot2Liberado.Value && slot2.Value == (int)tipo) slot = 2;
        else return false;

        // Evita usar duas vezes antes do servidor confirmar o consumo
        bloqueioUsoLocal = Time.time + 0.3f;

        ConsumirRpc(slot);
        return true;
    }

    [Rpc(SendTo.Server)]
    private void ConsumirRpc(int slot)
    {
        if (slot == 1) slot1.Value = (int)PowerUpTipo.Nenhum;
        else if (slot == 2) slot2.Value = (int)PowerUpTipo.Nenhum;
    }
}