using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private VisualElement telaInicial;
    private VisualElement telaMenu;
    private VisualElement telaSelect;
    private VisualElement telaOptions;

    private VisualElement[] telas;

    private Button jogar;
    private Button options;
    private Button sair;
    private Button partida;
    private Button criar;
    private Button sair2;
    private Button inicio;
    private Button sair3;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        // Telas
        telaInicial = root.Q<VisualElement>("TelaInicial");
        telaMenu = root.Q<VisualElement>("TelaMenu");
        telaSelect = root.Q<VisualElement>("TelaSelect");
        telaOptions = root.Q<VisualElement>("TelaOPtions");

        telas = new VisualElement[]
        {
            telaInicial,
            telaMenu,
            telaSelect,
            telaOptions
        };


        // Botões
        jogar = root.Q<Button>("play");
        options = root.Q<Button>("OPTIONS");
        sair = root.Q<Button>("Quit");
        partida = root.Q<Button>("Partidinha");
        criar = root.Q<Button>("CriarSala");
        sair2 = root.Q<Button>("Sair");
        sair3 = root.Q<Button>("Close");
        inicio = root.Q<Button>("Iniciar");


        // Eventos
        jogar.clicked += AbrirJogo;
        options.clicked += AbrirOptions;
        sair.clicked += SairJogo;

        partida.clicked += InicioJogo;
        criar.clicked += InicioJogo;

        inicio.clicked += AbrirMenu;
        sair2.clicked += VoltarTela;
        sair3.clicked += VoltarTela;


        // Tela inicial
        AbrirInicio();
    }

    private void MostrarTela(VisualElement tela)
    {
        foreach (VisualElement t in telas)
        {
            t.style.display = DisplayStyle.None;
        }

        tela.style.display = DisplayStyle.Flex;
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

    private void AbrirOptions()
    {
        MostrarTela(telaOptions);
    }

    private void InicioJogo()
    {
        SceneManager.LoadScene("Cadeia_Dentro");
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