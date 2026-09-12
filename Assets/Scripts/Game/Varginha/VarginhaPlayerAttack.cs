using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Varginha
{
    /// <summary>
    /// Ataque corpo a corpo de Edelzio para a Fase 2.
    /// Keyframes: antecipação (0-1), impacto (2), recuperação (3-5).
    /// </summary>
    [RequireComponent(typeof(EdelzioTopDownController))]
    [RequireComponent(typeof(VarginhaPlayerSpriteAnimation))]
    public sealed class VarginhaPlayerAttack : MonoBehaviour
    {
        [Header("Ataque")]
        [SerializeField] private float damage = 34f;
        [SerializeField] private float hitRadius = .52f;
        [SerializeField] private float hitDistance = .67f;
        [SerializeField] private float hitstopDuration = .075f;
        [SerializeField] private float screenShakeDuration = .09f;
        [SerializeField] private float screenShakeMagnitude = .045f;

        private EdelzioTopDownController _player;
        private VarginhaPlayerSpriteAnimation _animation;
        private Sprite[][] _attackFrames;
        private bool _isAttacking;
        private float _cooldownTimer;
        private bool _hitstopActive;
        private float _previousTimeScale = 1f;
        private Vector3 _attackBaseScale = Vector3.one;

        public bool IsAttacking => _isAttacking;
        public float CooldownRemaining => Mathf.Max(0f, _cooldownTimer);

        /// <summary>Compatibilidade com as fases antigas: Edelzio sempre ataca sem recarga.</summary>
        public void Configure(float attackCooldown, bool mouseOnly = false)
        {
            _cooldownTimer = 0f;
        }


        private void Awake()
        {
            _player = GetComponent<EdelzioTopDownController>();
            _animation = GetComponent<VarginhaPlayerSpriteAnimation>();
            _attackFrames = LoadAttackFrames();
        }

        private void Update()
        {
            _cooldownTimer -= Time.unscaledDeltaTime;
            if (Time.timeScale <= 0f || _isAttacking || _cooldownTimer > 0f || _player.IsInputLocked || VarginhaGameHUD.Instance?.IsDialogueOpen == true) return;

            var mouse = Mouse.current;
            bool pressed = mouse != null && mouse.leftButton.isPressed;
            if (pressed) TryAttack();
        }

        public bool TryAttack()
        {
            if (Time.timeScale <= 0f || _isAttacking || _cooldownTimer > 0f || _player.IsInputLocked || VarginhaGameHUD.Instance?.IsDialogueOpen == true) return false;
            if (_attackFrames == null) return false;
            StartCoroutine(AttackRoutine());
            return true;
        }

        private IEnumerator AttackRoutine()
        {
            _isAttacking = true;
            _cooldownTimer = 0f;
            Vector3 baseScale = transform.localScale;
            _attackBaseScale = baseScale;

            Vector2 direction = Cardinalize(_player.FacingDirection);
            int directionIndex = DirectionIndex(direction);
            // A antecipação repete o primeiro quadro, e a recuperação repete o
            // último. O braço e o objeto permanecem legíveis sem parecer um corte
            // seco entre a caminhada e o impacto.
            int[] sequence = { 0, 0, 1, 2, 3, 4, 5, 5, 4 };
            float[] frameDurations = { .08f, .055f, .07f, .075f, .06f, .06f, .07f, .055f, .085f };
            bool impactDone = false;
            for (int pose = 0; pose < sequence.Length; pose++)
            {
                int frame = sequence[pose];
                _animation.SetCombatPose(_attackFrames[directionIndex][frame], direction);
                float anticipation = pose < 3 ? pose / 3f : 1f - Mathf.Clamp01((pose - 3f) / 6f);
                transform.localScale = baseScale * (1f + Mathf.Sin(anticipation * Mathf.PI) * .035f);
                if (frame == 2 && !impactDone)
                {
                    impactDone = true;
                    yield return ImpactRoutine(direction);
                }
                yield return WaitUnscaled(frameDurations[pose]);
            }

            _animation.ClearActionPose();
            transform.localScale = baseScale;
            _isAttacking = false;
        }

        private IEnumerator ImpactRoutine(Vector2 direction)
        {
            CreateSlashEffect(direction, false);
            var hits = Physics2D.OverlapCircleAll((Vector2)transform.position + direction * hitDistance, hitRadius);
            bool connected = false;
            var struck = new System.Collections.Generic.HashSet<VarginhaCombatTarget>();
            for (int i = 0; i < hits.Length; i++)
            {
                var target = hits[i] != null ? hits[i].GetComponentInParent<VarginhaCombatTarget>() : null;
                if (target == null || target.transform == transform || !struck.Add(target)) continue;
                if (target.ReceiveHit(damage, direction, hitstopDuration)) connected = true;
            }

            if (!connected) yield break;
            CreateSlashEffect(direction, true);
            var camera = Camera.main;
            if (camera != null)
            {
                var shake = camera.GetComponent<VarginhaCameraShake>() ?? camera.gameObject.AddComponent<VarginhaCameraShake>();
                shake.Shake(screenShakeDuration, screenShakeMagnitude);
            }
            yield return HitstopRoutine();
        }

        private IEnumerator HitstopRoutine()
        {
            if (Time.timeScale <= 0f) yield break;
            _previousTimeScale = Time.timeScale;
            _hitstopActive = true;
            Time.timeScale = .02f;
            yield return new WaitForSecondsRealtime(hitstopDuration);
            Time.timeScale = _previousTimeScale;
            _hitstopActive = false;
        }

        private void CreateSlashEffect(Vector2 direction, bool impact)
        {
            var effect = new GameObject(impact ? "Impacto_Ataque" : "Arco_Ataque");
            effect.transform.SetParent(transform, false);
            effect.transform.localPosition = direction * (hitDistance + .06f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            effect.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            effect.transform.localScale = Vector3.one * .74f;
            var renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create(impact ? "Attack_Impact" : "Attack_Slash", impact ? Color.white : new Color(1f, .78f, .22f));
            renderer.sortingOrder = GetComponent<SpriteRenderer>() != null ? GetComponent<SpriteRenderer>().sortingOrder + 3 : 8;
            StartCoroutine(DestroyEffect(effect, impact ? .09f : .12f));
        }

        private static IEnumerator DestroyEffect(GameObject effect, float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            if (effect != null) Destroy(effect);
        }

        private static IEnumerator WaitUnscaled(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private static Vector2 Cardinalize(Vector2 direction)
        {
            if (direction.sqrMagnitude < .01f) return Vector2.down;
            return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? new Vector2(Mathf.Sign(direction.x), 0f)
                : new Vector2(0f, Mathf.Sign(direction.y));
        }

        private static int DirectionIndex(Vector2 direction)
        {
            if (direction.y < -.5f) return 0;
            if (direction.x < -.5f) return 1;
            if (direction.x > .5f) return 2;
            return 3;
        }

        private static Sprite[][] LoadAttackFrames()
        {
            var sheet = Resources.Load<Texture2D>("Varginha/EdelzioAttackV1");
            if (sheet == null) return null;
            sheet.filterMode = FilterMode.Point;
            var result = new Sprite[4][];
            for (int row = 0; row < 4; row++)
            {
                result[row] = new Sprite[6];
                float unityRow = 3f - row;
                for (int frame = 0; frame < 6; frame++)
                {
                    result[row][frame] = Sprite.Create(sheet, new Rect(frame * sheet.width / 6f, unityRow * sheet.height / 4f, sheet.width / 6f, sheet.height / 4f), new Vector2(.5f, .5f), 44.1379f);
                    result[row][frame].name = "Edelzio_Ataque_" + row + "_" + frame;
                }
            }
            return result;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (_hitstopActive)
            {
                Time.timeScale = _previousTimeScale;
                _hitstopActive = false;
            }
            if (_animation != null) _animation.ClearActionPose();
            if (_player != null)
            {
                _isAttacking = false;
            }
            transform.localScale = _attackBaseScale;
        }
    }
}
