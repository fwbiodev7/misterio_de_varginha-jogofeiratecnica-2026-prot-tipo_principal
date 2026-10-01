using UnityEngine;
using Game.UI;

namespace Game.Varginha
{
    /// <summary>Quiz de investigação que libera o notebook após confirmar pistas do Caso de Varginha.</summary>
    public class VarginhaNotebookQuiz : MonoBehaviour
    {
        public static VarginhaNotebookQuiz Instance { get; private set; }

        private readonly string[] _questions =
        {
            "EM QUAL CIDADE ACONTECEU O CASO DE 1996?",
            "QUAL OBJETO DEVOLVE AS MEMORIAS DE EDELZIO?",
            "QUAL E A PROFISSAO DE EDELZIO?"
        };
        private readonly string[][] _answers =
        {
            new[] { "VARGINHA", "CAMPINAS", "BELO HORIZONTE" },
            new[] { "O CADERNO DE 1996", "UMA MOEDA", "UM MAPA" },
            new[] { "PROFESSOR DE TI", "POLICIAL", "MEDICO" }
        };
        private readonly int[] _correctAnswers = { 0, 0, 0 };

        private Texture2D _pixel;
        private Texture2D _background;
        private GUIStyle _titleStyle;
        private GUIStyle _questionStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _feedbackStyle;
        private GUIStyle _sidebarStyle;
        private GUIStyle _codeStyle;
        private GUIStyle _lineNumberStyle;
        private EdelzioTopDownController _player;
        private InteractableProp _notebook;
        private int _questionIndex;
        private string _feedback;
        private bool _isOpen;
        public bool IsOpen => _isOpen;
        private float _timeScaleBeforeOpening = 1f;
        private float _reactionUntil;
        private float _webcamStarted;

        private void Awake()
        {
            Instance = this;
            _pixel = new Texture2D(1, 1);
            _pixel.SetPixel(0, 0, Color.white);
            _pixel.Apply();
            _background = Resources.Load<Texture2D>("Varginha/MenuBackgroundV1");
        }

        private void OnEnable()
        {
            // Garante que o quiz volta a ser encontrável após um reload de domínio/cena.
            Instance = this;
        }

        public void Open(EdelzioTopDownController player, InteractableProp notebook)
        {
            if (!isActiveAndEnabled || _isOpen || player == null || player.HasDecodedData) return;
            _player = player;
            _notebook = notebook;
            _questionIndex = 0;
            _feedback = "DECODIFIQUE AS PISTAS PARA LIBERAR O NOTEBOOK";
            _isOpen = true;
            _reactionUntil = 0f;
            _webcamStarted = Time.unscaledTime;
            _player.SetInputLocked(true);
            _timeScaleBeforeOpening = Time.timeScale;
            Time.timeScale = 0f;
        }

        private void OnDisable()
        {
            if (_isOpen) Close();
            if (Instance == this) Instance = null;
        }

        private void OnGUI()
        {
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            if (!_isOpen) return;
            if (_questionIndex < 0 || _questionIndex >= _questions.Length)
            {
                Close();
                return;
            }

            InitStyles();
            GUI.depth = -1000;
            int displayedQuestion = _questionIndex;

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _background != null ? _background : _pixel, ScaleMode.ScaleAndCrop);
            GUI.color = new Color(.005f, .015f, .04f, .78f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _pixel);
            GUI.color = Color.white;

            float margin = Mathf.Clamp(Screen.width * .05f, 10f, 60f);
            Rect panel = new Rect(margin, margin, Screen.width - margin * 2f, Screen.height - margin * 2f);
            PixelHUDFrame.Draw(panel, _pixel, new Color(.012f, .022f, .05f, .98f), new Color(.10f, .56f, .76f, .95f));

            float scale = Mathf.Clamp(Mathf.Min(panel.width / 920f, panel.height / 560f), .55f, 1f);
            ConfigureStyles(scale);

            float titleBarHeight = Mathf.Max(28f, 42f * scale);
            Rect titleBar = new Rect(panel.x + 4, panel.y + 4, panel.width - 8, titleBarHeight);
            DrawFill(titleBar, new Color(.02f, .10f, .20f));
            DrawFill(new Rect(titleBar.x + 12f * scale, titleBar.y + 12f * scale, 7f * scale, 7f * scale), new Color(.95f, .30f, .32f));
            DrawFill(new Rect(titleBar.x + 23f * scale, titleBar.y + 12f * scale, 7f * scale, 7f * scale), new Color(.96f, .72f, .25f));
            DrawFill(new Rect(titleBar.x + 34f * scale, titleBar.y + 12f * scale, 7f * scale, 7f * scale), new Color(.32f, .82f, .47f));
            GUI.Label(new Rect(titleBar.x + 52f * scale, titleBar.y, titleBar.width - 110f * scale, titleBar.height), "EDELZIO_NOTEBOOK  -  VARGINHA CASE", _titleStyle);

            float terminalHeight = Mathf.Max(48f, 82f * scale);
            float contentY = titleBar.yMax + 4f;
            float contentHeight = panel.yMax - contentY - terminalHeight - 8f;
            float sidebarWidth = Mathf.Clamp(188f * scale, 108f, panel.width * .29f);
            Rect sidebar = new Rect(panel.x + 4, contentY, sidebarWidth, contentHeight);
            Rect workspace = new Rect(sidebar.xMax + 4, contentY, panel.xMax - sidebar.xMax - 8, contentHeight);
            Rect terminal = new Rect(panel.x + 4, workspace.yMax + 4, panel.width - 8, terminalHeight);

            PixelHUDFrame.Draw(sidebar, _pixel, new Color(.018f, .055f, .10f), new Color(.10f, .25f, .37f));
            GUI.Label(new Rect(sidebar.x + 12f * scale, sidebar.y + 10f * scale, sidebar.width - 24f * scale, 18f * scale), "EXPLORER", _sidebarStyle);
            GUI.Label(new Rect(sidebar.x + 18f * scale, sidebar.y + 40f * scale, sidebar.width - 30f * scale, 18f * scale), "VARGINHA_1996", _sidebarStyle);
            GUI.Label(new Rect(sidebar.x + 28f * scale, sidebar.y + 65f * scale, sidebar.width - 38f * scale, 18f * scale), "> pistas/", _sidebarStyle);
            GUI.Label(new Rect(sidebar.x + 28f * scale, sidebar.y + 88f * scale, sidebar.width - 38f * scale, 34f * scale), "  caso_varginha.quiz", _sidebarStyle);
            GUI.Label(new Rect(sidebar.x + 28f * scale, sidebar.y + 126f * scale, sidebar.width - 38f * scale, 18f * scale), "  caderno_1996.log", _sidebarStyle);
            DrawWebcam(sidebar, scale);

            PixelHUDFrame.Draw(workspace, _pixel, new Color(.012f, .035f, .07f), new Color(.08f, .34f, .48f));
            float tabHeight = Mathf.Max(26f, 34f * scale);
            DrawFill(new Rect(workspace.x + 3, workspace.y + 3, workspace.width - 6, tabHeight), new Color(.035f, .12f, .20f));
            GUI.Label(new Rect(workspace.x + 13f * scale, workspace.y + 3, workspace.width - 20f * scale, tabHeight), "caso_varginha.quiz     x", _sidebarStyle);

            float editorX = workspace.x + 14f * scale;
            float lineWidth = 32f * scale;
            float editorY = workspace.y + tabHeight + 14f * scale;
            GUI.Label(new Rect(editorX, editorY, lineWidth, 25f * scale), "01", _lineNumberStyle);
            GUI.Label(new Rect(editorX + lineWidth, editorY, workspace.width - lineWidth - 28f * scale, 25f * scale), "// DECODIFICAR PISTA", _codeStyle);
            GUI.Label(new Rect(editorX, editorY + 29f * scale, lineWidth, 56f * scale), "02", _lineNumberStyle);
            GUI.Label(new Rect(editorX + lineWidth, editorY + 29f * scale, workspace.width - lineWidth - 28f * scale, 56f * scale), _questions[displayedQuestion], _questionStyle);
            GUI.Label(new Rect(editorX, editorY + 91f * scale, lineWidth, 20f * scale), "03", _lineNumberStyle);
            GUI.Label(new Rect(editorX + lineWidth, editorY + 91f * scale, workspace.width - lineWidth - 28f * scale, 20f * scale), "ESCOLHA A RESPOSTA CORRETA:", _codeStyle);

            float buttonX = workspace.x + 15f * scale;
            float buttonWidth = workspace.width - 30f * scale;
            float buttonHeight = Mathf.Clamp(40f * scale, 26f, 44f);
            float firstButtonY = editorY + 120f * scale;
            for (int i = 0; i < _answers[displayedQuestion].Length; i++)
            {
                Rect answerRect = new Rect(buttonX, firstButtonY + i * (buttonHeight + 8f * scale), buttonWidth, buttonHeight);
                PixelHUDFrame.Draw(answerRect, _pixel, new Color(.025f, .10f, .16f), new Color(.10f, .40f, .54f));
                if (GUI.Button(answerRect, GUIContent.none, GUIStyle.none))
                {
                    Answer(i);
                    GUI.depth = 0;
                    return;
                }
                GUI.Label(answerRect, $"[{(char)('A' + i)}]  {_answers[displayedQuestion][i]}", _buttonStyle);
            }

            PixelHUDFrame.Draw(terminal, _pixel, new Color(.006f, .023f, .04f), new Color(.06f, .42f, .42f));
            GUI.Label(new Rect(terminal.x + 14f * scale, terminal.y + 8f * scale, terminal.width - 180f * scale, 18f * scale), "TERMINAL DE INVESTIGACAO", _sidebarStyle);
            GUI.Label(new Rect(terminal.x + 14f * scale, terminal.y + 30f * scale, terminal.width - 180f * scale, terminal.height - 32f * scale), $"> {_feedback}", _feedbackStyle);
            Rect exitRect = new Rect(terminal.xMax - 108f * scale, terminal.y + terminal.height - 29f * scale, 92f * scale, 20f * scale);
            PixelHUDFrame.Draw(exitRect, _pixel, new Color(.13f, .04f, .06f), new Color(.62f, .20f, .25f));
            if (GUI.Button(exitRect, GUIContent.none, GUIStyle.none))
                Close();
            GUI.Label(exitRect, "SAIR", _buttonStyle);

            GUI.depth = 0;
        }

        private void Answer(int choice)
        {
            if (choice != _correctAnswers[_questionIndex])
            {
                _feedback = "RESPOSTA INCORRETA. ANALISE AS PISTAS.";
                return;
            }

            _questionIndex++;
            _reactionUntil = Time.unscaledTime + 1.1f;
            if (_questionIndex < _questions.Length)
            {
                _feedback = "PISTA CONFIRMADA. PROXIMA PERGUNTA.";
                return;
            }

            _player.HasDecodedData = true;
            _player.EquipNotebook();
            _notebook?.CompleteNotebookPuzzle(_player);
            VarginhaGameHUD.Instance?.ShowDialogue("NOTEBOOK", "DECODIFICACAO CONCLUIDA. O selo de 1898 esta colapsando sob Varginha.");
            VarginhaGameHUD.Instance?.ShowRodrigoHint("Rodrigo: 'Voce conseguiu! Agora va ate o Fusca antes que a entidade bloqueie o caminho.'");
            Close();
        }

        private void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            Time.timeScale = _timeScaleBeforeOpening;
            var action = _player != null ? _player.GetComponent<VarginhaPlayerActionAnimation>() : null;
            if (action != null && action.isActiveAndEnabled) action.FinishNotebookSession();
            else _player?.SetInputLocked(false);
        }

        public void CancelForPlayer(EdelzioTopDownController player)
        {
            if (_isOpen && _player == player) Close();
        }

        private void OnDestroy()
        {
            if (_pixel != null) Destroy(_pixel);
        }

        private void InitStyles()
        {
            if (_titleStyle != null) return;
            _titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
            _titleStyle.normal.textColor = new Color(.4f, 1f, .9f);
            _questionStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _questionStyle.normal.textColor = Color.white;
            _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            _buttonStyle.normal.textColor = new Color(.8f, 1f, .95f);
            _feedbackStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _feedbackStyle.normal.textColor = new Color(.7f, .9f, 1f);
            _sidebarStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, wordWrap = true };
            _sidebarStyle.normal.textColor = new Color(.56f, .78f, .87f);
            _codeStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, wordWrap = true };
            _codeStyle.normal.textColor = new Color(.46f, .95f, .74f);
            _lineNumberStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
            _lineNumberStyle.normal.textColor = new Color(.25f, .43f, .54f);
            PixelUIFont.Apply(_titleStyle);
            PixelUIFont.Apply(_questionStyle);
            PixelUIFont.Apply(_buttonStyle);
            PixelUIFont.Apply(_feedbackStyle);
            PixelUIFont.Apply(_sidebarStyle);
            PixelUIFont.Apply(_codeStyle);
            PixelUIFont.Apply(_lineNumberStyle);
        }

        private void DrawWebcam(Rect sidebar, float scale)
        {
            float size = Mathf.Min(sidebar.width - 16f * scale, sidebar.height - 168f * scale);
            if (size < 42f) return;
            Rect frame = new Rect(sidebar.center.x - size * .5f, sidebar.yMax - size - 24f * scale, size, size);
            PixelHUDFrame.Draw(frame, _pixel, new Color(.04f, .085f, .10f), new Color(.25f, .65f, .57f));
            // This is Edelzio's fictional notebook camera, animated even while the quiz pauses the world.
            var portrait = VarginhaInteractionSprites.Webcam(Time.unscaledTime - _webcamStarted, Time.unscaledTime < _reactionUntil);
            VarginhaDialoguePortraits.Draw(new Rect(frame.x + 5f, frame.y + 12f * scale, frame.width - 10f, frame.height - 16f * scale), portrait);
            DrawFill(new Rect(frame.x + 7f, frame.y + 7f, 4f * scale, 4f * scale), new Color(.35f, .95f, .58f));
            GUI.Label(new Rect(frame.x + 14f * scale, frame.y + 2f, frame.width - 20f * scale, 14f * scale), "CAM 01", _feedbackStyle);
            GUI.Label(new Rect(frame.x, frame.yMax + 3f, frame.width, 18f * scale), "EDELZIO • AO VIVO", _feedbackStyle);
        }

        private void ConfigureStyles(float scale)
        {
            _titleStyle.fontSize = Mathf.RoundToInt(14f * scale);
            _questionStyle.fontSize = Mathf.RoundToInt(13f * scale);
            _buttonStyle.fontSize = Mathf.RoundToInt(11f * scale);
            _feedbackStyle.fontSize = Mathf.RoundToInt(9f * scale);
            _sidebarStyle.fontSize = Mathf.RoundToInt(9f * scale);
            _codeStyle.fontSize = Mathf.RoundToInt(10f * scale);
            _lineNumberStyle.fontSize = Mathf.RoundToInt(9f * scale);
        }

        private void DrawFill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, _pixel);
            GUI.color = Color.white;
        }
    }
}
