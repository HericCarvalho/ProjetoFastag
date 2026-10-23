using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Singleton { get; private set; }

    private const int MAX_JOGADORES = 4;
    private const int JOGADORES_MINIMOS = 2;

    [Header("Prefabs dos personagens")]
    [SerializeField] private GameObject[] prefabsDoBem = new GameObject[5];
    [SerializeField] private GameObject[] prefabsDoMal = new GameObject[5];

    [Header("Sprites dos personagens")]
    [SerializeField] private Sprite[] spritesPersonagens;

    [Header("Configuração das rodadas")]
    [SerializeField, Min(1)] private int duracaoRodadaNormal = 90;
    [SerializeField, Min(1)] private int duracaoRodadaFinal = 60;
    [SerializeField, Min(0)] private float intervaloEntreRodadas = 3f;


    private VisualElement[] imagensPegadores = new VisualElement[2];
    private VisualElement[] imagensVitimas = new VisualElement[2];

    public struct DadosJogador : INetworkSerializable, System.IEquatable<DadosJogador>
    {
        public ulong ClientId;
        public int PersonagemIndex;
        public bool IsReady;
        public bool IsOccupied;
        public bool IsPegador;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PersonagemIndex);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref IsOccupied);
            serializer.SerializeValue(ref IsPegador);
        }

        public bool Equals(DadosJogador other)
        {
            return ClientId == other.ClientId &&
                   PersonagemIndex == other.PersonagemIndex &&
                   IsReady == other.IsReady &&
                   IsOccupied == other.IsOccupied &&
                   IsPegador == other.IsPegador;
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

    private int rodadaAtual;
    private int pontosMal;
    private int pontosBem;
    private bool rodadaEmAndamento;
    private bool partidaFinalizada;
    private bool contagemRegressivaEmAndamento;
    private Coroutine rotinaContagem;
    private Coroutine rotinaCronometro;
    private Coroutine rotinaProximaRodada;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        slotsJogadores = new NetworkList<DadosJogador>();
    }

    public override void OnNetworkSpawn()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument != null ? uiDocument.rootVisualElement : null;
        MapearElementosUI();

        slotsJogadores.OnListChanged += OnSlotsMudaram;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConectou;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDesconectou;

            if (slotsJogadores.Count == 0)
            {
                for (int i = 0; i < MAX_JOGADORES; i++)
                    slotsJogadores.Add(new DadosJogador { IsOccupied = false });
            }

            AdicionarJogadorAoLobby(NetworkManager.Singleton.LocalClientId);
            PlayerMovement.JogadorCapturadoNoServidor += AoCapturarJogador;
        }

        AtualizarUI();
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConectou;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDesconectou;
                PlayerMovement.JogadorCapturadoNoServidor -= AoCapturarJogador;
            }
        }

        slotsJogadores.OnListChanged -= OnSlotsMudaram;

        if (Singleton == this)
            Singleton = null;
    }

    private void MapearElementosUI()
    {
        if (root == null)
            return;

        painelLobby = root.Q<VisualElement>("Painel_Lobby");
        btnIniciarPartida = root.Q<Button>("BtnIniciarPartida");
        txtContagemRegressiva = root.Q<Label>("TxtContagemRegressiva");


        imagensPegadores[0] = root.Q<VisualElement>("Pegador_1");
        imagensPegadores[1] = root.Q<VisualElement>("Pegador_2");

        imagensVitimas[0] = root.Q<VisualElement>("Vitima_1");
        imagensVitimas[1] = root.Q<VisualElement>("Vitima_2");

        if (btnIniciarPartida != null)
        {
            btnIniciarPartida.clicked -= SolicitarInicioPartida;
            btnIniciarPartida.clicked += SolicitarInicioPartida;
        }

        for (int i = 0; i < MAX_JOGADORES; i++)
        {
            int index = i;
            VisualElement slot = root.Q<VisualElement>($"Slot_{index}");
            if (slot == null)
                continue;

            cardSlots[index] = slot;
            imgPersonagens[index] = slot.Q<VisualElement>("ImgPersonagem");
            lblTipos[index] = slot.Q<Label>("LblTipoJogador");
            btnAnterior[index] = slot.Q<Button>("BtnAnterior");
            btnProximo[index] = slot.Q<Button>("BtnProximo");
            btnPronto[index] = slot.Q<Button>("BtnPronto");

            if (btnAnterior[index] != null)
            {
                btnAnterior[index].userData = index;
                btnAnterior[index].clicked += () => SolicitarTrocaPersonagem(-1);
            }

            if (btnProximo[index] != null)
            {
                btnProximo[index].userData = index;
                btnProximo[index].clicked += () => SolicitarTrocaPersonagem(1);
            }

            if (btnPronto[index] != null)
                btnPronto[index].clicked += SolicitarAlternarPronto;
        }
    }

    private void OnClientConectou(ulong clientId)
    {
        if (IsServer && !partidaFinalizada)
            AdicionarJogadorAoLobby(clientId);
    }

    private void OnClientDesconectou(ulong clientId)
    {
        if (!IsServer)
            return;

        for (int i = 0; i < slotsJogadores.Count; i++)
        {
            if (slotsJogadores[i].IsOccupied && slotsJogadores[i].ClientId == clientId)
            {
                slotsJogadores[i] = new DadosJogador { IsOccupied = false };
                break;
            }
        }

        if (rodadaEmAndamento)
            VerificarVitimasRestantes();
    }

    private void AdicionarJogadorAoLobby(ulong clientId)
    {
        for (int i = 0; i < slotsJogadores.Count; i++)
        {
            if (slotsJogadores[i].IsOccupied && slotsJogadores[i].ClientId == clientId)
                return;
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
                    IsOccupied = true,
                    IsPegador = false
                };
                return;
            }
        }
    }

    private void SolicitarTrocaPersonagem(int direcao)
    {
        if (NetworkManager.Singleton == null || partidaFinalizada)
            return;

        TrocarPersonagemServerRpc(direcao);
    }

    [Rpc(SendTo.Server)]
    private void TrocarPersonagemServerRpc(int direcao, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        for (int i = 0; i < slotsJogadores.Count; i++)
        {
            if (!slotsJogadores[i].IsOccupied || slotsJogadores[i].ClientId != clientId)
                continue;

            DadosJogador dados = slotsJogadores[i];
            int quantidadeSprites = spritesPersonagens != null ? spritesPersonagens.Length : 0;
            if (quantidadeSprites == 0)
                return;

            dados.PersonagemIndex = (dados.PersonagemIndex + direcao + quantidadeSprites) % quantidadeSprites;
            dados.IsReady = false;
            slotsJogadores[i] = dados;
            return;
        }
    }

    private void SolicitarAlternarPronto()
    {
        if (NetworkManager.Singleton != null && !partidaFinalizada)
            AlternarProntoServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void AlternarProntoServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        for (int i = 0; i < slotsJogadores.Count; i++)
        {
            if (!slotsJogadores[i].IsOccupied || slotsJogadores[i].ClientId != clientId)
                continue;

            DadosJogador dados = slotsJogadores[i];
            dados.IsReady = !dados.IsReady;
            slotsJogadores[i] = dados;
            return;
        }
    }

    private void OnSlotsMudaram(NetworkListEvent<DadosJogador> changeEvent)
    {
        AtualizarUI();
    }

    private void AtualizarUI()
    {
        if (NetworkManager.Singleton == null)
            return;

        ulong meuId = NetworkManager.Singleton.LocalClientId;
        int jogadoresAtivos = 0;
        bool todosProntos = true;

        for (int i = 0; i < MAX_JOGADORES; i++)
        {
            if (cardSlots[i] == null)
                continue;

            if (i >= slotsJogadores.Count || !slotsJogadores[i].IsOccupied)
            {
                cardSlots[i].style.display = DisplayStyle.None;
                continue;
            }

            DadosJogador dados = slotsJogadores[i];
            jogadoresAtivos++;
            cardSlots[i].style.display = DisplayStyle.Flex;

            if (lblTipos[i] != null)
                lblTipos[i].text = dados.ClientId == NetworkManager.ServerClientId ? "Host" : "Client";

            if (imgPersonagens[i] != null && spritesPersonagens != null &&
                dados.PersonagemIndex >= 0 && dados.PersonagemIndex < spritesPersonagens.Length)
            {
                imgPersonagens[i].style.backgroundImage =
                    new StyleBackground(spritesPersonagens[dados.PersonagemIndex]);
            }

            bool eMeuSlot = dados.ClientId == meuId;
            if (btnAnterior[i] != null) btnAnterior[i].style.display = eMeuSlot ? DisplayStyle.Flex : DisplayStyle.None;
            if (btnProximo[i] != null) btnProximo[i].style.display = eMeuSlot ? DisplayStyle.Flex : DisplayStyle.None;

            if (btnPronto[i] != null)
            {
                btnPronto[i].style.display = eMeuSlot ? DisplayStyle.Flex : DisplayStyle.None;
                btnPronto[i].text = dados.IsReady ? "Pronto!" : "Está pronto?";
            }

            if (!dados.IsReady)
                todosProntos = false;
        }

        if (btnIniciarPartida != null)
        {
            bool ehHost = NetworkManager.Singleton.IsHost;
            bool quantidadeValida = jogadoresAtivos == 1 | jogadoresAtivos == 2 || jogadoresAtivos == 4;
            btnIniciarPartida.style.display = ehHost ? DisplayStyle.Flex : DisplayStyle.None;
            btnIniciarPartida.SetEnabled(
                ehHost && quantidadeValida && todosProntos && !contagemRegressivaEmAndamento);
        }
    }

    private void SolicitarInicioPartida()
    {
        if (!IsServer || contagemRegressivaEmAndamento || partidaFinalizada)
            return;

        int jogadoresAtivos = 0;
        bool todosProntos = true;

        foreach (DadosJogador jogador in slotsJogadores)
        {
            if (!jogador.IsOccupied)
                continue;

            jogadoresAtivos++;
            if (!jogador.IsReady)
                todosProntos = false;
        }

        if (jogadoresAtivos != 1 && jogadoresAtivos != 2 && jogadoresAtivos != 4)
        {
            Debug.LogWarning("A partida precisa de exatamente 1, 2 ou 4 jogadores.");
            return;
        }

        if (!todosProntos)
        {
            Debug.LogWarning("Todos os jogadores precisam estar prontos.");
            return;
        }

        contagemRegressivaEmAndamento = true;
        AtualizarUI();
        IniciarContagemRegressivaClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void IniciarContagemRegressivaClientRpc()
    {
        if (rotinaContagem != null)
            StopCoroutine(rotinaContagem);

        rotinaContagem = StartCoroutine(RoutineContagemRegressiva());
    }

    private IEnumerator RoutineContagemRegressiva()
    {
        for (int tempo = 5; tempo > 0; tempo--)
        {
            if (txtContagemRegressiva != null)
                txtContagemRegressiva.text = $"A partida começará em: {tempo}...";

            yield return new WaitForSeconds(1f);
        }

        if (txtContagemRegressiva != null)
            txtContagemRegressiva.text = string.Empty;

        EsconderUILobbyLocalmente();

        if (IsServer)
            IniciarPartida();
    }

    private void EsconderUILobbyLocalmente()
    {
        if (painelLobby == null && root != null)
            painelLobby = root.Q<VisualElement>("Painel_Lobby");

        if (painelLobby != null)
            painelLobby.style.display = DisplayStyle.None;
    }

    private void IniciarPartida()
    {
        Debug.Log("[PARTIDA] IniciarPartida foi chamado.");

        if (!IsServer)
        {
            Debug.LogWarning("[PARTIDA] Este cliente não é o servidor.");
            return;
        }

        List<int> indicesAtivos = ObterSlotsAtivos();

        Debug.Log($"[PARTIDA] Jogadores ativos: {indicesAtivos.Count}");

        if (indicesAtivos.Count != 1 &&
            indicesAtivos.Count != 2 &&
            indicesAtivos.Count != 4)
        {
            Debug.LogError("[PARTIDA] Quantidade de jogadores inválida.");
            return;
        }

        SortearEquipes(indicesAtivos);

        rodadaAtual = 1;
        pontosMal = 0;
        pontosBem = 0;
        partidaFinalizada = false;

        ExibirHUDClientRpc();
        BloquearCursorClientRpc();

        SpawnarJogadoresDaRodada();
        IniciarCronometro();
    }

    private List<int> ObterSlotsAtivos()
    {
        List<int> indices = new List<int>();

        for (int i = 0; i < slotsJogadores.Count; i++)
        {
            if (slotsJogadores[i].IsOccupied &&
                NetworkManager.Singleton.ConnectedClients.ContainsKey(slotsJogadores[i].ClientId))
            {
                indices.Add(i);
            }
        }

        return indices;
    }

    private void SortearEquipes(List<int> indicesAtivos)
    {
        for (int i = indicesAtivos.Count - 1; i > 0; i--)
        {
            int aleatorio = Random.Range(0, i + 1);
            int temp = indicesAtivos[i];
            indicesAtivos[i] = indicesAtivos[aleatorio];
            indicesAtivos[aleatorio] = temp;
        }

        int quantidadeMal = indicesAtivos.Count / 2;

        for (int i = 0; i < indicesAtivos.Count; i++)
        {
            int slotIndex = indicesAtivos[i];
            DadosJogador dados = slotsJogadores[slotIndex];
            dados.IsPegador = i < quantidadeMal;
            slotsJogadores[slotIndex] = dados;
        }
    }


    private void SpawnarJogadoresDaRodada()
    {
        Debug.Log("[TESTE] SolicitarInicioPartida foi chamado.");

        if (!IsServer)
            return;

        if (!IsServer)
        {
            Debug.LogError("[SPAWN] Este cliente não é o servidor.");
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[SPAWN] NetworkManager.Singleton é nulo.");
            return;
        }

        List<int> slotsAtivos = ObterSlotsAtivos();

        Debug.Log($"[SPAWN] Jogadores conectados: {slotsAtivos.Count}");

        if (slotsAtivos.Count == 0)
        {
            Debug.LogError("[SPAWN] Nenhum jogador conectado foi encontrado.");
            return;
        }

        foreach (int slotIndex in slotsAtivos)
        {

            DadosJogador slot = slotsJogadores[slotIndex];

            Debug.Log(
                $"[SPAWN] Slot={slotIndex}, ClientId={slot.ClientId}, " +
                $"Personagem={slot.PersonagemIndex}, Mal={slot.IsPegador}"
            );

            GameObject[] prefabs = slot.IsPegador
                ? prefabsDoMal
                : prefabsDoBem;

            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogError("[SPAWN] Array de prefabs vazio.");
                continue;
            }

            if (slot.PersonagemIndex < 0 ||
                slot.PersonagemIndex >= prefabs.Length)
            {
                Debug.LogError(
                    $"[SPAWN] Índice de personagem inválido: {slot.PersonagemIndex}"
                );
                continue;
            }

            GameObject prefab = prefabs[slot.PersonagemIndex];

            if (prefab == null)
            {
                Debug.LogError(
                    $"[SPAWN] O prefab do personagem {slot.PersonagemIndex} " +
                    $"do time {(slot.IsPegador ? "Mal" : "Bem")} não foi atribuído."
                );
                continue;
            }

            if (!prefab.TryGetComponent<NetworkObject>(out _))
            {
                Debug.LogError(
                    $"[SPAWN] O prefab '{prefab.name}' não possui NetworkObject."
                );
                continue;
            }

            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(slot.ClientId))
            {
                Debug.LogError(
                    $"[SPAWN] ClientId {slot.ClientId} não está conectado."
                );
                continue;
            }

            GameObject instancia = Instantiate(
                prefab,
                new Vector3(slotIndex * 3f, 1f, 0f),
                Quaternion.identity
            );

            NetworkObject networkObject =
                instancia.GetComponent<NetworkObject>();

            try
            {
                networkObject.SpawnWithOwnership(slot.ClientId);

                PlayerMovement movimento =
                    instancia.GetComponent<PlayerMovement>();

                if (movimento != null)
                {
                    movimento.isPegador.Value = slot.IsPegador;
                    movimento.foiCapturado.Value = false;
                }
                else
                {
                    Debug.LogWarning(
                        $"[SPAWN] '{prefab.name}' não possui PlayerMovement."
                    );
                }

                Debug.Log(
                    $"[SPAWN] SUCESSO: '{prefab.name}' para ClientId {slot.ClientId}."
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"[SPAWN] Falha ao criar '{prefab.name}': {e}"
                );

                if (instancia != null)
                    Destroy(instancia);
            }
        }
    }


    private void IniciarCronometro()
    {
        if (!IsServer)
            return;

        if (rotinaCronometro != null)
            StopCoroutine(rotinaCronometro);

        rodadaEmAndamento = true;
        rotinaCronometro = StartCoroutine(RotinaCronometro());
    }

    private IEnumerator RotinaCronometro()
    {
        int duracao = rodadaAtual == 3 ? duracaoRodadaFinal : duracaoRodadaNormal;
        int segundosRestantes = duracao;

        AtualizarHUDServidor(segundosRestantes);

        while (segundosRestantes > 0 && rodadaEmAndamento)
        {
            yield return new WaitForSeconds(1f);

            if (!rodadaEmAndamento)
                yield break;

            segundosRestantes--;
            AtualizarHUDServidor(segundosRestantes);
        }

        if (rodadaEmAndamento)
            FinalizarRodada(false);
    }

    private void AoCapturarJogador(PlayerMovement jogador)
    {
        if (!IsServer || !rodadaEmAndamento || jogador == null || jogador.isPegador.Value)
            return;

        VerificarVitimasRestantes();
    }

    private void VerificarVitimasRestantes()
    {
        if (!IsServer || !rodadaEmAndamento || NetworkManager.Singleton == null)
            return;

        foreach (NetworkObject objeto in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            PlayerMovement movimento = objeto.GetComponent<PlayerMovement>();
            if (movimento != null && !movimento.isPegador.Value && !movimento.foiCapturado.Value)
                return;
        }

        FinalizarRodada(true);
    }

    private void FinalizarRodada(bool malVenceu)
    {
        if (!IsServer || !rodadaEmAndamento || partidaFinalizada)
            return;

        rodadaEmAndamento = false;

        if (rotinaCronometro != null)
        {
            StopCoroutine(rotinaCronometro);
            rotinaCronometro = null;
        }

        if (malVenceu)
            pontosMal++;
        else
            pontosBem++;

        AtualizarHUDServidor(0);

        if (pontosMal >= 2 || pontosBem >= 2 || rodadaAtual >= 3)
        {
            partidaFinalizada = true;
            return;
        }

        if (rotinaProximaRodada != null)
            StopCoroutine(rotinaProximaRodada);

        rotinaProximaRodada = StartCoroutine(AvancarParaProximaRodada());
    }
    private void AtualizarSpritesEquipesLocalmente()
    {
        if (spritesPersonagens == null)
            return;

        for (int i = 0; i < imagensPegadores.Length; i++)
            ConfigurarImagemEquipe(imagensPegadores[i], null);

        for (int i = 0; i < imagensVitimas.Length; i++)
            ConfigurarImagemEquipe(imagensVitimas[i], null);

        int indicePegador = 0;
        int indiceVitima = 0;

        foreach (DadosJogador jogador in slotsJogadores)
        {
            if (!jogador.IsOccupied)
                continue;

            if (jogador.PersonagemIndex < 0 ||
                jogador.PersonagemIndex >= spritesPersonagens.Length)
                continue;

            Sprite sprite = spritesPersonagens[jogador.PersonagemIndex];

            if (jogador.IsPegador && indicePegador < imagensPegadores.Length)
            {
                ConfigurarImagemEquipe(imagensPegadores[indicePegador], sprite);
                indicePegador++;
            }
            else if (!jogador.IsPegador && indiceVitima < imagensVitimas.Length)
            {
                ConfigurarImagemEquipe(imagensVitimas[indiceVitima], sprite);
                indiceVitima++;
            }
        }
    }

    private void ConfigurarImagemEquipe(VisualElement elemento, Sprite sprite)
    {
        if (elemento == null)
            return;

        if (sprite == null)
        {
            elemento.style.backgroundImage = StyleKeyword.None;
            elemento.style.display = DisplayStyle.None;
            return;
        }

        elemento.style.backgroundImage = new StyleBackground(sprite);
        elemento.style.display = DisplayStyle.Flex;
    }

    private IEnumerator AvancarParaProximaRodada()
    {
        yield return new WaitForSeconds(intervaloEntreRodadas);

        DespawnarJogadores();

        rodadaAtual++;

        yield return null;

        SpawnarJogadoresDaRodada();

        BloquearCursorClientRpc();

        IniciarCronometro();

        rotinaProximaRodada = null;
    }

    private void DespawnarJogadores()
    {
        if (!IsServer || NetworkManager.Singleton == null)
            return;

        List<NetworkObject> jogadoresParaRemover = new List<NetworkObject>();

        foreach (NetworkObject objeto in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (objeto != null && objeto.GetComponent<PlayerMovement>() != null)
                jogadoresParaRemover.Add(objeto);
        }

        foreach (NetworkObject objeto in jogadoresParaRemover)
        {
            if (objeto != null && objeto.IsSpawned)
                objeto.Despawn(true);
        }
    }

    private void AtualizarHUDServidor(int segundos)
    {
        AtualizarHUDClientRpc(segundos, rodadaAtual, pontosMal, pontosBem);
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void AtualizarHUDClientRpc(int segundos,int rodada,int mal,int bem)
    {
        ControladorUI ui = FindFirstObjectByType<ControladorUI>();

        if (ui != null)
            ui.AtualizarHUD(segundos, rodada, mal, bem);

        AtualizarSpritesEquipesLocalmente();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ExibirHUDClientRpc()
    {
        ControladorUI ui = FindFirstObjectByType<ControladorUI>();
        if (ui != null)
            ui.ExibirHUD();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BloquearCursorClientRpc()
    {
        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }
}
