using System.Linq;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class VarginhaReferenceArtTests
    {
        [Test]
        public void HeartsAndFireHaveVisiblePixelsAndTransparentBackgrounds()
        {
            var sprites = new[] { VarginhaReferenceSprites.Heart(true), VarginhaReferenceSprites.Heart(false),
                VarginhaReferenceSprites.Fire(0), VarginhaReferenceSprites.Fire(1), VarginhaReferenceSprites.Fire(2) };
            foreach (var sprite in sprites)
            {
                Assert.IsNotNull(sprite);
                Assert.AreEqual(FilterMode.Point, sprite.texture.filterMode);
                var pixels = sprite.texture.GetPixels32();
                Assert.Greater(pixels.Count(p => p.a > 0), pixels.Length / 5);
                Assert.Greater(pixels.Count(p => p.a == 0), pixels.Length / 10);
                Assert.AreEqual(0, pixels[0].a, sprite.name);
                Assert.AreEqual(0, pixels[pixels.Length - 1].a, sprite.name);
            }
            Assert.AreSame(VarginhaReferenceSprites.Fire(0), VarginhaReferenceSprites.Fire(3));
        }

        [Test]
        public void SavedFurnitureRefreshKeepsItsTransformAndCollider()
        {
            var root = new GameObject("House_And_Yard");
            try
            {
                var desk = new GameObject("Desk_Office", typeof(SpriteRenderer), typeof(BoxCollider2D));
                desk.transform.SetParent(root.transform);
                desk.transform.position = new Vector3(2, 3, 0);
                desk.transform.localScale = new Vector3(1.7f, .9f, 1);
                var collider = desk.GetComponent<BoxCollider2D>();
                collider.size = new Vector2(.8f, .6f);
                VarginhaEnvironmentPolish.EnsureHouse(root.transform);
                Assert.That(desk.GetComponent<SpriteRenderer>().sprite.name, Does.StartWith("Reference_Wood_Table"));
                Assert.AreEqual(new Vector3(2, 3, 0), desk.transform.position);
                Assert.AreEqual(new Vector3(1.7f, .9f, 1), desk.transform.localScale);
                Assert.AreEqual(new Vector2(.8f, .6f), collider.size);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void DioceseHasSevenAnimatedFlamesWithoutDuplicateChildrenOrColliders()
        {
            var root = new GameObject("ReferenceDioceseTest");
            try
            {
                var church = VarginhaEnvironmentArt.EnsureDiocese(root.transform);
                var flames = church.GetComponentsInChildren<VarginhaFlameAnimation>(true);
                Assert.AreEqual(7, flames.Length);
                VarginhaEnvironmentArt.EnsureDiocese(root.transform);
                Assert.AreEqual(7, church.GetComponentsInChildren<VarginhaFlameAnimation>(true).Length);
                foreach (var flame in flames)
                {
                    Assert.IsNull(flame.GetComponent<Collider2D>());
                    Assert.That(flame.GetComponent<SpriteRenderer>().sprite.name, Does.StartWith("Reference_Fire_"));
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void KatanaPianoAndCagePropsAreValidWithTransparentBackgrounds()
        {
            VarginhaReferenceSprites.ClearCache();
            var katana = VarginhaReferenceSprites.GetProp("Katana");
            Assert.IsNotNull(katana);
            Assert.AreEqual(FilterMode.Point, katana.texture.filterMode);
            var kPixels = katana.texture.GetPixels32();
            Assert.Greater(kPixels.Count(p => p.a > 0), 200);
            Assert.Greater(kPixels.Count(p => p.a == 0), 100);
            Assert.AreEqual(0, kPixels[0].a);

            var piano = VarginhaReferenceSprites.GetProp("Piano");
            Assert.IsNotNull(piano);
            Assert.AreEqual(FilterMode.Point, piano.texture.filterMode);
            var pPixels = piano.texture.GetPixels32();
            Assert.Greater(pPixels.Count(p => p.a > 0), 1000);
            Assert.Greater(pPixels.Count(p => p.a == 0), 200);
            Assert.AreEqual(0, pPixels[0].a);

            var cage = VarginhaReferenceSprites.GetProp("Cage");
            Assert.IsNotNull(cage);
            Assert.AreEqual(FilterMode.Point, cage.texture.filterMode);
            var cPixels = cage.texture.GetPixels32();
            Assert.Greater(cPixels.Count(p => p.a > 0), 1500);
            Assert.Greater(cPixels.Count(p => p.a == 0), 300);
            Assert.AreEqual(0, cPixels[0].a);
        }

        [Test]
        public void EdelzioAndFabioSheetsGenerateValidDirectionalFrames()
        {
            VarginhaReferenceSprites.ClearCache();
            Assert.IsTrue(VarginhaReferenceSprites.HasEdelzio);

            var walk = VarginhaReferenceSprites.EdelzioWalkFrames();
            Assert.IsNotNull(walk);
            Assert.AreEqual(4, walk.Length);
            for (int d = 0; d < 4; d++)
            {
                Assert.AreEqual(4, walk[d].Length);
                for (int f = 0; f < 4; f++)
                {
                    Assert.IsNotNull(walk[d][f]);
                    Assert.AreEqual(FilterMode.Point, walk[d][f].texture.filterMode);
                    Assert.IsTrue(walk[d][f].name.StartsWith("Edelzio_Reference_"));
                }
            }

            var fabioTexture = VarginhaReferenceSprites.FabioAtlas();
            Assert.IsNotNull(fabioTexture);
            Assert.AreEqual(256, fabioTexture.width);
            Assert.AreEqual(256, fabioTexture.height);

            var fabioFrontFrame = VarginhaStudentSprites.Frame("Fabio", 0, 0);
            Assert.IsNotNull(fabioFrontFrame);
            Assert.AreEqual(64, fabioFrontFrame.rect.width);
            Assert.AreEqual(64, fabioFrontFrame.rect.height);

            var fabioPortrait = VarginhaStudentSprites.Portrait("Fabio");
            Assert.IsNotNull(fabioPortrait);
            Assert.AreEqual(fabioTexture, fabioPortrait.texture);
        }

        [Test]
        public void AttackAndHostageSpritesResolveToAuthoredProps()
        {
            VarginhaReferenceSprites.ClearCache();
            var katanaAttack = VarginhaPixelArtSprites.Create("StudentAttack_Katana", Color.white);
            Assert.IsNotNull(katanaAttack);
            Assert.That(katanaAttack.name, Does.StartWith("Reference_Katana"));

            var pianoAttack = VarginhaPixelArtSprites.Create("StudentAttack_FallingPiano", Color.white);
            Assert.IsNotNull(pianoAttack);
            Assert.That(pianoAttack.name, Does.StartWith("Reference_Piano"));

            var hostageCage = VarginhaPixelArtSprites.Create("HostageCage_Fabio", Color.white);
            Assert.IsNotNull(hostageCage);
            Assert.That(hostageCage.name, Does.StartWith("Reference_Cage"));
        }
    }
}
