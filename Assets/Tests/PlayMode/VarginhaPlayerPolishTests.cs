using System.Collections;
using Game.Player;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaPlayerPolishTests
    {
        private GameObject _root;
        private EdelzioTopDownController _player;
        private VarginhaPlayerAttack _attack;

        [SetUp]
        public void Setup()
        {
            Time.timeScale = 1f;
            _root = new GameObject("PlayerPolishTest");
            var player = new GameObject("Edelzio");
            player.transform.SetParent(_root.transform);
            player.AddComponent<CircleCollider2D>().radius = .12f;
            _player = player.AddComponent<EdelzioTopDownController>();
            _attack = player.AddComponent<VarginhaPlayerAttack>();
        }

        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(_root);
            Time.timeScale = 1f;
        }

        private HealthSystem Target(Vector2 position)
        {
            var go = new GameObject("Target");
            go.transform.SetParent(_root.transform);
            go.transform.position = position;
            go.AddComponent<CircleCollider2D>().radius = .15f;
            var target = go.AddComponent<VarginhaCombatTarget>();
            target.SetKind(VarginhaCombatTarget.EnemyKind.MinorManifestation);
            return go.GetComponent<HealthSystem>();
        }

        private IEnumerator FinishAttack()
        {
            float timeout = Time.realtimeSinceStartup + 2f;
            while (_attack.IsAttacking && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.IsFalse(_attack.IsAttacking, "O golpe deve terminar sem prender o jogador.");
        }

        [UnityTest]
        public IEnumerator ComboCyclesThreeStepsThenExpires()
        {
            for (int i = 1; i <= 4; i++)
            {
                Assert.IsTrue(_attack.TryAttack(Vector2.right));
                Assert.AreEqual((i - 1) % 3 + 1, _attack.ComboStep);
                Assert.IsFalse(_player.IsInputLocked);
                yield return FinishAttack();
            }
            yield return new WaitForSeconds(.8f);
            Assert.AreEqual(0, _attack.ComboStep);
            Assert.IsTrue(_attack.TryAttack(Vector2.left));
            Assert.AreEqual(1, _attack.ComboStep);
        }

        [UnityTest]
        public IEnumerator MeleeCannotDamageThroughSolidWall()
        {
            var health = Target(Vector2.right * .85f);
            var wall = new GameObject("Parede");
            wall.transform.SetParent(_root.transform);
            wall.transform.position = Vector2.right * .42f;
            wall.AddComponent<BoxCollider2D>().size = new Vector2(.1f, 2f);
            Physics2D.SyncTransforms();
            Assert.IsTrue(_attack.TryAttack(Vector2.right));
            yield return FinishAttack();
            Assert.AreEqual(100f, health.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator DirectionAndMultipleCollidersProduceOnlyOneHit()
        {
            var front = Target(Vector2.right * .85f);
            front.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one * .2f;
            var behind = Target(Vector2.left * .85f);
            Physics2D.SyncTransforms();
            Assert.IsTrue(_attack.TryAttack(Vector2.right));
            yield return FinishAttack();
            Assert.AreEqual(66f, front.CurrentHealth, .01f);
            Assert.AreEqual(100f, behind.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator DialogueCancelsWindupWithoutDamage()
        {
            var health = Target(Vector2.right * .85f);
            Physics2D.SyncTransforms();
            Assert.IsTrue(_attack.TryAttack(Vector2.right));
            _player.SetInputLocked(true);
            yield return FinishAttack();
            Assert.AreEqual(100f, health.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator DodgeHasBoundedDurationCooldownAndBlocksAttack()
        {
            Assert.IsTrue(_player.TryDodge(Vector2.right));
            Assert.IsTrue(_player.IsDodging);
            Assert.IsFalse(_attack.TryAttack());
            Assert.IsFalse(_player.TryDodge(Vector2.left));
            yield return new WaitForSeconds(.25f);
            Assert.IsFalse(_player.IsDodging);
            Assert.Greater(_player.DodgeCooldownRemaining, 0f);
            Assert.IsFalse(_player.TryDodge(Vector2.left));
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(_player.TryDodge(Vector2.left));
        }

        [Test]
        public void PauseAndDialogueBlockDodgeAndAttack()
        {
            Time.timeScale = 0f;
            Assert.IsFalse(_player.TryDodge(Vector2.up));
            Assert.IsFalse(_attack.TryAttack());
            Time.timeScale = 1f;
            _player.SetInputLocked(true);
            Assert.IsFalse(_player.TryDodge(Vector2.up));
            Assert.IsFalse(_attack.TryAttack());
        }

        [UnityTest]
        public IEnumerator PausingDuringHitstopDoesNotResumeGame()
        {
            Target(Vector2.right * .85f);
            Physics2D.SyncTransforms();
            _attack.TryAttack(Vector2.right);
            float timeout = Time.realtimeSinceStartup + 2f;
            while (Time.timeScale >= .99f && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.Less(Time.timeScale, .99f, "O golpe deve ter causado hitstop.");
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.AreEqual(0f, Time.timeScale);
            _attack.enabled = false;
            Assert.AreEqual(0f, Time.timeScale);
        }
    }
}
