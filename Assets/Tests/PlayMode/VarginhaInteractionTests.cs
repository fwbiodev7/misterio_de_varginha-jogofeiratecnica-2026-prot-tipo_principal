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

        [Test] public void HouseReferencePropsKeepTheirArtworkOnInitialization()
        {
            _root = new GameObject("House_And_Yard");
            var ids = new[] { "ToyBox_UnderBed", "Backpack_Prop", "Notebook_TI", "Coffee_Cup", "Doc_Historical", "FuseBox_Prop" };
            var types = new[] { PropType.ToyBoxUnderBed, PropType.Backpack, PropType.NotebookLaptop,
                PropType.CoffeeOrFood, PropType.OldDocument, PropType.FuseBox };
            for (int i = 0; i < ids.Length; i++)
            {
                var go = new GameObject(ids[i]);
                go.SetActive(false);
                go.transform.SetParent(_root.transform);
                var renderer = go.AddComponent<SpriteRenderer>();
                var prop = go.AddComponent<InteractableProp>();
                prop.Configure(types[i], ids[i], "Test");
                if (types[i] == PropType.Backpack) go.AddComponent<BackpackPickupAnimation>();
                if (types[i] == PropType.ToyBoxUnderBed) go.AddComponent<ToyBoxOpenAnimation>();
                var expected = renderer.sprite;
                go.SetActive(true);
                Assert.That(renderer.sprite.name, Does.StartWith("House512_"));
                Assert.AreSame(expected, renderer.sprite, ids[i] + " changed its art during Awake.");
            }
        }

        [UnityTest] public IEnumerator HouseReferenceChestKeepsItsOpenArtwork()
        {
            _root = new GameObject("House_And_Yard");
            var chest = new GameObject("ToyBox_UnderBed");
            chest.transform.SetParent(_root.transform);
            var renderer = chest.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaHouseReferenceArt.PropSprite(renderer, chest.name);
            var animation = chest.AddComponent<ToyBoxOpenAnimation>();
            animation.PlayOpen();
            yield return new WaitForSeconds(.35f);
            Assert.AreEqual("House512_ChestOpen", renderer.sprite.name);
            VarginhaHouseReferenceArt.Apply(_root.transform);
            Assert.AreEqual("House512_ChestOpen", renderer.sprite.name);
        }

        [UnityTest] public IEnumerator HouseReferenceCoffeeReturnsMatchingEmptyCup()
        {
            _root = new GameObject("House_And_Yard");
            var playerObject = new GameObject("CoffeePlayer");
            playerObject.transform.SetParent(_root.transform);
            playerObject.AddComponent<SpriteRenderer>();
            playerObject.AddComponent<Rigidbody2D>().gravityScale = 0f;
            playerObject.AddComponent<CircleCollider2D>();
            var player = playerObject.AddComponent<EdelzioTopDownController>();
            playerObject.AddComponent<VarginhaPlayerSpriteAnimation>();
            playerObject.AddComponent<VarginhaPlayerActionAnimation>();
            var cup = new GameObject("Coffee_Cup");
            cup.transform.SetParent(_root.transform);
            var renderer = cup.AddComponent<SpriteRenderer>();
            var prop = cup.AddComponent<InteractableProp>();
            prop.Configure(PropType.CoffeeOrFood, "Café", "Test");
            prop.Interact(player);
            yield return new WaitForSeconds(3.8f);
            Assert.IsTrue(renderer.enabled);
            Assert.AreEqual("House512_CupEmpty", renderer.sprite.name);
            Assert.IsFalse(player.IsInputLocked);
            VarginhaHouseReferenceArt.Apply(_root.transform);
            Assert.AreEqual("House512_CupEmpty", renderer.sprite.name);
        }

        [TestCase("Edelzio", "Edelzio")]
        [TestCase("Padre Fábio", "PadreFabio")]
        [TestCase("Fábio", "Fabio")]
        [TestCase("Ana Tavares", "AnaTavares")]
        [TestCase("Luis Miguel Messias", "LuisMiguelMessias")]
        public void PortraitUsesTheSpeakersCurrentAtlas(string speaker, string asset)
        {
            var portrait = VarginhaDialoguePortraits.ForSpeaker(speaker);
            if (asset == "PadreFabio")
            {
                var worldSprite = VarginhaReferenceSprites.PadreFabio();
                Assert.AreSame(worldSprite.texture, portrait.texture);
                Assert.Less(portrait.rect.height, worldSprite.rect.height);
            }
            else Assert.AreSame(Resources.Load<Texture2D>("Varginha/Allies/" + asset), portrait.texture);
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

        private VarginhaPlayerActionAnimation CreateSeatedCoffeeFixture(out EdelzioTopDownController player,
            out Transform cup, out Collider2D chair, out Collider2D table)
        {
            _root = new GameObject("House_And_Yard");
            _root.transform.position = new Vector3(100, 100);
            var playerObject = new GameObject("SeatedCoffeePlayer");
            playerObject.transform.SetParent(_root.transform, false);
            playerObject.transform.localPosition = new Vector3(3.1f, 0);
            playerObject.AddComponent<SpriteRenderer>();
            var body = playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            playerObject.AddComponent<CircleCollider2D>().radius = .49f;
            player = playerObject.AddComponent<EdelzioTopDownController>();
            playerObject.AddComponent<VarginhaPlayerSpriteAnimation>();
            var action = playerObject.AddComponent<VarginhaPlayerActionAnimation>();
            var chairObject = new GameObject("Cadeira_Cafe");
            chairObject.transform.SetParent(_root.transform, false);
            chair = chairObject.AddComponent<BoxCollider2D>();
            var tableObject = new GameObject("CoffeeTable_Kitchen");
            tableObject.transform.SetParent(_root.transform, false);
            tableObject.transform.localPosition = new Vector3(1.65f, 0);
            var box = tableObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.6f, .65f);
            table = box;
            cup = new GameObject("Coffee_Cup").transform;
            cup.SetParent(_root.transform, false);
            cup.localPosition = new Vector3(1.65f, .3f);
            cup.gameObject.AddComponent<SpriteRenderer>().sprite = VarginhaHouseReferenceArt.PropSprite(cup, cup.name);
            Physics2D.SyncTransforms();
            return action;
        }

        [UnityTest] public IEnumerator SeatedCoffeeRoutesAroundTableAndReturnsToStandingPosition()
        {
            var action = CreateSeatedCoffeeFixture(out var player, out var cup, out var chair, out var table);
            var start = player.transform.position;
            var collider = player.GetComponent<Collider2D>();
            var poses = player.GetComponent<VarginhaPlayerSpriteAnimation>();
            int completions = 0;
            bool sawSeatedDrink = false;
            action.PlayDrinkCoffee(cup, () => completions++);
            for (float elapsed = 0; elapsed < 10f && player.IsInputLocked; elapsed += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                Assert.IsFalse(collider.IsTouching(table), "A aproximação deve contornar a mesa.");
                if (poses.IsDrinking)
                {
                    sawSeatedDrink = true;
                    Assert.IsTrue(poses.IsSeated);
                    Assert.Less(Vector2.Distance(player.transform.position, chair.transform.position), .2f);
                    Assert.AreEqual(Vector2.right, poses.ActionFacingDirection);
                }
            }
            Assert.IsTrue(sawSeatedDrink);
            Assert.AreEqual(1, completions);
            Assert.IsFalse(player.IsInputLocked);
            Assert.IsFalse(player.IsScriptedMotion);
            Assert.IsFalse(poses.IsSeated);
            Assert.IsTrue(cup.GetComponent<SpriteRenderer>().enabled);
            Assert.IsFalse(Physics2D.GetIgnoreCollision(collider, chair));
            Assert.Less(Vector2.Distance(start, player.transform.position), .16f);
        }

        [UnityTest] public IEnumerator BlockedChairCancelsCoffeeAndAllowsRetry()
        {
            var action = CreateSeatedCoffeeFixture(out var player, out var cup, out var chair, out var table);
            var blocker = new GameObject("BlockedSeat", typeof(BoxCollider2D));
            blocker.transform.SetParent(_root.transform, false);
            blocker.transform.position = chair.transform.position;
            Physics2D.SyncTransforms();
            int completions = 0, cancellations = 0;
            action.PlayDrinkCoffee(cup, () => completions++, () => cancellations++);
            yield return null;
            Assert.AreEqual(0, completions);
            Assert.AreEqual(1, cancellations);
            Assert.IsFalse(player.IsInputLocked);
            Assert.IsTrue(cup.GetComponent<SpriteRenderer>().enabled);
            Assert.IsFalse(Physics2D.GetIgnoreCollision(player.GetComponent<Collider2D>(), chair));
            Object.DestroyImmediate(blocker);
            Physics2D.SyncTransforms();
            action.PlayDrinkCoffee(cup, () => completions++, () => cancellations++);
            for (float elapsed = 0; elapsed < 10f && player.IsInputLocked; elapsed += Time.fixedDeltaTime)
                yield return new WaitForFixedUpdate();
            Assert.AreEqual(1, completions);
            Assert.AreEqual(1, cancellations);
        }
    }
}
