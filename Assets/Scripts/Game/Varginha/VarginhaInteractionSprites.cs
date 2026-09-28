using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Quadros autorados do Edelzio atual: café, cadeira e câmera do notebook.</summary>
    public static class VarginhaInteractionSprites
    {
        public const string ResourcePath = "Varginha/EdelzioInteractionsV1";
        private static Texture2D _texture;
        private static Sprite[] _frames;

        public static Sprite Frame(int row, int frame)
        {
            if (_texture == null || _frames == null || _frames[0] == null)
            {
                _texture = Resources.Load<Texture2D>(ResourcePath);
                if (_texture == null || !_texture.isReadable) return null;
                _texture.filterMode = FilterMode.Point;
                _frames = new Sprite[12];
                int width = _texture.width / 4, height = _texture.height / 3;
                var pixels = _texture.GetPixels32();
                Rect first = Trim(pixels, new RectInt(0, height * 2, width, height));
                // Align feet and body height with the walking atlas, without touching physics.
                var walk = VarginhaReferenceSprites.EdelzioWalkFrames()?[0][0];
                float worldHeight = 1.15f, footY = -.6f;
                if (walk != null && walk.texture.isReadable)
                {
                    Rect visible = Trim(walk.texture.GetPixels32(), new RectInt((int)walk.rect.x,
                        (int)walk.rect.y, (int)walk.rect.width, (int)walk.rect.height), walk.texture.width);
                    worldHeight = visible.height / walk.pixelsPerUnit;
                    footY = (visible.yMin - walk.rect.yMin - walk.pivot.y) / walk.pixelsPerUnit;
                }
                float ppu = first.height / Mathf.Max(.1f, worldHeight);
                for (int r = 0; r < 3; r++)
                for (int f = 0; f < 4; f++)
                {
                    Rect bounds = Trim(pixels, new RectInt(f * width, (2 - r) * height, width, height));
                    Vector2 pivot = r == 2 ? new Vector2(.5f, .5f)
                        : new Vector2((f * width + width * .5f - bounds.xMin) / bounds.width, -footY * ppu / bounds.height);
                    var sprite = Sprite.Create(_texture, bounds, pivot, ppu, 0, SpriteMeshType.FullRect);
                    sprite.name = $"Edelzio_Interaction_{r}_{f}";
                    _frames[r * 4 + f] = sprite;
                }
            }
            return _frames[Mathf.Clamp(row, 0, 2) * 4 + (frame & 3)];
        }

        private static Rect Trim(Color32[] pixels, RectInt cell, int textureWidth = 0)
        {
            if (textureWidth == 0) textureWidth = _texture.width;
            int left = cell.xMax, right = cell.xMin, bottom = cell.yMax, top = cell.yMin;
            for (int y = cell.yMin; y < cell.yMax; y++)
            for (int x = cell.xMin; x < cell.xMax; x++)
            {
                if (pixels[y * textureWidth + x].a < 64) continue;
                left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1);
                bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1);
            }
            return right > left && top > bottom ? Rect.MinMaxRect(left, bottom, right, top)
                : new Rect(cell.x, cell.y, cell.width, cell.height);
        }

        public static Sprite Webcam(float time, bool reacting)
        {
            int frame = reacting ? 3 : time % 4.5f > 4.32f ? 1 : (Mathf.FloorToInt(time * 2f) & 1) == 0 ? 0 : 2;
            return Frame(2, frame);
        }
    }
}
