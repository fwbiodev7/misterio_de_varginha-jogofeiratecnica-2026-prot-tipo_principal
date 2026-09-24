using UnityEngine;

namespace Game.Varginha
{
    /// <summary>A restrained ground contact, following visibility and motion without changing lights.</summary>
    [DisallowMultipleComponent]
    public sealed class VarginhaContactShadow : MonoBehaviour
    {
        private SpriteRenderer _source, _shadow;
        private bool _actor;

        public static void Ensure(SpriteRenderer source, bool actor = true)
        {
            if (source == null || source.GetComponent<VarginhaContactShadow>() != null) return;
            var contact = source.gameObject.AddComponent<VarginhaContactShadow>();
            contact._actor = actor;
            contact._source = source;
            contact.CreateShadow();
        }

        private void CreateShadow()
        {
            if (_shadow != null) return;
            var child = new GameObject("Sombra_Contato_Movel");
            child.transform.SetParent(transform, false);
            _shadow = child.AddComponent<SpriteRenderer>();
            _shadow.sprite = VarginhaSceneryArt.Create("Shadow", Vector2.one);
            _shadow.color = new Color(1f, 1f, 1f, .78f);
        }

        private void LateUpdate()
        {
            if (_source == null) _source = GetComponent<SpriteRenderer>();
            CreateShadow();
            _shadow.enabled = _source != null && _source.enabled && _source.sprite != null && _source.color.a > .05f;
            if (!_shadow.enabled) return;
            Bounds bounds = _source.sprite.bounds;
            // Character atlases have six transparent pixels below their feet.
            bool atlas = _actor && _source.sprite.rect.height == 64 && _source.sprite.pixelsPerUnit < 55;
            float feet = bounds.min.y + (atlas ? 6f / _source.sprite.pixelsPerUnit : bounds.size.y * .10f);
            float width = bounds.size.x * (_actor ? .46f : .78f);
            float height = _actor ? .22f : Mathf.Max(.2f, bounds.size.y * .18f);
            _shadow.transform.localPosition = new Vector3(.025f, feet, .01f);
            _shadow.transform.localRotation = Quaternion.Inverse(transform.rotation);
            _shadow.transform.localScale = new Vector3(width, height, 1);
            _shadow.sortingLayerID = _source.sortingLayerID;
            _shadow.sortingOrder = Mathf.Max(1, _source.sortingOrder - 1);
        }

        private void OnDisable() { if (_shadow != null) _shadow.enabled = false; }
    }
}
