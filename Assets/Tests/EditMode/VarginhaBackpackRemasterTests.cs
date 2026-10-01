using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class VarginhaBackpackRemasterTests
    {
        private static readonly Vector2[] Directions =
        { Vector2.down, Vector2.left, Vector2.right, Vector2.up,
            new(-1,-1), new(1,-1), new(-1,1), new(1,1) };

        [Test]
        public void ManuallyEditedPunchPreviewMatchesThePackagedGameFrames()
        {
            var authored = Resources.Load<Texture2D>(EdelzioBackpackAppearance.UserPunchPath);
            Assert.IsNotNull(authored);
            Assert.AreEqual(new Vector2Int(256, 256), new Vector2Int(authored.width, authored.height));
            int[] samples = { -1, 3, 9, 15 };
            using var appearance = new EdelzioBackpackAppearance();
            for (int d = 0; d < 4; d++) for (int s = 0; s < samples.Length; s++)
            {
                var body = s == 0 ? VarginhaReferenceSprites.EdelzioWalkFrames()[d][0]
                    : VarginhaReferenceSprites.EdelzioAttackFrames()[d][samples[s]];
                var expected = authored.GetPixels(s * 64, (3 - d) * 64, 64, 64);
                foreach (var sprite in new[] { appearance.ComposeFrame(body, d), EdelzioBackpackFrames.Frame(body, d) })
                {
                    Assert.IsNotNull(sprite);
                    var actual = Pixels(sprite);
                    for (int i = 0; i < actual.Length; i++)
                    {
                        if (expected[i].a < .5f && actual[i].a < .5f) continue;
                        var e = (Color32)expected[i]; var a = (Color32)actual[i];
                        int difference = Mathf.Max(Mathf.Abs(e.r-a.r), Mathf.Abs(e.g-a.g), Mathf.Abs(e.b-a.b), Mathf.Abs(e.a-a.a));
                        Assert.LessOrEqual(difference, 3, $"Manual edit lost: direction={d}, sample={samples[s]}, pixel={i%64},{i/64}, sprite={sprite.name}");
                    }
                }
            }
        }

        [Test]
        public void MovementSelectsAllEightEquipmentDirections()
        {
            for (int i = 0; i < Directions.Length; i++)
                Assert.AreEqual(i, EdelzioBackpackAppearance.DirectionIndex(Directions[i]));
            Assert.AreEqual(2, EdelzioBackpackAppearance.DirectionIndex(new Vector2(1,.1f)));
        }

        [Test]
        public void AllEightRemasteredViewsAreAvailableInPackagedResources()
        {
            var atlas = Resources.Load<Texture2D>(EdelzioBackpackAppearance.ResourcePath);
            Assert.IsNotNull(atlas);
            Assert.IsTrue(atlas.isReadable);
            Assert.AreEqual(FilterMode.Point, atlas.filterMode);
            Assert.AreEqual(1, atlas.mipmapCount);
            for (int i = 0; i < 8; i++)
            {
                int opaque = 0;
                foreach (var c in atlas.GetPixels(i%4*32, (1-i/4)*32, 32, 32))
                    if (c.a > .5f) opaque++;
                Assert.Greater(opaque, 70, "Missing equipment view " + i);
            }
        }

        [Test]
        public void StrapsStayOnTheAuthoredTorsoAndKeepTheHeadAndFeetVisible()
        {
            var go = new GameObject("BackpackLayerTest");
            var renderer = go.AddComponent<SpriteRenderer>();
            // Existing scene children can be present without a renderer after hot reload.
            new GameObject("Mochila_Equipada").transform.SetParent(go.transform, false);
            new GameObject("Mochila_Alca").transform.SetParent(go.transform, false);
            using var appearance = new EdelzioBackpackAppearance();
            try
            {
                var walk = VarginhaReferenceSprites.EdelzioWalkFrames();
                for (int d = 0; d < 8; d++)
                for (int frame = 0; frame < 4; frame++)
                {
                    int cardinal = d < 4 ? d : d < 6 ? 0 : 3;
                    renderer.sprite = walk[cardinal][frame];
                    appearance.UpdatePose(go.transform, renderer, d, true);
                    var strap = go.transform.Find("Mochila_Alca").GetComponent<SpriteRenderer>();
                    var original = Pixels(renderer.sprite);
                    var overlay = Pixels(strap.sprite);
                    for (int i = 0; i < overlay.Length; i++)
                    {
                        if (overlay[i].a < .5f) continue;
                        var c = original[i];
                        Assert.Greater(c.a, .5f, "A strap cannot float outside the character");
                        int y = i / (int)renderer.sprite.rect.width;
                        Assert.That(y, Is.InRange(17, 38), "The user's bands belong to the torso and its outline.");
                    }
                }
                appearance.UpdatePose(go.transform, renderer, 2, false);
                Assert.IsFalse(go.transform.Find("Mochila_Alca").GetComponent<SpriteRenderer>().enabled);
                Assert.IsFalse(go.transform.Find("Mochila_Equipada").GetComponent<SpriteRenderer>().enabled);
                Object.DestroyImmediate(go.transform.Find("Mochila_Alca").GetComponent<SpriteRenderer>());
                appearance.UpdatePose(go.transform, renderer, 2, true);
                Assert.IsTrue(go.transform.Find("Mochila_Alca").GetComponent<SpriteRenderer>().enabled);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void DestroyedCachedEquipmentFramesAreRecreatedInsteadOfMakingThePlayerInvisible()
        {
            var body = VarginhaReferenceSprites.EdelzioWalkFrames()[0][0];
            var previous = EdelzioBackpackFrames.Frame(body,0);
            Assert.IsNotNull(previous);
            Object.DestroyImmediate(previous);
            using var appearance = new EdelzioBackpackAppearance();
            var recovered = appearance.GetFrame(body,0);
            Assert.IsTrue(recovered != null, "Unity's destroyed-object wrapper must not reach the renderer.");
            Assert.IsTrue(recovered.texture != null);
            Assert.AreNotSame(previous,recovered);
            Assert.AreSame(recovered,EdelzioBackpackFrames.Frame(body,0));
            Assert.That(recovered.texture.name, Does.StartWith("EdelzioEquipped"));
            Assert.Greater(System.Array.FindAll(Pixels(recovered),c=>c.a>.5f).Length,100);
        }

        [Test]
        public void EveryCurrentWalkPunchAndInteractionHasAnAuthoredEquippedFrame()
        {
            var walk = VarginhaReferenceSprites.EdelzioWalkFrames();
            var attack = VarginhaReferenceSprites.EdelzioAttackFrames();
            using var appearance = new EdelzioBackpackAppearance();
            int count = 0;
            for (int d = 0; d < 8; d++)
            {
                int cardinal = d < 4 ? d : d < 6 ? 0 : 3;
                var bodies = new System.Collections.Generic.List<Sprite>();
                bodies.AddRange(walk[cardinal]); bodies.AddRange(attack[cardinal]);
                for (int r = 0; r < 4; r++) for (int f = 0; f < 4; f++) bodies.Add(VarginhaInteractionSprites.Frame(r,f));
                foreach (var body in bodies)
                {
                    var equipped = EdelzioBackpackFrames.Frame(body,d);
                    Assert.IsNotNull(equipped, body.name + " direction=" + d);
                    Assert.AreEqual(body.rect.size, equipped.rect.size);
                    Assert.AreEqual(body.pivot, equipped.pivot);
                    Assert.AreEqual(body.pixelsPerUnit, equipped.pixelsPerUnit);
                    Assert.AreEqual(FilterMode.Point, equipped.texture.filterMode);
                    var original = Pixels(appearance.ComposeFrame(body,d)); var current = Pixels(equipped);
                    int width=(int)body.rect.width, height=(int)body.rect.height;
                    for(int y=Mathf.CeilToInt(height*.60f); y<height; y++)
                        for(int x=0;x<width;x++)
                        {
                            int i=y*width+x;
                            if(original[i].a==0 && current[i].a==0) continue;
                            Assert.AreEqual((Color32)original[i],(Color32)current[i], "Authored head edit lost in equipped atlas: "+body.name+" direction="+d);
                        }
                    count++;
                }
            }
            Assert.AreEqual(304,count);
        }

        [Test]
        public void AuthoredSideEquipmentHasNoDetachedComponents()
        {
            var go = new GameObject("BackpackAttachmentTest");
            var renderer = go.AddComponent<SpriteRenderer>();
            using var appearance = new EdelzioBackpackAppearance();
            try
            {
                foreach (int direction in new[] { 1, 2 })
                {
                    renderer.sprite = VarginhaReferenceSprites.EdelzioWalkFrames()[direction][0];
                    appearance.UpdatePose(go.transform, renderer, direction, true);
                    var pack = Pixels(go.transform.Find("Mochila_Equipada").GetComponent<SpriteRenderer>().sprite);
                    var strap = Pixels(go.transform.Find("Mochila_Alca").GetComponent<SpriteRenderer>().sprite);
                    for (int i = 0; i < pack.Length; i++) if (strap[i].a > .5f) pack[i] = strap[i];
                    var body = Pixels(renderer.sprite);
                    int first = System.Array.FindIndex(pack, c => c.a > .5f), total = 0, overlap = 0;
                    for (int i = 0; i < pack.Length; i++)
                    {
                        if (pack[i].a <= .5f) continue;
                        total++;
                        if (body[i].a > .5f && body[i].r > .43f && body[i].r > body[i].g * 1.15f && body[i].b < .20f) overlap++;
                    }
                    Assert.Greater(total, 0, "The authored lateral band must be visible.");
                    Assert.Greater(overlap, 0, "The equipment must attach to the shirt at the shoulder.");
                    var visited = new bool[pack.Length];
                    var queue = new System.Collections.Generic.Queue<int>();
                    int width = (int)renderer.sprite.rect.width;
                    for (int seed = first; seed < pack.Length; seed++)
                    {
                        if (visited[seed] || pack[seed].a <= .5f) continue;
                        queue.Enqueue(seed); visited[seed] = true;
                        bool attached = false;
                        while (queue.Count > 0)
                        {
                            int current = queue.Dequeue();
                            for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                            {
                                int x = current % width + dx, y = current / width + dy;
                                if (x < 0 || x >= width || y < 0 || y >= pack.Length / width) continue;
                                int next = y * width + x;
                                attached |= body[next].a > .5f;
                                if (visited[next] || pack[next].a <= .5f) continue;
                                visited[next] = true; queue.Enqueue(next);
                            }
                        }
                        Assert.IsTrue(attached, "No detached strap or backpack pixels may float beside the player.");
                    }
                }
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void PunchingDoesNotEnlargeOrMoveTheBackpackAndBodyGeometryStaysIntact()
        {
            var go = new GameObject("BackpackCombatTest");
            var renderer = go.AddComponent<SpriteRenderer>();
            using var appearance = new EdelzioBackpackAppearance();
            try
            {
                var walk = VarginhaReferenceSprites.EdelzioWalkFrames();
                var attack = VarginhaReferenceSprites.EdelzioAttackFrames();
                for (int d = 0; d < 4; d++)
                {
                    renderer.sprite = walk[d][0];
                    appearance.UpdatePose(go.transform, renderer, d, true);
                    var pack = go.transform.Find("Mochila_Equipada").GetComponent<SpriteRenderer>();
                    var expected = Pixels(pack.sprite);
                    for (int frame = 0; frame < 18; frame++)
                    {
                        renderer.sprite = attack[d][frame];
                        appearance.UpdatePose(go.transform, renderer, d, true);
                        CollectionAssert.AreEqual(expected, Pixels(pack.sprite), "Equipment moved with an extended arm");
                        Assert.AreSame(attack[d][frame], renderer.sprite);
                        Assert.AreEqual(renderer.sprite.pixelsPerUnit, pack.sprite.pixelsPerUnit);
                        Assert.AreEqual(renderer.sprite.pivot, pack.sprite.pivot);
                        Assert.AreEqual(Vector3.one, pack.transform.localScale);
                    }
                }
            }
            finally { Object.DestroyImmediate(go); }
        }

        private static Color[] Pixels(Sprite s) => s.texture.GetPixels((int)s.rect.x, (int)s.rect.y, (int)s.rect.width, (int)s.rect.height);
    }
}
