using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class ControladorUI : MonoBehaviour
{
    private VisualElement painelPrincipal;
    private VisualElement painelPort_IP;
    private VisualElement painelEntrar;
    private VisualElement painelLobby;

    private TextField campoIP;
    private TextField campoPorta;

    void OnEnable()
    {
        UIDocument uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("UIDocument não encontrado no GameObject!");
            return;
        }

        VisualElement root = uiDoc.rootVisualElement;

        painelPrincipal = root.Q<VisualElement>("Painel_Principal");
        painelEntrar = root.Q<VisualElement>("Painel_Entrar");
        painelPort_IP = root.Q<VisualElement>("Painel_Port-IP");
        painelLobby = root.Q<VisualElement>("Painel_Lobby");

        campoIP = root.Q<TextField>("IP");
        campoPorta = root.Q<TextField>("Port");

        Button btnClient = root.Q<Button>("Client");
        Button btnEntrar = root.Q<Button>("Entrar");
        Button btnServer = root.Q<Button>("Server");
        Button btnHost = root.Q<Button>("Host");
        Button btnExit = root.Q<Button>("Exit");
        Button btnVoltar = root.Q<Button>("Voltar");

        if (btnClient != null) btnClient.clicked += AbrirPainelPort_IP;
        if (btnEntrar != null) btnEntrar.clicked += IniciarConexaoCliente;
        if (btnServer != null) btnServer.clicked += IniciarServer;
        if (btnHost != null) btnHost.clicked += IniciarHost;
        if (btnExit != null) btnExit.clicked += Sair;
        if (btnVoltar != null) btnVoltar.clicked += OcultarPainelPort_IP;

        // Garante que no início apenas o painel principal esteja visível
        if (painelLobby != null) painelLobby.style.display = DisplayStyle.None;
    }

    void OnDisable()
    {
        VisualElement root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) return;

        Button btnClient = root.Q<Button>("Client");
        Button btnEntrar = root.Q<Button>("Entrar");
        Button btnServer = root.Q<Button>("Server");
        Button btnHost = root.Q<Button>("Host");
        Button btnExit = root.Q<Button>("Exit");
        Button btnVoltar = root.Q<Button>("Voltar");

        if (btnClient != null) btnClient.clicked -= AbrirPainelPort_IP;
        if (btnEntrar != null) btnEntrar.clicked -= IniciarConexaoCliente;
        if (btnServer != null) btnServer.clicked -= IniciarServer;
        if (btnHost != null) btnHost.clicked -= IniciarHost;
        if (btnExit != null) btnExit.clicked -= Sair;
        if (btnVoltar != null) btnVoltar.clicked -= OcultarPainelPort_IP;
    }

    void AbrirPainelPort_IP()
    {
        if (painelPort_IP != null) painelPort_IP.style.display = DisplayStyle.Flex;
        if (painelEntrar != null) painelEntrar.style.display = DisplayStyle.None;
    }

    void IniciarConexaoCliente()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport transport)
        {
            string ip = string.IsNullOrEmpty(campoIP?.value) ? "127.0.0.1" : campoIP.value;
            ushort porta = 7777;

            if (campoPorta != null && ushort.TryParse(campoPorta.value, out ushort portaDigitada))
            {
                porta = portaDigitada;
            }

            transport.SetConnectionData(ip, porta);
            Debug.Log($"Conectando ao servidor em {ip}:{porta}...");
        }

        if (NetworkManager.Singleton.StartClient())
        {
            ExibirLobby();
        }
    }

    void IniciarServer()
    {
        Debug.Log("Iniciando Servidor Dedicado...");
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartServer())
        {
            ExibirLobby();
        }
    }

    void IniciarHost()
    {
        Debug.Log("Iniciando Host...");
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.StartHost())
        {
            ExibirLobby();
        }
    }

    void ExibirLobby()
    {
        // Esconde telas de menu
        if (painelPrincipal != null) painelPrincipal.style.display = DisplayStyle.None;
        if (painelPort_IP != null) painelPort_IP.style.display = DisplayStyle.None;
        if (painelEntrar != null) painelEntrar.style.display = DisplayStyle.None;

        // Tenta reconectar a referência caso tenha sido perdida
        if (painelLobby == null)
        {
            UIDocument uiDoc = GetComponent<UIDocument>();
            if (uiDoc != null) painelLobby = uiDoc.rootVisualElement.Q<VisualElement>("Painel_Lobby");
        }

        // Exibe o painel do Lobby
        if (painelLobby != null)
        {
            painelLobby.style.display = DisplayStyle.Flex;
            Debug.Log("Painel_Lobby exibido com sucesso!");
        }
        else
        {
            Debug.LogError("ERRO: Não foi possível encontrar o elemento 'Painel_Lobby' no UXML!");
        }
    }

    void Sair()
    {
        Application.Quit();
    }

    void OcultarPainelPort_IP()
    {
        if (painelPort_IP != null) painelPort_IP.style.display = DisplayStyle.None;
        if (painelEntrar != null) painelEntrar.style.display = DisplayStyle.Flex;
    }
}