using NUnit.Framework;
using UnityEngine;
using Game.Managers;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Testa o ScoreManager em EditMode.
    /// Cobre: pontuação, highscore, multiplier, reset, eventos.
    /// </summary>
    public class ScoreManagerTests
    {
        private GameObject _go;
        private ScoreManager _score;

        [SetUp]
        public void SetUp()
        {
            ScoreManager.ResetForTesting();
            _go = new GameObject("ScoreManager");
            _score = _go.AddComponent<ScoreManager>();
            _score.ClearHighscore();
        }

        [TearDown]
        public void TearDown()
        {
            ScoreManager.ResetForTesting();
            Object.DestroyImmediate(_go);
        }

        // ─── Initialization ───────────────────────────────────────────────────

        [Test]
        public void Score_StartsAtZero()
        {
            Assert.AreEqual(0, _score.CurrentScore, "Score inicial deve ser 0.");
        }

        [Test]
        public void Multiplier_StartsAtOne()
        {
            Assert.AreEqual(1f, _score.Multiplier, 0.001f,
                "Multiplier inicial deve ser 1.");
        }

        // ─── AddScore ─────────────────────────────────────────────────────────

        [Test]
        public void AddScore_IncreasesScore()
        {
            _score.AddScore(50);
            Assert.AreEqual(50, _score.CurrentScore, "Score deve aumentar corretamente.");
        }

        [Test]
        public void AddScore_Accumulates()
        {
            _score.AddScore(30);
            _score.AddScore(70);
            Assert.AreEqual(100, _score.CurrentScore,
                "Score deve acumular em múltiplas adições.");
        }

        [Test]
        public void AddScore_NegativeValue_IsIgnored()
        {
            _score.AddScore(-50);
            Assert.AreEqual(0, _score.CurrentScore, "Score negativo não deve ser adicionado.");
        }

        [Test]
        public void AddScore_WithMultiplier_AppliesMultiplier()
        {
            _score.SetMultiplier(2f);
            _score.AddScore(100);
            Assert.AreEqual(200, _score.CurrentScore,
                "Score deve ser multiplicado pelo multiplier.");
        }

        // ─── Events ───────────────────────────────────────────────────────────

        [Test]
        public void OnScoreChanged_FiredWhenScoreAdded()
        {
            bool fired = false;
            _score.OnScoreChanged += s => fired = true;
            _score.AddScore(10);
            Assert.IsTrue(fired, "OnScoreChanged deve ser disparado.");
        }

        [Test]
        public void OnHighscoreBeaten_FiredWhenNewHighscore()
        {
            bool fired = false;
            _score.OnHighscoreBeaten += h => fired = true;
            _score.AddScore(1000);
            Assert.IsTrue(fired, "OnHighscoreBeaten deve ser disparado ao superar highscore.");
        }

        // ─── Multiplier ───────────────────────────────────────────────────────

        [Test]
        public void SetMultiplier_ClampedToMax()
        {
            _score.SetMultiplier(9999f);
            Assert.LessOrEqual(_score.Multiplier, 8f,
                "Multiplier deve ser clamped ao máximo.");
        }

        [Test]
        public void SetMultiplier_ClampedToMin()
        {
            _score.SetMultiplier(-5f);
            Assert.GreaterOrEqual(_score.Multiplier, 1f,
                "Multiplier deve ser no mínimo 1.");
        }

        [Test]
        public void IncreaseMultiplier_IncreasesValue()
        {
            _score.IncreaseMultiplier(1f);
            Assert.AreEqual(2f, _score.Multiplier, 0.001f,
                "Multiplier deve aumentar corretamente.");
        }

        [Test]
        public void ResetMultiplier_SetsToOne()
        {
            _score.SetMultiplier(4f);
            _score.ResetMultiplier();
            Assert.AreEqual(1f, _score.Multiplier, 0.001f,
                "Multiplier deve voltar para 1 ao resetar.");
        }

        // ─── Reset ───────────────────────────────────────────────────────────

        [Test]
        public void ResetScore_SetsScoreToZero()
        {
            _score.AddScore(500);
            _score.ResetScore();
            Assert.AreEqual(0, _score.CurrentScore, "Score deve ser 0 após reset.");
        }

        [Test]
        public void ResetScore_ResetsMultiplier()
        {
            _score.SetMultiplier(4f);
            _score.ResetScore();
            Assert.AreEqual(1f, _score.Multiplier, 0.001f,
                "Multiplier deve voltar para 1 após reset de score.");
        }

        // ─── Highscore ────────────────────────────────────────────────────────

        [Test]
        public void Highscore_UpdatesWhenExceeded()
        {
            _score.AddScore(100);
            Assert.GreaterOrEqual(_score.Highscore, 100,
                "Highscore deve ser atualizado ao superar.");
        }

        [Test]
        public void Highscore_DoesNotDecrease_OnReset()
        {
            _score.AddScore(300);
            int hs = _score.Highscore;
            _score.ResetScore();
            Assert.AreEqual(hs, _score.Highscore,
                "Highscore não deve diminuir ao resetar score.");
        }
    }
}
