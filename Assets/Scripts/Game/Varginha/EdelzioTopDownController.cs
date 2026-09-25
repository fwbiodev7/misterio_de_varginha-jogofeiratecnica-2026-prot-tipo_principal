using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Varginha
{
    /// <summary>
    /// Controlador Top-Down estilo Pokémon / RPG clássico de investigação para Edelzio.
    /// Movimento em 4/8 direções, corrida, interação com objetos e controle de sanidade/medo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EdelzioTopDownController : MonoBehaviour
    {
        public static EdelzioTopDownController Instance { get; private set; }

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float runSpeed = 7.5f;
        [SerializeField] private float acceleration = 25f;

        [Header("Sanity & Fear")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float currentSanity = 100f;

        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 1.8f;
        [SerializeField] private LayerMask interactableLayer;

        // Inventory State
        [SerializeField] private bool hasBackpack;
        public bool HasBackpack
        {
            get => hasBackpack;
            set
            {
                hasBackpack = value;
                GetComponent<VarginhaPlayerSpriteAnimation>()?.RefreshEquipmentAppearance();
            }
        }
        public bool IsBackpackVisible => hasBackpack && _carriedItemsVisible;
        public bool HasFuscaKey { get; set; }
        public bool HasResearchNotebook { get; set; }
        public bool HasDecodedData { get; set; }
        public bool HasHistoricalDocument { get; set; }
        public bool HasFlashlight { get; set; }
        public bool FlashlightActive { get; private set; }
        // Knowledge survives consumption; the hotbar only shows physical items still in use.
        private int _usedInventoryItems;

        public bool TryConsumeInventoryItem(int slot)
        {
            if (slot <= 0 || !HasInventoryItem(slot)) return false;
            _usedInventoryItems |= 1 << slot;
            if (slot == 3 && _heldNotebook != null) _heldNotebook.gameObject.SetActive(false);
            return true;
        }

        public void CompleteHouseInventory()
        {
            // Also supports starting the school/church directly in the Editor.
            // Do not respawn the key or documents when a new player is constructed.
            _usedInventoryItems |= (1 << 1) | (1 << 2) | (1 << 3) | (1 << 4);
            if (_heldNotebook != null) _heldNotebook.gameObject.SetActive(false);
        }

        // State
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Vector2 _moveInput;
        private Vector2 _lastFacing = Vector2.down;
        private bool _isRunning;
        private bool _inputLocked;
        private bool _combatLocked;
        private InteractableProp _nearestInteractable;
        private Transform _heldNotebook;
        private bool _carriedItemsVisible = true;
        private PhysicsMaterial2D _movementMaterial;
        private float _dodgeRemaining;
        private float _dodgeCooldown;
        private Vector2 _dodgeDirection;
        private float _dodgeTrailTimer;

        public bool IsDodging => _dodgeRemaining > 0f;
        public float DodgeCooldownRemaining => Mathf.Max(0f, _dodgeCooldown);
        private bool IsGameplayBlocked => IsInputLocked || Time.timeScale <= 0f || currentSanity <= 0f
            || GetComponent<Game.Player.HealthSystem>()?.IsDead == true
            || VarginhaGameHUD.Instance?.IsDialogueOpen == true
            || VarginhaGameHUD.Instance?.IsVictoryOpen == true;

        public bool TryDodge(Vector2 direction)
        {
            if (!isActiveAndEnabled || IsGameplayBlocked || _dodgeCooldown > 0f || IsScriptedMotion) return false;
            _dodgeDirection = direction.sqrMagnitude > .01f ? direction.normalized : _lastFacing.normalized;
            _dodgeRemaining = .18f;
            _dodgeCooldown = 1.1f;
            _dodgeTrailTimer = 0f;
            return true;
        }

        public bool HasInventoryItem(int slot)
        {
            if (slot < 0 || slot > 5 || (_usedInventoryItems & (1 << slot)) != 0) return false;
            switch (slot)
            {
                case 0: return HasBackpack;
                case 1: return HasFuscaKey;
                case 2: return HasResearchNotebook;
                case 3: return HasDecodedData;
                case 4: return HasHistoricalDocument;
                case 5: return HasFlashlight;
                default: return false;
            }
        }

        public void ToggleFlashlight()
        {
            if (!HasFlashlight) return;
            FlashlightActive = !FlashlightActive;
            var flashlight = GetComponent<EdelzioFlashlight>();
            if (flashlight != null) flashlight.enabled = FlashlightActive;
        }

        public float CurrentSanity => currentSanity;
        public float MaxSanity => maxSanity;
        public Vector2 FacingDirection => _lastFacing;
        public bool IsRunning => _isRunning;
        public bool IsInputLocked => _inputLocked || _combatLocked || VarginhaGameHUD.Instance?.BlocksGameplayInput == true;
        public void SetCombatLocked(bool locked)
        {
            _combatLocked = locked;
            if (locked) { _moveInput = Vector2.zero; if (_rb != null) _rb.linearVelocity = Vector2.zero; }
        }
        public bool IsScriptedMotion { get; set; }
        public bool IsMoving => _moveInput.sqrMagnitude > .01f;
        public InteractableProp NearestInteractable => _nearestInteractable;

        public event Action<float> OnSanityChanged;
        public event Action OnEdelzioPanicDeath;

        private void Awake()
        {
            Instance = this;
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();

            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            // Sem atrito lateral: segurar uma diagonal contra a parede não prende o jogador.
            _movementMaterial = new PhysicsMaterial2D("Edelzio_Movimento") { friction = 0f, bounciness = 0f };
            foreach (var collider in GetComponents<Collider2D>()) collider.sharedMaterial = _movementMaterial;

            currentSanity = maxSanity;
        }

        private void Update()
        {
            if (!IsGameplayBlocked)
            {
                _dodgeCooldown = Mathf.Max(0f, _dodgeCooldown - Time.deltaTime);
                _dodgeRemaining = Mathf.Max(0f, _dodgeRemaining - Time.deltaTime);
                if (IsDodging)
                {
                    _dodgeTrailTimer -= Time.deltaTime;
                    if (_dodgeTrailTimer <= 0f)
                    {
                        _dodgeTrailTimer = .05f;
                        var dust = new GameObject("Poeira_Esquiva");
                        dust.transform.position = transform.position + new Vector3(0f, -.35f, 0f);
                        var dustRenderer = dust.AddComponent<SpriteRenderer>();
                        dustRenderer.sprite = VarginhaPixelArtSprites.Create("Dodge_Dust", new Color(.5f, .85f, .95f));
                        dustRenderer.sortingOrder = _sr != null ? _sr.sortingOrder - 1 : 4;
                        Destroy(dust, .18f);
                    }
                }
            }
            else if (Time.timeScale > 0f) _dodgeRemaining = 0f;
            PollKeyboard();
            ScanInteractables();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void PollKeyboard()
        {
            if (IsGameplayBlocked)
            {
                _moveInput = Vector2.zero;
                if (!IsScriptedMotion) _rb.linearVelocity = Vector2.zero;
                return;
            }
            float x = 0f;
            float y = 0f;
            _isRunning = false;

            if (VarginhaInputBindings.IsPressed(VarginhaInputAction.MoveLeft)) x -= 1f;
            if (VarginhaInputBindings.IsPressed(VarginhaInputAction.MoveRight)) x += 1f;
            if (VarginhaInputBindings.IsPressed(VarginhaInputAction.MoveDown)) y -= 1f;
            if (VarginhaInputBindings.IsPressed(VarginhaInputAction.MoveUp)) y += 1f;

            _isRunning = VarginhaInputBindings.IsPressed(VarginhaInputAction.Run);

            if (VarginhaInputBindings.WasPressedThisFrame(VarginhaInputAction.Interact))
                TryInteract();

            // V key toggles the flashlight on/off
            var kb = Keyboard.current;
            if (kb != null && kb.vKey.wasPressedThisFrame && HasFlashlight)
                ToggleFlashlight();

            if (IsGameplayBlocked) return;
            _moveInput = new Vector2(x, y).normalized;

            if (VarginhaInputBindings.WasPressedThisFrame(VarginhaInputAction.Dodge)) TryDodge(_moveInput);

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                _lastFacing = _moveInput;
            }
        }

        private void Move()
        {
            if (IsScriptedMotion) return;
            if (IsGameplayBlocked) { _rb.linearVelocity = Vector2.zero; return; }
            if (IsDodging) { _rb.linearVelocity = _dodgeDirection * 12f; return; }
            float speed = _isRunning ? runSpeed : walkSpeed;
            Vector2 targetVel = _moveInput * speed;
            // Com a configuração padrão a resposta é imediata; valores menores
            // permitem uma entrada mais macia sem deixar Edelzio escorregar ao inverter.
            float response = Mathf.Clamp01(acceleration / 25f);
            _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, targetVel, response);
        }

        private void OnDisable()
        {
            _dodgeRemaining = _dodgeCooldown = 0f;
            _moveInput = Vector2.zero;
            _isRunning = false;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (_movementMaterial != null) Destroy(_movementMaterial);
            if (Instance == this) Instance = null;
        }

        private void ScanInteractables()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
            InteractableProp closest = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var prop = hit.GetComponent<InteractableProp>();
                if (prop != null && prop.CanInteract)
                {
                    float d = Vector2.Distance(transform.position, prop.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        closest = prop;
                    }
                }
            }

            _nearestInteractable = closest;
        }

        public void TryInteract()
        {
            if (IsGameplayBlocked || IsDodging) return;
            if (_nearestInteractable != null)
            {
                _nearestInteractable.Interact(this);
            }
        }

        public void DrainSanity(float amount)
        {
            currentSanity = Mathf.Max(0f, currentSanity - amount);
            OnSanityChanged?.Invoke(currentSanity);

            if (currentSanity <= 0f)
            {
                OnEdelzioPanicDeath?.Invoke();
                Debug.LogWarning("💀 [Edelzio] O pânico e a entidade consumiram a mente de Edelzio!");
            }
        }

        public void RestoreSanity(float amount)
        {
            currentSanity = Mathf.Min(maxSanity, currentSanity + amount);
            OnSanityChanged?.Invoke(currentSanity);
        }

        /// <summary>O café recupera exatamente um dos três corações do HUD.</summary>
        public void RestoreOneHeart()
        {
            RestoreSanity(maxSanity / 3f);
        }

        public void SetInputLocked(bool locked)
        {
            _inputLocked = locked;
            if (locked)
            {
                _moveInput = Vector2.zero;
                _isRunning = false;
                if (_rb != null) _rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>Ativa a mochila como parte da aparência e do estado de Edelzio.</summary>
        public void EquipBackpack()
        {
            HasBackpack = true;
        }

        /// <summary>Depois da decodificação, Edelzio carrega o notebook sob o braço.</summary>
        public void EquipNotebook()
        {
            if (_heldNotebook != null) return;

            var notebook = new GameObject("Notebook_Sob_o_Braco");
            notebook.transform.SetParent(transform, false);
            notebook.transform.localPosition = new Vector3(.22f, -.08f, 0f);
            notebook.transform.localScale = new Vector3(.38f, .38f, 1f);
            var renderer = notebook.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Notebook_Held", Color.gray);
            renderer.sortingOrder = _sr != null ? _sr.sortingOrder + 2 : 7;
            _heldNotebook = notebook.transform;
        }

        /// <summary>Esconde os itens carregados quando Edelzio entra na cabine.</summary>
        public void SetCarriedItemsVisible(bool visible)
        {
            _carriedItemsVisible = visible;
            SetChildSpriteVisible(_heldNotebook, visible);
            GetComponent<VarginhaPlayerSpriteAnimation>()?.RefreshEquipmentAppearance();
        }

        private static void SetChildSpriteVisible(Transform item, bool visible)
        {
            if (item == null) return;
            var renderer = item.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = visible;
        }

        private void LateUpdate()
        {
            if (_heldNotebook != null)
            {
                var animation = GetComponent<VarginhaPlayerSpriteAnimation>();
                Vector2 facing = animation != null && animation.HasActionPose ? animation.ActionFacingDirection : _lastFacing;
                float absX = Mathf.Abs(facing.x);
                float absY = Mathf.Abs(facing.y);
                bool vertical = absY >= absX;
                bool facingUp = vertical && facing.y > .05f;
                bool facingLeft = !vertical && facing.x < 0f;
                // Mantém o notebook colado à mão, sem atravessar cabeça ou pés.
                Vector3 targetPosition = vertical
                    ? new Vector3(.16f, -.205f, 0f)
                    : new Vector3(facingLeft ? -.19f : .19f, -.19f, 0f);
                // Attach directly: interpolating across the torso on a turn makes the item float.
                _heldNotebook.localPosition = targetPosition;
                _heldNotebook.localScale = new Vector3(vertical ? .22f : .17f, .25f, 1f);
                _heldNotebook.localRotation = Quaternion.Euler(0, 0, vertical ? -8 : facingLeft ? -12 : 12);
                var renderer = _heldNotebook.GetComponent<SpriteRenderer>();
                renderer.sortingLayerID = _sr.sortingLayerID;
                renderer.sortingOrder = _sr.sortingOrder + (facingUp ? -2 : 2);
                renderer.flipX = facingLeft;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
