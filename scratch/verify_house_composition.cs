using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Varginha;

public static class VerifyHouseComposition
{
    public static object Main()
    {
        var player = Object.FindAnyObjectByType<EdelzioTopDownController>();
        var origin = (Vector2)player.transform.position;
        const float step = .25f;
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        var start = new Vector2Int(Mathf.RoundToInt(origin.x / step), Mathf.RoundToInt(origin.y / step));
        queue.Enqueue(start); visited.Add(start);
        var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            foreach (var direction in directions)
            {
                var next = cell + direction;
                var point = (Vector2)next * step;
                if (point.x < -8.4f || point.x > 10 || point.y < -6.4f || point.y > 6.4f || visited.Contains(next)) continue;
                bool blocked = Physics2D.OverlapCircleAll(point, .28f).Any(c => !c.isTrigger &&
                    c.GetComponentInParent<EdelzioTopDownController>() == null);
                if (blocked) continue;
                visited.Add(next); queue.Enqueue(next);
            }
        }
        var targets = new[] { "ToyBox_UnderBed", "Backpack_Prop", "Notebook_TI", "Coffee_Cup", "Doc_Historical", "FuseBox_Prop" };
        var results = new Dictionary<string, object>();
        foreach (var name in targets)
        {
            var target = GameObject.Find(name);
            float nearest = visited.Min(p => Vector2.Distance((Vector2)p * step, target.transform.position));
            results[name] = new { nearest, accessible = nearest <= 1.8f };
        }
        return new { scene = player.gameObject.scene.name, reachableCells = visited.Count, targets = results };
    }
}
