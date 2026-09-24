using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Three cached, point-sampled frames, paused with the game.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VarginhaFlameAnimation : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private float _phase;
        private int _frame = -1;

        private void OnEnable()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _phase = Mathf.Abs(transform.position.x * .37f + transform.position.y * .61f);
            RefreshFrame();
        }

        private void Update() => RefreshFrame();

        private void RefreshFrame()
        {
            int frame = Mathf.FloorToInt((Time.time + _phase) * 8f) % 3;
            if (frame == _frame && _renderer.sprite != null) return;
            _frame = frame;
            _renderer.sprite = VarginhaReferenceSprites.Fire(frame);
        }

        public static void Ensure(Transform holder, Vector2 localPosition, float height)
        {
            if (holder == null || VarginhaReferenceSprites.Fire(0) == null) return;
            var flame = holder.Find("Chama_Referencia");
            if (flame == null)
            {
                flame = new GameObject("Chama_Referencia", typeof(SpriteRenderer)).transform;
                flame.SetParent(holder, false);
            }
            flame.localPosition = localPosition;
            flame.localScale = Vector3.one * height;
            var renderer = flame.GetComponent<SpriteRenderer>();
            var parent = holder.GetComponent<SpriteRenderer>();
            renderer.sprite = VarginhaReferenceSprites.Fire(0);
            renderer.color = Color.white;
            if (parent != null)
            {
                renderer.sortingLayerID = parent.sortingLayerID;
                renderer.sortingOrder = parent.sortingOrder + 1;
            }
            if (flame.GetComponent<VarginhaFlameAnimation>() == null)
                flame.gameObject.AddComponent<VarginhaFlameAnimation>();
        }
    }
}
