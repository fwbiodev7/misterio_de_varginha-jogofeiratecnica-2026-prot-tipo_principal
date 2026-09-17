using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Varginha
{
    /// <summary>
    /// Retículo de mira de combate estilo pixel art para alvejar ETs com o mouse.
    /// Altera visualmente para alerta/lock-on quando sobrevoa alvos hostis válidos.
    /// </summary>
    public sealed class VarginhaCombatCursor : MonoBehaviour
    {
        public static VarginhaCombatCursor Instance { get; private set; }

        private const int CursorSize = 32;
        private static readonly Vector2 Hotspot = new(16f, 16f);

        private Texture2D _normalCursorTex;
        private Texture2D _targetCursorTex;
        private bool _isLockOn;
        private bool _cursorActive;
        private Camera _mainCamera;

        public Vector2 WorldAimPosition { get; private set; }
        public VarginhaCombatTarget CurrentTarget { get; private set; }
        public bool HasTarget => CurrentTarget != null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            BuildCursorTextures();
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
            SetCombatCursor(false, true);
        }

        private void OnDisable()
        {
            RestoreSystemCursor();
            if (Instance == this) Instance = null;
        }

        private void OnDestroy()
        {
            RestoreSystemCursor();
            if (_normalCursorTex != null) Destroy(_normalCursorTex);
            if (_targetCursorTex != null) Destroy(_targetCursorTex);
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (_mainCamera == null || !_mainCamera.isActiveAndEnabled)
            {
                _mainCamera = Camera.main;
            }

            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mouseScreen = mouse.position.ReadValue();
            if (_mainCamera != null)
            {
                Vector3 world = _mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -_mainCamera.transform.position.z));
                WorldAimPosition = new Vector2(world.x, world.y);
            }

            // Em diálogos, inventário ou menus modais, devolve o cursor padrão
            bool inModal = (VarginhaGameHUD.Instance != null &&
                           (VarginhaGameHUD.Instance.IsDialogueOpen ||
                            VarginhaGameHUD.Instance.IsInventoryOpen ||
                            VarginhaGameHUD.Instance.IsVictoryOpen)) ||
                           Time.timeScale <= 0f;

            if (inModal)
            {
                if (_cursorActive) RestoreSystemCursor();
                CurrentTarget = null;
                return;
            }

            // Detecta se a mira está sobre um ET (VarginhaCombatTarget)
            VarginhaCombatTarget target = DetectTargetAtAim(WorldAimPosition);
            CurrentTarget = target;
            bool targetAcquired = target != null && !target.IsDead;

            if (!_cursorActive || targetAcquired != _isLockOn)
            {
                SetCombatCursor(targetAcquired);
            }
        }

        private VarginhaCombatTarget DetectTargetAtAim(Vector2 aim)
        {
            // Busca targets próximos à mira dentro de um raio de 1.1 unidade
            var targets = Object.FindObjectsByType<VarginhaCombatTarget>(FindObjectsInactive.Exclude);
            VarginhaCombatTarget closest = null;
            float closestDist = 1.15f;

            for (int i = 0; i < targets.Length; i++)
            {
                var t = targets[i];
                if (t == null || t.IsDead) continue;
                var col = t.GetComponent<Collider2D>();
                if (col != null && col.OverlapPoint(aim))
                {
                    return t;
                }

                float dist = Vector2.Distance(aim, t.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = t;
                }
            }

            return closest;
        }

        private void SetCombatCursor(bool lockOn, bool force = false)
        {
            if (!force && _cursorActive && _isLockOn == lockOn) return;

            _isLockOn = lockOn;
            _cursorActive = true;
            Texture2D tex = lockOn ? _targetCursorTex : _normalCursorTex;
            if (tex != null)
            {
                Cursor.SetCursor(tex, Hotspot, CursorMode.Auto);
            }
        }

        private void RestoreSystemCursor()
        {
            if (!_cursorActive) return;
            _cursorActive = false;
            _isLockOn = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void BuildCursorTextures()
        {
            if (_normalCursorTex == null)
            {
                _normalCursorTex = CreateReticleTexture(false);
            }
            if (_targetCursorTex == null)
            {
                _targetCursorTex = CreateReticleTexture(true);
            }
        }

        private static Texture2D CreateReticleTexture(bool lockOn)
        {
            var tex = new Texture2D(CursorSize, CursorSize, TextureFormat.RGBA32, false)
            {
                name = lockOn ? "CombatCursor_LockOn" : "CombatCursor_Normal",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            var pixels = new Color32[CursorSize * CursorSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(0, 0, 0, 0);

            Color32 outline = new(10, 15, 20, 240);
            Color32 fill = lockOn ? new Color32(255, 55, 75, 255) : new Color32(65, 235, 215, 255);
            Color32 highlight = lockOn ? new Color32(255, 190, 195, 255) : new Color32(230, 255, 255, 255);

            void SetPixel(int x, int y, Color32 color)
            {
                if (x >= 0 && x < CursorSize && y >= 0 && y < CursorSize)
                {
                    pixels[y * CursorSize + x] = color;
                }
            }

            void DrawBox(int x, int y, int w, int h, Color32 color)
            {
                for (int py = y; py < y + h; py++)
                for (int px = x; px < x + w; px++)
                    SetPixel(px, py, color);
            }

            // Ponto central nítido com contorno (hotspot central 15,15 a 16,16)
            DrawBox(14, 14, 4, 4, outline);
            DrawBox(15, 15, 2, 2, highlight);

            // Traços de mira cardeais: Cima, Baixo, Esquerda, Direita
            // Vertical Superior
            DrawBox(14, 21, 4, 8, outline);
            DrawBox(15, 22, 2, 6, fill);
            SetPixel(15, 27, highlight); SetPixel(16, 27, highlight);

            // Vertical Inferior
            DrawBox(14, 3, 4, 8, outline);
            DrawBox(15, 4, 2, 6, fill);
            SetPixel(15, 4, highlight); SetPixel(16, 4, highlight);

            // Horizontal Esquerda
            DrawBox(3, 14, 8, 4, outline);
            DrawBox(4, 15, 6, 2, fill);
            SetPixel(4, 15, highlight); SetPixel(4, 16, highlight);

            // Horizontal Direita
            DrawBox(21, 14, 8, 4, outline);
            DrawBox(22, 15, 6, 2, fill);
            SetPixel(27, 15, highlight); SetPixel(27, 16, highlight);

            if (lockOn)
            {
                // Colchetes angulares de mira travada (Lock-on brackets nos quatro cantos)
                // Canto Superior Esquerdo
                DrawBox(7, 21, 5, 4, outline); DrawBox(8, 22, 4, 2, fill);
                DrawBox(7, 21, 4, 5, outline); DrawBox(8, 22, 2, 4, fill);
                SetPixel(8, 25, highlight);

                // Canto Superior Direito
                DrawBox(20, 21, 5, 4, outline); DrawBox(20, 22, 4, 2, fill);
                DrawBox(21, 21, 4, 5, outline); DrawBox(22, 22, 2, 4, fill);
                SetPixel(23, 25, highlight);

                // Canto Inferior Esquerdo
                DrawBox(7, 7, 5, 4, outline); DrawBox(8, 8, 4, 2, fill);
                DrawBox(7, 6, 4, 5, outline); DrawBox(8, 6, 2, 4, fill);
                SetPixel(8, 6, highlight);

                // Canto Inferior Direito
                DrawBox(20, 7, 5, 4, outline); DrawBox(20, 8, 4, 2, fill);
                DrawBox(21, 6, 4, 5, outline); DrawBox(22, 6, 2, 4, fill);
                SetPixel(23, 6, highlight);
            }
            else
            {
                // Cantos de anel circular sutil no modo neutro
                DrawBox(8, 22, 3, 3, outline); SetPixel(9, 23, fill);
                DrawBox(21, 22, 3, 3, outline); SetPixel(22, 23, fill);
                DrawBox(8, 7, 3, 3, outline); SetPixel(9, 8, fill);
                DrawBox(21, 7, 3, 3, outline); SetPixel(22, 8, fill);
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }
    }
}
