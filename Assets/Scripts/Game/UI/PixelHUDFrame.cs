using UnityEngine;

namespace Game.UI
{
    /// <summary>Moldura pixelada reutilizável para os painéis da interface.</summary>
    public static class PixelHUDFrame
    {
        private static Texture2D _grain;

        public static void Draw(Rect rect, Texture2D pixel, Color background, Color border)
        {
            rect = new Rect(Mathf.Round(rect.x), Mathf.Round(rect.y), Mathf.Round(rect.width), Mathf.Round(rect.height));
            Fill(new Rect(rect.x + 3, rect.y + 4, rect.width, rect.height), pixel, new Color(.008f, .012f, .018f, .65f));
            Fill(rect, pixel, new Color(.025f, .035f, .045f, .98f));
            Fill(Inset(rect, 1), pixel, Color.Lerp(border, background, .24f));
            Rect interior = Inset(rect, 3);
            Fill(interior, pixel, background);

            // A repeating point-filtered grain gives the panels a quiet metal texture.
            // It is built once, never per OnGUI event or per panel.
            GUI.color = new Color(1f, 1f, 1f, background.a);
            GUI.DrawTextureWithTexCoords(interior, Grain(), new Rect(0, 0, interior.width / 16f, interior.height / 16f));
            Fill(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, 1), pixel, Color.Lerp(background, border, .38f));
            Fill(new Rect(rect.x + 3, rect.y + 4, 1, rect.height - 8), pixel, Color.Lerp(background, border, .22f));
            Fill(new Rect(rect.x + 3, rect.yMax - 4, rect.width - 6, 1), pixel, new Color(.018f, .025f, .032f, .9f));
            Fill(new Rect(rect.xMax - 4, rect.y + 4, 1, rect.height - 8), pixel, new Color(.018f, .025f, .032f, .9f));

            if (rect.width > 130f && rect.height > 55f)
            {
                Color fastener = Color.Lerp(background, border, .5f);
                Fill(new Rect(rect.x + 6, rect.y + 6, 2, 2), pixel, fastener);
                Fill(new Rect(rect.xMax - 8, rect.y + 6, 2, 2), pixel, fastener);
                Fill(new Rect(rect.x + 6, rect.yMax - 8, 2, 2), pixel, fastener);
                Fill(new Rect(rect.xMax - 8, rect.yMax - 8, 2, 2), pixel, fastener);
            }
            GUI.color = Color.white;
        }

        public static void DrawHeart(Rect rect, Texture2D pixel, Color color)
        {
            if (Game.Varginha.VarginhaReferenceSprites.Heart(true) == null)
                DrawLegacyHeart(rect, pixel, color);
            else DrawHealthHeart(rect, color.maxColorComponent > .5f ? 1f : 0f);
        }

        public static void DrawHealthHeart(Rect rect, float fill)
        {
            var full = Game.Varginha.VarginhaReferenceSprites.Heart(true);
            var empty = Game.Varginha.VarginhaReferenceSprites.Heart(false);
            var previousColor = GUI.color;
            fill = Mathf.Clamp01(fill);
            if (full == null || empty == null)
            {
                DrawLegacyHeart(rect, Texture2D.whiteTexture, Color.Lerp(new Color(.27f, .20f, .24f), new Color(.87f, .24f, .29f), fill));
                GUI.color = previousColor;
                return;
            }
            GUI.color = Color.white;
            GUI.DrawTexture(rect, empty.texture, ScaleMode.StretchToFill, true);
            if (fill > 0f)
            {
                GUI.BeginGroup(new Rect(rect.x, rect.yMax - rect.height * fill, rect.width, rect.height * fill));
                GUI.DrawTexture(new Rect(0, -rect.height * (1 - fill), rect.width, rect.height), full.texture);
                GUI.EndGroup();
            }
            GUI.color = previousColor;
        }

        private static void DrawLegacyHeart(Rect rect, Texture2D pixel, Color color)
        {
            float u = Mathf.Max(1f, Mathf.Floor(rect.width / 9f));
            rect.x = Mathf.Round(rect.x + (rect.width - u * 9f) * .5f);
            rect.y = Mathf.Round(rect.y);
            GUI.color = new Color(.025f, .018f, .035f);
            DrawBlock(rect, pixel, 1, 0, 3, 1, u);
            DrawBlock(rect, pixel, 5, 0, 3, 1, u);
            DrawBlock(rect, pixel, 0, 1, 9, 3, u);
            DrawBlock(rect, pixel, 1, 4, 7, 1, u);
            DrawBlock(rect, pixel, 2, 5, 5, 1, u);
            DrawBlock(rect, pixel, 3, 6, 3, 1, u);
            DrawBlock(rect, pixel, 4, 7, 1, 1, u);
            GUI.color = color;
            DrawBlock(rect, pixel, 1, 1, 3, 2, u);
            DrawBlock(rect, pixel, 5, 1, 3, 2, u);
            DrawBlock(rect, pixel, 2, 2, 5, 3, u);
            DrawBlock(rect, pixel, 3, 5, 3, 1, u);
            DrawBlock(rect, pixel, 4, 6, 1, 1, u);
            GUI.color = Color.Lerp(color, new Color(.10f, .025f, .06f), .38f);
            DrawBlock(rect, pixel, 6, 2, 2, 1, u);
            DrawBlock(rect, pixel, 6, 3, 1, 1, u);
            DrawBlock(rect, pixel, 5, 4, 1, 1, u);
            DrawBlock(rect, pixel, 4, 5, 1, 2, u);
            GUI.color = Color.Lerp(color, Color.white, color.maxColorComponent > .5f ? .4f : .1f);
            DrawBlock(rect, pixel, 2, 1, 1, 1, u);
            DrawBlock(rect, pixel, 1, 2, 1, 1, u);
            DrawBlock(rect, pixel, 6, 1, 1, 1, u);
            GUI.color = Color.white;
        }

        private static Rect Inset(Rect rect, float amount) => new Rect(rect.x + amount, rect.y + amount,
            Mathf.Max(0, rect.width - amount * 2), Mathf.Max(0, rect.height - amount * 2));

        private static void Fill(Rect rect, Texture2D pixel, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, pixel);
        }

        private static Texture2D Grain()
        {
            if (_grain != null) return _grain;
            _grain = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                name = "HUD_PixelGrain",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[256];
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                int value = (x * 17 + y * 29 + x * y * 3) % 23;
                pixels[y * 16 + x] = value < 3 ? new Color32(214, 226, 238, 8)
                    : value > 18 ? new Color32(0, 0, 0, 20) : new Color32(0, 0, 0, 0);
            }
            _grain.SetPixels32(pixels);
            _grain.Apply(false, true);
            return _grain;
        }

        private static void DrawBlock(Rect rect, Texture2D pixel, int x, int y, int width, int height, float unit)
        {
            GUI.DrawTexture(new Rect(rect.x + x * unit, rect.y + y * unit, width * unit, height * unit), pixel);
        }
    }
}
