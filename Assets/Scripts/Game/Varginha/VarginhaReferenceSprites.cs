using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Shared reference art for gameplay, summons and ally portraits.</summary>
    public static class VarginhaReferenceSprites
    {
        private static readonly Dictionary<string, Sprite> Cache = new();

        public static Sprite Prop(string id)
        {
            int row = 0, column;
            if (id == "StudentAttack_Art") column = 0;
            else if (id == "StudentAttack_Microphone") column = 1;
            else if (id == "StudentAttack_Katana") column = 2;
            else if (id == "StudentAttack_FallingPiano") { row = 1; column = 0; }
            else if (id.StartsWith("HostageCage_")) { row = 1; column = 1; }
            else return null;
            return Cell("AllyReferencePropsV1", row, column, row == 1 && column == 1 ? 1.25f : 1f);
        }

        public static Sprite Character(bool fabio, int direction, bool walking = false)
        {
            int row = (fabio ? 2 : 0) + (walking ? 1 : 0);
            return Cell("CharacterReferencesV1", row, Mathf.Clamp(direction, 0, 3), fabio ? 1f : 1.25f);
        }

        public static Sprite Action(int column) => Cell("EdelzioReferenceActionsV1", 3, column, 1.25f);
        public static Sprite Attack(int direction, int frame) => direction == 3
            ? Character(false, 3, (frame & 1) != 0)
            : Cell("EdelzioReferenceActionsV1", direction, frame, 1.25f);

        public static Sprite FabioPortrait()
        {
            const string key = "FabioPortrait";
            if (Cache.TryGetValue(key, out var existing) && existing != null) return existing;
            var body = Character(true, 0);
            if (body == null) return null;
            var rect = body.rect;
            float height = rect.height * .40f;
            rect.y += rect.height - height;
            rect.height = height;
            var portrait = Sprite.Create(body.texture, rect, new Vector2(.5f, .5f), height / .65f);
            portrait.name = key;
            Cache[key] = portrait;
            return portrait;
        }

        private static Sprite Cell(string atlas, int row, int column, float height)
        {
            string key = atlas + "_" + row + "_" + column;
            if (Cache.TryGetValue(key, out var existing) && existing != null) return existing;
            var texture = Resources.Load<Texture2D>("Varginha/" + atlas);
            if (texture == null) return null;
            bool prop = atlas == "AllyReferencePropsV1";
            int columns = prop ? 3 : 4;
            int x0 = Mathf.RoundToInt(column * texture.width / (float)columns);
            int x1 = Mathf.RoundToInt((column + 1) * texture.width / (float)columns);
            // Authored row boundaries include the small margins around the feet.
            float[] edges = atlas == "CharacterReferencesV1"
                ? new[] { 0f, 337f / 1254f, 640f / 1254f, 960f / 1254f, 1f }
                : prop ? new[] { 0f, .5f, 1f } : new[] { 0f, .25f, .5f, 930f / 1254f, 1f };
            int y0 = Mathf.RoundToInt((1f - edges[row + 1]) * texture.height);
            int y1 = Mathf.RoundToInt((1f - edges[row]) * texture.height);
            int left = x1, right = x0, bottom = y1, top = y0;
            // Trim transparent margins when slicing, never rescale the artwork.
            var pixels = texture.GetPixels32();
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                    if (pixels[y * texture.width + x].a > 64)
                    {
                        left = Mathf.Min(left, x); right = Mathf.Max(right, x);
                        bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y);
                    }
            if (right < left || top < bottom) return null;
            var rect = new Rect(left, bottom, right - left + 1, top - bottom + 1);
            // Keep scale stable across walking and seated/crouched poses.
            float ppu = prop ? Mathf.Max(rect.width, rect.height) / height : texture.height * .22f / height;
            // Rotate the upright katana around its handle instead of its blade center.
            Vector2 pivot = prop && column == 2 ? new Vector2(.5f, .18f) : new Vector2(.5f, .5f);
            if (!prop)
            {
                pivot.y = texture.height * .11f / rect.height;
                if (atlas == "EdelzioReferenceActionsV1" && row < 3 && column > 0)
                {
                    var neutral = Cell(atlas, row, 0, height);
                    if (neutral != null)
                        pivot.x = (neutral.rect.center.x + column * texture.width / 4f - rect.x) / rect.width;
                }
            }
            var sprite = Sprite.Create(texture, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }
    }
}
