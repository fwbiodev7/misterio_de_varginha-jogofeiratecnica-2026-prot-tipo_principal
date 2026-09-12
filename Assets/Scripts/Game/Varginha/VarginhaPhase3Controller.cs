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
                _squad.ActivateManualAllies();
            }
            else if (_squad != null)
            {
                _squad.Setup(_player != null ? _player.transform : null);
                _squad.ActivateManualAllies();
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
            _player.EquipBackpack();
            _player.EquipNotebook();

            // Comandos independentes: Edelzio sem recarga, alunos com 5s por aluno.
            var playerAttack = _player.GetComponent<VarginhaPlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.Configure(0f, true);
                playerAttack.enabled = true;
            }
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
                "Os ETs fecharam a passagem. Segure o clique esquerdo para Edelzio atacar sem recarga. Clique direito perto de um ET para comandar um aluno: cada aluno recarrega por 5 segundos. A turma escolhe a especialidade mais útil para a situação.");
            yield return new WaitForSecondsRealtime(.85f);

            _arrivalFinished = true;
            _player.SetCombatLocked(false);
            _player.SetInputLocked(false);
            VarginhaGameHUD.Instance?.CloseDialogue();
            VarginhaGameHUD.Instance?.ShowRodrigoHint("Rodrigo: 'Esquerdo: Edelzio. Direito: aluno, mirando no ET. Sem alvo na mira, a turma protege você. Derrote os ETs e examine o Padre Fábio.'");
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

            var mouse = Mouse.current;
            if (mouse == null || !mouse.rightButton.wasPressedThisFrame) return;
            if (_phaseFinished || _player == null || _player.IsInputLocked || Time.timeScale <= 0f || VarginhaGameHUD.Instance?.IsDialogueOpen == true) return;

            Vector2? aim = Camera.main != null ? (Vector2)Camera.main.ScreenToWorldPoint(mouse.position.ReadValue()) : (Vector2?)null;
            if (_squad != null && _squad.TryInvokeAttack(aim))
            {
                string student = _squad.LastInvokedStudentName ?? "um aluno";
                VarginhaGameHUD.Instance?.ShowRodrigoHint($"Rodrigo: '{student} atacou. Cada aluno precisa de 5 segundos para o próximo golpe.'");
            }
            else if (Time.unscaledTime - _lastNoAllyMessage > 1.25f)
            {
                _lastNoAllyMessage = Time.unscaledTime;
                VarginhaGameHUD.Instance?.ShowRodrigoHint("Rodrigo: 'Espere a turma preparar o comando, escolha um aluno pronto e mire em um ET próximo.'");
            }
        }

        private bool AreAllEtDefeated()
        {
            var enemies = Object.FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Include);
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
            if (Game.Varginha.VarginhaTravelCinematic.IsTravelling) return;
            if (!_arrivalFinished || _phaseFinished || _squad == null) return;
            InitGuiStyles();

            float width = Mathf.Min(430f, Screen.width - 32f);
            float height = 150f;
            Rect panel = new Rect(Screen.width - width - 16f, 124f, width, height);
            GUI.color = new Color(.015f, .035f, .06f, .94f);
            GUI.Box(panel, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, 22f), "FASE 3 • ALIADOS • " + VarginhaDifficulty.Label, _phaseStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 30f, panel.width - 24f, 18f), "ESQUERDO: EDELZIO • DIREITO: ALUNO (5s por aluno)", _allyStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 48f, panel.width - 24f, 18f), "Edelzio: sem recarga • Turma: " + (_squad.CommandCooldownRemaining > 0 ? "preparando comando" : "PRONTA"), _allyStyle);

            for (int i = 0; i < _squad.Allies.Count; i++)
            {
                var ally = _squad.Allies[i];
                if (ally == null) continue;
                int column = i % 3;
                int row = i / 3;
                float x = panel.x + 12f + column * (panel.width - 24f) / 3f;
                float y = panel.y + 73f + row * 20f;
                string state = ally.IsReadyForManualAttack ? "PRONTO" : $"{Mathf.CeilToInt(ally.ManualCooldownRemaining)}s";
                GUI.Label(new Rect(x, y, (panel.width - 24f) / 3f - 4f, 18f), $"{ally.StudentName}: {state}", _allyStyle);
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
            squad.ActivateManualAllies();
        }

        public static void EnsurePopulation(Transform root)
        {
            CreateManagers(root);
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
                squad.ActivateManualAllies();
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
            camera.backgroundColor = new Color(.025f, .018f, .04f);
            go.AddComponent<AudioListener>();
            go.AddComponent<CameraFollow2D>();
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
            return go;
        }

        private static void BuildChurch(Transform root)
        {
            var church = new GameObject("Igreja_Diocese_Ato_III").transform;
            church.SetParent(root);
            Color floor = new(.18f, .20f, .24f);
            for (int y = -6; y <= 6; y++)
            for (int x = -8; x <= 9; x++)
            {
                var tile = new GameObject("Piso_Igreja_" + x + "_" + y);
                tile.transform.SetParent(church);
                tile.transform.position = new Vector3(x + .5f, y + .5f, 0f);
                var renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = VarginhaPixelArtSprites.Create("ChurchFloor", ((x + y) & 1) == 0 ? floor : Color.Lerp(floor, Color.white, .05f));
                renderer.sortingOrder = 0;
            }

            CreateWall(church, "Parede_Norte_Igreja", new Vector3(.5f, 6.8f), new Vector3(18f, .7f, 1f));
            CreateWall(church, "Parede_Sul_Igreja", new Vector3(.5f, -6.8f), new Vector3(18f, .7f, 1f));
            CreateWall(church, "Parede_Oeste_Igreja", new Vector3(-8.8f, 0f), new Vector3(.7f, 13f, 1f));
            CreateWall(church, "Parede_Leste_Igreja", new Vector3(9.8f, 0f), new Vector3(.7f, 13f, 1f));

            var altar = new GameObject("Altar_Da_Diocese");
            altar.transform.SetParent(church);
            altar.transform.position = new Vector3(5.3f, .2f);
            altar.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            var altarRenderer = altar.AddComponent<SpriteRenderer>();
            altarRenderer.sprite = VarginhaPixelArtSprites.Create("ChurchAltar", new Color(.40f, .22f, .12f));
            altarRenderer.sortingOrder = 3;

            var seal = new GameObject("Simbolo_Do_Selo");
            seal.transform.SetParent(church);
            seal.transform.position = new Vector3(-2.3f, .1f);
            seal.transform.localScale = Vector3.one * 1.15f;
            var sealRenderer = seal.AddComponent<SpriteRenderer>();
            sealRenderer.sprite = VarginhaPixelArtSprites.Create("SealSymbol", new Color(.25f, .75f, .82f));
            sealRenderer.sortingOrder = 2;
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
                renderer.sprite = VarginhaPixelArtSprites.Create("ET_Subordinate_Guardiao_" + i, new Color(.25f, .35f, .48f));
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
