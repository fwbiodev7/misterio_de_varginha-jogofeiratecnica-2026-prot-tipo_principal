using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Managers;

namespace Game.Varginha
{
    /// <summary>Interactive escape vehicle for the Varginha level.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class FuscaLevelExit : MonoBehaviour
    {
        [Header("Próxima fase")]
        [SerializeField] private string nextSceneName = "Fase2_Escola_Resgate";
        [SerializeField] private bool transitionToPhase2 = true;

        private bool _isEscaped;
        private SpriteRenderer _doorRenderer;
        private Transform _doorPivot;
        private Sprite _doorClosed;
        private Sprite _doorAjar;
        private Sprite _doorOpen;
        private readonly Color _doorColor = new Color(.20f, .65f, .88f);

        public void TryEscape(EdelzioTopDownController edelzio)
        {
            if (_isEscaped) return;
            if (!edelzio.HasFuscaKey)
            {
                VarginhaGameHUD.Instance?.ShowDialogue("Fusca Trancado", "O Fusca esta trancado. Encontre a chave antes de sair.");
                return;
            }
            if (!edelzio.HasResearchNotebook)
            {
                VarginhaGameHUD.Instance?.ShowDialogue("Edelzio", "Nao posso partir sem o caderno de pesquisas de 1996.");
                return;
            }

            _isEscaped = true;
            Debug.Log("[Fusca] Edelzio deu a partida e acelerou pela noite de Varginha.");
            StartCoroutine(EscapeRoutine(edelzio));
        }

        private IEnumerator EscapeRoutine(EdelzioTopDownController edelzio)
        {
            // Trava a entrada antes da animação, sem deixar a física desfazer o deslocamento visual.
            edelzio.SetInputLocked(true);
            edelzio.enabled = false;
            var playerBody = edelzio.GetComponent<Rigidbody2D>();
            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
                playerBody.simulated = false;
            }
            // O notebook e a mochila ficam escondidos durante a tomada; antes
            // apareciam como um retângulo azul grande atravessando a porta.
            edelzio.SetCarriedItemsVisible(false);
            var exitCollider = GetComponent<Collider2D>();
            if (exitCollider != null) exitCollider.enabled = false;

            VarginhaGameHUD.Instance?.ShowDialogue("Edelzio", "O motor pegou. Segure firme - vamos sair daqui!");
            yield return new WaitForSeconds(0.65f);
            VarginhaGameHUD.Instance?.CloseDialogue();
            yield return OpenDoorRoutine();
            yield return MoveEdelzioIntoFusca(edelzio);
            yield return CloseDoorRoutine();

            var animation = GetComponent<FuscaDepartureAnimation>();
            if (animation != null)
            {
                bool animationFinished = false;
                // A cinematics de viagem assume o trajeto longo; a fase real só
                // mostra o carro deixando a vaga, sem jogá-lo para fora do mapa.
                animation.Depart(() => animationFinished = true, 4.2f);
                yield return new WaitUntil(() => animationFinished);
            }

            // A fuga da casa conclui a cinematica da Fase 1 e abre a chegada à escola.
            // O fallback mantém cenas antigas jogáveis caso a nova cena ainda não esteja no build.
            if (transitionToPhase2 && Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                Time.timeScale = 1f;
                VarginhaTravelCinematic.Begin(false);
                yield break;
            }

            ScoreManager.Instance?.AddScore(1000);
            GameManager.Instance?.TriggerWin();
            VarginhaGameHUD.Instance?.ShowVictory(
                "ATO I: O CHAMADO - CONCLUIDO!",
                "Edelzio liga o motor do Fusca e acelera pela estrada escura de Varginha!\n" +
                "O radio do carro chia e uma gravacao de 1996 ecoa: 'Nao deixa ela sair...'\n\n" +
                "A Entidade Ancestral despertou. Proxima parada: A Escola e o Padre Fabio!");
        }

        private IEnumerator MoveEdelzioIntoFusca(EdelzioTopDownController edelzio)
        {
            Vector3 start = edelzio.transform.position;
            var carRenderer = GetComponent<SpriteRenderer>();
            float carWidth = carRenderer != null ? carRenderer.bounds.size.x : 2.2f;
            // Primeiro ele chega até a porta, depois cruza para o banco: a entrada fica legível.
            Vector3 door = transform.position + Vector3.left * (carWidth * .42f) + Vector3.down * .04f;
            Vector3 seat = transform.position + Vector3.left * (carWidth * .06f) + Vector3.down * .03f;
            const float enterDuration = .42f;
            float elapsed = 0f;

            while (elapsed < enterDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / enterDuration);
                edelzio.transform.position = Vector3.Lerp(start, door, t);
                yield return null;
            }

            // A pose sentada acompanha a entrada para que o corpo não volte ao
            // sprite de caminhada enquanto cruza o vão da porta.
            var playerAnimation = edelzio.GetComponent<VarginhaPlayerSpriteAnimation>();
            playerAnimation?.SetActionPose("Edelzio_Sit");
            elapsed = 0f;
            while (elapsed < enterDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / enterDuration);
                edelzio.transform.position = Vector3.Lerp(door, seat, t);
                yield return null;
            }

            // Ele passa a acompanhar o carro antes de ficar oculto dentro da cabine.
            edelzio.transform.position = seat;
            edelzio.transform.SetParent(transform, true);
            yield return new WaitForSeconds(.16f);
            var playerRenderer = edelzio.GetComponent<SpriteRenderer>();
            if (playerRenderer != null) playerRenderer.enabled = false;
            edelzio.SetCarriedItemsVisible(false);
            // O ciclo idle/run não pode religar o sprite depois que Edelzio entrou na cabine.
            if (playerAnimation != null) playerAnimation.enabled = false;
            var playerCollider = edelzio.GetComponent<Collider2D>();
            if (playerCollider != null) playerCollider.enabled = false;
        }

        private IEnumerator OpenDoorRoutine()
        {
            EnsureDoor();

            _doorRenderer.enabled = true;
            _doorRenderer.sprite = _doorClosed;
            ApplyDoorPose(0f);
            float elapsed = 0f;
            const float duration = .48f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                _doorRenderer.sprite = t < .34f ? _doorClosed : t < .78f ? _doorAjar : _doorOpen;
                ApplyDoorPose(Mathf.Lerp(0f, -42f, eased));
                yield return null;
            }
            _doorRenderer.sprite = _doorOpen;
            ApplyDoorPose(-42f);
        }

        private IEnumerator CloseDoorRoutine()
        {
            if (_doorRenderer == null) yield break;
            float elapsed = 0f;
            const float duration = .42f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);
                _doorRenderer.sprite = t < .62f ? _doorAjar : _doorClosed;
                ApplyDoorPose(Mathf.Lerp(-42f, 0f, eased));
                yield return null;
            }
            _doorRenderer.sprite = _doorClosed;
            ApplyDoorPose(0f);
            _doorRenderer.enabled = false;
        }

        private void EnsureDoor()
        {
            if (_doorRenderer != null) return;
            _doorClosed = VarginhaPixelArtSprites.Create("FuscaDoor_Closed", _doorColor);
            _doorAjar = VarginhaPixelArtSprites.Create("FuscaDoor_Ajar", _doorColor);
            _doorOpen = VarginhaPixelArtSprites.Create("FuscaDoor_Open", _doorColor);

            // O pivô fica na borda traseira da folha; a porta gira a partir da
            // dobradiça em vez de orbitar pelo centro do sprite.
            _doorPivot = new GameObject("Dobradiça_da_Porta_do_Fusca").transform;
            _doorPivot.SetParent(transform, false);
            _doorPivot.localPosition = new Vector3(-.54f, -.03f, 0f);
            var door = new GameObject("Porta_do_Fusca");
            door.transform.SetParent(_doorPivot, false);
            // A folha usa a mesma escala física do carro; antes o sprite 64x64
            // recebia uma escala alta demais e parecia maior que a carroceria.
            door.transform.localPosition = new Vector3(.24f, 0f, 0f);
            door.transform.localScale = new Vector3(.48f, .64f, 1f);
            _doorRenderer = door.AddComponent<SpriteRenderer>();
            _doorRenderer.sprite = _doorClosed;
            var carRenderer = GetComponent<SpriteRenderer>();
            _doorRenderer.sortingOrder = carRenderer != null ? carRenderer.sortingOrder + 1 : 5;
            _doorRenderer.enabled = false;
        }

        private void ApplyDoorPose(float angleDegrees)
        {
            if (_doorRenderer == null) return;
            if (_doorPivot != null)
            {
                _doorPivot.localPosition = new Vector3(-.54f, -.03f, 0f);
                _doorPivot.localRotation = Quaternion.Euler(0f, 0f, angleDegrees);
            }
            else
            {
                _doorRenderer.transform.localPosition = new Vector3(-.30f, -.03f, 0f);
                _doorRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angleDegrees);
            }
        }
    }
}
