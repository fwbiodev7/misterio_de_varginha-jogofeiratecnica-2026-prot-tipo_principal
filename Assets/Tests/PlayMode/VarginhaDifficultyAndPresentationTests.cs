using System.Collections;
using Game.Player;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaDifficultyAndPresentationTests
    {
        private GameObject _root;
        [TearDown]
        public void Cleanup()
        {
            if (_root != null) Object.DestroyImmediate(_root);
            VarginhaDifficulty.Select(InvestigationDifficulty.Medium);
            Time.timeScale=1f;
        }
        [UnityTest]
        public IEnumerator DifficultyScalesEachEnemyOnce()
        {
            _root=new GameObject("Difficulty_Test");
            float previousHealth=0, previousSpeed=0, previousDamage=0;
            foreach (InvestigationDifficulty level in new[] { InvestigationDifficulty.Easy, InvestigationDifficulty.Medium, InvestigationDifficulty.Hard })
            {
                VarginhaDifficulty.Select(level);
                var enemy=new GameObject("ET_Test"); enemy.transform.SetParent(_root.transform);
                enemy.AddComponent<CircleCollider2D>();
                var ai=enemy.AddComponent<VarginhaCombatEnemy>();
                yield return null;
                var health=enemy.GetComponent<HealthSystem>();
                Assert.Greater(health.MaxHealth,previousHealth);
                Assert.Greater(ai.EffectiveSpeed,previousSpeed);
                Assert.Greater(ai.EffectiveDamage,previousDamage);
                float scaledHealth=health.MaxHealth;
                ai.enabled=false; ai.enabled=true;
                yield return null;
                Assert.AreEqual(scaledHealth,health.MaxHealth,"Reativação não deve multiplicar a dificuldade.");
                previousHealth=health.MaxHealth; previousSpeed=ai.EffectiveSpeed; previousDamage=ai.EffectiveDamage;
            }
        }
        [UnityTest]
        public IEnumerator EveryStudentHasVisibleAnticipationAndExactlyOneImpact()
        {
            _root=new GameObject("Presentation_Test");
            var ally=_root.AddComponent<VarginhaStudentAlly>();
            var presentation=_root.AddComponent<VarginhaAllyAttackPresentation>();
            for(int i=0;i<VarginhaPhase2Controller.StudentNames.Length;i++)
            {
                string name=VarginhaPhase2Controller.StudentNames[i];
                ally.Configure(name,Color.cyan,i);
                int impacts=0;
                var routine=presentation.Play(name,ally.AttackStyle,Color.cyan,Vector3.left*3,Vector3.zero,()=>true,()=>impacts++);
                _root.GetComponent<VarginhaStudentAlly>().StartCoroutine(routine);
                yield return new WaitForSeconds(.2f);
                Assert.IsTrue(presentation.IsPresenting,name);
                Assert.AreEqual(0,impacts,"Não pode causar dano antes da preparação visual.");
                yield return new WaitForSeconds(1.5f);
                Assert.AreEqual(1,impacts,name);
                Assert.IsFalse(presentation.IsPresenting,name);
            }
        }
        [Test]
        public void RouteDestinationsAreDistinctAndIncludedInBuild()
        {
            Assert.AreNotEqual(VarginhaTravelCinematic.SchoolScene,VarginhaTravelCinematic.ChurchScene);
            Assert.IsTrue(Application.CanStreamedLevelBeLoaded(VarginhaTravelCinematic.SchoolScene));
            Assert.IsTrue(Application.CanStreamedLevelBeLoaded(VarginhaTravelCinematic.ChurchScene));
        }
    }
}
