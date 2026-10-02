using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Singleton;

    private const int MAX_JOGADORES = 4;

    [Header("Prefab do Personagem")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Configuração de Sprites")]
    [SerializeField] private Sprite[] spritesPersonagens;

    public struct DadosJogador : INetworkSerializable, System.IEquatable<DadosJogador>
    {
        public ulong ClientId;
        public int PersonagemIndex;
        public bool IsReady;
        public bool IsOccupied;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PersonagemIndex);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref IsOccupied);
        }

        public bool Equals(DadosJogador other)
        {
            return ClientId == other.ClientId &&
                   PersonagemIndex == other.PersonagemIndex &&
                   IsReady == other.IsReady &&
                   IsOccupied == other.IsOccupied;
        }
    }

    public NetworkList<DadosJogador> slotsJogadores;

    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement painelLobby;

    private VisualElement[] cardSlots = new VisualElement[MAX_JOGADORES];
    private VisualElement[] imgPersonagens = new VisualElement[MAX_JOGADORES];
    private Label[] lblTipos = new Label[MAX_JOGADORES];
    private Button[] btnAnterior = new Button[MAX_JOGADORES];
    private Button[] btnProximo = new Button[MAX_JOGADORES];
    private Button[] btnPronto = new Button[MAX_JOGADORES];

    private Button btnIniciarPartida;
    private Label txtContagemRegressiva;

    private void Awake()
    {
        Singleton = this;
        slotsJogadores = new NetworkList<DadosJogador>();
    }

    public override void OnNetworkSpawn()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null) root = uiDocument.rootVisualElement;

        MapearElementosUI();

        slotsJogadores.OnListChanged += OnSlotsMudaram;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConectou;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDesconectou;

            for (int i = 0; i < MAX_JOGADORES; i++)
            {
                slotsJogadores.Add(new DadosJogador { IsOccupied = false });
            }

            AdicionarJogadorAoLobby(NetworkManager.Singleton.LocalClientId);
        }

        AtualizarUI();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConectou;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDesconectou;
        }
        slotsJogadores.OnListChanged -= OnSlotsMudaram;
    }

    private void MapearElementosUI()
    {
        if (root == null) return;

        painelLobby = root.Q<VisualElement>("Painel_Lobby");
        btnIniciarPartida = root.Q<Button>("BtnIniciarPartida");
        if (btnIniciarPartida != null)
        {
            btnIniciarPartida.clicked -= SolicitarInicioPartida;
            btnIniciarPartida.clicked += SolicitarInicioPartida;
        }
        txtContagemRegressiva = root.Q<Label>("TxtContagemRegressiva");

        for (int i = 0; i < MAX_JOGADORES; i++)
        {
            int index = i;
            VisualElement slot = root.Q<VisualElement>($"Slot_{index}");
            if (slot == null) continue;

            cardSlots[index] = slot;
            imgPersonagens[index] = slot.Q<VisualElement>("ImgPersonagem");
            lblTipos[index] = slot.Q<Label>("LblTipoJogador");

            btnAnterior[index] = slot.Q<Button>("BtnAnterior");
            btnProximo[index] = slot.Q<Button>("BtnProximo");
            btnPronto[index] = slot.Q<Button>("BtnPronto");

            if (btnAnterior[index] != null) btnAnterior[index].clicked += () => TrocarPersonagemRpc(index, -1);
            if (btnProximo[index] != null) btnProximo[index].clicked += () => TrocarPersonagemRpc(index, 1);
            if (btnPronto[index] != null) btnPronto[index].clicked += () => AlternarProntoRpc(index);
        }
    }

    private void OnClientConectou(ulong clientId)
    {
        if (!IsServer) return;
        AdicionarJogadorAoLobby(clientId);
    }

    private void OnClientDesconectou(ulong clientId)
    {
        if (!IsServer) return;
        for (int i = 0; i < slotsJogadores.Count; i++)
        {
            if (slotsJogadores[i].IsOccupied && slotsJogadores[i].ClientId == clientId)
            {
                slotsJogadores[i] = new DadosJogador { IsOccupied = false };
                break;
            }
        }
    }

    private void AdicionarJogadorAoLobby(ulong clientId)
    {
        for (int i = 0; i < slotsJogadores.Count; i++)
        {
            if (slotsJogadores[i].IsOccupied && slotsJogadores[i].ClientId == clientId)
            {
                return;
            }
        }

        for (int i = 0; i < slotsJogadores.Count; i++)
        {
            if (!slotsJogadores[i].IsOccupied)
            {
                slotsJogadores[i] = new DadosJogador
                {
                    ClientId = clientId,
                    PersonagemIndex = 0,
                    IsReady = false,
                    IsOccupied = true
                };
                break;
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void TrocarPersonagemRpc(int slotIndex, int direcao)
    {
        if (slotIndex < 0 || slotIndex >= slotsJogadores.Count) return;

        DadosJogador dados = slotsJogadores[slotIndex];
        if (!dados.IsOccupied) return;

        int novoIndex = dados.PersonagemIndex + direcao;
        if (spritesPersonagens != null && spritesPersonagens.Length > 0)
        {
            if (novoIndex < 0) novoIndex = spritesPersonagens.Length - 1;
            if (novoIndex >= spritesPersonagens.Length) novoIndex = 0;
        }

        dados.PersonagemIndex = novoIndex;
        slotsJogadores[slotIndex] = dados;
    }

    [Rpc(SendTo.Server)]
    private void AlternarProntoRpc(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotsJogadores.Count) return;

        DadosJogador dados = slotsJogadores[slotIndex];
        if (!dados.IsOccupied) return;

        dados.IsReady = !dados.IsReady;
        slotsJogadores[slotIndex] = dados;
    }

    private void OnSlotsMudaram(NetworkListEvent<DadosJogador> changeEvent)
    {
        AtualizarUI();
    }

    private void AtualizarUI()
    {
        bool todosProntos = true;
        int jogadoresAtivos = 0;

        for (int i = 0; i < MAX_JOGADORES; i++)
        {
            if (cardSlots[i] == null) continue;

            if (i < slotsJogadores.Count && slotsJogadores[i].IsOccupied)
            {
                DadosJogador dados = slotsJogadores[i];
                jogadoresAtivos++;

                cardSlots[i].style.display = DisplayStyle.Flex;

                if (lblTipos[i] != null)
                {
                    lblTipos[i].text = (dados.ClientId == NetworkManager.ServerClientId) ? "Host" : "Client";
                }

                if (imgPersonagens[i] != null && spritesPersonagens != null && spritesPersonagens.Length > dados.PersonagemIndex)
                {
                    imgPersonagens[i].style.backgroundImage = new StyleBackground(spritesPersonagens[dados.PersonagemIndex]);
                }

                bool eMeuSlot = (dados.ClientId == NetworkManager.Singleton.LocalClientId);

                if (btnAnterior[i] != null) btnAnterior[i].style.display = eMeuSlot ? DisplayStyle.Flex : DisplayStyle.None;
                if (btnProximo[i] != null) btnProximo[i].style.display = eMeuSlot ? DisplayStyle.Flex : DisplayStyle.None;
                if (btnPronto[i] != null)
                {
                    btnPronto[i].style.display = eMeuSlot ? DisplayStyle.Flex : DisplayStyle.None;
                    btnPronto[i].text = dados.IsReady ? "Pronto!" : "Está Pronto?";
                }

                if (!dados.IsReady) todosProntos = false;
            }
            else
            {
                cardSlots[i].style.display = DisplayStyle.None;
            }
        }

        if (btnIniciarPartida != null)
        {
            btnIniciarPartida.style.display = IsHost ? DisplayStyle.Flex : DisplayStyle.None;

            bool podeIniciar = IsHost && jogadoresAtivos > 0 && (jogadoresAtivos == 1 || todosProntos);
            btnIniciarPartida.SetEnabled(podeIniciar);
        }
    }

    private void SolicitarInicioPartida()
    {
        if (!IsHost) return;
        IniciarContagemRegressivaClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void IniciarContagemRegressivaClientRpc()
    {
        StartCoroutine(RoutineContagemRegressiva());
    }

    private IEnumerator RoutineContagemRegressiva()
    {
        int tempo = 5;
        while (tempo > 0)
        {
            if (txtContagemRegressiva != null)
            {
                txtContagemRegressiva.text = $"A partida começará em: {tempo}...";
            }
            yield return new WaitForSeconds(1f);
            tempo--;
        }

        EsconderUILobbyLocalmente();

        if (IsServer)
        {
            SpawnarTodosOsJogadores();
        }
    }

    private void EsconderUILobbyLocalmente()
    {
        if (painelLobby == null && uiDocument != null)
        {
            painelLobby = uiDocument.rootVisualElement.Q<VisualElement>("Painel_Lobby");
        }

        if (painelLobby != null)
        {
            painelLobby.style.display = DisplayStyle.None;
        }
    }

    private void SpawnarTodosOsJogadores()
    {
        foreach (var slot in slotsJogadores)
        {
            if (slot.IsOccupied && playerPrefab != null)
            {
                Vector3 posSpawn = new Vector3(slot.ClientId * 2f, 1f, 0f);
                GameObject jogadorInstanciado = Instantiate(playerPrefab, posSpawn, Quaternion.identity);

                jogadorInstanciado.GetComponent<NetworkObject>().SpawnWithOwnership(slot.ClientId);
            }
        }

        BloquearCursorClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BloquearCursorClientRpc()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }
}