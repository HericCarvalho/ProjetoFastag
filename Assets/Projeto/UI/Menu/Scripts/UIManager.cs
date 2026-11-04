using AudioSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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
    private VisualElement telaHUDPartida;

    private VisualElement telaAtual;
    private VisualElement subtelaAtual;

    private VisualElement TextoInicio;
    private Label texto;

    private TextField inputIP;
    private TextField inputPorta;

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
    private Button Close2;
    private Button Close3;
    private Button Voltar;
    private Button Config1;
    private Button Config2;
    private Button Config3;
    private Button Config4;
    private Button menuVitoria;
    private Button lobbyVitoria;

    [SerializeField] private SoundData clickSound;
    [SerializeField] private SoundData exitSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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
        telaHUDPartida = root.Q<VisualElement>("TelaHUD_Partida");

        inputIP = root.Q<TextField>("IP");
        inputPorta = root.Q<TextField>("Port");

        telas = new VisualElement[]
        {
            telaInicial,
            telaMenu,
            telaSelect,
            telaOptions,
            telaCustom,
            telaCustomL,
            telaHUDPartida
        };

        telaLobby?.SetEnabled(true);
        telaParty?.SetEnabled(true);
        telaPrincipal?.SetEnabled(true);
        telaMapa?.SetEnabled(true);
        telaConfig?.SetEnabled(true);
        telaHUDPartida?.SetEnabled(true);

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

        if (telaHUDPartida != null)
            telaHUDPartida.style.display = DisplayStyle.None;

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
        Close2 = root.Q<Button>("close2");
        Close3 = root.Q<Button>("close3");
        Voltar = root.Q<Button>("voltar");
        Config1 = root.Q<Button>("config1");
        Config2 = root.Q<Button>("config2");
        Config3 = root.Q<Button>("config3");
        Config4 = root.Q<Button>("config4");
        menuVitoria = root.Q<Button>("BotaoMenuPrincipal");
        lobbyVitoria = root.Q<Button>("BotaoVoltarLobby");


        // =====================================================
        // EVENTOS DOS BOTÕES
        // =====================================================

        //botões menu
        jogar?.RegisterCallback<ClickEvent>(AbrirJogo);
        options?.RegisterCallback<ClickEvent>(AbrirOptions);
        sair?.RegisterCallback<ClickEvent>(SairJogo);

        //botões tela select
        partida?.RegisterCallback<ClickEvent>(IniciarPartidaRede);
        criar?.RegisterCallback<ClickEvent>(AbrirCustom);


        //botões tela custom 
        Lobby?.RegisterCallback<ClickEvent>(AbrirLobby);
        Create?.RegisterCallback<ClickEvent>(AbrirParty);
        createLobby?.RegisterCallback<ClickEvent>(CriarPartidaHost);
        Entrar?.RegisterCallback<ClickEvent>(ConectarComoCliente);

        inicio?.RegisterCallback<ClickEvent>(AbrirMenu);

        //botões telaCustomL
        Iniciar?.RegisterCallback<ClickEvent>(IniciarPartidaRede);
        Conf?.RegisterCallback<ClickEvent>(AbrirConfig);
        Mapa?.RegisterCallback<ClickEvent> (AbrirMapa);
        Voltar?.RegisterCallback<ClickEvent>(VoltarLobby);


        //botoões saida
        sair2?.RegisterCallback<ClickEvent>(VoltarTela);
        sair3?.RegisterCallback<ClickEvent>(VoltarTela);
        sair4?.RegisterCallback<ClickEvent>(VoltarTela);
        Close2.RegisterCallback<ClickEvent>(VoltarSubtela);
        Close3.RegisterCallback<ClickEvent>(VoltarSubtela);

        //botoes vitoria
        menuVitoria?.RegisterCallback<ClickEvent>(VoltarMenuPrincipal);
        lobbyVitoria?.RegisterCallback<ClickEvent>(VoltarParaLobby);


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

        //botões telaCustomL
        Iniciar?.RegisterCallback<ClickEvent>(TocarSomBotao);
        Conf?.RegisterCallback<ClickEvent>(TocarSomBotao);
        Mapa?.RegisterCallback<ClickEvent>(TocarSomBotao); 
        Voltar?.RegisterCallback<ClickEvent>(TocarSomBotao);

        sair2?.RegisterCallback<ClickEvent>(TocarSomBotao);
        sair3?.RegisterCallback<ClickEvent>(TocarSomBotao);
        sair4?.RegisterCallback<ClickEvent>(TocarSomBotao);
        Close2.RegisterCallback<ClickEvent>(TocarSomBotao);
        Close3.RegisterCallback<ClickEvent>(TocarSomBotao);

        //botões vitoria
        menuVitoria?.RegisterCallback<ClickEvent>(TocarSomBotao);
        lobbyVitoria?.RegisterCallback<ClickEvent>(TocarSomBotao);
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

        partida?.UnregisterCallback<ClickEvent>(IniciarPartidaRede);
        criar?.UnregisterCallback<ClickEvent>(AbrirCustom);

        Lobby?.UnregisterCallback<ClickEvent>(AbrirLobby);
        Create?.UnregisterCallback<ClickEvent>(AbrirParty);
        createLobby?.UnregisterCallback<ClickEvent>(CriarPartidaHost);
        Entrar?.UnregisterCallback<ClickEvent>(ConectarComoCliente);

        inicio?.UnregisterCallback<ClickEvent>(AbrirMenu);

        //botões telaCustomL
        Iniciar?.UnregisterCallback<ClickEvent>(IniciarPartidaRede);
        Conf?.UnregisterCallback<ClickEvent>(AbrirConfig);
        Mapa?.UnregisterCallback<ClickEvent>(AbrirMapa);
        Voltar?.UnregisterCallback<ClickEvent>(VoltarLobby);

        sair2?.UnregisterCallback<ClickEvent>(VoltarTela);
        sair3?.UnregisterCallback<ClickEvent>(VoltarTela);
        sair4?.UnregisterCallback<ClickEvent>(VoltarTela);
        Close2.UnregisterCallback<ClickEvent>(VoltarSubtela);
        Close3.UnregisterCallback<ClickEvent>(VoltarSubtela);

        //botões vitoria
        menuVitoria?.UnregisterCallback<ClickEvent>(VoltarMenuPrincipal);
        lobbyVitoria?.UnregisterCallback<ClickEvent>(VoltarParaLobby);


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

        Iniciar?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        Conf?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        Mapa?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        Voltar?.UnregisterCallback<ClickEvent>(TocarSomBotao);

        sair2?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        sair3?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        sair4?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        Close2.UnregisterCallback<ClickEvent>(TocarSomBotao);
        Close3.UnregisterCallback<ClickEvent>(TocarSomBotao);

        //botões vitoria
        menuVitoria?.UnregisterCallback<ClickEvent>(TocarSomBotao);
        lobbyVitoria?.UnregisterCallback<ClickEvent>(TocarSomBotao);
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
        MostrarSubTela(telaPrincipal);
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
    private void VoltarSubtela(ClickEvent evt)
    {
        MostrarSubTela(telaPrincipal);
    }

    private void VoltarLobby(ClickEvent evt)
    {
        MostrarTela(telaCustom);
        MostrarSubTela(telaParty);
    }


    private void SairJogo(ClickEvent evt)
    {
        Debug.Log("Saindo");

        Application.Quit();
    }

    //Adionado pelo Heric
    private void CriarPartidaHost(ClickEvent evt)
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
        }

        AbrirCustomL(evt);
    
    }
    private void IniciarPartidaRede(ClickEvent evt)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("LoadScene", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene("LoadScene");
        }
    }
    public void AtivarHUDPartida()
    {
        if (telaHUDPartida != null)
        {
            if (telaInicial != null) telaInicial.style.display = DisplayStyle.None;
            if (telaMenu != null) telaMenu.style.display = DisplayStyle.None;
            if (telaSelect != null) telaSelect.style.display = DisplayStyle.None;
            if (telaOptions != null) telaOptions.style.display = DisplayStyle.None;
            if (telaCustom != null) telaCustom.style.display = DisplayStyle.None;
            if (telaLobby != null) telaLobby.style.display = DisplayStyle.None;
            if (telaParty != null) telaParty.style.display = DisplayStyle.None;
            if (telaCustomL != null) telaCustomL.style.display = DisplayStyle.None;
            if (telaPrincipal != null) telaPrincipal.style.display = DisplayStyle.None;
            if (telaMapa != null) telaMapa.style.display = DisplayStyle.None;
            if (telaConfig != null) telaConfig.style.display = DisplayStyle.None;

            telaHUDPartida.style.display = DisplayStyle.Flex;
        }
    }
    public void AtualizarHUD(int segundos, int rodada, int mal, int bem)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        if (root == null) return;

        Label lblTempo = root.Q<Label>("time_text");
        if (lblTempo != null)
        {
            int minutos = segundos / 60;
            int secs = segundos % 60;
            lblTempo.text = $"{minutos:00}:{secs:00}";
        }
        Label lblPlacar = root.Q<Label>("score_text");
        if (lblPlacar != null)
        {
            lblPlacar.text = $"{mal} / {bem}";
        }

        AtualizarSpritesEquipesUI(root);
    }
    private void AtualizarSpritesEquipesUI(VisualElement root)
    {
        if (LobbyManager.Singleton == null || LobbyManager.Singleton.spritesPersonagens == null)
            return;

        VisualElement[] imagensPegadores = new VisualElement[2];
        imagensPegadores[0] = root.Q<VisualElement>("Pegador_1");
        imagensPegadores[1] = root.Q<VisualElement>("Pegador_2");

        VisualElement[] imagensVitimas = new VisualElement[2];
        imagensVitimas[0] = root.Q<VisualElement>("Vitima_1");
        imagensVitimas[1] = root.Q<VisualElement>("Vitima_2");

        // Limpa as imagens primeiro
        for (int i = 0; i < imagensPegadores.Length; i++)
            ConfigurarImagemEquipe(imagensPegadores[i], null);

        for (int i = 0; i < imagensVitimas.Length; i++)
            ConfigurarImagemEquipe(imagensVitimas[i], null);

        int indicePegador = 0;
        int indiceVitima = 0;

        foreach (var jogador in LobbyManager.Singleton.slotsJogadores)
        {
            if (!jogador.IsOccupied) continue;

            Sprite[] sprites = LobbyManager.Singleton.spritesPersonagens;
            if (jogador.PersonagemIndex < 0 || jogador.PersonagemIndex >= sprites.Length)
                continue;

            Sprite sprite = sprites[jogador.PersonagemIndex];

            if (jogador.IsPegador && indicePegador < imagensPegadores.Length)
            {
                ConfigurarImagemEquipe(imagensPegadores[indicePegador], sprite);
                indicePegador++;
            }
            else if (!jogador.IsPegador && indiceVitima < imagensVitimas.Length)
            {
                ConfigurarImagemEquipe(imagensVitimas[indiceVitima], sprite);
                indiceVitima++;
            }
        }
    }

    private void ConfigurarImagemEquipe(VisualElement elemento, Sprite sprite)
    {
        if (elemento == null) return;

        if (sprite == null)
        {
            elemento.style.backgroundImage = StyleKeyword.None;
            elemento.style.display = DisplayStyle.None;
            return;
        }

        elemento.style.backgroundImage = new StyleBackground(sprite);
        elemento.style.display = DisplayStyle.Flex;
    }

    public void ExibirTelaVitoria(string vencedor, int mal, int bem)
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        if (root == null) return;
        Label lblVencedor = root.Q<Label>("TextoVencedor");
        if (lblVencedor != null)
        {
            lblVencedor.text = $"Vencedor: {vencedor}";
        }
        Label lblPlacar = root.Q<Label>("TextoPlacarFinal");
        if (lblPlacar != null)
        {
            lblPlacar.text = $"{mal} / {bem}";
        }
    }

    public void ExibirLobby()
    {
        if (telaHUDPartida != null)
            telaHUDPartida.style.display = DisplayStyle.None;

        MostrarTela(telaCustom);
        MostrarSubTela(telaParty);
    }
    private void ConectarComoCliente(ClickEvent evt)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        string enderecoIP = (inputIP != null && !string.IsNullOrEmpty(inputIP.value)) ? inputIP.value : "127.0.0.1";

        ushort porta = 7777;
        if (inputPorta != null && !string.IsNullOrEmpty(inputPorta.value))
        {
            if (ushort.TryParse(inputPorta.value, out ushort portaConvertida))
            {
                porta = portaConvertida;
            }
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(enderecoIP, porta);
        }
        else
        {
            return;
        }

        if (NetworkManager.Singleton.StartClient())
        {
            AbrirCustomL(evt);
        }
    }
    private void VoltarMenuPrincipal(ClickEvent evt)
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (telaHUDPartida != null)
        {
            telaHUDPartida.style.display = DisplayStyle.None;
        }

        MostrarTela(telaMenu);
    }

    private void VoltarParaLobby(ClickEvent evt)
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        ExibirLobby();
    }
}