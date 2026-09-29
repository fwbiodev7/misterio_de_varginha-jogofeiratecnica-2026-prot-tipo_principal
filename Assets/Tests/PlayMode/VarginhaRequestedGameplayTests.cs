using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Level;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaRequestedGameplayTests
    {
        private GameObject _root;
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        [SetUp] public void Setup()
        {
            Time.timeScale = 1;
            _root = new GameObject("RequestedGameplayTests");
            EdelzioTopDownController.PersistentHasFlashlight = false;
            EdelzioTopDownController.PersistentFlashlightActive = false;
            EdelzioTopDownController.PersistentFlashlightInHotbar = false;
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Object.Destroy(_root);
            Time.timeScale = 1;
            EdelzioTopDownController.PersistentHasFlashlight = false;
            EdelzioTopDownController.PersistentFlashlightActive = false;
            EdelzioTopDownController.PersistentFlashlightInHotbar = false;
            yield return null;
        }

        private EdelzioTopDownController Player()
        {
            var go = new GameObject("Edelzio_Test");
            go.transform.SetParent(_root.transform);
            go.AddComponent<CircleCollider2D>().radius = .24f;
            var player = go.AddComponent<EdelzioTopDownController>();
            go.AddComponent<VarginhaPlayerSpriteAnimation>();
            go.AddComponent<VarginhaPlayerActionAnimation>();
            return player;
        }

        [UnityTest] public IEnumerator PewStopsWalkingAndSeatRestoresCollisionWhenInterrupted()
        {
            VarginhaEnvironmentArt.EnsureDiocese(_root.transform);
            var pews = _root.GetComponentsInChildren<SpriteRenderer>()
                .Where(r => r.name.StartsWith("CenarioV2_Banco_Igreja_")).ToArray();
            Assert.AreEqual(10, pews.Length);
            Assert.IsTrue(pews.All(p => p.GetComponent<BoxCollider2D>()?.isTrigger == false));
            var seat = pews.Single(p => p.GetComponent<InteractableProp>() != null);
            var player = Player();
            var body = player.GetComponent<Rigidbody2D>();
            var collider = player.GetComponent<Collider2D>();
            body.position = (Vector2)seat.transform.position + Vector2.down * .9f;
            player.enabled = false;
            yield return null;
            for (int i = 0; i < 30; i++)
            {
                body.linearVelocity = Vector2.up * 3;
                yield return new WaitForFixedUpdate();
            }
            Assert.Less(body.position.y, seat.transform.position.y - .3f, "Walking must stop at the bench.");
            body.linearVelocity = Vector2.zero;
            player.enabled = true;
            var standing = body.position;
            var action = player.GetComponent<VarginhaPlayerActionAnimation>();
            seat.GetComponent<InteractableProp>().Interact(player);
            yield return new WaitForSeconds(.45f);
            Assert.IsTrue(player.GetComponent<VarginhaPlayerSpriteAnimation>().IsSeated);
            Assert.IsTrue(Physics2D.GetIgnoreCollision(collider, seat.GetComponent<Collider2D>()));
            action.enabled = false;
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(Physics2D.GetIgnoreCollision(collider, seat.GetComponent<Collider2D>()));
            Assert.Less(Vector2.Distance(standing, body.position), .04f);
            Assert.IsFalse(player.IsInputLocked);
        }

        [UnityTest] public IEnumerator FuscaBoardsImmediatelyWithoutDoorAndKeepsRequirements()
        {
            var player = Player();
            var car = new GameObject("Fusca_Test"); car.transform.SetParent(_root.transform);
            car.AddComponent<SpriteRenderer>();
            car.AddComponent<BoxCollider2D>();
            var exit = car.AddComponent<FuscaLevelExit>();
            exit.TryEscape(player);
            Assert.AreNotEqual(car.transform, player.transform.parent);
            player.HasFuscaKey = true; player.HasResearchNotebook = true;
            exit.TryEscape(player);
            Assert.AreEqual(car.transform, player.transform.parent);
            Assert.IsNull(car.GetComponent<FuscaDoorMotion>());
            Assert.IsFalse(player.GetComponent<SpriteRenderer>().enabled);
            Assert.IsFalse(player.GetComponent<Rigidbody2D>().simulated);
            Assert.Less(Vector2.Distance(player.transform.position, car.transform.position), .1f);
            // Stop before departure can initiate a scene transition in the isolated test.
            exit.StopAllCoroutines();
            yield return null;
        }

        [UnityTest] public IEnumerator ChurchHasDarknessAltarPriestAndClearFlashlightCone()
        {
            var phase = _root.AddComponent<VarginhaPhase3Controller>();
            yield return new WaitForSeconds(2);
            var player = _root.GetComponentInChildren<EdelzioTopDownController>();
            Assert.IsNotNull(player);
            player.SetInputLocked(true);
            var camera = _root.GetComponentInChildren<Camera>();
            Assert.Greater(VarginhaGameHUD.GameplayBottomInset, 0);
            Assert.GreaterOrEqual(camera.pixelRect.yMin, VarginhaGameHUD.GameplayBottomInset - 1);
            Assert.Greater(camera.WorldToScreenPoint(player.transform.position).y, VarginhaGameHUD.GameplayBottomInset);
            camera.GetComponent<CameraFollow2D>().enabled = false;
            camera.transform.position = new Vector3(.5f, 0, -10);
            camera.orthographicSize = 7.5f;
            foreach (var enemy in _root.GetComponentsInChildren<VarginhaCombatEnemy>()) enemy.enabled = false;
            Assert.AreEqual(new Vector3(5.2f, 1.2f), GameObject.Find("Padre_Fabio").transform.position);
            Assert.IsTrue(player.HasFlashlight);
            player.transform.position = new Vector3(-.2f, -5.25f);
            var flashlight = player.GetComponent<EdelzioFlashlight>();
            typeof(EdelzioFlashlight).GetField("followCursor", Private).SetValue(flashlight, false);
            typeof(EdelzioTopDownController).GetField("_lastFacing", Private).SetValue(player, Vector2.up);
            player.FlashlightActive = false;
            yield return null;
            var dark = Render(camera, "church-flashlight-off");
            player.FlashlightActive = true;
            flashlight.enabled = true;
            yield return null;
            yield return null;
            var lit = Render(camera, "church-flashlight-on");
            player.GetComponent<VarginhaDarkness>().enabled = false;
            var unobscured = Render(camera, "church-lighting-reference");
            Vector3 sample = camera.WorldToViewportPoint(new Vector3(-.2f, -2.6f));
            Color off = dark.GetPixelBilinear(sample.x, sample.y);
            Color on = lit.GetPixelBilinear(sample.x, sample.y);
            Assert.Less(off.grayscale, .055f, "The unlit room must be nearly black.");
            Assert.Greater(on.grayscale, off.grayscale * 5, "The beam must reveal the scene, not just tint it.");
            Assert.Greater(on.grayscale, unobscured.GetPixelBilinear(sample.x, sample.y).grayscale * .9f,
                "Inside the beam, at least 90% of the scene brightness must remain.");
            Assert.IsTrue(flashlight.IlluminatesPoint(new Vector2(-.2f, -2.6f)));
            Assert.IsFalse(flashlight.IlluminatesPoint(new Vector2(4, -2.6f)));
            Assert.IsTrue(_root.GetComponentsInChildren<VarginhaGlowingEyes>().Length >= 5);
            Object.Destroy(dark); Object.Destroy(lit); Object.Destroy(unobscured);
        }

        private static Texture2D Render(Camera camera, string name)
        {
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(960, 640, 24);
            var oldTarget = camera.targetTexture; var oldRect = camera.rect;
            camera.targetTexture = target; camera.rect = new Rect(0, 0, 1, 1);
            camera.Render(); RenderTexture.active = target;
            var texture = new Texture2D(960, 640, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, 960, 640), 0, 0); texture.Apply();
            Directory.CreateDirectory("scratch/gameplay-review");
            File.WriteAllBytes("scratch/gameplay-review/" + name + ".png", texture.EncodeToPNG());
            camera.targetTexture = oldTarget; camera.rect = oldRect;
            RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target);
            return texture;
        }

        [Test] public void HouseFurnitureResizesOnceAndSurgesIncludeRecovery()
        {
            var house = new GameObject("House_And_Yard"); house.transform.SetParent(_root.transform);
            var sofa = new GameObject("Sofa_LivingRoom"); sofa.transform.SetParent(house.transform);
            sofa.transform.position = new Vector3(4.5f, 3.55f);
            sofa.transform.localScale = new Vector3(3.8f, 1.9f, 1);
            VarginhaHouseComposition.Apply(house.transform);
            Vector3 once = sofa.transform.localScale;
            VarginhaHouseComposition.Apply(house.transform);
            Assert.AreEqual(once, sofa.transform.localScale);
            Assert.Less(once.y, 1.6f);
            Assert.Greater(VarginhaDarkness.SurgeOpacity(1), .8f);
            Assert.Less(VarginhaDarkness.SurgeOpacity(10), .1f);
            var player = Player();
            var chest = new GameObject("ToyBox_Test"); chest.transform.SetParent(_root.transform);
            var prop = chest.AddComponent<InteractableProp>();
            prop.Configure(PropType.ToyBoxUnderBed, "Bau", "", false);
            prop.Interact(player);
            Assert.IsTrue(player.GetComponent<VarginhaDarkness>().PowerSurgesActive);
            // World pickup is independently spawned; keep the test isolated.
            foreach (var pickup in Object.FindObjectsByType<FlashlightWorldPickup>(FindObjectsInactive.Include))
                Object.DestroyImmediate(pickup.gameObject);
        }
    }
}
