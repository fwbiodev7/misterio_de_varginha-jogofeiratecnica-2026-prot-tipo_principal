using System;
using System.Collections;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Pequenas encenações para as interações. Elas pausam somente o controle do Edelzio,
    /// mantendo os quadros nítidos e a física estável em um jogo top-down.
    /// </summary>
    [RequireComponent(typeof(EdelzioTopDownController))]
    public sealed class VarginhaPlayerActionAnimation : MonoBehaviour
    {
        private EdelzioTopDownController _player;
        private VarginhaPlayerSpriteAnimation _spriteAnimation;
        private SpriteRenderer _renderer;
        private Rigidbody2D _body;
        private bool _isActing;
        private GameObject _heldCup;
        private SpriteRenderer _worldCup;
        private bool _worldCupWasVisible;
        private readonly RaycastHit2D[] _motionHits = new RaycastHit2D[16];
        private Collider2D _seatCollider;
        private Collider2D _playerCollider;
        private bool _seatWasIgnored;
        private Vector3 _standingPosition;
        private bool _notebookSession;

        private void Awake()
        {
            _player = GetComponent<EdelzioTopDownController>();
            _spriteAnimation = GetComponent<VarginhaPlayerSpriteAnimation>();
            _renderer = GetComponent<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
        }

        public IEnumerator CrouchRoutine(float duration)
        {
            if (_isActing) yield break;
            yield return PoseRoutine("Edelzio_Crouch", duration, .72f);
        }

        public IEnumerator ReachRoutine(float duration)
        {
            if (_isActing) yield break;
            yield return PoseRoutine("Edelzio_Reach", duration, .92f);
        }

        public void PlayNotebookSession(Transform notebook, Action onReady)
        {
            if (_isActing) return;
            StartCoroutine(NotebookRoutine(notebook, onReady));
        }

        public void PlayDrinkCoffee(Transform coffee, Action onComplete)
        {
            if (_isActing) return;
            StartCoroutine(CoffeeRoutine(coffee, onComplete));
        }

        private IEnumerator NotebookRoutine(Transform notebook, Action onReady)
        {
            BeginAction();
            _notebookSession = true;
            _standingPosition = transform.position;
            var chair = GameObject.Find("Chair_Office");
            if (chair != null)
            {
                _seatCollider = chair.GetComponent<Collider2D>();
                _playerCollider = GetComponent<Collider2D>();
                if (_seatCollider != null && _playerCollider != null)
                {
                    _seatWasIgnored = Physics2D.GetIgnoreCollision(_playerCollider, _seatCollider);
                    Physics2D.IgnoreCollision(_playerCollider, _seatCollider, true);
                }
                yield return MoveToPosition(chair.transform.position, .34f);
            }
            else
                yield return MoveCloseTo(notebook, .38f, .72f);
            _spriteAnimation?.SetActionPose("Edelzio_Sit");
            yield return new WaitForSeconds(.24f);
            _spriteAnimation?.SetActionPose("Edelzio_UseNotebook");
            yield return new WaitForSeconds(.38f);
            // Preserva a pose sentada durante todo o quiz; Close encerra a sessão.
            onReady?.Invoke();
            if (VarginhaNotebookQuiz.Instance == null) FinishNotebookSession();
        }

        public void FinishNotebookSession()
        {
            if (!_notebookSession) { if (!_isActing) _player?.SetInputLocked(false); return; }
            _notebookSession = false;
            StartCoroutine(StandUpRoutine());
        }

        private IEnumerator StandUpRoutine()
        {
            _spriteAnimation?.ClearActionPose();
            yield return MoveToPosition(_standingPosition, .24f);
            RestoreSeatCollision();
            EndAction();
        }

        private void RestoreSeatCollision()
        {
            if (_playerCollider != null && _seatCollider != null)
                Physics2D.IgnoreCollision(_playerCollider, _seatCollider, _seatWasIgnored);
            _seatCollider = null;
        }

        private IEnumerator CoffeeRoutine(Transform coffee, Action onComplete)
        {
            BeginAction();
            yield return MoveCloseTo(coffee, .25f, .55f);
            var worldCup = coffee != null ? coffee.GetComponent<SpriteRenderer>() : null;
            _worldCup = worldCup;
            _worldCupWasVisible = worldCup != null && worldCup.enabled;
            if (worldCup != null) worldCup.enabled = false;
            var cup = CreateHeldProp("Coffee_Held", new Color(.80f, .40f, .20f), new Vector3(-.18f, -.20f, 0f), .25f);
            _heldCup = cup;
            _spriteAnimation?.SetActionPose("Edelzio_DrinkCoffee");
            // Três goles deixam claro que Edelzio tomou toda a xícara, não apenas um gole rápido.
            for (int sip = 0; sip < 3; sip++)
            {
                _spriteAnimation?.SetCoffeeFrame(1);
                if (cup != null) cup.transform.localPosition = new Vector3(-.10f, .04f, 0f);
                yield return new WaitForSeconds(.18f);
                _spriteAnimation?.SetCoffeeFrame(3);
                if (cup != null) cup.transform.localPosition = new Vector3(-.18f, -.20f, 0f);
                yield return new WaitForSeconds(.12f);
            }
            // O copo some da mão e volta ao ponto original como uma xícara vazia.
            if (cup != null)
            {
                Destroy(cup);
            }
            if (worldCup != null) worldCup.enabled = true;
            _worldCup = null;
            _heldCup = null;
            _spriteAnimation?.ClearActionPose();
            EndAction();
            onComplete?.Invoke();
        }

        private IEnumerator PoseRoutine(string pose, float duration, float verticalScale)
        {
            BeginAction();
            _spriteAnimation?.SetActionPose(pose);
            Vector3 standingScale = transform.localScale;
            Vector3 poseScale = new Vector3(standingScale.x * 1.035f, standingScale.y * verticalScale, standingScale.z);
            float blendElapsed = 0f;
            const float blendDuration = .12f;
            while (blendElapsed < blendDuration)
            {
                blendElapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(standingScale, poseScale, Mathf.SmoothStep(0f, 1f, blendElapsed / blendDuration));
                yield return null;
            }
            yield return new WaitForSeconds(duration);
            blendElapsed = 0f;
            while (blendElapsed < blendDuration)
            {
                blendElapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(poseScale, standingScale, Mathf.SmoothStep(0f, 1f, blendElapsed / blendDuration));
                yield return null;
            }
            transform.localScale = standingScale;
            _spriteAnimation?.ClearActionPose();
            EndAction();
        }

        private IEnumerator MoveCloseTo(Transform target, float duration, float distance)
        {
            if (target == null) yield break;
            Vector3 start = transform.position;
            Vector3 offset = (start - target.position).normalized * distance;
            if (offset.sqrMagnitude < .001f) offset = Vector3.down * distance;
            Vector3 destination = target.position + offset;
            if (Vector3.Distance(start, destination) > 1.2f) yield break;

            yield return MoveToPosition(destination, duration);
        }

        private IEnumerator MoveToPosition(Vector3 destination, float duration)
        {
            Vector3 start = transform.position;
            if (Vector3.Distance(start, destination) > 2.2f) yield break;
            _player.IsScriptedMotion = true;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.fixedDeltaTime;
                Vector2 next = Vector3.Lerp(start, destination, Mathf.Clamp01(elapsed / duration));
                if (_body == null) yield break;
                Vector2 delta = next - _body.position;
                float distance = delta.magnitude;
                var filter = new ContactFilter2D();
                filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
                filter.useTriggers = false;
                int count = _body.Cast(delta.normalized, filter, _motionHits, distance + .02f);
                for (int i = 0; i < count; i++)
                    if (_motionHits[i].collider != _seatCollider && Vector2.Dot(_motionHits[i].normal, delta) < 0f)
                        distance = Mathf.Min(distance, Mathf.Max(0f, _motionHits[i].distance - .02f));
                _body.MovePosition(_body.position + delta.normalized * distance);
                yield return new WaitForFixedUpdate();
            }
            _player.IsScriptedMotion = false;
            _body.linearVelocity = Vector2.zero;
        }

        private GameObject CreateHeldProp(string spriteId, Color color, Vector3 localPosition, float scale)
        {
            var prop = new GameObject(spriteId);
            prop.transform.SetParent(transform, false);
            prop.transform.localPosition = localPosition;
            prop.transform.localScale = Vector3.one * scale;
            var renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create(spriteId, color);
            if (_renderer != null) renderer.sortingLayerID = _renderer.sortingLayerID;
            // Objetos na mão ficam acima do corpo durante a pose de ação;
            // a posição curta evita que o copo atravesse o tronco.
            renderer.sortingOrder = _renderer != null ? _renderer.sortingOrder + 2 : 7;
            return prop;
        }

        private void BeginAction()
        {
            _isActing = true;
            _player?.SetInputLocked(true);
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
            }
        }

        private void EndAction(bool unlockInput = true)
        {
            _isActing = false;
            if (_player != null) _player.IsScriptedMotion = false;
            if (_body != null) _body.linearVelocity = Vector2.zero;
            if (unlockInput) _player?.SetInputLocked(false);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (_seatCollider != null && _body != null) _body.position = _standingPosition;
            RestoreSeatCollision();
            _notebookSession = false;
            if (_heldCup != null) Destroy(_heldCup);
            if (_worldCup != null) _worldCup.enabled = _worldCupWasVisible;
            _spriteAnimation?.ClearActionPose();
            if (_isActing) EndAction();
        }
    }
}
