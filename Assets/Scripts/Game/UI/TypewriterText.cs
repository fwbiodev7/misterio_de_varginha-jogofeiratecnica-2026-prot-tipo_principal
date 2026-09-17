using UnityEngine;

namespace Game.UI
{
    /// <summary>Revela texto gradualmente para diálogos e instruções, independente do Time.timeScale.</summary>
    public sealed class TypewriterText
    {
        public string FullText { get; private set; } = string.Empty;
        public string VisibleText => FullText.Substring(0, Mathf.Min(_visibleCharacters, FullText.Length));
        public bool IsComplete => _visibleCharacters >= FullText.Length;

        private int _visibleCharacters;
        private float _progress;
        public bool HasAdvanced { get; private set; }

        public void Set(string text)
        {
            FullText = text ?? string.Empty;
            _visibleCharacters = 0;
            _progress = 0f;
            HasAdvanced = false;
        }

        public void Tick(float charactersPerSecond)
        {
            HasAdvanced = false;
            if (IsComplete) return;
            int prev = _visibleCharacters;
            _progress += Time.unscaledDeltaTime * charactersPerSecond;
            _visibleCharacters = Mathf.Min(FullText.Length, Mathf.FloorToInt(_progress));
            HasAdvanced = _visibleCharacters > prev;
        }

        public void RevealImmediately()
        {
            _visibleCharacters = FullText.Length;
        }
    }
}
