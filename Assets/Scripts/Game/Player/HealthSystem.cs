using System;
using UnityEngine;

namespace Game.Player
{
    /// <summary>
    /// Sistema de vida genérico. Funciona para Player, Inimigos, etc.
    /// Emite eventos de dano, cura e morte.
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private bool destroyOnDeath = false;
        [SerializeField] private float invincibilityDuration = 0.5f;

        // Events
        public event Action<float, float> OnHealthChanged;   // (current, max)
        public event Action<float> OnDamageReceived;         // (amount)
        public event Action<float> OnHealReceived;           // (amount)
        public event Action OnDeath;

        // Properties
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthPercent => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        public bool IsDead => currentHealth <= 0f;
        public bool IsInvincible => _invincibilityTimer > 0f;

        private float _invincibilityTimer;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (_invincibilityTimer > 0f)
                _invincibilityTimer -= Time.deltaTime;
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>Aplica dano. Respeita invincibilidade.</summary>
        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            if (IsInvincible) return;
            if (amount <= 0f) return;

            float previous = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            _invincibilityTimer = invincibilityDuration;

            OnDamageReceived?.Invoke(amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
                Die();
        }

        /// <summary>Cura HP. Não ultrapassa maxHealth.</summary>
        public void Heal(float amount)
        {
            if (IsDead) return;
            if (amount <= 0f) return;

            float previous = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            float actual = currentHealth - previous;

            OnHealReceived?.Invoke(actual);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>Define vida máxima e ajusta vida atual proporcionalmente.</summary>
        public void SetMaxHealth(float newMax, bool keepPercent = true)
        {
            if (newMax <= 0f) return;
            float percent = keepPercent ? HealthPercent : 1f;
            maxHealth = newMax;
            currentHealth = maxHealth * percent;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>Restaura vida ao máximo.</summary>
        public void FullHeal()
        {
            if (IsDead) return;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>Mata instantaneamente ignorando invincibilidade.</summary>
        public void InstantKill()
        {
            if (IsDead) return;
            currentHealth = 0f;
            OnHealthChanged?.Invoke(0f, maxHealth);
            Die();
        }

        // ─── Private ──────────────────────────────────────────────────────────

        private void Die()
        {
            OnDeath?.Invoke();
            if (destroyOnDeath)
                Destroy(gameObject);
        }
    }
}
