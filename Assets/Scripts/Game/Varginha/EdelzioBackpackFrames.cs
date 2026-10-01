using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Authored equipment animations: one complete sprite per body pose.</summary>
    public static class EdelzioBackpackFrames
    {
        public const string ResourcePath = "Varginha/Equipment/EdelzioEquippedV1";
        [Serializable] public sealed class Catalog { public Entry[] frames; }
        [Serializable] public sealed class Entry
        {
            public string source, sheet;
            public int direction, x, y, width, height;
            public float pivotX, pivotY, pixelsPerUnit;
        }
        private static Dictionary<(string, int), Entry> _entries;
        private static readonly Dictionary<(string, int), Sprite> Frames = new();
        public static void ClearCache() { _entries = null; Frames.Clear(); }

        public static Sprite Frame(Sprite body, int direction)
        {
            if (body == null) return null;
            if (_entries == null)
            {
                var json = Resources.Load<TextAsset>(ResourcePath);
                if (json == null) return null;
                var catalog = JsonUtility.FromJson<Catalog>(json.text);
                if (catalog?.frames == null) return null;
                _entries = new Dictionary<(string, int), Entry>();
                foreach (var entry in catalog.frames)
                    _entries[(entry.source, entry.direction)] = entry;
            }
            var key = (body.name, Mathf.Clamp(direction, 0, 7));
            // Native sprites can be destroyed when entering Play with domain reload disabled.
            // A managed reference survives: Unity's overloaded null check must validate it.
            if (Frames.TryGetValue(key, out var cached) && cached != null && cached.texture != null) return cached;
            if (!_entries.TryGetValue(key, out var pose)) return null;
            var texture = Resources.Load<Texture2D>("Varginha/Equipment/" + pose.sheet);
            if (texture == null || pose.width <= 0 || pose.height <= 0 || pose.pixelsPerUnit <= 0
                || pose.x < 0 || pose.y < 0 || pose.x + pose.width > texture.width || pose.y + pose.height > texture.height) return null;
            var result = Sprite.Create(texture, new Rect(pose.x, pose.y, pose.width, pose.height),
                new Vector2(pose.pivotX, pose.pivotY), pose.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            result.name = pose.source + "_ComMochila_" + pose.direction;
            Frames[key] = result;
            return result;
        }
    }
}
