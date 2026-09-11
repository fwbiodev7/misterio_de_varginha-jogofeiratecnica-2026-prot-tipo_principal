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

        private enum Panel { None, Controls, Credits }
        private Panel panel;
        private Texture2D pixel;
        private Texture2D backgroundTexture;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle infoStyle;
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
            BuildStyles();
            DrawBackground();

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
            if (GUI.Button(new Rect(buttonX, buttonY + 66f * scale, buttonWidth, 42f * scale), "CONTROLES", buttonStyle))
                OpenPanel(Panel.Controls);
            if (GUI.Button(new Rect(buttonX, buttonY + 120f * scale, buttonWidth, 42f * scale), "CREDITOS", buttonStyle))
                OpenPanel(Panel.Credits);

            GUI.Label(new Rect(20, Screen.height - 42, Screen.width - 40, 24),
                "SINAL DETECTADO  •  VARGINHA / MG  •  21:17", smallStyle);

            if (panel != Panel.None) DrawPanel();
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
            float height = 250f;
            float x = (Screen.width - width) * .5f;
            float y = (Screen.height - height) * .5f;
            GUI.color = new Color(.03f, .08f, .13f, .98f);
            GUI.DrawTexture(new Rect(x, y, width, height), pixel);
            GUI.color = Color.white;

            string heading = panel == Panel.Controls ? "CONTROLES DE INVESTIGACAO" : "CREDITOS";
            GUI.Label(new Rect(x + 30, y + 24, width - 60, 32), heading, subtitleStyle);
            panelTypewriter.Tick(32f);
            GUI.Label(new Rect(x + 45, y + 75, width - 90, 115), panelTypewriter.VisibleText, infoStyle);
            if (GUI.Button(new Rect(x + width * .5f - 100, y + height - 55, 200, 34), "VOLTAR", buttonStyle))
                panel = Panel.None;
        }

        private void OpenPanel(Panel nextPanel)
        {
            panel = nextPanel;
            panelTypewriter.Set(nextPanel == Panel.Controls
                ? "WASD / SETAS  —  mover Edelzio\nSHIFT  —  correr\nE ou ESPAÇO  —  examinar objetos e pistas\n\nUse as pistas, o caderno e o notebook para reconstruir a verdade."
                : "Mistério de Varginha\nProtótipo de terror sobrenatural e investigação\n\nBaseado no GDD: Edelzio, a Entidade Ancestral e os segredos de Varginha.");
        }

        private void StartInvestigation()
        {
            if (Application.CanStreamedLevelBeLoaded(gameplaySceneName))
                SceneManager.LoadScene(gameplaySceneName);
            else
                Debug.LogWarning("[Mistério de Varginha] A cena de investigação ainda não está adicionada ao Build Settings.");
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
