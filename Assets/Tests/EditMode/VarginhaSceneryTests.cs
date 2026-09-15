using System.Linq;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class VarginhaSceneryTests
    {
        private GameObject _root;

        [SetUp] public void SetUp() => _root = new GameObject("SceneryTest");
        [TearDown] public void TearDown() => Object.DestroyImmediate(_root);

        [TestCase("Backpack_Worn")][TestCase("Backpack_Side")]
        [TestCase("Notebook_Inventory")][TestCase("Notebook_Held")]
        public void EquipmentRemainsGrayEvenWhenCallerUsesAnOldTint(string id)
        {
            var sprite = VarginhaPixelArtSprites.Create(id, Color.magenta);
            foreach (var pixel in sprite.texture.GetPixels32().Where(p => p.a > 0))
            { Assert.AreEqual(pixel.r, pixel.g); Assert.AreEqual(pixel.g, pixel.b); }
            Assert.AreEqual(FilterMode.Point, sprite.texture.filterMode);
        }

        [TestCase(.6f)][TestCase(1.2f)][TestCase(2.1f)]
        public void BeamWidthAndOriginFollowActualWindowBounds(float width)
        {
            var window = new GameObject("Window", typeof(SpriteRenderer)); window.transform.SetParent(_root.transform);
            var renderer = window.GetComponent<SpriteRenderer>();
            renderer.sprite = VarginhaSceneryArt.Create("Window", new Vector2(width, .6f));
            window.transform.position = new Vector3(3, 4, 0);
            var rayObject = new GameObject("Ray", typeof(SpriteRenderer)); rayObject.transform.SetParent(_root.transform);
            var ray = rayObject.GetComponent<SpriteRenderer>();
            VarginhaEnvironmentPolish.FitWindowBeam(window.transform, ray, Vector2.down, false);
            Assert.That(ray.sprite.bounds.size.y * .6f, Is.EqualTo(renderer.bounds.size.x * .8f).Within(.025f));
            Vector3 start = ray.transform.TransformPoint(new Vector3(ray.sprite.bounds.min.x, 0, 0));
            Assert.That(start.x, Is.EqualTo(renderer.bounds.center.x).Within(.001f));
            Assert.That(start.y, Is.EqualTo(renderer.bounds.min.y - .035f).Within(.001f));
        }

        private Transform Build(string phase)
        {
            if (phase == "school") return VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            if (phase == "diocese") return VarginhaEnvironmentArt.EnsureDiocese(_root.transform);
            VarginhaEnvironmentPolish.EnsureHouse(_root.transform);
            return _root.transform;
        }

        [TestCase("school")]
        [TestCase("diocese")]
        [TestCase("house")]
        public void SetupIsIdempotentAndDecorationHasNoColliders(string phase)
        {
            var environment = Build(phase);
            int objects = _root.GetComponentsInChildren<Transform>(true).Length;
            var colliders = _root.GetComponentsInChildren<Collider2D>(true);
            Build(phase);
            Assert.AreEqual(objects, _root.GetComponentsInChildren<Transform>(true).Length);
            CollectionAssert.AreEquivalent(colliders, _root.GetComponentsInChildren<Collider2D>(true));
            Assert.IsEmpty(environment.Find("Cenario_Acabamento_V3").GetComponentsInChildren<Collider2D>(true));
        }

        [TestCase("school")]
        [TestCase("diocese")]
        [TestCase("house")]
        public void LostProceduralSpriteIsRestoredWithoutDuplicateDecoration(string phase)
        {
            var environment = Build(phase);
            int objects = _root.GetComponentsInChildren<Transform>(true).Length;
            var decor = environment.Find("Cenario_Acabamento_V3");
            decor.GetComponentInChildren<SpriteRenderer>().sprite = null;
            Build(phase);
            Assert.AreEqual(objects, _root.GetComponentsInChildren<Transform>(true).Length);
            foreach (var renderer in environment.Find("Cenario_Acabamento_V3").GetComponentsInChildren<SpriteRenderer>(true))
            {
                Assert.IsNotNull(renderer.sprite, renderer.name);
                Assert.IsNotNull(renderer.sprite.texture, renderer.name);
            }
        }

        [Test]
        public void CathedralHasTenTranslucentFloorReflectionsBehindFurniture()
        {
            var environment = Build("diocese");
            var rays = environment.GetComponentsInChildren<SpriteRenderer>().Where(r => r.name.StartsWith("Reflexo_Vitral_")).ToArray();
            Assert.AreEqual(10, rays.Length);
            int pewOrder = environment.Find("CenarioV2_Banco_Igreja_0").GetComponent<SpriteRenderer>().sortingOrder;
            foreach (var ray in rays)
            {
                Assert.Less(ray.sortingOrder, pewOrder);
                Assert.IsTrue(ray.sprite.texture.GetPixels32().Any(p => p.a > 0 && p.a < 255));
                Assert.AreEqual(FilterMode.Bilinear, ray.sprite.texture.filterMode);
                Assert.IsFalse(ray.enabled, "Sources must be composed, not drawn over one another.");
            }
        }

        [Test]
        public void ParkingUsesFullNativeDimensionsWithoutExtraScaling()
        {
            var parking = Build("school").Find("CenarioV2_Vaga_Fusca");
            var sprite = parking.GetComponent<SpriteRenderer>().sprite;
            Assert.AreEqual(Vector3.one, parking.localScale);
            Assert.AreEqual(VarginhaEnvironmentArt.FuscaParkingPosition, parking.position);
            Assert.That(sprite.bounds.size.x, Is.EqualTo(VarginhaEnvironmentArt.FuscaParkingSize.x).Within(1f / 32));
            Assert.That(sprite.bounds.size.y, Is.EqualTo(VarginhaEnvironmentArt.FuscaParkingSize.y).Within(1f / 32));
        }

        [TestCase("Glass")][TestCase("GlassLight")][TestCase("WindowLight")]
        [TestCase("Glow")][TestCase("Shadow")][TestCase("Window")]
        [TestCase("Books")][TestCase("Noticeboard")][TestCase("Plant")]
        [TestCase("Column")][TestCase("Rug")][TestCase("Puddle")]
        [TestCase("Leaves")][TestCase("Parking")][TestCase("Mosaic")]
        [TestCase("Sconce")][TestCase("Pew")][TestCase("Altar")]
        [TestCase("Desk")][TestCase("Blackboard")][TestCase("StreetLamp")][TestCase("Dust")]
        public void ArtIsCachedPointFilteredAndVisible(string motif)
        {
            var size = new Vector2(1.5f, 1f);
            var sprite = VarginhaSceneryArt.Create(motif, size);
            Assert.AreSame(sprite, VarginhaSceneryArt.Create(motif, size));
            Assert.AreEqual(48, sprite.texture.width);
            Assert.AreEqual(32, sprite.texture.height);
            Assert.AreEqual(32, sprite.pixelsPerUnit);
            Assert.AreEqual(VarginhaSceneryArt.IsSoftLighting(motif) ? FilterMode.Bilinear : FilterMode.Point, sprite.texture.filterMode);
            Assert.AreEqual(1, sprite.texture.mipmapCount);
            Assert.IsTrue(sprite.texture.GetPixels32().Any(p => p.a > 0));
        }

        [TestCase("school")][TestCase("diocese")][TestCase("house")]
        public void EveryPhaseUsesOneBoundedFloorLightLayer(string phase)
        {
            var environment = Build(phase);
            var layers = environment.GetComponentsInChildren<VarginhaSoftLighting>();
            Assert.AreEqual(1, layers.Length);
            Assert.Greater(layers[0].SourceCount, 0);
            var output = layers[0].GetComponent<SpriteRenderer>();
            Assert.IsTrue(output.enabled);
            Assert.AreEqual(1, output.sortingOrder);
            Assert.LessOrEqual(output.sprite.texture.GetPixels32().Max(p => p.a), 98);
            if (phase != "house") Assert.Greater(layers[0].OccluderCount, 0);
        }

        [TestCase("GlassLight")][TestCase("WindowLight")][TestCase("Glow")][TestCase("Shadow")]
        public void LightingHasContinuousAlphaInsteadOfSteppedRings(string motif)
        {
            var pixels = VarginhaSceneryArt.Create(motif, new Vector2(4, 3)).texture.GetPixels32();
            Assert.Greater(pixels.Select(p => p.a).Distinct().Count(), 25);
        }
    }
}
