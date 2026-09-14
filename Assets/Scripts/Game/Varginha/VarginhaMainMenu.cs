using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.UI;

namespace Game.Varginha
{
    /// <summary>
    /// Tela inicial provisoria de Mistério de Varginha. Não depende de artes ou fontes externas,
    /// permitindo que o protótipo mantenha a identidade visual enquanto os assets finais chegam.
    /// </summary>
    public class VarginhaMainMenu : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "FaseTopView_Varginha";
        [SerializeField] private string phase3SceneName = "Fase3_Igreja_Guardiao";

        private enum Panel { None, Controls, Credits, Difficulty, Keybinds }
        private Panel panel;
        private string _pendingScene;
        private Vector2 _controlsScroll;
        private Vector2 _bindingsScroll;
        private VarginhaInputAction? _rebindingAction;
        private int _rebindArmedFrame = -1;
        private Texture2D pixel;
        private Texture2D backgroundTexture;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle infoStyle;
        private GUIStyle leftInfoStyle;
        private GUIStyle smallStyle;
        private readonly TypewriterText panelTypewriter = new TypewriterText();

        private void Awake()
        {
            // O menu usa OnGUI, mas o Game View ainda exige uma câmera ativa para não mostrar
            // "No cameras rendering" quando a cena é aberta isoladamente.
            if (Camera.main == null)
            {
                var cameraObject = new GameObject("Menu Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 0f, -10f);
                cameraObject.AddComponent<Camera>().backgroundColor = new Color(.005f, .015f, .04f);
                cameraObject.AddComponent<AudioListener>();
            }

            pixel = new Texture2D(1, 1);
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply();
            backgroundTexture = Resources.Load<Texture2D>("Varginha/MenuBackgroundV1");
        }

        private void OnGUI()
        {
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            BuildStyles();
            DrawBackground();

            if (panel != Panel.None)
            {
                if (panel == Panel.Difficulty) DrawDifficulty();
                else if (panel == Panel.Keybinds) DrawKeybinds();
                else DrawPanel();
                return;
            }

            float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1280f, Screen.height / 720f), .55f, 1.15f);
            ConfigureResponsiveFonts(scale);

            float width = Mathf.Min(940f * scale, Screen.width - 48f);
            float x = (Screen.width - width) * .5f;
            float y = Mathf.Max(24f, Screen.height * .07f);
            float titleLineHeight = 58f * scale;
            float subtitleY = y + titleLineHeight * 2f + 6f * scale;
            float descriptionPanelY = subtitleY + 34f * scale;
            float descriptionPanelHeight = 212f * scale;

            GUI.Label(new Rect(x, y, width, titleLineHeight), "MISTERIO DE", titleStyle);
            GUI.Label(new Rect(x, y + titleLineHeight, width, titleLineHeight), "VARGINHA", titleStyle);
            GUI.Label(new Rect(x, subtitleY, width, 28f * scale), "PROTOTIPO  •  INVESTIGACAO SOBRENATURAL", subtitleStyle);

            GUI.color = new Color(.025f, .06f, .10f, .88f);
            GUI.DrawTexture(new Rect(x, descriptionPanelY, width, descriptionPanelHeight), pixel);
            GUI.color = Color.white;

            GUI.Label(new Rect(x + 42f * scale, descriptionPanelY + 22f * scale, width - 84f * scale, 98f * scale),
                "Varginha, 2026. Luzes voltaram a surgir sobre a cidade — e as memórias de Edelzio sobre 1996 não são mais confiáveis.", infoStyle);
            GUI.Label(new Rect(x + 42f * scale, descriptionPanelY + 132f * scale, width - 84f * scale, 48f * scale),
                "Explore, conecte pistas e descubra o que ainda dorme sob a cidade.", smallStyle);

            float buttonWidth = Mathf.Min(480f * scale, width - 96f * scale);
            float buttonX = x + (width - buttonWidth) * .5f;
            float buttonY = descriptionPanelY + descriptionPanelHeight + 28f * scale;
            if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, 52f * scale), "INICIAR INVESTIGAÇÃO", buttonStyle))
                StartInvestigation();
            if (GUI.Button(new Rect(buttonX, buttonY + 66f * scale, buttonWidth, 42f * scale), "TUTORIAL / CONTROLES", buttonStyle))
                OpenPanel(Panel.Controls);
            if (GUI.Button(new Rect(buttonX, buttonY + 120f * scale, buttonWidth, 42f * scale), "CREDITOS", buttonStyle))
                OpenPanel(Panel.Credits);
            if (GUI.Button(new Rect(buttonX, buttonY + 174f * scale, buttonWidth, 42f * scale), "JOGAR FASE 3 — O GUARDIAO", buttonStyle))
                StartPhase3();

            GUI.Label(new Rect(20, Screen.height - 42, Screen.width - 40, 24),
                "SINAL DETECTADO  •  VARGINHA / MG  •  21:17", smallStyle);

            if (panel == Panel.Controls || panel == Panel.Credits) DrawPanel();
        }

        private void DrawBackground()
        {
            if (backgroundTexture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundTexture, ScaleMode.ScaleAndCrop);
                // Vinheta que garante contraste sem ocultar a arte do ET e da luz no lado direito.
                GUI.color = new Color(.005f, .015f, .04f, .56f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pixel);
            }
            else
            {
                GUI.color = new Color(.015f, .025f, .06f, 1f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pixel);
            }
            GUI.color = new Color(.12f, .9f, .88f, .10f);
            GUI.DrawTexture(new Rect(0, Screen.height * .42f, Screen.width, 2), pixel);
            GUI.DrawTexture(new Rect(Screen.width * .16f, 0, 2, Screen.height), pixel);
            GUI.DrawTexture(new Rect(Screen.width * .82f, 0, 2, Screen.height), pixel);
            GUI.color = Color.white;
        }

        private void DrawPanel()
        {
            float width = Mathf.Min(620f, Screen.width - 40f);
            float height = Mathf.Min(panel == Panel.Controls ? 550f : 300f, Screen.height - 24f);
            float x = (Screen.width - width) * .5f;
            float y = (Screen.height - height) * .5f;
            GUI.color = new Color(.03f, .08f, .13f, .98f);
            GUI.DrawTexture(new Rect(x, y, width, height), pixel);
            GUI.color = Color.white;

            string heading = panel == Panel.Controls ? "CONTROLES DE INVESTIGACAO" : "CREDITOS";
            GUI.Label(new Rect(x + 30, y + 24, width - 60, 32), heading, subtitleStyle);
            panelTypewriter.Tick(32f);
            Rect viewport = new Rect(x + 24, y + 65, width - 48, height - (panel == Panel.Controls ? 165 : 130));
            float contentHeight = infoStyle.CalcHeight(new GUIContent(panelTypewriter.VisibleText), viewport.width - 24);
            _controlsScroll = GUI.BeginScrollView(viewport, _controlsScroll, new Rect(0, 0, viewport.width - 24, contentHeight));
            GUI.Label(new Rect(0, 0, viewport.width - 24, contentHeight), panelTypewriter.VisibleText, infoStyle);
            GUI.EndScrollView();
            if (panel == Panel.Controls && GUI.Button(new Rect(x + width * .5f - 180, y + height - 92, 360, 32),
                "EDITAR CONTROLES DO TECLADO E MOUSE", buttonStyle))
                OpenPanel(Panel.Keybinds);
            if (GUI.Button(new Rect(x + width * .5f - 100, y + height - 55, 200, 34), "VOLTAR", buttonStyle))
                panel = Panel.None;
        }

        private void DrawKeybinds()
        {
            float width = Mathf.Min(820f, Screen.width - 32f);
            float height = Mathf.Min(650f, Screen.height - 24f);
            float x = (Screen.width - width) * .5f;
            float y = (Screen.height - height) * .5f;
            GUI.color = new Color(.025f, .07f, .12f, .99f);
            GUI.DrawTexture(new Rect(x, y, width, height), pixel);
            GUI.color = Color.white;
            GUI.Label(new Rect(x + 30, y + 20, width - 60, 30), "EDITAR CONTROLES", subtitleStyle);
            GUI.Label(new Rect(x + 30, y + 52, width - 60, 26),
                _rebindingAction.HasValue ? "PRESSIONE UMA TECLA OU CLIQUE UM BOTÃO DO MOUSE • ESC CANCELA" :
                    "Selecione um comando para trocar sua tecla ou botão principal.", smallStyle);

            Rect viewport = new Rect(x + 26, y + 88, width - 52, height - 158);
            float rowHeight = 45f;
            float contentHeight = Enum.GetValues(typeof(VarginhaInputAction)).Length * rowHeight + 12f;
            _bindingsScroll = GUI.BeginScrollView(viewport, _bindingsScroll,
                new Rect(0f, 0f, viewport.width - 18f, contentHeight));

            Rect activeBindingRect = Rect.zero;
            Rect resetRect = Rect.zero;
            bool pointerOverKeybindControl = false;
            for (int i = 0; i < Enum.GetValues(typeof(VarginhaInputAction)).Length; i++)
            {
                var action = (VarginhaInputAction)i;
                float rowY = 6f + i * rowHeight;
                GUI.color = new Color(.06f, .13f, .19f, 1f);
                GUI.DrawTexture(new Rect(0f, rowY, viewport.width - 18f, 38f), pixel);
                GUI.color = Color.white;
                GUI.Label(new Rect(14f, rowY + 8f, 250f, 24f), VarginhaInputBindings.ActionName(action), leftInfoStyle);

                Rect bindingRect = new Rect(viewport.width - 282f, rowY + 4f, 190f, 30f);
                Rect bindingScreenRect = new Rect(viewport.x + bindingRect.x, viewport.y + bindingRect.y - _bindingsScroll.y,
                    bindingRect.width, bindingRect.height);
                Rect resetButton = new Rect(viewport.width - 82f, rowY + 4f, 70f, 30f);
                Rect resetScreenRect = new Rect(viewport.x + resetButton.x, viewport.y + resetButton.y - _bindingsScroll.y,
                    resetButton.width, resetButton.height);
                if (Event.current.type == EventType.MouseDown
                    && (bindingScreenRect.Contains(Event.current.mousePosition) || resetScreenRect.Contains(Event.current.mousePosition)))
                    pointerOverKeybindControl = true;
                string bindingText = _rebindingAction == action ? "PRESSIONE..." : VarginhaInputBindings.DisplayName(action);
                if (GUI.Button(bindingRect, bindingText, buttonStyle))
                {
                    _rebindingAction = action;
                    _rebindArmedFrame = Time.frameCount;
                }
                if (_rebindingAction == action)
                    activeBindingRect = bindingScreenRect;

                if (GUI.Button(resetButton, "RESET", buttonStyle))
                {
                    VarginhaInputBindings.Reset(action);
                    if (_rebindingAction == action) _rebindingAction = null;
                }
                if (_rebindingAction == action)
                    resetRect = resetScreenRect;
            }
            GUI.EndScrollView();

            Rect backRect = new Rect(x + width * .5f - 125, y + height - 54, 250, 34);
            bool pointerOverBack = Event.current.type == EventType.MouseDown && backRect.Contains(Event.current.mousePosition);
            CaptureRebindInput(activeBindingRect, resetRect, pointerOverKeybindControl || pointerOverBack);
            if (GUI.Button(backRect, "VOLTAR", buttonStyle))
            {
                _rebindingAction = null;
                panel = Panel.None;
            }
        }

        private void CaptureRebindInput(Rect bindingRect, Rect resetRect, bool pointerOverProtectedControl)
        {
            if (!_rebindingAction.HasValue || Time.frameCount <= _rebindArmedFrame) return;
            Event current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                if (current.keyCode == KeyCode.Escape)
                {
                    _rebindingAction = null;
                    current.Use();
                    return;
                }
                if (VarginhaInputBindings.TrySetFromKeyCode(_rebindingAction.Value, current.keyCode))
                {
                    _rebindingAction = null;
                    current.Use();
                }
                return;
            }

            // Não capture o clique usado no botão RESET nem um clique que apenas
            // reabre a própria linha. O próximo clique fora dessas áreas é o novo mouse.
            if (current.type == EventType.MouseDown && !pointerOverProtectedControl
                && !bindingRect.Contains(current.mousePosition)
                && !resetRect.Contains(current.mousePosition) && current.button >= 0 && current.button <= 2)
            {
                VarginhaInputBindings.SetMouseButton(_rebindingAction.Value, current.button);
                _rebindingAction = null;
                current.Use();
            }
        }

        private void OpenPanel(Panel nextPanel)
        {
            panel = nextPanel;
            _controlsScroll = Vector2.zero;
            _bindingsScroll = Vector2.zero;
            _rebindingAction = null;
            if (nextPanel == Panel.Keybinds) return;
            panelTypewriter.Set(nextPanel == Panel.Controls
                ? "WASD / SETAS — mover em 8 direções\nSHIFT — correr\nE / ESPAÇO — examinar objetos e pistas\n\nATAQUE — segure para encadear o combo de três golpes diferentes. O segundo cruza a guarda; o terceiro é um finalizador mais forte.\nCOMANDO DE ALIADO — na fase final, chame um aluno pronto por vez; cada aluno tem recarga individual de 5 segundos.\n\nMire perto do ET desejado. Sem alvo na mira, a turma prioriza ameaças próximas e escolhe a especialidade adequada. A invocação exige proximidade e caminho livre de paredes. O golpe tem preparação e pode errar se o ET sair da área. Desvie dos círculos vermelhos dos inimigos.\n\nMarcos alterna três golpes e grita bordões durante as jogadas. Matias imobiliza; Luis Martins e Luis Miguel interrompem inimigos. Pedro, Luis Miguel e Yasmin atingem grupos. Anna e Ana fazem ricochete; Fabio finaliza inimigos feridos.\n\nUse EDITAR CONTROLES para trocar qualquer comando por uma tecla ou botão do mouse. As escolhas ficam salvas para as próximas fases."
                : "Mistério de Varginha\nProtótipo de terror sobrenatural e investigação\n\nBaseado no GDD: Edelzio, a Entidade Ancestral e os segredos de Varginha.");
            if (nextPanel == Panel.Controls) panelTypewriter.RevealImmediately();
        }

        private void StartInvestigation()
        {
            _pendingScene = gameplaySceneName;
            panel = Panel.Difficulty;
        }

        private void StartPhase3()
        {
            _pendingScene = phase3SceneName;
            panel = Panel.Difficulty;
        }

        private void DrawDifficulty()
        {
            titleStyle.fontSize = 26;
            infoStyle.fontSize = 12;
            subtitleStyle.fontSize = 14;
            buttonStyle.fontSize = 13;
            var previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) * .5f,
                (Screen.height - 720 * scale) * .5f), Quaternion.identity, Vector3.one * scale);
            GUI.color = new Color(.02f, .05f, .09f, .97f);
            GUI.DrawTexture(new Rect(90, 70, 1100, 580), pixel);
            GUI.color = Color.white;
            GUI.Label(new Rect(150, 105, 980, 60), "COMO VOCÊ VAI INVESTIGAR?", titleStyle);
            GUI.Label(new Rect(180, 175, 920, 40), "A escolha vale para todas as fases e para as novas tentativas.", infoStyle);
            string[] titles = { "FÁCIL", "MÉDIO", "DIFÍCIL" };
            string[] descriptions = {
                "Para explorar a história.\n\nETs menos resistentes e mais lentos. Avisos de ataque longos e dano reduzido.",
                "Uma investigação perigosa.\n\nETs mais resistentes, ataques frequentes e flanqueamento. Combine esquivas e aliados.",
                "Sobreviva à noite.\n\nETs rápidos, resistentes e agressivos. Avisos curtos e maior resistência aos atordoamentos."
            };
            Color[] colors = { new Color(.35f, .95f, .65f), new Color(1f, .78f, .35f), new Color(1f, .38f, .4f) };
            for (int i = 0; i < 3; i++)
            {
                Rect card = new Rect(140 + i * 340, 250, 320, 295);
                GUI.color = new Color(.055f, .11f, .17f);
                GUI.DrawTexture(card, pixel);
                GUI.color = colors[i];
                GUI.DrawTexture(new Rect(card.x, card.y, card.width, 4), pixel);
                GUI.Label(new Rect(card.x + 20, card.y + 24, 280, 35), titles[i], subtitleStyle);
                GUI.color = Color.white;
                GUI.Label(new Rect(card.x + 24, card.y + 75, 272, 140), descriptions[i], infoStyle);
                if (GUI.Button(new Rect(card.x + 25, card.y + 235, 270, 40), "JOGAR NO " + titles[i], buttonStyle))
                {
                    if (Application.CanStreamedLevelBeLoaded(_pendingScene))
                    {
                        VarginhaDifficulty.Select((InvestigationDifficulty)i);
                        Time.timeScale = 1f;
                        Game.Managers.GameManager.Instance?.StartGame();
                        SceneManager.LoadScene(_pendingScene);
                    }
                }
            }
            if (GUI.Button(new Rect(490, 580, 300, 40), "VOLTAR", buttonStyle)) panel = Panel.None;
            GUI.matrix = previous;
        }

        private void ConfigureResponsiveFonts(float scale)
        {
            titleStyle.fontSize = Mathf.RoundToInt(42f * scale);
            subtitleStyle.fontSize = Mathf.RoundToInt(14f * scale);
            infoStyle.fontSize = Mathf.RoundToInt(15f * scale);
            smallStyle.fontSize = Mathf.RoundToInt(12f * scale);
            buttonStyle.fontSize = Mathf.RoundToInt(14f * scale);
        }

        private void BuildStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 42, fontStyle = FontStyle.Bold, wordWrap = false };
            titleStyle.normal.textColor = new Color(.62f, 1f, .94f);
            subtitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold };
            subtitleStyle.normal.textColor = new Color(.38f, .85f, .9f);
            infoStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 15, wordWrap = true };
            infoStyle.normal.textColor = new Color(.88f, .95f, 1f);
            leftInfoStyle = new GUIStyle(infoStyle) { alignment = TextAnchor.MiddleLeft, wordWrap = false };
            smallStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12, wordWrap = true };
            smallStyle.normal.textColor = new Color(.54f, .72f, .78f);
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            buttonStyle.normal.textColor = new Color(.78f, 1f, .96f);

            PixelUIFont.Apply(titleStyle);
            PixelUIFont.Apply(subtitleStyle);
            PixelUIFont.Apply(infoStyle);
            PixelUIFont.Apply(smallStyle);
            PixelUIFont.Apply(buttonStyle);
        }
    }
}
