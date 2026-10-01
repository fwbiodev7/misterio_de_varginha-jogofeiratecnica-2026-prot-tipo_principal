using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.Editor.Testing
{
    public static class VarginhaIndustrialFacadeImporter
    {
        public static void Configure()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Varginha/SchoolIndustrialFacadeV1.png");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false; importer.isReadable = true;
            importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var slices = new[]
            {
                new SpriteRect { name="Industrial_Cursos_Portao", rect=new Rect(8,24,270,125),
                    alignment=SpriteAlignment.Custom, pivot=new(.5f,0), spriteID=GUID.Generate() },
                new SpriteRect { name="Industrial_Identificacao", rect=new Rect(325,24,210,125),
                    alignment=SpriteAlignment.Custom, pivot=new(.5f,0), spriteID=GUID.Generate() }
            };
            foreach(var slice in slices) foreach(var previous in provider.GetSpriteRects())
                if(previous.name==slice.name) slice.spriteID=previous.spriteID;
            provider.SetSpriteRects(slices);
            var ids = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(ids != null) ids.SetNameFileIdPairs(new[]
            {
                new SpriteNameFileIdPair(slices[0].name,slices[0].spriteID),
                new SpriteNameFileIdPair(slices[1].name,slices[1].spriteID)
            });
            provider.Apply(); importer.SaveAndReimport();
        }
    }
}
