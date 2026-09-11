using System;
using UnityEngine;

namespace Game.Managers
{
    /// <summary>
    /// Singleton de pontuação. Rastreia score, highscore e multiplier.
    /// Persiste highscore via PlayerPrefs.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string HighscoreKey = "Highscore";

        [Header("Score Settings")]
        [SerializeField] private int currentScore;
        [SerializeField] private int highscore;
        [SerializeField] private float multiplier = 1f;
        [SerializeField] private float multiplierMax = 8f;

        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnHighscoreBeaten;
        public event Action<float> OnMultiplierChanged;

        // Properties
        public int CurrentScore => currentScore;
        public int Highscore => highscore;
        public float Multiplier => multiplier;

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
            highscore = PlayerPrefs.GetInt(HighscoreKey, 0);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>Adiciona pontos multiplicados pelo multiplier atual.</summary>
        public void AddScore(int basePoints)
        {
            if (basePoints <= 0) return;
            int gained = Mathf.RoundToInt(basePoints * multiplier);
            currentScore += gained;
            OnScoreChanged?.Invoke(currentScore);

            if (currentScore > highscore)
            {
                highscore = currentScore;
                PlayerPrefs.SetInt(HighscoreKey, highscore);
                OnHighscoreBeaten?.Invoke(highscore);
            }
        }

        /// <summary>Define o multiplier (clamped entre 1 e multiplierMax).</summary>
        public void SetMultiplier(float value)
        {
            multiplier = Mathf.Clamp(value, 1f, multiplierMax);
            OnMultiplierChanged?.Invoke(multiplier);
        }

        /// <summary>Aumenta o multiplier em um step.</summary>
        public void IncreaseMultiplier(float step = 0.5f)
        {
            SetMultiplier(multiplier + step);
        }

        /// <summary>Reseta multiplier para 1.</summary>
        public void ResetMultiplier()
        {
            SetMultiplier(1f);
        }

        /// <summary>Reseta score atual para 0.</summary>
        public void ResetScore()
        {
            currentScore = 0;
            multiplier = 1f;
            OnScoreChanged?.Invoke(0);
        }

        /// <summary>Apaga highscore salvo (útil em testes).</summary>
        public void ClearHighscore()
        {
            highscore = 0;
            PlayerPrefs.DeleteKey(HighscoreKey);
        }

        /// <summary>Reseta singleton para testes.</summary>
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
