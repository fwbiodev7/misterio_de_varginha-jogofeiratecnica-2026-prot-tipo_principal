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
        public IEnumerator BackpackStateControlsIntegratedAppearanceInEveryDirection()
        {
            var animation = _go.GetComponent<VarginhaPlayerSpriteAnimation>();
            var renderer = _go.GetComponent<SpriteRenderer>();
            var atlas = VarginhaReferenceSprites.HasEdelzio
                ? VarginhaReferenceSprites.EdelzioWalkFrames()[0][0].texture
                : Resources.Load<Texture2D>("Varginha/EdelzioTopDownV3");
            var directions = new[] { Vector2.down, Vector2.left, Vector2.right, Vector2.up };
            var facing = typeof(EdelzioTopDownController).GetField("_lastFacing", BindingFlags.Instance | BindingFlags.NonPublic);
            _player.SetInputLocked(true);
            for (int direction = 0; direction < directions.Length; direction++)
            {
                facing.SetValue(_player, directions[direction]);
                yield return null;
                var body = renderer.sprite;
                Assert.AreSame(atlas, body.texture);
                _player.HasBackpack = true;
                var equipped = renderer.sprite;
                Assert.IsTrue(_player.IsBackpackVisible);
                Assert.That(equipped.name, Does.EndWith("_ComMochila_" + direction));
                Assert.AreNotSame(body.texture, equipped.texture);
                AssertSameSpriteGeometry(body, equipped);
                animation.RefreshEquipmentAppearance();
                _player.EquipBackpack();
                Assert.AreSame(equipped, renderer.sprite, "Repeated refresh reuses the same pose, without composing onto the backpack again.");

                _player.SetCarriedItemsVisible(false);
                Assert.IsTrue(_player.HasBackpack);
                Assert.IsFalse(_player.IsBackpackVisible);
                Assert.AreSame(body, renderer.sprite, "Entering the Fusca hides equipment without changing the character pose.");
                _player.SetCarriedItemsVisible(true);
                Assert.AreSame(equipped, renderer.sprite);
                _player.HasBackpack = false;
                Assert.IsFalse(_player.IsBackpackVisible);
                Assert.AreSame(body, renderer.sprite);
            }
            Assert.IsNull(_go.transform.Find("Mochila_Equipada"));
            Assert.IsNull(_go.transform.Find("Mochila_Alcas"));
            Assert.AreEqual(1, _go.GetComponentsInChildren<Collider2D>().Length);
            Assert.AreEqual(1, _go.GetComponentsInChildren<Rigidbody2D>().Length);
        }

        [Test]
        public void ReferenceArtCoversDirectionsWeaponsAndPortrait()
        {
            for (int direction = 0; direction < 4; direction++)
            {
                foreach (bool fabio in new[] { false, true })
                    foreach (bool walking in new[] { false, true })
                    {
                        var sprite = VarginhaReferenceSprites.Character(fabio, direction, walking);
                        Assert.IsNotNull(sprite);
                        Assert.AreEqual(FilterMode.Point, sprite.texture.filterMode);
                        Assert.Greater(sprite.bounds.size.y, .8f);
                        Assert.Less(sprite.bounds.size.y, 1.5f);
                    }
                for (int frame = 0; frame < 4; frame++)
                    Assert.IsNotNull(VarginhaReferenceSprites.Attack(direction, frame));
            }
            Assert.IsNotNull(VarginhaReferenceSprites.FabioPortrait());
            foreach (string id in new[] { "StudentAttack_Art", "StudentAttack_Microphone", "StudentAttack_Katana", "StudentAttack_FallingPiano", "HostageCage_Fabio" })
            {
                var sprite = VarginhaPixelArtSprites.Create(id, Color.magenta);
                Assert.AreEqual("AllyReferencePropsV1", sprite.texture.name);
            }
            Assert.AreNotSame(VarginhaPixelArtSprites.Create("Padre_Fabio", Color.white),
                VarginhaPixelArtSprites.Create("Student_Fabio", Color.white));
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
            var atlas = Resources.Load<Texture2D>("Varginha/EdelzioReferenceActionsV1");
            Assert.IsNotNull(atlas);
            var scale = _go.transform.localScale;
            var radius = _go.GetComponent<CircleCollider2D>().radius;
            foreach (var pose in new[] { "Edelzio_Crouch", "Edelzio_Reach", "Edelzio_DrinkCoffee", "Edelzio_Sit", "Edelzio_UseNotebook" })
            {
                _player.HasBackpack = false;
                animation.SetActionPose(pose);
                var body = renderer.sprite;
                Assert.AreSame(atlas, body.texture);
                _player.EquipBackpack();
                yield return null;
                Assert.That(renderer.sprite.name, Does.EndWith("_ComMochila_0"), pose);
                Assert.AreNotSame(atlas, renderer.sprite.texture, pose);
                AssertSameSpriteGeometry(body, renderer.sprite);
                Assert.AreEqual(scale, _go.transform.localScale);
                Assert.AreEqual(radius, _go.GetComponent<CircleCollider2D>().radius);
            }
            Assert.IsTrue(animation.IsSeated);
            animation.ClearActionPose();
            Assert.IsFalse(animation.IsSeated);
        }

        [Test]
        public void CombatPosesKeepEquipmentWithoutChangingGeometry()
        {
            var animation = _go.GetComponent<VarginhaPlayerSpriteAnimation>();
            var renderer = _go.GetComponent<SpriteRenderer>();
            var atlas = Resources.Load<Texture2D>("Varginha/EdelzioAttackV1");
            Assert.IsNotNull(atlas);
            var directions = new[] { Vector2.down, Vector2.left, Vector2.right, Vector2.up };
            var scale = _go.transform.localScale;
            var radius = _go.GetComponent<CircleCollider2D>().radius;
            for (int direction = 0; direction < directions.Length; direction++)
            for (int frame = 0; frame < 6; frame++)
            {
                var body = Sprite.Create(atlas,
                    new Rect(frame * atlas.width / 6f, (3 - direction) * atlas.height / 4f, atlas.width / 6f, atlas.height / 4f),
                    new Vector2(.5f, .5f), 44.1379f);
                body.name = "Edelzio_Ataque_" + direction + "_" + frame;
                try
                {
                    _player.EquipBackpack();
                    animation.SetCombatPose(body, directions[direction]);
                    var equipped = renderer.sprite;
                    Assert.That(equipped.name, Does.EndWith("_ComMochila_" + direction), body.name);
                    Assert.AreNotSame(atlas, equipped.texture);
                    AssertSameSpriteGeometry(body, equipped);
                    animation.RefreshEquipmentAppearance();
                    Assert.AreSame(equipped, renderer.sprite);
                    Assert.AreEqual(scale, _go.transform.localScale);
                    Assert.AreEqual(radius, _go.GetComponent<CircleCollider2D>().radius);
                    _player.HasBackpack = false;
                    Assert.AreSame(body, renderer.sprite);
                }
                finally
                {
                    animation.ClearActionPose();
                    Object.DestroyImmediate(body);
                }
            }
        }

        private static void AssertSameSpriteGeometry(Sprite expected, Sprite actual)
        {
            Assert.AreEqual(expected.rect.size, actual.rect.size);
            Assert.AreEqual(expected.pivot, actual.pivot);
            Assert.AreEqual(expected.pixelsPerUnit, actual.pixelsPerUnit);
        }
    }
}
