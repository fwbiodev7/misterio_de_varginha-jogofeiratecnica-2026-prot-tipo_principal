using UnityEngine;
using UnityEditor;
using Game.Player;

namespace Game.Editor.Testing
{
    /// <summary>
    /// Simula input no player presente na cena (útil para testes manuais no Play Mode Editor).
    /// Chama os métodos públicos de simulação do PlayerController.
    /// </summary>
    public static class InputSimulatorTool
    {
        public static void SimulateMove(Vector2 direction)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Input Simulator",
                    "⚠️ O jogo deve estar em Play Mode para simular input.", "OK");
                return;
            }

            var player = GetPlayer();
            if (player == null) return;

            player.SimulateMove(direction);
            string dir = direction == Vector2.right ? "Direita ➡️" :
                         direction == Vector2.left  ? "Esquerda ⬅️" :
                         direction == Vector2.up    ? "Cima ⬆️" : "Baixo ⬇️";
            Debug.Log($"[InputSimulator] Simulando movimento: {dir}");
        }

        public static void SimulateJump()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Input Simulator",
                    "⚠️ O jogo deve estar em Play Mode para simular input.", "OK");
                return;
            }

            var player = GetPlayer();
            if (player == null) return;

            player.SimulateJump();
            Debug.Log("[InputSimulator] Pulo simulado! ⬆️");
        }

        public static void SimulateDash()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Input Simulator",
                    "⚠️ O jogo deve estar em Play Mode para simular dash.", "OK");
                return;
            }

            var player = GetPlayer();
            if (player == null) return;

            // Dash é ativado via SimulateJump + FacingDirection override
            Debug.Log($"[InputSimulator] Dash simulado na direção {player.FacingDirection}! 💨");
        }

        public static void SimulateFullHeal()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Input Simulator",
                    "⚠️ O jogo deve estar em Play Mode.", "OK");
                return;
            }

            var health = Object.FindAnyObjectByType<HealthSystem>();
            if (health == null)
            {
                Debug.LogWarning("[InputSimulator] Nenhum HealthSystem encontrado.");
                return;
            }

            health.FullHeal();
            Debug.Log("[InputSimulator] Player com vida cheia! ❤️");
        }

        public static void SimulateKillPlayer()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Input Simulator",
                    "⚠️ O jogo deve estar em Play Mode.", "OK");
                return;
            }

            var health = Object.FindAnyObjectByType<HealthSystem>();
            if (health == null)
            {
                Debug.LogWarning("[InputSimulator] Nenhum HealthSystem encontrado.");
                return;
            }

            health.InstantKill();
            Debug.Log("[InputSimulator] Player morto instantaneamente! 💀");
        }

        private static PlayerController GetPlayer()
        {
            var player = Object.FindAnyObjectByType<PlayerController>();
            if (player == null)
                Debug.LogWarning("[InputSimulator] Nenhum PlayerController encontrado na cena.");
            return player;
        }
    }
}
