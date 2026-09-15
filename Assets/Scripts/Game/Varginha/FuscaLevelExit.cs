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
        private FuscaDoorMotion _doorMotion;

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
            // Only consume after both prerequisites succeeded, never on a failed attempt.
            edelzio.TryConsumeInventoryItem(1);
            edelzio.TryConsumeInventoryItem(2);
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
            Vector3 door = _doorMotion.EntryPosition;
            Vector3 seat = _doorMotion.SeatPosition;
            const float enterDuration = .42f;
            float elapsed = 0f;

            while (elapsed < enterDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / enterDuration);
                edelzio.transform.position = Vector3.Lerp(start, door, Mathf.SmoothStep(0, 1, t));
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
                edelzio.transform.position = Vector3.Lerp(door, seat, Mathf.SmoothStep(0, 1, t));
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
            _doorMotion = GetComponent<FuscaDoorMotion>() ?? gameObject.AddComponent<FuscaDoorMotion>();
            float elapsed = 0f;
            const float duration = .72f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * t * (t * (6f * t - 15f) + 10f);
                _doorMotion.SetOpenAmount(eased);
                yield return null;
            }
            _doorMotion.SetOpenAmount(1);
            yield return new WaitForSeconds(.12f);
        }

        private IEnumerator CloseDoorRoutine()
        {
            if (_doorMotion == null) yield break;
            float elapsed = 0f;
            const float duration = .58f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);
                _doorMotion.SetOpenAmount(1f - eased);
                yield return null;
            }
            _doorMotion.SetOpenAmount(0);
        }
    }
}
