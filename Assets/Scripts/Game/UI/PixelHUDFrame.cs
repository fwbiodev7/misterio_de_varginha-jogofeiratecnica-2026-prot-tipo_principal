using UnityEngine;

namespace Game.UI
{
    /// <summary>Moldura pixelada reutilizável para os painéis da interface.</summary>
    public static class PixelHUDFrame
    {
        public static void Draw(Rect rect, Texture2D pixel, Color background, Color border)
        {
            GUI.color = background;
            GUI.DrawTexture(rect, pixel);
            GUI.color = border;
            const float edge = 3f;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, edge), pixel);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - edge, rect.width, edge), pixel);
            GUI.DrawTexture(new Rect(rect.x, rect.y, edge, rect.height), pixel);
            GUI.DrawTexture(new Rect(rect.xMax - edge, rect.y, edge, rect.height), pixel);

            // Cantos e pequenos traços fazem o painel parecer um equipamento de investigação.
            GUI.DrawTexture(new Rect(rect.x + 7, rect.y + 7, 10, 3), pixel);
            GUI.DrawTexture(new Rect(rect.xMax - 17, rect.y + 7, 10, 3), pixel);
            GUI.DrawTexture(new Rect(rect.x + 7, rect.yMax - 10, 10, 3), pixel);
            GUI.DrawTexture(new Rect(rect.xMax - 17, rect.yMax - 10, 10, 3), pixel);
            GUI.color = Color.white;
        }

        public static void DrawHeart(Rect rect, Texture2D pixel, Color color)
        {
            float u = rect.width / 7f;
            GUI.color = color;
            DrawBlock(rect, pixel, 1, 0, 2, 1, u);
            DrawBlock(rect, pixel, 4, 0, 2, 1, u);
            DrawBlock(rect, pixel, 0, 1, 7, 2, u);
            DrawBlock(rect, pixel, 1, 3, 5, 1, u);
            DrawBlock(rect, pixel, 2, 4, 3, 1, u);
            DrawBlock(rect, pixel, 3, 5, 1, 1, u);
            GUI.color = Color.white;
        }

        private static void DrawBlock(Rect rect, Texture2D pixel, int x, int y, int width, int height, float unit)
        {
            GUI.DrawTexture(new Rect(rect.x + x * unit, rect.y + y * unit, width * unit, height * unit), pixel);
        }
    }
}
