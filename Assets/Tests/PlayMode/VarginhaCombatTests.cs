using System.Collections;
using Game.Player;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaCombatTests
    {
        private GameObject _targetObject;

        [TearDown]
        public void TearDown()
        {
            if (_targetObject != null) Object.DestroyImmediate(_targetObject);
        }

        [UnityTest]
        public IEnumerator EdelzioCanMoveDuringAttackAndAttackAgainWithoutCooldown()
        {
            _targetObject = new GameObject("Edelzio_Attack_Test");
            _targetObject.AddComponent<CircleCollider2D>();
            var player = _targetObject.AddComponent<EdelzioTopDownController>();
            var attack = _targetObject.AddComponent<VarginhaPlayerAttack>();
            attack.Configure(5f, true); // Configuração antiga não deve restabelecer a recarga.
            Assert.IsTrue(attack.TryAttack());
            Assert.IsFalse(player.IsInputLocked);
            Assert.AreEqual(0f, attack.CooldownRemaining);
            yield return new WaitForSecondsRealtime(.6f);
            Assert.IsFalse(attack.IsAttacking);
            Assert.IsTrue(attack.TryAttack());
        }

        [Test]
        public void MovementReversesAndStopsInAllEightDirections()
        {
            _targetObject = new GameObject("Edelzio_Movement_Test");
            _targetObject.AddComponent<CircleCollider2D>();
            var player = _targetObject.AddComponent<EdelzioTopDownController>();
            var body = player.GetComponent<Rigidbody2D>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var input = typeof(EdelzioTopDownController).GetField("_moveInput", flags);
            var move = typeof(EdelzioTopDownController).GetMethod("Move", flags);
            foreach (var direction in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) })
            {
                input.SetValue(player, direction.normalized);
                move.Invoke(player, null);
                Assert.Greater(Vector2.Dot(body.linearVelocity, direction.normalized), 4f);
                Assert.AreEqual(4.5f, body.linearVelocity.magnitude, .001f);
            }
            input.SetValue(player, Vector2.zero);
            move.Invoke(player, null);
            Assert.AreEqual(Vector2.zero, body.linearVelocity);
            Assert.AreEqual(0f, player.GetComponent<Collider2D>().sharedMaterial.friction);
        }

        [Test]
        public void AttackAtlasHasFourDirectionsAndSixKeyframes()
        {
            var atlas = Resources.Load<Texture2D>("Varginha/EdelzioAttackV1");
            Assert.IsNotNull(atlas);
            Assert.AreEqual(384, atlas.width);
            Assert.AreEqual(256, atlas.height);
        }

        [UnityTest]
        public IEnumerator MinorManifestationTakesDamageOnlyThroughCombatTarget()
        {
            _targetObject = new GameObject("MinorManifestation_Test");
            _targetObject.AddComponent<BoxCollider2D>();
            var health = _targetObject.AddComponent<HealthSystem>();
            var target = _targetObject.AddComponent<VarginhaCombatTarget>();
            target.SetKind(VarginhaCombatTarget.EnemyKind.MinorManifestation);
            yield return null;
            bool connected = target.ReceiveHit(34f, Vector2.right, .075f);
            Assert.IsTrue(connected);
            Assert.AreEqual(66f, health.CurrentHealth, .001f);
        }

        [UnityTest]
        public IEnumerator AncestralEntityRemainsInvulnerable()
        {
            _targetObject = new GameObject("Entity_Manifestation_Test");
            _targetObject.AddComponent<BoxCollider2D>();
            var health = _targetObject.AddComponent<HealthSystem>();
            var target = _targetObject.AddComponent<VarginhaCombatTarget>();
            target.SetKind(VarginhaCombatTarget.EnemyKind.AncestralEntity);
            yield return null;
            Assert.IsFalse(target.ReceiveHit(999f, Vector2.right, .075f));
            Assert.AreEqual(100f, health.CurrentHealth, .001f);
        }

        [UnityTest]
        public IEnumerator CombatCursorInstantiatesWithAttackAndDetectsTarget()
        {
            _targetObject = new GameObject("Edelzio_Cursor_Test");
            var player = _targetObject.AddComponent<EdelzioTopDownController>();
            var attack = _targetObject.AddComponent<VarginhaPlayerAttack>();
            yield return null;

            var cursor = _targetObject.GetComponent<VarginhaCombatCursor>();
            Assert.IsNotNull(cursor, "VarginhaCombatCursor must be attached automatically to the player attack object.");
            Assert.AreEqual(cursor, VarginhaCombatCursor.Instance);

            var enemyGo = new GameObject("Test_Enemy");
            enemyGo.transform.position = new Vector3(2f, 0f, 0f);
            var col = enemyGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            var combatTarget = enemyGo.AddComponent<VarginhaCombatTarget>();
            yield return null;

            // Invoca a detecção com coordenada próxima ao alvo
            var detectMethod = typeof(VarginhaCombatCursor).GetMethod("DetectTargetAtAim",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var detected = detectMethod.Invoke(cursor, new object[] { new Vector2(2f, 0.2f) });
            Assert.AreEqual(combatTarget, detected, "Cursor must acquire target when hovering near enemy combat target.");

            Object.DestroyImmediate(enemyGo);
        }
    }
}
