
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

    private TextField campoIP;
    private TextField campoPorta;

    private Label textoTempo;
    private Label textoPlacar;
    private Label textoRodada;

    private Button btnClient;
    private Button btnEntrar;
    private Button btnServer;
    private Button btnHost;
    private Button btnExit;
    private Button btnVoltar;

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

        campoIP = root.Q<TextField>("IP");
        campoPorta = root.Q<TextField>("Port");

        textoTempo = root.Q<Label>("time_text");
        textoPlacar = root.Q<Label>("score_text");
        textoRodada = root.Q<Label>("round_text");

        btnClient = root.Q<Button>("Client");
        btnEntrar = root.Q<Button>("Entrar");
        btnServer = root.Q<Button>("Server");
        btnHost = root.Q<Button>("Host");
        btnExit = root.Q<Button>("Exit");
        btnVoltar = root.Q<Button>("Voltar");
    }

    private void RegistrarEventos()
    {
        if (btnClient != null)
            btnClient.clicked += AbrirPainelPortIP;

        if (btnEntrar != null)
            btnEntrar.clicked += IniciarConexaoCliente;

        if (btnServer != null)
            btnServer.clicked += IniciarServidor;

        if (btnHost != null)
            btnHost.clicked += IniciarHost;

        if (btnExit != null)
            btnExit.clicked += Sair;

        if (btnVoltar != null)
            btnVoltar.clicked += VoltarAoMenuEntrar;
    }

    private void RemoverEventos()
    {
        if (btnClient != null)
            btnClient.clicked -= AbrirPainelPortIP;

        if (btnEntrar != null)
            btnEntrar.clicked -= IniciarConexaoCliente;

        if (btnServer != null)
            btnServer.clicked -= IniciarServidor;

        if (btnHost != null)
            btnHost.clicked -= IniciarHost;

        if (btnExit != null)
            btnExit.clicked -= Sair;

        if (btnVoltar != null)
            btnVoltar.clicked -= VoltarAoMenuEntrar;
    }

    private void MostrarMenuInicial()
    {
        DefinirVisibilidade(painelPrincipal, true);
        DefinirVisibilidade(painelEntrar, true);
        DefinirVisibilidade(painelPortIP, false);
        DefinirVisibilidade(painelLobby, false);
        DefinirVisibilidade(painelGameUI, false);
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
            Debug.LogError("[HUD] time_text não encontrado!");
        }

        if (textoRodada != null)
            textoRodada.text = rodada.ToString();
        else
            Debug.LogError("[HUD] round_text não encontrado!");

        if (textoPlacar != null)
        {
            textoPlacar.text = $"{mal}/{bem}";
            Debug.Log($"[HUD] Label score_text atualizado para {textoPlacar.text}");
        }
        else
        {
            Debug.LogError("[HUD] score_text não encontrado!");
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
