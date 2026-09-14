using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Ponto de entrada das fases futuras. Cria e ativa a turma resgatada como aliados
    /// sem alterar o fluxo da Fase 2, que termina no Fusca.
    /// </summary>
    public sealed class VarginhaStudentAllySquad : MonoBehaviour
    {
        private static readonly Color[] ShirtColors =
        {
            new(.25f, .52f, .88f), new(.88f, .36f, .30f), new(.28f, .68f, .45f),
            new(.78f, .44f, .18f), new(.58f, .34f, .82f), new(.85f, .52f, .68f),
            new(.22f, .66f, .70f), new(.78f, .68f, .22f), new(.45f, .52f, .58f)
        };

        [SerializeField] private Transform leader;
        [SerializeField] private bool activateOnStart;
        private readonly List<VarginhaStudentAlly> _allies = new();
        private int _nextInvocationIndex;
        private float _commandReadyAt;
        public float CommandCooldownRemaining => Mathf.Max(0f, _commandReadyAt - Time.time);
        private string _lastInvokedStudentName;
        private string _lastInvokedAttackDescription;

        public IReadOnlyList<VarginhaStudentAlly> Allies => _allies;
        public string LastInvokedStudentName => _lastInvokedStudentName;
        public string LastInvokedAttackDescription => _lastInvokedAttackDescription;
        public int ActiveCount
        {
            get
            {
                int count = 0;
                foreach (var ally in _allies) if (ally != null && ally.IsActive) count++;
                return count;
            }
        }

        private void Start()
        {
            if (activateOnStart) ActivateAllies();
        }

        /// <summary>Configura o líder da fase e prepara os nove alunos.</summary>
        public void Setup(Transform newLeader, bool activateImmediately = false)
        {
            leader = newLeader;
            EnsureRoster();
            if (activateImmediately) ActivateAllies();
        }

        /// <summary>Ativa a turma como esquadrão aliado na fase atual.</summary>
        public void ActivateAllies()
        {
            EnsureRoster();
            if (leader == null) leader = Object.FindAnyObjectByType<EdelzioTopDownController>()?.transform;
            for (int i = 0; i < _allies.Count; i++)
                _allies[i].Activate(leader, i);
        }

        public void DeactivateAllies()
        {
            _commandReadyAt = 0f;
            _lastInvokedStudentName = null;
            _lastInvokedAttackDescription = null;
            foreach (var ally in _allies)
                if (ally != null) ally.Deactivate();
        }

        /// <summary>Ativa a turma no modo manual da Fase 3: um aluno por clique.</summary>
        public void ActivateManualAllies(float cooldownSeconds = VarginhaStudentAlly.ManualCooldownSeconds)
        {
            // Mantém a API histórica liberando a turma inteira; controladores de
            // fases não finais usam ActivateForPhase para aplicar a dificuldade.
            ActivateManualAllies(cooldownSeconds, true, 3);
        }

        public void ActivateManualAllies(float cooldownSeconds, bool finalPhase)
        {
            ActivateManualAllies(cooldownSeconds, finalPhase, 2);
        }

        private void ActivateManualAllies(float cooldownSeconds, bool finalPhase, int phaseNumber)
        {
            EnsureRoster();
            if (leader == null) leader = Object.FindAnyObjectByType<EdelzioTopDownController>()?.transform;
            _nextInvocationIndex = 0;
            _commandReadyAt = 0f;
            _lastInvokedStudentName = null;
            _lastInvokedAttackDescription = null;
            for (int i = 0; i < _allies.Count; i++)
            {
                int allowed = finalPhase ? _allies.Count : VarginhaDifficulty.AlliedSummonCount(phaseNumber);
                if (i < allowed) _allies[i].ActivateManual(leader, i, cooldownSeconds);
                else _allies[i].Deactivate();
            }
        }

        /// <summary>Ativa a quantidade permitida pela dificuldade para uma fase comum.</summary>
        public void ActivateForPhase(int phaseNumber, float cooldownSeconds = VarginhaStudentAlly.ManualCooldownSeconds)
        {
            ActivateManualAllies(cooldownSeconds, phaseNumber >= 3, phaseNumber);
        }

        /// <summary>Invoca o próximo aluno pronto e manda um único golpe ao ET mais próximo.</summary>
        public bool TryInvokeNextAttack()
        {
            return TryInvokeAttack(null);
        }

        /// <summary>A mira escolhe o ET; sem alvo na mira, protege Edelzio e evita golpes duplicados.</summary>
        public bool TryInvokeAttack(Vector2? aim)
        {
            if (_allies.Count == 0 || CommandCooldownRemaining > 0f) return false;
            var targets = Object.FindObjectsByType<VarginhaCombatTarget>(FindObjectsInactive.Exclude);
            VarginhaStudentAlly selected = null;
            VarginhaCombatTarget selectedTarget = null;
            float bestScore = float.NegativeInfinity;
            int selectedIndex = 0;
            for (int offset = 0; offset < _allies.Count; offset++)
            {
                int index = (_nextInvocationIndex + offset) % _allies.Count;
                var ally = _allies[index];
                if (ally == null || !ally.IsReadyForManualAttack) continue;
                foreach (var target in targets)
                {
                    if (!ally.CanReachTarget(target)) continue;
                    if (target.GetComponent<Game.Player.HealthSystem>()?.IsInvincible == true) continue;
                    float distance = Vector2.Distance(leader.position, target.transform.position);
                    float score = 10f - distance - offset * .01f;
                    bool aimed = aim.HasValue && Vector2.Distance(aim.Value, target.transform.position) < 1.25f;
                    if (aimed) score += 100f;
                    foreach (var member in _allies)
                        if (member != null && member.CurrentTarget == target) score -= 12f;
                    int neighbors = 0;
                    foreach (var other in targets)
                        if (other != target && ally.CanChainToTarget(target.transform.position, other)
                            && Vector2.Distance(other.transform.position, target.transform.position) < ally.SecondaryRadius) neighbors++;
                    switch (ally.AttackStyle)
                    {
                        case VarginhaStudentAllyStyle.JiuJitsu: if (distance < 2.5f) score += 5f; break;
                        case VarginhaStudentAllyStyle.FallingPiano:
                            score += Mathf.Min(neighbors, 3) * 3f; break;
                        case VarginhaStudentAllyStyle.Art:
                            score += Mathf.Min(neighbors, 3) * 2.5f; break;
                        case VarginhaStudentAllyStyle.Volleyball:
                            score += Mathf.Min(neighbors, 2) * 2f + (distance > 3f ? 4f : 1f); break;
                        case VarginhaStudentAllyStyle.PingPong:
                            score += Mathf.Min(neighbors, ally.StudentName == "Anna Sabia" ? 2 : 1) * 3f; break;
                        case VarginhaStudentAllyStyle.Guitar:
                        case VarginhaStudentAllyStyle.Microphone:
                        case VarginhaStudentAllyStyle.Support:
                            score += Mathf.Min(neighbors, 3) * 2f;
                            var player = leader.GetComponent<EdelzioTopDownController>();
                            if (player != null && player.CurrentSanity < player.MaxSanity * .65f)
                                score += ally.AttackStyle == VarginhaStudentAllyStyle.Microphone ? 10f : 6f;
                            break;
                        case VarginhaStudentAllyStyle.Katana:
                            var health = target.GetComponent<Game.Player.HealthSystem>();
                            if (health != null && (health.CurrentHealth <= 38f || health.HealthPercent <= .35f)) score += 6f;
                            break;
                    }
                    if (score <= bestScore) continue;
                    bestScore = score;
                    selected = ally;
                    selectedTarget = target;
                    selectedIndex = index;
                }
            }
            if (selected == null || !selected.TryManualAttack(selectedTarget)) return false;
            _commandReadyAt = Time.time + .9f;
            _nextInvocationIndex = (selectedIndex + 1) % _allies.Count;
            _lastInvokedStudentName = selected.StudentName;
            _lastInvokedAttackDescription = selected.AttackDescription;
            return true;
        }

        /// <summary>
        /// Atalho para uma cena futura: cria o objeto da turma sob o pai indicado.
        /// </summary>
        public static VarginhaStudentAllySquad BuildForFuturePhase(Transform parent, Transform leader, bool activateImmediately = true)
        {
            var squadObject = new GameObject("Aliados_Alunos_3_Sistema");
            if (parent != null) squadObject.transform.SetParent(parent);
            var squad = squadObject.AddComponent<VarginhaStudentAllySquad>();
            squad.Setup(leader, activateImmediately);
            return squad;
        }

        private void EnsureRoster()
        {
            _allies.Clear();
            var existing = GetComponentsInChildren<VarginhaStudentAlly>(true);
            var byName = new Dictionary<string, VarginhaStudentAlly>();
            foreach (var ally in existing)
            {
                if (ally == null || byName.ContainsKey(ally.StudentName)) continue;
                byName.Add(ally.StudentName, ally);
            }

            for (int i = 0; i < VarginhaPhase2Controller.StudentNames.Length; i++)
            {
                string name = VarginhaPhase2Controller.StudentNames[i];
                if (!byName.TryGetValue(name, out var ally))
                {
                    var allyObject = new GameObject("Aliado_" + name.Replace(" ", "_"));
                    allyObject.transform.SetParent(transform);
                    allyObject.transform.position = leader != null ? leader.position : Vector3.zero;
                    var renderer = allyObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = VarginhaPixelArtSprites.Create("Student_" + name, ShirtColors[i]);
                    ally = allyObject.AddComponent<VarginhaStudentAlly>();
                    ally.Configure(name, ShirtColors[i], i);
                }
                _allies.Add(ally);
            }
        }
    }
}
