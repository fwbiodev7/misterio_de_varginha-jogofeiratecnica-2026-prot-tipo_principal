using System.Collections;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Anima a tampa do baú em três quadros pixelados ao coletar as memórias.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ToyBoxOpenAnimation : MonoBehaviour
    {
        [SerializeField] private Color boxColor = new Color(.95f, .65f, .15f);

        private SpriteRenderer _renderer;
        private Sprite _closed;
        private Sprite _opening;
        private Sprite _open;
        private bool _opened;

        public void Configure(Color color)
        {
            boxColor = color;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _closed = VarginhaPixelArtSprites.Create("ToyBox_UnderBed", boxColor);
            _opening = VarginhaPixelArtSprites.Create("ToyBox_Opening", boxColor);
            _open = VarginhaPixelArtSprites.Create("ToyBox_Open", boxColor);
        }

        public void PlayOpen(EdelzioTopDownController player = null)
        {
            if (_opened) return;
            _opened = true;
            StartCoroutine(OpenRoutine(player));
        }

        private IEnumerator OpenRoutine(EdelzioTopDownController player)
        {
            var action = player != null ? player.GetComponent<VarginhaPlayerActionAnimation>() : null;
            if (action != null) yield return action.CrouchRoutine(.22f);
            _renderer.sprite = _closed;
            yield return new WaitForSeconds(.10f);
            _renderer.sprite = _opening;
            yield return new WaitForSeconds(.14f);
            _renderer.sprite = _open;
        }
    }
}
