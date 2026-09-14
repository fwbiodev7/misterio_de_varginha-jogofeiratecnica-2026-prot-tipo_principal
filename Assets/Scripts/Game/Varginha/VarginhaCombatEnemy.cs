using Game.Player;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Disparo, investida e impacto em área, todos com preparação e recuperação.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(VarginhaCombatTarget))]
    public sealed class VarginhaCombatEnemy : MonoBehaviour
    {
        public enum EnemyRole { Automatic, Skirmisher, Rusher, Sentinel }
        public enum CombatState { Pursuing, Preparing, Attacking, Recovering, Stunned }

        [SerializeField] private EnemyRole role = EnemyRole.Automatic;
        [SerializeField] private float speed = 1.8f;
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float attackRange = 1.45f;
        [SerializeField] private float attackCooldown = .9f;
        [SerializeField] private float attackDamage = 18f;
        [SerializeField] private float attackSanityDamage = 14f;

        private EdelzioTopDownController _player;
        private Rigidbody2D _body;
        private VarginhaCombatTarget _target;
        private SpriteRenderer _renderer;
        private Color _normalColor;
        private EnemyRole _role;
        private CombatState _state;
        private float _freeze, _stagger, _attackTimer, _poiseUntil;
        private float _stateTime, _telegraphDuration, _hitFlash;
        private float _slowRemaining, _slowMultiplier = 1f;
        private Vector2 _knockback, _attackOrigin, _attackDestination, _attackDirection;
        private bool _strikeResolved;
        private GameObject _effect, _warning;
        private SpriteRenderer _warningProgress, _effectRenderer;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[32];
        private readonly Collider2D[] _neighbors = new Collider2D[16];

        public float EffectiveSpeed => speed;
        public float EffectiveDamage => attackDamage;
        public float EffectiveAttackInterval => attackCooldown;
        public EnemyRole Role => _role;
        public CombatState State => _freeze > 0f ? CombatState.Stunned : _state;
        public bool IsRecovering => _state == CombatState.Recovering && _freeze <= 0f;
        public float TelegraphProgress => _state == CombatState.Preparing
            ? Mathf.Clamp01(_stateTime / Mathf.Max(.01f, _telegraphDuration)) : 0f;
        public bool IsSlowed => _slowRemaining > 0f;
        public string RoleLabel => _role == EnemyRole.Rusher ? "INVESTIDOR"
            : _role == EnemyRole.Sentinel ? "SENTINELA" : "ATIRADOR";
        public Color RoleColor => _role == EnemyRole.Rusher ? new Color(1f, .52f, .16f)
            : _role == EnemyRole.Sentinel ? new Color(.77f, .45f, 1f) : new Color(.25f, .88f, 1f);
        private float SlamRadius => 1.65f;
        private bool CanFight => _player != null && !_player.IsInputLocked && !_player.IsScriptedMotion
            && _player.GetComponent<HealthSystem>()?.IsDead != true
            && !VarginhaTravelCinematic.IsTravelling && VarginhaGameHUD.Instance?.IsDialogueOpen != true && Time.timeScale > 0f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _target = GetComponent<VarginhaCombatTarget>();
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null) _normalColor = _renderer.color;
            _role = ResolveRole();
        }

        private void Start()
        {
            if (GetComponent<VarginhaEnemyFeedback>() == null) gameObject.AddComponent<VarginhaEnemyFeedback>();
            _role = ResolveRole();
            float roleSpeed = _role == EnemyRole.Rusher ? 1.2f : _role == EnemyRole.Sentinel ? .76f : 1f;
            float roleDamage = _role == EnemyRole.Sentinel ? 1.3f : _role == EnemyRole.Rusher ? 1.1f : 1f;
            speed *= VarginhaDifficulty.EnemySpeed * roleSpeed;
            attackDamage *= VarginhaDifficulty.EnemyDamage * roleDamage;
            attackSanityDamage *= VarginhaDifficulty.EnemyDamage;
            attackCooldown *= VarginhaDifficulty.EnemyAttackInterval;
            attackRange = _role == EnemyRole.Sentinel ? SlamRadius : _role == EnemyRole.Rusher ? 3.1f : 4.4f;
            detectionRange *= VarginhaDifficulty.Selected == InvestigationDifficulty.Hard ? 1.5f : 1.2f;
            var health = GetComponent<HealthSystem>();
            health.SetMaxHealth(health.MaxHealth * VarginhaDifficulty.EnemyHealth
                * (_role == EnemyRole.Sentinel ? 1.45f : 1f), true);
            _attackTimer = .65f + (StableNameHash() % 6) * .12f;
            if (_renderer != null)
            {
                // A função do inimigo continua mudando a IA e os golpes, mas a
                // espécie mantém uma leitura visual única: ET marrom com olhos
                // vermelhos. As cores de RoleColor ficam reservadas aos avisos e
                // efeitos de ataque.
                _renderer.sprite = VarginhaPixelArtSprites.Create("ET_Subordinate_" + _role,
                    new Color(.68f, .36f, .18f));
                _renderer.color = Color.white;
            }
        }

        private EnemyRole ResolveRole()
        {
            if (role != EnemyRole.Automatic) return role;
            // Nomes de ondas terminam em um número: cada trio contém os três papéis.
            int last = gameObject.name.Length - 1;
            int index = last >= 0 && char.IsDigit(gameObject.name[last])
                ? Mathf.Max(0, gameObject.name[last] - '1') : StableNameHash();
            return (EnemyRole)(1 + index % 3);
        }

        private int StableNameHash()
        {
            uint value = 2166136261;
            foreach (char letter in gameObject.name) value = unchecked((value ^ letter) * 16777619);
            return (int)(value & 0x7fffffff);
        }

        public void ReactToHit(Vector2 direction, float hitstop)
        {
            _hitFlash = .12f;
            if (_target.IsDead || Time.time < _poiseUntil) return;
            InterruptAttack();
            _freeze = Mathf.Max(_freeze, Mathf.Max(0f, hitstop) * VarginhaDifficulty.ControlResistance);
            _stagger = .17f * VarginhaDifficulty.ControlResistance;
            _knockback = direction.normalized * 3.5f;
            _body.linearVelocity = Vector2.zero;
            _poiseUntil = Time.time + (VarginhaDifficulty.Selected == InvestigationDifficulty.Hard ? .48f : .22f);
        }

        public void ApplyAllyControl(Vector2 direction, float duration, float knockbackStrength = 2f)
        {
            if (_target.IsDead) return;
            InterruptAttack();
            _freeze = Mathf.Max(_freeze, Mathf.Max(0f, duration) * VarginhaDifficulty.ControlResistance);
            _stagger = Mathf.Max(_stagger, .18f);
            _knockback = direction.normalized * Mathf.Clamp(knockbackStrength, 0f, 12f);
            _body.linearVelocity = Vector2.zero;
        }

        public void ApplyAllySlow(float duration, float speedMultiplier = .5f)
        {
            if (_target.IsDead) return;
            _slowRemaining = Mathf.Max(_slowRemaining, Mathf.Max(0f, duration));
            _slowMultiplier = Mathf.Min(_slowMultiplier, Mathf.Clamp(speedMultiplier, .2f, 1f));
        }

        private void Update()
        {
            if (_target.IsDead) { CancelAttack(); return; }
            if (_player == null) _player = FindAnyObjectByType<EdelzioTopDownController>();
            if (!CanFight)
            {
                // Conversas cancelam rajadas para não acertar ao fechar o painel.
                if (_state == CombatState.Preparing || _state == CombatState.Attacking) InterruptAttack();
                return;
            }
            float dt = Time.deltaTime;
            _hitFlash = Mathf.Max(0f, _hitFlash - dt);
            _slowRemaining = Mathf.Max(0f, _slowRemaining - dt);
            if (_slowRemaining <= 0f) _slowMultiplier = 1f;
            UpdateAppearance();
            if (_freeze > 0f) { _freeze = Mathf.Max(0f, _freeze - dt); return; }
            if (_stagger > 0f) { _stagger = Mathf.Max(0f, _stagger - dt); return; }

            _stateTime += dt;
            switch (_state)
            {
                case CombatState.Preparing:
                    UpdateWarning();
                    if (_stateTime >= _telegraphDuration) ReleaseAttack();
                    break;
                case CombatState.Attacking:
                    if (_role == EnemyRole.Skirmisher) AdvanceProjectile(dt);
                    else if (_role == EnemyRole.Sentinel)
                    {
                        if (_effect != null) _effect.transform.localScale = Vector3.one * Mathf.Lerp(.7f, SlamRadius * 2.1f, _stateTime / .28f);
                        if (_effectRenderer != null) _effectRenderer.color = new Color(1f, 1f, 1f, 1f - _stateTime / .3f);
                        if (_stateTime >= .3f) BeginRecovery();
                    }
                    else if (_stateTime >= .5f) BeginRecovery();
                    break;
                case CombatState.Recovering:
                    if (_stateTime >= RecoverySeconds()) { _state = CombatState.Pursuing; _stateTime = 0f; }
                    break;
                default:
                    _attackTimer -= dt;
                    if (_attackTimer <= 0f && Vector2.Distance(_body.position, _player.transform.position) <= attackRange
                        && ClearShot(_player.transform.position)) BeginAttack();
                    break;
            }
        }

        private void FixedUpdate()
        {
            _body.linearVelocity = Vector2.zero;
            if (_target.IsDead || _freeze > 0f || !CanFight) return;
            if (_stagger > 0f) { MoveBody(_knockback * Time.fixedDeltaTime); return; }
            if (_state == CombatState.Attacking && _role == EnemyRole.Rusher) { AdvanceCharge(); return; }
            if (_state != CombatState.Pursuing) return;

            Vector2 delta = (Vector2)_player.transform.position - _body.position;
            if (delta.magnitude > detectionRange) return;
            Vector2 forward = delta.normalized;
            Vector2 movement;
            if (_role == EnemyRole.Skirmisher)
            {
                if (delta.magnitude > 3.3f) movement = forward;
                else if (delta.magnitude < 2.2f) movement = -forward;
                else movement = new Vector2(-forward.y, forward.x) * (StableNameHash() % 2 == 0 ? .55f : -.55f);
            }
            else movement = delta.magnitude > (_role == EnemyRole.Sentinel ? 1.15f : 1.6f) ? forward : Vector2.zero;

            // Separação mantém a silhueta e a antecipação de cada inimigo visíveis.
            int neighbors = Physics2D.OverlapCircle(_body.position, 1.1f, SolidFilter(), _neighbors);
            Vector2 separation = Vector2.zero;
            for (int i = 0; i < neighbors; i++)
            {
                var other = _neighbors[i].GetComponentInParent<VarginhaCombatEnemy>();
                if (other == null || other == this || other._target.IsDead) continue;
                Vector2 away = _body.position - other._body.position;
                separation += away.normalized * Mathf.Clamp01(1.1f - away.magnitude);
            }
            movement = Vector2.ClampMagnitude(movement + separation * .9f, 1f);
            Vector2 motion = movement * (speed * _slowMultiplier * Time.fixedDeltaTime);
            Vector2 actual = MoveBody(motion);
            // Desliza pela borda de paredes sem atravessar colisores.
            if (motion.sqrMagnitude > .00001f && actual.sqrMagnitude < motion.sqrMagnitude * .25f)
            {
                Vector2 tangent = new Vector2(-movement.y, movement.x) * (StableNameHash() % 2 == 0 ? 1f : -1f);
                MoveBody(tangent * (speed * _slowMultiplier * Time.fixedDeltaTime));
            }
        }

        private Vector2 MoveBody(Vector2 motion)
        {
            if (motion.sqrMagnitude < .000001f) return Vector2.zero;
            int count = _body.Cast(motion.normalized, SolidFilter(), _hits, motion.magnitude + .025f);
            float distance = motion.magnitude;
            for (int i = 0; i < count; i++)
                if (Vector2.Dot(_hits[i].normal, motion) < 0f)
                    distance = Mathf.Min(distance, Mathf.Max(0f, _hits[i].distance - .025f));
            Vector2 actual = motion.normalized * distance;
            _body.MovePosition(_body.position + actual);
            return actual;
        }

        private ContactFilter2D SolidFilter()
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            filter.useTriggers = false;
            return filter;
        }

        private bool ClearShot(Vector3 destination) => ClearSegment(_body.position, destination);

        private bool ClearSegment(Vector2 origin, Vector2 destination)
        {
            int count = Physics2D.Linecast(origin, destination, SolidFilter(), _hits);
            for (int i = 0; i < count; i++)
            {
                Collider2D collider = _hits[i].collider;
                if (collider == null || collider.GetComponentInParent<VarginhaCombatTarget>() != null
                    || collider.GetComponentInParent<EdelzioTopDownController>() != null
                    || collider.GetComponentInParent<VarginhaStudentAlly>() != null) continue;
                return false;
            }
            return true;
        }

        private void BeginAttack()
        {
            _state = CombatState.Preparing;
            _stateTime = 0f;
            _strikeResolved = false;
            _attackOrigin = _body.position;
            Vector2 aim = _player.transform.position;
            var playerBody = _player.GetComponent<Rigidbody2D>();
            if (VarginhaDifficulty.Selected == InvestigationDifficulty.Hard && playerBody != null)
                aim += playerBody.linearVelocity * .12f;
            _attackDirection = (aim - _attackOrigin).normalized;
            if (_attackDirection.sqrMagnitude < .001f) _attackDirection = Vector2.down;
            _attackDestination = _role == EnemyRole.Sentinel ? _attackOrigin
                : _attackOrigin + _attackDirection * (_role == EnemyRole.Rusher
                    ? Mathf.Min(attackRange, Vector2.Distance(_attackOrigin, aim) + .5f) : attackRange);
            _telegraphDuration = VarginhaDifficulty.TelegraphSeconds
                + (_role == EnemyRole.Sentinel ? .3f : _role == EnemyRole.Rusher ? .18f : .06f);
            CreateWarning();
        }

        private void CreateWarning()
        {
            _warning = new GameObject("ET_Aviso_" + RoleLabel);
            float radius = _role == EnemyRole.Sentinel ? SlamRadius : _role == EnemyRole.Rusher ? .58f : .32f;
            CreateMarker("Limite_do_golpe", _attackDestination, radius * 2.15f, new Color(RoleColor.r, RoleColor.g, RoleColor.b, .9f));
            if (_role != EnemyRole.Sentinel)
            {
                int steps = Mathf.CeilToInt(Vector2.Distance(_attackOrigin, _attackDestination) / .45f);
                for (int i = 1; i < steps; i++)
                    CreateMarker("Trajeto_" + i, Vector2.Lerp(_attackOrigin, _attackDestination, i / (float)steps),
                        radius * 1.7f, new Color(RoleColor.r, RoleColor.g, RoleColor.b, .4f));
            }
            _warningProgress = CreateMarker("Preparacao", _attackDestination, .01f, Color.white);
        }

        private SpriteRenderer CreateMarker(string label, Vector2 position, float size, Color color)
        {
            var marker = new GameObject(label);
            marker.transform.SetParent(_warning.transform);
            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * size;
            var renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaAllyAttackPresentation.MarkerSprite;
            renderer.color = color;
            renderer.sortingOrder = 4;
            return renderer;
        }

        private void UpdateWarning()
        {
            if (_warningProgress == null) return;
            float diameter = (_role == EnemyRole.Sentinel ? SlamRadius : _role == EnemyRole.Rusher ? .58f : .32f) * 2.15f;
            _warningProgress.transform.localScale = Vector3.one * Mathf.Lerp(.08f, diameter, TelegraphProgress);
            _warningProgress.color = Color.Lerp(RoleColor, Color.white, TelegraphProgress);
        }

        private void ReleaseAttack()
        {
            if (!CanFight) { InterruptAttack(); return; }
            _state = CombatState.Attacking;
            _stateTime = 0f;
            if (_role == EnemyRole.Rusher) return;
            _effect = new GameObject(_role == EnemyRole.Sentinel ? "ET_Onda_de_Choque" : "ET_Rajada");
            _effect.transform.position = _attackOrigin;
            _effectRenderer = _effect.AddComponent<SpriteRenderer>();
            _effectRenderer.sprite = VarginhaPixelArtSprites.Create(_role == EnemyRole.Sentinel ? "ET_Shockwave" : "ET_Attack", RoleColor);
            _effectRenderer.sortingOrder = 12;
            if (_role == EnemyRole.Sentinel)
            {
                if (Vector2.Distance(_player.transform.position, _attackOrigin) <= SlamRadius
                    && ClearSegment(_attackOrigin, _player.transform.position)) TryDamagePlayer();
            }
            else
            {
                _effect.transform.localScale = Vector3.one * .72f;
                _effect.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_attackDirection.y, _attackDirection.x) * Mathf.Rad2Deg);
            }
        }

        private void AdvanceProjectile(float dt)
        {
            if (_effect == null) { BeginRecovery(); return; }
            Vector2 origin = _effect.transform.position;
            float remaining = Vector2.Distance(origin, _attackDestination);
            float distance = Mathf.Min(remaining, 7.5f * dt);
            int count = Physics2D.CircleCast(origin, .18f, _attackDirection, SolidFilter(), _hits, distance);
            float obstruction = float.PositiveInfinity;
            float playerDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider2D collider = _hits[i].collider;
                if (collider == null || collider.GetComponentInParent<VarginhaCombatTarget>() != null
                    || collider.GetComponentInParent<VarginhaStudentAlly>() != null) continue;
                if (collider.GetComponentInParent<EdelzioTopDownController>() != null)
                    playerDistance = Mathf.Min(playerDistance, _hits[i].distance);
                else obstruction = Mathf.Min(obstruction, _hits[i].distance);
            }
            if (playerDistance < obstruction && !_player.IsDodging)
            {
                TryDamagePlayer();
                BeginRecovery();
                return;
            }
            if (obstruction <= distance) { BeginRecovery(); return; }
            _effect.transform.position = origin + _attackDirection * distance;
            if (remaining <= distance + .001f) BeginRecovery();
        }

        private void AdvanceCharge()
        {
            Vector2 origin = _body.position;
            float remaining = Vector2.Distance(origin, _attackDestination);
            Vector2 motion = _attackDirection * Mathf.Min(remaining, 8f * Time.fixedDeltaTime);
            Vector2 actual = MoveBody(motion);
            Vector2 end = origin + actual;
            Vector2 closest = ClosestPointOnSegment(_player.transform.position, origin, end);
            if (!_strikeResolved && Vector2.Distance(_player.transform.position, closest) <= .82f
                && ClearSegment(origin, _player.transform.position)) TryDamagePlayer();
            if (remaining <= actual.magnitude + .03f || actual.magnitude < motion.magnitude * .5f) BeginRecovery();
        }

        private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            return start + segment * (segment.sqrMagnitude < .00001f ? 0f
                : Mathf.Clamp01(Vector2.Dot(point - start, segment) / segment.sqrMagnitude));
        }

        private bool TryDamagePlayer()
        {
            if (_strikeResolved || !CanFight || _player.IsDodging) return false;
            var health = _player.GetComponent<HealthSystem>();
            if (health == null || health.IsDead || health.IsInvincible) return false;
            _strikeResolved = true;
            float previousHealth = health.CurrentHealth;
            health.TakeDamage(attackDamage);
            if (health.CurrentHealth >= previousHealth) return false;
            _player.DrainSanity(attackSanityDamage);
            return true;
        }

        private float RecoverySeconds() => _role == EnemyRole.Sentinel ? .95f : _role == EnemyRole.Rusher ? .72f : .42f;

        private void BeginRecovery()
        {
            CancelAttack();
            _state = CombatState.Recovering;
            _stateTime = 0f;
            _attackTimer = attackCooldown;
        }

        private void InterruptAttack()
        {
            CancelAttack();
            _state = CombatState.Recovering;
            _stateTime = 0f;
            _attackTimer = Mathf.Max(_attackTimer, .45f);
        }

        private void CancelAttack()
        {
            if (_effect != null) Destroy(_effect);
            if (_warning != null) Destroy(_warning);
            _effect = null;
            _warning = null;
            _effectRenderer = null;
            _warningProgress = null;
        }

        private void UpdateAppearance()
        {
            if (_renderer == null) return;
            Color tint = _normalColor;
            if (_freeze > 0f) tint = Color.Lerp(tint, new Color(.45f, .9f, 1f), .55f);
            else if (_state == CombatState.Preparing) tint = Color.Lerp(tint, RoleColor, .25f + TelegraphProgress * .4f);
            else if (_state == CombatState.Recovering) tint = Color.Lerp(tint, new Color(.65f, .7f, .83f), .3f);
            if (_hitFlash > 0f) tint = Color.Lerp(tint, Color.white, _hitFlash / .12f);
            _renderer.color = tint;
            if (_state != CombatState.Attacking) _renderer.flipX = _player.transform.position.x < transform.position.x;
            // A animação nunca altera posição/escala da raiz: ela contém o corpo e o colisor.
        }

        private void OnDisable()
        {
            CancelAttack();
            _state = CombatState.Pursuing;
            _attackTimer = Mathf.Max(_attackTimer, .6f);
            if (_body != null) _body.linearVelocity = Vector2.zero;
            if (_renderer != null) _renderer.color = _normalColor;
        }
    }
}
