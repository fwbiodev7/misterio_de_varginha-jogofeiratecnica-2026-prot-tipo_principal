using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class VarginhaVisualPolishTests
    {
        [Test]
        public void ProtagonistIsLargerWithoutMovingHisFootBaseline()
        {
            var actor = VarginhaReferenceSprites.EdelzioWalkFrames()[0][0];
            var student = VarginhaStudentSprites.Frame("Matias", 0, 0);
            Assert.AreEqual(1.18f, student.pixelsPerUnit / actor.pixelsPerUnit, .001f);
            float originalFoot = (6f - 32f) / student.pixelsPerUnit;
            float enlargedFoot = (6f - actor.pivot.y) / actor.pixelsPerUnit;
            Assert.AreEqual(originalFoot, enlargedFoot, .001f);
        }

        [Test]
        public void EveryAttackHasDedicatedWindupContactAndRecoveryWithStableGeometry()
        {
            var walk = VarginhaReferenceSprites.EdelzioWalkFrames();
            var attacks = VarginhaReferenceSprites.EdelzioAttackFrames();
            for (int d = 0; d < 4; d++)
            {
                Assert.AreEqual(18, attacks[d].Length);
                for (int combo = 0; combo < 3; combo++)
                {
                    var windup = attacks[d][combo * 6 + 1];
                    var contact = attacks[d][combo * 6 + 3];
                    Assert.AreNotSame(walk[d][0], contact);
                    CollectionAssert.AreNotEqual(FramePixels(windup), FramePixels(contact));
                    Assert.AreEqual(walk[d][0].pixelsPerUnit / .92f, contact.pixelsPerUnit, .001f);
                    Assert.AreEqual((6f - walk[d][0].pivot.y) / walk[d][0].pixelsPerUnit,
                        (6f - contact.pivot.y) / contact.pixelsPerUnit, .001f);
                    Assert.AreEqual(FilterMode.Point, contact.texture.filterMode);
                    var pixels = FramePixels(contact);
                    for (int i = 0; i < 64; i++)
                    {
                        Assert.AreEqual(0, pixels[i].a, "No clipping at bottom edge");
                        Assert.AreEqual(0, pixels[63 * 64 + i].a, "No clipping at top edge");
                        Assert.AreEqual(0, pixels[i * 64].a);
                        Assert.AreEqual(0, pixels[i * 64 + 63].a);
                    }
                }
            }
        }

        [Test]
        public void CosmeticStrikeRetouchPreservesAllAuthoredSilhouettesAndFootPixels()
        {
            var atlas = Resources.Load<Texture2D>("Varginha/EdelzioPunchV2");
            var attacks = VarginhaReferenceSprites.EdelzioAttackFrames();
            int changed = 0;
            for (int direction = 0; direction < 4; direction++)
            for (int frame = 0; frame < 18; frame++)
            {
                var authored = atlas.GetPixels(64 * frame, (3 - direction) * 64, 64, 64);
                var current = FramePixels(attacks[direction][frame]);
                for (int i = 0; i < authored.Length; i++)
                {
                    Assert.AreEqual(authored[i].a, current[i].a, "The authored fist and limb silhouette must remain intact.");
                    if (i < 20 * 64) Assert.AreEqual(authored[i], current[i], "Feet and their baseline must remain intact.");
                    if (authored[i] != current[i]) changed++;
                }
            }
            Assert.Greater(changed, 0, "The strike palette and upper face must reflect the approved appearance.");
        }

        private static Color[] FramePixels(Sprite sprite) => sprite.texture.GetPixels(
            (int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);

        [Test]
        public void ClearCombatPoseClearsAttackPose()
        {
            var go = new GameObject("PlayerTest");
            var anim = go.AddComponent<VarginhaPlayerSpriteAnimation>();
            var testSprite = Sprite.Create(new Texture2D(4, 4), new Rect(0, 0, 4, 4), Vector2.one * .5f);
            testSprite.name = "TestPose";
            
            anim.SetCombatPose(testSprite, Vector2.down);
            Assert.IsTrue(anim.HasActionPose);

            anim.ClearCombatPose();
            Assert.IsFalse(anim.HasActionPose);
            Object.DestroyImmediate(go);
        }
    }
}
