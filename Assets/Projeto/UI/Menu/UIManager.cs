using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    VisualElement telaInicial;
    VisualElement telaMenu;
    VisualElement telaSelect;

    Button Jogar;
    Button Options;
    Button Sair;
    Button Partidinha;
    Button Criar;


    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        //Telas
        telaInicial = root.Q<VisualElement>("TelaInicial");
        telaMenu = root.Q<VisualElement>("TelaMenu");
        telaSelect = root.Q<VisualElement>("TelaSlect");

        //Botoes
        Jogar = root.Q<Button>("play");
        Options = root.Q<Button>("OPTIONS");
        Sair = root.Q<Button>("Quit");

        //Eventos

    }
}
