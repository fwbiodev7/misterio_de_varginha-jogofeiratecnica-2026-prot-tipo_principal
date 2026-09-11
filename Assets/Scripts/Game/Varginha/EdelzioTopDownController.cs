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

        // State
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Vector2 _moveInput;
        private Vector2 _lastFacing = Vector2.down;
        private bool _isRunning;
        private bool _inputLocked;
        private InteractableProp _nearestInteractable;
        private Transform _equippedBackpack;
        private Transform _heldNotebook;

        public float CurrentSanity => currentSanity;
        public float MaxSanity => maxSanity;
        public Vector2 FacingDirection => _lastFacing;
        public bool IsRunning => _isRunning;
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
            if (_inputLocked)
            {
                _moveInput = Vector2.zero;
                _rb.linearVelocity = Vector2.zero;
                return;
            }
            float x = 0f;
            float y = 0f;

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

            _moveInput = new Vector2(x, y).normalized;

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                _lastFacing = _moveInput;
            }
        }

        private void Move()
        {
            float speed = _isRunning ? runSpeed : walkSpeed;
            Vector2 targetVel = _moveInput * speed;
            _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, targetVel, acceleration * Time.fixedDeltaTime);
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
            if (locked && _rb != null) _rb.linearVelocity = Vector2.zero;
        }

        /// <summary>Coloca uma mochila visual atrás do personagem depois da coleta.</summary>
        public void EquipBackpack()
        {
            if (_equippedBackpack != null) return;

            var backpack = new GameObject("Mochila_Equipada");
            backpack.transform.SetParent(transform, false);
            backpack.transform.localScale = new Vector3(.56f, .56f, 1f);

            var renderer = backpack.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Backpack_Worn", new Color(.20f, .75f, .40f));
            renderer.sortingOrder = _sr != null ? _sr.sortingOrder - 1 : 4;
            _equippedBackpack = backpack.transform;
            UpdateBackpackPosition();
        }

        /// <summary>Depois da decodificação, Edelzio carrega o notebook sob o braço.</summary>
        public void EquipNotebook()
        {
            if (_heldNotebook != null) return;

            var notebook = new GameObject("Notebook_Sob_o_Braco");
            notebook.transform.SetParent(transform, false);
            notebook.transform.localPosition = new Vector3(.30f, -.03f, 0f);
            notebook.transform.localScale = new Vector3(.48f, .48f, 1f);
            var renderer = notebook.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Notebook_Held", new Color(.30f, .90f, 1f));
            renderer.sortingOrder = _sr != null ? _sr.sortingOrder + 1 : 6;
            _heldNotebook = notebook.transform;
        }

        /// <summary>Esconde os itens carregados quando Edelzio entra na cabine.</summary>
        public void SetCarriedItemsVisible(bool visible)
        {
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
        }

        private void UpdateBackpackPosition()
        {
            float side = _sr != null && _sr.flipX ? .26f : -.26f;
            _equippedBackpack.localPosition = new Vector3(side, .06f, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
