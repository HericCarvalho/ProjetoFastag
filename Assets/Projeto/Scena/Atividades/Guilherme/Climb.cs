
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

public class Climb : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private GameData gameData = new GameData();

    [Serializable]
    public class GameData : INotifyPropertyChanged
    {
        private int _moedas;
        private int _metros;
        private float _stamina = 100f;

        public event PropertyChangedEventHandler PropertyChanged;

        [CreateProperty]
        public int moedas
        {
            get => _moedas;
            set
            {
                if (_moedas == value)
                    return;

                _moedas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(moedasBIND));
            }
        }

        [CreateProperty]
        public int metros
        {
            get => _metros;
            set
            {
                if (_metros == value)
                    return;

                _metros = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(metrosBIND));
            }
        }

        [CreateProperty]
        public float stamina
        {
            get => _stamina;
            set
            {
                if (Mathf.Approximately(_stamina, value))
                    return;

                _stamina = value;
                OnPropertyChanged();
            }
        }

        [CreateProperty]
        public string moedasBIND => "MOEDAS: " + _moedas;

        [CreateProperty]
        public string metrosBIND => "METROS: " + _metros;

        private void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }

    private VisualElement painelPrincipal;
    private VisualElement climbScreen;
    private VisualElement optionsScreen;
    private VisualElement creditsScreen;

    private ProgressBar staminaBar;
    private Slider volumeSlider;

    private float lastClickTime;
    private int combo = 1;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
            return;

        var root = uiDocument.rootVisualElement;
        root.dataSource = gameData;

        painelPrincipal = root.Q<VisualElement>("Painel_Principal");
        climbScreen = root.Q<VisualElement>("UI_Climb");
        optionsScreen = root.Q<VisualElement>("UI_Options");
        creditsScreen = root.Q<VisualElement>("UI_Credits");

        var moedasLabel = root.Q<Label>("Moedas");
        var metrosLabel = root.Q<Label>("Metros");

        staminaBar = climbScreen?.Q<ProgressBar>("stamina");

        if (moedasLabel != null)
        {
            moedasLabel.SetBinding("text", new DataBinding
            {
                dataSource = gameData,
                dataSourcePath = new PropertyPath("moedasBIND"),
                bindingMode = BindingMode.ToTarget
            });
        }

        if (metrosLabel != null)
        {
            metrosLabel.SetBinding("text", new DataBinding
            {
                dataSource = gameData,
                dataSourcePath = new PropertyPath("metrosBIND"),
                bindingMode = BindingMode.ToTarget
            });
        }

        if (staminaBar != null)
        {
            staminaBar.SetBinding("value", new DataBinding
            {
                dataSource = gameData,
                dataSourcePath = new PropertyPath("stamina"),
                bindingMode = BindingMode.ToTarget
            });
        }

        var btnClimb = root.Q<Button>("Climb");
        var btnOptions = root.Q<Button>("Options");
        var btnCredits = root.Q<Button>("Credits");
        var btnExit = root.Q<Button>("Exit");

        volumeSlider = optionsScreen?.Q<Slider>("Music");

        var btnSairOptions = optionsScreen?.Q<Button>("Sair");
        var btnSairClimb = climbScreen?.Q<Button>("Sair");
        var btnSairCredits = creditsScreen?.Q<Button>("Sair");

        if (btnClimb != null)
            btnClimb.clicked += () => ShowScreen(climbScreen);

        if (btnOptions != null)
            btnOptions.clicked += () => ShowScreen(optionsScreen);

        if (btnCredits != null)
            btnCredits.clicked += () => ShowScreen(creditsScreen);

        if (btnExit != null)
            btnExit.clicked += () => Application.Quit();

        if (btnSairOptions != null)
            btnSairOptions.clicked += () => ShowScreen(painelPrincipal);

        if (btnSairClimb != null)
            btnSairClimb.clicked += () => ShowScreen(painelPrincipal);

        if (btnSairCredits != null)
            btnSairCredits.clicked += () => ShowScreen(painelPrincipal);

        var btnCharacterClimb = climbScreen?.Q<Button>("CLIMB");

        if (btnCharacterClimb != null)
            btnCharacterClimb.clicked += ClimbCharacter;

        if (backgroundMusic != null && volumeSlider != null)
        {
            volumeSlider.value = backgroundMusic.volume * 100f;

            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                backgroundMusic.volume = evt.newValue / 100f;
            });
        }
    }

    private void ClimbCharacter()
    {
        if (gameData.stamina < 15f)
            return;

        combo = Time.time - lastClickTime < 0.8f
            ? Mathf.Min(combo + 1, 5)
            : 1;

        lastClickTime = Time.time;

        int oldMilestone = gameData.metros / 10;

        gameData.metros += combo;

        int newMilestone = gameData.metros / 10;

        if (newMilestone > oldMilestone)
            gameData.moedas += newMilestone - oldMilestone;

        gameData.stamina = Mathf.Max(0f, gameData.stamina - 15f);
    }

    private void Update()
    {
        if (gameData.stamina < 100f)
        {
            gameData.stamina = Mathf.Min(
                100f,
                gameData.stamina + Time.deltaTime * 30f);
        }
    }

    private void ShowScreen(VisualElement targetScreen)
    {
        if (painelPrincipal != null)
            painelPrincipal.style.display = DisplayStyle.None;

        if (climbScreen != null)
            climbScreen.style.display = DisplayStyle.None;

        if (optionsScreen != null)
            optionsScreen.style.display = DisplayStyle.None;

        if (creditsScreen != null)
            creditsScreen.style.display = DisplayStyle.None;

        if (targetScreen != null)
            targetScreen.style.display = DisplayStyle.Flex;
    }
}
