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
            float elapsed = 0f;
            while (elapsed < startDuration)
            {
                AnimateFrame(elapsed);
                // Tremor curto de motor antes da arrancada.
                float shake = Mathf.Sin(elapsed * 42f) * .025f * (elapsed / startDuration);
                transform.position = startPosition + new Vector3(shake, 0f, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            float travelled = 0f;
            while (travelled < exitDistance)
            {
                AnimateFrame(elapsed);
                float acceleration = Mathf.Lerp(.28f, 1f, Mathf.Clamp01(travelled / 4.5f));
                float step = driveSpeed * acceleration * Time.deltaTime;
                float wheelBob = Mathf.Sin(elapsed * 16f) * .018f;
                transform.position += new Vector3(step, wheelBob, 0f);
                travelled += step;
                elapsed += Time.deltaTime;
                yield return null;
            }
            onComplete?.Invoke();
        }

        private void AnimateFrame(float elapsed)
        {
            if (departureFrames == null || departureFrames.Length == 0 || _renderer == null) return;
            _renderer.sprite = departureFrames[Mathf.FloorToInt(elapsed * frameRate) % departureFrames.Length];
        }
    }
}
