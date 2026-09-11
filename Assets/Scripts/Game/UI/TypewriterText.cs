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

        public void Set(string text)
        {
            FullText = text ?? string.Empty;
            _visibleCharacters = 0;
            _progress = 0f;
        }

        public void Tick(float charactersPerSecond)
        {
            if (IsComplete) return;
            _progress += Time.unscaledDeltaTime * charactersPerSecond;
            _visibleCharacters = Mathf.Min(FullText.Length, Mathf.FloorToInt(_progress));
        }

        public void RevealImmediately()
        {
            _visibleCharacters = FullText.Length;
        }
    }
}
