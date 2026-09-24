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

        private VarginhaStudentAlly Student(string name)
        {
            foreach (var ally in _squad.Allies)
                if (ally.StudentName == name) return ally;
            Assert.Fail("Aluno não encontrado: " + name);
            return null;
        }

        private void Wall(Vector2 position, Vector2 size)
        {
            var wall = new GameObject("Wall");
            wall.transform.SetParent(_root.transform);
            wall.transform.position = position;
            wall.AddComponent<BoxCollider2D>().size = size;
        }

        [TearDown]
        public void Cleanup()
        {
            Time.timeScale = 1f;
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [UnityTest]
        public IEnumerator DeathCancelsPendingAllyAndPlayerAttacksWhileTimeIsPaused()
        {
            CreateSquad();
            var health = _leader.gameObject.AddComponent<HealthSystem>();
            var playerAttack = _leader.gameObject.AddComponent<VarginhaPlayerAttack>();
            var target = Enemy(Vector2.right * 3f);
            var ally = Student("Fabio");
            Physics2D.SyncTransforms();
            Assert.IsTrue(ally.TryManualAttack(target));
            Assert.IsTrue(playerAttack.TryAttack(Vector2.right));
            health.InstantKill();
            Time.timeScale = 0f;
            yield return null;
            yield return null;
            Assert.IsFalse(ally.CanCommand);
            Assert.IsFalse(Student("Yasmin").TryManualAttack(target));
            Assert.IsNull(ally.CurrentTarget);
            Assert.IsNull(GameObject.Find("Animacao_Fabio"));
            Assert.IsFalse(playerAttack.IsAttacking);
            Assert.AreEqual(0f, Time.timeScale);
            Time.timeScale = 1f;
            yield return new WaitForSeconds(1.4f);
            Assert.AreEqual(100f, target.GetComponent<HealthSystem>().CurrentHealth);
        }

        [UnityTest]
        public IEnumerator DisabledAllyCancelsPresentationWithoutLeavingLateDamage()
        {
            CreateSquad();
            var target = Enemy(Vector2.right * 3f);
            var ally = Student("Fabio");
            Physics2D.SyncTransforms();
            Assert.IsTrue(ally.TryManualAttack(target));
            ally.enabled = false;
            yield return new WaitForSeconds(1.4f);
            Assert.IsNull(ally.CurrentTarget);
            Assert.IsNull(GameObject.Find("Animacao_Fabio"));
            Assert.AreEqual(100f, target.GetComponent<HealthSystem>().CurrentHealth);
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
            Assert.IsTrue(_squad.SelectStudent(0));
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
        public void AlliedSummonsAreGatedUntilHardPhasesButFinalPhaseAlwaysAllowsAll()
        {
            var previous = VarginhaDifficulty.Selected;
            try
            {
                VarginhaDifficulty.Select(InvestigationDifficulty.Easy);
                Assert.AreEqual(0, VarginhaDifficulty.AlliedSummonCount(2));
                VarginhaDifficulty.Select(InvestigationDifficulty.Medium);
                Assert.AreEqual(0, VarginhaDifficulty.AlliedSummonCount(2));
                VarginhaDifficulty.Select(InvestigationDifficulty.Hard);
                Assert.AreEqual(3, VarginhaDifficulty.AlliedSummonCount(2));
                Assert.AreEqual(9, VarginhaDifficulty.AlliedSummonCount(2, true));
                Assert.AreEqual(9, VarginhaDifficulty.AlliedSummonCount(3));
            }
            finally
            {
                VarginhaDifficulty.Select(previous);
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
            StringAssert.Contains("bolada de vôlei", VarginhaStudentAlly.DescribeAttack("Marcos"));
            Assert.AreEqual(7, (int)VarginhaStudentAllyStyle.Support, "Cenas antigas preservam o enum serializado.");
            Assert.AreEqual(8, (int)VarginhaStudentAllyStyle.Volleyball);
        }

        [UnityTest]
        public IEnumerator MarcosSpikesOnceAndSplashRespectsRange()
        {
            CreateSquad();
            var target = Enemy(new Vector2(3f, 0f));
            var nearby = Enemy(new Vector2(3.8f, .5f));
            var distant = Enemy(new Vector2(5.5f, 0f));
            var marcos = Student("Marcos");
            Physics2D.SyncTransforms();
            Assert.AreEqual(VarginhaStudentAllyStyle.Volleyball, marcos.AttackStyle);
            Assert.IsTrue(marcos.TryManualAttack(target));
            yield return new WaitForSeconds(.2f);
            Assert.AreEqual(100f, target.GetComponent<HealthSystem>().CurrentHealth, "A cortada precisa de antecipação.");
            var ball = GameObject.Find("Bola_de_volei");
            Assert.IsNotNull(ball, "Marcos deve apresentar uma bola de vôlei própria.");
            yield return new WaitForSeconds(1.3f);
            Assert.AreEqual(66f, target.GetComponent<HealthSystem>().CurrentHealth, .01f);
            Assert.AreEqual(79.6f, nearby.GetComponent<HealthSystem>().CurrentHealth, .01f);
            Assert.AreEqual(100f, distant.GetComponent<HealthSystem>().CurrentHealth, .01f);
            yield return new WaitForSeconds(.6f);
            Assert.AreEqual(66f, target.GetComponent<HealthSystem>().CurrentHealth, .01f, "Um comando não repete o dano.");
            Assert.IsFalse(marcos.IsReadyForManualAttack);
        }

        [UnityTest]
        public IEnumerator AreaImpactCannotCrossAWallBetweenTwoVisibleTargets()
        {
            CreateSquad();
            var target = Enemy(new Vector2(3f, -.6f));
            var blocked = Enemy(new Vector2(3f, .6f));
            Wall(new Vector2(3f, 0f), new Vector2(1f, .2f));
            Physics2D.SyncTransforms();
            var marcos = Student("Marcos");
            Assert.IsTrue(marcos.CanReachTarget(target));
            Assert.IsTrue(marcos.CanReachTarget(blocked), "Os dois ETs são visíveis ao jogador pelos lados da parede.");
            Assert.IsFalse(marcos.CanChainToTarget(target.transform.position, blocked));
            Assert.IsTrue(marcos.TryManualAttack(target));
            yield return new WaitForSeconds(1.4f);
            Assert.Less(target.GetComponent<HealthSystem>().CurrentHealth, 100f);
            Assert.AreEqual(100f, blocked.GetComponent<HealthSystem>().CurrentHealth);
        }

        [UnityTest]
        public IEnumerator AnnaRicochetsAcrossTwoNewTargetsWithoutRepeatingDamage()
        {
            CreateSquad();
            var first = Enemy(Vector2.right * 2f);
            var second = Enemy(Vector2.right * 4f);
            var third = Enemy(Vector2.right * 6f);
            Physics2D.SyncTransforms();
            Assert.IsTrue(Student("Anna Sabia").TryManualAttack(first));
            yield return new WaitForSeconds(1.4f);
            Assert.AreEqual(77f, first.GetComponent<HealthSystem>().CurrentHealth, .01f);
            Assert.AreEqual(82.75f, second.GetComponent<HealthSystem>().CurrentHealth, .01f);
            Assert.AreEqual(87.0625f, third.GetComponent<HealthSystem>().CurrentHealth, .01f);
        }

        [UnityTest]
        public IEnumerator KatanaPiercesOnlyTargetsBehindItsAim()
        {
            CreateSquad();
            var target = Enemy(Vector2.right * 2f);
            var behind = Enemy(Vector2.right * 4f);
            var beside = Enemy(new Vector2(3f, 1f));
            Physics2D.SyncTransforms();
            Assert.IsTrue(Student("Fabio").TryManualAttack(target));
            yield return new WaitForSeconds(1.4f);
            Assert.AreEqual(62f, target.GetComponent<HealthSystem>().CurrentHealth, .01f);
            Assert.AreEqual(71.5f, behind.GetComponent<HealthSystem>().CurrentHealth, .01f);
            Assert.AreEqual(100f, beside.GetComponent<HealthSystem>().CurrentHealth, .01f);
        }

        [UnityTest]
        public IEnumerator MicrophoneRestoresSanityOnlyOnSuccessfulImpact()
        {
            CreateSquad();
            _leader.DrainSanity(40f);
            var target = Enemy(Vector2.right * 3f);
            Physics2D.SyncTransforms();
            Assert.IsTrue(Student("Luis Miguel Messias").TryManualAttack(target));
            Assert.AreEqual(60f, _leader.CurrentSanity);
            yield return new WaitForSeconds(1.4f);
            Assert.AreEqual(72f, _leader.CurrentSanity, .01f);
            Assert.AreEqual(74f, target.GetComponent<HealthSystem>().CurrentHealth, .01f);
        }

        [UnityTest]
        public IEnumerator InputLockFreezesPendingPowerAndItsManualCooldown()
        {
            CreateSquad();
            var target = Enemy(Vector2.right * 3f);
            var marcos = Student("Marcos");
            Physics2D.SyncTransforms();
            Assert.IsTrue(marcos.TryManualAttack(target));
            yield return new WaitForSeconds(.15f);
            _leader.SetInputLocked(true);
            float remaining = marcos.ManualCooldownRemaining;
            yield return new WaitForSeconds(.9f);
            Assert.AreEqual(100f, target.GetComponent<HealthSystem>().CurrentHealth);
            Assert.AreEqual(remaining, marcos.ManualCooldownRemaining, .001f);
            _leader.SetInputLocked(false);
            yield return new WaitForSeconds(1.3f);
            Assert.AreEqual(66f, target.GetComponent<HealthSystem>().CurrentHealth, .01f);
        }
    }
}
