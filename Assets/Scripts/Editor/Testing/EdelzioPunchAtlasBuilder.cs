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
                        RectInt bounds = Bounds(source, Cell(source, direction, Poses[combo][phase]));
                        // Align the foot support, not the arm's changing bounding box.
                        int footLeft = bounds.xMax, footRight = bounds.xMin;
                        int footBand = Mathf.Max(1, neutral.height / 9);
                        for (int y = bounds.yMin; y < bounds.yMin + footBand; y++)
                        for (int x = bounds.xMin; x < bounds.xMax; x++)
                            if (source.GetPixel(x, y).a >= .5f)
                            { footLeft = Mathf.Min(footLeft, x); footRight = Mathf.Max(footRight, x); }
                        float anchor = (footLeft + footRight + 1) * .5f;
                        int column = combo * 6 + phase;
                        for (int y = 1; y < 63; y++)
                        for (int x = 1; x < 63; x++)
                        {
                            int sx = Mathf.FloorToInt(anchor + (x + .5f - 32) / scale);
                            int sy = Mathf.FloorToInt(bounds.yMin + (y + .5f - original.yMin % 64) / scale);
                            if (!bounds.Contains(new Vector2Int(sx, sy))) continue;
                            Color32 color = source.GetPixel(sx, sy);
                            if (color.a < 128) continue;
                            pixels[((3 - direction) * 64 + y) * atlas.width + column * 64 + x] = color;
                        }
                    }
                }
                atlas.SetPixels32(pixels);
                atlas.Apply();
                File.WriteAllBytes(Output, atlas.EncodeToPNG());
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

        private static RectInt Cell(Texture2D texture, int row, int column)
        {
            int x = Mathf.RoundToInt(column * texture.width / 9f);
            int right = Mathf.RoundToInt((column + 1) * texture.width / 9f);
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
