using System.Collections;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Item físico da Lanterna que salta do baú na primeira fase.
    /// Possui física 2D (Rigidbody2D + Collider2D), brilho e pode ser coletado pelo Edelzio.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class FlashlightWorldPickup : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private CircleCollider2D _col;
        private bool _canCollect;
        private bool _collected;
        private float _spawnTime;

        public static FlashlightWorldPickup SpawnFromChest(Vector3 chestPos)
        {
            var go = new GameObject("Flashlight_World_Pickup");
            go.transform.position = chestPos + new Vector3(0.15f, -0.1f, 0f);
            var pickup = go.AddComponent<FlashlightWorldPickup>();
            return pickup;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<CircleCollider2D>();

            // Configuração visual
            _sr.sprite = VarginhaPixelArtSprites.Create("Inventory_Flashlight", Color.white);
            _sr.sortingOrder = 7;
            transform.localScale = new Vector3(0.75f, 0.75f, 1f);

            // Configuração de física
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 0f;
            _rb.linearDamping = 3.5f;
            _rb.angularDamping = 2.5f;

            // Impulso físico saltando do baú para a frente
            float angle = Random.Range(-45f, 45f) * Mathf.Deg2Rad;
            Vector2 popDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) - 0.4f).normalized;
            _rb.linearVelocity = popDir * Random.Range(1.8f, 2.6f);
            _rb.angularVelocity = Random.Range(-180f, 180f);

            // Colisor de gatilho para coleta
            _col.isTrigger = true;
            _col.radius = 0.5f;

            _spawnTime = Time.time;
            StartCoroutine(EnableCollectionRoutine());
        }

        private IEnumerator EnableCollectionRoutine()
        {
            // Breve intervalo para o jogador ver a lanterna saltar do baú com física
            yield return new WaitForSeconds(0.28f);
            _canCollect = true;
        }

        private void Update()
        {
            if (_collected) return;

            // Efeito visual sutil de brilho pulsante no chão
            float pulse = 0.85f + 0.15f * Mathf.Sin((Time.time - _spawnTime) * 4f);
            _sr.color = new Color(1f, 1f, pulse, 1f);

            // Permite coleta com aproximação ou tecla de interação E
            if (_canCollect)
            {
                var edelzio = FindAnyObjectByType<EdelzioTopDownController>();
                if (edelzio != null)
                {
                    float dist = Vector2.Distance(transform.position, edelzio.transform.position);
                    if (dist < 0.75f)
                    {
                        Collect(edelzio);
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_canCollect || _collected) return;
            var edelzio = other.GetComponent<EdelzioTopDownController>();
            if (edelzio != null)
            {
                Collect(edelzio);
            }
        }

        public void Collect(EdelzioTopDownController edelzio)
        {
            if (_collected || edelzio == null) return;
            _collected = true;

            edelzio.HasFlashlight = true;
            edelzio.IsFlashlightEquippedInHotbar = false;

            // Notificação ao jogador
            var hud = VarginhaGameHUD.Instance ?? FindAnyObjectByType<VarginhaGameHUD>();
            hud?.ShowDialogue("Edelzio", "🔦 Você pegou a LANTERNA!\nEla foi guardada na sua Mochila.\nAbra a Mochila [G] para EQUIPÁ-LA na Hotbar e poder usá-la!");

            StartCoroutine(CollectAnimationRoutine(edelzio));
        }

        private IEnumerator CollectAnimationRoutine(EdelzioTopDownController edelzio)
        {
            _col.enabled = false;
            if (_rb != null) { _rb.linearVelocity = Vector2.zero; _rb.simulated = false; }

            Vector3 startPos = transform.position;
            Vector3 startScale = transform.localScale;
            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float arc = Mathf.Sin(t * Mathf.PI) * 0.35f;
                transform.position = Vector3.Lerp(startPos, edelzio.transform.position, t) + Vector3.up * arc;
                transform.localScale = Vector3.Lerp(startScale, startScale * 0.3f, t);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
