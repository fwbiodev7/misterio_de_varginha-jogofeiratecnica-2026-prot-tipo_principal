using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Tremor curto aplicado depois do impacto sem interferir na câmera que segue o jogador.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class VarginhaCameraShake : MonoBehaviour
    {
        private float _time;
        private float _duration;
        private float _magnitude;
        private Vector3 _offset;
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        public void Shake(float duration, float magnitude)
        {
            if (duration <= 0f || magnitude <= 0f || !isActiveAndEnabled) return;
            if (_time <= 0f) { _duration = 0f; _magnitude = 0f; }
            _duration = Mathf.Max(_duration, duration);
            _time = Mathf.Max(_time, duration);
            _magnitude = Mathf.Max(_magnitude, magnitude);
        }

        private void Update()
        {
            // CameraFollow2D lê a posição anterior em LateUpdate. Retirar o deslocamento
            // antes desse cálculo evita incorporá-lo ao SmoothDamp e produzir deriva.
            transform.position -= _offset;
            _offset = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (_time <= 0f)
            {
                _duration = 0f;
                _magnitude = 0f;
                return;
            }

            float strength = Mathf.Clamp01(_time / Mathf.Max(.001f, _duration));
            _offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * _magnitude * strength;
            // Quantiza apenas o efeito em pixels da tela, preservando o enquadramento
            // e as posições físicas dos sprites com diferentes pixels por unidade.
            if (_camera != null && _camera.orthographic && _camera.pixelHeight > 0)
            {
                float pixel = _camera.orthographicSize * 2f / _camera.pixelHeight;
                _offset.x = Mathf.Round(_offset.x / pixel) * pixel;
                _offset.y = Mathf.Round(_offset.y / pixel) * pixel;
            }
            transform.position += _offset;
            _time = Mathf.Max(0f, _time - Time.unscaledDeltaTime);
        }

        private void OnDisable()
        {
            transform.position -= _offset;
            _offset = Vector3.zero;
            _time = _duration = _magnitude = 0f;
        }
    }
}
