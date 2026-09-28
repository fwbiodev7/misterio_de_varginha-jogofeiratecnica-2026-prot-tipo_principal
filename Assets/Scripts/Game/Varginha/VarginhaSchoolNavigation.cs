using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Small static collision grid so rescued students use doors instead of crossing walls.</summary>
    public static class VarginhaSchoolNavigation
    {
        private const float Step = .5f;
        private const float Radius = .30f;
        private const int Width = 49, Height = 41;
        private static readonly Vector2 Origin = new(-12, -14);
        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        private static Transform _environment;
        private static bool[] _walkable;

        public static bool CanWalkSegment(Vector2 start, Vector2 end)
        {
            Vector2 delta = end - start;
            foreach (var hit in Physics2D.CircleCastAll(start, Radius, delta.normalized, delta.magnitude))
                if (IsWall(hit.collider)) return false;
            return true;
        }

        private static bool IsWall(Collider2D collider) => collider != null && !collider.isTrigger &&
            (collider.attachedRigidbody == null || collider.attachedRigidbody.bodyType == RigidbodyType2D.Static);

        public static void FindPath(Transform environment, Vector2 start, Vector2 destination, List<Vector2> path)
        {
            path.Clear();
            if (environment == null) return;
            if (_environment != environment || _walkable == null)
            {
                _environment = environment;
                _walkable = new bool[Width * Height];
                Physics2D.SyncTransforms();
                for (int i = 0; i < _walkable.Length; i++)
                {
                    _walkable[i] = true;
                    foreach (var collider in Physics2D.OverlapCircleAll(Point(i), Radius))
                        if (IsWall(collider)) { _walkable[i] = false; break; }
                }
            }
            if (CanWalkSegment(start, destination)) { path.Add(destination); return; }
            int first = Nearest(start, true);
            int last = Nearest(destination, false);
            if (first < 0 || last < 0) return;
            var previous = new int[_walkable.Length];
            System.Array.Fill(previous, -1);
            var queue = new Queue<int>();
            previous[first] = first;
            queue.Enqueue(first);
            while (queue.Count > 0 && previous[last] == -1)
            {
                int current = queue.Dequeue();
                int x = current % Width, y = current / Width;
                foreach (var direction in Directions)
                {
                    int nx = x + direction.x, ny = y + direction.y;
                    if (nx < 0 || nx >= Width || ny < 0 || ny >= Height) continue;
                    int next = ny * Width + nx;
                    if (!_walkable[next] || previous[next] != -1) continue;
                    previous[next] = current;
                    queue.Enqueue(next);
                }
            }
            if (previous[last] == -1) return;
            for (int i = last; i != first; i = previous[i]) path.Add(Point(i));
            path.Add(Point(first));
            path.Reverse();
            if (CanWalkSegment(Point(last), destination)) path.Add(destination);
        }

        private static int Nearest(Vector2 point, bool requireVisible)
        {
            int best = -1;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < _walkable.Length; i++)
            {
                if (!_walkable[i]) continue;
                float d = (Point(i) - point).sqrMagnitude;
                if (d >= distance || (requireVisible && !CanWalkSegment(point, Point(i)))) continue;
                distance = d;
                best = i;
            }
            return best;
        }

        private static Vector2 Point(int index) => Origin + new Vector2(index % Width, index / Width) * Step;
    }
}
