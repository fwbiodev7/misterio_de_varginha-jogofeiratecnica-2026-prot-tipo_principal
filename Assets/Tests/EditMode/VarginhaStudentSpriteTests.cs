using System.Collections.Generic;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public sealed class VarginhaStudentSpriteTests
    {
        [TestCase("Yasmin")]
        [TestCase("Pedro")]
        [TestCase("Matias")]
        [TestCase("Fabio")]
        [TestCase("Marcos")]
        [TestCase("Anna Sabia")]
        [TestCase("Ana Tavares")]
        [TestCase("Luis Miguel Messias")]
        [TestCase("Luis Martins")]
        public void EveryStudentUsesTheCanonicalAtlasForWalkingAndPortraits(string student)
        {
            var first = VarginhaStudentSprites.Frame(student);
            Assert.IsNotNull(first, "Atlas ausente: " + student);
            Assert.AreEqual(256, first.texture.width);
            Assert.AreEqual(256, first.texture.height);
            Assert.AreEqual(FilterMode.Point, first.texture.filterMode);
            Assert.AreEqual(1, first.texture.mipmapCount);
            var portrait = VarginhaStudentSprites.Portrait(student);
            Assert.IsNotNull(portrait);
            Assert.AreSame(first.texture, portrait.texture);
            Assert.That(portrait.rect.yMin, Is.GreaterThanOrEqualTo(192));
            Assert.That(portrait.rect.yMax, Is.LessThanOrEqualTo(256));

            Color32[] pixels = first.texture.GetPixels32();
            for (int direction = 0; direction < 4; direction++)
            {
                var poses = new HashSet<string>();
                for (int frame = 0; frame < 4; frame++)
                {
                    var sprite = VarginhaStudentSprites.Frame(student, direction, frame);
                    Assert.AreSame(first.texture, sprite.texture);
                    Assert.AreEqual(new Rect(frame * 64, (3 - direction) * 64, 64, 64), sprite.rect);
                    RectInt bounds = VisibleBounds(sprite);
                    Assert.AreEqual(6, bounds.yMin, student + " precisa manter os pés alinhados.");
                    Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(4));
                    Assert.That(bounds.xMax, Is.LessThanOrEqualTo(60));
                    Assert.That(bounds.yMax, Is.LessThanOrEqualTo(62));
                    var rgba = new byte[64 * 64 * 4];
                    for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 64; x++)
                    {
                        Color32 pixel = pixels[((3 - direction) * 64 + y) * 256 + frame * 64 + x];
                        int index = (y * 64 + x) * 4;
                        rgba[index] = pixel.r; rgba[index + 1] = pixel.g;
                        rgba[index + 2] = pixel.b; rgba[index + 3] = pixel.a;
                    }
                    poses.Add(System.Convert.ToBase64String(rgba));
                }
                Assert.AreEqual(4, poses.Count, student + " precisa de quatro poses distintas por direção.");
            }
            RectInt front = VisibleBounds(first);
            Assert.That(portrait.rect.yMax, Is.GreaterThanOrEqualTo(192 + front.yMax), "Retrato não pode cortar o cabelo.");
        }

        [Test]
        public void ShortAnnaAndTallAnaKeepDifferentHeights()
        {
            int shortHeight = VisibleBounds(VarginhaStudentSprites.Frame("Anna Sabia")).height;
            int regularHeight = VisibleBounds(VarginhaStudentSprites.Frame("Yasmin")).height;
            int tallHeight = VisibleBounds(VarginhaStudentSprites.Frame("Ana Tavares")).height;
            Assert.That(regularHeight - shortHeight, Is.GreaterThanOrEqualTo(4));
            Assert.That(tallHeight - regularHeight, Is.GreaterThanOrEqualTo(4));
        }

        [TestCase("Anna Tavares", "Ana Tavares")]
        [TestCase("Luis Messias", "Luis Miguel Messias")]
        public void BriefNameVariantsResolveToTheExistingStudent(string briefName, string gameName)
        {
            Assert.AreEqual(VarginhaStudentSprites.AssetName(gameName), VarginhaStudentSprites.AssetName(briefName));
            Assert.AreSame(VarginhaStudentSprites.Frame(gameName), VarginhaStudentSprites.Frame(briefName));
        }

        private static RectInt VisibleBounds(Sprite sprite)
        {
            Assert.IsNotNull(sprite);
            Color32[] pixels = sprite.texture.GetPixels32();
            int left = 64, bottom = 64, right = -1, top = -1;
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                if (pixels[((int)sprite.rect.y + y) * sprite.texture.width + (int)sprite.rect.x + x].a < 128) continue;
                left = Mathf.Min(left, x); right = Mathf.Max(right, x);
                bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y);
            }
            Assert.That(right, Is.GreaterThanOrEqualTo(left));
            return new RectInt(left, bottom, right - left + 1, top - bottom + 1);
        }
    }
}
