using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Testing
{
    /// <summary>Reproducible slicing of the ImageGen source into the game's 64px combat cells.</summary>
    public static class EdelzioPunchAtlasBuilder
    {
        private const string Output = "Assets/Resources/Varginha/EdelzioPunchV2.png";
        // Guard, anticipation, extension, contact, follow-through, return to guard.
        private static readonly int[][] Poses =
        {
            new[] { 0, 1, 2, 3, 4, 0 },
            new[] { 0, 4, 5, 5, 6, 0 },
            new[] { 0, 6, 7, 7, 8, 0 }
        };

        [MenuItem("Varginha/Art/Rebuild Edelzio Punch Atlas")]
        public static void Build()
        {
            BuildFurniture();
            BuildHouseProps();
            BuildWalk("Assets/ArtSource/EdelzioCleanSourceV2.png", "Assets/Resources/Varginha/Allies/Edelzio.png");
            BuildWalk("Assets/ArtSource/PadreFabioSourceV1.png", "Assets/Resources/Varginha/PadreFabioV1.png");
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var baseline = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var atlas = new Texture2D(18 * 64, 4 * 64, TextureFormat.RGBA32, false);
            try
            {
                source.LoadImage(File.ReadAllBytes("Assets/ArtSource/EdelzioPunchSourceV2.png"));
                baseline.LoadImage(File.ReadAllBytes("Assets/Resources/Varginha/Allies/Edelzio.png"));
                var pixels = new Color32[atlas.width * atlas.height];
                for (int direction = 0; direction < 4; direction++)
                {
                    var neutral = Bounds(source, Cell(source, direction, 0));
                    var original = Bounds(baseline, new RectInt(0, (3 - direction) * 64, 64, 64));
                    // One scale for the entire row: raised fists must not shrink the body.
                    float scale = original.height / (float)neutral.height;
                    for (int combo = 0; combo < 3; combo++)
                    for (int phase = 0; phase < 6; phase++)
                    {
                        int column = combo * 6 + phase;
                        if (phase == 0 && combo == 0)
                        {
                            for (int y = 0; y < 64; y++)
                            for (int x = 0; x < 64; x++)
                            {
                                Color32 baseColor = baseline.GetPixel(x, (3 - direction) * 64 + y);
                                pixels[((3 - direction) * 64 + y) * atlas.width + column * 64 + x] = baseColor;
                            }
                            continue;
                        }

                        RectInt bounds = Bounds(source, Cell(source, direction, Poses[combo][phase]));
                        // Align the foot support, not the arm's changing bounding box.
                        int footLeft = bounds.xMax, footRight = bounds.xMin;
                        int footBand = Mathf.Max(1, neutral.height / 9);
                        for (int y = bounds.yMin; y < bounds.yMin + footBand; y++)
                        for (int x = bounds.xMin; x < bounds.xMax; x++)
                            if (source.GetPixel(x, y).a >= .5f)
                            { footLeft = Mathf.Min(footLeft, x); footRight = Mathf.Max(footRight, x); }
                        float anchor = (footLeft + footRight + 1) * .5f;
                        for (int y = 1; y < 63; y++)
                        for (int x = 1; x < 63; x++)
                        {
                            int sx = Mathf.FloorToInt(anchor + (x + .5f - 32) / scale);
                            int sy = Mathf.FloorToInt(bounds.yMin + (y + .5f - original.yMin % 64) / scale);
                            if (!bounds.Contains(new Vector2Int(sx, sy))) continue;
                            Color32 color = source.GetPixel(sx, sy);
                            if (color.a < 128) continue;
                            // Preserve the authored skin/shirt/hair palette shared with the walk sheet.
                            pixels[((3 - direction) * 64 + y) * atlas.width + column * 64 + x] = color;
                        }
                    }
                }
                atlas.SetPixels32(pixels);
                atlas.Apply();
                WritePng(Output, atlas);
                AssetDatabase.ImportAsset(Output, ImportAssetOptions.ForceUpdate);
                Debug.Log("Edelzio punch atlas built: 72 aligned frames, " + Output);
            }
            finally
            {
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(baseline);
                Object.DestroyImmediate(atlas);
            }
        }

        private static void BuildWalk(string sourcePath, string outputPath)
        {
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var output = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            try
            {
                source.LoadImage(File.ReadAllBytes(sourcePath));
                var pixels = new Color32[256 * 256];
                for (int row = 0; row < 4; row++)
                {
                    var neutral = Bounds(source, GridCell(source, row, 0, 4));
                    float scale = 42f / neutral.height;
                    for (int frame = 0; frame < 4; frame++)
                    {
                        var bounds = Bounds(source, GridCell(source, row, frame, 4));
                        for (int y = 1; y < 63; y++)
                        for (int x = 1; x < 63; x++)
                        {
                            int sx = Mathf.FloorToInt(bounds.center.x + (x + .5f - 32) / scale);
                            int sy = Mathf.FloorToInt(bounds.yMin + (y + .5f - 6) / scale);
                            if (!bounds.Contains(new Vector2Int(sx, sy))) continue;
                            Color32 color = source.GetPixel(sx, sy);
                            if (color.a < 128) continue;
                            pixels[((3 - row) * 64 + y) * 256 + frame * 64 + x] = color;
                        }
                    }
                }
                output.SetPixels32(pixels);
                output.Apply();
                WritePng(outputPath, output);
                AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            }
            finally { Object.DestroyImmediate(source); Object.DestroyImmediate(output); }
        }

        private static void BuildFurniture()
        {
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var output = new Texture2D(256, 192, TextureFormat.RGBA32, false);
            const string path = "Assets/Resources/Varginha/FurnitureV1.png";
            try
            {
                source.LoadImage(File.ReadAllBytes("Assets/ArtSource/FurnitureSourceV1.png"));
                // Source gutters are irregular: these boundaries avoid clipping the wide desk and sofa.
                float[] columns = { 0, .285f, .485f, .75f, 1 };
                var pixels = new Color32[256 * 192];
                for (int row = 0; row < 3; row++)
                for (int column = 0; column < 4; column++)
                {
                    int left = Mathf.RoundToInt(columns[column] * source.width);
                    int right = Mathf.RoundToInt(columns[column + 1] * source.width);
                    int bottom = Mathf.RoundToInt((2 - row) * source.height / 3f);
                    int top = Mathf.RoundToInt((3 - row) * source.height / 3f);
                    var bounds = Bounds(source, new RectInt(left, bottom, right - left, top - bottom));
                    // The saved scene's transform supplies each object's width/height, just as for legacy art.
                    for (int y = 4; y < 60; y++)
                    for (int x = 4; x < 60; x++)
                    {
                        int sx = bounds.xMin + Mathf.FloorToInt((x - 3.5f) * bounds.width / 56f);
                        int sy = bounds.yMin + Mathf.FloorToInt((y - 3.5f) * bounds.height / 56f);
                        Color32 color = source.GetPixel(sx, sy);
                        if (color.a < 128) continue;
                        pixels[((2 - row) * 64 + y) * 256 + column * 64 + x] = color;
                    }
                }
                output.SetPixels32(pixels);
                output.Apply();
                WritePng(path, output);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            finally { Object.DestroyImmediate(source); Object.DestroyImmediate(output); }
        }

        private static void BuildHouseProps()
        {
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var output = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            const string path = "Assets/Resources/Varginha/HousePropsV1.png";
            try
            {
                source.LoadImage(File.ReadAllBytes("Assets/ArtSource/HousePropsSourceV1.png"));
                var pixels = new Color32[256 * 256];
                float[] rows = { 0f, .29f, .51f, .735f, 1f };
                for (int row = 0; row < 4; row++)
                for (int column = 0; column < 4; column++)
                {
                    int left = Mathf.RoundToInt(column * source.width / 4f);
                    int right = Mathf.RoundToInt((column + 1) * source.width / 4f);
                    int bottom = Mathf.RoundToInt((1f - rows[row + 1]) * source.height);
                    int top = Mathf.RoundToInt((1f - rows[row]) * source.height);
                    var bounds = Bounds(source, new RectInt(left, bottom, right - left, top - bottom));
                    for (int y = 4; y < 60; y++)
                    for (int x = 4; x < 60; x++)
                    {
                        int sx = bounds.xMin + Mathf.FloorToInt((x - 3.5f) * bounds.width / 56f);
                        int sy = bounds.yMin + Mathf.FloorToInt((y - 3.5f) * bounds.height / 56f);
                        Color32 color = source.GetPixel(sx, sy);
                        if (color.a >= 128)
                            pixels[((3 - row) * 64 + y) * 256 + column * 64 + x] = color;
                    }
                }
                output.SetPixels32(pixels);
                output.Apply();
                WritePng(path, output);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            finally { Object.DestroyImmediate(source); Object.DestroyImmediate(output); }
        }

        private static void WritePng(string path, Texture2D texture)
        {
            var bytes = texture.EncodeToPNG();
            if (File.Exists(path))
            {
                var previous = File.ReadAllBytes(path);
                bool same = previous.Length == bytes.Length;
                for (int i = 0; same && i < bytes.Length; i++) same = bytes[i] == previous[i];
                if (same) return;
            }
            AssetDatabase.ReleaseCachedFileHandles();
            File.WriteAllBytes(path, bytes);
        }

        private static RectInt Cell(Texture2D texture, int row, int column)
            => GridCell(texture, row, column, 9);

        private static RectInt GridCell(Texture2D texture, int row, int column, int columns)
        {
            int x = Mathf.RoundToInt(column * texture.width / (float)columns);
            int right = Mathf.RoundToInt((column + 1) * texture.width / (float)columns);
            int y = Mathf.RoundToInt((3 - row) * texture.height / 4f);
            int top = Mathf.RoundToInt((4 - row) * texture.height / 4f);
            return new RectInt(x, y, right - x, top - y);
        }

        private static RectInt Bounds(Texture2D texture, RectInt cell)
        {
            int left = cell.xMax, right = cell.xMin, bottom = cell.yMax, top = cell.yMin;
            for (int y = cell.yMin; y < cell.yMax; y++)
            for (int x = cell.xMin; x < cell.xMax; x++)
                if (texture.GetPixel(x, y).a >= .5f)
                { left = Mathf.Min(left, x); right = Mathf.Max(right, x); bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y); }
            if (left > right || bottom > top) throw new InvalidDataException("Empty Edelzio source cell: " + cell);
            return new RectInt(left, bottom, right - left + 1, top - bottom + 1);
        }
    }
}
