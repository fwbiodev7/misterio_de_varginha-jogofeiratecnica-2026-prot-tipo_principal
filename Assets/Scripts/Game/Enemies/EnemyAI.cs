using UnityEngine;
using Game.Player;
using Game.Managers;

namespace Game.Enemies
{
    /// <summary>
    /// IA de inimigo simples: patrulha entre dois pontos e persegue o player ao detectá-lo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyAI : MonoBehaviour
    {
        public enum EnemyState { Patrol, Chase, Attack, Dead }

        [Header("Patrol")]
        [SerializeField] private Transform patrolPointA;
        [SerializeField] private Transform patrolPointB;
        [SerializeField] private float patrolSpeed = 3f;
        [SerializeField] private float patrolWaitTime = 0.5f;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Combat")]
        [SerializeField] private float attackDamage = 20f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float chaseSpeed = 5f;

        [Header("Score")]
        [SerializeField] private int scoreOnKill = 100;

        // Components
        private Rigidbody2D _rb;
        private HealthSystem _health;

        // State
        private EnemyState _currentState = EnemyState.Patrol;
        private Transform _targetPatrol;
        private Transform _player;
        private float _waitTimer;
        private float _attackTimer;
        private bool _isWaiting;

        // Properties (readable by tests)
        public EnemyState CurrentState => _currentState;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public bool HasPlayer => _player != null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<HealthSystem>();
            _targetPatrol = patrolPointA;

            if (playerLayer.value == 0)
            {
                playerLayer = ~0;
            }

            _health.OnDeath += HandleDeath;
        }

        private void Update()
        {
            if (_currentState == EnemyState.Dead) return;

            _attackTimer -= Time.deltaTime;
            DetectPlayer();

            switch (_currentState)
            {
                case EnemyState.Patrol: UpdatePatrol(); break;
                case EnemyState.Chase:  UpdateChase();  break;
                case EnemyState.Attack: UpdateAttack(); break;
            }
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>Força o inimigo a ir para estado Chase com target (para testes).</summary>
        public void ForceChase(Transform target)
        {
            _player = target;
            _currentState = EnemyState.Chase;
        }

        /// <summary>Força o estado de patrulha (para testes).</summary>
        public void ForcePatrol()
        {
            _player = null;
            _currentState = EnemyState.Patrol;
        }

        // ─── State updates ────────────────────────────────────────────────────

        private void DetectPlayer()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
            if (hit != null && (hit.CompareTag("Player") || hit.GetComponent<PlayerController>() != null))
            {
                _player = hit.transform;
                float dist = Vector2.Distance(transform.position, _player.position);
                _currentState = dist <= attackRange ? EnemyState.Attack : EnemyState.Chase;
            }
            else if (_currentState == EnemyState.Chase || _currentState == EnemyState.Attack)
            {
                _currentState = EnemyState.Patrol;
                _player = null;
            }
        }

        private void UpdatePatrol()
        {
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f) _isWaiting = false;
                return;
            }

            if (_targetPatrol == null) return;

            Vector2 dir = (_targetPatrol.position - transform.position).normalized;
            _rb.linearVelocity = new Vector2(dir.x * patrolSpeed, _rb.linearVelocity.y);

            if (Vector2.Distance(transform.position, _targetPatrol.position) < 0.2f)
            {
                _rb.linearVelocity = Vector2.zero;
                _targetPatrol = _targetPatrol == patrolPointA ? patrolPointB : patrolPointA;
                _waitTimer = patrolWaitTime;
                _isWaiting = true;
            }
        }

        private void UpdateChase()
        {
            if (_player == null) { _currentState = EnemyState.Patrol; return; }
            Vector2 dir = (_player.position - transform.position).normalized;
            _rb.linearVelocity = new Vector2(dir.x * chaseSpeed, _rb.linearVelocity.y);
        }

        private void UpdateAttack()
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            if (_attackTimer > 0f) return;

            _attackTimer = attackCooldown;
            _player?.GetComponent<HealthSystem>()?.TakeDamage(attackDamage);
        }

        private void HandleDeath()
        {
            _currentState = EnemyState.Dead;
            _rb.linearVelocity = Vector2.zero;
            ScoreManager.Instance?.AddScore(scoreOnKill);
            Destroy(gameObject, 0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
