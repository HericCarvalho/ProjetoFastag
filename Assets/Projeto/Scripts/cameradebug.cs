using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class cameradebug : MonoBehaviour
{
    private float proximoLog;

    private void Update()
    {
        if (Time.time < proximoLog) return;
        proximoLog = Time.time + 2f;

        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[DEBUG] Camera.main é NULL (a Main Camera está sem a tag 'MainCamera'?)");
            return;
        }

        var brain = cam.GetComponent<CinemachineBrain>();
        Debug.Log($"[DEBUG] Main Camera: {cam.name} | Brain: {(brain != null)} | " +
                  $"Vcam ativa: {(brain != null && brain.ActiveVirtualCamera != null ? brain.ActiveVirtualCamera.Name : "NENHUMA")}");

        foreach (var v in FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Debug.Log($"[DEBUG] Vcam '{v.name}' ativa={v.gameObject.activeInHierarchy} " +
                      $"prio={v.Priority.Value} follow={(v.Follow != null ? v.Follow.name : "null")}");
        }

        foreach (var p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            Debug.Log($"[DEBUG] Player '{p.name}' IsOwner={p.IsOwner} pos={p.transform.position}");
        }
    }
}