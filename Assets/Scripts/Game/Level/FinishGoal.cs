using UnityEngine;
using Game.Managers;
using Game.Player;

namespace Game.Level
{
    /// <summary>
    /// Ponto de chegada / Final da fase.
    /// Ao colidir com o jogador, completa o nível e dispara os eventos de vitória.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FinishGoal : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int bonusScore = 500;
        [SerializeField] private string playerTag = "Player";

        [Header("Visual Effects")]
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseScale = 0.15f;

        private bool _isFinished;
        private Vector3 _baseScale;

        public bool IsFinished => _isFinished;

        private void Start()
        {
            _baseScale = transform.localScale;
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            if (_isFinished) return;

            // Animação de pulso visual para destacar a bandeira/meta
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = _baseScale * scale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isFinished) return;

            if (other.CompareTag(playerTag) || other.GetComponent<PlayerController>() != null)
            {
                CompleteLevel();
            }
        }

        public void CompleteLevel()
        {
            if (_isFinished) return;
            _isFinished = true;

            Debug.Log("🏁🎉 [FinishGoal] FASE CONCLUÍDA COM SUCESSO!");

            ScoreManager.Instance?.AddScore(bonusScore);
            GameManager.Instance?.TriggerWin();
        }
    }
}
