using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Cached pixel articulation of the current actor: windup, reach, contact and recovery.</summary>
    public static class VarginhaEdelzioCombatArt
    {
        private static readonly float[] Reach = { .08f, -.24f, .55f, 1f, .72f, .12f };

        public static Sprite Frame(Sprite source, int direction, int phase, int combo)
        {
            const int size = 64;
            var input = source.texture.GetPixels((int)source.rect.x, (int)source.rect.y, size, size);
            var output = new Color[size * size];
            float reach = Reach[phase];
            bool side = direction == 1 || direction == 2;
            int facing = direction == 1 ? -1 : 1;
            int handSide = combo == 1 ? -1 : 1;
            int lean = Mathf.RoundToInt(reach * (combo == 2 ? 3 : 2));
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                if (input[y * size + x].a == 0) continue;
                // Keep the foot baseline and head identity; rotate the torso into the strike.
                int dx = y > 19 ? (side ? facing : handSide) * lean : 0;
                int dy = y > 19 && combo == 2 && phase == 1 ? -2 : 0;
                int px = x + dx, py = y + dy;
                if (px >= 0 && px < size && py >= 0 && py < size) output[py * size + px] = input[y * size + x];
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { name = "Edelzio_Golpe_" + direction + "_" + combo + "_" + phase,
                filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels(output);
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), source.pivot / size, source.pixelsPerUnit);
            sprite.name = texture.name;
            return sprite;
        }

        private static void Line(Color[] pixels, Vector2 start, Vector2 end, int radius, Color color)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(start, end)));
            for (int i = 0; i <= steps; i++)
            {
                Vector2 point = Vector2.Lerp(start, end, i / (float)steps);
                int cx = Mathf.RoundToInt(point.x), cy = Mathf.RoundToInt(point.y);
                for (int y = -radius + 1; y < radius; y++) for (int x = -radius + 1; x < radius; x++)
                {
                    int px = cx + x, py = cy + y;
                    if (px > 0 && px < 63 && py > 0 && py < 63) pixels[py * 64 + px] = color;
                }
            }
        }
    }
}
