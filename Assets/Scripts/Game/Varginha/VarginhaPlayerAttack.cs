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
        private static readonly float[] Durations = { .025f, .05f, .025f, .07f, .05f, .04f };
        private static readonly int[][] ComboFrames =
        {
            new[] { 0, 1, 2, 3, 4, 5 }, // jab
            new[] { 0, 1, 2, 3, 4, 5 }, // direto com o braço oposto
            new[] { 0, 1, 2, 3, 4, 5 }  // soco finalizador
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
            if (GetComponent<VarginhaCombatCursor>() == null) gameObject.AddComponent<VarginhaCombatCursor>();
        }

        private void Update()
        {
            if (_isAttacking && MustInterrupt)
            {
                CancelAttack();
                return;
            }
            if (!CanAttack) { _bufferUntil = float.NegativeInfinity; return; }
            var keyboard = Keyboard.current;
            // J/K continuam aceitos como compatibilidade com os protótipos anteriores;
            // o botão configurado no menu é sempre a entrada principal.
            bool legacyHeld = keyboard?.jKey.isPressed == true || keyboard?.kKey.isPressed == true;
            bool held = VarginhaInputBindings.IsPressed(VarginhaInputAction.Attack) || legacyHeld;
            bool legacyPressed = keyboard?.jKey.wasPressedThisFrame == true || keyboard?.kKey.wasPressedThisFrame == true;
            bool pressed = VarginhaInputBindings.WasPressedThisFrame(VarginhaInputAction.Attack) || legacyPressed;
            Vector2 direction = _player.FacingDirection;
            if (pressed) QueueAttack(direction);
            if (!_isAttacking && (held || Time.time <= _bufferUntil)) TryAttack(held ? direction : _bufferDirection);
        }

        public void QueueAttack(Vector2 direction)
        {
            if (!CanAttack) return;
            // One pending punch survives anticipation/contact; repeated clicks never queue an endless chain.
            _bufferUntil = _isAttacking ? float.PositiveInfinity : Time.time + .18f;
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
            const int impactPose = 3;
            int variant = _attackFrames[row].Length >= 18 ? (_comboStep - 1) * 6 : 0;
            bool interrupted = false;
            float remaining = 0f;
            for (int pose = 0; pose < frames.Length; pose++)
            {
                if (MustInterrupt)
                {
                    interrupted = true;
                    break;
                }
                _animation.SetCombatPose(_attackFrames[row][variant + frames[pose]], direction);
                if (pose == impactPose) yield return ImpactRoutine(direction);
                float duration = Durations[pose] * (_comboStep == 3 ? 1.12f : 1f);
                remaining += duration;
                while (remaining > 0f)
                {
                    yield return null;
                    remaining -= Time.deltaTime;
                    if (MustInterrupt) { interrupted = true; break; }
                }
                if (interrupted) break;
            }
            _animation.ClearCombatPose();
            if (interrupted)
            {
                _comboStep = 0;
                _lastAttackFinished = _bufferUntil = float.NegativeInfinity;
            }
            else _lastAttackFinished = Time.time;
            _isAttacking = false;
        }

        private IEnumerator ImpactRoutine(Vector2 direction)
        {
            if (!CanAttack) yield break;
            bool finisher = _comboStep == 3;
            PlayCombatAudio(false, finisher);
            CreatePunchEffect(direction, false, _comboStep);
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
                if (target.GetComponentInParent<VarginhaCombatEnemy>() != null)
                    CreateAlienIchorEffect(target.transform.position, direction, finisher);
                if (finisher) target.GetComponent<VarginhaCombatEnemy>()?.ApplyAllyControl(direction, .38f);
            }
            if (!connected) yield break;
            PlayCombatAudio(true, finisher);
            CreatePunchEffect(direction, true, _comboStep);
            var camera = Camera.main;
            if (camera != null)
            {
                var shake = camera.GetComponent<VarginhaCameraShake>() ?? camera.gameObject.AddComponent<VarginhaCameraShake>();
                shake.Shake(screenShakeDuration * (finisher ? 1.35f : 1f), screenShakeMagnitude * (finisher ? 1.75f : 1f));
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

        private void CreatePunchEffect(Vector2 direction, bool impact, int comboStep)
        {
            var effect = new GameObject(impact ? "Impacto_Soco" : "Rastro_Soco");
            effect.transform.position = transform.position + (Vector3)direction * (hitDistance + .06f);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            bool finisher = comboStep == 3;
            effect.transform.localScale = Vector3.one * (finisher ? .50f : .36f);
            var renderer = effect.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create(impact ? "Attack_Impact" : "Attack_PunchTrail",
                impact ? new Color(1f, .94f, .78f) : new Color(.86f, .73f, .43f));
            renderer.color = new Color(1f, 1f, 1f, impact ? .88f : .40f);
            renderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 3;
            Destroy(effect, impact ? .09f : .06f);
        }

        private void CreateAlienIchorEffect(Vector3 hitPoint, Vector2 direction, bool finisher)
        {
            var ichor = new GameObject("Ichor_ET_Splash");
            ichor.transform.position = hitPoint + (Vector3)direction * 0.08f;
            ichor.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
            ichor.transform.localScale = Vector3.one * (finisher ? 1.25f : .85f);
            var renderer = ichor.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Alien_Ichor", new Color(.20f, .95f, .38f));
            renderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 4;
            Destroy(ichor, finisher ? .22f : .16f);
        }

        private void RestoreTimeScale()
        {
            // Não desfaz uma pausa que começou durante o impacto.
            if (_hitstopActive && Mathf.Approximately(Time.timeScale, _hitstopScale)) Time.timeScale = _previousTimeScale;
            _hitstopActive = false;
        }

        private static AudioClip _whooshClip;
        private static AudioClip _impactClip;
        private static AudioSource _combatAudio;

        private static void PlayCombatAudio(bool isImpact, bool finisher)
        {
            if (_combatAudio == null)
            {
                var go = new GameObject("VarginhaCombatAudio");
                Object.DontDestroyOnLoad(go);
                _combatAudio = go.AddComponent<AudioSource>();
                _combatAudio.playOnAwake = false;
            }

            if (isImpact)
            {
                if (_impactClip == null)
                {
                    int sampleRate = 22050;
                    float dur = 0.12f;
                    int count = Mathf.RoundToInt(sampleRate * dur);
                    float[] data = new float[count];
                    for (int i = 0; i < count; i++)
                    {
                        float t = (float)i / sampleRate;
                        float env = Mathf.Exp(-t * 32f);
                        float freq = Mathf.Lerp(160f, 45f, (float)i / count);
                        float noise = (UnityEngine.Random.value * 2f - 1f) * 0.35f;
                        data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.65f + noise) * env * 0.5f;
                    }
                    _impactClip = AudioClip.Create("CombatThudSplat", count, 1, sampleRate, false);
                    _impactClip.SetData(data, 0);
                }
                _combatAudio.pitch = finisher ? 0.85f : UnityEngine.Random.Range(0.95f, 1.15f);
                _combatAudio.PlayOneShot(_impactClip, finisher ? 0.45f : 0.32f);
            }
            else
            {
                if (_whooshClip == null)
                {
                    int sampleRate = 22050;
                    float dur = 0.09f;
                    int count = Mathf.RoundToInt(sampleRate * dur);
                    float[] data = new float[count];
                    for (int i = 0; i < count; i++)
                    {
                        float t = (float)i / sampleRate;
                        float env = Mathf.Sin(Mathf.PI * (float)i / count);
                        float freq = Mathf.Lerp(280f, 120f, (float)i / count);
                        float noise = (UnityEngine.Random.value * 2f - 1f) * 0.45f;
                        data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f + noise) * env * 0.28f;
                    }
                    _whooshClip = AudioClip.Create("CombatWhoosh", count, 1, sampleRate, false);
                    _whooshClip.SetData(data, 0);
                }
                _combatAudio.pitch = finisher ? 0.9f : UnityEngine.Random.Range(1.0f, 1.25f);
                _combatAudio.PlayOneShot(_whooshClip, 0.24f);
            }
        }

        private static Vector2 Cardinalize(Vector2 direction) => direction.sqrMagnitude < .01f ? Vector2.down
            : Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? new Vector2(Mathf.Sign(direction.x), 0f) : new Vector2(0f, Mathf.Sign(direction.y));
        private static int DirectionIndex(Vector2 direction) => direction.y < -.5f ? 0 : direction.x < -.5f ? 1 : direction.x > .5f ? 2 : 3;

        private static Sprite[][] LoadAttackFrames()
        {
            var reference = VarginhaReferenceSprites.EdelzioAttackFrames();
            if (reference != null) return reference;

            if (VarginhaReferenceSprites.Attack(0, 0) != null)
            {
                var frames = new Sprite[4][];
                int[] recovery = { 0, 0, 1, 3, 2, 0 };
                for (int direction = 0; direction < 4; direction++)
                {
                    frames[direction] = new Sprite[6];
                    for (int frame = 0; frame < 6; frame++)
                        frames[direction][frame] = VarginhaReferenceSprites.Attack(direction, recovery[frame]);
                }
                return frames;
            }
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

        private void OnDisable() => CancelAttack();

        private bool MustInterrupt => _player == null || _player.IsDodging || _player.IsInputLocked
            || _player.CurrentSanity <= 0f || GetComponent<Game.Player.HealthSystem>()?.IsDead == true
            || VarginhaGameHUD.Instance?.IsDialogueOpen == true || VarginhaGameHUD.Instance?.IsVictoryOpen == true;

        private void CancelAttack()
        {
            StopAllCoroutines();
            RestoreTimeScale();
            if (_isAttacking && _animation != null) _animation.ClearCombatPose();
            _isAttacking = false;
            _comboStep = 0;
            _lastAttackFinished = _bufferUntil = float.NegativeInfinity;
        }

        private void OnDestroy()
        {
            if (_attackFrames == null) return;
            if (ReferenceEquals(_attackFrames, VarginhaReferenceSprites.EdelzioAttackFrames())) return;
            foreach (var row in _attackFrames) foreach (var sprite in row) if (sprite != null) Destroy(sprite);
        }
    }
}
