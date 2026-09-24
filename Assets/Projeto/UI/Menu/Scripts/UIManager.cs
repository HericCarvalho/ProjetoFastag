using AudioSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private VisualElement telaInicial;
    private VisualElement telaMenu;
    private VisualElement telaSelect;
    private VisualElement telaOptions;
    private VisualElement telaCustom;
    private VisualElement telaLobby;
    private VisualElement telaParty;
    private VisualElement telaCustomL;
    private VisualElement telaPrincipal;
    private VisualElement telaMapa;
    private VisualElement telaConfig;

    private VisualElement telaAtual;
    private VisualElement subtelaAtual;

    private VisualElement TextoInicio;
    private Label texto;

    private VisualElement[] telas;

    private Button jogar;
    private Button options;
    private Button sair;
    private Button partida;
    private Button criar;
    private Button sair2;
    private Button inicio;
    private Button sair3;
    private Button sair4;
    private Button Lobby;
    private Button Create;
    private Button createLobby;
    private Button Entrar;
    private Button Iniciar;
    private Button Conf;
    private Button Mapa ;

    [SerializeField] private SoundData clickSound;
    [SerializeField] private SoundData exitSound;


    // =========================================================
    // ON ENABLE
    // =========================================================

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;


        // =====================================================
        // ELEMENTOS
        // =====================================================

        // Secundários
        TextoInicio = root.Q<VisualElement>("TituloJogo");
        TextoInicio.pickingMode = PickingMode.Ignore;

        texto = root.Q<Label>("continue");
        texto?.AddToClassList("text-pisca");

        if (texto != null)
        {
            Piscar();
        }


        // =====================================================
        // TELAS
        // =====================================================

        telaInicial = root.Q<VisualElement>("TelaInicial");
        telaMenu = root.Q<VisualElement>("TelaMenu");
        telaSelect = root.Q<VisualElement>("TelaSelect");
        telaOptions = root.Q<VisualElement>("TelaOPtions");
        telaCustom = root.Q<VisualElement>("TelaCustom");
        telaLobby = root.Q<VisualElement>("enterLobby");
        telaParty = root.Q<VisualElement>("createParty");
        telaCustomL = root.Q<VisualElement>("TelaCustomConfig");
        telaPrincipal = root.Q<VisualElement>("tela_principal");
        telaMapa = root.Q<VisualElement>("tela_mapa");
        telaConfig = root.Q<VisualElement>("tela_config");

        telas = new VisualElement[]
        {
            telaInicial,
            telaMenu,
            telaSelect,
            telaOptions,
            telaCustom,
            telaCustomL
        };

        telaLobby?.SetEnabled(true);
        telaParty?.SetEnabled(true);
        telaPrincipal?.SetEnabled(true);
        telaMapa?.SetEnabled(true);
        telaConfig?.SetEnabled(true);

        if (telaLobby != null)
            telaLobby.style.display = DisplayStyle.None;

        if (telaParty != null)
            telaParty.style.display = DisplayStyle.None;

        if(telaPrincipal != null) 
            telaPrincipal.style.display = DisplayStyle.None;

        if (telaConfig != null) 
            telaConfig.style.display = DisplayStyle.None;

        if (telaMapa != null)
            telaMapa.style.display = DisplayStyle.None;

        telaAtual = telaInicial;


        // =====================================================
        // BOTÕES
        // =====================================================

        jogar = root.Q<Button>("play");
        options = root.Q<Button>("OPTIONS");
        sair = root.Q<Button>("Quit");
        partida = root.Q<Button>("Partidinha");
        criar = root.Q<Button>("CriarSala");
        sair2 = root.Q<Button>("Sair");
        sair3 = root.Q<Button>("Close");
        inicio = root.Q<Button>("Iniciar");
        sair4 = root.Q<Button>("fechar");
        Lobby = root.Q<Button>("lobby");
        Create = root.Q<Button>("create");
        createLobby = root.Q<Button>("createLobby");
        Entrar = root.Q<Button>("enter");
        Iniciar = root.Q<Button>("iniciar");
        Conf = root.Q<Button>("conf");
        Mapa = root.Q<Button>("mapas");


        // =====================================================
        // EVENTOS DOS BOTÕES
        // =====================================================

        //botões menu
        jogar?.RegisterCallback<ClickEvent>(AbrirJogo);
        options?.RegisterCallback<ClickEvent>(AbrirOptions);
        sair?.RegisterCallback<ClickEvent>(SairJogo);

        //botões tela select
        partida?.RegisterCallback<ClickEvent>(InicioJogo);
        criar?.RegisterCallback<ClickEvent>(AbrirCustom);


        //botões tela custom 
        Lobby?.RegisterCallback<ClickEvent>(AbrirLobby);
        Create?.RegisterCallback<ClickEvent>(AbrirParty);
        createLobby?.RegisterCallback<ClickEvent>(AbrirCustomL);
        Entrar?.RegisterCallback<ClickEvent>(InicioJogo);

        inicio?.RegisterCallback<ClickEvent>(AbrirMenu);

        //botões telaCustomL
        Iniciar?.RegisterCallback<ClickEvent>(InicioJogo);
        Conf?.RegisterCallback<ClickEvent>(AbrirConfig);
        Mapa?.RegisterCallback<ClickEvent> (AbrirMapa);


        //botoões saida
        sair2?.RegisterCallback<ClickEvent>(VoltarTela);
        sair3?.RegisterCallback<ClickEvent>(VoltarTela);
        sair4?.RegisterCallback<ClickEvent>(VoltarTela);


        // =====================================================
        // SOM DOS BOTÕES
        // =====================================================

        jogar?.RegisterCallback<ClickEvent>(TocarSomBotao);
        options?.RegisterCallback<ClickEvent>(TocarSomBotao);
        sair?.RegisterCallback<ClickEvent>(TocarSomBotao);

        partida?.RegisterCallback<ClickEvent>(TocarSomBotao);
        criar?.RegisterCallback<ClickEvent>(TocarSomBotao);

        Lobby?.RegisterCallback<ClickEvent>(TocarSomBotao);
        Create?.RegisterCallback<ClickEvent>(TocarSomBotao);
        createLobby?.RegisterCallback<ClickEvent>(TocarSomBotao);
        Entrar?.RegisterCallback<ClickEvent>(TocarSomBotao);

        inicio?.RegisterCallback<ClickEvent>(TocarSomBotao);

        sair2?.RegisterCallback<ClickEvent>(TocarSomBotao);
        sair3?.RegisterCallback<ClickEvent>(TocarSomBotao);
        sair4?.RegisterCallback<ClickEvent>(TocarSomBotao);
    }


    // =========================================================
    // ON DISABLE
    // =========================================================

    private void OnDisable()
    {
        // =====================================================
        // REMOVER EVENTOS DOS BOTÕES
        // =====================================================


        //botoes 
        jogar?.UnregisterCallback<ClickEvent>(AbrirJogo);
        options?.UnregisterCallback<ClickEvent>(AbrirOptions);
        sair?.UnregisterCallback<ClickEvent>(SairJogo);

        partida?.UnregisterCallback<ClickEvent>(InicioJogo);
        criar?.UnregisterCallback<ClickEvent>(AbrirCustom);

        Lobby?.UnregisterCallback<ClickEvent>(AbrirLobby);
        Create?.UnregisterCallback<ClickEvent>(AbrirParty);
        createLobby?.UnregisterCallback<ClickEvent>(AbrirCustomL);
        Entrar?.UnregisterCallback<ClickEvent>(InicioJogo);

        inicio?.UnregisterCallback<ClickEvent>(AbrirMenu);

        sair2?.UnregisterCallback<ClickEvent>(VoltarTela);
        sair3?.UnregisterCallback<ClickEvent>(VoltarTela);
        sair4?.UnregisterCallback<ClickEvent>(VoltarTela);


        // =====================================================
        // REMOVER SOM DOS BOTÕES
        // =====================================================

        jogar?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        options?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        sair?.UnregisterCallback<ClickEvent>(TocarSomBotao);

        partida?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        criar?.UnregisterCallback<ClickEvent>(TocarSomBotao);

        Lobby?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        Create?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        createLobby?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        Entrar?.UnregisterCallback<ClickEvent>(TocarSomBotao);

        inicio?.UnregisterCallback<ClickEvent>(TocarSomBotao);

        sair2?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        sair3?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        sair4?.UnregisterCallback<ClickEvent>(TocarSomBotao);
    }


    // =========================================================
    // TRANSIÇÃO ENTRE TELAS
    // =========================================================

    private void MostrarTela(VisualElement novaTela)
    {
        if (telaAtual == novaTela)
        {
            return;
        }

        VisualElement antigaTela = telaAtual;

        if (antigaTela != null)
        {
            antigaTela.AddToClassList("tela-saindo");

            antigaTela.schedule.Execute(() =>
            {
                antigaTela.style.display = DisplayStyle.None;
                antigaTela.RemoveFromClassList("tela-saindo");

                AbrirNovaTela(novaTela);

            }).StartingIn(500);
        }
        else
        {
            AbrirNovaTela(novaTela);
        }
    }


    private void AbrirNovaTela(VisualElement novaTela)
    {
        novaTela.style.display = DisplayStyle.Flex;

        novaTela.AddToClassList("tela-entrando");

        novaTela.schedule.Execute(() =>
        {
            novaTela.RemoveFromClassList("tela-entrando");

        }).StartingIn(20);

        telaAtual = novaTela;
    }


    // =========================================================
    // SUBTELAS
    // =========================================================

    private void MostrarSubTela(VisualElement novaSubTela)
    {
        if (subtelaAtual != null)
        {
            subtelaAtual.style.display = DisplayStyle.None;
        }

        novaSubTela.style.display = DisplayStyle.Flex;

        subtelaAtual = novaSubTela;
    }


    // =========================================================
    // PISCAR TEXTO
    // =========================================================

    private void Piscar()
    {
        texto.style.opacity = 0.5f;
        texto.style.scale = new Scale(new Vector2(1.0f, 1.0f));

        texto.schedule.Execute(() =>
        {
            texto.style.opacity = 1f;
            texto.style.scale = new Scale(new Vector2(1.05f, 1.05f));

            texto.schedule.Execute(() =>
            {
                Piscar();

            }).StartingIn(500);

        }).StartingIn(500);
    }


    // =========================================================
    // SOM
    // =========================================================

    private void TocarSomBotao(ClickEvent evt)
    {
        Button botao = evt.currentTarget as Button;

        if (botao == null)
            return;

        SoundData som = clickSound;

        if (botao.name == "Sair" ||
            botao.name == "Close" ||
            botao.name == "fechar" ||
            botao.name == "Quit")
        {
            som = exitSound;
        }

        TocarSom(som);
    }


    private void TocarSom(SoundData som)
    {
        if (som == null)
            return;

        SoundManager.Instance
            .CreateSoundBuilder()
            .Play(som);
    }


    // =========================================================
    // NAVEGAÇÃO
    // =========================================================

    private void AbrirInicio()
    {
        MostrarTela(telaInicial);
    }


    private void AbrirMenu(ClickEvent evt)
    {
        MostrarTela(telaMenu);
    }


    private void AbrirJogo(ClickEvent evt)
    {
        MostrarTela(telaSelect);
    }


    private void AbrirCustom(ClickEvent evt)
    {
        MostrarTela(telaCustom);
        MostrarSubTela(telaLobby);
    }


    private void AbrirLobby(ClickEvent evt)
    {
        MostrarSubTela(telaLobby);
    }


    private void AbrirParty(ClickEvent evt)
    {
        MostrarSubTela(telaParty);
    }


    private void AbrirOptions(ClickEvent evt)
    {
        MostrarTela(telaOptions);
    }


    private void AbrirCustomL(ClickEvent evt)
    {
        MostrarTela(telaCustomL);
        MostrarSubTela(telaInicial);
    }

    private void AbrirMapa(ClickEvent evt)
    {
        MostrarSubTela(telaMapa);
    }

    private void AbrirConfig(ClickEvent evt)
    {
        MostrarSubTela(telaConfig);
    }


    private void InicioJogo(ClickEvent evt)
    {
        SceneManager.LoadScene("LoadScene");
    }


    private void VoltarTela(ClickEvent evt)
    {
        MostrarTela(telaMenu);
    }


    private void SairJogo(ClickEvent evt)
    {
        Debug.Log("Saindo");

        Application.Quit();
    }
}