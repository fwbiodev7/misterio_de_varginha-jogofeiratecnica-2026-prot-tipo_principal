using UnityEngine;
using Game.Managers;

namespace Game.Enemies
{
    /// <summary>
    /// Item coletável 2D. Adiciona score e pode curar o player.
    /// Usa trigger 2D para detecção.
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        public enum CollectibleType { Coin, Star, Heart, Key, PowerUp }

        [Header("Settings")]
        [SerializeField] private CollectibleType type = CollectibleType.Coin;
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private float healValue = 0f;
        [SerializeField] private string playerTag = "Player";

        [Header("Animation")]
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private float rotateSpeed = 90f;

        // State
        private bool _collected;
        private Vector3 _startPos;

        // Properties (readable by tests)
        public bool IsCollected => _collected;
        public CollectibleType Type => type;
        public int ScoreValue => scoreValue;
        public float HealValue => healValue;

        private void Start()
        {
            _startPos = transform.position;
        }

        private void Update()
        {
            if (_collected) return;
            // Bob animation
            float y = _startPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            // Rotation
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag(playerTag)) return;

            Collect(other);
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>Coleta o item programaticamente (útil em testes).</summary>
        public void Collect(Collider2D playerCollider = null)
        {
            if (_collected) return;
            _collected = true;

            // Score
            ScoreManager.Instance?.AddScore(scoreValue);

            // Heal player if applicable
            if (healValue > 0f && playerCollider != null)
            {
                var health = playerCollider.GetComponent<Player.HealthSystem>();
                health?.Heal(healValue);
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = type switch
            {
                CollectibleType.Coin   => Color.yellow,
                CollectibleType.Star   => Color.cyan,
                CollectibleType.Heart  => Color.red,
                CollectibleType.Key    => Color.white,
                CollectibleType.PowerUp => Color.magenta,
                _ => Color.gray
            };
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
