using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Player;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Testa física 2D em PlayMode.
    /// Cobre: colisão, triggers, Rigidbody2D, RaycastHit2D, overlap.
    /// </summary>
    public class PhysicsTests
    {
        private GameObject _groundGo;
        private GameObject _playerGo;
        private Rigidbody2D _rb;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Chão
            _groundGo = new GameObject("Ground");
            _groundGo.AddComponent<BoxCollider2D>().size = new Vector2(20f, 1f);
            _groundGo.transform.position = new Vector3(0f, -5f, 0f);

            // Corpo físico
            _playerGo = new GameObject("PhysicsBody");
            _playerGo.transform.position = new Vector3(0f, 2f, 0f);
            _rb = _playerGo.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 5f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = _playerGo.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            yield return new WaitForSeconds(0.5f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_playerGo);
            Object.Destroy(_groundGo);
            yield return null;
        }

        // ─── Gravity ──────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Rigidbody2D_FallsWithGravity()
        {
            float startY = _playerGo.transform.position.y;
            yield return new WaitForSeconds(0.3f);
            Assert.Less(_playerGo.transform.position.y, startY,
                "Rigidbody2D deve cair com gravidade.");
        }

        [UnityTest]
        public IEnumerator Rigidbody2D_CollidesWith_StaticGround()
        {
            yield return new WaitForSeconds(2f);
            // Deve ter parado perto do chão
            Assert.Less(Mathf.Abs(_rb.linearVelocity.y), 0.5f,
                "Corpo deve parar ao colidir com o chão.");
        }

        // ─── Forces ───────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator AddForce_ChangesVelocity()
        {
            _rb.gravityScale = 0f;
            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(Vector2.right * 100f, ForceMode2D.Impulse);
            yield return null;
            Assert.Greater(_rb.linearVelocity.x, 0f,
                "AddForce deve aumentar velocidade na direção correta.");
        }

        [UnityTest]
        public IEnumerator SetVelocity_DirectlyMovesBody()
        {
            _rb.gravityScale = 0f;
            _rb.linearVelocity = new Vector2(5f, 0f);
            yield return new WaitForSeconds(0.2f);
            Assert.Greater(_playerGo.transform.position.x, 0f,
                "Definir velocidade direta deve mover o corpo.");
        }

        // ─── Raycast ──────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Raycast2D_HitsGround_FromAbove()
        {
            yield return new WaitForSeconds(0.1f);
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(0f, 5f), Vector2.down, 20f);
            Assert.IsTrue(hit.collider != null,
                "Raycast de cima para baixo deve detectar o chão.");
        }

        [UnityTest]
        public IEnumerator Raycast2D_MissesGround_FromSide()
        {
            yield return null;
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(0f, 5f), Vector2.right, 5f);
            Assert.IsTrue(hit.collider == null,
                "Raycast horizontal não deve colidir com o chão plano.");
        }

        // ─── OverlapCircle ────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator OverlapCircle_DetectsNearbyCollider()
        {
            yield return new WaitForSeconds(0.1f);
            // Chão em y=-5, detecção em y=-4.5
            Collider2D result = Physics2D.OverlapCircle(new Vector2(0f, -4.5f), 0.6f);
            Assert.IsNotNull(result, "OverlapCircle deve detectar o colisor do chão.");
        }

        [UnityTest]
        public IEnumerator OverlapCircle_ReturnsNull_WhenEmpty()
        {
            yield return null;
            Collider2D result = Physics2D.OverlapCircle(new Vector2(100f, 100f), 0.1f);
            Assert.IsNull(result, "OverlapCircle em área vazia deve retornar null.");
        }

        // ─── Triggers ─────────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Trigger_IsDetectedByOverlapArea()
        {
            var triggerGo = new GameObject("Trigger");
            var triggerCol = triggerGo.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector2(2f, 2f);
            triggerGo.transform.position = new Vector3(50f, 0f, 0f);

            yield return null;

            Collider2D[] results = Physics2D.OverlapAreaAll(
                new Vector2(49f, -1f), new Vector2(51f, 1f));

            bool found = System.Array.Exists(results, c => c == triggerCol);
            Assert.IsTrue(found, "OverlapArea deve detectar trigger.");
            Object.Destroy(triggerGo);
        }

        // ─── Freeze Constraints ───────────────────────────────────────────────

        [UnityTest]
        public IEnumerator FreezeRotation_PreventsRotation()
        {
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.AddForce(Vector2.right * 500f, ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0f, _rb.rotation, 1f,
                "FreezeRotation deve impedir rotação do corpo.");
        }

        [UnityTest]
        public IEnumerator FreezePosition_PreventsMovement()
        {
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            float startX = _playerGo.transform.position.x;
            _rb.AddForce(Vector2.right * 1000f, ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(startX, _playerGo.transform.position.x, 0.01f,
                "FreezeAll deve impedir qualquer movimento.");
        }
    }
}
