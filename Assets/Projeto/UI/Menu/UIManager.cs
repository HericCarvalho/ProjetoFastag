using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    VisualElement telaInicial;
    VisualElement telaMenu;
    VisualElement telaSelect;

    Button Jogar;
    Button Options;
    Button Sair;
    Button Partida;
    Button Criar;


    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        //Telas
        telaInicial = root.Q<VisualElement>("TelaInicial");
        telaMenu = root.Q<VisualElement>("TelaMenu");
        telaSelect = root.Q<VisualElement>("TelaSelect");

        //Botoes
        Jogar = root.Q<Button>("play");
        Options = root.Q<Button>("OPTIONS");
        Sair = root.Q<Button>("Quit");
        Partida = root.Q<Button>("Partidinha");
        Criar = root.Q<Button>("CriarSala");


        //Eventos
        Jogar.clicked += AbrirJogo;
        //Options.clicked += AbrirOptions;
        Sair.clicked += SairJogo;
        Partida.clicked += InicioJogo;
        Criar.clicked += InicioJogo;

        AbrirInicio();
    }

    void AbrirInicio()
    {
        telaInicial.style.display = DisplayStyle.None;
        telaMenu.style.display = DisplayStyle.Flex;
        telaSelect.style.display = DisplayStyle.None;
    }

    void AbrirJogo()
    {
        telaInicial.style.display= DisplayStyle.None;
        telaMenu.style.display = DisplayStyle.None;
        telaSelect.style.display =DisplayStyle.Flex;
    }

    void InicioJogo()
    {
        SceneManager.LoadScene("Cadeia_Dentro");
    }

    void SairJogo()
    {
        Application.Quit();
    }

}
