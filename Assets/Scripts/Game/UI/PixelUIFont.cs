using UnityEngine;

namespace Game.UI
{
    /// <summary>Fonte única para todos os textos das interfaces IMGUI do protótipo.</summary>
    public static class PixelUIFont
    {
        private static Font _font;

        public static void Apply(GUIStyle style)
        {
            if (_font == null)
                _font = Resources.Load<Font>("Fonts/PressStart2P-Regular");

            if (_font != null)
                style.font = _font;
        }
    }
}
