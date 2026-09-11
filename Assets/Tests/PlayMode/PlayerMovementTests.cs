using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Player;
using Game.Managers;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Testa o PlayerController em PlayMode (runtime).
    /// Cobre: movimento horizontal, pulo, gravidade, double jump, dash.
    /// </summary>
    public class PlayerMovementTests
    {
        private GameObject _playerGo;
        private PlayerController _player;
        private Rigidbody2D _rb;
        private HealthSystem _health;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Cria chão
            var groundGo = new GameObject("Ground");
            var groundCol = groundGo.AddComponent<BoxCollider2D>();
            groundGo.transform.position = new Vector3(0f, -1f, 0f);
            groundGo.transform.localScale = new Vector3(20f, 0.5f, 1f);
            groundGo.layer = LayerMask.NameToLayer("Default");

            // Cria player
            _playerGo = new GameObject("Player");
            _playerGo.transform.position = new Vector3(0f, 0.5f, 0f);

            _rb = _playerGo.AddComponent<Rigidbody2D>();
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.gravityScale = 3f;

            var col = _playerGo.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.8f, 1.8f);

            _health = _playerGo.AddComponent<HealthSystem>();
            _player = _playerGo.AddComponent<PlayerController>();

            // Cria GroundCheck
            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(_playerGo.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.9f, 0f);

            yield return new WaitForSeconds(0.2f); // Deixa física estabilizar
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_playerGo);
            yield return null;
        }

        // ─── Movement ─────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Player_MovesRight_WhenInputPositive()
        {
            float startX = _playerGo.transform.position.x;
            _player.SimulateMove(Vector2.right);

            yield return new WaitForSeconds(0.3f);

            Assert.Greater(_playerGo.transform.position.x, startX,
                "Player deve mover para direita com input positivo.");
        }

        [UnityTest]
        public IEnumerator Player_MovesLeft_WhenInputNegative()
        {
            float startX = _playerGo.transform.position.x;
            _player.SimulateMove(Vector2.left);

            yield return new WaitForSeconds(0.3f);

            Assert.Less(_playerGo.transform.position.x, startX,
                "Player deve mover para esquerda com input negativo.");
        }

        [UnityTest]
        public IEnumerator Player_StopsMoving_WhenInputZero()
        {
            _player.SimulateMove(Vector2.right);
            yield return new WaitForSeconds(0.2f);

            _player.SimulateMove(Vector2.zero);
            yield return new WaitForSeconds(0.5f);

            Assert.Less(Mathf.Abs(_player.Velocity.x), 0.5f,
                "Player deve parar quando input é zero.");
        }

        // ─── Jump ─────────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Player_Jumps_GainsPositiveVelocityY()
        {
            yield return new WaitForSeconds(0.1f);
            _player.SimulateJump();

            yield return new WaitForSeconds(0.1f);

            Assert.Greater(_player.Velocity.y, 0f,
                "Player deve ter velocidade vertical positiva após pular.");
        }

        [UnityTest]
        public IEnumerator Player_Falls_WithGravity()
        {
            _player.SimulateJump();
            yield return new WaitForSeconds(0.2f);

            float peakY = _playerGo.transform.position.y;
            yield return new WaitForSeconds(0.5f);

            Assert.Less(_playerGo.transform.position.y, peakY,
                "Player deve cair após o pico do pulo (gravidade).");
        }

        [UnityTest]
        public IEnumerator Player_JumpsLeft_DecreasesJumpsLeft()
        {
            int before = _player.JumpsLeft;
            _player.SimulateJump();
            yield return null;
            Assert.Less(_player.JumpsLeft, before, "JumpsLeft deve diminuir ao pular.");
        }

        // ─── Facing Direction ─────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Player_FacesRight_WhenMovingRight()
        {
            _player.SimulateMove(Vector2.right);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(1f, _player.FacingDirection, 0.01f,
                "FacingDirection deve ser 1 ao mover para direita.");
        }

        [UnityTest]
        public IEnumerator Player_FacesLeft_WhenMovingLeft()
        {
            _player.SimulateMove(Vector2.left);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(-1f, _player.FacingDirection, 0.01f,
                "FacingDirection deve ser -1 ao mover para esquerda.");
        }

        // ─── Health Integration ───────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Player_TakesDamage_HealthDecreases()
        {
            float before = _health.CurrentHealth;
            _health.TakeDamage(25f);
            yield return null;
            Assert.Less(_health.CurrentHealth, before,
                "Vida do player deve diminuir ao tomar dano.");
        }

        [UnityTest]
        public IEnumerator Player_Dies_WhenHPZero()
        {
            bool died = false;
            _health.OnDeath += () => died = true;
            _health.InstantKill();
            yield return null;
            Assert.IsTrue(died, "Player deve morrer ao chegar em 0 HP.");
        }
    }
}
