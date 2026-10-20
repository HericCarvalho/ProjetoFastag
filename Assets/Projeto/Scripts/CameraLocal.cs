using Unity.Cinemachine;
using UnityEngine;

public class CameraLocal : MonoBehaviour
{
    private CinemachineCamera cameraVirtual;

    private void Awake()
    {
        cameraVirtual = GetComponent<CinemachineCamera>();
    }

    private void Update()
    {
        // Se a câmara já estiver configurada a seguir um alvo, não precisa continuar a procurar
        if (cameraVirtual != null && cameraVirtual.Follow != null)
            return;

        // Procura por todos os PlayerMovement ativos na cena
        PlayerMovement[] jogadores = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var jogador in jogadores)
        {
            // Encontra apenas o jogador que pertence a esta máquina local
            if (jogador.IsOwner)
            {
                // Tenta usar o cameraTarget do player se existir; caso contrário, usa a raiz do transform
                Transform alvo = jogador.transform;

                // Configura corretamente o Cinemachine para seguir e olhar para o jogador local
                cameraVirtual.Follow = alvo;
                cameraVirtual.LookAt = alvo;

                // Força o Cinemachine a atualizar imediatamente o estado
                cameraVirtual.enabled = false;
                cameraVirtual.enabled = true;

                Debug.Log("Câmara do Cinemachine acoplada com sucesso ao jogador local!");
                break;
            }
        }
    }
}