using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private string relayJoinCodeKey = "RelayJoinCode";

    private async void Start()
    {
        // 1. Inicializa os serviços da Unity e faz login anônimo
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    // Chamado pelo botão de "Criar Sala" na UI
    public async void CriarLobby()
    {
        try
        {
            int maxJogadores = 4;

            // 2. Cria a alocação no Unity Relay
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxJogadores - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // CORREÇÃO: Usando AllocationUtils para converter os dados para o formato correto exigido pelo UnityTransport
            var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // 3. CORREÇÃO: A propriedade correta da API é "Data", e não "DataObject"
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { relayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            };

            // 4. Cria o Lobby na nuvem
            hostLobby = await LobbyService.Instance.CreateLobbyAsync("Minha Sala 3D", maxJogadores, options);

            // Envia batimentos cardíacos para manter o lobby ativo na lista pública
            InvokeRepeating(nameof(KeepLobbyAlive), 15f, 15f);

            // 5. Inicia o Netcode como Host
            NetworkManager.Singleton.StartHost();
            Debug.Log($"Lobby Criado! Código Relay: {joinCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    // Chamado pelo botão de "Entrar na Sala" (passando o ID do lobby encontrado na busca)
    public async void EntrarNoLobby(string lobbyId)
    {
        try
        {
            // 1. Entra no Lobby da Unity
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            // 2. Recupera o código do Relay salvo dentro do Lobby
            string relayJoinCode = lobby.Data[relayJoinCodeKey].Value;

            // 3. Junta-se à alocação do Relay
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            // CORREÇÃO: Usando AllocationUtils para converter a entrada do cliente também
            var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // 4. Inicia o Netcode como Cliente
            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void KeepLobbyAlive()
    {
        if (hostLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }
}
