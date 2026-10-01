using UnityEditor;
using UnityEngine;

namespace Game.Editor.Testing
{
    public sealed class VarginhaTravelTextureImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/Varginha/TravelPixel/")) return;
            var importer=(TextureImporter)assetImporter;
            bool originalTree = assetPath.EndsWith("/TreeReference.png");
            importer.textureType=originalTree ? TextureImporterType.Sprite : TextureImporterType.Default;
            if (originalTree)
            {
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=100;
            }
            importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;
            importer.isReadable=true;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;
        }
    }
}
