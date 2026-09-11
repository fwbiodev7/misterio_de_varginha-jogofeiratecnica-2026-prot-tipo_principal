using UnityEngine;
using Game.Player;

namespace Game.Level
{
    /// <summary>
    /// Obstáculo perigoso (espinhos, poço sem fundo, etc).
    /// Causa dano ou morte imediata ao jogador ao encostar.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hazard2D : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float damage = 25f;
        [SerializeField] private bool instantKill = false;
        [SerializeField] private string playerTag = "Player";

        private void Start()
        {
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag) && other.GetComponent<PlayerController>() == null) return;

            var health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                if (instantKill)
                {
                    health.InstantKill();
                    Debug.Log($"⚠️ [Hazard] Jogador caiu no abismo/morte instantânea: {name}");
                }
                else
                {
                    health.TakeDamage(damage);
                    Debug.Log($"⚠️ [Hazard] Jogador sofreu {damage} de dano por: {name}");
                }
            }
        }
    }
}
