
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

    private void Start()
    {
        VisualElement root = document.rootVisualElement;

        Master = root.Q<Slider>("Vol_Master");
        Musica = root.Q<Slider>("Musica");
        SFX = root.Q<Slider>("SFX");
        UI = root.Q<Slider>("UI");

        
        float masterVol = PlayerPrefs.GetFloat("MasterVol", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 1f);
        float uiVol = PlayerPrefs.GetFloat("UIVol", 1f);

      
        Master.value = masterVol;
        Musica.value = musicVol;
        SFX.value = sfxVol;
        UI.value = uiVol;

       
        SetVolume("MasterVol", masterVol);
        SetVolume("MusicVol", musicVol);
        SetVolume("SFXVol", sfxVol);
        SetVolume("UIVol", uiVol);

        
        Master.RegisterValueChangedCallback(evt =>
        {
            SetVolume("MasterVol", evt.newValue);
        });

        Musica.RegisterValueChangedCallback(evt =>
        {
            SetVolume("MusicVol", evt.newValue);
        });

        SFX.RegisterValueChangedCallback(evt =>
        {
            SetVolume("SFXVol", evt.newValue);
        });

        UI.RegisterValueChangedCallback(evt =>
        {
            SetVolume("UIVol", evt.newValue);
        });
    }

    private void SetVolume(string parameter, float value)
    {
        
        PlayerPrefs.SetFloat(parameter, value);
        PlayerPrefs.Save();

       
        float volumeDB = value > 0.0001f
            ? Mathf.Log10(value) * 20f
            : -80f;

        audioMixer.SetFloat(parameter, volumeDB);
    }
}
