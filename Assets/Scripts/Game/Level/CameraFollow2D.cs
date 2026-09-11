using UnityEngine;
using Game.Player;

namespace Game.Level
{
    /// <summary>
    /// Câmera 2D que segue suavemente o jogador com amortecimento e limites.
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(2f, 1.5f, -10f);

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.25f;
        [SerializeField] private bool limitMinY = true;
        [SerializeField] private float minY = -2f;

        private Vector3 _velocity = Vector3.zero;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Start()
        {
            if (target == null)
            {
                var player = Object.FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPos = target.position + offset;
            if (limitMinY && targetPos.y < minY)
            {
                targetPos.y = minY;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime);
        }
    }
}
