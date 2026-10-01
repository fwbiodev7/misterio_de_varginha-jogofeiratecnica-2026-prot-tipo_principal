using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Exact idle head and planted legs; only the authored boxing torso/arms articulate.</summary>
    public static class VarginhaEdelzioCombatMatch
    {
        private const int Size = 64;
        public static void Apply(Color[] strike, Sprite walking, int direction)
        {
            if (walking == null) return;
            var reference = walking.texture.GetPixels((int)walking.rect.x, (int)walking.rect.y, Size, Size);
            ShirtBand(reference, out int waist, out int neck);
            ShirtBand(strike, out int sourceWaist, out int sourceNeck);
            var input = (Color[])strike.Clone();
            var restingHands = RestingHands(reference, waist);
            System.Array.Clear(strike, 0, strike.Length);
            var oldHead = HeadBounds(input, sourceNeck + 1);
            var head = HeadBounds(reference, neck + 1);
            float oldCenter = oldHead.center.x, center = head.center.x;
            float horizontal = head.width / (float)Mathf.Max(1, oldHead.width);
            var palette = new System.Collections.Generic.List<Color>();
            foreach (var color in reference) if (IsShirt(color) && !palette.Contains(color)) palette.Add(color);
            for (int y = waist; y <= neck; y++)
            for (int x = 1; x < 63; x++)
            {
                int sx = Mathf.Clamp(Mathf.FloorToInt(oldCenter + (x + .5f - center) / horizontal), 0, 63);
                int sy = sourceWaist + Mathf.Min(sourceNeck - sourceWaist,
                    (y - waist) * (sourceNeck - sourceWaist + 1) / Mathf.Max(1, neck - waist + 1));
                var color = input[sy * Size + sx];
                if (IsShirt(color) && palette.Count > 0)
                {
                    var closest = palette[0]; float distance = float.MaxValue;
                    foreach (var sample in palette)
                    {
                        float next = Mathf.Abs(Luminance(sample) - Luminance(color));
                        if (next < distance) { closest = sample; distance = next; }
                    }
                    closest.a = color.a; color = closest;
                }
                strike[y * Size + x] = color;
            }
            // Raised back-facing fists remain above the shoulder, outside the old head.
            if (direction == 3)
                for (int y = sourceNeck + 1; y < 63; y++)
                for (int x = 1; x < 63; x++)
                {
                    if (oldHead.Contains(new Vector2Int(x, y))) continue;
                    var color = input[y * Size + x];
                    if (color.a < .5f) continue;
                    int px = Mathf.RoundToInt(center + (x - oldCenter) * horizontal);
                    if (px > 0 && px < 63) strike[y * Size + px] = color;
                }
            // Pixel-for-pixel identity, including glasses, beard and foot support.
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                int i = y * Size + x;
                if (y < waist) strike[i] = restingHands[i] ? Color.clear : reference[i];
                else if (y > neck && (direction != 3 || head.Contains(new Vector2Int(x,y)) || reference[i].a > 0))
                    strike[i] = reference[i];
            }
        }
        private static bool IsShirt(Color c) => c.a > .5f && c.r > .43f && c.g > .27f
            && c.r > c.g * 1.15f && c.b < Mathf.Min(.20f, c.g * .45f);
        private static bool[] RestingHands(Color[] reference, int waist)
        {
            var mask = new bool[Size * Size];
            // The idle's hanging hands overlap the hip band. Copying the entire lower
            // image into a punch creates a third arm. Remove their skin and one-pixel
            // outline while retaining the canonical legs and foot support.
            for (int y = 12; y < waist; y++)
            for (int x = 1; x < Size - 1; x++)
            {
                var c = reference[y * Size + x];
                if (c.a < .5f || c.r < .55f || c.g < .3f || c.r < c.g * 1.08f || c.b < c.g * .58f) continue;
                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (y + dy >= 12 && y + dy < waist) mask[(y + dy) * Size + x + dx] = true;
            }
            return mask;
        }
        private static float Luminance(Color c) => c.r * .3f + c.g * .59f + c.b * .11f;
        private static void ShirtBand(Color[] pixels, out int waist, out int neck)
        {
            waist = 64; neck = 0;
            for (int y = 12; y < 38; y++) for (int x = 29; x < 35; x++)
                if (IsShirt(pixels[y * Size + x])) { waist = Mathf.Min(waist,y); neck = Mathf.Max(neck,y); }
            if (waist > neck) { waist = 20; neck = 29; }
        }
        private static RectInt HeadBounds(Color[] pixels, int bottom)
        {
            int left=64,right=0,top=bottom;
            for (int y=bottom;y<63;y++) for(int x=19;x<45;x++)
                if(pixels[y*Size+x].a>.5f && pixels[y*Size+x].r<.55f)
                { left=Mathf.Min(left,x);right=Mathf.Max(right,x);top=Mathf.Max(top,y); }
            return left>right ? new RectInt(22,bottom,20,20) : new RectInt(left-1,bottom,right-left+3,top-bottom+2);
        }
    }
}
