using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private UIDocument document;

    private Slider Master;
    private Slider Musica;
    private Slider SFX;
    private Slider UI;

    void Start()
    {
        VisualElement root = document.rootVisualElement;

        Master = root.Q<Slider>("Vol_Master");
        Musica = root.Q<Slider>("Musica");
        SFX = root.Q<Slider>("SFX");
        UI = root.Q<Slider>("UI");

        Master.RegisterValueChangedCallback(evt =>
        {
            audioMixer.SetFloat("MasterVol", evt.newValue);
        });

        Musica.RegisterValueChangedCallback(evt =>
        {
            audioMixer.SetFloat("MusicVol", evt.newValue);
        });

        SFX.RegisterValueChangedCallback(evt =>
        {
            audioMixer.SetFloat("SFXVol", evt.newValue);
        });

        UI.RegisterValueChangedCallback(evt =>
        {
            audioMixer.SetFloat("UI/Menu", evt.newValue);
        });


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
