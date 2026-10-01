using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Varginha
{
    /// <summary>Sort a whole prop or actor by its ground contact, including attached sprites.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(10000)]
    public sealed class VarginhaWorldDepth : MonoBehaviour
    {
        [SerializeField] private bool actor, behindActors;
        [SerializeField] private Transform support;
        [SerializeField] private Collider2D footprint;
        [SerializeField] private int relativeOrder;
        private SortingGroup _group;
        private SpriteRenderer _renderer;

        public static int OrderAt(float groundY) => Mathf.Clamp(12000 - Mathf.RoundToInt(groundY * 100), 1000, 22000);

        public static VarginhaWorldDepth Ensure(SpriteRenderer renderer, bool isActor = false,
            bool background = false, Collider2D ground = null, Transform supportingObject = null, int offset = 0)
        {
            if (renderer == null) return null;
            var depth = renderer.GetComponent<VarginhaWorldDepth>();
            if (depth == null) depth = renderer.gameObject.AddComponent<VarginhaWorldDepth>();
            depth._renderer = renderer;
            depth.actor = isActor; depth.behindActors = background;
            depth.footprint = ground; depth.support = supportingObject; depth.relativeOrder = offset;
            depth.Refresh();
            return depth;
        }

        private void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null) return;
            if (_group == null) _group = GetComponent<SortingGroup>();
            if (_group == null) _group = gameObject.AddComponent<SortingGroup>();
            float y;
            if (support != null)
            {
                var parentDepth = support.GetComponent<VarginhaWorldDepth>();
                if (parentDepth != null && parentDepth != this)
                {
                    parentDepth.Refresh();
                    _group.sortingLayerID = parentDepth._group.sortingLayerID;
                    _group.sortingOrder = parentDepth._group.sortingOrder + relativeOrder;
                    return;
                }
                y = support.position.y;
            }
            else y = actor ? transform.position.y : footprint != null && footprint.enabled
                ? footprint.transform.TransformPoint(footprint.offset).y : _renderer.bounds.min.y + _renderer.bounds.size.y * .15f;
            _group.sortingLayerID = _renderer.sortingLayerID;
            _group.sortingOrder = behindActors ? 3 : OrderAt(y) + relativeOrder;
        }
    }
}
