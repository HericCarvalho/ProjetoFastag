
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GraficoConfig : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private AudioSettings audioSettings;

    [Header("Configurações Iniciais")]
    [SerializeField] private int defaultWidth = 1920;
    [SerializeField] private int defaultHeight = 1080;
    [SerializeField] private bool defaultFullScreen = true;

    private List<Resolution> avaliableResolutions = new();

    private DropdownField resolutionDrop;
    private DropdownField modoDrop;

    private Button aceita;
    private Button recusa;

    private const string WidthKey = "ResolutionWidth";
    private const string HeightKey = "ResolutionHeight";
    private const string ModeKey = "ScreenMode";

    void Start()
    {
        VisualElement root = document.rootVisualElement;

        resolutionDrop = root.Q<DropdownField>("resolution_tela");
        modoDrop = root.Q<DropdownField>("TelaModo");

        aceita = root.Q<Button>("botao_aceita");
        recusa = root.Q<Button>("botao_recusa");

        if (resolutionDrop == null ||
            modoDrop == null ||
            aceita == null ||
            recusa == null)
        {
            Debug.LogError("Elementos não encontrados");
            return;
        }

        CarregarResolutions();

        modoDrop.choices = new List<string>()
        {
            "Janela",
            "Tela Cheia"
        };

        // CORREÇÃO: utiliza o método sem parâmetros
        aceita.clicked += AplicarConfig;
        recusa.clicked += RestaurarConfigurations;

        CarregarConfigurations();
    }

    void CarregarResolutions()
    {
        resolutionDrop.choices = new List<string>();
        avaliableResolutions.Clear();

        foreach (Resolution resolution in Screen.resolutions)
        {
            bool existe = avaliableResolutions.Exists(r =>
                r.width == resolution.width &&
                r.height == resolution.height);

            if (existe)
                continue;

            avaliableResolutions.Add(resolution);

            resolutionDrop.choices.Add(
                $"{resolution.width} x {resolution.height}"
            );
        }

        if (avaliableResolutions.Count == 0)
        {
            Resolution current = Screen.currentResolution;

            avaliableResolutions.Add(current);

            resolutionDrop.choices.Add(
                $"{current.width} x {current.height}"
            );
        }
    }

    void CarregarConfigurations()
    {
        int width = PlayerPrefs.GetInt(WidthKey, defaultWidth);
        int height = PlayerPrefs.GetInt(HeightKey, defaultHeight);

        int mode = PlayerPrefs.GetInt(
            ModeKey,
            defaultFullScreen ? 1 : 0
        );

        resolutionDrop.index = EncontrarResolutions(width, height);

        modoDrop.index = Mathf.Clamp(mode, 0, 1);

        AplicarConfigurations(false);
    }

    int EncontrarResolutions(int width, int height)
    {
        int melhorIndice = 0;
        int menorDiff = int.MaxValue;

        for (int i = 0; i < avaliableResolutions.Count; i++)
        {
            Resolution resolution = avaliableResolutions[i];

            int diferenca =
                Mathf.Abs(resolution.width - width) +
                Mathf.Abs(resolution.height - height);

            if (diferenca < menorDiff)
            {
                menorDiff = diferenca;
                melhorIndice = i;
            }

            if (diferenca == 0)
            {
                break;
            }
        }

        // CORREÇÃO: retorno fora do FOR
        return melhorIndice;
    }

    void AplicarConfig()
    {
        AplicarConfigurations(true);
    }

    void AplicarConfigurations(bool salvar)
    {
        if (avaliableResolutions.Count == 0)
            return;

        int resolutionIndex = Mathf.Clamp(
            resolutionDrop.index,
            0,
            avaliableResolutions.Count - 1
        );

        Resolution selectedResolution =
            avaliableResolutions[resolutionIndex];

        FullScreenMode screenMode = modoDrop.index == 0
            ? FullScreenMode.Windowed
            : FullScreenMode.FullScreenWindow;

        Screen.SetResolution(
            selectedResolution.width,
            selectedResolution.height,
            screenMode
        );

        if (salvar)
        {
            PlayerPrefs.SetInt(WidthKey, selectedResolution.width);
            PlayerPrefs.SetInt(HeightKey, selectedResolution.height);
            PlayerPrefs.SetInt(ModeKey, modoDrop.index);

            PlayerPrefs.Save();
        }
    }

    void RestaurarConfigurations()
    {
        resolutionDrop.index = EncontrarResolutions(
            defaultWidth,
            defaultHeight
        );

        modoDrop.index = defaultFullScreen ? 1 : 0;

        AplicarConfigurations(true);

        Debug.Log("Configurações restauradas!");
    }
}
