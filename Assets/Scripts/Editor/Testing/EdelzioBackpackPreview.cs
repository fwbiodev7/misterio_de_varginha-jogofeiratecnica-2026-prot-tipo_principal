using System.IO;
using Game.Varginha;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Editor.Testing
{
    public static class EdelzioBackpackPreview
    {
        [MenuItem("Tools/Varginha/Exportar preview da mochila do Edelzio")]
        public static void Export()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            Debug.Log("Edelzio backpack QA pipeline: " + (pipeline == null ? "Built-in" : pipeline.GetType().FullName));
            var sheet = Resources.Load<Texture2D>("Varginha/Allies/Edelzio");
            var attack = Resources.Load<Texture2D>("Varginha/EdelzioPunchV2");
            Directory.CreateDirectory("Logs/BackpackQA");
            using var appearance = new EdelzioBackpackAppearance();
            ExportAtlas(sheet, 4, 4, appearance, "Logs/BackpackQA/edelzio-equipped-atlas.png");
            ExportAtlas(attack, 18, 4, appearance, "Logs/BackpackQA/edelzio-equipped-combat.png");
            var comparison = new Texture2D(256, 128, TextureFormat.RGBA32, false);
            comparison.SetPixels(new Color[256 * 128]);
            for (int direction = 0; direction < 4; direction++)
            {
                var body = Sprite.Create(sheet, new Rect(0, (3 - direction) * 64, 64, 64), new Vector2(.5f, .5f), VarginhaReferenceSprites.EdelzioPixelsPerUnit);
                body.name = "Edelzio_Preview_" + direction;
                comparison.SetPixels(direction * 64, 64, 64, 64, sheet.GetPixels(0, (3 - direction) * 64, 64, 64));
                comparison.SetPixels(direction * 64, 0, 64, 64, appearance.GetFrame(body, direction).texture.GetPixels());
                Object.DestroyImmediate(body);
            }
            WriteEnlarged(comparison, "Logs/BackpackQA/edelzio-before-after.png", 4);
            Object.DestroyImmediate(comparison);
            Debug.Log("Edelzio backpack previews exported to Logs/BackpackQA.");
        }

        private static void ExportAtlas(Texture2D sheet, int columns, int rows,
            EdelzioBackpackAppearance appearance, string path)
        {
            int w = sheet.width / columns, h = sheet.height / rows;
            var result = new Texture2D(sheet.width, sheet.height, TextureFormat.RGBA32, false);
            result.SetPixels(new Color[sheet.width * sheet.height]);
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                var rect = new Rect(column * w, (rows - 1 - row) * h, w, h);
                var body = Sprite.Create(sheet, rect, new Vector2(.5f, .5f), w / 1.45f);
                body.name = "Edelzio_Preview_" + row + "_" + column;
                var equipped = appearance.GetFrame(body, row < 4 ? row : 0);
                result.SetPixels((int)rect.x, (int)rect.y, w, h, equipped.texture.GetPixels(
                    (int)equipped.rect.x, (int)equipped.rect.y, w, h));
                Object.DestroyImmediate(body);
            }
            WriteEnlarged(result, path, 3);
            Object.DestroyImmediate(result);
        }

        private static void WriteEnlarged(Texture2D source, string path, int scale)
        {
            int width = source.width * scale, height = source.height * scale;
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var input = source.GetPixels32();
            var output = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++) output[y * width + x] = input[(y / scale) * source.width + x / scale];
            result.SetPixels32(output);
            result.Apply();
            File.WriteAllBytes(path, result.EncodeToPNG());
            Object.DestroyImmediate(result);
        }
    }
}
