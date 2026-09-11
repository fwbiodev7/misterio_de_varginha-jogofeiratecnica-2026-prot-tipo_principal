using System.Collections;
using UnityEngine;

namespace Game.Varginha
{
    // Mantém a animação de coleta independente do sistema de inventário.
    /// <summary>Anima a mochila do chão até as costas do Edelzio quando ela é coletada.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BackpackPickupAnimation : MonoBehaviour
    {
        [SerializeField] private float pickupDuration = .32f;
        private bool _isPickingUp;

        public void PlayPickup(EdelzioTopDownController player)
        {
            if (_isPickingUp || player == null) return;
            StartCoroutine(PickupRoutine(player));
        }

        private IEnumerator PickupRoutine(EdelzioTopDownController player)
        {
            _isPickingUp = true;
            var action = player.GetComponent<VarginhaPlayerActionAnimation>();
            if (action != null) yield return action.CrouchRoutine(.24f);
            var sprite = GetComponent<SpriteRenderer>();
            var collider = GetComponent<Collider2D>();
            Vector3 start = transform.position;
            Vector3 originalScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < pickupDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / pickupDuration);
                float arc = Mathf.Sin(t * Mathf.PI) * .45f;
                transform.position = Vector3.Lerp(start, player.transform.position, t) + Vector3.up * arc;
                transform.localScale = Vector3.Lerp(originalScale, originalScale * .45f, t);
                yield return null;
            }

            if (sprite != null) sprite.enabled = false;
            if (collider != null) collider.enabled = false;
            player.EquipBackpack();
        }
    }
}
