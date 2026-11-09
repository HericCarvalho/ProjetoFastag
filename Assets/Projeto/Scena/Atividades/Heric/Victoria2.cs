using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

public class Recursos : INotifyBindablePropertyChanged
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
    void Notify(string prop) =>
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(prop));

    int trigo, pedra, ferro, fe, moedas;
    bool pedreiraAtiva, industriaAtiva, catedralAtiva;

    public int Trigo { get => trigo; set { if (trigo == value) return; trigo = value; Notify(nameof(TextoTrigo)); } }
    public int Pedra { get => pedra; set { if (pedra == value) return; pedra = value; Notify(nameof(TextoPedra)); } }
    public int Ferro { get => ferro; set { if (ferro == value) return; ferro = value; Notify(nameof(TextoFerro)); } }
    public int Fe { get => fe; set { if (fe == value) return; fe = value; Notify(nameof(TextoFe)); } }
    public int Moedas { get => moedas; set { if (moedas == value) return; moedas = value; Notify(nameof(TextoMoedas)); } }

    public bool PedreiraAtiva { get => pedreiraAtiva; set { if (pedreiraAtiva == value) return; pedreiraAtiva = value; Notify(nameof(InfoPedreira)); } }
    public bool IndustriaAtiva { get => industriaAtiva; set { if (industriaAtiva == value) return; industriaAtiva = value; Notify(nameof(InfoIndustria)); } }
    public bool CatedralAtiva { get => catedralAtiva; set { if (catedralAtiva == value) return; catedralAtiva = value; Notify(nameof(InfoCatedral)); } }

    [CreateProperty] public string TextoTrigo => $"1M/Sacos de Trigo: {trigo}";
    [CreateProperty] public string TextoPedra => $"5M/Blocos de Pedra: {pedra}";
    [CreateProperty] public string TextoFerro => $"10M/Peças de Ferro: {ferro}";
    [CreateProperty] public string TextoFe => $"Fé: {fe}";
    [CreateProperty] public string TextoMoedas => $"Moedas: {moedas}";

    [CreateProperty] public string InfoPedreira => pedreiraAtiva ? "" : "Desbloqueia com:\n20 Moedas / 20 Trigos";
    [CreateProperty] public string InfoIndustria => industriaAtiva ? "" : "Desbloqueia com:\n100 Moedas / 100 Pedras";
    [CreateProperty] public string InfoCatedral => catedralAtiva ? "" : "Desbloqueia com:\n500 Moedas / 500 Ferros";
}

[RequireComponent(typeof(UIDocument))]
public class Victoria2 : MonoBehaviour
{
    [Header("Producao por clique")]
    [SerializeField] int trigoPorClique = 1;
    [SerializeField] int pedraPorClique = 1;
    [SerializeField] int ferroPorClique = 1;
    [SerializeField] int fePorClique = 1;

    [Header("Preco de venda (moedas por unidade)")]
    [SerializeField] int precoTrigo = 1;
    [SerializeField] int precoPedra = 5;
    [SerializeField] int precoFerro = 10;

    [Header("Custo para desbloquear")]
    [SerializeField] int pedreiraMoedas = 20;
    [SerializeField] int pedreiraTrigo = 20;
    [SerializeField] int industriaMoedas = 100;
    [SerializeField] int industriaPedra = 100;
    [SerializeField] int catedralMoedas = 500;
    [SerializeField] int catedralFerro = 500;

    readonly Recursos recursos = new Recursos();
    VisualElement[] telas;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var botoes = root.Q("Botoes");
        var single = root.Q("UI_SinglePlayer");
        var options = root.Q("UI_Options");

        if (botoes != null && single != null && options != null)
        {
            telas = new[] { botoes, single, options };
            IrPara(botoes); 

            var cima = botoes.Q("BotoesCima");
            var listaCima = cima != null ? new List<VisualElement>(cima.Children()) : new List<VisualElement>();

            if (listaCima.Count > 0)
                listaCima[0].RegisterCallback<ClickEvent>(_ => IrPara(single));

            root.Q("Sair")?.RegisterCallback<ClickEvent>(_ => IrPara(botoes));
        }

        var fazenda = root.Q("Fazenda");
        var pedreira = root.Q("Pedreira");
        var industria = root.Q("Industria");
        var catedral = root.Q("Catedral");
        var mercado = root.Q("Mercado");

        if (fazenda == null || pedreira == null || industria == null || catedral == null || mercado == null) return;

        Ligar(Labels(fazenda, 0), nameof(Recursos.TextoTrigo));
        Ligar(Labels(pedreira, 0), nameof(Recursos.TextoPedra));
        Ligar(Labels(pedreira, 1), nameof(Recursos.InfoPedreira));
        Ligar(Labels(industria, 0), nameof(Recursos.TextoFerro));
        Ligar(Labels(industria, 1), nameof(Recursos.InfoIndustria));
        Ligar(Labels(catedral, 0), nameof(Recursos.TextoFe));
        Ligar(Labels(catedral, 1), nameof(Recursos.InfoCatedral));
        Ligar(Labels(mercado, 0), nameof(Recursos.TextoMoedas));

        MostrarBloqueio(pedreira, false);
        MostrarBloqueio(industria, false);
        MostrarBloqueio(catedral, false);

        fazenda.RegisterCallback<ClickEvent>(_ => recursos.Trigo += trigoPorClique);

        pedreira.RegisterCallback<ClickEvent>(_ =>
        {
            if (recursos.PedreiraAtiva) { recursos.Pedra += pedraPorClique; return; }
            if (recursos.Moedas >= pedreiraMoedas && recursos.Trigo >= pedreiraTrigo)
            {
                recursos.Moedas -= pedreiraMoedas;
                recursos.Trigo -= pedreiraTrigo;
                recursos.PedreiraAtiva = true;
                MostrarBloqueio(pedreira, true);
            }
        });

        industria.RegisterCallback<ClickEvent>(_ =>
        {
            if (recursos.IndustriaAtiva) { recursos.Ferro += ferroPorClique; return; }
            if (recursos.Moedas >= industriaMoedas && recursos.Pedra >= industriaPedra)
            {
                recursos.Moedas -= industriaMoedas;
                recursos.Pedra -= industriaPedra;
                recursos.IndustriaAtiva = true;
                MostrarBloqueio(industria, true);
            }
        });

        catedral.RegisterCallback<ClickEvent>(_ =>
        {
            if (recursos.CatedralAtiva) { recursos.Fe += fePorClique; return; }
            if (recursos.Moedas >= catedralMoedas && recursos.Ferro >= catedralFerro)
            {
                recursos.Moedas -= catedralMoedas;
                recursos.Ferro -= catedralFerro;
                recursos.CatedralAtiva = true;
                MostrarBloqueio(catedral, true);
            }
        });

        mercado.RegisterCallback<ClickEvent>(_ => Vender());
    }

    void IrPara(VisualElement destino)
    {
        foreach (var tela in telas)
            tela.style.display = tela == destino ? DisplayStyle.Flex : DisplayStyle.None;
    }

    static void MostrarBloqueio(VisualElement card, bool desbloqueado)
    {
        card.style.opacity = desbloqueado ? 1f : 0.4f;
    }

    static Label Labels(VisualElement card, int indice)
    {
        var lista = card.Query<Label>().ToList();
        if (indice >= lista.Count)
        {
            return null;
        }
        return lista[indice];
    }

    void Ligar(Label label, string propriedade)
    {
        if (label == null) return;
        label.dataSource = recursos;
        label.SetBinding("text", new DataBinding
        {
            dataSourcePath = new PropertyPath(propriedade),
            bindingMode = BindingMode.ToTarget
        });
    }

    void Vender()
    {
        int total = recursos.Trigo * precoTrigo
                  + recursos.Pedra * precoPedra
                  + recursos.Ferro * precoFerro;
        if (total <= 0) return;

        recursos.Moedas += total;
        recursos.Trigo = 0;
        recursos.Pedra = 0;
        recursos.Ferro = 0;
    }
}