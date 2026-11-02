using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Pontos de Spawn nesta Cena")]
    [SerializeField] private Transform[] pontosSpawnPegadores;
    [SerializeField] private Transform[] pontosSpawnVitimas;

    private void Awake()
    {
        Instance = this;
    }

    public Transform[] GetSpawnsPegadores()
    {
        return pontosSpawnPegadores;
    }

    public Transform[] GetSpawnsVitimas()
    {
        return pontosSpawnVitimas;
    }
}