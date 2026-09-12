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
            if (_isDeparting) return;
            GetComponent<FuscaEngineSound>()?.PlayRev();
            StartCoroutine(DepartureRoutine(onComplete));
        }

        private IEnumerator DepartureRoutine(Action onComplete)
        {
            _isDeparting = true;
            Vector3 startPosition = transform.position;
            Vector3 baseScale = transform.localScale;
            Quaternion baseRotation = transform.localRotation;
            float elapsed = 0f;
            while (elapsed < startDuration)
            {
                AnimateFrame(elapsed);
                // Os quadros dão vida ao motor sem deslocar o carro e seus passageiros.
                transform.position = startPosition;
                ApplySuspensionMotion(baseScale, baseRotation, elapsed, .35f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            float travelled = 0f;
            float distance = Mathf.Max(0f, exitDistance);
            float speed = Mathf.Max(.01f, driveSpeed);
            float direction = _renderer != null && _renderer.flipX ? -1f : 1f;
            while (travelled < distance)
            {
                AnimateFrame(elapsed);
                float acceleration = Mathf.Lerp(.28f, 1f, Mathf.Clamp01(travelled / 4.5f));
                travelled = Mathf.Min(distance, travelled + speed * acceleration * Time.deltaTime);
                // Posição absoluta: Y/Z não acumulam oscilação nem variam com o FPS.
                transform.position = startPosition + Vector3.right * (direction * travelled);
                float launch = Mathf.Clamp01(travelled / 2.2f);
                ApplySuspensionMotion(baseScale, baseRotation, elapsed, Mathf.Lerp(.35f, 0f, launch));
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = baseScale;
            transform.localRotation = baseRotation;
            transform.position = startPosition + Vector3.right * (direction * distance);
            onComplete?.Invoke();
        }

        private void ApplySuspensionMotion(Vector3 baseScale, Quaternion baseRotation, float elapsed, float strength)
        {
            float bob = Mathf.Sin(elapsed * 38f) * engineShake * strength;
            float lean = Mathf.Sin(elapsed * 22f) * launchLean * strength;
            transform.localScale = new Vector3(baseScale.x * (1f + bob), baseScale.y * (1f - bob * .55f), baseScale.z);
            transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, lean);
        }

        private void AnimateFrame(float elapsed)
        {
            if (departureFrames == null || departureFrames.Length == 0 || _renderer == null) return;
            _renderer.sprite = departureFrames[Mathf.FloorToInt(elapsed * frameRate) % departureFrames.Length];
        }
    }
}
