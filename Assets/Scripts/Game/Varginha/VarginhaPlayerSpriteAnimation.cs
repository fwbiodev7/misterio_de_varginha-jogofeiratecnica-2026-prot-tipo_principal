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
        [SerializeField] private float idleFrameRate = 2.4f;
        [SerializeField] private float runFrameRate = 12.5f;

        private EdelzioTopDownController _controller;
        private SpriteRenderer _renderer;
        private Sprite[] _idleFrames;
        private Sprite[] _runFrames;
        private Sprite[][] _directionalWalkFrames;
        private Sprite[][] _directionalIdleFrames;
        private Sprite[][] _directionalRunFrames;
        // Mantém o personagem visível mesmo durante uma recompilação/hot reload do Editor.
        private Sprite _fallbackSprite;
        private Sprite _actionPose;
        private Sprite[] _actionFrames;
        private SpriteRenderer _beardRenderer;
        private bool _attackPose;
        public bool IsSeated { get; private set; }
        public bool IsDrinking { get; private set; }
        public bool HasActionPose => _actionPose != null;
        public Vector2 ActionFacingDirection { get; private set; } = Vector2.down;

        public void SetCombatPose(Sprite pose, Vector2 direction)
        {
            _actionPose = pose;
            _attackPose = true;
            ActionFacingDirection = direction;
            IsSeated = IsDrinking = false;
            if (_renderer != null && pose != null) { _renderer.flipX = false; _renderer.sprite = pose; }
            SetBeard(true, DirectionIndex(direction));
        }
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
            if (_directionalIdleFrames == null && _directionalWalkFrames != null)
            {
                _directionalIdleFrames = new Sprite[_directionalWalkFrames.Length][];
                for (int direction = 0; direction < _directionalWalkFrames.Length; direction++)
                {
                    Sprite[] walk = _directionalWalkFrames[direction];
                    _directionalIdleFrames[direction] = walk != null && walk.Length >= 4
                        ? new[] { walk[0], walk[1], walk[0], walk[3] }
                        : null;
                }
            }
            if (_directionalRunFrames == null && _directionalWalkFrames != null)
            {
                _directionalRunFrames = new Sprite[_directionalWalkFrames.Length][];
                for (int direction = 0; direction < _directionalWalkFrames.Length; direction++)
                {
                    Sprite[] walk = _directionalWalkFrames[direction];
                    // O retorno passa por poses intermediárias em vez de saltar
                    // diretamente do quarto para o primeiro quadro.
                    _directionalRunFrames[direction] = walk != null && walk.Length >= 4
                        ? new[] { walk[0], walk[1], walk[2], walk[3], walk[2], walk[1], walk[0], walk[1] }
                        : null;
                }
            }
            Sprite[] referenceFrames = _directionalWalkFrames != null ? _directionalWalkFrames[0] : null;
            if (_idleFrames == null || _idleFrames.Length == 0)
            {
                _idleFrames = referenceFrames != null
                    ? new[] { referenceFrames[0], referenceFrames[1], referenceFrames[0], referenceFrames[3] }
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

            // A cena antiga pode ter um sprite procedural verde serializado no
            // SpriteRenderer. Assim que a folha original fica disponível ela
            // passa a ser a fonte principal, mesmo que já exista um sprite.
            if (referenceFrames != null && referenceFrames.Length > 0)
                _fallbackSprite = referenceFrames[0];
            else if (_fallbackSprite == null && _idleFrames != null && _idleFrames.Length > 0)
                _fallbackSprite = _idleFrames[0];

            _renderer.enabled = true;
            _renderer.color = Color.white;
            _renderer.sortingOrder = Mathf.Max(_renderer.sortingOrder, 5);
            if (_actionPose == null && referenceFrames != null && referenceFrames.Length > 0)
                _renderer.sprite = referenceFrames[0];
            else if (_renderer.sprite == null) _renderer.sprite = _fallbackSprite;
            EnsureBeardLayer();
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
                SetBeard(_attackPose, DirectionIndex(ActionFacingDirection));
                return;
            }

            bool moving = _controller != null && _controller.IsMoving;
            SetRunning(moving);
            // O relógio não depende do frame rate e continua acompanhando uma
            // câmera em câmera lenta sem saltar quadros de pixel art.
            _time += Time.unscaledDeltaTime;
            Sprite[] directionalFrames = _isRunning ? GetDirectionalFrames() : GetDirectionalIdleFrames();
            Sprite[] frames = directionalFrames ?? (_isRunning ? _runFrames : _idleFrames);
            float frameRate = _isRunning ? runFrameRate : idleFrameRate;
            if (frames == null || frames.Length == 0)
            {
                _renderer.enabled = true;
                _renderer.sprite = _fallbackSprite;
                return;
            }
            int frameIndex = Mathf.FloorToInt(_time * frameRate) % frames.Length;
            _renderer.flipX = false;
            _renderer.sprite = frames[frameIndex];
            SetBeard(false, DirectionIndex(_controller != null ? _controller.FacingDirection : Vector2.down));
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
            ActionFacingDirection = Vector2.down;
            _attackPose = false;
            EnsureFrames();
            IsSeated = poseId == "Edelzio_Sit" || poseId == "Edelzio_UseNotebook";
            IsDrinking = poseId == "Edelzio_DrinkCoffee";
            int index = poseId == "Edelzio_Crouch" ? 4 : poseId == "Edelzio_Reach" ? 5 :
                poseId == "Edelzio_Sit" ? 6 : poseId == "Edelzio_UseNotebook" ? 7 : 0;
            _actionPose = _actionFrames != null ? _actionFrames[index] : _fallbackSprite;
            if (_renderer != null) { _renderer.flipX = false; _renderer.sprite = _actionPose; }
            SetBeard(false, DirectionIndex(ActionFacingDirection));
        }

        public void SetCoffeeFrame(int frame)
        {
            SetActionPose("Edelzio_DrinkCoffee");
            if (_actionFrames != null) _actionPose = _actionFrames[Mathf.Clamp(frame, 0, 3)];
            if (_renderer != null) _renderer.sprite = _actionPose;
        }

        public void ClearActionPose()
        {
            _actionPose = null;
            _attackPose = false;
            IsSeated = false;
            IsDrinking = false;
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
            return _directionalRunFrames != null && _directionalRunFrames[direction] != null
                ? _directionalRunFrames[direction]
                : _directionalWalkFrames[direction];
        }

        private void EnsureBeardLayer()
        {
            if (_renderer == null) return;
            // Nunca reutiliza um componente destruído durante hot-reload. O
            // objeto legado tinha o nome da camada, mas não possuía
            // SpriteRenderer e gerava MissingComponentException no Play Mode.
            if (_beardRenderer == null || _beardRenderer.gameObject == null)
            {
                var beard = transform.Find("Edelzio_Barba_Overlay_Runtime");
                if (beard == null)
                {
                    var beardObject = new GameObject("Edelzio_Barba_Overlay_Runtime");
                    beardObject.transform.SetParent(transform, false);
                    beard = beardObject.transform;
                }
                var existingRenderer = beard.GetComponent<SpriteRenderer>();
                _beardRenderer = existingRenderer != null
                    ? existingRenderer
                    : beard.gameObject.AddComponent<SpriteRenderer>();
            }
            if (_beardRenderer == null) return;
            _beardRenderer.sortingLayerID = _renderer.sortingLayerID;
            _beardRenderer.sortingOrder = _renderer.sortingOrder + 1;
            _beardRenderer.enabled = _renderer.enabled;
        }

        private void SetBeard(bool attackSheet, int direction)
        {
            EnsureBeardLayer();
            if (_beardRenderer == null) return;
            if (_renderer == null || _beardRenderer == null) return;
            _beardRenderer.sprite = VarginhaPixelArtSprites.CreateEdelzioBeard(attackSheet, direction);
            _beardRenderer.enabled = _renderer != null && _renderer.enabled;
        }

        private static int DirectionIndex(Vector2 direction)
        {
            if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x)) return direction.y >= 0f ? 3 : 0;
            return direction.x < 0f ? 1 : 2;
        }

        private void OnDisable()
        {
            if (_beardRenderer != null) _beardRenderer.enabled = false;
        }

        private Sprite[] GetDirectionalIdleFrames()
        {
            if (_directionalIdleFrames == null || _controller == null) return null;
            Vector2 facing = _controller.FacingDirection;
            float absX = Mathf.Abs(facing.x);
            float absY = Mathf.Abs(facing.y);
            int direction = absY >= absX
                ? (facing.y >= 0f ? 3 : 0)
                : (facing.x < 0f ? 1 : 2);
            return _directionalIdleFrames[direction];
        }

        private Sprite[][] LoadReferenceWalkFrames()
        {
            var sheet = Resources.Load<Texture2D>("Varginha/EdelzioTopDownV3");
            if (sheet == null) return null;

            sheet.filterMode = FilterMode.Point;
            float cellWidth = sheet.width / 4f;
            float cellHeight = sheet.height / 6f;
            // Aumenta somente a arte, preservando o collider e as passagens da casa.
            float ppu = Mathf.Max(cellWidth, cellHeight) / 1.45f;
            _actionFrames = new Sprite[8];
            for (int i = 0; i < 8; i++)
                _actionFrames[i] = Sprite.Create(sheet,
                    new Rect((i % 4) * cellWidth, (i < 4 ? 1 : 0) * cellHeight, cellWidth, cellHeight),
                    new Vector2(.5f, .5f), ppu);
            var directions = new Sprite[4][];
            // Linhas do arquivo: baixo, esquerda, direita, cima. Cada uma traz quatro quadros de caminhada.
            for (int direction = 0; direction < directions.Length; direction++)
            {
                directions[direction] = new Sprite[4];
                float unityRow = 5f - direction;
                for (int frame = 0; frame < directions[direction].Length; frame++)
                {
                    directions[direction][frame] = Sprite.Create(sheet,
                        new Rect(frame * cellWidth, unityRow * cellHeight, cellWidth, cellHeight),
                        new Vector2(.5f, .5f), ppu);
                    directions[direction][frame].name = "Edelzio_" + direction + "_Andar_" + frame;
                }
            }
            return directions;
        }
    }
}
