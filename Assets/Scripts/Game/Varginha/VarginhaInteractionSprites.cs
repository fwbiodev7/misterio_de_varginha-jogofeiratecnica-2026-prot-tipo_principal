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
                int width = _texture.width / 4;
                var pixels = _texture.GetPixels32();
                int[] rows = RowBoundaries(pixels);
                Rect first = Trim(pixels, new RectInt(0, rows[2], width, rows[3] - rows[2]));
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
                    Rect bounds = Trim(pixels, new RectInt(f * width, rows[2 - r], width, rows[3 - r] - rows[2 - r]));
                    Vector2 pivot = r == 2 ? new Vector2(.5f, .5f)
                        : new Vector2((f * width + width * .5f - bounds.xMin) / bounds.width, -footY * ppu / bounds.height);
                    var sprite = Sprite.Create(_texture, bounds, pivot, ppu, 0, SpriteMeshType.FullRect);
                    sprite.name = $"Edelzio_Interaction_{r}_{f}";
                    _frames[r * 4 + f] = sprite;
                }
            }
            return _frames[Mathf.Clamp(row, 0, 2) * 4 + (frame & 3)];
        }

        private static int[] RowBoundaries(Color32[] pixels)
        {
            // A arte autorada tem margens de alturas diferentes. Dividir em três
            // partes iguais corta os pés do café e os coloca na pose de sentar.
            int height = _texture.height, width = _texture.width;
            int[] rows = { 0, height / 3, height * 2 / 3, height };
            for (int boundary = 1; boundary <= 2; boundary++)
            {
                int start = rows[boundary] - height / 6;
                int end = rows[boundary] + height / 6;
                int gapStart = start, longestGap = 0;
                for (int y = start; y <= end; y++)
                {
                    bool occupied = y == end;
                    for (int x = 0; !occupied && x < width; x++)
                        occupied = pixels[y * width + x].a >= 64;
                    if (!occupied) continue;
                    if (y - gapStart > longestGap)
                    {
                        longestGap = y - gapStart;
                        rows[boundary] = (gapStart + y) / 2;
                    }
                    gapStart = y + 1;
                }
            }
            return rows;
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
