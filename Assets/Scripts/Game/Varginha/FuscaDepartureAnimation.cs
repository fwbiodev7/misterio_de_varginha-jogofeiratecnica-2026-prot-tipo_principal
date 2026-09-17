using System;
using System.Collections;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Animates the Fusca, then drives it off-screen.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FuscaDepartureAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] departureFrames;
        [SerializeField] private float frameRate = 11f;
        [SerializeField] private float startDuration = .7f;
        [SerializeField] private float driveSpeed = 12f;
        [SerializeField] private float exitDistance = 15f;
        [SerializeField] private float engineShake = .018f;
        [SerializeField] private float launchLean = 2.5f;

        private SpriteRenderer _renderer;
        private bool _isDeparting;

        private void Awake() => _renderer = GetComponent<SpriteRenderer>();

        public void Configure(Sprite[] frames)
        {
            departureFrames = frames;
            if (departureFrames != null && departureFrames.Length > 0 && _renderer != null)
                _renderer.sprite = departureFrames[0];
        }

        public void Depart(Action onComplete)
        {
            Depart(onComplete, -1f);
        }

        /// <summary>
        /// Faz uma saída curta quando a cinematics de viagem assumirá o restante
        /// do trajeto. Assim o carro não precisa atravessar a borda da fase real.
        /// </summary>
        public void Depart(Action onComplete, float distanceOverride)
        {
            if (_isDeparting) return;
            GetComponent<FuscaEngineSound>()?.PlayRev();
            StartCoroutine(DepartureRoutine(onComplete, distanceOverride));
        }

        private IEnumerator DepartureRoutine(Action onComplete, float distanceOverride)
        {
            _isDeparting = true;
            Vector3 startPosition = transform.position;
            Vector3 baseScale = transform.localScale;
            Quaternion baseRotation = transform.localRotation;
            float elapsed = 0f;
            float nextSmoke = 0f;
            while (elapsed < startDuration)
            {
                AnimateFrame(elapsed);
                ApplySuspensionMotion(startPosition, baseScale, baseRotation, elapsed, .35f);
                if (elapsed >= nextSmoke)
                {
                    SpawnExhaustSmoke();
                    nextSmoke = elapsed + UnityEngine.Random.Range(0.12f, 0.18f);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            float travelled = 0f;
            float distance = distanceOverride >= 0f ? distanceOverride : exitDistance;
            distance = Mathf.Max(0f, distance);
            float speed = Mathf.Max(.01f, driveSpeed);
            float direction = _renderer != null && _renderer.flipX ? -1f : 1f;
            nextSmoke = 0f;
            while (travelled < distance)
            {
                AnimateFrame(elapsed);
                float acceleration = Mathf.Lerp(.28f, 1f, Mathf.Clamp01(travelled / 4.5f));
                travelled = Mathf.Min(distance, travelled + speed * acceleration * Time.deltaTime);
                float launch = Mathf.Clamp01(travelled / 2.2f);
                ApplySuspensionMotion(startPosition + Vector3.right * (direction * travelled),
                    baseScale, baseRotation, elapsed, Mathf.Lerp(.35f, 0f, launch));
                if (elapsed >= nextSmoke && travelled < distance * 0.7f)
                {
                    SpawnExhaustSmoke();
                    nextSmoke = elapsed + 0.14f;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = baseScale;
            transform.localRotation = baseRotation;
            transform.position = startPosition + Vector3.right * (direction * distance);
            onComplete?.Invoke();
        }

        private void ApplySuspensionMotion(Vector3 basePosition, Vector3 baseScale, Quaternion baseRotation,
            float elapsed, float strength)
        {
            float bob = Mathf.Sin(elapsed * 38f) * engineShake * strength;
            float lean = Mathf.Sin(elapsed * 22f) * launchLean * strength / 90f;
            // A suspensão é visual; a posição central não sai da faixa da estrada.
            transform.position = basePosition;
            transform.localScale = new Vector3(baseScale.x * (1f + bob), baseScale.y * (1f - bob * .55f), baseScale.z);
            // A carroceria não gira mais em torno do centro durante a saída. Isso
            // mantinha as rodas fora da faixa e fazia passageiros/porta orbitarem.
            transform.localRotation = baseRotation;
            transform.localScale = new Vector3(transform.localScale.x * (1f + lean),
                transform.localScale.y * (1f - lean * .55f), transform.localScale.z);
        }

        private void AnimateFrame(float elapsed)
        {
            if (departureFrames == null || departureFrames.Length == 0 || _renderer == null) return;
            _renderer.sprite = departureFrames[Mathf.FloorToInt(elapsed * frameRate) % departureFrames.Length];
        }

        private void SpawnExhaustSmoke()
        {
            float dir = _renderer != null && _renderer.flipX ? -1f : 1f;
            Vector3 exhaustPos = transform.position + new Vector3(-dir * 1.15f, -0.28f, 0f);
            var puff = new GameObject("Fusca_Fumaca_Escapamento");
            puff.transform.position = exhaustPos + (Vector3)UnityEngine.Random.insideUnitCircle * 0.05f;
            puff.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.45f, 0.70f);
            var sr = puff.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create("Dodge_Dust", new Color(.78f, .82f, .85f, .75f));
            sr.sortingOrder = _renderer != null ? _renderer.sortingOrder - 1 : 4;
            Destroy(puff, 0.35f);
        }
    }
}
