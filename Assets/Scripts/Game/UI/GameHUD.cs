using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Player;
using Game.Managers;

namespace Game.UI
{
    /// <summary>
    /// HUD em jogo que exibe Vida do Jogador, Pontuação, Objetivo e telas de Vitória / Game Over.
    /// Funciona nativamente sem depender de Canvas ou fontes externas.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        private HealthSystem _playerHealth;
        private GUIStyle _titleStyle;
        private GUIStyle _subStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _hudBoxStyle;
        private Texture2D _whiteTex;
        private readonly TypewriterText _objectiveTypewriter = new TypewriterText();

        private void Start()
        {
            FindPlayer();
            _objectiveTypewriter.Set("MISSAO: DESVIE DOS INIMIGOS E ALCANCE A META");

            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                GameManager.Instance.StartGame();
            }
        }

        private void FindPlayer()
        {
            var pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null)
            {
                _playerHealth = pc.GetComponent<HealthSystem>();
            }
        }

        private void InitStyles()
        {
            if (_titleStyle != null) return;

            _whiteTex = new Texture2D(1, 1);
            _whiteTex.SetPixel(0, 0, Color.white);
            _whiteTex.Apply();

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = Color.yellow;

            _subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            _subStyle.normal.textColor = Color.white;

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            _hudBoxStyle = new GUIStyle(GUI.skin.box);

            PixelUIFont.Apply(_titleStyle);
            PixelUIFont.Apply(_subStyle);
            PixelUIFont.Apply(_buttonStyle);
            PixelUIFont.Apply(_hudBoxStyle);
        }

        private void OnGUI()
        {
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            InitStyles();
            _objectiveTypewriter.Tick(30f);

            if (_playerHealth == null)
            {
                FindPlayer();
            }

            DrawHUD();

            bool isWin = GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Win;
            bool isDead = _playerHealth != null && _playerHealth.IsDead;

            if (isWin)
            {
                DrawWinScreen();
            }
            else if (isDead)
            {
                DrawGameOverScreen();
            }
        }

        private void DrawHUD()
        {
            // Barra de vida (Canto Superior Esquerdo)
            float hp = _playerHealth != null ? _playerHealth.CurrentHealth : 100f;
            float maxHp = _playerHealth != null ? _playerHealth.MaxHealth : 100f;
            float hpPercent = Mathf.Clamp01(hp / maxHp);

            Rect playerPanel = new Rect(18, 18, 310, 94);
            PixelHUDFrame.Draw(playerPanel, _whiteTex, new Color(.10f, .04f, .06f, .96f), new Color(1f, .3f, .35f, .92f));
            GUI.Label(new Rect(playerPanel.x, playerPanel.y + 8, playerPanel.width, 22), "VIDA", _hudBoxStyle);

            int filledHearts = Mathf.Clamp(Mathf.CeilToInt(hpPercent * 3f), 0, 3);
            for (int heart = 0; heart < 3; heart++)
            {
                Color heartColor = heart < filledHearts ? new Color(.95f, .12f, .18f) : new Color(.24f, .06f, .08f);
                PixelHUDFrame.DrawHeart(new Rect(48 + heart * 82, 40, 56, 48), _whiteTex, heartColor);
            }

            var lifeStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            lifeStyle.normal.textColor = Color.white;
            PixelUIFont.Apply(lifeStyle);
            GUI.Label(new Rect(32, 90, 282, 18), $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}", lifeStyle);

            // Pontuação (Canto Superior Direito)
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            PixelHUDFrame.Draw(new Rect(Screen.width - 248, 18, 230, 60), _whiteTex, new Color(.10f, .08f, .025f, .96f), new Color(1f, .78f, .2f, .9f));
            var scoreStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            scoreStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
            PixelUIFont.Apply(scoreStyle);
            GUI.Label(new Rect(Screen.width - 248, 35, 230, 30), $"PONTOS {score}", scoreStyle);

            // Objetivo no topo centro
            PixelHUDFrame.Draw(new Rect(Screen.width / 2f - 260, 18, 520, 60), _whiteTex, new Color(.025f, .09f, .13f, .96f), new Color(.25f, .85f, 1f, .9f));
            var objStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            objStyle.normal.textColor = new Color(0.85f, 0.95f, 1f);
            PixelUIFont.Apply(objStyle);
            GUI.Label(new Rect(Screen.width / 2f - 250, 27, 500, 42), _objectiveTypewriter.VisibleText, objStyle);
        }

        private void DrawWinScreen()
        {
            Rect modal = GetModalRect(700f, 400f);
            float pad = Mathf.Clamp(modal.width * .06f, 12f, 38f);
            float buttonHeight = Mathf.Clamp(modal.height * .16f, 30f, 48f);
            _titleStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 30f), 10, 24);
            _subStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 16);
            _buttonStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 16);

            PixelHUDFrame.Draw(modal, _whiteTex, new Color(.025f, .12f, .08f, .98f), new Color(.28f, 1f, .55f, .9f));

            float titleHeight = _titleStyle.CalcHeight(new GUIContent("FASE CONCLUIDA"), modal.width - pad * 2f);
            GUI.Label(new Rect(modal.x + pad, modal.y + pad, modal.width - pad * 2f, titleHeight), "FASE CONCLUIDA", _titleStyle);
            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            float contentTop = modal.y + pad + titleHeight + pad * .6f;
            GUI.Label(new Rect(modal.x + pad, contentTop, modal.width - pad * 2f, modal.yMax - contentTop - buttonHeight - pad * 1.6f), $"PARABENS! VOCE ALCANCOU O FINAL DA FASE!\n\nPONTUACAO FINAL: {finalScore}", _subStyle);

            float buttonWidth = Mathf.Min(modal.width - pad * 2f, 300f);
            if (GUI.Button(new Rect(modal.x + (modal.width - buttonWidth) * .5f, modal.yMax - buttonHeight - pad * .55f, buttonWidth, buttonHeight), "JOGAR NOVAMENTE", _buttonStyle))
            {
                RestartLevel();
            }
        }

        private void DrawGameOverScreen()
        {
            Rect modal = GetModalRect(620f, 340f);
            float pad = Mathf.Clamp(modal.width * .06f, 12f, 38f);
            float buttonHeight = Mathf.Clamp(modal.height * .16f, 30f, 48f);
            _titleStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 30f), 10, 24);
            _subStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 16);
            _buttonStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(modal.width / 48f), 8, 16);

            PixelHUDFrame.Draw(modal, _whiteTex, new Color(.18f, .025f, .055f, .98f), new Color(1f, .22f, .35f, .9f));

            var deadStyle = new GUIStyle(_titleStyle);
            deadStyle.normal.textColor = Color.red;
            PixelUIFont.Apply(deadStyle);
            float titleHeight = deadStyle.CalcHeight(new GUIContent("GAME OVER"), modal.width - pad * 2f);
            GUI.Label(new Rect(modal.x + pad, modal.y + pad, modal.width - pad * 2f, titleHeight), "GAME OVER", deadStyle);
            float contentTop = modal.y + pad + titleHeight + pad * .6f;
            GUI.Label(new Rect(modal.x + pad, contentTop, modal.width - pad * 2f, modal.yMax - contentTop - buttonHeight - pad * 1.6f), "SUA VIDA CHEGOU A ZERO!", _subStyle);

            float buttonWidth = Mathf.Min(modal.width - pad * 2f, 300f);
            if (GUI.Button(new Rect(modal.x + (modal.width - buttonWidth) * .5f, modal.yMax - buttonHeight - pad * .55f, buttonWidth, buttonHeight), "TENTAR NOVAMENTE", _buttonStyle))
            {
                RestartLevel();
            }
        }

        private void RestartLevel()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
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
    }
}
