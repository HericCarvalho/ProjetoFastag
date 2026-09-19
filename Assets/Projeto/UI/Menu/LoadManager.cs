using System.Collections;
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
                "ERRO: ProgressBar não encontrada. Confira se o Name é exatamente 'ProgressBar'."
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
        carregamento = SceneManager.LoadSceneAsync(cenaCarregar);

        // Impede a troca automática de cena
        carregamento.allowSceneActivation = false;

        // Carrega até 90%
        while (carregamento.progress < 0.9f)
        {
            float progresso = carregamento.progress / 0.9f;

            float porcentagem = progresso * 100f;

            progressBar.value = porcentagem;

            yield return null;
        }

        // Cena está pronta
        progressFim = true;

        // Completa visualmente a barra
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
        // Só inicia a cena quando os dois terminaram
        if (progressFim && videoFim)
        {
            carregamento.allowSceneActivation = true;
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