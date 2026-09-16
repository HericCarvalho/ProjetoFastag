using UnityEngine;
using PrimeTween;

public class Texto_Anim : MonoBehaviour
{
    private void OnEnable()
    {
        transform.localScale = Vector3.one;

        Tween.Scale(transform, Vector3.one * 1.15f, 0.6f, cycles: -1, cycleMode: CycleMode.Yoyo);
    }
}
