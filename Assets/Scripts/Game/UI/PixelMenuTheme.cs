using UnityEngine;

namespace Game.UI
{
    /// <summary>Radio panels share the pause screen's quiet metal and paper palette.</summary>
    public static class PixelMenuTheme
    {
        public static readonly Color Background = new(.042f, .056f, .068f, .97f);
        public static readonly Color Border = new(.28f, .40f, .44f, .98f);
        public static readonly Color Paper = new(.92f, .91f, .82f);
        public static readonly Color Muted = new(.64f, .73f, .75f);
        private static GUIStyle _text;

        public static void Panel(Rect rect) => PixelHUDFrame.Draw(rect, Texture2D.whiteTexture, Background, Border);

        public static void Label(Rect rect, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft)
        {
            if (_text == null)
            {
                _text = new GUIStyle(GUI.skin.label) { padding = new RectOffset(), margin = new RectOffset(),
                    wordWrap = false, clipping = TextClipping.Clip, fontStyle = FontStyle.Normal };
                PixelUIFont.Apply(_text);
            }
            _text.alignment = align;
            _text.normal.textColor = color;
            _text.fontSize = Mathf.Max(4, size);
            var content = new GUIContent(text);
            while (_text.fontSize > 4 && (_text.CalcSize(content).x > rect.width || _text.CalcSize(content).y > rect.height))
                _text.fontSize--;
            GUI.Label(rect, content, _text);
        }

        public static void Separator(Rect rect)
        {
            var color = GUI.color;
            GUI.color = Border;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = color;
        }

        public static bool Button(Rect rect, string text, string marker = ">", int size = 12)
        {
            bool hover = GUI.enabled && rect.Contains(Event.current.mousePosition);
            Color accent = new(.64f, .84f, .86f);
            PixelHUDFrame.Draw(rect, Texture2D.whiteTexture, hover ? new Color(.10f, .15f, .17f, .98f) : Background,
                hover ? accent : Border);
            Label(new Rect(rect.x + 14, rect.y, 26, rect.height), hover ? ">" : marker, 9, Muted);
            Label(new Rect(rect.x + 48, rect.y, rect.width - 60, rect.height), text, size, hover ? Color.white : Paper);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }
    }
}
