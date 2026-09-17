using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Distance-driven four-direction walk. Stops on a stable pose and never stretches pixels.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VarginhaStudentAnimation : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private string _student;
        private Vector3 _previous;
        private float _stride;
        private int _direction;
        public int Facing => _direction;
        public int FrameIndex { get; private set; }

        public void Configure(string student)
        {
            _student = student;
            _renderer = GetComponent<SpriteRenderer>();
            _previous = transform.position;
            _stride = 0;
            _direction = 0;
            Present(0);
        }
        private void OnEnable() => _previous = transform.position;

        private void LateUpdate()
        {
            var movement = (Vector2)(transform.position - _previous);
            _previous = transform.position;
            if (_renderer == null || !_renderer.enabled || string.IsNullOrEmpty(_student)) return;
            if (Time.timeScale <= 0f) return;
            float distance = movement.magnitude;
            if (distance > .002f && distance < .75f)
            {
                // A small hysteresis prevents diagonal steering from flickering left/up.
                Vector2 facing = _direction == 1 ? Vector2.left : _direction == 2 ? Vector2.right
                    : _direction == 3 ? Vector2.up : Vector2.down;
                if (Vector2.Dot(facing, movement.normalized) < .6f)
                    _direction = VarginhaStudentSprites.Direction(movement);
                _stride += distance * 7f;
                Present(Mathf.FloorToInt(_stride) & 3);
            }
            else
            {
                _stride = 0;
                Present(1);
            }
        }

        private void Present(int frame)
        {
            var sprite = VarginhaStudentSprites.Frame(_student, _direction, frame);
            if (sprite == null) return;
            FrameIndex = frame;
            _renderer.sprite = sprite;
            _renderer.flipX = false;
        }
    }
}
