using UnityEditor;
using UnityEngine;

namespace Game.Editor.Testing
{
    public sealed class ReferenceCharacterAtlasImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath != "Assets/Resources/Varginha/CharacterReferencesV1.png"
                && assetPath != "Assets/Resources/Varginha/AllyReferencePropsV1.png"
                && assetPath != "Assets/Resources/Varginha/EdelzioReferenceActionsV1.png") return;
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
