using System.Collections;
using System.Collections.Generic;
using Game.Player;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Estilos de ataque dos alunos aliados nas fases posteriores.</summary>
    public enum VarginhaStudentAllyStyle
    {
        JiuJitsu,
        PingPong,
        Guitar,
        Art,
        Microphone,
        Katana,
        FallingPiano,
        Support,
        // Adicionado ao final para preservar os valores já serializados nas cenas.
        Volleyball,
        MarcosChute,
        MarcosCotovelo
    }

    /// <summary>
    /// Aliado simples e reutilizável. A fase que quiser usar a turma só precisa
    /// ativar o esquadrão; cada nome recebe automaticamente seu golpe.
    /// </summary>
    public sealed class VarginhaStudentAlly : MonoBehaviour
    {
        public const float ManualCooldownSeconds = 5f;

        private struct AttackProfile
        {
            public VarginhaStudentAllyStyle Style;
            public string Description;
            public float Damage;
            public float Range;
            public float Cooldown;
            public float Windup;

            public AttackProfile(VarginhaStudentAllyStyle style, string description, float damage, float range, float cooldown, float windup)
            {
                Style = style;
                Description = description;
                Damage = damage;
                Range = range;
                Cooldown = cooldown;
                Windup = windup;
            }
        }

        [SerializeField] private string studentName = "Aluno";
        [SerializeField] private VarginhaStudentAllyStyle attackStyle = VarginhaStudentAllyStyle.Support;
        [SerializeField] private float followSpeed = 2.8f;

        private Transform _leader;
        private VarginhaStudentAllySquad _squad;
        private Vector3 _formationOffset;
        private SpriteRenderer _renderer;
        private SpriteRenderer _headRenderer;
        private Color _shirtColor = new Color(.25f, .52f, .88f);
        private AttackProfile _profile;
        private float _cooldownTimer;
        private bool _active;
        private bool _attacking;
        private bool _manualMode;
        private bool _manualPresentation;
        private bool _equippedPresentation;
        public bool IsEquippedPresentation => _equippedPresentation;

        public void SetEquippedPresentation(bool equipped)
        {
            if (equipped && !_equippedPresentation && _manualMode && _leader != null)
                transform.position = ResolveFreeDestination(_leader.position + new Vector3(-.7f, -.5f, 0));
            _equippedPresentation = equipped;
            if (_manualMode) SetManualPresentation(true);
        }
        private int _marcosAttackIndex;
        private float _manualCooldownSeconds = ManualCooldownSeconds;
        private GUIStyle _nameStyle;

        public string StudentName => studentName;
        public string AttackDescription => _profile.Description;
        public VarginhaStudentAllyStyle AttackStyle => attackStyle;
        public bool IsActive => _active;
        public bool IsManualMode => _manualMode;
        public bool IsReadyForManualAttack => _active && _manualMode && !_attacking && _cooldownTimer <= 0f;
        public float ManualCooldownRemaining => Mathf.Max(0f, _cooldownTimer);
        public float ManualCooldownDuration => _manualCooldownSeconds;
        public float SecondaryRadius => attackStyle == VarginhaStudentAllyStyle.FallingPiano ? 2.3f
            : attackStyle == VarginhaStudentAllyStyle.Microphone ? 2.2f
            : attackStyle == VarginhaStudentAllyStyle.Art ? 1.8f
            : attackStyle == VarginhaStudentAllyStyle.PingPong ? 2.5f
            : attackStyle == VarginhaStudentAllyStyle.MarcosChute ? 1.75f
            : attackStyle == VarginhaStudentAllyStyle.MarcosCotovelo ? 1.35f : 1.6f;
        public VarginhaCombatTarget CurrentTarget { get; private set; }
        public bool CanCommand => _active && isActiveAndEnabled && _leader != null && Time.timeScale > 0f
            && _leader.GetComponent<EdelzioTopDownController>()?.IsInputLocked != true
            && _leader.GetComponent<EdelzioTopDownController>()?.IsScriptedMotion != true
            && !VarginhaTravelCinematic.IsTravelling
            && VarginhaGameHUD.Instance?.IsDialogueOpen != true;

        // Invocações são locais e respeitam paredes; a turma não acerta ETs do outro lado do mapa.
        public bool CanReachTarget(VarginhaCombatTarget target)
        {
            if (!CanCommand || target == null || target.IsDead || !target.isActiveAndEnabled
                || target.Kind == VarginhaCombatTarget.EnemyKind.AncestralEntity) return false;
            Vector2 origin = _leader.position;
            if (Vector2.Distance(origin, target.transform.position) > 7f) return false;
            return HasClearPath(origin, target.transform.position);
        }

        // Verifica também o trecho entre ETs: ricochetes e explosões não atravessam quinas.
        public bool CanChainToTarget(Vector2 origin, VarginhaCombatTarget target)
        {
            return CanReachTarget(target) && HasClearPath(origin, target.transform.position);
        }

        private bool HasClearPath(Vector2 origin, Vector2 destination)
        {
            foreach (var hit in Physics2D.LinecastAll(origin, destination))
            {
                if (hit.collider.isTrigger || hit.transform == _leader || hit.transform.IsChildOf(_leader)
                    || hit.collider.GetComponentInParent<VarginhaCombatTarget>() != null
                    || hit.collider.GetComponentInParent<VarginhaStudentAlly>() != null) continue;
                return false;
            }
            return true;
        }

        public void Configure(string name, Color shirtColor, int index)
        {
            studentName = name;
            _shirtColor = shirtColor;
            _profile = ProfileFor(name);
            attackStyle = _profile.Style;
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null) _renderer = gameObject.AddComponent<SpriteRenderer>();
            if (_renderer != null)
            {
                _renderer.sprite = VarginhaPixelArtSprites.Create("Student_" + name, shirtColor);
                _renderer.sortingOrder = 7 + Mathf.Clamp(index, 0, 8);
                _renderer.enabled = true;
            }
            if (_headRenderer != null)
            {
                _headRenderer.sprite = VarginhaPixelArtSprites.Create("StudentHead_" + name, shirtColor);
                _headRenderer.sortingOrder = 8 + Mathf.Clamp(index, 0, 8);
            }
            var animation = GetComponent<VarginhaStudentAnimation>();
            if (animation == null) animation = gameObject.AddComponent<VarginhaStudentAnimation>();
            animation.Configure(studentName);
        }

        public void Activate(Transform leader, int index)
        {
            StopAllCoroutines();
            GetComponent<VarginhaAllyAttackPresentation>()?.Cancel();
            _attacking = false;
            CurrentTarget = null;
            _leader = leader;
            _squad = GetComponentInParent<VarginhaStudentAllySquad>();
            _active = true;
            _manualMode = false;
            _manualPresentation = false;
            _cooldownTimer = 0f;
            if (studentName == "Marcos")
            {
                _marcosAttackIndex = 0;
                _profile = ProfileFor(studentName);
                attackStyle = _profile.Style;
            }
            int column = index % 3;
            int row = index / 3;
            _formationOffset = new Vector3(-1.45f - column * .90f, (1 - row) * .95f, 0f);
            if (_renderer != null) _renderer.enabled = true;
            if (_headRenderer != null) _headRenderer.enabled = false;
        }

        /// <summary>Ativa o aliado para a Fase 3, onde cada clique dispara apenas um golpe.</summary>
        public void ActivateManual(Transform leader, int index, float cooldownSeconds = ManualCooldownSeconds)
        {
            Activate(leader, index);
            _manualMode = true;
            _manualCooldownSeconds = Mathf.Max(.1f, cooldownSeconds);
            EnsureHeadRenderer();
            SetManualPresentation(true);
        }

        public void Deactivate()
        {
            _active = false;
            _attacking = false;
            _manualMode = false;
            _manualPresentation = false;
            CurrentTarget = null;
            StopAllCoroutines();
            GetComponent<VarginhaAllyAttackPresentation>()?.Cancel();
            if (_renderer != null) _renderer.enabled = false;
            if (_headRenderer != null) _headRenderer.enabled = false;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _profile = ProfileFor(studentName);
            attackStyle = _profile.Style;
        }

        private void Update()
        {
            if (!_active) return;
            if (_leader != null && (!_manualMode || _equippedPresentation))
            {
                // A formação é uma preferência, não um trilho. O pequeno movimento
                // de circulação e a separação entre colegas deixam a turma explorar
                // o espaço sem abandonar Edelzio nem empilhar sprites.
                float phase = Time.time * (.62f + (Mathf.Abs(studentName.GetHashCode()) % 7) * .035f);
                Vector3 freeMotion = new Vector3(Mathf.Sin(phase), Mathf.Cos(phase * 1.17f), 0f) * .30f;
                Vector3 separation = Vector3.zero;
                var squadMembers = _squad?.Allies;
                if (squadMembers != null) for (int i = 0; i < squadMembers.Count; i++)
                {
                    var other = squadMembers[i];
                    if (other == null || other == this || !other.IsActive || (other.IsManualMode && !other.IsEquippedPresentation)) continue;
                    Vector3 delta = transform.position - other.transform.position;
                    float otherDistance = delta.magnitude;
                    if (otherDistance > .01f && otherDistance < .82f) separation += delta.normalized * (.82f - otherDistance);
                }
                Vector3 offset = _manualMode ? new Vector3(-.7f, -.5f, 0) : _formationOffset;
                Vector3 destination = _leader.position + offset + freeMotion + separation * .75f;
                destination = ResolveFreeDestination(destination);
                Vector3 prevPos = transform.position;
                float distance = Vector3.Distance(transform.position, destination);
                transform.position = Vector3.MoveTowards(transform.position, destination,
                    Mathf.Max(followSpeed, distance * 3f) * Time.deltaTime);

                // The directional atlas supplies the walk; fixed scale keeps every pixel crisp.
                transform.localScale = Vector3.one;
            }

            if (!CanCommand) return;
            _cooldownTimer -= Time.deltaTime;
            if (_manualMode || _attacking || _cooldownTimer > 0f) return;
            var target = FindNearestTargetForNextAttack();
            if (target != null) StartCoroutine(AttackRoutine(target));
        }

        private void EnsureHeadRenderer()
        {
            if (_headRenderer != null) return;
            var headObject = new GameObject("Cabeca_Aliado_" + studentName.Replace(" ", "_"));
            headObject.transform.SetParent(transform, false);
            headObject.transform.localPosition = new Vector3(0f, .12f, 0f);
            headObject.transform.localScale = Vector3.one * .72f;
            _headRenderer = headObject.AddComponent<SpriteRenderer>();
            _headRenderer.sprite = VarginhaPixelArtSprites.Create("StudentHead_" + studentName, _shirtColor);
            if (_renderer != null) _headRenderer.sortingLayerID = _renderer.sortingLayerID;
            _headRenderer.sortingOrder = _renderer != null ? _renderer.sortingOrder + 1 : 10;
        }

        private void SetManualPresentation(bool manual)
        {
            _manualPresentation = manual;
            if (manual) EnsureHeadRenderer();
            if (_renderer != null) _renderer.enabled = _active && (!manual || (_equippedPresentation && !_attacking));
            if (_headRenderer != null) _headRenderer.enabled = false;
        }

        private void OnGUI()
        {
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            if (!_manualPresentation || !_active || !_equippedPresentation || _renderer == null || !_renderer.enabled
                || VarginhaGameHUD.Instance?.IsInventoryOpen == true) return;
            var camera = Camera.main;
            if (camera == null) return;
            Vector3 screen = camera.WorldToScreenPoint(transform.position + Vector3.up * .67f);
            if (screen.z <= 0f) return;
            screen.y = Screen.height - screen.y;
            if (_nameStyle == null)
            {
                _nameStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10,
                    fontStyle = FontStyle.Bold,
                    wordWrap = false
                };
            }
            // O estado de prontidão fica somente no HUD lateral. No cenário o
            // rótulo é um nome curto e único, evitando a pilha "nome + PRONTO"
            // sobre os sprites dos aliados.
            string displayName = studentName == "Luis Miguel Messias" ? "Luis Miguel" : studentName;
            _nameStyle.fontSize = Screen.width < 520 ? 9 : 10;
            GUI.color = new Color(.55f, 1f, .72f);
            GUI.Label(new Rect(screen.x - 54f, screen.y - 10f, 108f, 18f), displayName, _nameStyle);
            GUI.color = Color.white;
        }

        private VarginhaCombatTarget FindNearestTarget(bool ignoreRange = false)
        {
            VarginhaCombatTarget best = null;
            float bestDistance = ignoreRange ? float.MaxValue : _profile.Range;
            foreach (var target in Object.FindObjectsByType<VarginhaCombatTarget>(FindObjectsInactive.Exclude))
            {
                if (!CanReachTarget(target)) continue;
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance <= bestDistance)
                {
                    best = target;
                    bestDistance = distance;
                }
            }
            return best;
        }

        /// <summary>Encontra um ET para uma invocação manual, mesmo que esteja além do alcance corpo a corpo.</summary>
        public VarginhaCombatTarget FindNearestTargetForManual()
        {
            return FindNearestTarget(true);
        }

        /// <summary>Executa um único golpe manual e inicia o cooldown individual do aluno.</summary>
        public bool TryManualAttack(VarginhaCombatTarget target)
        {
            if (!IsReadyForManualAttack || target == null)
                return false;
            if (!PrepareAttackProfileForTarget(target)) return false;
            StartCoroutine(AttackRoutine(target));
            return true;
        }

        private IEnumerator AttackRoutine(VarginhaCombatTarget target)
        {
            _attacking = true;
            if (_manualMode) SetManualPresentation(true);
            CurrentTarget = target;
            _cooldownTimer = _manualMode ? _manualCooldownSeconds : _profile.Cooldown;
            var presentation = GetComponent<VarginhaAllyAttackPresentation>() ?? gameObject.AddComponent<VarginhaAllyAttackPresentation>();
            Vector3 impactPoint = target.transform.position;
            Vector2 direction = (impactPoint - (_leader != null ? _leader.position : transform.position)).normalized;
            yield return presentation.Play(studentName, attackStyle, _shirtColor, transform.position, impactPoint,
                () => CanCommand,
                () => {
                    // O dano acompanha o contato visual. O ET pode sair da área durante a preparação.
                    if (CanReachTarget(target) && Vector2.Distance(target.transform.position, impactPoint) <= 1.25f)
                        ApplyTacticalHit(target, direction, impactPoint, presentation);
                }, SecondaryRadius);

            _attacking = false;
            CurrentTarget = null;
            if (_manualMode) SetManualPresentation(true);
        }

        private void ApplyTacticalHit(VarginhaCombatTarget target, Vector2 direction, Vector2 impactPoint,
            VarginhaAllyAttackPresentation presentation)
        {
            float damage = _profile.Damage;
            var health = target.GetComponent<HealthSystem>();
            if (attackStyle == VarginhaStudentAllyStyle.Katana && health != null && health.HealthPercent <= .35f)
                damage += 14f;
            if (!target.ReceiveHit(damage, direction, .055f)) return;
            if (studentName == "Marcos")
                presentation.ShowBanter("Marcos", MarcosBanter(attackStyle), impactPoint);
            float stun = ControlDuration();
            target.GetComponent<VarginhaCombatEnemy>()?.ApplyAllyControl(direction, stun);
            float encouragement = attackStyle == VarginhaStudentAllyStyle.Microphone ? 12f
                : attackStyle == VarginhaStudentAllyStyle.Guitar ? 6f
                : attackStyle == VarginhaStudentAllyStyle.Support ? 8f : 0f;
            if (encouragement > 0f)
            {
                _leader.GetComponent<EdelzioTopDownController>()?.RestoreSanity(encouragement);
                presentation.ShowEncouragement(_leader.position, encouragement);
            }

            if (attackStyle == VarginhaStudentAllyStyle.PingPong)
            {
                StartCoroutine(RicochetRoutine(target, impactPoint, presentation));
                return;
            }
            if (attackStyle == VarginhaStudentAllyStyle.Support) return;
            var nearby = new List<VarginhaCombatTarget>();
            foreach (var other in Object.FindObjectsByType<VarginhaCombatTarget>(FindObjectsInactive.Exclude))
            {
                if (other == target || !CanChainToTarget(impactPoint, other)) continue;
                Vector2 delta = (Vector2)other.transform.position - impactPoint;
                if (attackStyle == VarginhaStudentAllyStyle.Katana)
                {
                    // O corte continua atrás do alvo, em um corredor estreito e legível.
                    float forward = Vector2.Dot(delta, direction);
                    float sideways = Mathf.Abs(delta.x * direction.y - delta.y * direction.x);
                    if (forward <= 0f || forward > 2.5f || sideways > .65f) continue;
                }
                else if (delta.sqrMagnitude > SecondaryRadius * SecondaryRadius) continue;
                nearby.Add(other);
            }
            nearby.Sort((a, b) => ((Vector2)a.transform.position - impactPoint).sqrMagnitude
                .CompareTo(((Vector2)b.transform.position - impactPoint).sqrMagnitude));
            int limit = attackStyle == VarginhaStudentAllyStyle.JiuJitsu ? 1 : 3;
            float multiplier = attackStyle == VarginhaStudentAllyStyle.Art ? .4f
                : attackStyle == VarginhaStudentAllyStyle.Katana ? .75f : .6f;
            for (int i = 0; i < nearby.Count && i < limit; i++)
            {
                var other = nearby[i];
                Vector2 push = ((Vector2)other.transform.position - impactPoint).normalized;
                if (other.ReceiveHit(_profile.Damage * multiplier, push, .055f))
                {
                    other.GetComponent<VarginhaCombatEnemy>()?.ApplyAllyControl(push, stun);
                    presentation.ShowSecondaryImpact(impactPoint, other.transform.position, attackStyle, _shirtColor);
                }
            }
        }

        private float ControlDuration()
        {
            switch (attackStyle)
            {
                case VarginhaStudentAllyStyle.JiuJitsu: return 1.5f;
                case VarginhaStudentAllyStyle.Art: return 1.6f;
                case VarginhaStudentAllyStyle.FallingPiano: return 1f;
                case VarginhaStudentAllyStyle.Microphone: return .9f;
                case VarginhaStudentAllyStyle.Volleyball: return .75f;
                case VarginhaStudentAllyStyle.MarcosChute: return .65f;
                case VarginhaStudentAllyStyle.MarcosCotovelo: return .9f;
                case VarginhaStudentAllyStyle.Guitar: return .6f;
                default: return .25f;
            }
        }

        private IEnumerator RicochetRoutine(VarginhaCombatTarget first, Vector2 origin,
            VarginhaAllyAttackPresentation presentation)
        {
            var struck = new HashSet<VarginhaCombatTarget> { first };
            int bounceCount = studentName == "Anna Sabia" ? 2 : 1;
            for (int bounce = 0; bounce < bounceCount; bounce++)
            {
                VarginhaCombatTarget next = null;
                float closest = SecondaryRadius;
                foreach (var candidate in Object.FindObjectsByType<VarginhaCombatTarget>(FindObjectsInactive.Exclude))
                {
                    if (struck.Contains(candidate) || !CanChainToTarget(origin, candidate)) continue;
                    float distance = Vector2.Distance(origin, candidate.transform.position);
                    if (distance > closest) continue;
                    closest = distance;
                    next = candidate;
                }
                if (next == null) yield break;
                Vector2 destination = next.transform.position;
                yield return presentation.PlayRicochet(origin, destination, () => CanCommand);
                if (!presentation.IsPresenting || !CanChainToTarget(origin, next)
                    || Vector2.Distance(next.transform.position, destination) > .8f)
                    yield break;
                Vector2 push = (destination - origin).normalized;
                float multiplier = studentName == "Anna Sabia" ? Mathf.Pow(.75f, bounce + 1) : .95f;
                if (next.ReceiveHit(_profile.Damage * multiplier, push, .045f))
                {
                    next.GetComponent<VarginhaCombatEnemy>()?.ApplyAllyControl(push, .35f);
                    presentation.ShowSecondaryImpact(origin, destination, attackStyle, _shirtColor);
                }
                struck.Add(next);
                origin = destination;
            }
        }

        public static bool HasProfile(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name != "Aluno";
        }

        public static string DescribeAttack(string name)
        {
            return ProfileFor(name).Description;
        }

        private void PrepareAttackProfile()
        {
            if (studentName != "Marcos") return;
            int variant = _marcosAttackIndex % 3;
            _marcosAttackIndex = (_marcosAttackIndex + 1) % 3;
            switch (variant)
            {
                case 0: _profile = new AttackProfile(VarginhaStudentAllyStyle.Volleyball,
                    "bolada de vôlei: cortada e onda de impacto", 34f, 4.8f, 3.5f, .2f); break;
                case 1: _profile = new AttackProfile(VarginhaStudentAllyStyle.MarcosChute,
                    "chute voador: rasteira em área", 30f, 4.8f, 3.5f, .18f); break;
                default: _profile = new AttackProfile(VarginhaStudentAllyStyle.MarcosCotovelo,
                    "cotovelada: pancada curta e atordoante", 38f, 4.8f, 3.5f, .22f); break;
            }
            attackStyle = _profile.Style;
        }

        private VarginhaCombatTarget FindNearestTargetForNextAttack()
        {
            AttackProfile previousProfile = _profile;
            VarginhaStudentAllyStyle previousStyle = attackStyle;
            int previousIndex = _marcosAttackIndex;
            if (studentName == "Marcos") PrepareAttackProfile();
            var target = FindNearestTarget();
            if (target == null)
            {
                _profile = previousProfile;
                attackStyle = previousStyle;
                _marcosAttackIndex = previousIndex;
            }
            return target;
        }

        private bool PrepareAttackProfileForTarget(VarginhaCombatTarget target)
        {
            AttackProfile previousProfile = _profile;
            VarginhaStudentAllyStyle previousStyle = attackStyle;
            int previousIndex = _marcosAttackIndex;
            if (studentName == "Marcos") PrepareAttackProfile();
            if (CanReachTarget(target)) return true;
            _profile = previousProfile;
            attackStyle = previousStyle;
            _marcosAttackIndex = previousIndex;
            return false;
        }

        private Vector3 ResolveFreeDestination(Vector3 destination)
        {
            Vector2 origin = transform.position;
            Vector2 delta = (Vector2)destination - origin;
            float distance = delta.magnitude;
            if (distance < .05f) return destination;
            Vector2 direction = delta / distance;
            foreach (var hit in Physics2D.CircleCastAll(origin, .20f, direction, distance, Physics2D.AllLayers))
            {
                if (!IsSolidForAllyMovement(hit.collider)) continue;
                Vector2 side = new Vector2(-direction.y, direction.x);
                Vector2 candidate = origin + side * Mathf.Min(distance, .72f);
                if (!HasSolidBetween(origin, candidate)) return candidate;
                candidate = origin - side * Mathf.Min(distance, .72f);
                if (!HasSolidBetween(origin, candidate)) return candidate;
                return transform.position;
            }
            return destination;
        }

        private bool HasSolidBetween(Vector2 origin, Vector2 destination)
        {
            foreach (var hit in Physics2D.LinecastAll(origin, destination))
                if (IsSolidForAllyMovement(hit.collider)) return true;
            return false;
        }

        private bool IsSolidForAllyMovement(Collider2D collider)
        {
            if (collider == null || collider.isTrigger) return false;
            if (collider.transform == transform || collider.transform.IsChildOf(transform)) return false;
            if (_leader != null && (collider.transform == _leader || collider.transform.IsChildOf(_leader))) return false;
            if (collider.GetComponentInParent<VarginhaStudentAlly>() != null) return false;
            if (collider.GetComponentInParent<VarginhaCombatTarget>() != null) return false;
            return true;
        }

        private static string MarcosBanter(VarginhaStudentAllyStyle style)
        {
            switch (style)
            {
                case VarginhaStudentAllyStyle.Volleyball: return "to doido com vc então uai!";
                case VarginhaStudentAllyStyle.MarcosChute: return "o Exu!!";
                case VarginhaStudentAllyStyle.MarcosCotovelo: return "o cu!!!";
                default: return "agora aguenta, sô!";
            }
        }

        private static AttackProfile ProfileFor(string name)
        {
            switch (name)
            {
                case "Matias": return new AttackProfile(VarginhaStudentAllyStyle.JiuJitsu, "jiujitsu: arremesso que derruba dois ETs", 30f, 1.3f, 2.8f, .16f);
                case "Anna Sabia": return new AttackProfile(VarginhaStudentAllyStyle.PingPong, "ping-pong: topspin com dois ricochetes", 23f, 4f, 2.7f, .13f);
                case "Ana Tavares": return new AttackProfile(VarginhaStudentAllyStyle.PingPong, "ping-pong: smash duplo de alta potência", 28f, 4f, 3f, .13f);
                case "Pedro": return new AttackProfile(VarginhaStudentAllyStyle.Guitar, "guitarra: acorde em área e +6 de sanidade", 31f, 2.8f, 3.5f, .18f);
                case "Luis Martins": return new AttackProfile(VarginhaStudentAllyStyle.Art, "arte: tinta em área imobiliza os ETs", 24f, 3.5f, 3.4f, .15f);
                case "Luis Miguel Messias": return new AttackProfile(VarginhaStudentAllyStyle.Microphone, "microfone: onda sônica e +12 de sanidade", 26f, 3.8f, 4f, .14f);
                case "Yasmin": return new AttackProfile(VarginhaStudentAllyStyle.FallingPiano, "piano caindo na cabeça: impacto em área", 45f, 5f, 4.5f, .35f);
                case "Fabio": return new AttackProfile(VarginhaStudentAllyStyle.Katana, "katana: corte perfurante e finalização", 38f, 2f, 3.2f, .16f);
                case "Marcos": return new AttackProfile(VarginhaStudentAllyStyle.Volleyball, "bolada de vôlei: cortada e onda de impacto", 34f, 4.8f, 3.5f, .2f);
                default: return new AttackProfile(VarginhaStudentAllyStyle.Support, "golpe de apoio", 16f, 1.10f, 1.05f, .18f);
            }
        }
    }
}
