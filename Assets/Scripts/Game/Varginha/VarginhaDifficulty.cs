using UnityEngine;

namespace Game.Varginha
{
    public enum InvestigationDifficulty { Easy, Medium, Hard }

    /// <summary>Uma escolha por investigação; atravessa cenas e tentativas sem multiplicar atributos novamente.</summary>
    public static class VarginhaDifficulty
    {
        public static InvestigationDifficulty Selected { get; private set; } = InvestigationDifficulty.Medium;
        public static string Label => Selected == InvestigationDifficulty.Easy ? "FÁCIL" : Selected == InvestigationDifficulty.Hard ? "DIFÍCIL" : "MÉDIO";
        public static float EnemyHealth => Selected == InvestigationDifficulty.Easy ? 1f : Selected == InvestigationDifficulty.Hard ? 2.2f : 1.55f;
        public static float EnemySpeed => Selected == InvestigationDifficulty.Easy ? .85f : Selected == InvestigationDifficulty.Hard ? 1.4f : 1.1f;
        public static float EnemyDamage => Selected == InvestigationDifficulty.Easy ? .65f : Selected == InvestigationDifficulty.Hard ? 1.35f : 1f;
        public static float EnemyAttackInterval => Selected == InvestigationDifficulty.Easy ? 1.6f : Selected == InvestigationDifficulty.Hard ? .8f : 1.15f;
        public static float TelegraphSeconds => Selected == InvestigationDifficulty.Easy ? .85f : Selected == InvestigationDifficulty.Hard ? .38f : .6f;
        public static float ControlResistance => Selected == InvestigationDifficulty.Easy ? 1f : Selected == InvestigationDifficulty.Hard ? .45f : .7f;
        public static bool AlliedSummonsUnlocked => Selected == InvestigationDifficulty.Hard;

        /// <summary>
        /// Em fases difíceis, apenas um pequeno núcleo de alunos pode ser chamado.
        /// A fase final ignora a limitação e libera os nove resgatados.
        /// </summary>
        public static int AlliedSummonCount(int phaseNumber, bool finalPhase = false)
        {
            if (finalPhase || phaseNumber >= 3) return 9;
            return AlliedSummonsUnlocked && phaseNumber >= 2 ? 3 : 0;
        }

        public static void Select(InvestigationDifficulty difficulty) => Selected = difficulty;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => Selected = InvestigationDifficulty.Medium;
    }
}
