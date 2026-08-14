using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;

public class ControladorUI : MonoBehaviour
{
    private VisualElement painelPrincipal;
    private VisualElement painelPort_IP;
    private VisualElement painelEntrar;


    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        painelPrincipal = root.Q<VisualElement>("Painel");
        painelEntrar = root.Q<VisualElement>("PainelEntrar");
        painelPort_IP = root.Q<VisualElement>("PainelPort_IP");

        Button Client = root.Q<Button>("Client");
        Button Server = root.Q<Button>("Server");
        Button Host = root.Q<Button>("Host");
        Button Exit = root.Q<Button>("Exit");
        Button Voltar = root.Q<Button>("Voltar");

        if (Client != null) Client.clicked += client;
        if (Server != null) Server.clicked += server;
        if (Host != null) Host.clicked += host;
        if (Exit != null) Exit.clicked += exit;
    }

    void OnDisable()
    {
        VisualElement root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root != null)
        {
            Button Client = root.Q<Button>("Client");
            Button Server = root.Q<Button>("Server");
            Button Host = root.Q<Button>("Host");
            Button Exit = root.Q<Button>("Exit");

            if (Client != null) Client.clicked -= client;
            if (Server != null) Server.clicked -= server;
            if (Host != null) Host.clicked -= host;
            if (Exit != null) Exit.clicked -= exit;
        }
    }

    void client()
    {
        Debug.Log("Client button clicked - Iniciando Cliente...");

        if (NetworkManager.Singleton != null) NetworkManager.Singleton.StartClient();

        abrirpainelPort_IP();
    }

    void server()
    {
        Debug.Log("Server button clicked - Iniciando Servidor Dedicado...");

        if (NetworkManager.Singleton != null) NetworkManager.Singleton.StartServer();

        OcultarPainelPrincipal();
    }

    void host()
    {
        Debug.Log("Host button clicked - Iniciando Host (Servidor + Cliente)...");

        if (NetworkManager.Singleton != null) NetworkManager.Singleton.StartHost();

        OcultarPainelPrincipal();
    }
    void exit ()
    {
        Debug.Log("Exit button clicked - Saindo do jogo...");
        Application.Quit();
    }
    void OcultarPainelPrincipal()
    {
        if (painelPrincipal != null)
        {
            painelPrincipal.style.display = DisplayStyle.None;
        }
    }
    void abrirpainelPort_IP()
    {
        if (painelPort_IP != null)
        {
            painelPort_IP.style.display = DisplayStyle.Flex;
        }
        if (painelEntrar != null)
        {
            painelEntrar.style.display = DisplayStyle.None;
        }
    }
    void OcultarPainelPort_IP()
    {
        if (painelPort_IP != null)
        {
            painelPort_IP.style.display = DisplayStyle.None;
        }
        if (painelEntrar != null)
        {
            painelEntrar.style.display = DisplayStyle.Flex;
        }
    }
}
