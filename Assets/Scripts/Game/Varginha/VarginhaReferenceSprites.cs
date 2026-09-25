using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Original reference JPEGs and authored atlases, sliced once with point sampling. No scene or collider resizing.</summary>
    public static class VarginhaReferenceSprites
    {
        private static readonly Dictionary<string, Sprite> Sprites = new();
        private static readonly Dictionary<string, Texture2D> Sheets = new();
        private static readonly Dictionary<string, Sprite> Cache = new();
        public const float EdelzioVisualScale = 1.18f;
        public const float EdelzioPixelsPerUnit = VarginhaStudentSprites.PixelsPerUnit / EdelzioVisualScale;
        private static Sprite[][] _walk, _attack;
        private static Texture2D _fabio;
        private static readonly RectInt[] FireRects = { new(458, 155, 61, 103), new(535, 157, 63, 103), new(622, 161, 76, 99) };
        public static bool HasEdelzio => Source("Edelzio") != null;

        public static void ClearCache()
        {
            Cache.Clear();
            Sprites.Clear();
            Sheets.Clear();
            _walk = null;
            _attack = null;
            _fabio = null;
        }

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

        private static Texture2D Source(string name)
        {
            if (Sheets.TryGetValue(name, out var cached) && cached != null) return cached;
            var data = Resources.Load<TextAsset>("Varginha/References/" + name);
            if (data == null) return null;
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, data.bytes)) { Object.Destroy(texture); return null; }
            texture.name = "ReferenceSource_" + name;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            Sheets[name] = texture;
            return texture;
        }

        // Coordinates use the top-left of the submitted image, including JPEG compression tolerance.
        private static Texture2D Cut(string source, RectInt rect, int width, int height, string key)
        {
            var input = Source(source);
            if (input == null || rect.x < 0 || rect.y < 0 || rect.xMax > input.width || rect.yMax > input.height
                || width <= 0 || height <= 0 || rect.width <= 0 || rect.height <= 0) return null;
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int sx = rect.x + Mathf.Min(rect.width - 1, (int)((x + .5f) * rect.width / width));
                int sy = rect.y + Mathf.Min(rect.height - 1, (int)((y + .5f) * rect.height / height));
                Color32 p = input.GetPixel(sx, input.height - 1 - sy);
                if (Background(source, p)) p = new Color32(0, 0, 0, 0);
                pixels[(height - 1 - y) * width + x] = p;
            }
            return Texture(key, width, height, pixels);
        }

        private static bool Near(Color32 p, int r, int g, int b, int tolerance)
            => Mathf.Abs(p.r - r) <= tolerance && Mathf.Abs(p.g - g) <= tolerance && Mathf.Abs(p.b - b) <= tolerance;

        private static bool Background(string source, Color32 p)
        {
            switch (source)
            {
                case "Katana": return Near(p, 105, 110, 79, 28);
                case "Cage": return Near(p, 208, 210, 212, 34) || Near(p, 202, 203, 205, 30);
                case "Edelzio": case "Fabio": return Near(p, 163, 164, 172, 32) || Near(p, 164, 167, 174, 30);
                case "Wood": return Near(p, 150, 138, 138, 22) || Near(p, 102, 85, 95, 12);
                case "Hearts": return Near(p, 110, 99, 99, 19) || Near(p, 115, 115, 112, 19);
                case "Fire": return p.r < 95 && p.g < 85 && p.b > p.r;
                default: return p.r > 235 && p.g > 235 && p.b > 235;
            }
        }

        private static Texture2D Texture(string name, int width, int height, Color32[] pixels)
        {
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false)
            { name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, anisoLevel = 0 };
            result.SetPixels32(pixels);
            result.Apply(false, false);
            return result;
        }

        private static Sprite Slice(string key, string source, RectInt rect, int width, int height, float ppu)
        {
            if (Sprites.TryGetValue(key, out var sprite) && sprite != null && sprite.texture != null) return sprite;
            var texture = Cut(source, rect, width, height, key);
            if (texture == null) return null;
            sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * .5f, ppu, 0, SpriteMeshType.FullRect);
            sprite.name = "Reference_" + key;
            Sprites[key] = sprite;
            return sprite;
        }

        public static Sprite GetProp(string id)
        {
            switch (id)
            {
                case "Palette": return Slice(id, id, new RectInt(70, 33, 580, 468), 80, 64, 64);
                case "Microphone": return Slice(id, id, new RectInt(301, 140, 184, 470), 24, 64, 64);
                // The scabbard is not part of the cutting blade.
                case "Katana": return Slice(id, id, new RectInt(378, 39, 96, 650), 14, 84, 75);
                case "Piano": return Slice(id, id, new RectInt(11, 72, 698, 578), 80, 66, 64);
                case "Cage": return Slice(id, id, new RectInt(216, 236, 626, 520), 80, 66, 48);
                default: return null;
            }
        }

        public static Sprite ForId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (id.StartsWith("HostageCage")) return GetProp("Cage");
            if (id == "StudentAttack_Katana") return GetProp("Katana");
            if (id == "StudentAttack_FallingPiano") return GetProp("Piano");
            if (id == "StudentAttack_Microphone") return GetProp("Microphone");
            if (id == "StudentAttack_Art") return GetProp("Palette");
            if (id.StartsWith("ToyBox_Open")) return Wood("ChestOpen", Vector2.one);
            if (id.StartsWith("ToyBox")) return Wood("Chest", Vector2.one);
            if (id.StartsWith("CoffeeTable") || id.StartsWith("Desk") || id.StartsWith("SchoolDesk")) return Wood("Table", Vector2.one);
            if (id.StartsWith("SchoolChair") || id.StartsWith("Chair")) return Wood("Stool", Vector2.one);
            if (id.StartsWith("ChurchPew")) return Wood("Bench", Vector2.one);
            if (id.StartsWith("ChurchAltar")) return Wood("Altar", Vector2.one);
            if (id.StartsWith("Nightstand")) return Wood("Crate", Vector2.one);
            if (id.StartsWith("Dresser")) return Wood("WideCrate", Vector2.one);
            return null;
        }

        public static Sprite Wood(string item, Vector2 size)
        {
            RectInt rect;
            switch (item)
            {
                case "Chest": rect = new RectInt(57, 148, 155, 147); break;
                case "ChestOpen": rect = new RectInt(255, 76, 148, 219); break;
                case "Barrel": rect = new RectInt(444, 99, 123, 198); break;
                case "Bucket": rect = new RectInt(603, 176, 84, 113); break;
                case "Table": case "Desk": case "Altar": rect = new RectInt(80, 306, 258, 186); break;
                case "Stool": rect = new RectInt(350, 337, 96, 111); break;
                case "Bench": case "Pew": rect = new RectInt(448, 337, 210, 115); break;
                case "Crate": rect = new RectInt(79, 516, 139, 149); break;
                case "WideCrate": rect = new RectInt(276, 516, 194, 149); break;
                case "Sign": rect = new RectInt(530, 449, 128, 218); break;
                default: return null;
            }
            int width = Mathf.Max(4, Mathf.RoundToInt(size.x * 32));
            int height = Mathf.Max(4, Mathf.RoundToInt(size.y * 32));
            return Slice("Wood_" + item + "_" + width + "x" + height, "Wood", rect, width, height, 32);
        }

        public static Sprite Heart(bool full) => Slice(full ? "HeartFull" : "HeartEmpty", "Hearts",
            full ? new RectInt(382, 483, 71, 78) : new RectInt(529, 483, 71, 78), 28, 30, 30);

        public static Sprite Fire(int frame)
        {
            int index = ((frame % 3) + 3) % 3;
            return Slice("Fire_" + index, "Fire", FireRects[index], 22, 32, 32);
        }

        public static Texture2D FabioAtlas()
        {
            // Load the pre-built 256×256 atlas generated from the new Fabio sprite sheet.
            // Falls back to the runtime JPEG slicer if the asset is missing.
            if (_fabio == null)
                _fabio = Resources.Load<Texture2D>("Varginha/Allies/Fabio") ?? CharacterAtlas("Fabio");
            return _fabio;
        }

        private static Texture2D CharacterAtlas(string name)
        {
            var input = Source(name);
            if (input == null) return null;
            float scaleX = input.width / 637f;
            float scaleY = input.height / 1024f;
            bool edelzio = name == "Edelzio";
            // Coordinates measured from the new 637×1024 sprite sheets (Fabio = black Metallica shirt; Edelzio = yellow shirt).
            // Row 2 (4 directional standing poses): front, back, right-side, left-side.
            // Row 3 (8 walk frames): 4 front-walk then 4 side-walk.
            RectInt front = edelzio
                ? new(Mathf.RoundToInt(60 * scaleX), Mathf.RoundToInt(605 * scaleY), Mathf.RoundToInt(80 * scaleX), Mathf.RoundToInt(217 * scaleY))
                : new(Mathf.RoundToInt(43 * scaleX), Mathf.RoundToInt(524 * scaleY), Mathf.RoundToInt(86 * scaleX), Mathf.RoundToInt(236 * scaleY));
            RectInt back = edelzio
                ? new(Mathf.RoundToInt(201 * scaleX), Mathf.RoundToInt(606 * scaleY), Mathf.RoundToInt(78 * scaleX), Mathf.RoundToInt(215 * scaleY))
                : new(Mathf.RoundToInt(197 * scaleX), Mathf.RoundToInt(524 * scaleY), Mathf.RoundToInt(85 * scaleX), Mathf.RoundToInt(236 * scaleY));
            RectInt side = edelzio
                ? new(Mathf.RoundToInt(362 * scaleX), Mathf.RoundToInt(606 * scaleY), Mathf.RoundToInt(63 * scaleX), Mathf.RoundToInt(216 * scaleY))
                : new(Mathf.RoundToInt(364 * scaleX), Mathf.RoundToInt(524 * scaleY), Mathf.RoundToInt(71 * scaleX), Mathf.RoundToInt(236 * scaleY));
            int rowY = Mathf.RoundToInt((edelzio ? 846 : 788) * scaleY);
            int rowH = Mathf.RoundToInt((edelzio ? 147 : 164) * scaleY);
            // xs[0..4] = left-edge x of front walk frames 1-4 (frame 0 uses the standing front rect above).
            // xs[4..8] = left-edge x of side walk frames (all 4 have dedicated cells).
            int[] baseXs = edelzio
                ? new[] { 27, 99, 173, 247, 321, 397, 474, 550, 617 }
                : new[] { 20, 97, 174, 250, 330, 404, 477, 553, 623 };
            int[] xs = new int[baseXs.Length];
            for (int i = 0; i < xs.Length; i++) xs[i] = Mathf.RoundToInt(baseXs[i] * scaleX);

            var pixels = new Color32[256 * 256];
            for (int direction = 0; direction < 4; direction++)
            for (int frame = 0; frame < 4; frame++)
            {
                RectInt box = direction == 0 ? front : direction == 3 ? back : side;
                if (frame > 0 && direction != 3)
                {
                    int i = direction == 0 ? frame - 1 : 3 + frame;
                    if (i + 1 < xs.Length)
                        box = new RectInt(xs[i], rowY, xs[i + 1] - xs[i], rowH);
                }
                // Trim transparent JPEG margins before fitting each frame to a common foot baseline.
                var crop = Cut(name, box, box.width, box.height, name + "_crop");
                if (crop == null) continue;
                Color32[] cropPixels = crop.GetPixels32();
                int minX = crop.width, maxX = -1, minY = crop.height, maxY = -1;
                for (int y = 0; y < crop.height; y++) for (int x = 0; x < crop.width; x++)
                    if (cropPixels[y * crop.width + x].a > 0)
                    { minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x); minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y); }
                if (maxX < minX || maxY < minY)
                {
                    if (Application.isPlaying) Object.Destroy(crop); else Object.DestroyImmediate(crop);
                    continue;
                }
                int h = 50, w = Mathf.Clamp(Mathf.RoundToInt((maxX - minX + 1) * 50f / (maxY - minY + 1)), 1, 50);
                for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                {
                    int sx = minX + Mathf.Min(maxX - minX, (int)((x + .5f) * (maxX - minX + 1) / w));
                    int sy = minY + Mathf.Min(maxY - minY, (int)((y + .5f) * (maxY - minY + 1) / h));
                    int dx = direction == 1 ? w - 1 - x : x;
                    // Back reference has one pose; articulate the lower legs while keeping the head still.
                    int stride = direction == 3 && y < 22 ? (frame == 1 ? (x < w / 2 ? -1 : 1)
                        : frame == 2 ? (x < w / 2 ? 1 : -1) : frame == 3 ? 1 : 0) : 0;
                    int px = frame * 64 + (64 - w) / 2 + dx + stride;
                    int py = (3 - direction) * 64 + 6 + y;
                    if (px >= 0 && px < 256 && py >= 0 && py < 256)
                        pixels[py * 256 + px] = cropPixels[sy * crop.width + sx];
                }
                if (Application.isPlaying) Object.Destroy(crop); else Object.DestroyImmediate(crop);
            }
            return Texture("Reference_" + name + "Atlas", 256, 256, pixels);
        }

        public static Sprite[][] EdelzioWalkFrames()
        {
            if (_walk != null && _walk[0][0] != null && _walk[0][0].texture != null) return _walk;
            // Load the pre-built 256×256 atlas generated from the new Edelzio sprite sheet.
            // Falls back to the runtime JPEG slicer if the asset is missing.
            var texture = Resources.Load<Texture2D>("Varginha/Allies/Edelzio") ?? CharacterAtlas("Edelzio");
            if (texture == null) return null;
            texture.filterMode = FilterMode.Point;
            _walk = new Sprite[4][];
            for (int d = 0; d < 4; d++)
            {
                _walk[d] = new Sprite[4];
                for (int f = 0; f < 4; f++)
                {
                    _walk[d][f] = Sprite.Create(texture, new Rect(f * 64, (3 - d) * 64, 64, 64), new Vector2(.5f, .5f - (26f / 64f) * (1f - 1f / EdelzioVisualScale)), EdelzioPixelsPerUnit);
                    _walk[d][f].name = "Edelzio_Reference_" + d + "_" + f;
                }
            }
            return _walk;
        }

        public static Sprite EdelzioActionFrame(string poseId, int frame = 0)
        {
            var walk = EdelzioWalkFrames();
            if (walk == null) return null;
            return walk[0][frame & 3];
        }

        public static Sprite[][] EdelzioAttackFrames()
        {
            if (_attack != null && _attack[0] != null && _attack[0][0] != null
                && _attack[0][0].texture != null) return _attack;
            var texture = Resources.Load<Texture2D>("Varginha/EdelzioPunchV2")
                ?? Resources.Load<Texture2D>("Varginha/EdelzioAttackV1");
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                int cols = texture.width / 64;
                _attack = new Sprite[4][];
                for (int d = 0; d < 4; d++)
                {
                    _attack[d] = new Sprite[cols];
                    for (int f = 0; f < cols; f++)
                    {
                        _attack[d][f] = Sprite.Create(texture,
                            new Rect(f * 64, (3 - d) * 64, 64, 64),
                            new Vector2(.5f, .5f - (26f / 64f) * (1f - 1f / EdelzioVisualScale)),
                            EdelzioPixelsPerUnit);
                        _attack[d][f].name = $"Edelzio_Attack_{d}_{f}";
                    }
                }
                return _attack;
            }

            var walk = EdelzioWalkFrames();
            if (walk == null) return null;
            _attack = new Sprite[4][];

            for (int d = 0; d < 4; d++)
            {
                _attack[d] = new Sprite[18];
                for (int combo = 0; combo < 3; combo++)
                for (int f = 0; f < 6; f++)
                    _attack[d][combo * 6 + f] = VarginhaEdelzioCombatArt.Frame(walk[d][0], d, f, combo);
            }
            return _attack;
        }
    }
}
