using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Managers;
using Game.UI;
using UnityEngine.InputSystem;

namespace Game.Varginha
{
    /// <summary>
    /// HUD de investigação estilo RPG / Pokémon de terror sobrenatural (O Segredo de Varginha).
    /// Inclui: Guia Rodrigo, Medidor de Saúde, Inventário, Diálogos e Telas de Fim de Fase.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class VarginhaGameHUD : MonoBehaviour
    {
        public static VarginhaGameHUD Instance { get; private set; }
        private VarginhaBackpackInventory _backpack;
        private int _blockInputThroughFrame = -1;
        public bool IsInventoryOpen => _backpack != null && _backpack.IsOpen;
        public bool BlocksGameplayInput => _isGameOver || _isPaused || VarginhaMainMenu.IsOpen || IsInventoryOpen || Time.frameCount <= _blockInputThroughFrame;

        public bool OpenBackpack()
        {
            if (_edelzio == null) _edelzio = Object.FindAnyObjectByType<EdelzioTopDownController>();
            if (_backpack == null) _backpack = GetComponent<VarginhaBackpackInventory>() ?? gameObject.AddComponent<VarginhaBackpackInventory>();
            return _backpack.Open(_edelzio);
        }

        public void CloseBackpack()
        {
            if (!IsInventoryOpen) return;
            _backpack.Close();
            _blockInputThroughFrame = Time.frameCount + 1;
        }

        public Rect HotbarSlotRect(int index)
        {
            float size = HotbarSlotSize;
            float width = size * 6f + 30f;
            return new Rect(Mathf.Round((Screen.width - width) * .5f) + index * (size + 6f),
                Mathf.Max(8, Mathf.Round(Screen.height - HotbarHeight - 8)) + 22, size, size);
        }

        private EdelzioTopDownController _edelzio;
        private string _activeSpeaker;
        private string _activeDialogue;
        private readonly TypewriterText _dialogueTypewriter = new TypewriterText();
        private readonly TypewriterText _rodrigoTypewriter = new TypewriterText();
        private readonly TypewriterText _interactionTypewriter = new TypewriterText();
        private string _lastInteractionInstruction;
        private bool _isDialogueOpen;
        public bool IsDialogueOpen => _isDialogueOpen;

        private string _rodrigoHint = "Rodrigo: 'Edelzio, explore sua casa. Encontre a mochila, a chave do Fusca e seu antigo caderno antes de sair.'";
        private string _victoryTitle;
        private string _victoryDescription;
        private bool _isVictoryOpen;
        public bool IsVictoryOpen => _isVictoryOpen;
        private bool _isGameOver;
        private string _gameOverReason;
        private float _gameOverOpenedAt;
        public bool IsGameplayVisible => !VarginhaMainMenu.IsOpen && _edelzio != null
            && _edelzio.gameObject.scene == SceneManager.GetActiveScene();
        public bool IsGameOver => _isGameOver;
        private bool _isPaused;
        public bool IsPaused => _isPaused;
        private bool _isControlsOpen;
        private GUIStyle _pauseTextStyle;
        private Vector2 _controlsScroll;
        private static readonly string[] PauseLabels = { "CONTINUAR", "CONTROLES", "SAIR AO MENU" };
        private static readonly VarginhaInputAction[] ControlActions = (VarginhaInputAction[])System.Enum.GetValues(typeof(VarginhaInputAction));

        private Texture2D _whiteTex;
        private GUIStyle _dialogueBoxStyle;
        private GUIStyle _speakerStyle;
        private GUIStyle _dialogueTextStyle;
        private GUIStyle _rodrigoStyle;
        private GUIStyle _promptStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _hotbarLabelStyle;
        private GUIStyle _hotbarNumberStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _healthStyle;
        private static readonly Color PanelColor = new(.042f, .056f, .068f, .97f);
        private static readonly Color PanelBorder = new(.28f, .40f, .44f, .98f);
        private static readonly Color PaperColor = new(.92f, .91f, .82f);
        private static readonly Color FocusColor = new(.96f, .78f, .34f);
        private readonly Sprite[] _hotbarIcons = new Sprite[6];
        private readonly bool[] _hotbarOwned = new bool[6];
        private readonly float[] _hotbarCollectedAt = new float[6];
        private static readonly string[] HotbarNames = { "MOCHILA", "CHAVE DO FUSCA", "CADERNO DE 1996", "NOTEBOOK", "DOCUMENTO DE 1898", "LANTERNA" };
        private int _selectedSlot;
        private float HotbarSlotSize => Mathf.Clamp((Screen.width - 44f) / 6f, 28f, 72f);
        private float HotbarHeight => HotbarSlotSize + (Screen.height < 420f ? 38f : 46f);

        public void TogglePause()
        {
            if (!IsGameplayVisible || _isVictoryOpen || _isGameOver || VarginhaTravelCinematic.IsTravelling) return;
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;
            if (_isPaused)
            {
                _isControlsOpen = false;
                _controlsScroll = Vector2.zero;
                CloseBackpack();
            }
        }

        public void ResumePause()
        {
            if (!_isPaused) return;
            _isPaused = false;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (_edelzio == null) _edelzio = Object.FindAnyObjectByType<EdelzioTopDownController>();
            if (!IsGameplayVisible || VarginhaTravelCinematic.IsTravelling) return;
            var keyboard = Keyboard.current;

            // Pause toggle — ESC or P, always available unless victory/game-over
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
            {
                if (!_isVictoryOpen && !_isGameOver)
                {
                    if (_isControlsOpen) { _isControlsOpen = false; return; }
                    TogglePause();
                    return;
                }
            }

            if (_isPaused) return; // block all gameplay input while paused

            if (_edelzio == null) return;
            if (IsInventoryOpen) return;
            for (int i = 0; i < 6; i++)
            {
                bool owned = (i == 5)
                    ? (_edelzio.HasFlashlight && _edelzio.IsFlashlightEquippedInHotbar)
                    : _edelzio.HasInventoryItem(i);
                if (owned && !_hotbarOwned[i])
                {
                    _hotbarCollectedAt[i] = Time.unscaledTime;
                    _selectedSlot = i;
                }
                _hotbarOwned[i] = owned;
            }
            if (_isDialogueOpen || _isVictoryOpen || _edelzio.IsInputLocked || Time.timeScale == 0f) return;
            var mouse = Mouse.current;
            if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame))
            {
                Vector2 point = mouse.position.ReadValue(); point.y = Screen.height - point.y;
                for (int slot = 0; slot < 6; slot++)
                {
                    if (!HotbarSlotRect(slot).Contains(point)) continue;
                    _selectedSlot = slot;
                    if (slot == 0 && mouse.leftButton.wasPressedThisFrame) OpenBackpack();
                    if (slot == 5 && mouse.leftButton.wasPressedThisFrame && _edelzio.HasFlashlight && _edelzio.IsFlashlightEquippedInHotbar)
                    {
                        _edelzio.ToggleFlashlight();
                    }
                    _blockInputThroughFrame = Time.frameCount + 1;
                    return;
                }
            }
            if (keyboard == null) return;
            if (keyboard.gKey.wasPressedThisFrame) { _selectedSlot = 0; OpenBackpack(); }
            if (keyboard.digit1Key.wasPressedThisFrame) _selectedSlot = 0;
            if (keyboard.digit2Key.wasPressedThisFrame) _selectedSlot = 1;
            if (keyboard.digit3Key.wasPressedThisFrame) _selectedSlot = 2;
            if (keyboard.digit4Key.wasPressedThisFrame) _selectedSlot = 3;
            if (keyboard.digit5Key.wasPressedThisFrame) _selectedSlot = 4;
            if (keyboard.digit6Key.wasPressedThisFrame) _selectedSlot = 5;

            // Se o slot 5 (Lanterna) estiver selecionado na hotbar, ESPAÇO, ENTER ou V liga/desliga a lanterna
            if (_selectedSlot == 5 && _edelzio.HasFlashlight && _edelzio.IsFlashlightEquippedInHotbar)
            {
                if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.vKey.wasPressedThisFrame)
                {
                    _edelzio.ToggleFlashlight();
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { enabled = false; return; }
            Instance = this;
            _rodrigoTypewriter.Set(_rodrigoHint);
        }

        private void OnEnable()
        {
            // Com Enter Play Mode Options, Awake pode não rodar entre tentativas.
            // Reafirma a instância que desenha as interfaces de interação.
            if (Instance != null && Instance != this) { enabled = false; return; }
            Instance = this;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            CloseBackpack();
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (Instance == this) Instance = null;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CloseBackpack();
            // Este HUD vive junto aos managers; uma nova fase precisa sempre iniciar limpa.
            _isVictoryOpen = false;
            _isDialogueOpen = false;
            _isGameOver = false;
            _isPaused = false;
            _isControlsOpen = false;
            Time.timeScale = 1f;
            _lastInteractionInstruction = null;
            _edelzio = Object.FindAnyObjectByType<EdelzioTopDownController>();
            System.Array.Clear(_hotbarOwned, 0, _hotbarOwned.Length);
            System.Array.Clear(_hotbarCollectedAt, 0, _hotbarCollectedAt.Length);
            _selectedSlot = 0;
        }

        private void Start()
        {
            _edelzio = Object.FindAnyObjectByType<EdelzioTopDownController>();
            if (IsGameplayVisible && GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                GameManager.Instance.StartGame();
            }
        }

        private void InitStyles()
        {
            if (_dialogueBoxStyle != null) return;

            _whiteTex = new Texture2D(1, 1) { filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave };
            _whiteTex.SetPixel(0, 0, Color.white);
            _whiteTex.Apply();

            _dialogueBoxStyle = new GUIStyle(GUI.skin.box);

            _speakerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _speakerStyle.normal.textColor = new Color(.64f, .84f, .86f);

            _dialogueTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            _dialogueTextStyle.normal.textColor = PaperColor;

            _rodrigoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(15, 15, 10, 10)
            };
            _rodrigoStyle.normal.textColor = PaperColor;

            _promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            _promptStyle.normal.textColor = PaperColor;
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            PixelUIFont.Apply(_dialogueBoxStyle);
            PixelUIFont.Apply(_speakerStyle);
            PixelUIFont.Apply(_dialogueTextStyle);
            PixelUIFont.Apply(_rodrigoStyle);
            PixelUIFont.Apply(_promptStyle);
            PixelUIFont.Apply(_buttonStyle);
            _hotbarLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11, fontStyle = FontStyle.Bold };
            _hotbarLabelStyle.normal.textColor = PaperColor;
            _hotbarNumberStyle = new GUIStyle(_hotbarLabelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 10, padding = new RectOffset(0, 0, 0, 0) };
            PixelUIFont.Apply(_hotbarLabelStyle);
            PixelUIFont.Apply(_hotbarNumberStyle);
            _statusStyle = new GUIStyle(_hotbarLabelStyle) { fontSize = 11 };
            _healthStyle = new GUIStyle(_hotbarLabelStyle) { fontSize = 12 };
            _hotbarIcons[0] = VarginhaPixelArtSprites.Create("Backpack_Inventory", Color.gray);
            _hotbarIcons[1] = VarginhaPixelArtSprites.Create("Inventory_Key", Color.white);
            _hotbarIcons[2] = VarginhaPixelArtSprites.Create("Inventory_Journal", Color.white);
            _hotbarIcons[3] = VarginhaPixelArtSprites.Create("Notebook_Inventory", new Color(.30f, .90f, 1f));
            _hotbarIcons[4] = VarginhaPixelArtSprites.Create("Doc_Inventory", new Color(.9f, .85f, .7f));
            _hotbarIcons[5] = VarginhaPixelArtSprites.Create("Inventory_Flashlight", Color.white);
        }

        private Sprite EnsureHotbarIcon(int index)
        {
            var icon = _hotbarIcons[index];
            if (icon != null && icon.texture != null) return icon;

            switch (index)
            {
                case 0: icon = VarginhaPixelArtSprites.Create("Backpack_Inventory", Color.gray); break;
                case 1: icon = VarginhaPixelArtSprites.Create("Inventory_Key", Color.white); break;
                case 2: icon = VarginhaPixelArtSprites.Create("Inventory_Journal", Color.white); break;
                case 3: icon = VarginhaPixelArtSprites.Create("Notebook_Inventory", new Color(.30f, .90f, 1f)); break;
                case 4: icon = VarginhaPixelArtSprites.Create("Doc_Inventory", new Color(.9f, .85f, .7f)); break;
                case 5: icon = VarginhaPixelArtSprites.Create("Inventory_Flashlight", Color.white); break;
                default: icon = VarginhaPixelArtSprites.Create("Inventory_Flashlight", Color.white); break;
            }

            _hotbarIcons[index] = icon;
            return icon;
        }

        public void ShowDialogue(string speaker, string message)
        {
            CloseBackpack();
            _activeSpeaker = speaker;
            _activeDialogue = message;
            _dialogueTypewriter.Set(message);
            _isDialogueOpen = true;
        }

        public void CloseDialogue()
        {
            _isDialogueOpen = false;
        }

        public void ShowRodrigoHint(string hint)
        {
            _rodrigoHint = hint;
            _rodrigoTypewriter.Set(hint);
        }

        public void ShowVictory(string title, string description)
        {
            CloseBackpack();
            _isDialogueOpen = false;
            _victoryTitle = title;
            _victoryDescription = description;
            _isVictoryOpen = true;
        }

        /// <summary>Death overlay remains until the player chooses where to continue.</summary>
        public void ShowGameOver(string reason = "EDELZIO FOI DERROTADO")
        {
            CloseBackpack();
            _isDialogueOpen = false;
            _isVictoryOpen = false;
            _isPaused = _isControlsOpen = false;
            _gameOverReason = reason;
            _gameOverOpenedAt = Time.unscaledTime;
            _isGameOver = true;
        }

        private void OnGUI()
        {
            if (!IsGameplayVisible) return;
            if (IsInventoryOpen) return;
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            InitStyles();

            _dialogueTypewriter.Tick(38f);
            _rodrigoTypewriter.Tick(28f);
            _interactionTypewriter.Tick(42f);

            if ((_isDialogueOpen && _dialogueTypewriter.HasAdvanced) || _rodrigoTypewriter.HasAdvanced)
            {
                PlayTypewriterBlip();
            }

            if (_isDialogueOpen && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                if (_dialogueTypewriter.IsComplete) CloseDialogue();
                else _dialogueTypewriter.RevealImmediately();
                Event.current.Use();
            }

            if (_edelzio == null)
            {
                _edelzio = Object.FindAnyObjectByType<EdelzioTopDownController>();
            }

            if (_isGameOver || GameManager.Instance?.IsGameOver == true)
            {
                DrawGameOverWindow();
                return;
            }
            DrawTopBar();

            // Pause screen overrides all other overlays except the top bar
            if (_isPaused)
            {
                DrawPauseScreen();
                return;
            }

            DrawInventoryBar();
            DrawCombatHint();
            DrawInteractionPrompt();

            if (_isDialogueOpen)
            {
                DrawDialogueWindow();
            }

            if (_isVictoryOpen)
            {
                DrawVictoryWindow();
            }
            else if (_isGameOver || GameManager.Instance?.IsGameOver == true || (_edelzio != null && _edelzio.CurrentSanity <= 0f))
            {
                DrawGameOverWindow();
            }
        }

        private void DrawTopBar()
        {
            // Saúde do jogador (Canto Superior Esquerdo)
            float sanity = _edelzio != null ? _edelzio.CurrentSanity : 100f;
            float maxSanity = _edelzio != null ? _edelzio.MaxSanity : 100f;
            float ratio = Mathf.Clamp01(sanity / maxSanity);

            float panelWidth = Mathf.Min(340f, Mathf.Max(220f, Screen.width - 36f));
            float panelHeight = Screen.height < 420f ? 84f : 100f;
            Rect stabilityPanel = new Rect(18, 16, panelWidth, panelHeight);
            PixelHUDFrame.Draw(stabilityPanel, _whiteTex, PanelColor, PanelBorder);
            GUI.Label(new Rect(stabilityPanel.x, stabilityPanel.y + 6, stabilityPanel.width, 20), "SAUDE", _promptStyle);

            float heartSize = Mathf.Clamp((stabilityPanel.width - 44f) / 3f, 28f, panelHeight - 52f);
            float heartGap = Mathf.Clamp(6f * heartSize / 52f, 3f, 6f);
            float heartX = stabilityPanel.x + (stabilityPanel.width - heartSize * 3f - heartGap * 2f) * .5f;
            for (int heart = 0; heart < 3; heart++)
            {
                PixelHUDFrame.DrawHealthHeart(new Rect(heartX + heart * (heartSize + heartGap), stabilityPanel.y + 28f, heartSize, heartSize), Mathf.Clamp01(ratio * 3f - heart));
            }

            _healthStyle.normal.textColor = ratio <= 1f / 3f ? new Color(1f, .61f, .60f) : PaperColor;
            GUI.Label(new Rect(stabilityPanel.x + 8f, stabilityPanel.yMax - 28f, stabilityPanel.width - 16f, 18), $"{Mathf.CeilToInt(ratio * 100f)}%", _healthStyle);
            Rect healthTrack = new Rect(stabilityPanel.x + 12f, stabilityPanel.yMax - 9f, stabilityPanel.width - 24f, 3f);
            DrawHudBlock(healthTrack, new Color(.025f, .03f, .04f));
            // Barra de saúde com cor suave que pulsa quando crítica
            Color barColor = ratio <= 1f / 3f
                ? Color.Lerp(new Color(.86f, .31f, .34f), new Color(1f, .55f, .58f), 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f))
                : new Color(.57f, .75f, .69f);
            DrawHudBlock(new Rect(healthTrack.x, healthTrack.y, Mathf.Round(healthTrack.width * ratio), healthTrack.height), barColor);
            for (int segment = 1; segment < 3; segment++)
                DrawHudBlock(new Rect(healthTrack.x + Mathf.Round(healthTrack.width * segment / 3f), healthTrack.y, 2f, healthTrack.height), PanelColor);

            // Guia Rodrigo (Topo) - Responsivo e sempre visível em qualquer resolução
            float leftSpace = stabilityPanel.xMax + 14f;
            float rw = Mathf.Clamp(Screen.width - leftSpace - 24f, 220f, 680f);
            if (Screen.width > 520f)
            {
                float rx = Screen.width > 920f ? (Screen.width - rw) * .5f : leftSpace;
                if (rx < leftSpace) rx = leftSpace;
                Rect guidePanel = new Rect(rx, 16, rw, panelHeight);
                PixelHUDFrame.Draw(guidePanel, _whiteTex, PanelColor, PanelBorder);
                Rect guideHeader = new Rect(guidePanel.x + 10f, guidePanel.y + 5f, guidePanel.width - 20f, 16f);
                float recBlink = Mathf.PingPong(Time.unscaledTime * 2.2f, 1f);
                string recDot = recBlink > 0.4f ? "●" : "○";
                GUI.Label(guideHeader, $"{recDot} TRANSMISSAO • SINAL 96.4 MHz • RODRIGO", _hotbarNumberStyle);
                Rect guideContent = new Rect(guidePanel.x + 10f, guidePanel.y + 20f, guidePanel.width - 20f, guidePanel.height - 26f);
                GUI.Label(guideContent, _rodrigoTypewriter.VisibleText, _rodrigoStyle);
                GUI.color = Color.white;
            }
        }

        private void DrawInventoryBar()
        {
            if (_edelzio == null || _isVictoryOpen || _edelzio.CurrentSanity <= 0 || Time.timeScale == 0f) return;
            float slotSize = HotbarSlotSize;
            const float gap = 6f;
            float width = slotSize * 6f + gap * 5f;
            float x = Mathf.Round((Screen.width - width) * .5f);
            float y = Mathf.Round(Screen.height - HotbarHeight - 8f);
            y = Mathf.Max(8f, y);
            _hotbarLabelStyle.fontSize = Screen.width < 480 ? 8 : 11;
            DrawHudBlock(new Rect(x, y + 1f, width, 18f), new Color(.035f, .045f, .055f, .88f));
            GUI.Label(new Rect(x, y, width, 20f), "ITENS COLETADOS", _hotbarLabelStyle);
            int hovered = -1;
            for (int i = 0; i < 6; i++)
            {
                Rect slot = new Rect(x + i * (slotSize + gap), y + 22f, slotSize, slotSize);
                bool owned = (i == 5)
                    ? (_edelzio.HasFlashlight && _edelzio.IsFlashlightEquippedInHotbar)
                    : _edelzio.HasInventoryItem(i);
                bool selected = i == _selectedSlot;
                bool isHovered = slot.Contains(Event.current.mousePosition);
                bool newlyCollected = owned && Time.unscaledTime - _hotbarCollectedAt[i] < 1.4f;
                bool isFlashlightOn = i == 5 && owned && _edelzio.FlashlightActive;
                Color border = isFlashlightOn ? new Color(.98f, .85f, .30f)
                    : selected ? FocusColor : isHovered ? new Color(.65f, .74f, .76f)
                    : owned ? PanelBorder : new Color(.23f, .28f, .32f);
                if (newlyCollected) border = Color.Lerp(border, Color.white, .3f + .25f * Mathf.Sin(Time.unscaledTime * 8f));
                PixelHUDFrame.Draw(slot, _whiteTex, selected ? new Color(.15f, .17f, .18f, .99f) : PanelColor, border);
                if (owned)
                {
                    var sprite = EnsureHotbarIcon(i);
                    if (sprite != null && sprite.texture != null)
                    {
                        var texture = sprite.texture;
                        var source = sprite.rect;
                        GUI.DrawTextureWithTexCoords(new Rect(slot.x + 10f, slot.y + 10f, slotSize - 20f, slotSize - 20f), texture,
                            new Rect(source.x / texture.width, source.y / texture.height, source.width / texture.width, source.height / texture.height));
                    }
                }
                else
                {
                    float centerX = Mathf.Round(slot.center.x), centerY = Mathf.Round(slot.center.y);
                    DrawHudBlock(new Rect(centerX - 5, centerY - 1, 10, 2), new Color(.24f, .28f, .32f));
                }
                Rect numberBadge = new Rect(slot.x + 4, slot.y + 4, 15, 15);
                DrawHudBlock(numberBadge, new Color(.025f, .035f, .045f, .95f));
                _hotbarNumberStyle.normal.textColor = selected ? FocusColor : new Color(.68f, .73f, .75f);
                GUI.Label(numberBadge, (i + 1).ToString(), _hotbarNumberStyle);
                if (selected)
                    DrawHudBlock(new Rect(slot.x + 8, slot.yMax - 6, slot.width - 16, 2), isFlashlightOn ? new Color(.98f, .85f, .30f) : FocusColor);
                if (owned)
                {
                    Color indicatorColor = isFlashlightOn ? new Color(.98f, .88f, .25f) : new Color(.58f, .79f, .70f);
                    DrawHudBlock(new Rect(slot.xMax - 9, slot.yMax - 10, isFlashlightOn ? 5 : 3, isFlashlightOn ? 5 : 3), indicatorColor);
                }
                if (isHovered) hovered = i;
                // Input is handled in Update before combat, preventing click-through attacks.
            }
            int describedSlot = hovered >= 0 ? hovered : _selectedSlot;
            string label;
            if (describedSlot == 0 && _edelzio.HasBackpack)
                label = "MOCHILA • CLIQUE OU [G]";
            else if (describedSlot == 5)
            {
                if (_edelzio.HasFlashlight && _edelzio.IsFlashlightEquippedInHotbar)
                    label = _edelzio.FlashlightActive
                        ? "LANTERNA • LIGADA [CLIQUE/ESPAÇO/V]"
                        : "LANTERNA • DESLIGADA [CLIQUE/ESPAÇO/V]";
                else
                    label = "ESPAÇO VAZIO • EQUIPE NA MOCHILA [G]";
            }
            else
            {
                label = _edelzio.HasInventoryItem(describedSlot) ? HotbarNames[describedSlot] : "ESPAÇO VAZIO";
            }
            Rect description = new Rect(x, y + slotSize + (Screen.height < 420f ? 14f : 24f), width, 20f);
            DrawHudBlock(description, new Color(.035f, .045f, .055f, .92f));
            GUI.Label(description, label, _hotbarLabelStyle);
        }

        private void DrawInteractionPrompt()
        {
            if (_edelzio == null || _edelzio.NearestInteractable == null || _isDialogueOpen || _isVictoryOpen) return;

            var prop = _edelzio.NearestInteractable;
            string instruction = $"[E] EXAMINAR\n{prop.PropName.ToUpperInvariant()}";
            float maxWidth = Mathf.Min(620f, Screen.width - 32f);
            float desiredWidth = _promptStyle.CalcSize(new GUIContent(instruction)).x + 44f;
            float w = Mathf.Clamp(desiredWidth, 260f, maxWidth);
            float h = Mathf.Max(52f, _promptStyle.CalcHeight(new GUIContent(instruction), w - 28f) + 22f);
            float x = (Screen.width - w) / 2f;
            float y = Screen.height - HotbarHeight - h - 22f;
            if (_lastInteractionInstruction != instruction)
            {
                _lastInteractionInstruction = instruction;
                _interactionTypewriter.Set(instruction);
            }

            Rect promptPanel = new Rect(x, y, w, h);
            PixelHUDFrame.Draw(promptPanel, _whiteTex, PanelColor, new Color(.48f, .69f, .71f));
            GUI.Label(new Rect(promptPanel.x + 12f, promptPanel.y + 7f, promptPanel.width - 24f, promptPanel.height - 14f), _interactionTypewriter.VisibleText, _promptStyle);
            GUI.color = Color.white;
        }

        private void DrawCombatHint()
        {
            // Bug fix: cache GetComponent result instead of calling it twice per frame
            var attack = _edelzio?.GetComponent<VarginhaPlayerAttack>();
            if (_edelzio == null || attack == null || _isDialogueOpen || _isVictoryOpen || _edelzio.CurrentSanity <= 0f) return;
            float hintWidth = Mathf.Min(360f, Screen.width - 36f);
            float hintHeight = Screen.height < 420f ? 44f : 50f;
            float hotbarTop = Screen.height - HotbarHeight - 8f;
            float hintY = hotbarTop - hintHeight - 12f;
            if (hintY < 122f && Screen.width > 560f)
                hintY = 122f;
            else if (hintY < 8f)
                hintY = 8f;
            Rect hint = new Rect(18f, hintY, hintWidth, hintHeight);
            bool hasTarget = VarginhaCombatCursor.Instance != null && VarginhaCombatCursor.Instance.HasTarget;
            // Pulsação sutil na borda quando há alvo
            float targetPulse = hasTarget ? 0.72f + 0.28f * Mathf.Sin(Time.unscaledTime * 5.5f) : 1f;
            Color hintBorder = hasTarget
                ? new Color(.92f * targetPulse, .35f, .38f)
                : new Color(.59f, .49f, .34f);
            PixelHUDFrame.Draw(hint, _whiteTex, PanelColor, hintBorder);
            string combo = attack.ComboStep > 0 ? $"COMBO {attack.ComboStep}/3" : "COMBO 3 GOLPES";
            if (hasTarget) combo += " \u2022 ALVO TRAVADO";
            string attackBinding = VarginhaInputBindings.DisplayName(VarginhaInputAction.Attack);
            string attackLine = $"{attackBinding}  {combo}";
            _statusStyle.fontSize = Screen.width < 480 ? 9 : 11;
            _statusStyle.wordWrap = false;
            _statusStyle.clipping = TextClipping.Clip;
            float lineWidth = hint.width - 16f;
            while (_statusStyle.fontSize > 8 && _statusStyle.CalcSize(new GUIContent(attackLine)).x > lineWidth)
                _statusStyle.fontSize--;
            GUI.Label(new Rect(hint.x + 8f, hint.y + 3f, lineWidth, 18f), attackLine, _statusStyle);
            string dodge = _edelzio.IsDodging ? "ESQUIVANDO" : _edelzio.DodgeCooldownRemaining > 0f
                ? $"ESQUIVA {_edelzio.DodgeCooldownRemaining:0.0}s" : "ESQUIVA PRONTA";
            string dodgeBinding = VarginhaInputBindings.DisplayName(VarginhaInputAction.Dodge);
            string dodgeLine = $"{dodgeBinding}  {dodge}";
            while (_statusStyle.fontSize > 8 && _statusStyle.CalcSize(new GUIContent(dodgeLine)).x > lineWidth)
                _statusStyle.fontSize--;
            GUI.Label(new Rect(hint.x + 8f, hint.y + 22f, lineWidth, 18f), dodgeLine, _statusStyle);
        }

        private void DrawDialogueWindow()
        {
            Rect modal = GetBottomDialogRect(760f, Mathf.Min(300f, Screen.height * .48f));
            float pad = Mathf.Clamp(modal.width * .055f, 12f, 34f);
            float buttonHeight = Mathf.Clamp(modal.height * .18f, 30f, 46f);
            float speakerHeight = Mathf.Clamp(modal.height * .18f, 24f, 40f);
            _speakerStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 42f), 9, 17);
            _dialogueTextStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 55f), 8, 15);
            _buttonStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 14);

            PixelHUDFrame.Draw(modal, _whiteTex, PanelColor, PanelBorder);
            GUI.color = Color.white;

            GUI.Label(new Rect(modal.x + pad, modal.y + pad * .35f, modal.width - pad * 2f, speakerHeight), $"> {_activeSpeaker}", _speakerStyle);
            GUI.Label(new Rect(modal.x + pad, modal.y + speakerHeight + pad, modal.width - pad * 2f, modal.height - speakerHeight - buttonHeight - pad * 2.5f), _dialogueTypewriter.VisibleText, _dialogueTextStyle);

            float buttonWidth = Mathf.Min(modal.width - pad * 2f, 250f);
            if (GUI.Button(new Rect(modal.x + (modal.width - buttonWidth) * .5f, modal.yMax - buttonHeight - pad * .55f, buttonWidth, buttonHeight), "CONTINUAR", _buttonStyle))
            {
                if (_dialogueTypewriter.IsComplete) CloseDialogue();
                else _dialogueTypewriter.RevealImmediately();
            }

            if (_dialogueTypewriter.IsComplete)
            {
                float blink = Mathf.PingPong(Time.unscaledTime * 3.5f, 1f);
                if (blink > 0.25f)
                {
                    GUI.Label(new Rect(modal.xMax - pad - 120f, modal.yMax - buttonHeight - pad * .45f, 110f, 20f), "[ESPAÇO] ▼", _hotbarNumberStyle);
                }
            }
        }

        private static AudioClip _typewriterClip;
        private static AudioSource _typewriterSource;

        private static void PlayTypewriterBlip()
        {
            if (_typewriterClip == null)
            {
                int sampleRate = 22050;
                float duration = 0.03f;
                int count = Mathf.RoundToInt(sampleRate * duration);
                float[] samples = new float[count];
                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / sampleRate;
                    float env = 1f - (float)i / count;
                    samples[i] = Mathf.Sin(2f * Mathf.PI * 520f * t) * env * 0.16f;
                }
                _typewriterClip = AudioClip.Create("TypewriterBlip", count, 1, sampleRate, false);
                _typewriterClip.SetData(samples, 0);
            }

            if (_typewriterSource == null)
            {
                var go = new GameObject("TypewriterAudioSource");
                Object.DontDestroyOnLoad(go);
                _typewriterSource = go.AddComponent<AudioSource>();
                _typewriterSource.playOnAwake = false;
            }

            _typewriterSource.pitch = UnityEngine.Random.Range(0.92f, 1.08f);
            _typewriterSource.PlayOneShot(_typewriterClip, 0.18f);
        }

        // The pause and radio dialogue share the same frame, grain and paper palette.
        private void PauseLabel(Rect rect, string text, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            if (_pauseTextStyle == null)
            {
                _pauseTextStyle = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    wordWrap = false,
                    clipping = TextClipping.Clip,
                    fontStyle = FontStyle.Normal
                };
                PixelUIFont.Apply(_pauseTextStyle);
            }
            _pauseTextStyle.alignment = alignment;
            _pauseTextStyle.normal.textColor = color;
            _pauseTextStyle.fontSize = Mathf.Max(4, fontSize);
            var content = new GUIContent(text);
            // Measure the actual pixel font; inherited button padding used to crop labels.
            while (_pauseTextStyle.fontSize > 4 &&
                (_pauseTextStyle.CalcSize(content).x > rect.width || _pauseTextStyle.CalcSize(content).y > rect.height))
                _pauseTextStyle.fontSize--;
            GUI.Label(rect, content, _pauseTextStyle);
        }

        private void PauseSeparator(Rect modal, float y)
        {
            DrawHudBlock(new Rect(modal.x + 16f, y, modal.width - 32f, 1f), PanelBorder);
        }

        private void PauseHeader(Rect modal, string title, string subtitle, float scale)
        {
            float pad = 22f;
            Color muted = new Color(.64f, .73f, .75f);
            // A small steady radio indicator avoids an oversized neon title.
            DrawHudBlock(new Rect(modal.x + pad, modal.y + 20f * scale, 4f, 4f), muted);
            PauseLabel(new Rect(modal.x + pad + 12f, modal.y + 12f * scale, modal.width - pad * 2f - 12f, 20f * scale),
                "TRANSMISSAO • 96.4 MHz • SISTEMA", Mathf.RoundToInt(9f * scale), muted);
            PauseSeparator(modal, modal.y + 40f * scale);
            PauseLabel(new Rect(modal.x + pad, modal.y + 53f * scale, modal.width - pad * 2f, 30f * scale),
                title, Mathf.RoundToInt(20f * scale), PaperColor);
            PauseLabel(new Rect(modal.x + pad, modal.y + 89f * scale, modal.width - pad * 2f, 18f * scale),
                subtitle, Mathf.RoundToInt(9f * scale), muted);
        }

        private bool PauseButton(Rect rect, string label, string marker, float scale)
        {
            bool hover = rect.Contains(Event.current.mousePosition);
            Color accent = new Color(.64f, .84f, .86f);
            PixelHUDFrame.Draw(rect, _whiteTex,
                hover ? new Color(.10f, .15f, .17f, .98f) : PanelColor,
                hover ? accent : PanelBorder);
            if (hover) DrawHudBlock(new Rect(rect.x + 3f, rect.y + 5f, 2f, rect.height - 10f), accent);
            PauseLabel(new Rect(rect.x + 14f, rect.y, 26f, rect.height), hover ? ">" : marker,
                Mathf.RoundToInt(9f * scale), hover ? accent : new Color(.48f, .59f, .61f));
            PauseLabel(new Rect(rect.x + 48f, rect.y, rect.width - 60f, rect.height), label,
                Mathf.RoundToInt(12f * scale), hover ? Color.white : PaperColor);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        private void DrawPauseScreen()
        {
            DrawHudBlock(new Rect(0, 0, Screen.width, Screen.height), new Color(.005f, .01f, .015f, .76f));
            if (_isControlsOpen)
            {
                DrawControlsPanel();
                return;
            }

            Rect modal = GetModalRect(520f, 344f);
            float scale = Mathf.Min(1f, modal.height / 344f);
            PixelHUDFrame.Draw(modal, _whiteTex, PanelColor, PanelBorder);
            PauseHeader(modal, "PAUSADO", "INVESTIGACAO EM ESPERA", scale);

            float buttonY = modal.y + 124f * scale;
            for (int i = 0; i < PauseLabels.Length; i++)
            {
                Rect button = new Rect(modal.x + 22f, buttonY + i * 54f * scale, modal.width - 44f, 44f * scale);
                if (!PauseButton(button, PauseLabels[i], "0" + (i + 1), scale)) continue;
                switch (i)
                {
                    case 0: ResumePause(); break;
                    case 1: _isControlsOpen = true; break;
                    case 2: QuitToMenu(); break;
                }
            }
            PauseSeparator(modal, modal.yMax - 43f * scale);
            PauseLabel(new Rect(modal.x + 22f, modal.yMax - 34f * scale, modal.width - 44f, 20f * scale),
                "[ESC] / [P]  RETOMAR INVESTIGACAO", Mathf.RoundToInt(9f * scale),
                new Color(.64f, .73f, .75f), TextAnchor.MiddleCenter);
        }

        private void DrawControlsPanel()
        {
            Rect modal = GetModalRect(600f, 506f);
            float scale = Mathf.Min(1f, modal.height / 344f);
            PixelHUDFrame.Draw(modal, _whiteTex, PanelColor, PanelBorder);
            PauseHeader(modal, "CONTROLES", "GUIA DO INVESTIGADOR", scale);

            Rect viewport = new Rect(modal.x + 22f, modal.y + 119f * scale,
                modal.width - 44f, Mathf.Max(24f, modal.height - 193f * scale));
            const float rowHeight = 32f;
            float contentHeight = ControlActions.Length * rowHeight;
            float contentWidth = viewport.width - (contentHeight > viewport.height ? 18f : 0f);
            _controlsScroll = GUI.BeginScrollView(viewport, _controlsScroll,
                new Rect(0, 0, contentWidth, contentHeight));
            // Both columns use the same measured size, even with long custom bindings.
            int rowFont = Mathf.Clamp(Mathf.FloorToInt(contentWidth / 48f), 6, 10);
            for (int i = 0; i < ControlActions.Length; i++)
            {
                float y = i * rowHeight;
                if (i % 2 == 0) DrawHudBlock(new Rect(0, y, contentWidth, rowHeight), new Color(.10f, .14f, .16f, .55f));
                PauseLabel(new Rect(8f, y, contentWidth * .55f - 12f, rowHeight),
                    VarginhaInputBindings.ActionName(ControlActions[i]).ToUpperInvariant(), rowFont, PaperColor);
                PauseLabel(new Rect(contentWidth * .55f, y, contentWidth * .45f - 8f, rowHeight),
                    VarginhaInputBindings.DisplayName(ControlActions[i]), rowFont,
                    new Color(.64f, .84f, .86f), TextAnchor.MiddleRight);
            }
            GUI.EndScrollView();
            PauseSeparator(modal, modal.yMax - 66f * scale);
            if (PauseButton(new Rect(modal.x + 22f, modal.yMax - 54f * scale, modal.width - 44f, 38f * scale),
                "VOLTAR", "<", scale)) _isControlsOpen = false;
        }
        private void QuitToMenu()
        {
            ResumePause();
            VarginhaGameOverFlow.ReturnToMenu();
        }

        private void DrawVictoryWindow()
        {
            Rect modal = GetModalRect(800f, Mathf.Min(520f, Screen.height * .88f));
            float pad = Mathf.Clamp(modal.width * .06f, 12f, 42f);
            float buttonHeight = Mathf.Clamp(modal.height * .15f, 30f, 52f);
            _buttonStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 14);

            PixelHUDFrame.Draw(modal, _whiteTex, PanelColor, new Color(.46f, .71f, .57f));
            GUI.color = Color.white;

            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 30f), 10, 22), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle.normal.textColor = new Color(0.3f, 1f, 0.6f);
            PixelUIFont.Apply(titleStyle);
            float titleHeight = titleStyle.CalcHeight(new GUIContent(_victoryTitle), modal.width - pad * 2f);
            GUI.Label(new Rect(modal.x + pad, modal.y + pad, modal.width - pad * 2f, titleHeight), _victoryTitle, titleStyle);

            var descStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 62f), 8, 13), wordWrap = true, alignment = TextAnchor.UpperCenter };
            descStyle.normal.textColor = Color.white;
            PixelUIFont.Apply(descStyle);
            float contentTop = modal.y + pad + titleHeight + pad * .6f;
            float availableHeight = modal.yMax - contentTop - buttonHeight - pad * 1.6f;
            float descriptionHeight = descStyle.CalcHeight(new GUIContent(_victoryDescription), modal.width - pad * 2f);
            float descriptionY = contentTop + Mathf.Max(0f, (availableHeight - descriptionHeight) * .22f);
            GUI.Label(new Rect(modal.x + pad, descriptionY, modal.width - pad * 2f, Mathf.Min(descriptionHeight, availableHeight)), _victoryDescription, descStyle);

            float buttonWidth = Mathf.Min(modal.width - pad * 2f, 300f);
            if (GUI.Button(new Rect(modal.x + (modal.width - buttonWidth) * .5f, modal.yMax - buttonHeight - pad * .55f, buttonWidth, buttonHeight), "JOGAR NOVAMENTE", _buttonStyle))
            {
                RestartScene();
            }
        }

        private void DrawGameOverWindow()
        {
            float fade = Mathf.Clamp01((Time.unscaledTime - _gameOverOpenedAt) / .4f);
            DrawHudBlock(new Rect(0, 0, Screen.width, Screen.height), new Color(.005f, .01f, .015f, .84f * fade));
            Rect modal = GetModalRect(560f, 390f);
            float scale = Mathf.Min(1f, modal.height / 390f);
            PixelHUDFrame.Draw(modal, _whiteTex, PanelColor, new Color(.51f, .32f, .34f));
            PauseHeader(modal, "SINAL PERDIDO", _gameOverReason ?? "EDELZIO FOI DERROTADO", scale);
            PauseLabel(new Rect(modal.x + 22, modal.y + 119f * scale, modal.width - 44, 22f * scale),
                "A investigacao pode continuar. Tente outra vez.", Mathf.RoundToInt(9 * scale), PaperColor);
            bool previousEnabled = GUI.enabled;
            // A held attack click cannot immediately dismiss the death screen.
            GUI.enabled = previousEnabled && Time.unscaledTime - _gameOverOpenedAt >= .65f;
            if (PauseButton(new Rect(modal.x + 22, modal.y + 161f * scale, modal.width - 44, 44f * scale),
                "TENTAR ESTA FASE NOVAMENTE", "01", scale)) VarginhaGameOverFlow.RetryCurrentPhase();
            if (PauseButton(new Rect(modal.x + 22, modal.y + 215f * scale, modal.width - 44, 44f * scale),
                "REINICIAR INVESTIGACAO", "02", scale)) VarginhaGameOverFlow.ReturnToPhaseOne();
            if (PauseButton(new Rect(modal.x + 22, modal.y + 269f * scale, modal.width - 44, 44f * scale),
                "SAIR AO MENU", "03", scale)) VarginhaGameOverFlow.ReturnToMenu();
            GUI.enabled = previousEnabled;
            PauseSeparator(modal, modal.yMax - 43f * scale);
            PauseLabel(new Rect(modal.x + 22, modal.yMax - 34f * scale, modal.width - 44, 20f * scale),
                "A DIFICULDADE ESCOLHIDA SERA MANTIDA", Mathf.RoundToInt(9 * scale), PixelMenuTheme.Muted, TextAnchor.MiddleCenter);
        }

        private void RestartScene()
        {
            Time.timeScale = 1f;
            _isVictoryOpen = false;
            _isDialogueOpen = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void DrawHudBlock(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, _whiteTex);
            GUI.color = Color.white;
        }

        private static Rect GetModalRect(float maxWidth, float maxHeight)
        {
            float horizontalMargin = Mathf.Clamp(Screen.width * .04f, 8f, 36f);
            float verticalMargin = Mathf.Clamp(Screen.height * .05f, 8f, 36f);
            float width = Mathf.Min(maxWidth, Screen.width - horizontalMargin * 2f);
            float height = Mathf.Min(maxHeight, Screen.height - verticalMargin * 2f);
            return new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, width, height);
        }

        private Rect GetBottomDialogRect(float maxWidth, float maxHeight)
        {
            Rect rect = GetModalRect(maxWidth, maxHeight);
            float bottomMargin = Mathf.Clamp(Screen.height * .04f, 8f, 30f);
            rect.height = Mathf.Min(rect.height, Mathf.Max(80f, Screen.height - HotbarHeight - bottomMargin * 2f - 12f));
            rect.y = Screen.height - HotbarHeight - rect.height - bottomMargin - 12f;
            return rect;
        }
    }
}
