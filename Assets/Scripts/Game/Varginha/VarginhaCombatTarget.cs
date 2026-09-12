using Game.Player;
using UnityEngine;

namespace Game.Varginha
{
    [RequireComponent(typeof(HealthSystem))]
    public sealed class VarginhaCombatTarget : MonoBehaviour
    {
        public enum EnemyKind { MinorManifestation, AlteredCreature, AncestralEntity }
        [SerializeField] private EnemyKind kind;
        private HealthSystem _health;
        public EnemyKind Kind => kind;
        public bool IsDead => _health != null && _health.IsDead;
        public void SetKind(EnemyKind value) => kind = value;

        private void Awake()
        {
            _health = GetComponent<HealthSystem>();
            // A entidade principal segue o GDD: ela persegue e causa medo, mas não pode ser derrotada pelo golpe comum.
            if (GetComponent<EntityManifestationAI>() != null) kind = EnemyKind.AncestralEntity;
        }

        public bool ReceiveHit(float damage, Vector2 direction, float hitstop)
        {
            if (kind == EnemyKind.AncestralEntity || _health == null || _health.IsDead) return false;
            float before = _health.CurrentHealth;
            _health.TakeDamage(damage);
            if (_health.CurrentHealth >= before) return false;
            GetComponent<VarginhaCombatEnemy>()?.ReactToHit(direction, hitstop);
            if (_health.IsDead)
            {
                foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
                foreach (var renderer in GetComponentsInChildren<SpriteRenderer>()) renderer.enabled = false;
                var body = GetComponent<Rigidbody2D>();
                if (body != null) { body.linearVelocity = Vector2.zero; body.simulated = false; }
            }
            return true;
        }
    }
}
