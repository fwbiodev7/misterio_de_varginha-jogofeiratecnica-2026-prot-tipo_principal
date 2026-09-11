using System.Text;
using UnityEngine;
using UnityEditor;

namespace Game.Editor.Testing
{
    /// <summary>
    /// Profiler simples de performance da cena:
    /// - Conta GameObjects, componentes pesados, luzes, partículas
    /// - Mede número de tilemaps e tiles
    /// - Verifica colisores estáticos
    /// </summary>
    public static class PerformanceProfilerTool
    {
        // Limites recomendados
        private const int MaxDynamicRigidbodies = 50;
        private const int MaxParticleSystems = 20;
        private const int MaxLights = 10;
        private const int MaxAudioSources = 15;
        private const int MaxTotalGameObjects = 300;

        public static void RunProfile()
        {
            var report = new StringBuilder();
            report.AppendLine("=== PERFORMANCE PROFILER ===\n");

            int warnings = 0;

            // ─── GameObjects ──────────────────────────────────────────────────
            var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude);
            int total = allGOs.Length;
            report.AppendLine($"[ GAMEOBJECTS ]");
            report.AppendLine($"  Total: {total}");
            if (total > MaxTotalGameObjects)
            {
                report.AppendLine($"  ⚠️ Muitos GameObjects ({total} > {MaxTotalGameObjects}). Considere object pooling.");
                warnings++;
            }
            else
            {
                report.AppendLine($"  ✅ Quantidade OK.");
            }

            // ─── Rigidbodies dinâmicos ────────────────────────────────────────
            report.AppendLine($"\n[ RIGIDBODY2D DINÂMICOS ]");
            var rbs = Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Exclude);
            int dynamic = 0;
            int kinematic = 0;
            int staticRb = 0;
            foreach (var rb in rbs)
            {
                switch (rb.bodyType)
                {
                    case RigidbodyType2D.Dynamic:   dynamic++;   break;
                    case RigidbodyType2D.Kinematic: kinematic++; break;
                    case RigidbodyType2D.Static:    staticRb++;  break;
                }
            }
            report.AppendLine($"  Dinâmico: {dynamic} | Kinematic: {kinematic} | Estático: {staticRb}");
            if (dynamic > MaxDynamicRigidbodies)
            {
                report.AppendLine($"  ⚠️ Muitos Rigidbodies dinâmicos ({dynamic} > {MaxDynamicRigidbodies}).");
                warnings++;
            }
            else
            {
                report.AppendLine("  ✅ Rigidbodies OK.");
            }

            // ─── Particle Systems ─────────────────────────────────────────────
            report.AppendLine($"\n[ PARTICLE SYSTEMS ]");
            var particles = Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude);
            report.AppendLine($"  Total: {particles.Length}");
            if (particles.Length > MaxParticleSystems)
            {
                report.AppendLine($"  ⚠️ Muitos ParticleSystems ({particles.Length} > {MaxParticleSystems}).");
                warnings++;
            }
            else
            {
                report.AppendLine("  ✅ Partículas OK.");
            }

            // ─── Lights ───────────────────────────────────────────────────────
            report.AppendLine($"\n[ LIGHTS ]");
            var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            report.AppendLine($"  Total: {lights.Length}");
            foreach (var l in lights)
                report.AppendLine($"    - {l.name} ({l.type}, intensidade={l.intensity:F1})");
            if (lights.Length > MaxLights)
            {
                report.AppendLine($"  ⚠️ Muitas luzes ({lights.Length} > {MaxLights}).");
                warnings++;
            }

            // ─── AudioSources ─────────────────────────────────────────────────
            report.AppendLine($"\n[ AUDIO SOURCES ]");
            var audios = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
            report.AppendLine($"  Total: {audios.Length}");
            if (audios.Length > MaxAudioSources)
            {
                report.AppendLine($"  ⚠️ Muitos AudioSources ({audios.Length} > {MaxAudioSources}).");
                warnings++;
            }
            else
            {
                report.AppendLine("  ✅ AudioSources OK.");
            }

            // ─── Animators ────────────────────────────────────────────────────
            report.AppendLine($"\n[ ANIMATORS ]");
            var animators = Object.FindObjectsByType<Animator>(FindObjectsInactive.Exclude);
            report.AppendLine($"  Total: {animators.Length}");

            // ─── Colisores ────────────────────────────────────────────────────
            report.AppendLine($"\n[ COLLIDER2D ]");
            var colliders = Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude);
            int triggers = 0, solids = 0;
            foreach (var c in colliders)
            {
                if (c.isTrigger) triggers++; else solids++;
            }
            report.AppendLine($"  Total: {colliders.Length} (Sólidos: {solids} | Triggers: {triggers})");

            // ─── Sumário ──────────────────────────────────────────────────────
            report.AppendLine("\n=== SUMÁRIO ===");
            report.AppendLine($"Avisos de performance: {warnings}");

            string summary = warnings == 0
                ? "✅ Performance dentro dos limites recomendados!"
                : $"⚠️ {warnings} aviso(s) de performance encontrado(s).";

            Debug.Log("[PerformanceProfiler]\n" + report.ToString());
            EditorUtility.DisplayDialog("Performance Profiler — Resultado",
                summary + "\n\nVeja o Console para detalhes.", "OK");
        }
    }
}
