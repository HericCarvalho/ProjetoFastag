using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
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

    private VisualElement telaAtual;
    private VisualElement subtelaAtual;

    private VisualElement TextoInicio;

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

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        //secundarios
        TextoInicio =root.Q<VisualElement>("TituloJogo");
        TextoInicio.pickingMode = PickingMode.Ignore;


        // Telas
        telaInicial = root.Q<VisualElement>("TelaInicial");
        telaMenu = root.Q<VisualElement>("TelaMenu");
        telaSelect = root.Q<VisualElement>("TelaSelect");
        telaOptions = root.Q<VisualElement>("TelaOPtions");
        telaCustom = root.Q<VisualElement>("TelaCustom");
        telaLobby = root.Q<VisualElement>("enterLobby");
        telaParty = root.Q<VisualElement>("createParty");

        telas = new VisualElement[]
        {
            telaInicial,
            telaMenu,
            telaSelect,
            telaOptions,
            telaCustom,
            
        };

        telaLobby.style.display = DisplayStyle.None;
        telaParty.style.display = DisplayStyle.None;

        telaAtual = telaInicial;



        // Botões
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




        // Eventos
        jogar.clicked += AbrirJogo;
        options.clicked += AbrirOptions;
        sair.clicked += SairJogo;

        partida.clicked += InicioJogo;
        criar.clicked += AbrirCustom;
        Lobby.clicked += AbrirLobby;
        Create.clicked += AbrirParty;
        createLobby.clicked += InicioJogo;
        Entrar.clicked += InicioJogo;

        inicio.clicked += AbrirMenu;
        sair2.clicked += VoltarTela;
        sair3.clicked += VoltarTela;
        sair4.clicked += VoltarTela;


        // Tela inicial
        //AbrirInicio();
    }

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

    private void MostrarSubTela(VisualElement novaSubTela)
    {
        if (subtelaAtual != null)
        {
            subtelaAtual.style.display = DisplayStyle.None;
        }

        novaSubTela.style.display= DisplayStyle.Flex;

        subtelaAtual = novaSubTela;
    }

    private void AbrirInicio()
    {
        MostrarTela(telaInicial);
    }

    private void AbrirMenu()
    {
        MostrarTela(telaMenu);
    }

    private void AbrirJogo()
    {
        MostrarTela(telaSelect);
    }

    private void AbrirCustom()
    {
        MostrarTela(telaCustom);
        MostrarSubTela(telaLobby);
    }

    private void AbrirLobby()
    {
        MostrarSubTela(telaLobby);
    }

    private void AbrirParty()
    {
        MostrarSubTela(telaParty);
    }

    private void AbrirOptions()
    {
        MostrarTela(telaOptions);
    }

    private void InicioJogo()
    {
        SceneManager.LoadScene("LoadScene");
    }

    private void VoltarTela()
    {
        MostrarTela(telaMenu);
    }

    private void SairJogo()
    {
        Debug.Log("Saindo");

        Application.Quit();
    }
}