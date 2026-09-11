using System;
using UnityEngine;

namespace Game.Managers
{
    /// <summary>
    /// Singleton central do jogo. Controla o estado global (Menu, Playing, Paused, GameOver).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState { Menu, Playing, Paused, GameOver, Win }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Menu;

        // Events
        public event Action<GameState> OnStateChanged;
        public event Action OnGameStart;
        public event Action OnGamePause;
        public event Action OnGameResume;
        public event Action OnGameOver;
        public event Action OnLevelComplete;

        // Properties
        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsGameOver => currentState == GameState.GameOver;

        // Session data
        private float _sessionStartTime;
        private float _totalPlayTime;
        public float TotalPlayTime => _totalPlayTime + (IsPlaying ? Time.time - _sessionStartTime : 0f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Os builders antigos podiam criar este objeto dentro de uma raiz de fase.
            // Torná-lo raiz antes de persistir elimina o aviso do Unity e preserva o estado global.
            if (transform.parent != null) transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (IsPlaying)
                _totalPlayTime = Time.time - _sessionStartTime;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Public API ───────────────────────────────────────────────────────

        public void StartGame()
        {
            SetState(GameState.Playing);
            _sessionStartTime = Time.time;
            _totalPlayTime = 0f;
            OnGameStart?.Invoke();
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            OnGamePause?.Invoke();
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            SetState(GameState.Playing);
            Time.timeScale = 1f;
            OnGameResume?.Invoke();
        }

        public void TriggerGameOver()
        {
            if (IsGameOver) return;
            SetState(GameState.GameOver);
            OnGameOver?.Invoke();
        }

        public void TriggerWin()
        {
            SetState(GameState.Win);
            OnLevelComplete?.Invoke();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.Menu);
        }

        /// <summary>Reseta o singleton (útil em testes).</summary>
        public static void ResetForTesting()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
