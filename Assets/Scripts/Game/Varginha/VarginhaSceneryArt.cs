using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Environment art drawn at 32 pixels per world unit, including wide surfaces.</summary>
    public static class VarginhaSceneryArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new();
        private static readonly Color Ink = Hex(0x171c29);
        private static readonly Color Gold = Hex(0xc6a365);
        private static readonly Color[] GlassColors = { Hex(0x568ed5), Hex(0xd95c64), Hex(0xdeb96b), Hex(0x8e70bb), Hex(0x65b4aa) };

        public static Sprite Create(string motif, Vector2 size)
        {
            if (motif == "Desk" || motif == "Pew" || motif == "Altar")
            {
                var reference = VarginhaReferenceSprites.Wood(motif, size);
                if (reference != null) return reference;
            }
            int width = Mathf.Max(4, Mathf.RoundToInt(size.x * 32));
            int height = Mathf.Max(4, Mathf.RoundToInt(size.y * 32));
            string key = motif + "_" + width + "x" + height;
            if (Cache.TryGetValue(key, out var cached) && cached != null && cached.texture != null) return cached;
            var canvas = new Canvas(width, height);
            if (motif == "Glass") Glass(canvas);
            else if (motif == "GlassLight") Projection(canvas, true);
            else if (motif == "WindowLight") Projection(canvas, false);
            else if (motif == "Glow") Glow(canvas, false);
            else if (motif == "Shadow") Glow(canvas, true);
            else if (motif == "Window") Window(canvas);
            else if (motif == "Books") Books(canvas);
            else if (motif == "Noticeboard") Noticeboard(canvas);
            else if (motif == "Plant") Plant(canvas);
            else if (motif == "Column") Column(canvas);
            else if (motif == "Rug") Rug(canvas);
            else if (motif == "Puddle") Puddle(canvas);
            else if (motif == "Leaves") Leaves(canvas);
            else if (motif == "Parking") Parking(canvas);
            else if (motif == "Mosaic") Mosaic(canvas);
            else if (motif == "Sconce") Sconce(canvas);
            else if (motif == "Pew") Pew(canvas);
            else if (motif == "Altar") Altar(canvas);
            else if (motif == "Desk") Desk(canvas);
            else if (motif == "Blackboard") Blackboard(canvas);
            else if (motif == "StreetLamp") StreetLamp(canvas);
            else if (motif == "Dust") { canvas.Rect(0, 0, width, height, Color.white); }
            else throw new System.ArgumentException("Unknown scenery motif: " + motif);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            { name = "Cenario_" + key, filterMode = IsSoftLighting(motif) ? FilterMode.Bilinear : FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp, anisoLevel = 0 };
            texture.SetPixels32(canvas.Pixels);
            // Same restrained material grain as the house/yard, including school and church props.
            // Light projections retain their continuous alpha and never receive surface noise.
            if (!IsSoftLighting(motif)) VarginhaPixelArtSprites.AddSurfaceFinish(texture, motif);
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), 32f, 0, SpriteMeshType.FullRect);
            sprite.name = texture.name;
            Cache[key] = sprite;
            return sprite;
        }

        private static Color Hex(int rgb, float alpha = 1f) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, alpha);
        public static bool IsSoftLighting(string motif) => motif == "GlassLight" || motif == "WindowLight" || motif == "Glow" || motif == "Shadow";

        // Both the glass and its projected light use the same pane pattern.
        private static Color Pane(float u, float v)
        {
            int column = Mathf.Clamp((int)(u * 3), 0, 2);
            int row = Mathf.Clamp((int)(v * 5), 0, 4);
            if (column == 1 && row >= 2) return GlassColors[2];
            return GlassColors[(column * 2 + row) % GlassColors.Length];
        }

        private static void Glass(Canvas c)
        {
            for (int y = 0; y < c.H; y++)
            for (int x = 0; x < c.W; x++)
            {
                float u = (x + .5f) / c.W, v = (y + .5f) / c.H;
                float edge = Mathf.Abs(u - .5f) * 2f;
                float arch = .99f - edge * edge * .26f;
                if (u < .04f || u > .96f || v < .025f || v > arch) continue;
                bool frame = u < .14f || u > .86f || v < .08f || v > arch - .08f;
                Color color = frame ? (x % 4 == 0 ? Gold : Hex(0x61545c)) : Pane(u, v);
                if (!frame)
                {
                    if (x % Mathf.Max(4, c.W / 3) < 2 || y % Mathf.Max(6, c.H / 5) < 2) color = Ink;
                    else if ((x + y * 2) % 17 < 2) color = Color.Lerp(color, Color.white, .35f);
                }
                c.Pixel(x, y, color);
            }
            c.Rect(1, 0, c.W - 2, 3, Gold);
        }

        private static void Projection(Canvas c, bool stained)
        {
            for (int y = 0; y < c.H; y++)
            for (int x = 0; x < c.W; x++)
            {
                float along = x / (float)(c.W - 1);
                float spread = .30f + .18f * along;
                float across = ((y / (float)c.H) - .5f) / spread * .5f + .5f;
                if (across <= 0 || across >= 1 || along < .025f || along > .975f) continue;
                float edge = Mathf.Min(across, 1f - across);
                float fade = Mathf.SmoothStep(0, 1, edge / .32f)
                    * Mathf.SmoothStep(0, 1, along / .14f) * Mathf.SmoothStep(0, 1, (1f - along) / .48f);
                // Broad, continuous penumbra. Blur the projected panes, not the window art itself.
                Color color = Hex(0xa8d9e0);
                if (stained)
                {
                    color = Color.clear;
                    for (int sy = -2; sy <= 2; sy++) for (int sx = -2; sx <= 2; sx++)
                        color += Pane(Mathf.Clamp01(across + sx * .055f), Mathf.Clamp01(1f - along + sy * .045f)) / 25f;
                    color = Color.Lerp(color, Hex(0xdacbab), .22f);
                }
                color.a = (stained ? .29f : .18f) * fade;
                c.Pixel(x, y, color);
            }
        }

        private static void Glow(Canvas c, bool shadow)
        {
            for (int y = 0; y < c.H; y++)
            for (int x = 0; x < c.W; x++)
            {
                float dx = (x + .5f) / c.W * 2 - 1, dy = (y + .5f) / c.H * 2 - 1;
                float strength = Mathf.Clamp01(1f - dx * dx - dy * dy);
                float alpha = strength * strength * strength * (shadow ? .34f : .24f);
                c.Pixel(x, y, shadow ? Hex(0x0b1020, alpha) : Hex(0xffce83, alpha));
            }
        }

        private static void Window(Canvas c)
        {
            c.Rect(0, 0, c.W, c.H, Ink);
            c.Rect(2, 2, c.W - 4, c.H - 4, Hex(0x466b85));
            for (int y = 4; y < c.H - 4; y++)
                c.Rect(4, y, c.W - 8, 1, Color.Lerp(Hex(0xb3dfe3), Hex(0x385379), y / (float)c.H));
            c.Rect(c.W / 2 - 1, 2, 2, c.H - 4, Hex(0xdec6a0));
            c.Rect(2, c.H / 2, c.W - 4, 2, Hex(0xdec6a0));
            c.Rect(0, 0, c.W, 3, Gold);
            for (int x = 6; x < c.W - 5; x++) if (x % 11 < 3) c.Rect(x, 4, 2, c.H / 3, Hex(0xd4efec, .65f));
        }

        private static void Books(Canvas c)
        {
            c.Rect(0, 0, c.W, c.H, Ink); c.Rect(2, 2, c.W - 4, c.H - 4, Hex(0x513824));
            Color[] colors = { Hex(0x94714d), Hex(0x577b80), Hex(0xb45151), Hex(0x698257), Hex(0xc3a069) };
            for (int y = 5; y < c.H - 7; y += 18)
            {
                for (int x = 5; x < c.W - 7; x += 6)
                {
                    int h = 9 + (x * 3 + y) % 5;
                    c.Rect(x, y, 4, h, colors[(x / 6 + y) % colors.Length]);
                    c.Rect(x, y + 3, 4, 1, Gold);
                }
                c.Rect(2, y - 2, c.W - 4, 2, Hex(0xa57a48));
            }
            c.Rect(0, c.H - 3, c.W, 3, Gold);
        }

        private static void Noticeboard(Canvas c)
        {
            c.Rect(0, 0, c.W, c.H, Hex(0x513523));
            c.Rect(3, 3, c.W - 6, c.H - 6, Hex(0x967b4d));
            for (int i = 0; i < 6; i++)
            {
                int x = 6 + (i % 3) * (c.W - 10) / 3, y = 6 + (i / 3) * (c.H - 9) / 2;
                int w = (c.W - 15) / 3, h = (c.H - 14) / 2;
                c.Rect(x + 1, y - 1, w, h, Hex(0x624a39));
                c.Rect(x, y, w, h, i % 2 == 0 ? Hex(0xe3d4a6) : Hex(0xa7c4bc));
                c.Rect(x + 3, y + h - 4, w - 6, 1, Hex(0x767362));
                c.Rect(x + 3, y + h - 7, w - 8, 1, Hex(0x767362));
                c.Pixel(x + w / 2, y + h - 2, Hex(0xb94c4c));
            }
        }

        private static void Plant(Canvas c)
        {
            int cx = c.W / 2;
            c.Rect(cx - c.W / 5, 2, c.W * 2 / 5, c.H / 3, Hex(0x8c543b));
            c.Rect(cx - c.W / 4, c.H / 3, c.W / 2, 3, Hex(0xc78c58));
            c.Rect(cx, c.H / 3, 2, c.H / 2, Hex(0x445c43));
            for (int i = 0; i < 9; i++)
            {
                int x = cx + (i % 2 == 0 ? -1 : 1) * (3 + i % 4 * 2), y = c.H / 3 + i * (c.H / 17);
                c.Ellipse(x, y, Mathf.Max(3, c.W / 5), Mathf.Max(2, c.H / 12), i % 3 == 0 ? Hex(0x809959) : Hex(0x3c7057));
            }
        }

        private static void Column(Canvas c)
        {
            c.Rect(2, 0, c.W - 4, c.H, Ink);
            c.Rect(c.W / 5, 3, c.W * 3 / 5, c.H - 6, Hex(0x655e59));
            c.Rect(c.W / 4, 5, Mathf.Max(2, c.W / 8), c.H - 10, Hex(0x938376));
            c.Rect(c.W * 2 / 3, 4, 3, c.H - 8, Hex(0x423e44));
            for (int y = 2; y < c.H; y += 17) c.Rect(c.W / 5, y, c.W * 3 / 5, 1, Hex(0x373640));
            c.Rect(0, 1, c.W, 4, Hex(0x8a7c6b));
            c.Rect(0, c.H - 5, c.W, 4, Hex(0xac9978));
        }

        private static void Rug(Canvas c)
        {
            c.Rect(2, 2, c.W - 4, c.H - 4, Hex(0x322638));
            c.Rect(4, 4, c.W - 8, c.H - 8, Gold);
            c.Rect(6, 6, c.W - 12, c.H - 12, Hex(0x742f42));
            for (int x = 9; x < c.W - 8; x += 12)
            { c.Rect(x, 8, 3, 2, Gold); c.Rect(x, c.H - 10, 3, 2, Gold); }
            for (int y = 10; y < c.H - 10; y += 12)
            for (int x = 15; x < c.W - 12; x += 24)
            { c.Rect(x, y, 1, 5, Hex(0xac695c)); c.Rect(x - 2, y + 2, 5, 1, Hex(0xac695c)); }
            for (int y = 3; y < c.H - 3; y += 3)
            { c.Rect(0, y, 2, 1, Gold); c.Rect(c.W - 2, y, 2, 1, Gold); }
        }

        private static void Puddle(Canvas c)
        {
            for (int y = 1; y < c.H - 1; y++)
            for (int x = 2; x < c.W - 2; x++)
            {
                float dx = (x - c.W * .5f) / (c.W * .46f), dy = (y - c.H * .5f) / (c.H * .43f);
                float d = dx * dx + dy * dy;
                if (d > .83f + .13f * Mathf.Sin(x * .45f)) continue;
                c.Pixel(x, y, d > .67f ? Hex(0x7395a3, .4f) : Hex(0x30495e, .72f));
                if (y % 7 == 2 && x % 21 < 13) c.Pixel(x, y, Hex(0xaed7de, .48f));
            }
        }

        private static void Leaves(Canvas c)
        {
            for (int i = 0; i < 30; i++)
            {
                int x = 3 + (i * 29) % (c.W - 6), y = 3 + (i * 13) % (c.H - 6);
                Color color = i % 3 == 0 ? Hex(0xb19451, .75f) : Hex(0x53694c, .85f);
                c.Rect(x, y, 3, 1, color); c.Rect(x + 1, y + 1, 3, 1, color);
            }
        }

        private static void Parking(Canvas c)
        {
            c.Rect(0, 0, c.W, c.H, Hex(0x28333d));
            for (int y = 1; y < c.H; y += 4)
            for (int x = 1; x < c.W; x += 7) c.Pixel(x + y % 3, y, Hex(0x36434b));
            c.Rect(3, 4, c.W - 6, 2, Gold); c.Rect(3, c.H - 6, c.W - 6, 2, Gold);
            c.Rect(3, 4, 2, c.H - 8, Gold); c.Rect(c.W - 5, 4, 2, c.H - 8, Gold);
            c.Rect(8, c.H / 4, 3, c.H / 2, Hex(0x64707a));
        }

        private static void Mosaic(Canvas c)
        {
            for (int y = 0; y < c.H; y++)
            for (int x = 0; x < c.W; x++)
            {
                float dx = (x + .5f) / c.W * 2 - 1, dy = (y + .5f) / c.H * 2 - 1;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                if (r > .98f) continue;
                Color color = r > .87f || (r > .67f && r < .73f) ? Gold : Hex(0x323d47);
                if (Mathf.Abs(dx) + Mathf.Abs(dy) < .56f) color = Hex(0x718880);
                if (x % 4 == 0 || y % 4 == 0) color = Color.Lerp(color, Ink, .45f);
                c.Pixel(x, y, color);
            }
        }

        private static void Sconce(Canvas c)
        {
            int cx = c.W / 2;
            c.Rect(cx - 3, 2, 6, c.H / 2, Hex(0x76604b));
            c.Rect(cx - 7, c.H / 3, 14, 3, Gold);
            c.Rect(cx - 2, c.H / 3, 4, c.H / 3, Hex(0xeee0bb));
            if (VarginhaReferenceSprites.Fire(0) == null)
            {
                c.Ellipse(cx, c.H * 3 / 4, 3, 5, Hex(0xe9a04e));
                c.Rect(cx, c.H * 3 / 4 - 2, 1, 4, Hex(0xfff0bf));
            }
        }

        private static void Pew(Canvas c)
        {
            c.Rect(3, 1, 5, 7, Ink); c.Rect(c.W - 8, 1, 5, 7, Ink);
            c.Rect(1, 7, c.W - 2, c.H - 9, Hex(0x332526));
            c.Rect(4, 10, c.W - 8, c.H - 14, Hex(0x785036));
            c.Rect(4, c.H - 6, c.W - 8, 2, Hex(0xa57a4b));
            c.Rect(6, c.H / 2, c.W - 12, 2, Hex(0x492f27));
            c.Rect(1, 5, c.W - 2, 5, Hex(0x9a6944));
            c.Rect(1, 9, c.W - 2, 1, Hex(0xbe8e59));
            for (int x = 5; x < c.W - 5; x += 14) c.Rect(x, c.H - 11, 9, 1, Hex(0x885e3c));
            c.Rect(0, 6, 3, c.H - 7, Hex(0x5d402f)); c.Rect(c.W - 3, 6, 3, c.H - 7, Hex(0x5d402f));
        }

        private static void Altar(Canvas c)
        {
            c.Rect(3, 1, c.W - 6, 5, Hex(0x4a3b36));
            c.Rect(6, 5, c.W - 12, c.H - 17, Hex(0x77624a));
            c.Rect(3, c.H - 17, c.W - 6, 10, Hex(0xeadfc1));
            c.Rect(5, 8, c.W - 10, c.H - 17, Hex(0xbfb49b));
            c.Rect(5, 9, c.W - 10, 2, Gold);
            for (int x = 9; x < c.W - 7; x += 8) c.Rect(x, 12, 2, c.H - 22, Hex(0xded3b5));
            int cx = c.W / 2;
            c.Rect(cx - 1, 15, 3, 9, Hex(0x9f7745)); c.Rect(cx - 4, 20, 9, 2, Hex(0x9f7745));
            c.Rect(cx - 6, c.H - 14, 13, 5, Hex(0x6b3d36));
            c.Rect(cx - 5, c.H - 12, 11, 3, Hex(0xffecc5)); c.Rect(cx, c.H - 12, 1, 3, Gold);
            for (int x = 8; x < c.W - 6; x += Mathf.Max(8, c.W - 18))
            {
                c.Rect(x, c.H - 13, 2, 8, Hex(0xffe3a2)); c.Pixel(x, c.H - 3, Hex(0xffd078));
            }
        }

        private static void Desk(Canvas c)
        {
            c.Rect(3, 0, 3, 5, Ink); c.Rect(c.W - 6, 0, 3, 5, Ink);
            c.Rect(0, 4, c.W, c.H - 6, Hex(0x593b2c));
            c.Rect(2, 7, c.W - 4, c.H - 10, Hex(0xae8755));
            c.Rect(2, c.H - 4, c.W - 4, 1, Hex(0xd3b47a));
            c.Rect(5, 8, c.W / 3, c.H / 2, Hex(0xe7dab5));
            c.Rect(6, 10, c.W / 3 - 3, 1, Hex(0x92988c));
            c.Rect(c.W - 8, 8, 2, c.H / 2, Hex(0x5f8c91));
        }

        private static void Blackboard(Canvas c)
        {
            c.Rect(0, 0, c.W, c.H, Hex(0x523c2b)); c.Rect(3, 3, c.W - 6, c.H - 6, Hex(0x243f3e));
            c.Rect(2, 1, c.W - 4, 2, Hex(0xab8451));
            for (int i = 0; i < 3; i++)
            {
                int y = c.H - 7 - i * 5;
                c.Rect(7, y, c.W / 2 - i * 4, 1, Hex(0xb9c3aa));
                c.Rect(7 + i * 5, y - 2, c.W / 3, 1, Hex(0x78968e));
            }
            c.Rect(c.W - 18, 6, 10, 9, Hex(0x547972)); c.Rect(c.W - 17, 7, 8, 7, Hex(0x243f3e));
            c.Rect(c.W - 14, 7, 1, 10, Hex(0xc1b885));
        }

        private static void StreetLamp(Canvas c)
        {
            int cx = c.W / 2;
            c.Rect(cx - 5, 1, 10, 3, Hex(0x536367));
            c.Rect(cx - 2, 4, 4, c.H - 19, Ink); c.Rect(cx, 5, 1, c.H - 21, Hex(0x7b8581));
            c.Rect(1, c.H - 19, c.W - 2, 14, Ink);
            c.Rect(4, c.H - 16, c.W - 8, 9, Hex(0xf2cf89));
            c.Rect(0, c.H - 5, c.W, 3, Hex(0x5f7278));
            c.Rect(cx - 1, c.H - 18, 2, 13, Hex(0x7d7664));
        }

        private sealed class Canvas
        {
            public readonly int W, H;
            public readonly Color32[] Pixels;
            public Canvas(int width, int height) { W = width; H = height; Pixels = new Color32[W * H]; }
            public void Pixel(int x, int y, Color color) { if (x >= 0 && y >= 0 && x < W && y < H) Pixels[y * W + x] = color; }
            public void Rect(int x, int y, int w, int h, Color color)
            { for (int iy = Mathf.Max(0, y); iy < Mathf.Min(H, y + h); iy++) for (int ix = Mathf.Max(0, x); ix < Mathf.Min(W, x + w); ix++) Pixel(ix, iy, color); }
            public void Ellipse(int cx, int cy, int rx, int ry, Color color)
            {
                for (int y = cy - ry; y <= cy + ry; y++) for (int x = cx - rx; x <= cx + rx; x++)
                    if ((x - cx) * (x - cx) / (float)(rx * rx) + (y - cy) * (y - cy) / (float)(ry * ry) <= 1f) Pixel(x, y, color);
            }
        }
    }
}
