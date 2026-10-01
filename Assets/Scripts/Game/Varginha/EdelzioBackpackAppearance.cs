using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Composes the collected backpack artwork onto each pose without changing its geometry.</summary>
    public sealed class EdelzioBackpackAppearance : IDisposable
    {
        private readonly Dictionary<(Sprite, int), Sprite> _frames = new();
        private static Color[] _packPixels;
        private static int _packWidth, _packHeight;
        private static readonly Color32 Strap = new(52, 54, 58, 255);
        private static readonly Color32 Brass = new(180, 185, 192, 255);

        private static bool LoadPack()
        {
            if (_packPixels != null) return true;
            foreach (var sprite in Resources.LoadAll<Sprite>("Varginha/HouseReference512/Props"))
            {
                if (sprite.name != "House512_Backpack" || !sprite.texture.isReadable) continue;
                var rect = sprite.rect;
                int w = (int)rect.width, h = (int)rect.height;
                var pixels = sprite.texture.GetPixels((int)rect.x, (int)rect.y, w, h);
                int left = w, right = -1, bottom = h, top = -1;
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a <= .5f) continue;
                    left = Mathf.Min(left, x); right = Mathf.Max(right, x);
                    bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y);
                }
                if (right < left) continue;
                _packWidth = right - left + 1; _packHeight = top - bottom + 1;
                _packPixels = sprite.texture.GetPixels((int)rect.x + left, (int)rect.y + bottom, _packWidth, _packHeight);
                return true;
            }
            return false;
        }

        public Sprite GetFrame(Sprite body, int direction)
        {
            if (body == null || body.texture == null || !body.texture.isReadable || !LoadPack()) return body;
            var key = (body, direction);
            if (_frames.TryGetValue(key, out var cached) && cached != null) return cached;
            Rect rect = body.rect;
            int width = Mathf.RoundToInt(rect.width), height = Mathf.RoundToInt(rect.height);
            var source = body.texture.GetPixels((int)rect.x, (int)rect.y, width, height);
            var original = new Color32[source.Length];
            for (int i = 0; i < source.Length; i++) original[i] = source[i];
            var pixels = (Color32[])original.Clone();
            // A punching arm changes the shirt's bounding box, not the backpack's size.
            // Anchor combat equipment to the same torso used by the directional idle.
            var anchor = original;
            if (body.name.StartsWith("Edelzio_Attack_"))
            {
                var idle = VarginhaReferenceSprites.EdelzioWalkFrames()?[direction][0];
                if (idle != null && idle.rect.size == body.rect.size)
                {
                    var idlePixels = idle.texture.GetPixels((int)idle.rect.x, (int)idle.rect.y, width, height);
                    anchor = new Color32[idlePixels.Length];
                    for (int i = 0; i < anchor.Length; i++) anchor[i] = idlePixels[i];
                }
            }
            int left = width, right = -1, bottom = height, top = -1;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (!IsShirt(anchor[y * width + x])) continue;
                left = Mathf.Min(left, x); right = Mathf.Max(right, x);
                bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y);
            }
            if (right >= left)
            {
                int shirtWidth = right - left + 1, shirtHeight = top - bottom + 1;
                if (direction == 0)
                {
                    int[] straps = { left + Mathf.RoundToInt(shirtWidth * .18f), right - Mathf.RoundToInt(shirtWidth * .18f) };
                    foreach (int x in straps)
                    for (int y = bottom; y <= top; y++)
                        if (IsShirt(original[y * width + x]))
                            pixels[y * width + x] = y == bottom + shirtHeight / 2 ? Brass : Strap;
                }
                else
                {
                    bool rear = direction == 3;
                    int packWidth = Mathf.Max(3, Mathf.RoundToInt(shirtWidth * (rear ? .80f : .48f)));
                    int packHeight = Mathf.Max(4, Mathf.Min(Mathf.RoundToInt(shirtWidth * .88f), shirtHeight + (rear ? 1 : 2)));
                    int x0 = rear ? (left + right - packWidth + 1) / 2
                        : direction == 1 ? right - packWidth / 2 : left - packWidth / 2;
                    int y0 = top + 1 - packHeight;
                    for (int y = 0; y < packHeight; y++)
                    for (int x = 0; x < packWidth; x++)
                    {
                        int targetX = x0 + x, targetY = y0 + y;
                        if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height) continue;
                        int i = targetY * width + targetX;
                        if (!rear && original[i].a > 0 && (IsSkin(original[i])
                            || (direction == 1 ? targetX < left + shirtWidth / 2 : targetX > left + shirtWidth / 2))) continue;
                        var color = _packPixels[Mathf.Min(_packHeight - 1, y * _packHeight / packHeight) * _packWidth
                            + Mathf.Min(_packWidth - 1, x * _packWidth / packWidth)];
                        if (color.a > .5f) pixels[i] = color;
                    }
                }
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = body.name + "_ComMochila_" + direction, filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave
            };
            texture.SetPixels32(pixels); texture.Apply(false, false);
            var result = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(body.pivot.x / width, body.pivot.y / height), body.pixelsPerUnit, 0, SpriteMeshType.FullRect, body.border);
            result.name = texture.name; result.hideFlags = HideFlags.DontSave;
            _frames[key] = result;
            return result;
        }

        private static bool IsShirt(Color32 c) => c.a > 128 && c.r > 110 && c.g > 70 && c.r > c.g * 1.15f && c.b < c.g * .58f;
        private static bool IsSkin(Color32 c) => c.a > 128 && c.r > 140 && c.g > 85 && c.b >= c.g * .58f;

        public void Dispose()
        {
            foreach (var sprite in _frames.Values)
            {
                if (sprite == null) continue;
                Destroy(sprite.texture); Destroy(sprite);
            }
            _frames.Clear();
        }

        private static void Destroy(UnityEngine.Object item)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(item);
            else UnityEngine.Object.DestroyImmediate(item);
        }
    }
}
