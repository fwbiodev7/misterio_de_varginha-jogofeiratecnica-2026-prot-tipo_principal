using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Enemies;
using Game.Managers;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Testa Collectibles em PlayMode.
    /// Cobre: coleta via trigger, incremento de score, destruição do objeto.
    /// </summary>
    public class CollectibleTests
    {
        private GameObject _scoreGo;
        private ScoreManager _scoreManager;
        private GameObject _collectibleGo;
        private Collectible _collectible;
        private GameObject _playerGo;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // ScoreManager
            ScoreManager.ResetForTesting();
            _scoreGo = new GameObject("ScoreManager");
            _scoreManager = _scoreGo.AddComponent<ScoreManager>();
            _scoreManager.ClearHighscore();

            // Player (com Rigidbody2D + Collider + tag)
            _playerGo = new GameObject("Player");
            _playerGo.tag = "Player";
            _playerGo.AddComponent<Rigidbody2D>().gravityScale = 0f;
            var playerCol = _playerGo.AddComponent<CircleCollider2D>();
            playerCol.isTrigger = false;
            _playerGo.transform.position = new Vector3(10f, 0f, 0f); // longe do collectible

            // Collectible
            _collectibleGo = new GameObject("Coin");
            _collectibleGo.AddComponent<Rigidbody2D>().gravityScale = 0f;
            var col = _collectibleGo.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            _collectible = _collectibleGo.AddComponent<Collectible>();
            _collectibleGo.transform.position = Vector3.zero;

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            ScoreManager.ResetForTesting();
            if (_scoreGo) Object.Destroy(_scoreGo);
            if (_playerGo) Object.Destroy(_playerGo);
            if (_collectibleGo) Object.Destroy(_collectibleGo);
            yield return null;
        }

        // ─── Initial State ────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Collectible_NotCollected_Initially()
        {
            yield return null;
            Assert.IsFalse(_collectible.IsCollected,
                "Collectible não deve ser coletado no início.");
        }

        [UnityTest]
        public IEnumerator Collectible_IsActive_Initially()
        {
            yield return null;
            Assert.IsTrue(_collectibleGo.activeSelf,
                "Collectible deve estar ativo no início.");
        }

        // ─── Manual Collection ────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Collectible_Collect_SetsIsCollected()
        {
            _collectible.Collect();
            yield return null;
            Assert.IsTrue(_collectible.IsCollected,
                "IsCollected deve ser true após coletar.");
        }

        [UnityTest]
        public IEnumerator Collectible_Collect_AddsScore()
        {
            int scoreBefore = _scoreManager.CurrentScore;
            _collectible.Collect();
            yield return null;
            Assert.Greater(_scoreManager.CurrentScore, scoreBefore,
                "Coletar deve aumentar o score.");
        }

        [UnityTest]
        public IEnumerator Collectible_Collect_DestroysObject()
        {
            _collectible.Collect();
            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(_collectibleGo == null || !_collectibleGo.activeInHierarchy,
                "Objeto do collectible deve ser destruído após coletar.");
        }

        // ─── Double Collection Prevention ─────────────────────────────────────

        [UnityTest]
        public IEnumerator Collectible_CannotBeCollectedTwice()
        {
            _collectible.Collect();
            int scoreAfterFirst = _scoreManager.CurrentScore;
            _collectible.Collect(); // deve ser ignorado
            yield return null;
            Assert.AreEqual(scoreAfterFirst, _scoreManager.CurrentScore,
                "Segunda coleta não deve adicionar score.");
        }

        // ─── Score Value ──────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Collectible_AddsCorrectScoreValue()
        {
            int expected = _collectible.ScoreValue;
            _collectible.Collect();
            yield return null;
            Assert.AreEqual(expected, _scoreManager.CurrentScore,
                "Score adicionado deve ser igual ao ScoreValue do collectible.");
        }
    }
}
