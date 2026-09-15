using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Varginha
{
    /// <summary>Combo direcional com contato único, recuperação curta e finalizador.</summary>
    [RequireComponent(typeof(EdelzioTopDownController))]
    [RequireComponent(typeof(VarginhaPlayerSpriteAnimation))]
    public sealed class VarginhaPlayerAttack : MonoBehaviour
    {
        [SerializeField] private float damage = 34f;
        [SerializeField] private float hitRadius = .52f;
        [SerializeField] private float hitDistance = .67f;
        [SerializeField] private float hitstopDuration = .045f;
        [SerializeField] private float screenShakeDuration = .09f;
        [SerializeField] private float screenShakeMagnitude = .045f;
        private const float ComboWindow = .75f;
        private static readonly float[] Durations = { .055f, .05f, .065f, .07f, .075f, .085f };
        private static readonly int[][] ComboFrames =
        {
            new[] { 0, 0, 1, 2, 3, 4 }, // corte horizontal
            new[] { 1, 1, 2, 3, 4, 5 }, // cruzado, com mais alcance visual
            new[] { 2, 2, 3, 4, 5, 5 }  // finalizador pesado
        };
        private EdelzioTopDownController _player;
        private VarginhaPlayerSpriteAnimation _animation;
        private Sprite[][] _attackFrames;
        private bool _isAttacking;
        private float _lastAttackFinished = float.NegativeInfinity;
        private float _bufferUntil = float.NegativeInfinity;
        private Vector2 _bufferDirection;
        private int _comboStep;
        private bool _hitstopActive;
        private float _previousTimeScale = 1f;
        private float _hitstopScale;

        public bool IsAttacking => _isAttacking;
        public float CooldownRemaining => 0f;
        public int ComboStep => _isAttacking || Time.time - _lastAttackFinished <= ComboWindow ? _comboStep : 0;
        private bool CanAttack => isActiveAndEnabled && Time.timeScale > 0f && _player != null
            && !_player.IsInputLocked && !_player.IsDodging && _player.CurrentSanity > 0f
            && GetComponent<Game.Player.HealthSystem>()?.IsDead != true
            && VarginhaGameHUD.Instance?.IsDialogueOpen != true
            && VarginhaGameHUD.Instance?.IsVictoryOpen != true;

        public void Configure(float attackCooldown, bool mouseOnly = false) { }
        public void EndHitstopForModal() => RestoreTimeScale();

        private void Awake()
        {
            _player = GetComponent<EdelzioTopDownController>();
            _animation = GetComponent<VarginhaPlayerSpriteAnimation>();
            _attackFrames = LoadAttackFrames();
        }

        private void Update()
        {
            if (!CanAttack) { _bufferUntil = float.NegativeInfinity; return; }
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            bool mouseHeld = VarginhaInputBindings.GetMouseButton(VarginhaInputAction.Attack) >= 0
                && VarginhaInputBindings.IsPressed(VarginhaInputAction.Attack);
            // J/K continuam aceitos como compatibilidade com os protótipos anteriores;
            // o botão configurado no menu é sempre a entrada principal.
            bool legacyHeld = keyboard?.jKey.isPressed == true || keyboard?.kKey.isPressed == true;
            bool held = VarginhaInputBindings.IsPressed(VarginhaInputAction.Attack) || legacyHeld;
            bool legacyPressed = keyboard?.jKey.wasPressedThisFrame == true || keyboard?.kKey.wasPressedThisFrame == true;
            bool pressed = VarginhaInputBindings.WasPressedThisFrame(VarginhaInputAction.Attack) || legacyPressed;
            Vector2 direction = _player.FacingDirection;
            if (mouseHeld && mouse != null && Camera.main != null)
            {
                Vector2 aim = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
                if ((aim - (Vector2)transform.position).sqrMagnitude > .04f) direction = aim - (Vector2)transform.position;
            }
            if (pressed) QueueAttack(direction);
            if (!_isAttacking && (held || Time.time <= _bufferUntil)) TryAttack(held ? direction : _bufferDirection);
        }

        public void QueueAttack(Vector2 direction)
        {
            if (!CanAttack) return;
            _bufferUntil = Time.time + .18f;
            _bufferDirection = direction;
        }

        public bool TryAttack() => TryAttack(_player != null ? _player.FacingDirection : Vector2.down);

        public bool TryAttack(Vector2 direction)
        {
            if (!CanAttack || _isAttacking || _attackFrames == null) return false;
            _bufferUntil = float.NegativeInfinity;
            _comboStep = Time.time - _lastAttackFinished <= ComboWindow ? _comboStep % 3 + 1 : 1;
            StartCoroutine(AttackRoutine(Cardinalize(direction)));
            return true;
        }

        private IEnumerator AttackRoutine(Vector2 direction)
        {
            _isAttacking = true;
            int row = DirectionIndex(direction);
            int[] frames = ComboFrames[Mathf.Clamp(_comboStep - 1, 0, ComboFrames.Length - 1)];
            int impactPose = Mathf.Clamp(_comboStep + 1, 2, 4);
            bool interrupted = false;
            for (int pose = 0; pose < frames.Length; pose++)
            {
                if (_player.IsDodging || _player.IsInputLocked || _player.CurrentSanity <= 0f
                    || GetComponent<Game.Player.HealthSystem>()?.IsDead == true
                    || VarginhaGameHUD.Instance?.IsDialogueOpen == true
                    || VarginhaGameHUD.Instance?.IsVictoryOpen == true)
                {
                    interrupted = true;
                    break;
                }
                _animation.SetCombatPose(_attackFrames[row][frames[pose]], direction);
                if (pose == impactPose) yield return ImpactRoutine(direction);
                float elapsed = 0f;
                float duration = Durations[pose] * (_comboStep == 3 ? 1.12f : 1f);
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
            _animation.ClearActionPose();
            if (interrupted)
            {
                _comboStep = 0;
                _lastAttackFinished = float.NegativeInfinity;
            }
            else _lastAttackFinished = Time.time;
            _isAttacking = false;
        }

        private IEnumerator ImpactRoutine(Vector2 direction)
        {
            if (!CanAttack) yield break;
            bool finisher = _comboStep == 3;
            CreateSlashEffect(direction, false, _comboStep);
            var hits = Physics2D.OverlapCircleAll((Vector2)transform.position + direction * hitDistance,
                hitRadius + (finisher ? .18f : 0f));
            bool connected = false;
            var struck = new HashSet<VarginhaCombatTarget>();
            foreach (var hit in hits)
            {
                var target = hit != null ? hit.GetComponentInParent<VarginhaCombatTarget>() : null;
                if (target == null || !struck.Add(target) || !HasClearHit(target)) continue;
                float multiplier = finisher ? 1.45f : _comboStep == 2 ? 1.10f : 1f;
                if (!target.ReceiveHit(damage * multiplier, direction, hitstopDuration)) continue;
                connected = true;
                if (finisher) target.GetComponent<VarginhaCombatEnemy>()?.ApplyAllyControl(direction, .38f);
            }
            if (!connected) yield break;
            CreateSlashEffect(direction, true, _comboStep);
            var camera = Camera.main;
            if (camera != null)
            {
                var shake = camera.GetComponent<VarginhaCameraShake>() ?? camera.gameObject.AddComponent<VarginhaCameraShake>();
                shake.Shake(screenShakeDuration, screenShakeMagnitude * (finisher ? 1.4f : 1f));
            }
            if (Time.timeScale <= 0f) yield break;
            _previousTimeScale = Time.timeScale;
            _hitstopActive = true;
            _hitstopScale = Mathf.Min(.02f, _previousTimeScale);
            Time.timeScale = _hitstopScale;
            yield return new WaitForSecondsRealtime(hitstopDuration * (finisher ? 1.3f : 1f));
            RestoreTimeScale();
        }

        private bool HasClearHit(VarginhaCombatTarget target)
        {
            foreach (var hit in Physics2D.LinecastAll(transform.position, target.transform.position))
            {
                if (hit.collider.isTrigger || hit.transform == transform || hit.transform.IsChildOf(transform)
                    || hit.collider.GetComponentInParent<VarginhaCombatTarget>() != null
                    || hit.collider.GetComponentInParent<VarginhaStudentAlly>() != null) continue;
                return false;
            }
            return true;
        }

        private void CreateSlashEffect(Vector2 direction, bool impact, int comboStep)
        {
            var effect = new GameObject(impact ? "Impacto_Ataque" : "Arco_Ataque");
            effect.transform.position = transform.position + (Vector3)direction * (hitDistance + .06f);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            bool finisher = comboStep == 3;
            effect.transform.localScale = Vector3.one * (finisher ? 1.05f : comboStep == 2 ? .88f : .74f);
            var renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create(impact ? "Attack_Impact" : comboStep == 3 ? "Attack_HeavySlash"
                : comboStep == 2 ? "Attack_CrossSlash" : "Attack_Slash",
                impact ? Color.white : new Color(1f, .78f, .22f));
            renderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 3;
            Destroy(effect, impact ? .12f : .16f);
        }

        private void RestoreTimeScale()
        {
            // Não desfaz uma pausa que começou durante o impacto.
            if (_hitstopActive && Mathf.Approximately(Time.timeScale, _hitstopScale)) Time.timeScale = _previousTimeScale;
            _hitstopActive = false;
        }

        private static Vector2 Cardinalize(Vector2 direction) => direction.sqrMagnitude < .01f ? Vector2.down
            : Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? new Vector2(Mathf.Sign(direction.x), 0f) : new Vector2(0f, Mathf.Sign(direction.y));
        private static int DirectionIndex(Vector2 direction) => direction.y < -.5f ? 0 : direction.x < -.5f ? 1 : direction.x > .5f ? 2 : 3;

        private static Sprite[][] LoadAttackFrames()
        {
            var sheet = Resources.Load<Texture2D>("Varginha/EdelzioAttackV1");
            if (sheet == null) return null;
            sheet.filterMode = FilterMode.Point;
            var result = new Sprite[4][];
            for (int row = 0; row < 4; row++)
            {
                result[row] = new Sprite[6];
                for (int frame = 0; frame < 6; frame++)
                {
                    result[row][frame] = Sprite.Create(sheet, new Rect(frame * sheet.width / 6f, (3 - row) * sheet.height / 4f,
                        sheet.width / 6f, sheet.height / 4f), new Vector2(.5f, .5f), 44.1379f);
                    result[row][frame].name = "Edelzio_Ataque_" + row + "_" + frame;
                }
            }
            return result;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            RestoreTimeScale();
            if (_animation != null) _animation.ClearActionPose();
            _isAttacking = false;
            _comboStep = 0;
            _lastAttackFinished = _bufferUntil = float.NegativeInfinity;
        }

        private void OnDestroy()
        {
            if (_attackFrames == null) return;
            foreach (var row in _attackFrames) foreach (var sprite in row) if (sprite != null) Destroy(sprite);
        }
    }
}
