
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GraficoConfig : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private UIDocument document;
    [SerializeField] private AudioSettings audioSettings;

    [Header("Configurações Iniciais")]
    [SerializeField] private int defaultWidth = 1920;
    [SerializeField] private int defaultHeight = 1080;
    [SerializeField] private bool defaultFullScreen = true;

    [Header("Viewport")]
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    private VisualElement screenRoot;
    private VisualElement viewport;

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
        if (document == null)
        {
            Debug.LogError("UIDocument não configurado!");
            return;
        }

        VisualElement root = document.rootVisualElement;

        // CONFIGURAÇÃO DO VIEWPORT
        ConfigurarViewport(root);

        // BUSCAR ELEMENTOS DO MENU
        resolutionDrop = root.Q<DropdownField>("resolution_tela");
        modoDrop = root.Q<DropdownField>("TelaModo");

        aceita = root.Q<Button>("botao_aceita");
        recusa = root.Q<Button>("botao_recusa");

        if (resolutionDrop == null ||
            modoDrop == null ||
            aceita == null ||
            recusa == null)
        {
            Debug.LogError("Elementos do menu não encontrados!");
            return;
        }

        CarregarResolutions();

        modoDrop.choices = new List<string>()
        {
            "Janela",
            "Tela Cheia"
        };

        aceita.clicked += AplicarConfig;
        recusa.clicked += RestaurarConfigurations;

        CarregarConfigurations();
    }

    // ==========================================
    // VIEWPORT 16:9
    // ==========================================

    void ConfigurarViewport(VisualElement root)
    {
        screenRoot = root.Q<VisualElement>("fullScreen_root");
        viewport = root.Q<VisualElement>("TELA_16X9");

        if (screenRoot == null || viewport == null)
        {
            Debug.LogError(
                "FullScreenRoot ou Area16x9 não encontrados no UI Builder!"
            );
            return;
        }

        // Configuração do contêiner externo
        screenRoot.style.width = Length.Percent(100);
        screenRoot.style.height = Length.Percent(100);

        screenRoot.style.position = Position.Relative;
        screenRoot.style.overflow = Overflow.Hidden;

        // Fundo das áreas excedentes (letterbox)
        screenRoot.style.backgroundColor = Color.clear;

        // Configuração da área 16:9
        viewport.style.position = Position.Absolute;

        viewport.style.width = ReferenceWidth;
        viewport.style.height = ReferenceHeight;

        viewport.style.flexGrow = 0;
        viewport.style.flexShrink = 0;

        viewport.style.overflow = Overflow.Hidden;

        // Atualiza quando o tamanho da tela mudar
        screenRoot.RegisterCallback<GeometryChangedEvent>(
            AtualizarViewport
        );

        AjustarViewport();
    }

    void AtualizarViewport(GeometryChangedEvent evt)
    {
        AjustarViewport();
    }

    void AjustarViewport()
    {
        if (screenRoot == null || viewport == null)
            return;

        float largura = screenRoot.contentRect.width;
        float altura = screenRoot.contentRect.height;

        if (largura <= 0 || altura <= 0)
            return;

        // Centraliza a área 1920x1080
        float posicaoX = (largura - ReferenceWidth) / 2f;
        float posicaoY = (altura - ReferenceHeight) / 2f;

        viewport.style.left = posicaoX;
        viewport.style.top = posicaoY;
    }

    // ==========================================
    // RESOLUÇÕES
    // ==========================================

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
                break;
        }

        return melhorIndice;
    }

    // ==========================================
    // APLICAR CONFIGURAÇÕES
    // ==========================================

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

        Debug.Log(
            $"Resolução solicitada: {selectedResolution.width}x{selectedResolution.height}"
        );
    }

    // ==========================================
    // RESTAURAR CONFIGURAÇÕES
    // ==========================================

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

    private void OnDestroy()
    {
        if (screenRoot != null)
        {
            screenRoot.UnregisterCallback<GeometryChangedEvent>(
                AtualizarViewport
            );
        }

        if (aceita != null)
            aceita.clicked -= AplicarConfig;

        if (recusa != null)
            recusa.clicked -= RestaurarConfigurations;
    }
}
