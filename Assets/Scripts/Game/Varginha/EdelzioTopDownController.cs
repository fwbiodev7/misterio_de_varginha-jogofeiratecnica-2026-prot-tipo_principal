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
        public bool HasBackpack { get; set; }
        public bool HasFuscaKey { get; set; }
        public bool HasResearchNotebook { get; set; }
        public bool HasDecodedData { get; set; }
        public bool HasHistoricalDocument { get; set; }

        // State
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Vector2 _moveInput;
        private Vector2 _lastFacing = Vector2.down;
        private bool _isRunning;
        private bool _inputLocked;
        private bool _combatLocked;
        private InteractableProp _nearestInteractable;
        private Transform _equippedBackpack;
        private Transform _heldNotebook;
        private SpriteRenderer _backpackStraps;
        private bool _carriedItemsVisible = true;
        private PhysicsMaterial2D _movementMaterial;

        public bool HasInventoryItem(int slot)
        {
            switch (slot)
            {
                case 0: return HasBackpack;
                case 1: return HasFuscaKey;
                case 2: return HasResearchNotebook;
                case 3: return HasDecodedData;
                case 4: return HasHistoricalDocument;
                default: return false;
            }
        }

        public float CurrentSanity => currentSanity;
        public float MaxSanity => maxSanity;
        public Vector2 FacingDirection => _lastFacing;
        public bool IsRunning => _isRunning;
        public bool IsInputLocked => _inputLocked || _combatLocked;
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
            PollKeyboard();
            ScanInteractables();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void PollKeyboard()
        {
            if (IsInputLocked)
            {
                _moveInput = Vector2.zero;
                if (!IsScriptedMotion) _rb.linearVelocity = Vector2.zero;
                return;
            }
            float x = 0f;
            float y = 0f;
            _isRunning = false;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;

                _isRunning = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

                if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    TryInteract();
                }
            }

            if (IsInputLocked) return;
            _moveInput = new Vector2(x, y).normalized;

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                _lastFacing = _moveInput;
            }
        }

        private void Move()
        {
            if (IsScriptedMotion) return;
            if (IsInputLocked) { _rb.linearVelocity = Vector2.zero; return; }
            float speed = _isRunning ? runSpeed : walkSpeed;
            Vector2 targetVel = _moveInput * speed;
            // Com a configuração padrão a resposta é imediata; valores menores
            // permitem uma entrada mais macia sem deixar Edelzio escorregar ao inverter.
            float response = Mathf.Clamp01(acceleration / 25f);
            _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, targetVel, response);
        }

        private void OnDisable()
        {
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
            if (IsInputLocked) return;
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

        /// <summary>Coloca uma mochila visual atrás do personagem depois da coleta.</summary>
        public void EquipBackpack()
        {
            HasBackpack = true;
            if (_equippedBackpack != null) return;

            var backpack = new GameObject("Mochila_Equipada");
            backpack.transform.SetParent(transform, false);
            backpack.transform.localScale = new Vector3(.56f, .56f, 1f);

            var renderer = backpack.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Backpack_Worn", Color.gray);
            renderer.sortingOrder = _sr != null ? _sr.sortingOrder - 2 : 3;
            _equippedBackpack = backpack.transform;
            var straps = new GameObject("Mochila_Alcas");
            straps.transform.SetParent(transform, false);
            straps.transform.localPosition = new Vector3(0f, -.16f, 0f);
            straps.transform.localScale = new Vector3(.90f, .90f, 1f);
            _backpackStraps = straps.AddComponent<SpriteRenderer>();
            _backpackStraps.sprite = VarginhaPixelArtSprites.Create("Backpack_Straps", Color.gray);
            UpdateBackpackPosition();
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
            renderer.sprite = VarginhaPixelArtSprites.Create("Notebook_Held", new Color(.30f, .90f, 1f));
            renderer.sortingOrder = _sr != null ? _sr.sortingOrder + 2 : 7;
            _heldNotebook = notebook.transform;
        }

        /// <summary>Esconde os itens carregados quando Edelzio entra na cabine.</summary>
        public void SetCarriedItemsVisible(bool visible)
        {
            _carriedItemsVisible = visible;
            if (_backpackStraps != null) _backpackStraps.enabled = visible;
            SetChildSpriteVisible(_equippedBackpack, visible);
            SetChildSpriteVisible(_heldNotebook, visible);
        }

        private static void SetChildSpriteVisible(Transform item, bool visible)
        {
            if (item == null) return;
            var renderer = item.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = visible;
        }

        private void LateUpdate()
        {
            if (_equippedBackpack != null) UpdateBackpackPosition();
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
                    ? new Vector3(.18f, facingUp ? .16f : -.16f, 0f)
                    : new Vector3(facingLeft ? -.28f : .28f, -.04f, 0f);
                float blend = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
                _heldNotebook.localPosition = Vector3.Lerp(_heldNotebook.localPosition, targetPosition, blend);
                _heldNotebook.localScale = Vector3.Lerp(_heldNotebook.localScale,
                    Vector3.one * (vertical ? .38f : .34f), blend);
                var renderer = _heldNotebook.GetComponent<SpriteRenderer>();
                renderer.sortingLayerID = _sr.sortingLayerID;
                renderer.sortingOrder = _sr.sortingOrder + (facingUp ? -2 : 2);
                renderer.flipX = facingLeft;
            }
        }

        private void UpdateBackpackPosition()
        {
            var animation = GetComponent<VarginhaPlayerSpriteAnimation>();
            Vector2 facing = animation != null && animation.HasActionPose ? animation.ActionFacingDirection : _lastFacing;
            bool sideways = Mathf.Abs(facing.x) > Mathf.Abs(facing.y);
            float side = sideways ? (facing.x > 0 ? -.25f : .25f) : 0f;
            bool facingUp = !sideways && facing.y > .05f;
            _equippedBackpack.localPosition = new Vector3(side, facingUp ? .10f : -.16f, 0f);
            _equippedBackpack.localScale = new Vector3(sideways ? .58f : .82f, .78f, 1f);
            var renderer = _equippedBackpack.GetComponent<SpriteRenderer>();
            renderer.sortingLayerID = _sr.sortingLayerID;
            renderer.sortingOrder = _sr.sortingOrder + (facingUp ? 1 : -2);
            renderer.enabled = _carriedItemsVisible;
            if (_backpackStraps != null)
            {
                _backpackStraps.sortingLayerID = _sr.sortingLayerID;
                _backpackStraps.sortingOrder = _sr.sortingOrder + (facingUp ? -1 : 1);
                _backpackStraps.enabled = _carriedItemsVisible && !sideways && !facingUp;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
