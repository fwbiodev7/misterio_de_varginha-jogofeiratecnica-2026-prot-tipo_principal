using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace Game.Editor.Testing
{
    /// <summary>
    /// Valida tilemaps na cena ativa:
    /// - Detecta tilemaps vazios
    /// - Verifica presença de TilemapCollider2D
    /// - Verifica bounds razoáveis
    /// - Lista tiles faltando (null tiles)
    /// </summary>
    public static class TilemapValidatorTool
    {
        private const int MinExpectedTiles = 5;
        private const int MaxBoundSize = 500;

        public static void RunValidation()
        {
            var tilemaps = Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Exclude);

            if (tilemaps.Length == 0)
            {
                EditorUtility.DisplayDialog("Tilemap Validator",
                    "⚠️ Nenhum Tilemap encontrado na cena.\n\n" +
                    "Certifique-se de ter um Grid com Tilemap na hierarquia.",
                    "OK");
                return;
            }

            var report = new StringBuilder();
            report.AppendLine($"=== TILEMAP VALIDATOR ===");
            report.AppendLine($"Tilemaps encontrados: {tilemaps.Length}\n");

            int totalIssues = 0;
            int totalWarnings = 0;

            foreach (var tm in tilemaps)
            {
                report.AppendLine($"▶ [{tm.name}]");

                // Verificar tiles usados
                int tileCount = tm.GetUsedTilesCount();
                report.AppendLine($"  Tiles usados: {tileCount}");

                if (tileCount == 0)
                {
                    report.AppendLine("  ❌ ERRO: Tilemap está VAZIO!");
                    totalIssues++;
                }
                else if (tileCount < MinExpectedTiles)
                {
                    report.AppendLine($"  ⚠️ AVISO: Apenas {tileCount} tiles (esperado ≥ {MinExpectedTiles})");
                    totalWarnings++;
                }
                else
                {
                    report.AppendLine($"  ✅ Contagem de tiles OK.");
                }

                // Verificar bounds
                tm.CompressBounds();
                var bounds = tm.cellBounds;
                int bw = bounds.size.x;
                int bh = bounds.size.y;
                report.AppendLine($"  Bounds: {bw}x{bh} células");

                if (bw > MaxBoundSize || bh > MaxBoundSize)
                {
                    report.AppendLine($"  ⚠️ AVISO: Bounds muito grandes ({bw}x{bh}). Pode impactar performance.");
                    totalWarnings++;
                }

                // Verificar TilemapCollider2D
                var col2D = tm.GetComponent<TilemapCollider2D>();
                if (col2D == null)
                {
                    report.AppendLine("  ⚠️ AVISO: Sem TilemapCollider2D (tiles não terão colisão).");
                    totalWarnings++;
                }
                else
                {
                    report.AppendLine($"  ✅ TilemapCollider2D presente.");
                }

                // Verificar TilemapRenderer
                var renderer = tm.GetComponent<TilemapRenderer>();
                if (renderer == null || !renderer.enabled)
                {
                    report.AppendLine("  ⚠️ AVISO: TilemapRenderer ausente ou desabilitado.");
                    totalWarnings++;
                }
                else
                {
                    report.AppendLine($"  ✅ TilemapRenderer ativo.");
                }

                // Verificar tiles nulos dentro dos bounds
                int nullTiles = 0;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    var tile = tm.GetTile(pos);
                    if (tile == null && tm.HasTile(pos))
                        nullTiles++;
                }
                if (nullTiles > 0)
                {
                    report.AppendLine($"  ❌ ERRO: {nullTiles} posições com tile nulo!");
                    totalIssues++;
                }

                report.AppendLine();
            }

            // Resumo
            report.AppendLine("=== RESUMO ===");
            report.AppendLine($"Erros: {totalIssues} | Avisos: {totalWarnings}");

            string summary = totalIssues == 0 && totalWarnings == 0
                ? "✅ Todos os tilemaps estão OK!"
                : $"⚠️ {totalIssues} erro(s), {totalWarnings} aviso(s) encontrado(s).";

            Debug.Log("[TilemapValidator]\n" + report.ToString());
            EditorUtility.DisplayDialog("Tilemap Validator — Resultado", summary + "\n\nVeja o Console para detalhes.", "OK");
        }
    }
}
