using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    /// <summary>
    /// Controla o movimento do jogador usando o novo Input System.
    /// Suporta: andar, pular, correr, agachar e dash.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float runMultiplier = 1.6f;
        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float deceleration = 20f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 16f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.1f;
        [SerializeField] private Transform groundCheck;

        [Header("Dash")]
        [SerializeField] private float dashForce = 20f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.8f;

        // State
        private Rigidbody2D _rb;
        private HealthSystem _health;
        private Vector2 _moveInput;
        private bool _isGrounded;
        private bool _isRunning;
        private bool _isDashing;
        private bool _jumpConsumed;
        private int _jumpsLeft;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private float _facingDirection = 1f;

        // Properties (read by tests)
        public bool IsGrounded => _isGrounded;
        public bool IsDashing => _isDashing;
        public int JumpsLeft => _jumpsLeft;
        public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;
        public float FacingDirection => _facingDirection;
        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<HealthSystem>();
            _jumpsLeft = maxJumps;

            if (groundLayer.value == 0)
            {
                groundLayer = ~(1 << gameObject.layer);
            }
        }

        private void Update()
        {
            CheckGround();
            HandleDashTimer();
            PollKeyboardInput();
        }

        private void PollKeyboardInput()
        {
            if (Keyboard.current == null) return;

            float x = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;

            if (x != 0f || _moveInput.y == 0f)
            {
                _moveInput = new Vector2(x, _moveInput.y);
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                ExecuteJump();
            }

            if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            {
                _isRunning = true;
            }
            else if (!Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.rightShiftKey.isPressed && x == 0f)
            {
                _isRunning = false;
            }

            if ((Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.kKey.wasPressedThisFrame) && _dashCooldownTimer <= 0f)
            {
                StartDash();
            }
        }

        private void FixedUpdate()
        {
            if (_isDashing) return;
            ApplyMovement();
            ApplyBetterGravity();
        }

        // ─── Input System callbacks ───────────────────────────────────────────

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (!value.isPressed) return;
            ExecuteJump();
        }

        private void ExecuteJump()
        {
            if (_jumpsLeft > 0)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _jumpsLeft--;
                _jumpConsumed = true;
            }
        }

        public void OnRun(InputValue value)
        {
            _isRunning = value.isPressed;
        }

        public void OnDash(InputValue value)
        {
            if (!value.isPressed) return;
            if (_dashCooldownTimer > 0f) return;
            StartDash();
        }

        // ─── Public API (used by tests and other systems) ─────────────────────

        /// <summary>Simula movimento diretamente (útil em testes).</summary>
        public void SimulateMove(Vector2 direction)
        {
            _moveInput = direction;
        }

        /// <summary>Simula pulo diretamente (útil em testes).</summary>
        public void SimulateJump()
        {
            if (_jumpsLeft > 0)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                _jumpsLeft--;
            }
        }

        /// <summary>Retorna true se o player está no chão.</summary>
        public bool CheckGroundPublic()
        {
            return CheckGroundAt(groundCheck != null ? groundCheck.position : transform.position);
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private void CheckGround()
        {
            Vector2 checkPos = groundCheck != null ? groundCheck.position : transform.position - Vector3.up * 0.5f;
            bool wasGrounded = _isGrounded;
            _isGrounded = CheckGroundAt(checkPos);

            if (_isGrounded && !wasGrounded)
            {
                _jumpsLeft = maxJumps;
                _jumpConsumed = false;
            }
        }

        private bool CheckGroundAt(Vector2 position)
        {
            return Physics2D.OverlapCircle(position, groundCheckRadius, groundLayer);
        }

        private void ApplyMovement()
        {
            float targetSpeed = _moveInput.x * moveSpeed * (_isRunning ? runMultiplier : 1f);
            float speedDiff = targetSpeed - _rb.linearVelocity.x;
            float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            float force = speedDiff * rate;
            _rb.AddForce(Vector2.right * force, ForceMode2D.Force);

            if (Mathf.Abs(_moveInput.x) > 0.01f)
                _facingDirection = Mathf.Sign(_moveInput.x);
        }

        private void ApplyBetterGravity()
        {
            if (_rb.linearVelocity.y < 0)
                _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            else if (_rb.linearVelocity.y > 0 && !_jumpConsumed)
                _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;
            _rb.linearVelocity = new Vector2(_facingDirection * dashForce, 0f);
        }

        private void HandleDashTimer()
        {
            if (_dashCooldownTimer > 0f)
                _dashCooldownTimer -= Time.deltaTime;

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f)
                    _isDashing = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
