using System.Collections;
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
        Support
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
        private GUIStyle _nameStyle;

        public string StudentName => studentName;
        public string AttackDescription => _profile.Description;
        public VarginhaStudentAllyStyle AttackStyle => attackStyle;
        public bool IsActive => _active;
        public bool IsManualMode => _manualMode;
        public bool IsReadyForManualAttack => _active && _manualMode && !_attacking && _cooldownTimer <= 0f;
        public float ManualCooldownRemaining => Mathf.Max(0f, _cooldownTimer);
        public VarginhaCombatTarget CurrentTarget { get; private set; }
        public bool CanCommand => _active && _leader != null && Time.timeScale > 0f
            && _leader.GetComponent<EdelzioTopDownController>()?.IsInputLocked != true
            && VarginhaGameHUD.Instance?.IsDialogueOpen != true;

        // Invocações são locais e respeitam paredes; a turma não acerta ETs do outro lado do mapa.
        public bool CanReachTarget(VarginhaCombatTarget target)
        {
            if (!CanCommand || target == null || target.IsDead || !target.isActiveAndEnabled
                || target.Kind == VarginhaCombatTarget.EnemyKind.AncestralEntity) return false;
            Vector2 origin = _leader.position;
            if (Vector2.Distance(origin, target.transform.position) > 7f) return false;
            foreach (var hit in Physics2D.LinecastAll(origin, target.transform.position))
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
        }

        public void Activate(Transform leader, int index)
        {
            _leader = leader;
            _active = true;
            _manualMode = false;
            _manualPresentation = false;
            _cooldownTimer = 0f;
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
            if (!CanCommand) return;
            if (_leader != null)
            {
                Vector3 destination = _leader.position + _formationOffset;
                float distance = Vector3.Distance(transform.position, destination);
                transform.position = Vector3.MoveTowards(transform.position, destination,
                    Mathf.Max(followSpeed, distance * 3f) * Time.deltaTime);
            }

            _cooldownTimer -= Time.deltaTime;
            if (_manualMode || _attacking || _cooldownTimer > 0f) return;
            var target = FindNearestTarget();
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
            if (_renderer != null) _renderer.enabled = _active && !manual;
            if (_headRenderer != null) _headRenderer.enabled = _active && manual;
        }

        private void OnGUI()
        {
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            if (!_manualPresentation || !_active || _headRenderer == null || !_headRenderer.enabled) return;
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
            string state = _attacking ? "ATACANDO" : IsReadyForManualAttack ? "PRONTO" : Mathf.CeilToInt(ManualCooldownRemaining) + "s";
            GUI.color = IsReadyForManualAttack ? new Color(.45f, 1f, .58f) : new Color(.78f, .82f, .86f);
            GUI.Label(new Rect(screen.x - 60f, screen.y - 12f, 120f, 30f), studentName + "\n" + state, _nameStyle);
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
            if (!IsReadyForManualAttack || !CanReachTarget(target))
                return false;
            StartCoroutine(AttackRoutine(target));
            return true;
        }

        private IEnumerator AttackRoutine(VarginhaCombatTarget target)
        {
            _attacking = true;
            CurrentTarget = target;
            _cooldownTimer = ManualCooldownSeconds;
            var presentation = GetComponent<VarginhaAllyAttackPresentation>() ?? gameObject.AddComponent<VarginhaAllyAttackPresentation>();
            Vector3 impactPoint = target.transform.position;
            Vector2 direction = (impactPoint - (_leader != null ? _leader.position : transform.position)).normalized;
            yield return presentation.Play(studentName, attackStyle, _shirtColor, transform.position, impactPoint,
                () => CanCommand,
                () => {
                    // O dano acompanha o contato visual. O ET pode sair da área durante a preparação.
                    if (CanReachTarget(target) && Vector2.Distance(target.transform.position, impactPoint) <= 1.25f)
                        ApplyTacticalHit(target, direction);
                });

            _attacking = false;
            CurrentTarget = null;
        }

        private void ApplyTacticalHit(VarginhaCombatTarget target, Vector2 direction)
        {
            if (!target.ReceiveHit(_profile.Damage, direction, .055f)) return;
            // Controle de grupo abre uma janela segura para Edelzio entrar no corpo a corpo.
            float stun = attackStyle == VarginhaStudentAllyStyle.JiuJitsu ? 1.2f
                : attackStyle == VarginhaStudentAllyStyle.Art ? .8f
                : attackStyle == VarginhaStudentAllyStyle.Microphone ? .6f : .2f;
            target.GetComponent<VarginhaCombatEnemy>()?.ApplyAllyControl(direction, stun);
            if (attackStyle == VarginhaStudentAllyStyle.Support)
                _leader.GetComponent<EdelzioTopDownController>()?.RestoreSanity(8f);

            bool area = attackStyle == VarginhaStudentAllyStyle.FallingPiano
                || attackStyle == VarginhaStudentAllyStyle.Guitar
                || attackStyle == VarginhaStudentAllyStyle.Microphone;
            bool bounce = attackStyle == VarginhaStudentAllyStyle.PingPong;
            if (!area && !bounce) return;
            var nearby = new System.Collections.Generic.List<VarginhaCombatTarget>();
            foreach (var other in Object.FindObjectsByType<VarginhaCombatTarget>(FindObjectsInactive.Exclude))
                if (other != target && CanReachTarget(other)
                    && Vector2.Distance(target.transform.position, other.transform.position) <= (bounce ? 2.5f : 1.6f)) nearby.Add(other);
            nearby.Sort((a, b) => (a.transform.position - target.transform.position).sqrMagnitude
                .CompareTo((b.transform.position - target.transform.position).sqrMagnitude));
            for (int i = 0; i < nearby.Count && i < (bounce ? 1 : 3); i++)
            {
                var other = nearby[i];
                Vector2 push = (other.transform.position - _leader.position).normalized;
                if (other.ReceiveHit(_profile.Damage * .6f, push, .055f))
                    other.GetComponent<VarginhaCombatEnemy>()?.ApplyAllyControl(push, stun);
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

        private static AttackProfile ProfileFor(string name)
        {
            switch (name)
            {
                case "Matias": return new AttackProfile(VarginhaStudentAllyStyle.JiuJitsu, "golpe de jiujitsu", 28f, 1.15f, .85f, .16f);
                case "Anna Sabia": return new AttackProfile(VarginhaStudentAllyStyle.PingPong, "raquetada de ping-pong", 22f, 1.55f, .72f, .13f);
                case "Ana Tavares": return new AttackProfile(VarginhaStudentAllyStyle.PingPong, "raquetada de ping-pong", 22f, 1.55f, .72f, .13f);
                case "Pedro": return new AttackProfile(VarginhaStudentAllyStyle.Guitar, "golpe de guitarra", 31f, 1.35f, .92f, .18f);
                case "Luis Martins": return new AttackProfile(VarginhaStudentAllyStyle.Art, "golpe de arte", 24f, 1.40f, .88f, .15f);
                case "Luis Miguel Messias": return new AttackProfile(VarginhaStudentAllyStyle.Microphone, "golpe de microfone", 26f, 1.45f, .82f, .14f);
                case "Yasmin": return new AttackProfile(VarginhaStudentAllyStyle.FallingPiano, "piano caindo na cabeça", 45f, 5.00f, 1.35f, .35f);
                case "Fabio": return new AttackProfile(VarginhaStudentAllyStyle.Katana, "golpe de katana", 38f, 1.55f, .78f, .16f);
                case "Marcos": return new AttackProfile(VarginhaStudentAllyStyle.Support, "empurrão de apoio", 18f, 1.30f, 1.00f, .17f);
                default: return new AttackProfile(VarginhaStudentAllyStyle.Support, "golpe de apoio", 16f, 1.10f, 1.05f, .18f);
            }
        }
    }
}
