using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Eight equipment views with shoulder straps fitted to the shirt.</summary>
    public sealed class EdelzioBackpackAppearance : IDisposable
    {
        public const string ResourcePath = "Varginha/Equipment/BackpackRemasterAtlasV1";
        public const string UserDirectionsPath = "Varginha/Equipment/EdelzioBackpackUserDirectionsV2";
        public const string UserPunchPath = "Varginha/Equipment/EdelzioBackpackUserPunchV3";
        public const string PunchBaselinePath = "Varginha/Equipment/EdelzioBackpackPunchBaselineV3";
        private readonly Dictionary<(Sprite, int), Layers> _frames = new();
        private SpriteRenderer _pack, _strap;
        private Texture2D _atlas;
        private Texture2D _userDirections;
        private Texture2D _userPunch, _punchBaseline;
        private static readonly Color32 StrapDark = new(27, 29, 31, 255);
        private static readonly Color32 StrapLight = new(91, 96, 100, 255);
        private static readonly int[] ArtDirection = { 1, 3, 2, 0, 6, 7, 4, 5 };
        private sealed class Layers { public Sprite Pack, Strap, Composite; public bool InFront; }

        // S, W, E, N, SW, SE, NW, NE. Body animation retains its four directions.
        public static int DirectionIndex(Vector2 facing)
        {
            float x = Mathf.Abs(facing.x), y = Mathf.Abs(facing.y);
            if (Mathf.Min(x, y) > .42f * Mathf.Max(x, y))
                return facing.y > 0 ? (facing.x < 0 ? 6 : 7) : (facing.x < 0 ? 4 : 5);
            return y >= x ? (facing.y > 0 ? 3 : 0) : (facing.x < 0 ? 1 : 2);
        }

        public void UpdatePose(Transform player, SpriteRenderer playerSr, int direction, bool visible)
        {
            if (player == null || playerSr == null) return;
            EnsureChild(player);
            if (!visible || playerSr.sprite == null) { _pack.enabled = _strap.enabled = false; return; }
            var layers = GetLayers(playerSr.sprite, Mathf.Clamp(direction, 0, 7));
            if (layers == null) { _pack.enabled = _strap.enabled = false; return; }
            _pack.sprite = layers.Pack;
            _strap.sprite = layers.Strap;
            _pack.sortingLayerID = _strap.sortingLayerID = playerSr.sortingLayerID;
            _pack.sortingOrder = playerSr.sortingOrder + (layers.InFront ? 1 : -1);
            _strap.sortingOrder = playerSr.sortingOrder + 2;
            _pack.transform.localPosition = _strap.transform.localPosition = Vector3.zero;
            _pack.transform.localScale = _strap.transform.localScale = Vector3.one;
            _pack.flipX = _strap.flipX = playerSr.flipX;
            _pack.enabled = direction != 0;
            _strap.enabled = true;
        }

        /// <summary>Flattened preview of the same layers used in gameplay.</summary>
        public Sprite GetFrame(Sprite body, int direction)
        {
            var authored = EdelzioBackpackFrames.Frame(body, direction);
            if (authored != null) return authored;
            var composed = ComposeFrame(body, direction);
            return composed != null ? composed : body;
        }

        /// <summary>Editor authoring path, also covers a newly added pose until its atlas is rebuilt.</summary>
        public Sprite ComposeFrame(Sprite body, int direction) => GetLayers(body, Mathf.Clamp(direction, 0, 7))?.Composite ?? body;

        public static void HideLegacyLayers(Transform player)
        {
            foreach (string name in new[] { "Mochila_Equipada", "Mochila_Alca", "Mochila_Alcas" })
            {
                var child = player.Find(name);
                if (child != null && child.gameObject.activeSelf) child.gameObject.SetActive(false);
            }
        }

        private Layers GetLayers(Sprite body, int direction)
        {
            if (body == null || body.texture == null || !body.texture.isReadable) return null;
            var key = (body, direction);
            if (_frames.TryGetValue(key, out var cached) && cached.Pack != null && cached.Strap != null
                && cached.Composite != null && cached.Composite.texture != null) return cached;
            if (_atlas == null) _atlas = Resources.Load<Texture2D>(ResourcePath);
            if (_atlas == null || !_atlas.isReadable) return null;
            int w = (int)body.rect.width, h = (int)body.rect.height;
            var original = body.texture.GetPixels((int)body.rect.x, (int)body.rect.y, w, h);
            int cardinal = direction < 4 ? direction : direction < 6 ? 0 : 3;
            var anchor = original;
            if (body.name.StartsWith("Edelzio_Attack_"))
            {
                var idle = VarginhaReferenceSprites.EdelzioWalkFrames()?[cardinal][0];
                if (idle != null && idle.rect.size == body.rect.size)
                    anchor = idle.texture.GetPixels((int)idle.rect.x, (int)idle.rect.y, w, h);
            }
            var torso = Torso(anchor, w, h);
            var packPixels = new Color[w * h];
            var strapPixels = new Color[w * h];
            bool inFront = direction == 3 || direction >= 6;
            if (!PlaceUserEquipment(packPixels, strapPixels, original, body, torso, direction))
            {
                if (direction != 0) PlacePack(packPixels, w, h, torso, direction);
                PlaceStraps(strapPixels, original, w, h, torso, direction);
            }
            var composite = new Color[w * h];
            for (int i = 0; i < composite.Length; i++)
            {
                composite[i] = inFront ? Over(original[i], packPixels[i]) : Over(packPixels[i], original[i]);
                composite[i] = Over(composite[i], strapPixels[i]);
            }
            ApplyUserPunchEdits(composite, original, body, torso, direction);
            var layers = new Layers
            {
                Pack = Create(body, packPixels, "Mochila_Volume_" + direction),
                Strap = Create(body, strapPixels, "Mochila_Alca_" + direction),
                Composite = Create(body, composite, body.name + "_ComMochila_" + direction),
                InFront = inFront
            };
            _frames[key] = layers;
            return layers;
        }

        private void ApplyUserPunchEdits(Color[] composite, Color[] original, Sprite body, RectInt torso, int direction)
        {
            bool attack = body.name.StartsWith("Edelzio_Attack_");
            if (!attack && !body.name.StartsWith("Edelzio_Reference_")) return;
            if (body.rect.width != 64 || body.rect.height != 64) return;
            if (_userPunch == null) _userPunch = Resources.Load<Texture2D>(UserPunchPath);
            if (_punchBaseline == null) _punchBaseline = Resources.Load<Texture2D>(PunchBaselinePath);
            if (_userPunch == null || _punchBaseline == null || !_userPunch.isReadable || !_punchBaseline.isReadable) return;
            int cardinal = direction < 4 ? direction : direction < 6 ? 0 : 3;
            int sample = 0;
            if (attack)
            {
                if (!int.TryParse(body.name.Substring(body.name.LastIndexOf('_') + 1), out int frame)) return;
                // Three six-frame punches: use the matching edited strike while active,
                // and the edited idle when each strike begins and returns to rest.
                if (frame % 6 != 0 && frame % 6 != 5) sample = Mathf.Clamp(frame / 6 + 1, 1, 3);
            }
            var edited = _userPunch.GetPixels(sample * 64, (3 - cardinal) * 64, 64, 64);
            var baseline = _punchBaseline.GetPixels(sample * 64, (3 - cardinal) * 64, 64, 64);
            var idle = VarginhaReferenceSprites.EdelzioWalkFrames()[cardinal][0];
            var idlePixels = idle.texture.GetPixels((int)idle.rect.x, (int)idle.rect.y, 64, 64);
            var referenceBody = attack && sample > 0 ? VarginhaReferenceSprites.EdelzioAttackFrames()[cardinal][(sample - 1) * 6 + 3] : idle;
            var referencePixels = referenceBody.texture.GetPixels((int)referenceBody.rect.x, (int)referenceBody.rect.y, 64, 64);
            var idleTorso = Torso(idlePixels, 64, 64);
            int dx = attack ? 0 : Mathf.RoundToInt(torso.center.x - idleTorso.center.x);
            int dy = attack ? 0 : torso.yMax - idleTorso.yMax;
            // The latest manual revision also changes the head and stance during punches.
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                int source = y * 64 + x;
                if (SamePixel(edited[source], baseline[source])) continue;
                int px = x + dx, py = y + dy;
                if (px < 0 || px >= 64 || py < 0 || py >= 64) continue;
                int target = py * 64 + px;
                // A moving limb may vacate an edited pixel between sampled poses.
                if (original[target].a < .5f && referencePixels[source].a > .5f && baseline[source].a > .5f) continue;
                composite[target] = edited[source];
            }
        }

        private bool PlaceUserEquipment(Color[] pack, Color[] straps, Color[] original,
            Sprite body, RectInt torso, int direction)
        {
            if (_userDirections == null) _userDirections = Resources.Load<Texture2D>(UserDirectionsPath);
            if (_userDirections == null || !_userDirections.isReadable) return false;
            int cardinal = direction < 4 ? direction : direction < 6 ? 0 : 3;
            var reference = VarginhaReferenceSprites.EdelzioWalkFrames()[cardinal][0];
            var naked = reference.texture.GetPixels((int)reference.rect.x, (int)reference.rect.y, 64, 64);
            var authored = _userDirections.GetPixels(direction % 4 * 64, (1 - direction / 4) * 64, 64, 64);
            var sourceTorso = Torso(naked, 64, 64);
            int w = (int)body.rect.width, h = (int)body.rect.height;
            float scale = body.pixelsPerUnit / reference.pixelsPerUnit;
            bool back = direction == 3 || direction >= 6;
            // Read only the authored equipment, preserving the current head, hands and feet.
            // Tiny RGB differences from the PNG round trip are not equipment edits.
            for (int y = 17; y <= sourceTorso.yMax + 1; y++)
            for (int x = 0; x < 64; x++)
            {
                var color = authored[y * 64 + x];
                var previous = naked[y * 64 + x];
                if (color.a < .5f || !IsEquipmentColor(color) || SamePixel(color, previous)) continue;
                bool shoulder = !back && previous.a > .5f;
                float scaleX = shoulder ? torso.width / (float)sourceTorso.width : scale;
                float scaleY = shoulder ? torso.height / (float)sourceTorso.height : scale;
                int x0 = Mathf.RoundToInt(torso.center.x + (x - sourceTorso.center.x) * scaleX);
                int y0 = Mathf.RoundToInt(torso.yMax + (y - sourceTorso.yMax) * scaleY);
                int x1 = Mathf.Max(x0 + 1, Mathf.RoundToInt(torso.center.x + (x + 1 - sourceTorso.center.x) * scaleX));
                int y1 = Mathf.Max(y0 + 1, Mathf.RoundToInt(torso.yMax + (y + 1 - sourceTorso.yMax) * scaleY));
                for (int py = Mathf.Max(0, y0); py < Mathf.Min(h, y1); py++)
                for (int px = Mathf.Max(0, x0); px < Mathf.Min(w, x1); px++)
                {
                    int i = py * w + px;
                    if (shoulder)
                    {
                        // Walking can lower the torso by one pixel; keep the source
                        // outline out of the pants below the authored equipment area.
                        if (w == 64 && h == 64 && py < 17) continue;
                        // The user's curved bands include the shirt outline. During an
                        // action, moving skin occludes them instead of inheriting grey pixels.
                        if (IsShirt(original[i]) || IsEquipmentColor(original[i]) && original[i].a > .5f
                            || scale == 1 && SamePixel(original[i], previous)) straps[i] = color;
                    }
                    else pack[i] = color;
                }
            }
            return true;
        }

        private static bool IsEquipmentColor(Color color) =>
            Mathf.Abs(color.r - color.g) < .14f && Mathf.Abs(color.r - color.b) < .14f;

        private static bool SamePixel(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) <= 3f / 255 && Mathf.Abs(a.g - b.g) <= 3f / 255
            && Mathf.Abs(a.b - b.b) <= 3f / 255 && Mathf.Abs(a.a - b.a) <= 3f / 255;

        private void PlacePack(Color[] target, int w, int h, RectInt torso, int direction)
        {
            int art = ArtDirection[direction], cell = _atlas.width / 4;
            var pixels = _atlas.GetPixels(art % 4 * cell, (1 - art / 4) * cell, cell, cell);
            int left = cell, right = -1, bottom = cell, top = -1;
            for (int y = 0; y < cell; y++) for (int x = 0; x < cell; x++)
                if (pixels[y * cell + x].a > .5f)
                { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
            if (right < left) return;
            bool side = direction == 1 || direction == 2;
            int height = Mathf.RoundToInt(h / 64f * (direction == 3 || direction >= 6 ? 18 : 15));
            int width = Mathf.Max(side ? 7 : 3, Mathf.RoundToInt(height * (right - left + 1f) / (top - bottom + 1f)));
            bool west = direction == 1 || direction == 4 || direction == 6;
            int center = Mathf.RoundToInt(torso.center.x);
            if (side) center = west ? torso.xMax + width / 2 - 3 : torso.xMin - width / 2 + 2;
            else if (direction == 4 || direction == 5) center += west ? 9 : -9;
            else if (direction >= 6) center += west ? 2 : -2;
            int x0 = center - width / 2, y0 = torso.yMax - height + (side ? 1 : 0);
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                int px = x0 + x, py = y0 + y;
                if (px < 0 || px >= w || py < 0 || py >= h) continue;
                int sx = left + Mathf.Min(right - left, Mathf.FloorToInt((x + .5f) * (right - left + 1) / width));
                int sy = bottom + Mathf.Min(top - bottom, Mathf.FloorToInt((y + .5f) * (top - bottom + 1) / height));
                var color = pixels[sy * cell + sx];
                if (color.a > .5f) { color.a = 1; target[py * w + px] = color; }
            }
            if (side)
            {
                // The rear shoulder band belongs underneath the body and reaches the bag.
                // The arm can occlude it during a punch without leaving a floating loop.
                var shoulder = Point(torso, west ? .83f : .17f, .95f);
                int nearest = -1;
                float distance = float.MaxValue;
                for (int y = Mathf.Max(y0, torso.yMax - 5); y < Mathf.Min(h, y0 + height); y++)
                for (int x = Mathf.Max(0, x0); x < Mathf.Min(w, x0 + width); x++)
                {
                    int i = y * w + x;
                    if (target[i].a < .5f) continue;
                    float delta = (new Vector2(x, y) - shoulder).sqrMagnitude;
                    if (delta < distance) { nearest = i; distance = delta; }
                }
                if (nearest >= 0)
                {
                    var attachment = new Vector2(nearest % w, nearest / w);
                    int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(shoulder, attachment) * 2));
                    for (int step = 0; step <= steps; step++)
                    {
                        var p = Vector2.Lerp(shoulder, attachment, step / (float)steps);
                        int x = Mathf.RoundToInt(p.x), y = Mathf.RoundToInt(p.y);
                        if (x >= 0 && x < w && y >= 0 && y < h) target[y * w + x] = StrapLight;
                    }
                }
            }
        }

        private static void PlaceStraps(Color[] result, Color[] body, int w, int h, RectInt torso, int direction)
        {
            if (direction == 3 || direction >= 6) return;
            if (direction == 1 || direction == 2)
            {
                // A short padded band hugs the shoulder, without a free-standing loop.
                float rear = direction == 1 ? .83f : .17f;
                float middle = direction == 1 ? .60f : .40f;
                Band(result, body, w, h, Point(torso, rear, .95f), Point(torso, middle, .70f));
                Band(result, body, w, h, Point(torso, middle, .70f), Point(torso, .50f, .12f));
            }
            else
            {
                Band(result, body, w, h, Point(torso, .13f, .94f), Point(torso, .25f, .05f));
                Band(result, body, w, h, Point(torso, .87f, .94f), Point(torso, .75f, .05f));
            }
        }

        private static Vector2 Point(RectInt rect, float x, float y) =>
            new(rect.xMin + x * (rect.width - 1), rect.yMin + y * (rect.height - 1));

        private static void Band(Color[] result, Color[] body, int w, int h, Vector2 from, Vector2 to)
        {
            int steps = Mathf.CeilToInt(Vector2.Distance(from, to) * 2);
            for (int step = 0; step <= steps; step++)
            {
                var point = Vector2.Lerp(from, to, step / (float)Mathf.Max(1, steps));
                int x = Mathf.RoundToInt(point.x), y = Mathf.RoundToInt(point.y);
                for (int dx = 0; dx < 2; dx++)
                {
                    int px = x + dx;
                    if (px < 0 || px >= w || y < 0 || y >= h) continue;
                    int i = y * w + px;
                    if (IsShirt(body[i])) result[i] = dx == 0 ? StrapDark : StrapLight;
                }
            }
        }

        private static RectInt Torso(Color[] pixels, int w, int h)
        {
            int left = w, right = -1, bottom = h, top = -1;
            for (int y = Mathf.RoundToInt(h * .19f); y < Mathf.RoundToInt(h * .60f); y++)
            for (int x = Mathf.RoundToInt(w * .40f); x < Mathf.RoundToInt(w * .60f); x++)
                if (IsShirt(pixels[y * w + x]))
                { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
            return right < left ? new RectInt(w / 2 - 5, h / 3, 11, 10) : new RectInt(left, bottom, right - left + 1, top - bottom + 1);
        }

        private static bool IsShirt(Color c) => c.a > .5f && c.r > .43f && c.g > .27f
            && c.r > c.g * 1.15f && c.b < Mathf.Min(.20f, c.g * .45f);

        private static Color Over(Color back, Color front)
        {
            float alpha = front.a + back.a * (1 - front.a);
            if (alpha <= 0) return Color.clear;
            return new Color((front.r * front.a + back.r * back.a * (1 - front.a)) / alpha,
                (front.g * front.a + back.g * back.a * (1 - front.a)) / alpha,
                (front.b * front.a + back.b * back.a * (1 - front.a)) / alpha, alpha);
        }

        private static Sprite Create(Sprite body, Color[] pixels, string name)
        {
            int w = (int)body.rect.width, h = (int)body.rect.height;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            { name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            texture.SetPixels(pixels); texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, w, h), new Vector2(body.pivot.x/w, body.pivot.y/h), body.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = name; sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private void EnsureChild(Transform parent)
        {
            if (_pack == null) _pack = Child(parent, "Mochila_Equipada");
            if (_strap == null) _strap = Child(parent, "Mochila_Alca");
        }

        private static SpriteRenderer Child(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child == null) { child = new GameObject(name).transform; child.SetParent(parent, false); }
            child.gameObject.hideFlags = HideFlags.DontSave;
            var renderer = child.GetComponent<SpriteRenderer>();
            // Unity's missing-component wrapper must be checked with its overloaded ==.
            if (renderer == null) renderer = child.gameObject.AddComponent<SpriteRenderer>();
            return renderer;
        }

        public void Dispose()
        {
            if (_pack != null) Destroy(_pack.gameObject);
            if (_strap != null) Destroy(_strap.gameObject);
            foreach (var layers in _frames.Values)
                foreach (var sprite in new[] { layers.Pack, layers.Strap, layers.Composite })
                { if (sprite != null) { Destroy(sprite.texture); Destroy(sprite); } }
            _frames.Clear();
            _pack = _strap = null;
        }

        private static void Destroy(UnityEngine.Object item)
        { if (Application.isPlaying) UnityEngine.Object.Destroy(item); else UnityEngine.Object.DestroyImmediate(item); }
    }
}
