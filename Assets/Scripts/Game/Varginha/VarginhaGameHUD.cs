using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Managers;
using Game.UI;

namespace Game.Varginha
{
    /// <summary>
    /// HUD de investigação estilo RPG / Pokémon de terror sobrenatural (O Segredo de Varginha).
    /// Inclui: Guia Rodrigo, Medidor de Saúde, Inventário, Diálogos e Telas de Fim de Fase.
    /// </summary>
    public class VarginhaGameHUD : MonoBehaviour
    {
        public static VarginhaGameHUD Instance { get; private set; }

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

        private Texture2D _whiteTex;
        private GUIStyle _dialogueBoxStyle;
        private GUIStyle _speakerStyle;
        private GUIStyle _dialogueTextStyle;
        private GUIStyle _rodrigoStyle;
        private GUIStyle _promptStyle;
        private GUIStyle _buttonStyle;

        private void Awake()
        {
            Instance = this;
            _rodrigoTypewriter.Set(_rodrigoHint);
        }

        private void OnEnable()
        {
            // Com Enter Play Mode Options, Awake pode não rodar entre tentativas.
            // Reafirma a instância que desenha as interfaces de interação.
            Instance = this;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (Instance == this) Instance = null;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Este HUD vive junto aos managers; uma nova fase precisa sempre iniciar limpa.
            _isVictoryOpen = false;
            _isDialogueOpen = false;
            _lastInteractionInstruction = null;
            _edelzio = Object.FindAnyObjectByType<EdelzioTopDownController>();
        }

        private void Start()
        {
            _edelzio = Object.FindAnyObjectByType<EdelzioTopDownController>();
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                GameManager.Instance.StartGame();
            }
        }

        private void InitStyles()
        {
            if (_dialogueBoxStyle != null) return;

            _whiteTex = new Texture2D(1, 1);
            _whiteTex.SetPixel(0, 0, Color.white);
            _whiteTex.Apply();

            _dialogueBoxStyle = new GUIStyle(GUI.skin.box);

            _speakerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _speakerStyle.normal.textColor = new Color(0.3f, 0.9f, 1f);

            _dialogueTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            _dialogueTextStyle.normal.textColor = Color.white;

            _rodrigoStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            _promptStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
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
        }

        public void ShowDialogue(string speaker, string message)
        {
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
            _isDialogueOpen = false;
            _victoryTitle = title;
            _victoryDescription = description;
            _isVictoryOpen = true;
        }

        private void OnGUI()
        {
            InitStyles();

            _dialogueTypewriter.Tick(38f);
            _rodrigoTypewriter.Tick(28f);
            _interactionTypewriter.Tick(42f);

            if (_isDialogueOpen && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                _dialogueTypewriter.RevealImmediately();
                Event.current.Use();
            }

            if (_edelzio == null)
            {
                _edelzio = Object.FindAnyObjectByType<EdelzioTopDownController>();
            }

            DrawTopBar();
            DrawInventoryBar();
            DrawInteractionPrompt();

            if (_isDialogueOpen)
            {
                DrawDialogueWindow();
            }

            if (_isVictoryOpen)
            {
                DrawVictoryWindow();
            }
            else if (_edelzio != null && _edelzio.CurrentSanity <= 0f)
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

            Rect stabilityPanel = new Rect(18, 16, 340, 100);
            PixelHUDFrame.Draw(stabilityPanel, _whiteTex, new Color(.035f, .09f, .14f, .94f), new Color(.22f, .9f, .95f, .9f));
            GUI.Label(new Rect(stabilityPanel.x, stabilityPanel.y + 10, stabilityPanel.width, 24), "SAUDE", _promptStyle);

            int filledHearts = Mathf.Clamp(Mathf.CeilToInt(ratio * 3f), 0, 3);
            for (int heart = 0; heart < 3; heart++)
            {
                Color heartColor = heart < filledHearts ? new Color(.95f, .12f, .18f) : new Color(.24f, .06f, .08f);
                PixelHUDFrame.DrawHeart(new Rect(62 + heart * 78, 43, 52, 45), _whiteTex, heartColor);
            }

            var sanityStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            sanityStyle.normal.textColor = Color.white;
            PixelUIFont.Apply(sanityStyle);
            GUI.Label(new Rect(32, 88, 312, 18), $"{Mathf.CeilToInt(sanity)}%", sanityStyle);

            // Guia Rodrigo (Topo Centro)
            float rw = Mathf.Min(720, Screen.width - 760);
            if (rw > 280)
            {
                GUI.color = new Color(0.12f, 0.18f, 0.28f, 0.95f);
                Rect guidePanel = new Rect((Screen.width - rw) / 2f, 16, rw, 88);
                PixelHUDFrame.Draw(guidePanel, _whiteTex, new Color(.04f, .10f, .19f, .96f), new Color(.22f, .65f, .9f, .85f));
                GUI.Label(guidePanel, _rodrigoTypewriter.VisibleText, _rodrigoStyle);
                GUI.color = Color.white;
            }
        }

        private void DrawInventoryBar()
        {
            // Inventário (Canto Superior Direito)
            float w = 320;
            float x = Screen.width - w - 18;
            Rect inventoryPanel = new Rect(x, 16, w, 150);
            PixelHUDFrame.Draw(inventoryPanel, _whiteTex, new Color(.035f, .09f, .14f, .94f), new Color(.22f, .9f, .95f, .9f));
            GUI.Label(new Rect(x, 26, w, 22), "INVENTARIO", _promptStyle);

            bool bp = _edelzio != null && _edelzio.HasBackpack;
            bool key = _edelzio != null && _edelzio.HasFuscaKey;
            bool note = _edelzio != null && _edelzio.HasResearchNotebook;
            bool lap = _edelzio != null && _edelzio.HasDecodedData;

            int missions = (bp ? 1 : 0) + (key && note ? 1 : 0) + (lap ? 1 : 0);
            var itemStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            PixelUIFont.Apply(itemStyle);

            GUI.Label(new Rect(x + 14, 50, w - 28, 18), $"MISSOES {missions}/3", itemStyle);
            GUI.Label(new Rect(x + 14, 74, w - 28, 18), $"MOCHILA {(bp ? "OK" : "PENDENTE")}", itemStyle);
            GUI.Label(new Rect(x + 14, 98, w - 28, 18), $"CHAVE E CADERNO {(key && note ? "OK" : "PENDENTE")}", itemStyle);
            GUI.Label(new Rect(x + 14, 122, w - 28, 18), $"NOTEBOOK {(lap ? "OK" : "PENDENTE")}", itemStyle);
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
            float y = Screen.height - h - 30f;
            if (_lastInteractionInstruction != instruction)
            {
                _lastInteractionInstruction = instruction;
                _interactionTypewriter.Set(instruction);
            }

            GUI.color = new Color(0.1f, 0.8f, 0.9f, 0.9f);
            Rect promptPanel = new Rect(x, y, w, h);
            PixelHUDFrame.Draw(promptPanel, _whiteTex, new Color(.025f, .11f, .14f, .96f), new Color(.15f, .85f, .9f, .95f));
            GUI.Label(new Rect(promptPanel.x + 12f, promptPanel.y + 7f, promptPanel.width - 24f, promptPanel.height - 14f), _interactionTypewriter.VisibleText, _promptStyle);
            GUI.color = Color.white;
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

            GUI.color = new Color(0.08f, 0.12f, 0.22f, 0.96f);
            PixelHUDFrame.Draw(modal, _whiteTex, new Color(.025f, .07f, .13f, .98f), new Color(.20f, .82f, .9f, .9f));
            GUI.color = Color.white;

            GUI.Label(new Rect(modal.x + pad, modal.y + pad * .35f, modal.width - pad * 2f, speakerHeight), $"> {_activeSpeaker}", _speakerStyle);
            GUI.Label(new Rect(modal.x + pad, modal.y + speakerHeight + pad, modal.width - pad * 2f, modal.height - speakerHeight - buttonHeight - pad * 2.5f), _dialogueTypewriter.VisibleText, _dialogueTextStyle);

            float buttonWidth = Mathf.Min(modal.width - pad * 2f, 250f);
            if (GUI.Button(new Rect(modal.x + (modal.width - buttonWidth) * .5f, modal.yMax - buttonHeight - pad * .55f, buttonWidth, buttonHeight), "CONTINUAR", _buttonStyle))
            {
                if (_dialogueTypewriter.IsComplete) CloseDialogue();
                else _dialogueTypewriter.RevealImmediately();
            }
        }

        private void DrawVictoryWindow()
        {
            Rect modal = GetModalRect(800f, Mathf.Min(520f, Screen.height * .88f));
            float pad = Mathf.Clamp(modal.width * .06f, 12f, 42f);
            float buttonHeight = Mathf.Clamp(modal.height * .15f, 30f, 52f);
            _buttonStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 14);

            GUI.color = new Color(0.06f, 0.18f, 0.12f, 0.98f);
            PixelHUDFrame.Draw(modal, _whiteTex, new Color(.025f, .12f, .08f, .98f), new Color(.28f, 1f, .55f, .9f));
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
            Rect modal = GetModalRect(700f, 360f);
            float pad = Mathf.Clamp(modal.width * .06f, 12f, 38f);
            float buttonHeight = Mathf.Clamp(modal.height * .16f, 30f, 48f);
            _buttonStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 14);

            GUI.color = new Color(0.25f, 0.05f, 0.08f, 0.98f);
            PixelHUDFrame.Draw(modal, _whiteTex, new Color(.18f, .025f, .055f, .98f), new Color(1f, .22f, .35f, .9f));
            GUI.color = Color.white;

            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 30f), 10, 22), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle.normal.textColor = Color.red;
            PixelUIFont.Apply(titleStyle);
            float titleHeight = titleStyle.CalcHeight(new GUIContent("COLAPSO SOBRENATURAL"), modal.width - pad * 2f);
            GUI.Label(new Rect(modal.x + pad, modal.y + pad, modal.width - pad * 2f, titleHeight), "COLAPSO SOBRENATURAL", titleStyle);

            var descStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 15), wordWrap = true, alignment = TextAnchor.MiddleCenter };
            descStyle.normal.textColor = Color.white;
            PixelUIFont.Apply(descStyle);
            float contentTop = modal.y + pad + titleHeight + pad * .6f;
            GUI.Label(new Rect(modal.x + pad, contentTop, modal.width - pad * 2f, modal.yMax - contentTop - buttonHeight - pad * 1.6f), "A Entidade Ancestral drenou completamente a sanidade de Edelzio antes de alcançar o Fusca...", descStyle);

            float buttonWidth = Mathf.Min(modal.width - pad * 2f, 300f);
            if (GUI.Button(new Rect(modal.x + (modal.width - buttonWidth) * .5f, modal.yMax - buttonHeight - pad * .55f, buttonWidth, buttonHeight), "TENTAR NOVAMENTE", _buttonStyle))
            {
                RestartScene();
            }
        }

        private void RestartScene()
        {
            Time.timeScale = 1f;
            _isVictoryOpen = false;
            _isDialogueOpen = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private static Rect GetModalRect(float maxWidth, float maxHeight)
        {
            float horizontalMargin = Mathf.Clamp(Screen.width * .04f, 8f, 36f);
            float verticalMargin = Mathf.Clamp(Screen.height * .05f, 8f, 36f);
            float width = Mathf.Min(maxWidth, Screen.width - horizontalMargin * 2f);
            float height = Mathf.Min(maxHeight, Screen.height - verticalMargin * 2f);
            return new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, width, height);
        }

        private static Rect GetBottomDialogRect(float maxWidth, float maxHeight)
        {
            Rect rect = GetModalRect(maxWidth, maxHeight);
            float bottomMargin = Mathf.Clamp(Screen.height * .04f, 8f, 30f);
            rect.y = Screen.height - rect.height - bottomMargin;
            return rect;
        }
    }
}
