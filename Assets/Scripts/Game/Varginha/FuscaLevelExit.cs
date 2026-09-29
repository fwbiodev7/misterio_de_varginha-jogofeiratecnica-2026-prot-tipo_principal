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

            BoardImmediately(edelzio);
            VarginhaGameHUD.Instance?.ShowDialogue("Edelzio", "O motor pegou. Segure firme - vamos sair daqui!");
            yield return new WaitForSeconds(0.65f);
            VarginhaGameHUD.Instance?.CloseDialogue();

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

        private void BoardImmediately(EdelzioTopDownController edelzio)
        {
            var oldDoor = transform.Find("Porta_Fusca_Dobradica_Dianteira");
            if (oldDoor != null) oldDoor.gameObject.SetActive(false);
            var motion = GetComponent<FuscaDoorMotion>();
            if (motion != null) motion.enabled = false;
            var flashlight = edelzio.GetComponent<EdelzioFlashlight>();
            if (flashlight != null) flashlight.enabled = false;
            edelzio.transform.position = transform.TransformPoint(new Vector3(-.02f, .055f, 0));
            edelzio.transform.SetParent(transform, true);
            var animation = edelzio.GetComponent<VarginhaPlayerSpriteAnimation>();
            if (animation != null) animation.enabled = false;
            foreach (var renderer in edelzio.GetComponentsInChildren<Renderer>()) renderer.enabled = false;
            foreach (var collider in edelzio.GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        }
    }
}
