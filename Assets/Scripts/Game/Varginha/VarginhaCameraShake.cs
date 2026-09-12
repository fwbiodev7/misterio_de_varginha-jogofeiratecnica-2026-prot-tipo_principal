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

        public void Shake(float duration, float magnitude)
        {
            _duration = Mathf.Max(_duration, duration);
            _time = Mathf.Max(_time, duration);
            _magnitude = Mathf.Max(_magnitude, magnitude);
        }

        private void LateUpdate()
        {
            transform.position -= _offset;
            if (_time <= 0f)
            {
                _offset = Vector3.zero;
                return;
            }

            float strength = Mathf.Clamp01(_time / Mathf.Max(.001f, _duration));
            _offset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * _magnitude * strength;
            transform.position += _offset;
            _time -= Time.unscaledDeltaTime;
        }
    }
}
