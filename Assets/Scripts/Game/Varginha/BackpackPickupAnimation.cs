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
        private EdelzioTopDownController _collectingPlayer;

        private void Awake()
        {
            GetComponent<SpriteRenderer>().sprite = VarginhaHouseReferenceArt.PropSprite(this, "Backpack_Prop")
                ?? VarginhaPixelArtSprites.Create("Backpack_Prop", Color.gray);
        }

        public void PlayPickup(EdelzioTopDownController player, System.Action onComplete = null)
        {
            if (_isPickingUp || player == null) return;
            StartCoroutine(PickupRoutine(player, onComplete));
        }

        private IEnumerator PickupRoutine(EdelzioTopDownController player, System.Action onComplete = null)
        {
            _isPickingUp = true;
            _collectingPlayer = player;
            foreach (var hitbox in GetComponents<Collider2D>()) hitbox.enabled = false;
            var body = GetComponent<Rigidbody2D>();
            if (body != null) { body.linearVelocity = Vector2.zero; body.simulated = false; }

            var action = player.GetComponent<VarginhaPlayerActionAnimation>();
            if (action != null) yield return action.CrouchRoutine(.24f);

            player.SetInputLocked(true);
            var sprite = GetComponent<SpriteRenderer>();
            var collider = GetComponent<Collider2D>();
            Vector3 start = transform.position;
            Vector3 originalScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < pickupDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / pickupDuration);
                // Smooth ease-in/out using sine so the bag decelerates as it lands.
                float tEased = Mathf.SmoothStep(0f, 1f, t);
                // Arc peaks at mid-flight and lands slightly above the player centre (upper-back).
                float arc = Mathf.Sin(t * Mathf.PI) * .25f;
                Vector3 destination = player.transform.position + new Vector3(0f, .18f, 0f);
                transform.position = Vector3.Lerp(start, destination, tEased) + Vector3.up * arc;
                // Scale shrinks to zero so the item visually merges into the character.
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, tEased);
                yield return null;
            }

            if (sprite != null) sprite.enabled = false;
            if (collider != null) collider.enabled = false;

            try
            {
                player.EquipBackpack();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BackpackPickupAnimation] Erro ao equipar mochila: {ex.Message}");
            }
            finally
            {
                player.SetInputLocked(false);
                _collectingPlayer = null;
                onComplete?.Invoke();
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (_collectingPlayer != null)
            {
                try { _collectingPlayer.EquipBackpack(); } catch { }
                _collectingPlayer.SetInputLocked(false);
                _collectingPlayer = null;
            }
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }
}
