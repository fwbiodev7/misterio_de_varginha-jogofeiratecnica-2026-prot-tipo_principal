using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Varginha
{
    /// <summary>Configuração de nitidez que preserva o enquadramento e os PPUs já autorados.</summary>
    public static class VarginhaPixelPresentation
    {
        public enum Pipeline { BuiltIn, Universal, HighDefinition, Custom }

        public static Pipeline DetectPipeline()
        {
            var asset = GraphicsSettings.currentRenderPipeline;
            if (asset == null) return Pipeline.BuiltIn;
            string typeName = asset.GetType().FullName ?? string.Empty;
            if (typeName.Contains("Universal")) return Pipeline.Universal;
            if (typeName.Contains("HighDefinition")) return Pipeline.HighDefinition;
            return Pipeline.Custom;
        }

        public static void Configure(Camera camera)
        {
            Pipeline pipeline = DetectPipeline();
            if (camera == null || !camera.orthographic || pipeline == Pipeline.HighDefinition) return;
            camera.allowMSAA = false;
            camera.allowHDR = false;
            camera.allowDynamicResolution = false;
            // O mapa combina folhas de 44.1379/66.2 PPU e sprites procedurais de 64 PPU.
            // Um PixelPerfectCamera imporia outro zoom/grade; manter o ortho de cada fase
            // evita recortar arenas e comprometer as animações existentes.
        }
    }
}
