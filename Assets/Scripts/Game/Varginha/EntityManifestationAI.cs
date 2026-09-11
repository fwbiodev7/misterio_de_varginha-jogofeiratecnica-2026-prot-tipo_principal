using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Inimigo de terror sobrenatural em Top-Down: Manifestação da Entidade Ancestral de Varginha.
    /// Surge após o caderno ser descoberto, persegue Edelzio, distorce o ambiente e drena sua sanidade.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EntityManifestationAI : MonoBehaviour
    {
        private static EntityManifestationAI _instance;

        [Header("Patrol & Chase")]
        [SerializeField] private float patrolSpeed = 2.0f;
        [SerializeField] private float chaseSpeed = 3.8f;
        [SerializeField] private float detectionRadius = 6.5f;
        [SerializeField] private float sanityDrainPerSecond = 20f;
        [SerializeField] private float drainRadius = 2.0f;

        [Header("Waypoints")]
        [SerializeField] private Transform[] patrolWaypoints;
        private int _currentWaypointIndex;

        [Header("State")]
        [SerializeField] private bool isActive = false;

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Transform _playerTransform;
        private EdelzioTopDownController _playerController;
        private float _flickerTimer;

        public bool IsActive => isActive;

        public static void AwakenEntity()
        {
            if (_instance != null)
            {
                _instance.gameObject.SetActive(true);
                _instance.isActive = true;
                Debug.Log("👁️ [Entidade Ancestral] A manifestação sobrenatural desperta na casa!");
            }
        }

        private void Awake()
        {
            _instance = this;
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();

            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true; // Permite perseguição e drenagem sem empurrar rigidamente
        }

        private void Start()
        {
            var p = Object.FindAnyObjectByType<EdelzioTopDownController>();
            if (p != null)
            {
                _playerController = p;
                _playerTransform = p.transform;
            }

            // Inicia oculta até o jogador encontrar o caderno
            if (!isActive)
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isActive) return;

            AnimateSupernaturalFlicker();
            CheckSanityDrain();
        }

        private void FixedUpdate()
        {
            if (!isActive) return;

            if (_playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                if (dist <= detectionRadius)
                {
                    ChasePlayer();
                    return;
                }
            }

            Patrol();
        }

        private void ChasePlayer()
        {
            Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * chaseSpeed;
        }

        private void Patrol()
        {
            if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Transform targetPoint = patrolWaypoints[_currentWaypointIndex];
            if (targetPoint == null) return;

            Vector2 dir = ((Vector2)targetPoint.position - (Vector2)transform.position).normalized;
            _rb.linearVelocity = dir * patrolSpeed;

            if (Vector2.Distance(transform.position, targetPoint.position) < 0.3f)
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % patrolWaypoints.Length;
            }
        }

        private void CheckSanityDrain()
        {
            if (_playerController == null || _playerTransform == null) return;

            float dist = Vector2.Distance(transform.position, _playerTransform.position);
            if (dist <= drainRadius)
            {
                _playerController.DrainSanity(sanityDrainPerSecond * Time.deltaTime);
            }
        }

        private void AnimateSupernaturalFlicker()
        {
            // Pulso sobrenatural vermelho/sombra e transparência instável
            _flickerTimer += Time.deltaTime * 6f;
            float alpha = 0.5f + Mathf.PingPong(_flickerTimer, 0.45f);
            if (_sr != null)
            {
                _sr.color = new Color(0.9f, 0.1f, 0.2f, alpha);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, drainRadius);
        }
    }
}
