using System.Collections;
using Game.Player;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaMenuAndDeathTests
    {
        private GameObject _root;
        [TearDown]
        public void Cleanup()
        {
            if (_root != null) Object.DestroyImmediate(_root);
            Time.timeScale = 1f;
        }

        private VarginhaGameHUD Setup(out GameObject actor)
        {
            _root = new GameObject("MenuAndDeathTest");
            actor = new GameObject("Edelzio_Test");
            actor.transform.SetParent(_root.transform);
            actor.AddComponent<SpriteRenderer>();
            actor.AddComponent<Rigidbody2D>().gravityScale = 0;
            actor.AddComponent<CircleCollider2D>();
            actor.AddComponent<EdelzioTopDownController>();
            actor.AddComponent<HealthSystem>();
            actor.AddComponent<VarginhaGameOverFlow>();
            return _root.AddComponent<VarginhaGameHUD>();
        }

        [UnityTest]
        public IEnumerator PersistentHudCannotDrawOrPauseOverMenu()
        {
            var hud = Setup(out _);
            yield return null;
            Assert.IsTrue(hud.IsGameplayVisible);
            var menu = new GameObject("Menu_Test");
            menu.transform.SetParent(_root.transform);
            menu.AddComponent<VarginhaMainMenu>();
            Assert.IsFalse(hud.IsGameplayVisible);
            hud.TogglePause();
            Assert.IsFalse(hud.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Object.DestroyImmediate(menu);
            Assert.IsTrue(hud.IsGameplayVisible);
        }

        [UnityTest]
        public IEnumerator DeathClosesPauseAndWaitsForAnExplicitChoice()
        {
            var hud = Setup(out var actor);
            yield return null;
            hud.TogglePause();
            Assert.IsTrue(hud.IsPaused);
            actor.GetComponent<HealthSystem>().TakeDamage(10000);
            Assert.IsTrue(hud.IsGameOver);
            Assert.IsFalse(hud.IsPaused);
            Assert.IsTrue(hud.BlocksGameplayInput);
            yield return new WaitForSecondsRealtime(3.5f);
            Assert.IsNotNull(actor, "No forced scene reload after the old 3.2s delay");
            Assert.IsTrue(hud.IsGameOver);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator ContactShadowFollowsVisibilityWithoutChangingPhysics()
        {
            Setup(out var actor);
            var sprite = actor.GetComponent<SpriteRenderer>();
            sprite.sprite = VarginhaReferenceSprites.EdelzioWalkFrames()[0][0];
            var scale = actor.transform.localScale;
            VarginhaContactShadow.Ensure(sprite);
            VarginhaContactShadow.Ensure(sprite);
            yield return null;
            Assert.AreEqual(1, actor.GetComponents<VarginhaContactShadow>().Length);
            Assert.AreEqual(1, actor.GetComponentsInChildren<Collider2D>().Length);
            Assert.AreEqual(scale, actor.transform.localScale);
            var shadow = actor.transform.Find("Sombra_Contato_Movel").GetComponent<SpriteRenderer>();
            Assert.IsTrue(shadow.enabled);
            sprite.enabled = false;
            yield return null;
            Assert.IsFalse(shadow.enabled);
        }
    }
}
