using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    /// <summary>
    /// Gerencia carregamento de cenas com suporte a fade e loading screen.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "SampleScene";
        [SerializeField] private string testScene = "TestScene";
        [SerializeField] private string fase1Scene = "Fase1_deTestes";

        [Header("Loading")]
        [SerializeField] private float minLoadTime = 0.5f;

        private bool _isLoading;
        public bool IsLoading => _isLoading;
        public string CurrentScene => SceneManager.GetActiveScene().name;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (transform.parent != null) transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Public API ───────────────────────────────────────────────────────

        public void LoadMainMenu()   => LoadScene(mainMenuScene);
        public void LoadTestScene()  => LoadScene(testScene);
        public void LoadFase1()      => LoadScene(fase1Scene);

        public void LoadScene(string sceneName)
        {
            if (_isLoading) return;
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void LoadScene(int buildIndex)
        {
            if (_isLoading) return;
            StartCoroutine(LoadSceneAsync(buildIndex));
        }

        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        // ─── Private ──────────────────────────────────────────────────────────

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            _isLoading = true;
            float startTime = Time.realtimeSinceStartup;

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
                yield return null;

            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < minLoadTime)
                yield return new WaitForSecondsRealtime(minLoadTime - elapsed);

            op.allowSceneActivation = true;
            _isLoading = false;
        }

        private IEnumerator LoadSceneAsync(int buildIndex)
        {
            _isLoading = true;
            AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);
            yield return op;
            _isLoading = false;
        }

        /// <summary>Reseta para testes.</summary>
        public static void ResetForTesting()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}
