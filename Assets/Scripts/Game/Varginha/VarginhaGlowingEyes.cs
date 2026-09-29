using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Only the red eye pixels remain faintly emissive in the church.</summary>
    public sealed class VarginhaGlowingEyes : MonoBehaviour
    {
        private SpriteRenderer _body, _eyes;
        private Sprite _source, _sprite;
        private Texture2D _texture;
        private void LateUpdate()
        {
            if (_body == null) _body = GetComponent<SpriteRenderer>();
            if (_body == null || _body.sprite == null) return;
            if (_source != _body.sprite) Rebuild();
            if (_eyes == null) return;
            _eyes.enabled = _body.enabled && GetComponent<VarginhaCombatTarget>()?.IsDead != true;
            _eyes.flipX = _body.flipX; _eyes.flipY = _body.flipY;
            _eyes.color = new Color(.6f, .13f, .1f, .72f);
        }
        private void Rebuild()
        {
            _source = _body.sprite;
            if (!_source.texture.isReadable) return;
            if (_sprite != null) Destroy(_sprite);
            if (_texture != null) Destroy(_texture);
            var rect = _source.rect;
            int width = (int)rect.width, height = (int)rect.height;
            var pixels = _source.texture.GetPixels((int)rect.x, (int)rect.y, width, height);
            for (int i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                pixels[i] = i / width > height * .48f && c.r > .48f && c.r > c.g * 2.2f && c.r > c.b * 2.2f
                    ? new Color(1, 1, 1, c.a) : Color.clear;
            }
            _texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            _texture.SetPixels(pixels); _texture.Apply();
            _sprite = Sprite.Create(_texture, new Rect(0, 0, width, height), _source.pivot / rect.size, _source.pixelsPerUnit);
            if (_eyes == null)
            {
                var child = new GameObject("Olhos_Vermelhos_Brilho"); child.transform.SetParent(transform, false);
                _eyes = child.AddComponent<SpriteRenderer>(); _eyes.sortingOrder = 30001;
            }
            _eyes.sprite = _sprite;
        }
        private void OnDestroy()
        {
            if (_eyes != null) Destroy(_eyes.gameObject);
            if (_sprite != null) Destroy(_sprite);
            if (_texture != null) Destroy(_texture);
        }
    }
}
