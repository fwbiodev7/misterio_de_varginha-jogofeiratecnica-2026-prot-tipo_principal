using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Testing
{
    /// <summary>Point-sampled production atlases with identical feet alignment and in-game pixel density.</summary>
    public sealed class VarginhaStudentAtlasImporter : AssetPostprocessor
    {
        public const string SourceFolder = "Assets/ArtSource/Allies";
        public const string OutputFolder = "Assets/Resources/Varginha/Allies";
        private const int FootBaseline = 6;
        private static readonly HashSet<string> PendingSources = new HashSet<string>();

        public override uint GetVersion() => 2;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(SourceFolder + "/") && !assetPath.StartsWith(OutputFolder + "/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;
            importer.maxTextureSize = 4096;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets) QueueSource(path);
            foreach (string path in movedAssets) QueueSource(path);
        }

        private static void QueueSource(string path)
        {
            // Only source imports enqueue work. Importing the generated PNG cannot enqueue itself.
            if (!path.StartsWith(SourceFolder + "/", StringComparison.Ordinal)
                || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return;
            if (!PendingSources.Add(path)) return;
            EditorApplication.delayCall -= ExportPending;
            EditorApplication.delayCall += ExportPending;
        }

        [InitializeOnLoadMethod]
        private static void RestoreMissingAtlases()
        {
            if (!Directory.Exists(SourceFolder)) return;
            foreach (string path in Directory.GetFiles(SourceFolder, "*.png"))
                if (!File.Exists(OutputFolder + "/" + Path.GetFileName(path)))
                    QueueSource(path.Replace('\\', '/'));
        }

        private static void ExportPending()
        {
            var paths = new List<string>(PendingSources);
            PendingSources.Clear();
            foreach (string path in paths)
            {
                if (!File.Exists(path)) continue;
                try { Export(path); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }

        [MenuItem("Tools/Varginha/Atualizar atlas dos nove alunos")]
        public static void ExportAll()
        {
            if (!Directory.Exists(SourceFolder)) return;
            Directory.CreateDirectory(OutputFolder);
            foreach (var path in Directory.GetFiles(SourceFolder, "*.png")) Export(path.Replace('\\', '/'));
            AssetDatabase.Refresh();
        }

        private static void Export(string path)
        {
            var source = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (source == null) throw new InvalidOperationException("Atlas não importado: " + path);
            if (source.width != source.height)
                throw new InvalidOperationException("O atlas precisa ser quadrado com 4 × 4 células: " + path);
            var input = source.GetPixels32();
            var boxes = new RectInt[16];
            int tallest = 1, widest = 1;
            for (int row = 0; row < 4; row++) for (int col = 0; col < 4; col++)
            {
                int x0 = col * source.width / 4, x1 = (col + 1) * source.width / 4;
                int y0 = (3 - row) * source.height / 4, y1 = (4 - row) * source.height / 4;
                int minX = x1, maxX = x0, minY = y1, maxY = y0;
                for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++)
                {
                    if (input[y * source.width + x].a < 128) continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
                if (maxX < minX || maxY < minY) throw new InvalidOperationException("Célula vazia: " + path);
                boxes[row * 4 + col] = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
                tallest = Mathf.Max(tallest, maxY - minY + 1);
                widest = Mathf.Max(widest, maxX - minX + 1);
            }
            var pixels = new Color32[256 * 256];
            string student = Path.GetFileNameWithoutExtension(path);
            int targetHeight = student == "AnnaSabia" ? 44 : student == "AnaTavares" ? 56 : 50;
            // One ratio per sheet preserves stride/bob and the requested short/tall silhouettes.
            float ratio = Mathf.Min(targetHeight / (float)tallest, 56f / widest);
            for (int i = 0; i < boxes.Length; i++)
            {
                var box = boxes[i];
                int width = Mathf.Max(1, Mathf.RoundToInt(box.width * ratio));
                int height = Mathf.Max(1, Mathf.RoundToInt(box.height * ratio));
                int baseX = (i % 4) * 64 + (64 - width) / 2;
                int baseY = (3 - i / 4) * 64 + FootBaseline;
                var frame = new Color32[width * height];
                int lowestSolid = height;
                for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                {
                    int sx = box.x + Mathf.Min(box.width - 1, Mathf.FloorToInt((x + .5f) / ratio));
                    int sy = box.y + Mathf.Min(box.height - 1, Mathf.FloorToInt((y + .5f) / ratio));
                    Color32 pixel = input[sy * source.width + sx];
                    frame[y * width + x] = pixel;
                    if (pixel.a >= 128) lowestSolid = Mathf.Min(lowestSolid, y);
                }
                if (lowestSolid == height) throw new InvalidOperationException("Célula sem pixels visíveis após redução: " + path);
                // Downsampling can skip a one-pixel shoe outline; align the resulting feet, too.
                for (int y = lowestSolid; y < height; y++) for (int x = 0; x < width; x++)
                    pixels[(baseY + y - lowestSolid) * 256 + baseX + x] = frame[y * width + x];
            }
            var output = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            output.SetPixels32(pixels); output.Apply();
            string destination = OutputFolder + "/" + Path.GetFileName(path);
            var encoded = output.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(output);
            Directory.CreateDirectory(OutputFolder);
            if (File.Exists(destination) && SameBytes(File.ReadAllBytes(destination), encoded)) return;
            File.WriteAllBytes(destination, encoded);
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Atlas de aluno atualizado: " + destination);
        }

        private static bool SameBytes(byte[] existing, byte[] generated)
        {
            if (existing.Length != generated.Length) return false;
            for (int i = 0; i < existing.Length; i++)
                if (existing[i] != generated[i]) return false;
            return true;
        }
    }
}
