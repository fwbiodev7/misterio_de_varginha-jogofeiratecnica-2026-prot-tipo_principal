using System.Collections;
using Game.Managers;
using Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Varginha
{
    /// <summary>Mostra o fim de jogo para os dois tipos de morte e reinicia a investigação pela Fase 1.</summary>
    [DisallowMultipleComponent]
    public sealed class VarginhaGameOverFlow : MonoBehaviour
    {
        public const string PhaseOneScene = "FaseTopView_Varginha";
        private const float ReturnDelay = 3.2f;

        private EdelzioTopDownController _player;
        private HealthSystem _health;
        private bool _triggered;

        private void Awake()
        {
            _player = GetComponent<EdelzioTopDownController>();
            _health = GetComponent<HealthSystem>();
        }

        private void OnEnable()
        {
            if (_player == null) _player = GetComponent<EdelzioTopDownController>();
            if (_health == null) _health = GetComponent<HealthSystem>();
            if (_player != null) _player.OnEdelzioPanicDeath += Trigger;
            if (_health != null) _health.OnDeath += Trigger;
        }

        private void OnDisable()
        {
            if (_player != null) _player.OnEdelzioPanicDeath -= Trigger;
            if (_health != null) _health.OnDeath -= Trigger;
        }

        private void Trigger()
        {
            if (_triggered || VarginhaTravelCinematic.IsTravelling) return;
            _triggered = true;
            GameManager.Instance?.TriggerGameOver();
            VarginhaGameHUD.Instance?.ShowGameOver();
            Time.timeScale = 0f;
            StartCoroutine(ReturnAfterGameOver());
        }

        private IEnumerator ReturnAfterGameOver()
        {
            yield return new WaitForSecondsRealtime(ReturnDelay);
            if (_triggered) ReturnToPhaseOne();
        }

        public static void ReturnToPhaseOne()
        {
            Time.timeScale = 1f;
            GameManager.Instance?.StartGame();
            SceneManager.LoadScene(PhaseOneScene);
        }
    }
}
