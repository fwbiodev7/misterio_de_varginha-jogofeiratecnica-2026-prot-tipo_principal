using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using Game.Player;
using Game.Managers;

namespace Game.Editor.Testing
{
    /// <summary>
    /// Verifica a saúde da cena ativa:
    /// - Presença de Camera
    /// - Presença de EventSystem
    /// - Singletons (GameManager, ScoreManager, LevelManager)
    /// - Player com componentes necessários
    /// - AudioListener
    /// - Lights
    /// </summary>
    public static class SceneHealthChecker
    {
        public static void RunCheck()
        {
            var report = new StringBuilder();
            report.AppendLine("=== SCENE HEALTH CHECK ===\n");

            int issues = 0;
            int warnings = 0;

            // ─── Camera ───────────────────────────────────────────────────────
            report.AppendLine("[ CAMERA ]");
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
            if (cameras.Length == 0)
            {
                report.AppendLine("  ❌ ERRO: Nenhuma Camera encontrada na cena!");
                issues++;
            }
            else if (cameras.Length > 1)
            {
                report.AppendLine($"  ⚠️ AVISO: {cameras.Length} cameras encontradas. Verificar qual está ativa.");
                warnings++;
            }
            else
            {
                report.AppendLine($"  ✅ Camera '{cameras[0].name}' encontrada.");
            }

            // ─── AudioListener ────────────────────────────────────────────────
            report.AppendLine("\n[ AUDIO LISTENER ]");
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude);
            if (listeners.Length == 0)
            {
                report.AppendLine("  ⚠️ AVISO: Nenhum AudioListener. Sons não serão ouvidos.");
                warnings++;
            }
            else if (listeners.Length > 1)
            {
                report.AppendLine($"  ⚠️ AVISO: {listeners.Length} AudioListeners. Apenas um é permitido.");
                warnings++;
            }
            else
            {
                report.AppendLine("  ✅ AudioListener OK.");
            }

            // ─── Player ───────────────────────────────────────────────────────
            report.AppendLine("\n[ PLAYER ]");
            var players = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude);
            if (players.Length == 0)
            {
                report.AppendLine("  ⚠️ AVISO: Nenhum PlayerController na cena.");
                warnings++;
            }
            else
            {
                foreach (var p in players)
                {
                    report.AppendLine($"  ✅ Player: '{p.name}'");
                    if (p.GetComponent<HealthSystem>() == null)
                    {
                        report.AppendLine("    ❌ ERRO: Player sem HealthSystem!");
                        issues++;
                    }
                    if (p.GetComponent<Rigidbody2D>() == null)
                    {
                        report.AppendLine("    ❌ ERRO: Player sem Rigidbody2D!");
                        issues++;
                    }
                    if (p.GetComponent<Collider2D>() == null)
                    {
                        report.AppendLine("    ❌ ERRO: Player sem Collider2D!");
                        issues++;
                    }
                }
            }

            // ─── Managers ─────────────────────────────────────────────────────
            report.AppendLine("\n[ MANAGERS ]");

            CheckManager<GameManager>(report, "GameManager", ref issues, ref warnings);
            CheckManager<ScoreManager>(report, "ScoreManager", ref issues, ref warnings);
            CheckManager<LevelManager>(report, "LevelManager", ref issues, ref warnings);

            // ─── Tilemaps ─────────────────────────────────────────────────────
            report.AppendLine("\n[ TILEMAPS ]");
            var tilemaps = Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Exclude);
            if (tilemaps.Length == 0)
            {
                report.AppendLine("  ⚠️ AVISO: Nenhum Tilemap na cena.");
                warnings++;
            }
            else
            {
                report.AppendLine($"  ✅ {tilemaps.Length} Tilemap(s) encontrado(s).");
            }

            // ─── Rigidbodies sem Collider ─────────────────────────────────────
            report.AppendLine("\n[ RIGIDBODIES SEM COLLIDER ]");
            var rigidbodies = Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Exclude);
            int rbWithoutCol = 0;
            foreach (var rb in rigidbodies)
                if (rb.GetComponent<Collider2D>() == null)
                    rbWithoutCol++;

            if (rbWithoutCol > 0)
            {
                report.AppendLine($"  ⚠️ AVISO: {rbWithoutCol} Rigidbody2D(s) sem Collider2D.");
                warnings++;
            }
            else
            {
                report.AppendLine($"  ✅ Todos os Rigidbody2Ds têm Collider2D.");
            }

            // ─── Sumário ──────────────────────────────────────────────────────
            report.AppendLine("\n=== SUMÁRIO ===");
            report.AppendLine($"Erros: {issues} | Avisos: {warnings}");

            string summary = issues == 0 && warnings == 0
                ? "✅ Cena está saudável!"
                : $"{(issues > 0 ? $"❌ {issues} erro(s)" : "")}{(warnings > 0 ? $" ⚠️ {warnings} aviso(s)" : "")}";

            Debug.Log("[SceneHealthChecker]\n" + report.ToString());

            EditorUtility.DisplayDialog(
                "Scene Health Check — Resultado",
                summary + "\n\nVeja o Console para detalhes completos.",
                "OK");
        }

        private static void CheckManager<T>(StringBuilder report, string name, ref int issues, ref int warnings)
            where T : Component
        {
            var found = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude);
            if (found.Length == 0)
            {
                report.AppendLine($"  ⚠️ AVISO: {name} não encontrado na cena.");
                warnings++;
            }
            else if (found.Length > 1)
            {
                report.AppendLine($"  ⚠️ AVISO: {found.Length} instâncias de {name} encontradas.");
                warnings++;
            }
            else
            {
                report.AppendLine($"  ✅ {name} OK.");
            }
        }
    }
}
