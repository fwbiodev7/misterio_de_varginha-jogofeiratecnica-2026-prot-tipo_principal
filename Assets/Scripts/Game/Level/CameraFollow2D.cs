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
        private Camera _camera;
        private bool _bounded;
        private Rect _mapBounds;
        private float _preferredSize;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public void ConfigureTopDown(Transform followTarget)
        {
            target = followTarget;
            offset = new Vector3(0, 0, -10);
            limitMinY = false;
            _velocity = Vector3.zero;
        }

        public void ConfigureMap(Transform followTarget, Rect bounds, float size)
        {
            ConfigureTopDown(followTarget);
            _camera = GetComponent<Camera>();
            _mapBounds = bounds;
            _preferredSize = size;
            _bounded = _camera != null && bounds.width > 0f && bounds.height > 0f;
            FitViewport();
            if (target != null) transform.position = ConstrainPosition(target.position + offset);
        }

        private void FitViewport()
        {
            if (!_bounded) return;
            _camera.orthographic = true;
            // Wider windows must still fit inside the map, including after a resize.
            _camera.orthographicSize = Mathf.Min(_preferredSize, Mathf.Min(_mapBounds.height * .5f,
                _mapBounds.width * .5f / Mathf.Max(.01f, _camera.aspect)));
        }

        public Vector3 ConstrainPosition(Vector3 position)
        {
            if (!_bounded) return position;
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            position.x = Mathf.Clamp(position.x, _mapBounds.xMin + halfWidth, _mapBounds.xMax - halfWidth);
            position.y = Mathf.Clamp(position.y, _mapBounds.yMin + halfHeight, _mapBounds.yMax - halfHeight);
            return position;
        }

        private void Start()
        {
            if (Game.Varginha.VarginhaCameraFraming.Configure(this, gameObject.scene.name)) return;
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

            FitViewport();
            Vector3 targetPos = target.position + offset;
            if (limitMinY && targetPos.y < minY)
            {
                targetPos.y = minY;
            }

            targetPos = ConstrainPosition(targetPos);
            transform.position = ConstrainPosition(Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, smoothTime));
        }
    }
}
