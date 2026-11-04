using Unity.Netcode;
using UnityEngine;

public class SpawnDebug : MonoBehaviour
{
    private float proximo;

    private void Update()
    {
        if (Time.time < proximo || NetworkManager.Singleton == null) return;
        proximo = Time.time + 2f;

        string cena = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"[SPAWNDBG] cena={cena} | client={NetworkManager.Singleton.IsClient} | " +
                  $"spawnados={NetworkManager.Singleton.SpawnManager.SpawnedObjects.Count}");

        foreach (var kv in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
            Debug.Log($"[SPAWNDBG] id={kv.Key} nome={kv.Value.name} dono={kv.Value.OwnerClientId}");
    }
}