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
        if (cameraVirtual != null && cameraVirtual.Follow != null)
            return;

        PlayerMovement[] jogadores = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var jogador in jogadores)
        {
            if (jogador.IsOwner)
            {
                Transform alvo = jogador.transform;

                cameraVirtual.Follow = alvo;
                cameraVirtual.LookAt = alvo;

                cameraVirtual.enabled = false;
                cameraVirtual.enabled = true;

                Debug.Log("Câmara do Cinemachine acoplada com sucesso ao jogador local!");
                break;
            }
        }
    }
}