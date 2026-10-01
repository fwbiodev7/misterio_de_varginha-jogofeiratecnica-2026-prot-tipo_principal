using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Retouches the authored strikes with the approved walking palette and upper-face details.
    /// Original alpha, fists, limbs, feet and timing remain intact.</summary>
    public static class VarginhaEdelzioCombatAppearance
    {
        private const int Size = 64;
        private static readonly Vector2Int[] Neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        public static void Apply(Color[] strike, Sprite walking, int direction)
        {
            if (walking == null) return;
            var reference = walking.texture.GetPixels((int)walking.rect.x, (int)walking.rect.y, Size, Size);
            var shirt = new List<Color>();
            foreach (var color in reference)
                if (IsShirt(color) && !shirt.Contains(color)) shirt.Add(color);
            if (shirt.Count == 0) return;
            var head = HairBounds(strike);
            var approvedHead = HairBounds(reference);
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                if (y < 20) continue;
                int i = y * Size + x;
                Color original = strike[i];
                if (IsShirt(original))
                {
                    Color closest = shirt[0];
                    float distance = float.MaxValue;
                    foreach (var color in shirt)
                    {
                        float next = Mathf.Abs(Luminance(color) - Luminance(original));
                        if (next >= distance) continue;
                        closest = color; distance = next;
                    }
                    closest.a = original.a;
                    strike[i] = closest;
                }
                if (head.width == 0 || approvedHead.width == 0 || !head.Contains(new Vector2Int(x, y))
                    || original.a < .5f || original.r + original.g + original.b < .27f) continue;
                int rx = approvedHead.x + Mathf.Min(approvedHead.width - 1, (x - head.x) * approvedHead.width / head.width);
                int ry = approvedHead.y + Mathf.Min(approvedHead.height - 1, (y - head.y) * approvedHead.height / head.height);
                Color detail = reference[ry * Size + rx];
                if (detail.a < .5f || IsShirt(detail)) continue;
                detail.a = original.a;
                strike[i] = detail;
            }
            // The front-facing left wrist is on screen right. A single dark watch pixel
            // is enough at this scale; skip when the hand is raised in front of the face.
            if (direction == 0 && head.width > 0)
            {
                int center = head.x + head.width / 2;
                for (int y = 21; y < 30; y++)
                for (int x = center + 4; x < Mathf.Min(Size - 1, center + 10); x++)
                    if (IsSkin(strike[y * Size + x]) && IsShirt(strike[(y + 1) * Size + x]))
                    {
                        strike[y * Size + x] = new Color(.09f, .085f, .08f, strike[y * Size + x].a);
                        return;
                    }
            }
        }

        private static float Luminance(Color c) => c.r * .3f + c.g * .59f + c.b * .11f;
        private static bool IsShirt(Color c) => c.a > .5f && c.r > .43f && c.g > .27f
            && c.r > c.g * 1.15f && c.b < c.g * .58f;
        private static bool IsSkin(Color c) => c.a > .5f && c.r > .55f && c.g > .33f && c.b >= c.g * .58f;
        private static bool IsHair(Color c) => c.a > .5f && c.r > .11f && c.r < .48f
            && c.g > .07f && c.g < .4f && c.b < .35f && c.r > c.g * 1.12f;

        private static RectInt HairBounds(Color[] pixels)
        {
            var seen = new bool[pixels.Length];
            var queue = new Queue<int>();
            int largest = 0;
            RectInt best = default;
            for (int y = 32; y < Size; y++)
            for (int x = 12; x < 52; x++)
            {
                int start = y * Size + x;
                if (seen[start] || !IsHair(pixels[start])) continue;
                seen[start] = true; queue.Enqueue(start);
                int count = 0, left = x, right = x, bottom = y, top = y;
                while (queue.Count > 0)
                {
                    int current = queue.Dequeue(), cx = current % Size, cy = current / Size;
                    count++; left = Mathf.Min(left, cx); right = Mathf.Max(right, cx);
                    bottom = Mathf.Min(bottom, cy); top = Mathf.Max(top, cy);
                    foreach (var offset in Neighbors)
                    {
                        int nx = cx + offset.x, ny = cy + offset.y;
                        if (nx < 12 || nx >= 52 || ny < 32 || ny >= Size) continue;
                        int next = ny * Size + nx;
                        if (seen[next] || !IsHair(pixels[next])) continue;
                        seen[next] = true; queue.Enqueue(next);
                    }
                }
                if (count <= largest) continue;
                largest = count;
                int width = right - left + 3;
                // Only the upper head: raised fists and the existing jaw/goatee are preserved.
                int minY = Mathf.Max(36, top + 2 - Mathf.RoundToInt(width * .7f));
                best = new RectInt(left - 1, minY, width, top + 2 - minY);
            }
            return best;
        }
    }
}
