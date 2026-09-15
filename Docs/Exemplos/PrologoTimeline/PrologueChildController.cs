using UnityEngine;

namespace Game.Varginha.Prologue
{
    // Exemplo do guia: copiar para Assets somente ao implementar o prologo.
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PrologueChildController : MonoBehaviour
    {
        [SerializeField] private Animator visualAnimator;
        [SerializeField, Min(0.1f)] private float walkSpeed = 2.2f;
        [SerializeField, Min(0.1f)] private float runSpeed = 3.4f;

        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private Rigidbody2D _body;
        private Vector2 _input;
        private float _speed;
        private bool _cinematic;
        private bool _wasSimulated;

        public float InputStrength { get; set; } = 1f;
        public bool CanInteract => !_cinematic && InputStrength >= 0.99f
            && Time.timeScale > 0f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            if (visualAnimator == null)
            {
                Debug.LogError("Associe o Animator do objeto Visual.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (_cinematic) return;
            if (Time.timeScale <= 0f)
            {
                _input = Vector2.zero;
                return;
            }

            float x = (VarginhaInputBindings.IsPressed(VarginhaInputAction.MoveRight) ? 1f : 0f)
                - (VarginhaInputBindings.IsPressed(VarginhaInputAction.MoveLeft) ? 1f : 0f);
            float y = (VarginhaInputBindings.IsPressed(VarginhaInputAction.MoveUp) ? 1f : 0f)
                - (VarginhaInputBindings.IsPressed(VarginhaInputAction.MoveDown) ? 1f : 0f);
            _input = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            _speed = VarginhaInputBindings.IsPressed(VarginhaInputAction.Run)
                ? runSpeed : walkSpeed;
            visualAnimator.SetFloat(SpeedId, _input.magnitude * Mathf.Clamp01(InputStrength));
        }

        private void FixedUpdate()
        {
            if (!_cinematic)
                _body.linearVelocity = _input * (_speed * Mathf.Clamp01(InputStrength));
        }

        public void BeginCinematic()
        {
            if (_cinematic) return;
            _cinematic = true;
            _input = Vector2.zero;
            _wasSimulated = _body.simulated;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
        }

        public void ResumeGameplay()
        {
            if (_cinematic) _body.simulated = _wasSimulated;
            _cinematic = false;
            _input = Vector2.zero;
            InputStrength = 1f;
            _body.linearVelocity = Vector2.zero;
            visualAnimator.SetFloat(SpeedId, 0f);
        }

        private void OnDisable()
        {
            if (_body != null && _body.simulated) _body.linearVelocity = Vector2.zero;
        }
    }
}
