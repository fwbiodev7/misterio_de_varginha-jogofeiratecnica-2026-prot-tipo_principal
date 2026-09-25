using UnityEditor;
using UnityEngine;

namespace Game.Editor.Testing
{
    /// <summary>Importa os keyframes de ataque com pixels nítidos e canal alpha.</summary>
    public sealed class EdelzioAttackAtlasImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath != "Assets/Resources/Varginha/EdelzioAttackV1.png"
                && assetPath != "Assets/Resources/Varginha/EdelzioPunchV2.png"
                && assetPath != "Assets/Resources/Varginha/PadreFabioV1.png") return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
