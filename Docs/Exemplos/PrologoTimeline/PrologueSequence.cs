using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Game.Varginha.Prologue
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class PrologueSequence : MonoBehaviour
    {
        [SerializeField] private PrologueChildController child;
        [SerializeField] private Transform captureOrigin;
        [SerializeField] private Transform motionRig;
        [SerializeField] private CanvasGroup loadingCover;
        [SerializeField] private PlayableDirector anomalyDirector;
        [SerializeField, Min(0.01f)] private float takeoverSeconds = 1.5f;
        [SerializeField] private string nextScene = "FaseTopView_Varginha";
        [SerializeField] private UnityEvent onAnomalyStarted = new UnityEvent();
        [SerializeField] private UnityEvent onCancelled = new UnityEvent();

        private PlayableDirector _director;
        private Vector3 _savedOrigin;
        private Vector3 _savedRig;
        private Vector3 _savedChild;
        private bool _running;
        private bool _timelineStarted;
        private bool _endingMarked;
        private bool _loading;

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
            _director.playOnAwake = false;
            _director.extrapolationMode = DirectorWrapMode.None;
            _director.timeUpdateMode = DirectorUpdateMode.GameTime;
        }

        private void OnEnable() => _director.stopped += OnStopped;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PrologueChildController>() == child) Begin();
        }

        // Ligue este metodo ao evento de uma interacao ou a um gatilho de proximidade.
        public void Begin()
        {
            if (!isActiveAndEnabled || _running || _loading || Time.timeScale <= 0f) return;
            if (child == null || !child.isActiveAndEnabled || captureOrigin == null
                || motionRig == null || loadingCover == null || _director.playableAsset == null
                || child.transform.parent != motionRig || motionRig.parent != captureOrigin)
            {
                Debug.LogError("Confira as referencias e a hierarquia do prologo.", this);
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(nextScene))
            {
                Debug.LogError("Adicione a cena ao Build Profile: " + nextScene, this);
                return;
            }

            _savedOrigin = captureOrigin.position;
            _savedRig = motionRig.localPosition;
            _savedChild = child.transform.position;
            _running = true;
            _timelineStarted = _endingMarked = false;
            loadingCover.alpha = 0f;
            loadingCover.blocksRaycasts = false;
            onAnomalyStarted.Invoke();
            if (anomalyDirector != null)
            {
                anomalyDirector.time = 0;
                anomalyDirector.Play();
            }
            StartCoroutine(TakeOver());
        }

        private IEnumerator TakeOver()
        {
            float elapsed = 0f;
            while (elapsed < takeoverSeconds)
            {
                elapsed += Time.deltaTime;
                child.InputStrength = 1f - Mathf.Clamp01(elapsed / takeoverSeconds);
                yield return null;
            }

            if (anomalyDirector != null) anomalyDirector.Stop();
            child.InputStrength = 0f;
            child.BeginCinematic();
            Vector3 worldPosition = child.transform.position;
            captureOrigin.position = worldPosition;
            motionRig.localPosition = Vector3.zero;
            child.transform.localPosition = Vector3.zero;

            _timelineStarted = true;
            _director.time = 0;
            _director.Play();
        }

        // Signal Receiver: sinal final da narrativa, pouco antes do fim do asset.
        public void MarkNarrativeComplete()
        {
            if (_running && _timelineStarted) _endingMarked = true;
        }

        private void OnStopped(PlayableDirector director)
        {
            if (director != _director || !_running || !_timelineStarted || _loading) return;
            if (_endingMarked)
            {
                _loading = true;
                // Esta cobertura nao pode ter Animator nem binding na Timeline.
                loadingCover.alpha = 1f;
                loadingCover.blocksRaycasts = true;
            }
            StartCoroutine(FinishAfterGraphStops());
        }

        private IEnumerator FinishAfterGraphStops()
        {
            // stopped ocorre antes dos callbacks finais do grafo de Playables.
            yield return null;
            if (!_running) yield break;
            if (!_endingMarked)
            {
                Debug.LogWarning("Timeline interrompida ou sinal final ausente.", this);
                Cancel();
                yield break;
            }

            AsyncOperation operation = null;
            try
            {
                operation = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
            }
            catch (Exception error)
            {
                Debug.LogException(error, this);
            }
            if (operation == null)
            {
                _loading = false;
                Cancel();
                yield break;
            }
            yield return operation;
        }

        // Use Cancel para abortar: nao chame Stop diretamente depois do sinal final.
        public void Cancel()
        {
            if (!_running || _loading) return;
            _running = false;
            _endingMarked = _timelineStarted = false;
            StopAllCoroutines();
            _director.Stop();
            if (anomalyDirector != null) anomalyDirector.Stop();
            if (captureOrigin != null) captureOrigin.position = _savedOrigin;
            if (motionRig != null) motionRig.localPosition = _savedRig;
            if (child != null)
            {
                child.transform.position = _savedChild;
                child.ResumeGameplay();
            }
            if (loadingCover != null)
            {
                loadingCover.alpha = 0f;
                loadingCover.blocksRaycasts = false;
            }
            onCancelled.Invoke();
        }

        private void OnDisable()
        {
            if (_director != null) _director.stopped -= OnStopped;
            Cancel();
        }
    }
}
