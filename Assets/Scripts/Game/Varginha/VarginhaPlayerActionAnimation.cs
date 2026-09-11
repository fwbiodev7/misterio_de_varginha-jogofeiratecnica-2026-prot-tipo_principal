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
        private bool _bodyWasSimulated;
        private bool _isActing;

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
            var chair = GameObject.Find("Chair_Office");
            if (chair != null)
                yield return MoveToPosition(chair.transform.position, .34f);
            else
                yield return MoveCloseTo(notebook, .38f, .72f);
            _spriteAnimation?.SetActionPose("Edelzio_Sit");
            yield return new WaitForSeconds(.24f);
            _spriteAnimation?.SetActionPose("Edelzio_UseNotebook");
            yield return new WaitForSeconds(.38f);
            _spriteAnimation?.ClearActionPose();
            // O quiz mantém o input bloqueado até ser fechado.
            EndAction(false);
            onReady?.Invoke();
        }

        private IEnumerator CoffeeRoutine(Transform coffee, Action onComplete)
        {
            BeginAction();
            yield return MoveCloseTo(coffee, .25f, .55f);
            var worldCup = coffee != null ? coffee.GetComponent<SpriteRenderer>() : null;
            if (worldCup != null) worldCup.enabled = false;
            var cup = CreateHeldProp("Coffee_Held", new Color(.80f, .40f, .20f), new Vector3(.28f, .10f, 0f), .42f);
            _spriteAnimation?.SetActionPose("Edelzio_DrinkCoffee");
            // Três goles deixam claro que Edelzio tomou toda a xícara, não apenas um gole rápido.
            for (int sip = 0; sip < 3; sip++)
            {
                if (cup != null) cup.transform.localPosition = new Vector3(.34f, .28f, 0f);
                yield return new WaitForSeconds(.18f);
                if (cup != null) cup.transform.localPosition = new Vector3(.28f, .10f, 0f);
                yield return new WaitForSeconds(.12f);
            }
            // O copo some da mão e volta ao ponto original como uma xícara vazia.
            if (cup != null)
            {
                Destroy(cup);
            }
            if (worldCup != null) worldCup.enabled = true;
            _spriteAnimation?.ClearActionPose();
            EndAction();
            onComplete?.Invoke();
        }

        private IEnumerator PoseRoutine(string pose, float duration, float verticalScale)
        {
            BeginAction();
            Vector3 originalScale = transform.localScale;
            _spriteAnimation?.SetActionPose(pose);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
                transform.localScale = new Vector3(originalScale.x * (1f + .06f * t), originalScale.y * Mathf.Lerp(1f, verticalScale, t), originalScale.z);
                yield return null;
            }
            transform.localScale = originalScale;
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

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, destination, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
        }

        private IEnumerator MoveToPosition(Vector3 destination, float duration)
        {
            Vector3 start = transform.position;
            if (Vector3.Distance(start, destination) > 2.2f) yield break;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, destination, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
        }

        private GameObject CreateHeldProp(string spriteId, Color color, Vector3 localPosition, float scale)
        {
            var prop = new GameObject(spriteId);
            prop.transform.SetParent(transform, false);
            prop.transform.localPosition = localPosition;
            prop.transform.localScale = Vector3.one * scale;
            var renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create(spriteId, color);
            renderer.sortingOrder = _renderer != null ? _renderer.sortingOrder + 1 : 6;
            return prop;
        }

        private void BeginAction()
        {
            _isActing = true;
            _player?.SetInputLocked(true);
            if (_body != null)
            {
                _bodyWasSimulated = _body.simulated;
                _body.linearVelocity = Vector2.zero;
                _body.simulated = false;
            }
        }

        private void EndAction(bool unlockInput = true)
        {
            _isActing = false;
            if (_body != null) _body.simulated = _bodyWasSimulated;
            if (unlockInput) _player?.SetInputLocked(false);
        }
    }
}
