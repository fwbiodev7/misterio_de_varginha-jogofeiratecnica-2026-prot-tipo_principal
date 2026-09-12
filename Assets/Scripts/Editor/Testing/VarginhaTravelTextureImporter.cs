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
            importer.textureType=TextureImporterType.Default;
            importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;
            importer.isReadable=true;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;
        }
    }
}
