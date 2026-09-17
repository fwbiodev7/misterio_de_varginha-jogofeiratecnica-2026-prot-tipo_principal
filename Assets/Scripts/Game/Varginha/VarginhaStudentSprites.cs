using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>One canonical atlas per student, shared by gameplay, portraits and special attacks.</summary>
    public static class VarginhaStudentSprites
    {
        public const int CellSize = 64;
        public const float PixelsPerUnit = 64f / 1.35f;
        private sealed class Atlas
        {
            public Texture2D Texture;
            public readonly Sprite[] Frames = new Sprite[16];
            public Sprite Portrait;
        }
        private static readonly Dictionary<string, Atlas> Atlases = new();

        public static string AssetName(string student)
        {
            string name = (student ?? string.Empty).Trim().Replace(" ", "");
            // The direction sheet uses these display-name variants for the same two students.
            return name == "AnnaTavares" ? "AnaTavares" : name == "LuisMessias" ? "LuisMiguelMessias" : name;
        }

        private static Atlas Load(string student)
        {
            string assetName = AssetName(student);
            if (string.IsNullOrEmpty(assetName)) return null;
            if (Atlases.TryGetValue(assetName, out var existing) && existing.Texture != null
                && existing.Frames[0] != null) return existing;
            var texture = Resources.Load<Texture2D>("Varginha/Allies/" + assetName);
            if (texture == null) return null;
            if (texture.width != CellSize * 4 || texture.height != CellSize * 4)
            {
                Debug.LogError("Atlas do aluno precisa de 4 × 4 células de 64 pixels: " + assetName);
                return null;
            }
            var atlas = new Atlas { Texture = texture };
            texture.filterMode = FilterMode.Point;
            for (int direction = 0; direction < 4; direction++)
            for (int frame = 0; frame < 4; frame++)
            {
                var rect = new Rect(frame * CellSize, (3 - direction) * CellSize, CellSize, CellSize);
                var sprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
                sprite.name = "Student_" + student + "_" + direction + "_" + frame;
                atlas.Frames[direction * 4 + frame] = sprite;
            }
            // A tight head-and-shoulders crop of the actual character, never a second design.
            atlas.Portrait = Sprite.Create(texture, PortraitRect(texture),
                new Vector2(.5f, .5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
            atlas.Portrait.name = "StudentHead_" + student;
            Atlases[assetName] = atlas;
            return atlas;
        }

        private static Rect PortraitRect(Texture2D texture)
        {
            // Follow the actual front-frame head so short Anna and tall Ana both fit the portrait.
            if (!texture.isReadable) return new Rect(10, 3 * CellSize + 25, 44, 36);
            Color32[] pixels = texture.GetPixels32();
            int bottom = CellSize, top = 0;
            for (int y = 0; y < CellSize; y++)
            for (int x = 0; x < CellSize; x++)
            {
                if (pixels[(3 * CellSize + y) * texture.width + x].a < 128) continue;
                bottom = Mathf.Min(bottom, y);
                top = Mathf.Max(top, y + 1);
            }
            if (top <= bottom) return new Rect(10, 3 * CellSize + 25, 44, 36);
            int height = Mathf.Clamp(Mathf.RoundToInt((top - bottom) * .65f) + 2, 26, 38);
            int upper = Mathf.Min(CellSize, top + 2);
            return new Rect(10, 3 * CellSize + Mathf.Max(0, upper - height), 44, height);
        }

        public static Sprite Frame(string student, int direction = 0, int frame = 0)
            => Load(student)?.Frames[Mathf.Clamp(direction, 0, 3) * 4 + (frame & 3)];
        public static Sprite Portrait(string student) => Load(student)?.Portrait;
        public static int Direction(Vector2 movement)
            => Mathf.Abs(movement.x) > Mathf.Abs(movement.y)
                ? movement.x < 0 ? 1 : 2 : movement.y > 0 ? 3 : 0;
    }
}
