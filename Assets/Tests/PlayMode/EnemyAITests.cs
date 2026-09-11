using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Enemies;
using Game.Player;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Testa a EnemyAI em PlayMode.
    /// Cobre: estado inicial, patrulha, detecção, chase, ataque.
    /// </summary>
    public class EnemyAITests
    {
        private GameObject _enemyGo;
        private EnemyAI _enemy;
        private HealthSystem _enemyHealth;
        private GameObject _playerGo;
        private HealthSystem _playerHealth;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Player
            _playerGo = new GameObject("Player");
            _playerGo.tag = "Player";
            _playerGo.layer = LayerMask.NameToLayer("Default");
            _playerGo.AddComponent<Rigidbody2D>().gravityScale = 0f;
            _playerGo.AddComponent<CircleCollider2D>();
            _playerHealth = _playerGo.AddComponent<HealthSystem>();
            _playerGo.transform.position = new Vector3(50f, 0f, 0f); // longe inicialmente

            // Patrol points
            var ptA = new GameObject("PatrolA");
            ptA.transform.position = new Vector3(-3f, 0f, 0f);
            var ptB = new GameObject("PatrolB");
            ptB.transform.position = new Vector3(3f, 0f, 0f);

            // Enemy
            _enemyGo = new GameObject("Enemy");
            _enemyGo.transform.position = Vector3.zero;
            var rb = _enemyGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _enemyGo.AddComponent<CircleCollider2D>();
            _enemyHealth = _enemyGo.AddComponent<HealthSystem>();
            _enemy = _enemyGo.AddComponent<EnemyAI>();

            yield return new WaitForSeconds(0.1f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_enemyGo) Object.Destroy(_enemyGo);
            if (_playerGo) Object.Destroy(_playerGo);
            yield return null;
        }

        // ─── Initial State ────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Enemy_StartsInPatrolState()
        {
            yield return null;
            Assert.AreEqual(EnemyAI.EnemyState.Patrol, _enemy.CurrentState,
                "Inimigo deve começar em estado Patrol.");
        }

        [UnityTest]
        public IEnumerator Enemy_HasNoPlayer_Initially()
        {
            yield return null;
            Assert.IsFalse(_enemy.HasPlayer,
                "Inimigo não deve ter player detectado inicialmente.");
        }

        // ─── Detection ────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Enemy_DetectsPlayer_WhenInRange()
        {
            // Coloca player perto do inimigo
            _playerGo.transform.position = new Vector3(2f, 0f, 0f);
            yield return new WaitForSeconds(0.3f);
            // Enemy deve ter mudado para Chase ou Attack
            Assert.AreNotEqual(EnemyAI.EnemyState.Patrol, _enemy.CurrentState,
                "Inimigo deve detectar player próximo e sair do Patrol.");
        }

        [UnityTest]
        public IEnumerator Enemy_GoesToChase_WhenPlayerDetected()
        {
            _enemy.ForceChase(_playerGo.transform);
            yield return null;
            Assert.AreEqual(EnemyAI.EnemyState.Chase, _enemy.CurrentState,
                "ForceChase deve colocar inimigo em Chase.");
        }

        // ─── Patrol ───────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Enemy_ReturnsToPatrol_WhenForced()
        {
            _enemy.ForceChase(_playerGo.transform);
            yield return null;
            _enemy.ForcePatrol();
            yield return null;
            Assert.AreEqual(EnemyAI.EnemyState.Patrol, _enemy.CurrentState,
                "ForcePatrol deve retornar para estado Patrol.");
        }

        // ─── Death ────────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Enemy_Dies_WhenHPZero()
        {
            _enemyHealth.InstantKill();
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(EnemyAI.EnemyState.Dead, _enemy.CurrentState,
                "Inimigo deve ir para Dead quando HP for 0.");
        }

        [UnityTest]
        public IEnumerator Enemy_IsDestroyed_AfterDeath()
        {
            _enemyHealth.InstantKill();
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(_enemyGo == null,
                "Inimigo deve ser destruído após morrer.");
        }

        // ─── Range Getters ────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Enemy_DetectionRange_IsPositive()
        {
            yield return null;
            Assert.Greater(_enemy.DetectionRange, 0f,
                "DetectionRange deve ser positivo.");
        }

        [UnityTest]
        public IEnumerator Enemy_AttackRange_LessThanDetectionRange()
        {
            yield return null;
            Assert.Less(_enemy.AttackRange, _enemy.DetectionRange,
                "AttackRange deve ser menor que DetectionRange.");
        }
    }
}
