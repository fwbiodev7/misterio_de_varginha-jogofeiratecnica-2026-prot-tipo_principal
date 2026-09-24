using Game.Managers;
using Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Varginha
{
    /// <summary>One death event, then an explicit retry or return to the menu.</summary>
    [DisallowMultipleComponent]
    public sealed class VarginhaGameOverFlow : MonoBehaviour
    {
        public const string PhaseOneScene = "FaseTopView_Varginha";
        public const string MenuScene = "Menu_MisterioDeVarginha";

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
            _player?.SetInputLocked(true);
            VarginhaGameHUD.Instance?.ShowGameOver(_health != null && _health.IsDead
                ? "EDELZIO FOI DERROTADO" : "A SANIDADE DE EDELZIO SE ESGOTOU");
            Time.timeScale = 0f;
        }

        public static void RetryCurrentPhase()
        {
            LoadInvestigation(SceneManager.GetActiveScene().name);
        }

        public static void ReturnToPhaseOne() => LoadInvestigation(PhaseOneScene);

        private static void LoadInvestigation(string scene)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene)) return;
            Time.timeScale = 1f;
            GameManager.Instance?.StartGame();
            SceneManager.LoadScene(scene);
        }

        public static void ReturnToMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(MenuScene)) return;
            Time.timeScale = 1f;
            GameManager.Instance?.ReturnToMenu();
            SceneManager.LoadScene(MenuScene);
        }
    }
}
