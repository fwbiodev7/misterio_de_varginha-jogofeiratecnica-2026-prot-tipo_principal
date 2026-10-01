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
                    Assert.AreEqual(walk[d][0].pixelsPerUnit, contact.pixelsPerUnit, .001f);
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
        public void EveryStrikeKeepsExactIdleHeadFeetAndReturnPose()
        {
            var walk = VarginhaReferenceSprites.EdelzioWalkFrames();
            var attacks = VarginhaReferenceSprites.EdelzioAttackFrames();
            int changed = 0;
            for (int direction = 0; direction < 4; direction++)
            for (int frame = 0; frame < 18; frame++)
            {
                var authored = FramePixels(walk[direction][0]);
                var current = FramePixels(attacks[direction][frame]);
                for (int i = 0; i < authored.Length; i++)
                {
                    int y = i / 64;
                    if (y < 12 || (y >= 36 && (direction != 3 || authored[i].a > .5f)))
                        Assert.AreEqual(authored[i], current[i], "Idle head and planted feet must remain identical.");
                    if (frame % 6 == 0 || frame % 6 == 5)
                        Assert.AreEqual(authored[i], current[i], "Guard and recovery reconnect exactly to idle.");
                    if (authored[i] != current[i]) changed++;
                }
            }
            Assert.Greater(changed, 0, "The arms must articulate into a strike.");
        }

        private static Color[] FramePixels(Sprite sprite) => sprite.texture.GetPixels(
            (int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);

        [Test]
        public void RenderedCombatHeadMatchesWholeIdleHeadWithAndWithoutBackpack()
        {
            var walk = VarginhaReferenceSprites.EdelzioWalkFrames();
            var attacks = VarginhaReferenceSprites.EdelzioAttackFrames();
            int[] headBottom = { 29, 30, 31, 35 };
            using var backpack = new EdelzioBackpackAppearance();
            for (int direction = 0; direction < 4; direction++)
            foreach (bool equipped in new[] { false, true })
            {
                var idle = equipped ? backpack.GetFrame(walk[direction][0], direction) : walk[direction][0];
                var expected = FramePixels(idle);
                for (int frame = 0; frame < attacks[direction].Length; frame++)
                {
                    var pose = attacks[direction][frame];
                    var actual = FramePixels(equipped ? backpack.GetFrame(pose, direction) : pose);
                    for (int y = headBottom[direction]; y < 64; y++)
                    for (int x = 21; x < 43; x++)
                        Assert.AreEqual(expected[y * 64 + x], actual[y * 64 + x],
                            $"Whole head changed: direction={direction}, frame={frame}, backpack={equipped}, pixel={x},{y}");
                }
            }
        }

        [Test]
        public void ContactFramesDoNotRetainTheIdleHandsAtTheHips()
        {
            var attacks = VarginhaReferenceSprites.EdelzioAttackFrames();
            for (int direction = 0; direction < 3; direction++)
            foreach (int frame in new[] { 3, 9, 15 })
            {
                var pixels = FramePixels(attacks[direction][frame]);
                for (int y = 12; y < 20; y++)
                for (int x = 0; x < 64; x++)
                {
                    var c = pixels[y * 64 + x];
                    bool skin = c.a > .5f && c.r > .55f && c.g > .3f
                        && c.r > c.g * 1.08f && c.b >= c.g * .58f;
                    Assert.IsFalse(skin, $"Extra idle hand below the punching arms: {direction}/{frame} at {x},{y}");
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
