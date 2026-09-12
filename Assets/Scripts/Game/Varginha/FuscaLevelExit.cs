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
                animation.Depart(() => animationFinished = true);
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
            if (_doorRenderer == null)
            {
                var door = new GameObject("Porta_do_Fusca");
                door.transform.SetParent(transform, false);
                door.transform.localPosition = new Vector3(-.30f, -.03f, 0f);
                // A folha ocupa a área real da porta, mantendo o pixel art nítido
                // mesmo quando a carroceria está em escala 2.6.
                door.transform.localScale = new Vector3(.62f, .88f, 1f);
                _doorRenderer = door.AddComponent<SpriteRenderer>();
                _doorRenderer.sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Closed", new Color(.20f, .65f, .88f));
                var carRenderer = GetComponent<SpriteRenderer>();
                _doorRenderer.sortingOrder = carRenderer != null ? carRenderer.sortingOrder + 1 : 5;
            }

            _doorRenderer.enabled = true;
            _doorRenderer.sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Closed", new Color(.20f, .65f, .88f));
            _doorRenderer.transform.localRotation = Quaternion.identity;
            float elapsed = 0f;
            const float duration = .34f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                if (t > .25f && t < .70f)
                    _doorRenderer.sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Ajar", new Color(.20f, .65f, .88f));
                else if (t >= .70f)
                    _doorRenderer.sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Open", new Color(.20f, .65f, .88f));
                ApplyDoorPose(Mathf.Lerp(0f, -32f, eased));
                yield return null;
            }
            _doorRenderer.sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Open", new Color(.20f, .65f, .88f));
            ApplyDoorPose(-32f);
        }

        private IEnumerator CloseDoorRoutine()
        {
            if (_doorRenderer == null) yield break;
            float elapsed = 0f;
            const float duration = .27f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t * (3f - 2f * t);
                if (t < .55f)
                    _doorRenderer.sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Ajar", new Color(.20f, .65f, .88f));
                else
                    _doorRenderer.sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Closed", new Color(.20f, .65f, .88f));
                ApplyDoorPose(Mathf.Lerp(-32f, 0f, eased));
                yield return null;
            }
            _doorRenderer.sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Closed", new Color(.20f, .65f, .88f));
            _doorRenderer.transform.localRotation = Quaternion.identity;
            _doorRenderer.transform.localPosition = new Vector3(-.30f, -.03f, 0f);
            _doorRenderer.enabled = false;
        }

        private void ApplyDoorPose(float angleDegrees)
        {
            if (_doorRenderer == null) return;
            const float closedX = -.30f;
            const float closedY = -.03f;
            const float halfWidth = .31f;
            float radians = angleDegrees * Mathf.Deg2Rad;
            Vector2 hinge = new Vector2(closedX - halfWidth, closedY);
            Vector2 center = hinge + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * halfWidth;
            _doorRenderer.transform.localPosition = center;
            _doorRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angleDegrees);
        }
    }
}
