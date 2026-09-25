using NUnit.Framework;
using UnityEngine;
using Game.Player;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Testa o HealthSystem em PlayMode, onde Awake inicializa a vida.
    /// Cobre: dano, cura, morte, invincibilidade, limites.
    /// </summary>
    public class HealthSystemTests
    {
        private GameObject _go;
        private HealthSystem _health;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestPlayer");
            _health = _go.AddComponent<HealthSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // ─── Initialization ───────────────────────────────────────────────────

        [Test]
        public void Health_StartsAtMaxHealth()
        {
            Assert.AreEqual(_health.MaxHealth, _health.CurrentHealth,
                "HP inicial deve ser igual ao MaxHealth.");
        }

        [Test]
        public void Health_IsNotDead_Initially()
        {
            Assert.IsFalse(_health.IsDead, "Personagem não deve começar morto.");
        }

        // ─── Damage ───────────────────────────────────────────────────────────

        [Test]
        public void TakeDamage_ReducesHP()
        {
            float before = _health.CurrentHealth;
            _health.TakeDamage(25f);
            Assert.Less(_health.CurrentHealth, before, "Dano deve reduzir HP.");
        }

        [Test]
        public void TakeDamage_CorrectAmount()
        {
            _health.TakeDamage(30f);
            Assert.AreEqual(70f, _health.CurrentHealth, 0.001f,
                "HP deve ser MaxHealth - dano.");
        }

        [Test]
        public void TakeDamage_NegativeAmount_IsIgnored()
        {
            float before = _health.CurrentHealth;
            _health.TakeDamage(-10f);
            Assert.AreEqual(before, _health.CurrentHealth, 0.001f,
                "Dano negativo não deve afetar HP.");
        }

        [Test]
        public void TakeDamage_CannotGoBelowZero()
        {
            _health.TakeDamage(9999f);
            Assert.GreaterOrEqual(_health.CurrentHealth, 0f, "HP não pode ser negativo.");
        }

        // ─── Death ────────────────────────────────────────────────────────────

        [Test]
        public void TakeDamage_FatalDamage_SetsDead()
        {
            _health.TakeDamage(_health.MaxHealth);
            Assert.IsTrue(_health.IsDead, "HP zero deve ser Dead.");
        }

        [Test]
        public void OnDeath_EventFired_WhenDead()
        {
            bool fired = false;
            _health.OnDeath += () => fired = true;
            _health.TakeDamage(_health.MaxHealth);
            Assert.IsTrue(fired, "Evento OnDeath deve ser disparado.");
        }

        [Test]
        public void TakeDamage_WhenDead_IsIgnored()
        {
            _health.InstantKill();
            // Qualquer tentativa de dano adicional não deve disparar evento novamente
            int deathCount = 0;
            _health.OnDeath += () => deathCount++;
            _health.TakeDamage(10f);
            Assert.AreEqual(0, deathCount, "Morto não deve tomar dano extra.");
        }

        // ─── Heal ─────────────────────────────────────────────────────────────

        [Test]
        public void Heal_RestoresHP()
        {
            _health.TakeDamage(50f);
            _health.Heal(20f);
            Assert.AreEqual(70f, _health.CurrentHealth, 0.001f,
                "Cura deve restaurar HP corretamente.");
        }

        [Test]
        public void Heal_CannotExceedMaxHealth()
        {
            _health.Heal(9999f);
            Assert.AreEqual(_health.MaxHealth, _health.CurrentHealth, 0.001f,
                "HP não pode ultrapassar MaxHealth.");
        }

        [Test]
        public void Heal_WhenDead_IsIgnored()
        {
            _health.InstantKill();
            _health.Heal(100f);
            Assert.IsTrue(_health.IsDead, "Morto não pode ser curado.");
        }

        [Test]
        public void FullHeal_RestoresToMax()
        {
            _health.TakeDamage(60f);
            _health.FullHeal();
            Assert.AreEqual(_health.MaxHealth, _health.CurrentHealth, 0.001f,
                "FullHeal deve restaurar HP ao máximo.");
        }

        // ─── Events ───────────────────────────────────────────────────────────

        [Test]
        public void OnHealthChanged_FiredOnDamage()
        {
            bool fired = false;
            _health.OnHealthChanged += (c, m) => fired = true;
            _health.TakeDamage(10f);
            Assert.IsTrue(fired, "OnHealthChanged deve ser disparado ao tomar dano.");
        }

        [Test]
        public void OnHealthChanged_FiredOnHeal()
        {
            bool fired = false;
            _health.TakeDamage(30f);
            _health.OnHealthChanged += (c, m) => fired = true;
            _health.Heal(10f);
            Assert.IsTrue(fired, "OnHealthChanged deve ser disparado ao curar.");
        }

        // ─── MaxHealth ────────────────────────────────────────────────────────

        [Test]
        public void SetMaxHealth_UpdatesMax()
        {
            _health.SetMaxHealth(200f);
            Assert.AreEqual(200f, _health.MaxHealth, 0.001f,
                "MaxHealth deve ser atualizado.");
        }

        [Test]
        public void SetMaxHealth_KeepsPercent()
        {
            _health.TakeDamage(50f); // 50/100 = 50%
            _health.SetMaxHealth(200f, keepPercent: true);
            Assert.AreEqual(100f, _health.CurrentHealth, 0.001f,
                "HP deve manter percentual após mudança de MaxHealth.");
        }

        // ─── HealthPercent ────────────────────────────────────────────────────

        [Test]
        public void HealthPercent_IsOne_WhenFull()
        {
            Assert.AreEqual(1f, _health.HealthPercent, 0.001f,
                "HealthPercent deve ser 1 quando cheio.");
        }

        [Test]
        public void HealthPercent_IsHalf_WhenHalfHP()
        {
            _health.TakeDamage(_health.MaxHealth / 2f);
            Assert.AreEqual(0.5f, _health.HealthPercent, 0.001f,
                "HealthPercent deve ser 0.5 com metade do HP.");
        }

        [Test]
        public void InstantKill_SetsHPToZero()
        {
            _health.InstantKill();
            Assert.AreEqual(0f, _health.CurrentHealth, 0.001f,
                "InstantKill deve zerar HP.");
        }
    }
}
