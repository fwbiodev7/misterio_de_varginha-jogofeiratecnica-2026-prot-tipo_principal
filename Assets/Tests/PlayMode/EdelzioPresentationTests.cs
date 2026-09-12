using System.Collections;
using System.Reflection;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class EdelzioPresentationTests
    {
        private GameObject _go;
        private EdelzioTopDownController _player;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Edelzio_Test");
            _go.AddComponent<SpriteRenderer>();
            _go.AddComponent<CircleCollider2D>().radius = .45f;
            _go.AddComponent<Rigidbody2D>();
            _player = _go.AddComponent<EdelzioTopDownController>();
            _go.AddComponent<VarginhaPlayerSpriteAnimation>();
            _go.AddComponent<VarginhaPlayerActionAnimation>();
        }

        [TearDown]
        public void TearDown() { Object.DestroyImmediate(_go); }

        [UnityTest]
        public IEnumerator LockStopsResidualMotion()
        {
            var body = _go.GetComponent<Rigidbody2D>();
            body.linearVelocity = new Vector2(4, 2);
            var start = body.position;
            _player.SetInputLocked(true);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(body.position, Is.EqualTo(start));
            Assert.IsFalse(_player.IsMoving);
        }

        [UnityTest]
        public IEnumerator BackpackFollowsBackInsteadOfFlipX()
        {
            _player.EquipBackpack();
            var item = _go.transform.Find("Mochila_Equipada");
            yield return null;
            Assert.That(item.localPosition.x, Is.EqualTo(0f));
            Assert.Less(item.GetComponent<SpriteRenderer>().sortingOrder, _go.GetComponent<SpriteRenderer>().sortingOrder);
            typeof(EdelzioTopDownController).GetField("_lastFacing", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_player, Vector2.up);
            yield return null;
            Assert.That(item.localPosition.x, Is.EqualTo(0f));
            Assert.Greater(item.GetComponent<SpriteRenderer>().sortingOrder, _go.GetComponent<SpriteRenderer>().sortingOrder);
            Assert.IsNull(item.GetComponent<Collider2D>());
            Assert.IsNull(item.GetComponent<Rigidbody2D>());
            Assert.GreaterOrEqual(item.localScale.y, .85f);
            var straps = _go.transform.Find("Mochila_Alcas").GetComponent<SpriteRenderer>();
            Assert.IsFalse(straps.enabled);
            typeof(EdelzioTopDownController).GetField("_lastFacing", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_player, Vector2.down);
            yield return null;
            Assert.IsTrue(straps.enabled);
            Assert.Greater(straps.sortingOrder, _go.GetComponent<SpriteRenderer>().sortingOrder);
            _player.SetCarriedItemsVisible(false);
            yield return null;
            Assert.IsFalse(straps.enabled);
            Assert.IsFalse(item.GetComponent<SpriteRenderer>().enabled);
            _player.SetCarriedItemsVisible(true);
            yield return null;
            Assert.IsTrue(straps.enabled);
            Assert.IsTrue(item.GetComponent<SpriteRenderer>().enabled);
        }

        [Test]
        public void HotbarReflectsActualCollectedItems()
        {
            for (int slot = 0; slot < 5; slot++) Assert.IsFalse(_player.HasInventoryItem(slot));
            _player.EquipBackpack();
            _player.HasFuscaKey = true;
            _player.HasResearchNotebook = true;
            for (int slot = 0; slot < 3; slot++) Assert.IsTrue(_player.HasInventoryItem(slot));
            Assert.IsFalse(_player.HasInventoryItem(3));
            Assert.IsFalse(_player.HasInventoryItem(4));
            _player.HasDecodedData = true;
            _player.HasHistoricalDocument = true;
            Assert.IsTrue(_player.HasInventoryItem(3));
            Assert.IsTrue(_player.HasInventoryItem(4));
            Assert.IsFalse(_player.HasInventoryItem(-1));
            Assert.IsFalse(_player.HasInventoryItem(5));
        }

        [UnityTest]
        public IEnumerator ActionsKeepNewAppearanceAndColliderSize()
        {
            var animation = _go.GetComponent<VarginhaPlayerSpriteAnimation>();
            var renderer = _go.GetComponent<SpriteRenderer>();
            var atlas = Resources.Load<Texture2D>("Varginha/EdelzioTopDownV3");
            Assert.IsNotNull(atlas);
            var scale = _go.transform.localScale;
            var radius = _go.GetComponent<CircleCollider2D>().radius;
            foreach (var pose in new[] { "Edelzio_Crouch", "Edelzio_Reach", "Edelzio_DrinkCoffee", "Edelzio_Sit", "Edelzio_UseNotebook" })
            {
                animation.SetActionPose(pose);
                yield return null;
                Assert.AreSame(atlas, renderer.sprite.texture);
                Assert.AreEqual(scale, _go.transform.localScale);
                Assert.AreEqual(radius, _go.GetComponent<CircleCollider2D>().radius);
            }
            Assert.IsTrue(animation.IsSeated);
            animation.ClearActionPose();
            Assert.IsFalse(animation.IsSeated);
        }
    }
}
