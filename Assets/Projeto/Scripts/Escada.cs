using UnityEngine;

public class Escada : MonoBehaviour
{
    [SerializeField] private Transform pontoTopo;
    [SerializeField] private Transform pontoBase;

    public Transform PontoTopo => pontoTopo;
    public Transform PontoBase => pontoBase;
}