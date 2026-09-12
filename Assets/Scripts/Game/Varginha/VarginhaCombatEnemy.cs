using System.Collections;
using Game.Player;
using UnityEngine;

namespace Game.Varginha
{
    [RequireComponent(typeof(Rigidbody2D), typeof(VarginhaCombatTarget))]
    public sealed class VarginhaCombatEnemy : MonoBehaviour
    {
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
        private float _freeze, _stagger, _attackTimer, _poiseUntil;
        private Vector2 _knockback;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[16];
        private bool _attacking;
        private int _interruptVersion;
        private GameObject _effect, _warning;
        private Vector3 _baseScale;
        private Vector3 _baseLocalPosition;
        private float _animTime;
        private float _hitFlash;
        public float EffectiveSpeed => speed;
        public float EffectiveDamage => attackDamage;
        public float EffectiveAttackInterval => attackCooldown;
        private bool CanFight => _player != null && !_player.IsInputLocked && !_player.IsScriptedMotion
            && !VarginhaTravelCinematic.IsTravelling && VarginhaGameHUD.Instance?.IsDialogueOpen != true && Time.timeScale > 0f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>(); _body.gravityScale = 0;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _target = GetComponent<VarginhaCombatTarget>();
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null) _normalColor = _renderer.color;
            _baseScale = transform.localScale.sqrMagnitude > .001f ? transform.localScale : Vector3.one;
            _baseLocalPosition = transform.localPosition;
        }
        private void Start()
        {
            if (GetComponent<VarginhaEnemyFeedback>() == null)
                gameObject.AddComponent<VarginhaEnemyFeedback>();
            speed *= VarginhaDifficulty.EnemySpeed;
            attackDamage *= VarginhaDifficulty.EnemyDamage;
            attackSanityDamage *= VarginhaDifficulty.EnemyDamage;
            attackCooldown *= VarginhaDifficulty.EnemyAttackInterval;
            attackRange = Mathf.Max(attackRange,3.8f);
            detectionRange *= VarginhaDifficulty.Selected == InvestigationDifficulty.Hard ? 1.5f : 1.2f;
            var health = GetComponent<HealthSystem>();
            health.SetMaxHealth(health.MaxHealth * VarginhaDifficulty.EnemyHealth,true);
            _attackTimer = .4f + Mathf.Abs(GetHashCode() % 7) * .13f;
        }
        public void ReactToHit(Vector2 direction, float hitstop)
        {
            if (Time.time < _poiseUntil) return;
            _interruptVersion++;
            _freeze = Mathf.Max(_freeze,hitstop * VarginhaDifficulty.ControlResistance);
            _stagger = .2f * VarginhaDifficulty.ControlResistance;
            _knockback = direction.normalized * 3.5f;
            _body.linearVelocity = Vector2.zero;
            _hitFlash = .12f;
            // Ataques comuns não mantêm o ET permanentemente travado; habilidades longas ainda funcionam.
            if (hitstop > .1f) _freeze = hitstop * VarginhaDifficulty.ControlResistance;
            _poiseUntil = Time.time + (VarginhaDifficulty.Selected == InvestigationDifficulty.Hard ? .65f : .25f);
        }
        public void ApplyAllyControl(Vector2 direction, float duration)
        {
            _interruptVersion++;
            _freeze = Mathf.Max(_freeze,duration * VarginhaDifficulty.ControlResistance);
            _knockback = direction.normalized * 2f;
        }
        private void Update()
        {
            if (_target.IsDead) { CancelAttack(); return; }
            if (_player == null) _player = FindAnyObjectByType<EdelzioTopDownController>();
            if (!CanFight) return;
            _animTime += Time.deltaTime;
            _hitFlash = Mathf.Max(0f, _hitFlash - Time.deltaTime);
            _attackTimer -= Time.deltaTime;
            if (_freeze > 0) { _freeze -= Time.deltaTime; AnimateBody(true); if (_renderer != null) _renderer.color = Color.white; return; }
            AnimateBody(false);
            if (_renderer != null)
            {
                Color tint = _attacking ? Color.Lerp(_normalColor,Color.red,.5f + .3f*Mathf.Sin(Time.time*24)) : _normalColor;
                if (_hitFlash > 0f) tint = Color.Lerp(tint, Color.white, _hitFlash / .12f);
                _renderer.color = tint;
                if (_player != null) _renderer.flipX = _player.transform.position.x < transform.position.x;
            }
            if (_stagger > 0) { _stagger -= Time.deltaTime; return; }
            if (!_attacking && _attackTimer <= 0 && Vector2.Distance(transform.position,_player.transform.position) <= attackRange
                && ClearShot(_player.transform.position))
            {
                _attackTimer = attackCooldown;
                Vector3 aim = _player.transform.position;
                if (VarginhaDifficulty.Selected == InvestigationDifficulty.Hard)
                    aim += (Vector3)_player.GetComponent<Rigidbody2D>().linearVelocity * .12f;
                StartCoroutine(AttackRoutine(aim));
            }
        }
        private bool ClearShot(Vector3 destination)
        {
            foreach (var hit in Physics2D.LinecastAll(transform.position,destination))
            {
                if (hit.collider.isTrigger || hit.collider.GetComponentInParent<VarginhaCombatTarget>() != null
                    || hit.collider.GetComponentInParent<EdelzioTopDownController>() != null) continue;
                return false;
            }
            return true;
        }
        private void FixedUpdate()
        {
            if (_target.IsDead || _freeze > 0 || !CanFight || _attacking) { _body.linearVelocity = Vector2.zero; return; }
            Vector2 delta = (Vector2)_player.transform.position - _body.position;
            Vector2 velocity = Vector2.zero;
            if (_stagger > 0) velocity = _knockback;
            else if (delta.magnitude < detectionRange)
            {
                Vector2 forward = delta.normalized;
                if (delta.magnitude > 2.5f) velocity = forward * speed;
                else if (VarginhaDifficulty.Selected != InvestigationDifficulty.Easy)
                {
                    float side = GetHashCode() % 2 == 0 ? 1 : -1;
                    velocity = (new Vector2(-forward.y,forward.x)*side*.75f - forward*(delta.magnitude<1.5f ? .6f : 0)).normalized * speed;
                }
            }
            Vector2 motion = velocity * Time.fixedDeltaTime;
            if (motion.sqrMagnitude < .000001f) { _body.linearVelocity = Vector2.zero; return; }
            var filter = new ContactFilter2D(); filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); filter.useTriggers = false;
            int count = _body.Cast(motion.normalized,filter,_hits,motion.magnitude+.02f);
            float distance = motion.magnitude;
            for (int i=0;i<count;i++) if (Vector2.Dot(_hits[i].normal,motion)<0) distance=Mathf.Min(distance,Mathf.Max(0,_hits[i].distance-.02f));
            _body.MovePosition(_body.position+motion.normalized*distance);
        }
        private IEnumerator AttackRoutine(Vector3 targetPosition)
        {
            _attacking = true;
            int version = _interruptVersion;
            _warning = new GameObject("ET_Aviso_Esquive");
            _warning.transform.position = targetPosition;
            var warning = _warning.AddComponent<SpriteRenderer>();
            warning.sprite = VarginhaAllyAttackPresentation.MarkerSprite;
            warning.color = new Color(1,.2f,.2f,.8f); warning.sortingOrder = 30;
            float elapsed = 0;
            while (elapsed < VarginhaDifficulty.TelegraphSeconds)
            {
                if (_target.IsDead || version != _interruptVersion) { CancelAttack(); yield break; }
                if (!CanFight) { yield return null; continue; }
                elapsed += Time.deltaTime;
                _warning.transform.localScale = Vector3.one*Mathf.Lerp(2.2f,1.7f,elapsed/VarginhaDifficulty.TelegraphSeconds);
                yield return null;
            }
            Vector3 origin=transform.position;
            _effect=new GameObject("ET_Rajada");
            var renderer=_effect.AddComponent<SpriteRenderer>(); renderer.sprite=VarginhaPixelArtSprites.Create("ET_Attack",new Color(.4f,1f,.25f)); renderer.sortingOrder=35;
            _effect.transform.localScale=Vector3.one*1.1f;
            Vector2 direction=(targetPosition-origin).normalized;
            _effect.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            elapsed=0;
            while(elapsed<.2f)
            {
                if (_target.IsDead || version != _interruptVersion) { CancelAttack(); yield break; }
                if (!CanFight) { yield return null; continue; }
                elapsed+=Time.deltaTime; _effect.transform.position=Vector3.Lerp(origin,targetPosition,elapsed/.2f); yield return null;
            }
            if (CanFight && version == _interruptVersion && ClearShot(targetPosition)
                && Vector2.Distance(_player.transform.position,targetPosition)<.85f)
            {
                _player.DrainSanity(attackSanityDamage);
                _player.GetComponent<HealthSystem>()?.TakeDamage(attackDamage);
            }
            CancelAttack();
        }
        private void CancelAttack()
        {
            if (_effect != null) Destroy(_effect); if (_warning != null) Destroy(_warning);
            _effect=null; _warning=null; _attacking=false;
        }
        private void AnimateBody(bool frozen)
        {
            if (_renderer == null) return;
            float pulse = frozen ? 0f : Mathf.Sin(_animTime * 7.5f + transform.position.x * 1.7f) * .035f;
            if (_attacking) pulse += Mathf.Sin(_animTime * 18f) * .045f;
            transform.localScale = _baseScale * (1f + pulse);
            float bob = frozen ? 0f : Mathf.Sin(_animTime * 5.5f + transform.position.y * 1.3f) * .018f;
            transform.localPosition = _baseLocalPosition + Vector3.up * bob;
        }
        private void OnDisable() { StopAllCoroutines(); CancelAttack(); }
    }
}
