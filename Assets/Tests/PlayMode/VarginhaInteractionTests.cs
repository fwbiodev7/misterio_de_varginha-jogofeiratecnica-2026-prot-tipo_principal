using System.Collections;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaInteractionTests
    {
        private GameObject _root;
        [TearDown] public void Cleanup()
        {
            Time.timeScale = 1f;
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [Test] public void CoffeeSeatingAndWebcamHaveTheirOwnFrames()
        {
            var atlas = Resources.Load<Texture2D>(VarginhaInteractionSprites.ResourcePath);
            Assert.IsNotNull(atlas);
            Assert.IsTrue(atlas.isReadable);
            Assert.AreEqual(FilterMode.Point, atlas.filterMode);
            for (int row = 0; row < 3; row++)
            for (int frame = 0; frame < 4; frame++)
            {
                var sprite = VarginhaInteractionSprites.Frame(row, frame);
                Assert.AreSame(atlas, sprite.texture);
                Assert.Greater(sprite.rect.width, 20);
                if (frame > 0) Assert.IsFalse(sprite.rect.Overlaps(VarginhaInteractionSprites.Frame(row, frame - 1).rect));
            }
            Time.timeScale = 0f;
            Assert.AreNotSame(VarginhaInteractionSprites.Webcam(0f, false), VarginhaInteractionSprites.Webcam(4.4f, false));
            Assert.AreSame(VarginhaInteractionSprites.Frame(2, 3), VarginhaInteractionSprites.Webcam(1f, true));
        }

        [Test] public void MissingNotebookChairIsCreatedOnlyOnce()
        {
            _root = new GameObject("InteractionTest");
            var notebook = new GameObject("Notebook_TI").transform;
            notebook.SetParent(_root.transform);
            notebook.position = new Vector3(-5f, -3.3f);
            var first = VarginhaHouseComposition.EnsureNotebookChair(notebook);
            Assert.AreSame(first, VarginhaHouseComposition.EnsureNotebookChair(notebook));
            Assert.IsNotNull(first.GetComponent<SpriteRenderer>().sprite);
            Assert.IsNotNull(first.GetComponent<Collider2D>());
            Assert.Less(Vector2.Distance(first.transform.position, notebook.position), 1.5f);
        }

        [TestCase("Edelzio", "Edelzio")]
        [TestCase("Padre Fábio", "PadreFabio")]
        [TestCase("Fábio", "Fabio")]
        [TestCase("Ana Tavares", "AnaTavares")]
        [TestCase("Luis Miguel Messias", "LuisMiguelMessias")]
        public void PortraitUsesTheSpeakersCurrentAtlas(string speaker, string asset)
        {
            Assert.AreSame(Resources.Load<Texture2D>("Varginha/Allies/" + asset),
                VarginhaDialoguePortraits.ForSpeaker(speaker).texture);
        }

        [UnityTest] public IEnumerator InterruptedCoffeeRestoresCupAndMovement()
        {
            _root = new GameObject("CoffeeActionTest");
            _root.AddComponent<SpriteRenderer>();
            _root.AddComponent<Rigidbody2D>().gravityScale = 0f;
            _root.AddComponent<CircleCollider2D>();
            var player = _root.AddComponent<EdelzioTopDownController>();
            _root.AddComponent<VarginhaPlayerSpriteAnimation>();
            var action = _root.AddComponent<VarginhaPlayerActionAnimation>();
            var cup = new GameObject("Cup");
            cup.transform.SetParent(_root.transform);
            var renderer = cup.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Coffee_Hot", Color.white);
            int completions = 0;
            action.PlayDrinkCoffee(cup.transform, () => completions++);
            yield return new WaitForSeconds(.6f);
            action.enabled = false;
            yield return null;
            Assert.IsTrue(renderer.enabled);
            Assert.IsFalse(player.IsScriptedMotion);
            Assert.IsFalse(player.GetComponent<VarginhaPlayerSpriteAnimation>().IsDrinking);
            Assert.AreEqual(0, completions, "Interromper não deve conceder a cura nem consumir o café.");
        }
    }
}
