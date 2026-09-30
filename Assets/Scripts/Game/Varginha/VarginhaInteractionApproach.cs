using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Short collision-aware routes around tables, used only when starting a seated action.</summary>
    public static class VarginhaInteractionApproach
    {
        private const float Step = .25f;
        private const int Width = 37, Half = Width / 2;
        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        public static bool FindPath(Vector2 start, Vector2 destination, Collider2D player, Collider2D seat, List<Vector2> path)
        {
            path.Clear();
            // The current protagonist is scaled up: plan with his actual collision footprint.
            float radius = player != null ? Mathf.Max(.24f, player.bounds.extents.x + .03f,
                player.bounds.extents.y + .03f) : .28f;
            if (Vector2.Distance(start, destination) > 4.4f) return false;
            if (ClearSegment(start, destination, radius, player, seat)) { path.Add(destination); return true; }
            Vector2 origin = start - Vector2.one * Half * Step;
            Vector2Int end = new(Mathf.RoundToInt((destination.x - origin.x) / Step), Mathf.RoundToInt((destination.y - origin.y) / Step));
            if (end.x < 0 || end.y < 0 || end.x >= Width || end.y >= Width) return false;
            int first = Half * Width + Half, last = end.y * Width + end.x;
            var previous = new int[Width * Width];
            System.Array.Fill(previous, -1);
            var queue = new Queue<int>();
            previous[first] = first;
            queue.Enqueue(first);
            Vector2 Point(int index) => origin + new Vector2(index % Width, index / Width) * Step;
            while (queue.Count > 0 && previous[last] == -1)
            {
                int current = queue.Dequeue();
                foreach (var direction in Directions)
                {
                    int x = current % Width + direction.x, y = current / Width + direction.y;
                    if (x < 0 || y < 0 || x >= Width || y >= Width) continue;
                    int next = y * Width + x;
                    if (previous[next] != -1 || !ClearSegment(Point(current), Point(next), radius, player, seat)) continue;
                    previous[next] = current;
                    queue.Enqueue(next);
                }
            }
            if (previous[last] == -1 || !ClearSegment(Point(last), destination, radius, player, seat)) return false;
            var raw = new List<Vector2> { destination };
            for (int i = last; i != first; i = previous[i]) raw.Add(Point(i));
            raw.Add(start); raw.Reverse();
            int at = 0;
            while (at < raw.Count - 1)
            {
                int next = raw.Count - 1;
                while (next > at + 1 && !ClearSegment(raw[at], raw[next], radius, player, seat)) next--;
                path.Add(raw[next]);
                at = next;
            }
            return true;
        }

        private static bool ClearSegment(Vector2 start, Vector2 end, float radius, Collider2D player, Collider2D seat)
        {
            Vector2 delta = end - start;
            foreach (var hit in Physics2D.CircleCastAll(start, radius, delta.normalized, delta.magnitude))
            {
                var collider = hit.collider;
                if (collider == null || collider.isTrigger || collider == player || collider == seat) continue;
                if (collider.GetComponentInParent<EdelzioTopDownController>() != null) continue;
                if (collider.attachedRigidbody != null && collider.attachedRigidbody.bodyType != RigidbodyType2D.Static) continue;
                return false;
            }
            return true;
        }
    }
}
