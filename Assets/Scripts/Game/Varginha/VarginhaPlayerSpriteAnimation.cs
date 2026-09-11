using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Máquina de estados pixelada equivalente a um Animator Idle/Run.
    /// As transições são instantâneas para não introduzir atraso visual.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class VarginhaPlayerSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private float idleFrameRate = 2f;
        [SerializeField] private float runFrameRate = 11f;

        private EdelzioTopDownController _controller;
        private SpriteRenderer _renderer;
        private Sprite[] _idleFrames;
        private Sprite[] _runFrames;
        private Sprite[][] _directionalWalkFrames;
        // Mantém o personagem visível mesmo durante uma recompilação/hot reload do Editor.
        private Sprite _fallbackSprite;
        private Sprite _actionPose;
        private Vector3 _lastValidScale;
        private bool _isRunning;
        private float _time;

        private void Awake()
        {
            _controller = GetComponent<EdelzioTopDownController>();
            _renderer = GetComponent<SpriteRenderer>();
            _fallbackSprite = _renderer != null ? _renderer.sprite : null;
            _lastValidScale = IsVisibleScale(transform.localScale) ? transform.localScale : new Vector3(1.08f, 1.08f, 1f);
            NormalizeScale();
            EnsureFrames();
        }

        private void OnValidate()
        {
            // Evita que uma alteração manual com algum eixo em zero deixe o Edelzio invisível.
            if (!IsVisibleScale(transform.localScale)) transform.localScale = new Vector3(1.08f, 1.08f, 1f);
        }

        private void OnEnable()
        {
            EnsureFrames();
        }

        private void EnsureFrames()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null) return;

            if (_fallbackSprite == null) _fallbackSprite = _renderer.sprite;
            Color jacket = new Color(.62f, .82f, .38f);
            if (_directionalWalkFrames == null) _directionalWalkFrames = LoadReferenceWalkFrames();
            Sprite[] referenceFrames = _directionalWalkFrames != null ? _directionalWalkFrames[0] : null;
            if (_idleFrames == null || _idleFrames.Length == 0)
            {
                _idleFrames = referenceFrames != null
                    ? new[] { referenceFrames[0], referenceFrames[0] }
                    : new[]
                {
                    VarginhaPixelArtSprites.Create("Edelzio_IdleA", jacket),
                    VarginhaPixelArtSprites.Create("Edelzio_IdleB", jacket)
                };
            }
            if (_runFrames == null || _runFrames.Length == 0)
            {
                _runFrames = referenceFrames ?? new[]
                {
                    VarginhaPixelArtSprites.Create("Edelzio_RunA", jacket),
                    VarginhaPixelArtSprites.Create("Edelzio_RunB", jacket)
                };
            }

            if (_fallbackSprite == null && _idleFrames != null && _idleFrames.Length > 0)
                _fallbackSprite = _idleFrames[0];

            _renderer.enabled = true;
            _renderer.color = Color.white;
            _renderer.sortingOrder = Mathf.Max(_renderer.sortingOrder, 5);
            if (_renderer.sprite == null) _renderer.sprite = _fallbackSprite;
        }

        private void Update()
        {
            NormalizeScale();
            EnsureFrames();
            if (_renderer == null) return;

            if (_actionPose != null)
            {
                _renderer.enabled = true;
                _renderer.sprite = _actionPose;
                return;
            }

            SetRunning(_controller != null && _controller.IsMoving);
            _time += Time.deltaTime;
            Sprite[] directionalFrames = GetDirectionalFrames();
            Sprite[] frames = directionalFrames ?? (_isRunning ? _runFrames : _idleFrames);
            float frameRate = _isRunning ? runFrameRate : idleFrameRate;
            if (frames == null || frames.Length == 0)
            {
                _renderer.enabled = true;
                _renderer.sprite = _fallbackSprite;
                return;
            }
            int frameIndex = _isRunning ? Mathf.FloorToInt(_time * frameRate) % frames.Length : 0;
            _renderer.flipX = false;
            _renderer.sprite = frames[frameIndex];
        }

        public void SetRunning(bool running)
        {
            if (_isRunning == running) return;
            _isRunning = running;
            _time = 0f; // duração de transição zero, como no Animator do tutorial.
        }

        /// <summary>Congela brevemente o ciclo idle/run em uma pose de interação.</summary>
        public void SetActionPose(string poseId)
        {
            _actionPose = VarginhaPixelArtSprites.Create(poseId, new Color(.20f, .85f, 1f));
        }

        public void ClearActionPose()
        {
            _actionPose = null;
            _time = 0f;
        }

        private void NormalizeScale()
        {
            if (IsVisibleScale(transform.localScale))
            {
                _lastValidScale = transform.localScale;
                return;
            }

            transform.localScale = IsVisibleScale(_lastValidScale)
                ? _lastValidScale
                : new Vector3(1.08f, 1.08f, 1f);
        }

        private static bool IsVisibleScale(Vector3 scale)
        {
            return Mathf.Abs(scale.x) >= .02f && Mathf.Abs(scale.y) >= .02f && Mathf.Abs(scale.z) >= .02f;
        }

        private Sprite[] GetDirectionalFrames()
        {
            if (_directionalWalkFrames == null || _controller == null) return null;
            Vector2 facing = _controller.FacingDirection;
            float absX = Mathf.Abs(facing.x);
            float absY = Mathf.Abs(facing.y);
            // As diagonais preservam a direção dominante: 8 direções de movimento sem voltar ao sprite de baixo.
            int direction = absY >= absX
                ? (facing.y >= 0f ? 3 : 0) // cima / baixo
                : (facing.x < 0f ? 1 : 2); // esquerda / direita
            return _directionalWalkFrames[direction];
        }

        private static Sprite[][] LoadReferenceWalkFrames()
        {
            var sheet = Resources.Load<Texture2D>("Varginha/EdelzioTopDownV2");
            if (sheet == null) return null;

            sheet.filterMode = FilterMode.Point;
            float cellWidth = sheet.width / 4f;
            float cellHeight = sheet.height / 4f;
            var directions = new Sprite[4][];
            // Linhas do arquivo: baixo, esquerda, direita, cima. Cada uma traz quatro quadros de caminhada.
            for (int direction = 0; direction < directions.Length; direction++)
            {
                directions[direction] = new Sprite[4];
                float unityRow = 3f - direction;
                for (int frame = 0; frame < directions[direction].Length; frame++)
                {
                    directions[direction][frame] = Sprite.Create(sheet,
                        new Rect(frame * cellWidth, unityRow * cellHeight, cellWidth, cellHeight),
                        new Vector2(.5f, .5f), Mathf.Max(cellWidth, cellHeight));
                    directions[direction][frame].name = "Edelzio_" + direction + "_Andar_" + frame;
                }
            }
            return directions;
        }
    }
}
