using UnityEditor;
using UnityEngine;

namespace Game.Editor.Testing
{
    /// <summary>Importa a folha única de caminhada e ações sem borrar os pixels.</summary>
    public sealed class EdelzioAtlasImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath != "Assets/Resources/Varginha/EdelzioTopDownV3.png") return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            // A folha contém 24 quadros (4 colunas x 6 linhas). Mantê-la como
            // Multiple preserva os sprites recortados usados pelo jogador e
            // evita que o renderer caia no sprite procedural da cena.
            importer.spriteImportMode = SpriteImportMode.Multiple;
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
