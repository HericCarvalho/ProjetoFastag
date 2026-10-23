
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class ControladorUI : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement root;

    private VisualElement painelPrincipal;
    private VisualElement painelPortIP;
    private VisualElement painelEntrar;
    private VisualElement painelLobby;
    private VisualElement painelGameUI;
    private VisualElement painelVitoria;

    private TextField campoIP;
    private TextField campoPorta;

    private Label textoTempo;
    private Label textoPlacar;
    private Label textoRodada;
    private Label textoVencedor;
    private Label textoPlacarFinal;

    private Button botaoClient;
    private Button botaoEntrar;
    private Button botaoServer;
    private Button botaoHost;
    private Button botaoExit;
    private Button botaoVoltar;
    private Button botaoVoltarLobby;
    private Button botaoMenuPrincipal;

    private void OnEnable()
    {
        uiDoc = GetComponent<UIDocument>();

        if (uiDoc == null)
        {
            Debug.LogError("ControladorUI precisa de um UIDocument.");
            return;
        }
        

        if (painelGameUI != null)
            painelGameUI.style.display = DisplayStyle.None;

        root = uiDoc.rootVisualElement;

        MapearElementos();
        RegistrarEventos();
        MostrarMenuInicial();
    }

    private void OnDisable()
    {
        RemoverEventos();
    }

    private void MapearElementos()
    {
        painelPrincipal = root.Q<VisualElement>("Painel_Principal");
        painelEntrar = root.Q<VisualElement>("Painel_Entrar");
        painelPortIP = root.Q<VisualElement>("Painel_Port-IP");
        painelLobby = root.Q<VisualElement>("Painel_Lobby");
        painelGameUI = root.Q<VisualElement>("Painel_GameUI");
        painelVitoria = root.Q<VisualElement>("Painel_Vitoria");

        campoIP = root.Q<TextField>("IP");
        campoPorta = root.Q<TextField>("Port");

        textoTempo = root.Q<Label>("Tempo_text");
        textoPlacar = root.Q<Label>("Placar_text");
        textoRodada = root.Q<Label>("Rodada_text");
        textoVencedor = root.Q<Label>("TextoVencedor");
        textoPlacarFinal = root.Q<Label>("TextoPlacarFinal");

        botaoClient = root.Q<Button>("Client");
        botaoEntrar = root.Q<Button>("Entrar");
        botaoServer = root.Q<Button>("Server");
        botaoHost = root.Q<Button>("Host");
        botaoExit = root.Q<Button>("Exit");
        botaoVoltar = root.Q<Button>("Voltar");
        botaoVoltarLobby = root.Q<Button>("BotaoVoltarLobby");
        botaoMenuPrincipal = root.Q<Button>("BotaoMenuPrincipal");
    }

    private void RegistrarEventos()
    {
        if (botaoClient != null)
            botaoClient.clicked += AbrirPainelPortIP;

        if (botaoEntrar != null)
            botaoEntrar.clicked += IniciarConexaoCliente;

        if (botaoServer != null)
            botaoServer.clicked += IniciarServidor;

        if (botaoHost != null)
            botaoHost.clicked += IniciarHost;

        if (botaoExit != null)
            botaoExit.clicked += Sair;

        if (botaoVoltar != null)
            botaoVoltar.clicked += VoltarAoMenuEntrar;

        if (botaoVoltarLobby != null)
            botaoVoltarLobby.clicked += VoltarParaLobby;

        if (botaoMenuPrincipal != null)
            botaoMenuPrincipal.clicked += VoltarAoMenuPrincipal;
    }

    private void RemoverEventos()
    {
        if (botaoClient != null)
            botaoClient.clicked -= AbrirPainelPortIP;

        if (botaoEntrar != null)
            botaoEntrar.clicked -= IniciarConexaoCliente;

        if (botaoServer != null)
            botaoServer.clicked -= IniciarServidor;

        if (botaoHost != null)
            botaoHost.clicked -= IniciarHost;

        if (botaoExit != null)
            botaoExit.clicked -= Sair;

        if (botaoVoltar != null)
            botaoVoltar.clicked -= VoltarAoMenuEntrar;

        if (botaoVoltarLobby != null)
            botaoVoltarLobby.clicked -= VoltarParaLobby;

        if (botaoMenuPrincipal != null)
            botaoMenuPrincipal.clicked -= VoltarAoMenuPrincipal;
    }

    private void MostrarMenuInicial()
    {
        DefinirVisibilidade(painelPrincipal, true);
        DefinirVisibilidade(painelEntrar, true);
        DefinirVisibilidade(painelPortIP, false);
        DefinirVisibilidade(painelLobby, false);
        DefinirVisibilidade(painelGameUI, false);
        DefinirVisibilidade(painelVitoria, false);
    }

    private void AbrirPainelPortIP()
    {
        DefinirVisibilidade(painelPortIP, true);
        DefinirVisibilidade(painelEntrar, false);
    }

    private void IniciarConexaoCliente()
    {
        NetworkManager manager = NetworkManager.Singleton;

        if (manager == null)
            return;

        if (manager.NetworkConfig.NetworkTransport is UnityTransport transport)
        {
            string ip = string.IsNullOrWhiteSpace(campoIP?.value)
                ? "127.0.0.1"
                : campoIP.value.Trim();

            ushort porta = 7777;

            if (!string.IsNullOrWhiteSpace(campoPorta?.value) &&
                !ushort.TryParse(campoPorta.value, out porta))
            {
                return;
            }

            transport.SetConnectionData(ip, porta);
        }

        if (manager.StartClient())
            ExibirLobby();
    }

    private void IniciarServidor()
    {
        NetworkManager manager = NetworkManager.Singleton;

        if (manager != null && manager.StartServer())
            ExibirLobby();
    }

    private void IniciarHost()
    {
        NetworkManager manager = NetworkManager.Singleton;

        if (manager != null && manager.StartHost())
            ExibirLobby();
    }

    public void ExibirLobby()
    {
        DefinirVisibilidade(painelPrincipal, false);
        DefinirVisibilidade(painelEntrar, false);
        DefinirVisibilidade(painelPortIP, false);
        DefinirVisibilidade(painelLobby, true);
        DefinirVisibilidade(painelGameUI, false);
        DefinirVisibilidade(painelVitoria, false);
    }
    public void AtualizarHUD(int segundos, int rodada, int mal, int bem)
    {
        Debug.Log(
            $"[HUD] AtualizarHUD chamado: tempo={segundos}, " +
            $"rodada={rodada}, placar={mal}/{bem}"
        );

        if (textoTempo != null)
        {
            int minutos = segundos / 60;
            int segundosRestantes = segundos % 60;
            textoTempo.text = $"{minutos:00}:{segundosRestantes:00}";
        }
        else
        {
            Debug.LogError("[HUD] textoTempo não encontrado!");
        }

        if (textoRodada != null)
            textoRodada.text = rodada.ToString();
        else
            Debug.LogError("[HUD] textoRodada não encontrado!");

        if (textoPlacar != null)
        {
            textoPlacar.text = $"{mal}/{bem}";
            Debug.Log($"[HUD] Label textoPlacar atualizado para {textoPlacar.text}");
        }
        else
        {
            Debug.LogError("[HUD] textoPlacar não encontrado!");
        }
    }


    public void ExibirHUD()
    {
        if (painelPrincipal != null)
            painelPrincipal.style.display = DisplayStyle.None;

        if (painelEntrar != null)
            painelEntrar.style.display = DisplayStyle.None;

        if (painelPortIP != null)
            painelPortIP.style.display = DisplayStyle.None;

        if (painelLobby != null)
            painelLobby.style.display = DisplayStyle.None;

        if (painelVitoria != null)
            painelVitoria.style.display = DisplayStyle.None;

        if (painelGameUI == null)
        {
            UIDocument documento = GetComponent<UIDocument>();

            if (documento != null)
            {
                painelGameUI = documento.rootVisualElement
                    .Q<VisualElement>("Painel_GameUI");
            }
        }

        if (painelGameUI != null)
            painelGameUI.style.display = DisplayStyle.Flex;


    }

    public void ExibirMenuInicial()
    {
        MostrarMenuInicial();
    }

    private void VoltarAoMenuEntrar()
    {
        DefinirVisibilidade(painelPortIP, false);
        DefinirVisibilidade(painelEntrar, true);
    }
    public void ExibirTelaVitoria(string vencedor, int mal, int bem)
    {
        DefinirVisibilidade(painelPrincipal, false);
        DefinirVisibilidade(painelEntrar, false);
        DefinirVisibilidade(painelPortIP, false);
        DefinirVisibilidade(painelLobby, false);
        DefinirVisibilidade(painelGameUI, false);
        DefinirVisibilidade(painelVitoria, true);

        if (textoVencedor != null)
            textoVencedor.text = $"Time vencedor: {vencedor}!";

        if (textoPlacarFinal != null)
            textoPlacarFinal.text = $"Placar final: {mal}/{bem}";

        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void VoltarParaLobby()
    {
        if (LobbyManager.Singleton != null)
            LobbyManager.Singleton.SolicitarVoltaAoLobby();
    }

    private void VoltarAoMenuPrincipal()
    {
        NetworkManager manager = NetworkManager.Singleton;

        if (manager != null && manager.IsListening)
            manager.Shutdown();

        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        MostrarMenuInicial();
    }

    private void DefinirVisibilidade(
        VisualElement elemento,
        bool visivel)
    {
        if (elemento != null)
        {
            elemento.style.display = visivel
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
    private void OnApplicationFocus(bool temFoco)
    {
        if (temFoco &&
            painelGameUI != null &&
            painelGameUI.style.display != DisplayStyle.None)
        {
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    private void Sair()
    {
        Application.Quit();
    }
}
