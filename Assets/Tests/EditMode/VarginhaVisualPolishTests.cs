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
                    CollectionAssert.AreNotEqual(windup.texture.GetPixels32(), contact.texture.GetPixels32());
                    Assert.AreEqual(walk[d][0].pivot, contact.pivot);
                    Assert.AreEqual(walk[d][0].pixelsPerUnit, contact.pixelsPerUnit);
                    Assert.AreEqual(FilterMode.Point, contact.texture.filterMode);
                    var pixels = contact.texture.GetPixels32();
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
