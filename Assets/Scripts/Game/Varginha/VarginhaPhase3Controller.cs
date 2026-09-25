using System.Collections;
using Game.Level;
using Game.Managers;
using Game.Player;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Varginha
{
    /// <summary>
    /// Ato III do GDD: Edelzio chega à área secreta da diocese, enfrenta os ETs
    /// e encontra Padre Fábio e o Livro do Tombo Secreto.
    /// </summary>
    public sealed class VarginhaPhase3Controller : MonoBehaviour
    {
        public static VarginhaPhase3Controller Active { get; private set; }

        private EdelzioTopDownController _player;
        private VarginhaStudentAllySquad _squad;
        private InteractableProp _padre;
        private InteractableProp _tome;
        private bool _arrivalFinished;
        private bool _combatCleared;
        private bool _phaseFinished;
        private float _lastNoAllyMessage;
        private GUIStyle _phaseStyle;
        private GUIStyle _allyStyle;

        public bool CombatCleared => _combatCleared;
        public bool IsComplete => _phaseFinished;
        public VarginhaStudentAllySquad Squad => _squad;

        private void Awake()
        {
            Active = this;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (Object.FindAnyObjectByType<EdelzioTopDownController>() == null)
                VarginhaPhase3RuntimeFactory.Build(transform);
            else
                VarginhaPhase3RuntimeFactory.EnsurePopulation(transform);

            _player = Object.FindAnyObjectByType<EdelzioTopDownController>();
            _padre = GameObject.Find("Padre_Fabio")?.GetComponent<InteractableProp>();
            _tome = GameObject.Find("Livro_Tombo_Secreto")?.GetComponent<InteractableProp>();
            _squad = Object.FindAnyObjectByType<VarginhaStudentAllySquad>();

            if (_squad == null && _player != null)
            {
                _squad = VarginhaStudentAllySquad.BuildForFuturePhase(transform, _player.transform, false);
                _squad.ActivateManualAllies(VarginhaStudentAlly.ManualCooldownSeconds, true);
            }
            else if (_squad != null)
            {
                _squad.Setup(_player != null ? _player.transform : null);
                _squad.ActivateManualAllies(VarginhaStudentAlly.ManualCooldownSeconds, true);
            }

            if (_padre != null) _padre.OnInteracted += HandlePadreInteracted;
            if (_tome != null) _tome.OnInteracted += HandleTomeInteracted;

            PrepareEdelzio();
            StartCoroutine(ArrivalCinematic());
        }

        private void OnDestroy()
        {
            if (_padre != null) _padre.OnInteracted -= HandlePadreInteracted;
            if (_tome != null) _tome.OnInteracted -= HandleTomeInteracted;
            if (Active == this) Active = null;
        }

        private void PrepareEdelzio()
        {
            if (_player == null) return;
            _player.HasBackpack = true;
            _player.HasFuscaKey = true;
            _player.HasResearchNotebook = true;
            _player.HasDecodedData = true;
            _player.HasHistoricalDocument = true;
            if (EdelzioTopDownController.PersistentHasFlashlight)
            {
                _player.HasFlashlight = true;
                _player.IsFlashlightEquippedInHotbar = EdelzioTopDownController.PersistentFlashlightInHotbar;
                _player.FlashlightActive = EdelzioTopDownController.PersistentFlashlightActive;
            }
            _player.EquipBackpack();
            _player.CompleteHouseInventory();

            // Comandos independentes: Edelzio sem recarga, alunos com 5s por aluno.
            var playerAttack = _player.GetComponent<VarginhaPlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.Configure(0f, true);
                playerAttack.enabled = true;
            }
            if (_player.GetComponent<EdelzioFlashlight>() == null)
                _player.gameObject.AddComponent<EdelzioFlashlight>();
            _player.SetInputLocked(true);
            _player.SetCombatLocked(true);
        }

        private IEnumerator ArrivalCinematic()
        {
            if (_player == null)
            {
                _arrivalFinished = true;
                yield break;
            }

            VarginhaGameHUD.Instance?.ShowDialogue(
                "Edelzio",
                "As coordenadas do notebook apontam para a área subterrânea da diocese.\n" +
                "Se o Livro do Tombo existir, ele pode explicar o que aconteceu em 1898.");
            yield return new WaitForSecondsRealtime(.85f);
            VarginhaGameHUD.Instance?.ShowDialogue(
                "Rodrigo",
                "Os ETs fecharam a passagem. Abra a mochila na hotbar ou com [G], entre na aba Alunos e equipe um colega. O comando especial chama somente esse aluno, com recarga de 5 segundos.");
            yield return new WaitForSecondsRealtime(.85f);

            _arrivalFinished = true;
            _player.SetCombatLocked(false);
            _player.SetInputLocked(false);
            VarginhaGameHUD.Instance?.CloseDialogue();
            VarginhaGameHUD.Instance?.ShowRodrigoHint("Rodrigo: 'Mochila [G], aba Alunos: escolher aliado. O especial usa o aluno equipado. Mire no ET, derrote as manifestações e examine o Padre Fábio.'");
        }

        private void Update()
        {
            if (!_arrivalFinished || _phaseFinished) return;

            if (!_combatCleared && AreAllEtDefeated())
            {
                _combatCleared = true;
                VarginhaGameHUD.Instance?.ShowDialogue(
                    "Padre Fábio",
                    "A passagem foi protegida por décadas. Agora venha, Edelzio: o Livro do Tombo Secreto guarda a verdade sobre Zé Gomes.");
                VarginhaGameHUD.Instance?.ShowRodrigoHint("Rodrigo: 'Fale com Padre Fábio e depois examine o Livro do Tombo Secreto.'");
            }

            if (!VarginhaInputBindings.WasPressedThisFrame(VarginhaInputAction.AllyCommand)) return;
            if (_phaseFinished || _player == null || _player.IsInputLocked || Time.timeScale <= 0f || VarginhaGameHUD.Instance?.IsDialogueOpen == true) return;

            var mouse = Mouse.current;
            Vector2? aim = mouse != null && VarginhaInputBindings.GetMouseButton(VarginhaInputAction.AllyCommand) >= 0
                && Camera.main != null ? (Vector2)Camera.main.ScreenToWorldPoint(mouse.position.ReadValue()) : (Vector2?)null;
            if (_squad != null && _squad.TryInvokeAttack(aim))
            {
                string student = _squad.LastInvokedStudentName ?? "um aluno";
                string attack = _squad.LastInvokedAttackDescription ?? VarginhaStudentAlly.DescribeAttack(student);
                VarginhaGameHUD.Instance?.ShowRodrigoHint($"Rodrigo: '{student}: {attack}! Aproveite a abertura para completar o combo.'");
            }
            else if (Time.unscaledTime - _lastNoAllyMessage > 1.25f)
            {
                _lastNoAllyMessage = Time.unscaledTime;
                VarginhaGameHUD.Instance?.ShowRodrigoHint("Rodrigo: 'Espere a turma preparar o comando, escolha um aluno pronto e mire em um ET próximo.'");
            }
        }

        private bool AreAllEtDefeated()
        {
            // Bug fix: usar Exclude para ignorar objetos inativos e evitar falsos positivos
            var enemies = Object.FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Exclude);
            if (enemies.Length == 0) return false;
            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                var target = enemy.GetComponent<VarginhaCombatTarget>();
                if (target != null && !target.IsDead) return false;
            }
            return true;
        }

        private void HandlePadreInteracted(EdelzioTopDownController edelzio)
        {
            if (!_combatCleared)
            {
                VarginhaGameHUD.Instance?.ShowDialogue("Padre Fábio", "As manifestações ainda estão bloqueando a passagem. Ajude meus alunos e derrote os ETs primeiro.");
                return;
            }

            VarginhaGameHUD.Instance?.ShowDialogue("Padre Fábio", "Seu nome está no Livro do Tombo desde 1898, Edelzio. Leia-o e descubra por que o selo escolheu você.");
        }

        private void HandleTomeInteracted(EdelzioTopDownController edelzio)
        {
            if (!_combatCleared)
            {
                VarginhaGameHUD.Instance?.ShowDialogue("Livro do Tombo", "A capa pulsa com energia. Os ETs ainda impedem que você abra o livro.");
                return;
            }

            CompletePhase();
        }

        private void CompletePhase()
        {
            if (_phaseFinished) return;
            _phaseFinished = true;
            _player?.SetInputLocked(true);
            _player?.SetCombatLocked(true);
            _squad?.DeactivateAllies();
            ScoreManager.Instance?.AddScore(1800);
            GameManager.Instance?.TriggerWin();
            VarginhaGameHUD.Instance?.ShowVictory(
                "FASE 3: O GUARDIÃO — CONCLUÍDA!",
                "Edelzio invocou a turma para abrir caminho, encontrou Padre Fábio e leu o Livro do Tombo Secreto.\n\n" +
                "A verdade sobre Zé Gomes e o selo de 1898 agora aponta para a mata e para Ouzana.");
        }

        private void OnGUI()
        {
            if (VarginhaGameHUD.Instance?.IsInventoryOpen == true) return;
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            if (!_arrivalFinished || _phaseFinished || _squad == null || VarginhaGameHUD.Instance?.IsDialogueOpen == true) return;
            InitGuiStyles();

            float width = Mathf.Min(430f, Screen.width - 32f);
            float height = Mathf.Min(118f, Screen.height - 140f);
            Rect panel = new Rect(Screen.width - width - 16f, 124f, width, height);

            // Painel visual consistente com o HUD principal
            bool allyReady = _squad.SelectedStudent?.IsReadyForManualAttack == true;
            float pulse = allyReady ? 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 4.8f) : 1f;
            Color panelBorder = allyReady
                ? new Color(.42f * pulse, .92f * pulse, .72f)
                : new Color(.32f, .46f, .54f);
            var whiteTex = VarginhaGameHUD.Instance != null
                ? Texture2D.whiteTexture
                : Texture2D.whiteTexture;
            PixelHUDFrame.Draw(panel, whiteTex, new Color(.015f, .035f, .06f, .94f), panelBorder);

            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, 22f), "FASE 3 \u2022 ALIADOS \u2022 " + VarginhaDifficulty.Label, _phaseStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 30f, panel.width - 24f, 18f),
                VarginhaInputBindings.DisplayName(VarginhaInputAction.Attack) + ": EDELZIO \u2022 "
                + VarginhaInputBindings.DisplayName(VarginhaInputAction.AllyCommand) + ": ALUNO \u2022 G: MOCHILA", _allyStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 48f, panel.width - 24f, 18f),
                VarginhaInputBindings.DisplayName(VarginhaInputAction.Dodge) + ": ESQUIVAR \u2022 Turma: "
                + (_squad.CommandCooldownRemaining > 0 ? "preparando comando" : "PRONTA"), _allyStyle);

            if (_squad.SelectedStudent == null)
            {
                GUI.Label(new Rect(panel.x + 12, panel.y + 76, panel.width - 24, 22), "Nenhum aluno equipado \u2022 escolha na aba Alunos", _allyStyle);
            }
            else
            {
                var ally = _squad.SelectedStudent;
                float x = panel.x + 12f;
                float y = panel.y + 73f;
                float cellWidth = panel.width - 24f;
                string state = ally.CurrentTarget != null ? "GOLPE" : ally.IsReadyForManualAttack ? "PRONTO" : $"{Mathf.CeilToInt(ally.ManualCooldownRemaining)}s";
                string name = ally.StudentName == "Luis Miguel Messias" ? "Luis Miguel" : ally.StudentName;
                name = "> " + name;
                GUI.color = ally.IsReadyForManualAttack ? new Color(.5f, 1f, .75f) : new Color(.8f, .84f, .9f);
                GUI.Label(new Rect(x, y, cellWidth, 18f), name, _allyStyle);
                GUI.Label(new Rect(x, y + 15f, cellWidth, 18f), state, _allyStyle);
                // Barra de cooldown usando GUI blocks
                GUI.color = new Color(.06f, .10f, .14f);
                GUI.DrawTexture(new Rect(x, y + 32f, cellWidth, 3f), Texture2D.whiteTexture);
                float cooldownFill = 1f - Mathf.Clamp01(ally.ManualCooldownRemaining / ally.ManualCooldownDuration);
                GUI.color = ally.IsReadyForManualAttack ? new Color(.4f, 1f, .68f) : new Color(.85f, .66f, .3f);
                GUI.DrawTexture(new Rect(x, y + 32f, cellWidth * cooldownFill, 3f), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }

        private void InitGuiStyles()
        {
            if (_phaseStyle != null) return;
            _phaseStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            _phaseStyle.normal.textColor = new Color(.45f, 1f, .88f);
            _allyStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = false };
            _allyStyle.normal.textColor = new Color(.88f, .95f, 1f);
            PixelUIFont.Apply(_phaseStyle);
            PixelUIFont.Apply(_allyStyle);
        }
    }

    /// <summary>Construtor runtime da igreja/diocese do Ato III.</summary>
    internal static class VarginhaPhase3RuntimeFactory
    {
        private static readonly Vector3[] EnemyPositions =
        {
            new(-1.6f, 2.8f), new(2.2f, 2.3f), new(4.8f, .1f), new(1.8f, -2.7f), new(-2.8f, -2.1f)
        };

        private static readonly Color[] AllyShirts =
        {
            new(.25f, .52f, .88f), new(.88f, .36f, .30f), new(.28f, .68f, .45f),
            new(.78f, .44f, .18f), new(.58f, .34f, .82f), new(.85f, .52f, .68f),
            new(.22f, .66f, .70f), new(.78f, .68f, .22f), new(.45f, .52f, .58f)
        };

        public static void Build(Transform root)
        {
            CreateManagers(root);
            var player = CreatePlayer(root, new Vector3(-5.4f, -3.3f));
            var camera = CreateCamera(root);
            camera.GetComponent<CameraFollow2D>().Target = player.transform;
            BuildChurch(root);
            CreateEnemyWave(root);
            CreatePadre(root);
            CreateTome(root);

            var squad = VarginhaStudentAllySquad.BuildForFuturePhase(root, player.transform, false);
            squad.ActivateManualAllies(VarginhaStudentAlly.ManualCooldownSeconds, true);
        }

        public static void EnsurePopulation(Transform root)
        {
            CreateManagers(root);
            BuildChurch(root);
            var player = Object.FindAnyObjectByType<EdelzioTopDownController>();
            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera == null) camera = CreateCamera(root).GetComponent<Camera>();
            var follow = camera.GetComponent<CameraFollow2D>() ?? camera.gameObject.AddComponent<CameraFollow2D>();
            if (player != null) follow.Target = player.transform;

            if (Object.FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Include).Length < EnemyPositions.Length)
                CreateEnemyWave(root);
            if (GameObject.Find("Padre_Fabio") == null) CreatePadre(root);
            if (GameObject.Find("Livro_Tombo_Secreto") == null) CreateTome(root);
            if (Object.FindAnyObjectByType<VarginhaStudentAllySquad>() == null && player != null)
            {
                var squad = VarginhaStudentAllySquad.BuildForFuturePhase(root, player.transform, false);
                squad.ActivateManualAllies(VarginhaStudentAlly.ManualCooldownSeconds, true);
            }
        }

        private static void CreateManagers(Transform root)
        {
            if (Object.FindAnyObjectByType<VarginhaGameHUD>() != null) return;
            var managers = new GameObject("GameManagers_Fase3");
            managers.transform.SetParent(root);
            if (GameManager.Instance == null) managers.AddComponent<GameManager>();
            if (ScoreManager.Instance == null) managers.AddComponent<ScoreManager>();
            if (LevelManager.Instance == null) managers.AddComponent<LevelManager>();
            managers.AddComponent<VarginhaGameHUD>();
            managers.AddComponent<VarginhaNotebookQuiz>();
        }

        private static GameObject CreateCamera(Transform root)
        {
            var go = new GameObject("Main Camera");
            go.transform.SetParent(root);
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 7.2f;
            // Atmosfera sombria e misteriosa de catacumba sob a diocese
            camera.backgroundColor = new Color(.008f, .006f, .014f);
            go.AddComponent<AudioListener>();
            go.AddComponent<CameraFollow2D>();
            VarginhaPixelPresentation.Configure(camera);
            return go;
        }

        private static GameObject CreatePlayer(Transform root, Vector3 position)
        {
            var go = new GameObject("Edelzio_Protagonist");
            go.tag = "Player";
            go.transform.SetParent(root);
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.08f, 1.08f, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Edelzio", new Color(.62f, .82f, .38f));
            renderer.sortingOrder = 5;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = .45f;
            var health = go.AddComponent<HealthSystem>();
            health.SetMaxHealth(100f, false);
            go.AddComponent<EdelzioTopDownController>();
            go.AddComponent<VarginhaPlayerSpriteAnimation>();
            go.AddComponent<VarginhaPlayerActionAnimation>();
            go.AddComponent<VarginhaPlayerAttack>();
            go.AddComponent<EdelzioFlashlight>();
            return go;
        }

        private static void BuildChurch(Transform root)
        {
            VarginhaEnvironmentArt.EnsureDiocese(root);
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            var renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Wall_Church", new Color(.24f, .24f, .30f));
            renderer.sortingOrder = 3;
            wall.AddComponent<BoxCollider2D>();
        }

        private static void CreateEnemyWave(Transform root)
        {
            foreach (var existing in Object.FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Include))
                if (existing != null) Object.Destroy(existing.gameObject);

            for (int i = 0; i < EnemyPositions.Length; i++)
            {
                var go = new GameObject("ET_Guardiao_" + (i + 1));
                go.transform.SetParent(root);
                go.transform.position = EnemyPositions[i];
                go.transform.localScale = new Vector3(1.28f, 1.28f, 1f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = VarginhaPixelArtSprites.Create("ET_Subordinate_Guardiao_" + i, new Color(.68f, .36f, .18f));
                renderer.sortingOrder = 5;
                var body = go.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                var collider = go.AddComponent<CircleCollider2D>();
                collider.radius = .42f;
                var health = go.AddComponent<HealthSystem>();
                health.SetMaxHealth(42f, false);
                var target = go.AddComponent<VarginhaCombatTarget>();
                target.SetKind(VarginhaCombatTarget.EnemyKind.MinorManifestation);
                go.AddComponent<VarginhaCombatEnemy>();
            }
        }

        private static void CreatePadre(Transform root)
        {
            var padre = new GameObject("Padre_Fabio");
            padre.transform.SetParent(root);
            padre.transform.position = new Vector3(6.2f, 2.7f);
            padre.transform.localScale = Vector3.one * 1.12f;
            var renderer = padre.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Padre_Fabio", new Color(.52f, .40f, .28f));
            renderer.sortingOrder = 8;
            var collider = padre.AddComponent<CircleCollider2D>();
            collider.radius = .55f;
            collider.isTrigger = true;
            padre.AddComponent<InteractableProp>().Configure(
                PropType.PadreFabio,
                "Padre Fábio",
                "Eu protejo os registros da diocese. Derrote os invasores e depois leia o Livro do Tombo Secreto.",
                true);
        }

        private static void CreateTome(Transform root)
        {
            var tome = new GameObject("Livro_Tombo_Secreto");
            tome.transform.SetParent(root);
            tome.transform.position = new Vector3(5.3f, .2f);
            tome.transform.localScale = Vector3.one * .72f;
            var renderer = tome.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Tome_Prop", new Color(.48f, .22f, .12f));
            renderer.sortingOrder = 8;
            var collider = tome.AddComponent<CircleCollider2D>();
            collider.radius = .62f;
            collider.isTrigger = true;
            tome.AddComponent<InteractableProp>().Configure(
                PropType.SecretTome,
                "Livro do Tombo Secreto",
                "As páginas citam Zé Gomes, 1898, a ruptura subterrânea e o nome de Edelzio como parte do selo.",
                true);
        }
    }
}
