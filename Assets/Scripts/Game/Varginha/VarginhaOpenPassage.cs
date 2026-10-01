using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Open wall endings with matching jambs and a recessed threshold.</summary>
    public static class VarginhaOpenPassage
    {
        public static void Ensure(Transform parent, string name, Vector2 center, float span,
            float wallDepth, bool horizontal, Sprite jamb, Sprite threshold)
        {
            if (parent == null || jamb == null || threshold == null) return;
            string id = "Passagem_" + name;
            var previous = parent.Find(id);
            if (previous != null)
            {
                bool valid = true;
                foreach (var sr in previous.GetComponentsInChildren<SpriteRenderer>())
                    if (sr.sprite == null || sr.sprite.texture == null) { valid = false; break; }
                if (valid) return;
                Remove(parent, id);
            }
            var root = new GameObject(id).transform;
            root.SetParent(parent, false);
            Vector2 along = horizontal ? Vector2.right : Vector2.up;
            Vector2 capSize = horizontal ? new Vector2(.18f, wallDepth + .12f) : new Vector2(wallDepth + .12f, .18f);
            for (int side = -1; side <= 1; side += 2)
            {
                var position = center + along * (span * .5f * side);
                Piece(root, side < 0 ? "Acabamento_A" : "Acabamento_B", jamb, position, capSize, 4, Color.white);
                var inset = position + along * (-side * .035f);
                Piece(root, side < 0 ? "Rebaixo_A" : "Rebaixo_B", threshold, inset,
                    horizontal ? new Vector2(.035f, wallDepth) : new Vector2(wallDepth, .035f),
                    4, new Color(.58f, .51f, .42f));
            }
            Piece(root, "Soleira", threshold, center,
                horizontal ? new Vector2(span - .18f, .10f) : new Vector2(.10f, span - .18f),
                1, new Color(.70f, .62f, .52f));
        }

        private static void Piece(Transform parent, string name, Sprite sprite, Vector2 center, Vector2 size, int order, Color tint)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = size;
            sr.color = tint;
            sr.sortingOrder = order;
        }

        public static void Remove(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child == null) return;
            child.name += "_Replacing";
            child.gameObject.SetActive(false);
            if (Application.isPlaying) Object.Destroy(child.gameObject); else Object.DestroyImmediate(child.gameObject);
        }
    }
}
