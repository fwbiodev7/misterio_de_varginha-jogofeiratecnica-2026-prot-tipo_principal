using Game.Varginha;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Player;

namespace Game.Tests.PlayMode
{
    public class VarginhaStudentAllyTests
    {
        private GameObject _root;
        private EdelzioTopDownController _leader;
        private VarginhaStudentAllySquad _squad;

        private void CreateSquad()
        {
            _root = new GameObject("TacticalTests");
            var player = new GameObject("Leader");
            player.transform.SetParent(_root.transform);
            player.AddComponent<CircleCollider2D>().radius = .2f;
            _leader = player.AddComponent<EdelzioTopDownController>();
            _squad = VarginhaStudentAllySquad.BuildForFuturePhase(_root.transform, player.transform, false);
            _squad.ActivateManualAllies();
        }

        private VarginhaCombatTarget Enemy(Vector2 position)
        {
            var enemy = new GameObject("TestEnemy");
            enemy.transform.SetParent(_root.transform);
            enemy.transform.position = position;
            enemy.AddComponent<CircleCollider2D>().radius = .2f;
            return enemy.AddComponent<VarginhaCombatTarget>();
        }

        [TearDown]
        public void Cleanup()
        {
            Time.timeScale = 1f;
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [UnityTest]
        public IEnumerator IndividualCooldownBlocksRepeatButLeavesOtherStudentsReady()
        {
            CreateSquad();
            var enemy = Enemy(Vector2.right * 3f);
            Physics2D.SyncTransforms();
            var ally = _squad.Allies[0];
            Assert.IsTrue(ally.TryManualAttack(enemy));
            Assert.AreEqual(5f, ally.ManualCooldownRemaining, .001f);
            Assert.IsFalse(ally.TryManualAttack(enemy));
            Assert.IsTrue(_squad.Allies[1].IsReadyForManualAttack);
            yield return new WaitForSeconds(4.8f);
            Assert.IsFalse(ally.IsReadyForManualAttack);
            yield return new WaitForSeconds(.3f);
            Assert.IsTrue(ally.IsReadyForManualAttack);
        }

        [Test]
        public void MouseAimOverridesNearThreatAndWallsBlockCommands()
        {
            CreateSquad();
            Enemy(Vector2.left * 2f);
            var aimed = Enemy(Vector2.right * 4f);
            Physics2D.SyncTransforms();
            Assert.IsTrue(_squad.TryInvokeAttack(aimed.transform.position));
            bool selectedAim = false;
            foreach (var ally in _squad.Allies) selectedAim |= ally.CurrentTarget == aimed;
            Assert.IsTrue(selectedAim);
            var wall = new GameObject("Wall");
            wall.transform.SetParent(_root.transform);
            wall.transform.position = Vector2.right * 2f;
            wall.AddComponent<BoxCollider2D>().size = new Vector2(.3f, 3f);
            Physics2D.SyncTransforms();
            Assert.IsFalse(_squad.Allies[1].CanReachTarget(aimed));
        }

        [Test]
        public void NoTargetOrLockedPlayerDoesNotSpendCooldown()
        {
            CreateSquad();
            Assert.IsFalse(_squad.TryInvokeNextAttack());
            var enemy = Enemy(Vector2.right * 3f);
            _leader.SetInputLocked(true);
            Assert.IsFalse(_squad.Allies[0].TryManualAttack(enemy));
            Assert.AreEqual(0f, _squad.Allies[0].ManualCooldownRemaining);
        }

        [Test]
        public void EveryRescuedStudentHasAnAllyProfile()
        {
            foreach (var student in VarginhaPhase2Controller.StudentNames)
            {
                Assert.IsTrue(VarginhaStudentAlly.HasProfile(student), $"Perfil ausente para {student}");
                Assert.IsNotEmpty(VarginhaStudentAlly.DescribeAttack(student), $"Golpe ausente para {student}");
            }
        }

        [Test]
        public void SignatureStudentsKeepTheirRequestedAttacks()
        {
            StringAssert.Contains("jiujitsu", VarginhaStudentAlly.DescribeAttack("Matias"));
            StringAssert.Contains("ping-pong", VarginhaStudentAlly.DescribeAttack("Anna Sabia"));
            StringAssert.Contains("guitarra", VarginhaStudentAlly.DescribeAttack("Pedro"));
            StringAssert.Contains("microfone", VarginhaStudentAlly.DescribeAttack("Luis Miguel Messias"));
            StringAssert.Contains("katana", VarginhaStudentAlly.DescribeAttack("Fabio"));
            StringAssert.Contains("piano", VarginhaStudentAlly.DescribeAttack("Yasmin"));
        }
    }
}
