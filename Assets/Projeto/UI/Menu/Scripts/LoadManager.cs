using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class LoadManager : MonoBehaviour
{
    [SerializeField] private string cenaCarregar;
    [SerializeField] private VideoPlayer videoPlayer;

    private AsyncOperation carregamento;

    private bool videoFim = false;
    private bool progressFim = false;

    private ProgressBar progressBar;


    private void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError(
                "ERRO: UIDocument não está no mesmo GameObject que o LoadManager."
            );
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        progressBar = root.Q<ProgressBar>("Barra");

        if (progressBar == null)
        {
            Debug.LogError(
                "ERRO: ProgressBar não encontrada. Confira se o Name é exatamente 'Barra'."
            );
            return;
        }

        if (videoPlayer == null)
        {
            Debug.LogError(
                "ERRO: VideoPlayer está NULL no LoadManager."
            );
            return;
        }

        progressBar.value = 0;

        videoPlayer.loopPointReached += VideoFim;

        StartCoroutine(CarregarCena());
    }
    private IEnumerator CarregarCena()
    {
        float tempoDecorrido = 0f;
        float duracaoEstimada = (float)videoPlayer.length;
        if (duracaoEstimada <= 0f) duracaoEstimada = 4f;

        while (tempoDecorrido < duracaoEstimada && !videoFim)
        {
            tempoDecorrido += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempoDecorrido / duracaoEstimada) * 100f;
            progressBar.value = progresso;
            yield return null;
        }

        progressFim = true;
        progressBar.value = 100f;

        VerificarFim();
    }
    private void VideoFim(VideoPlayer player)
    {
        videoFim = true;

        VerificarFim();
    }
    private void VerificarFim()
    {
        if (progressFim && videoFim)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(cenaCarregar, LoadSceneMode.Single);
            }
            else if (NetworkManager.Singleton == null)
            {
                SceneManager.LoadScene(cenaCarregar);
            }
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= VideoFim;
        }
    }
}