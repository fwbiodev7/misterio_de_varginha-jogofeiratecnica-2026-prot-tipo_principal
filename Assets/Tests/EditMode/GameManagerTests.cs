using NUnit.Framework;
using UnityEngine;
using Game.Managers;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Testa o GameManager: estados, transições, eventos, singleton.
    /// </summary>
    public class GameManagerTests
    {
        private GameObject _go;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            GameManager.ResetForTesting();
            _go = new GameObject("GameManager");
            _gm = _go.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            GameManager.ResetForTesting();
            if (_go != null) Object.DestroyImmediate(_go);
        }

        // ─── Singleton ────────────────────────────────────────────────────────

        [Test]
        public void Singleton_IsSet_AfterAwake()
        {
            Assert.IsNotNull(GameManager.Instance, "Singleton deve existir após criação.");
        }

        [Test]
        public void Singleton_OnlyOneInstance()
        {
            var go2 = new GameObject("GM2");
            var gm2 = go2.AddComponent<GameManager>();
            // O segundo deve se destruir
            Assert.AreSame(_gm, GameManager.Instance, "Apenas um singleton deve existir.");
            Object.DestroyImmediate(go2);
        }

        // ─── Initial State ────────────────────────────────────────────────────

        [Test]
        public void InitialState_IsMenu()
        {
            Assert.AreEqual(GameManager.GameState.Menu, _gm.CurrentState,
                "Estado inicial deve ser Menu.");
        }

        [Test]
        public void IsPlaying_FalseInitially()
        {
            Assert.IsFalse(_gm.IsPlaying, "IsPlaying deve ser false no estado Menu.");
        }

        // ─── StartGame ────────────────────────────────────────────────────────

        [Test]
        public void StartGame_SetsPlayingState()
        {
            _gm.StartGame();
            Assert.AreEqual(GameManager.GameState.Playing, _gm.CurrentState,
                "StartGame deve mudar estado para Playing.");
        }

        [Test]
        public void StartGame_IsPlayingTrue()
        {
            _gm.StartGame();
            Assert.IsTrue(_gm.IsPlaying, "IsPlaying deve ser true após StartGame.");
        }

        [Test]
        public void OnGameStart_EventFired()
        {
            bool fired = false;
            _gm.OnGameStart += () => fired = true;
            _gm.StartGame();
            Assert.IsTrue(fired, "OnGameStart deve ser disparado.");
        }

        // ─── Pause / Resume ───────────────────────────────────────────────────

        [Test]
        public void PauseGame_SetsPausedState()
        {
            _gm.StartGame();
            _gm.PauseGame();
            Assert.AreEqual(GameManager.GameState.Paused, _gm.CurrentState,
                "PauseGame deve mudar estado para Paused.");
        }

        [Test]
        public void PauseGame_SetsTimeScaleZero()
        {
            _gm.StartGame();
            _gm.PauseGame();
            Assert.AreEqual(0f, Time.timeScale, 0.001f,
                "TimeScale deve ser 0 quando pausado.");
        }

        [Test]
        public void ResumeGame_SetsPlayingState()
        {
            _gm.StartGame();
            _gm.PauseGame();
            _gm.ResumeGame();
            Assert.AreEqual(GameManager.GameState.Playing, _gm.CurrentState,
                "ResumeGame deve retornar para Playing.");
        }

        [Test]
        public void ResumeGame_RestoresTimeScale()
        {
            _gm.StartGame();
            _gm.PauseGame();
            _gm.ResumeGame();
            Assert.AreEqual(1f, Time.timeScale, 0.001f,
                "TimeScale deve voltar para 1 ao retomar.");
        }

        [Test]
        public void PauseGame_OnlyFromPlayingState()
        {
            // Tenta pausar sem estar jogando
            _gm.PauseGame();
            Assert.AreNotEqual(GameManager.GameState.Paused, _gm.CurrentState,
                "PauseGame deve ser ignorado se não estiver Playing.");
        }

        // ─── GameOver ─────────────────────────────────────────────────────────

        [Test]
        public void TriggerGameOver_SetsGameOverState()
        {
            _gm.StartGame();
            _gm.TriggerGameOver();
            Assert.AreEqual(GameManager.GameState.GameOver, _gm.CurrentState,
                "TriggerGameOver deve mudar para GameOver.");
        }

        [Test]
        public void OnGameOver_EventFired()
        {
            bool fired = false;
            _gm.OnGameOver += () => fired = true;
            _gm.StartGame();
            _gm.TriggerGameOver();
            Assert.IsTrue(fired, "OnGameOver deve ser disparado.");
        }

        [Test]
        public void TriggerGameOver_Twice_OnlyFiresOnce()
        {
            int count = 0;
            _gm.OnGameOver += () => count++;
            _gm.StartGame();
            _gm.TriggerGameOver();
            _gm.TriggerGameOver(); // deve ser ignorado
            Assert.AreEqual(1, count, "OnGameOver deve disparar apenas uma vez.");
        }

        // ─── Win ─────────────────────────────────────────────────────────────

        [Test]
        public void TriggerWin_SetsWinState()
        {
            _gm.StartGame();
            _gm.TriggerWin();
            Assert.AreEqual(GameManager.GameState.Win, _gm.CurrentState,
                "TriggerWin deve mudar para Win.");
        }

        [Test]
        public void OnLevelComplete_EventFired()
        {
            bool fired = false;
            _gm.OnLevelComplete += () => fired = true;
            _gm.StartGame();
            _gm.TriggerWin();
            Assert.IsTrue(fired, "OnLevelComplete deve ser disparado.");
        }

        // ─── ReturnToMenu ─────────────────────────────────────────────────────

        [Test]
        public void ReturnToMenu_SetsMenuState()
        {
            _gm.StartGame();
            _gm.ReturnToMenu();
            Assert.AreEqual(GameManager.GameState.Menu, _gm.CurrentState,
                "ReturnToMenu deve voltar para Menu.");
        }

        [Test]
        public void ReturnToMenu_RestoresTimeScale()
        {
            _gm.StartGame();
            _gm.PauseGame();
            _gm.ReturnToMenu();
            Assert.AreEqual(1f, Time.timeScale, 0.001f,
                "TimeScale deve ser 1 ao voltar para o menu.");
        }
    }
}
