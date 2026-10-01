using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.Editor.Testing
{
    public static class VarginhaSchoolComputerLabImporter
    {
        public static void Configure()
        {
            const string path = "Assets/Resources/Varginha/SchoolComputerLab.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 128;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            // Bounds measured on the generated 1774x887 atlas, rather than assuming a perfect grid.
            var slices = new[]
            {
                Crop("Desk", 10, 195, 435, 240), Crop("ComputerDesk", 480, 95, 425, 340),
                Crop("Chair", 1015, 155, 180, 280), Crop("Whiteboard", 1305, 120, 459, 260),
                Crop("Window", 5, 525, 555, 275), Crop("Shelf", 605, 480, 225, 365),
                Crop("Projector", 995, 525, 210, 190), Crop("TeacherDesk", 1275, 610, 489, 230)
            };
            foreach (var slice in slices)
                foreach (var previous in provider.GetSpriteRects())
                    if (previous.name == slice.name) slice.spriteID = previous.spriteID;
            provider.SetSpriteRects(slices);
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names != null)
            {
                var pairs = new System.Collections.Generic.List<SpriteNameFileIdPair>();
                foreach (var slice in slices) pairs.Add(new SpriteNameFileIdPair(slice.name, slice.spriteID));
                names.SetNameFileIdPairs(pairs);
            }
            provider.Apply();
            importer.SaveAndReimport();
        }
        private static SpriteRect Crop(string name, int x, int top, int width, int height) => new()
        {
            name = name, rect = new Rect(x, 887 - top - height, width, height),
            alignment = SpriteAlignment.Center, pivot = Vector2.one * .5f, spriteID = GUID.Generate()
        };
    }
}
