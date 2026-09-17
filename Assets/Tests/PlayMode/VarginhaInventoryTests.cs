using System.Collections;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaInventoryTests
    {
        private GameObject _root;
        private EdelzioTopDownController _player;
        [SetUp] public void Setup()
        {
            Time.timeScale = 1;
            _root = new GameObject("InventoryTests");
            var player = new GameObject("Edelzio"); player.transform.SetParent(_root.transform);
            player.AddComponent<CircleCollider2D>(); player.AddComponent<SpriteRenderer>();
            _player = player.AddComponent<EdelzioTopDownController>();
        }
        [TearDown] public void Cleanup()
        {
            Object.DestroyImmediate(_root);
            Time.timeScale = 1;
        }

        [Test] public void ConsumedItemsDisappearButKnowledgeAndBackpackRemain()
        {
            _player.HasBackpack = _player.HasFuscaKey = _player.HasResearchNotebook = _player.HasDecodedData = _player.HasHistoricalDocument = true;
            Assert.IsFalse(_player.TryConsumeInventoryItem(0));
            for (int i = 1; i < 5; i++)
            {
                Assert.IsTrue(_player.HasInventoryItem(i));
                Assert.IsTrue(_player.TryConsumeInventoryItem(i));
                Assert.IsFalse(_player.HasInventoryItem(i));
                Assert.IsFalse(_player.TryConsumeInventoryItem(i));
            }
            Assert.IsTrue(_player.HasBackpack);
            Assert.IsTrue(_player.HasDecodedData);
            Assert.IsTrue(_player.HasHistoricalDocument);
            Assert.IsFalse(_player.TryConsumeInventoryItem(-1));
            Assert.IsFalse(_player.TryConsumeInventoryItem(5));
        }

        [Test] public void FailedEscapeKeepsKeyAndSuccessfulEscapeConsumesItOnce()
        {
            var car = new GameObject("Fusca", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(FuscaLevelExit));
            car.transform.SetParent(_root.transform);
            var prop = car.AddComponent<InteractableProp>(); prop.Configure(PropType.FuscaVehicle, "Fusca", "");
            _player.HasFuscaKey = true;
            prop.Interact(_player);
            Assert.IsTrue(_player.HasInventoryItem(1));
            Assert.IsTrue(prop.CanInteract);
            _player.HasResearchNotebook = true;
            prop.Interact(_player);
            Assert.IsFalse(_player.HasInventoryItem(1));
            Assert.IsFalse(_player.HasInventoryItem(2));
            Assert.IsTrue(_player.IsInputLocked);
        }

        [Test] public void NewPhaseDoesNotRepopulateUsedItems()
        {
            _player.CompleteHouseInventory();
            _player.HasFuscaKey = _player.HasResearchNotebook = _player.HasDecodedData = _player.HasHistoricalDocument = true;
            for (int i = 1; i < 5; i++) Assert.IsFalse(_player.HasInventoryItem(i));
        }

        [Test] public void NoStudentAppearsOrAttacksUntilEquipped()
        {
            var squad = VarginhaStudentAllySquad.BuildForFuturePhase(_root.transform, _player.transform, false);
            squad.ActivateManualAllies();
            Assert.AreEqual(-1, squad.SelectedStudentIndex);
            Assert.IsNull(squad.SelectedStudent);
            Assert.IsFalse(squad.TryInvokeAttack(null));
            foreach (var ally in squad.Allies)
                foreach (var renderer in ally.GetComponentsInChildren<SpriteRenderer>()) Assert.IsFalse(renderer.enabled);
            Assert.IsTrue(squad.SelectStudent(2));
            Assert.IsTrue(squad.Allies[2].GetComponent<SpriteRenderer>().enabled);
            Assert.IsTrue(squad.SelectStudent(4));
            Assert.IsFalse(squad.Allies[2].GetComponent<SpriteRenderer>().enabled);
            Assert.IsTrue(squad.Allies[4].GetComponent<SpriteRenderer>().enabled);
        }

        [UnityTest] public IEnumerator EquippedStudentDoesNotRotateAndChangingDoesNotResetCooldowns()
        {
            var squad = VarginhaStudentAllySquad.BuildForFuturePhase(_root.transform, _player.transform, false);
            squad.ActivateManualAllies();
            Assert.IsTrue(squad.SelectStudent(3));
            Assert.IsFalse(squad.SelectStudent(-1));
            Assert.IsFalse(squad.SelectStudent(9));
            var enemy = new GameObject("ET", typeof(CircleCollider2D), typeof(VarginhaCombatTarget));
            enemy.transform.SetParent(_root.transform); enemy.transform.position = Vector3.right * 3;
            Physics2D.SyncTransforms();
            Assert.IsTrue(squad.TryInvokeAttack(Vector2.right * 3));
            Assert.AreEqual(squad.Allies[3].StudentName, squad.LastInvokedStudentName);
            Assert.AreEqual(5, squad.Allies[3].ManualCooldownRemaining, .001f);
            Assert.IsTrue(squad.SelectStudent(4));
            Assert.Greater(squad.Allies[3].ManualCooldownRemaining, 0);
            Assert.IsFalse(squad.TryInvokeAttack(null), "Global command cooldown also survives switching.");
            Assert.IsTrue(squad.SelectStudent(3));
            yield return new WaitForSeconds(1);
            Assert.IsFalse(squad.TryInvokeAttack(null), "A cooling student cannot silently be replaced.");
            Assert.AreEqual(3, squad.SelectedStudentIndex);
            Assert.IsTrue(squad.Allies[4].IsReadyForManualAttack);
        }

        [Test] public void BackpackOwnsPauseAndDoesNotOpenWithoutBackpackOrDuringDialogue()
        {
            var hudGo = new GameObject("HUD", typeof(VarginhaGameHUD)); hudGo.transform.SetParent(_root.transform);
            var hud = hudGo.GetComponent<VarginhaGameHUD>();
            Assert.IsFalse(hud.OpenBackpack());
            _player.HasBackpack = true;
            hud.ShowDialogue("Test", "Test"); Assert.IsFalse(hud.OpenBackpack()); hud.CloseDialogue();
            Assert.IsTrue(hud.OpenBackpack()); Assert.AreEqual(0, Time.timeScale); Assert.IsTrue(_player.IsInputLocked);
            var inventory = hud.GetComponent<VarginhaBackpackInventory>();
            Assert.IsFalse(inventory.ShowingStudents, "Physical objects are the default tab.");
            inventory.ShowTab(true); Assert.IsTrue(inventory.ShowingStudents);
            inventory.ShowTab(false); Assert.IsFalse(inventory.ShowingStudents);
            hud.CloseBackpack(); Assert.IsFalse(hud.IsInventoryOpen); Assert.AreEqual(1, Time.timeScale);
        }

        [TestCase(false)][TestCase(true)] public void DoorHingeStaysFixedAndProjectionIsContinuous(bool flipped)
        {
            var car = new GameObject("Car", typeof(SpriteRenderer), typeof(FuscaDoorMotion)); car.transform.SetParent(_root.transform);
            car.GetComponent<SpriteRenderer>().sprite = VarginhaPixelArtSprites.Create("Fusca_Fallback", Color.blue);
            car.GetComponent<SpriteRenderer>().flipX = flipped;
            var motion = car.GetComponent<FuscaDoorMotion>(); motion.SetOpenAmount(.01f);
            var mesh = car.GetComponentInChildren<MeshFilter>().sharedMesh;
            Vector3 hinge = mesh.vertices[0];
            Assert.That(Vector3.Distance(mesh.vertices[0], mesh.vertices[3]), Is.EqualTo(.35f).Within(.001f));
            Assert.That(Vector3.Distance(mesh.vertices[0], mesh.vertices[1]), Is.EqualTo(.275f).Within(.001f));
            Assert.That(mesh.vertices[0].y, Is.EqualTo(-.12f).Within(.001f));
            Assert.That(mesh.vertices[3].y, Is.EqualTo(.23f).Within(.001f));
            Assert.That(mesh.vertices[0].x, Is.EqualTo(flipped ? -.115f : .115f).Within(.001f));
            for (int i = 1; i <= 100; i++)
            {
                motion.SetOpenAmount(i / 100f);
                Assert.AreEqual(hinge, mesh.vertices[0]);
                Assert.AreEqual(Vector3.zero, car.transform.position);
                var a = FuscaDoorMotion.ProjectFreeEdge((i - 1) / 100f, 1);
                var b = FuscaDoorMotion.ProjectFreeEdge(i / 100f, 1);
                Assert.Less(Vector2.Distance(a, b), .015f);
            }
            motion.SetOpenAmount(0); Assert.IsFalse(car.GetComponentInChildren<MeshRenderer>().enabled);
        }
    }
}
