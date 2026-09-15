using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Level;
using Game.Managers;
using Game.Player;

namespace Game.Varginha
{
    /// <summary>Fluxo simples da Fase 2: chegada, combate aos subordinados e resgate da turma.</summary>
    public sealed class VarginhaPhase2Controller : MonoBehaviour
    {
        public static VarginhaPhase2Controller Active { get; private set; }

        public static readonly string[] StudentNames =
        {
            "Yasmin", "Pedro", "Matias", "Fabio", "Marcos",
            "Anna Sabia", "Ana Tavares", "Luis Miguel Messias", "Luis Martins"
        };

        private EdelzioTopDownController _player;
        private Transform _fusca;
        private FuscaDepartureAnimation _fuscaDeparture;
        private SpriteRenderer _fuscaRenderer;
        private VarginhaStudentHostage[] _students = new VarginhaStudentHostage[0];
        private bool _arrivalFinished;
        private bool _rescueStarted;
        private bool _departureStarted;
        private bool _encounterReady;
        private int _studentsAtFusca;
        private readonly HashSet<VarginhaStudentHostage> _arrivedStudents = new();

        public bool RescueStarted => _rescueStarted;
        public bool IsComplete => _departureStarted;
        public int StudentCount => _students.Length;
        public int StudentsAtFusca => _studentsAtFusca;

        private void Awake()
        {
            Active = this;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (FindAnyObjectByType<EdelzioTopDownController>() == null)
                VarginhaPhase2RuntimeFactory.Build(transform);
            else
                VarginhaPhase2RuntimeFactory.EnsurePopulation(transform);
            _player = FindAnyObjectByType<EdelzioTopDownController>();
            var car = GameObject.Find("Fusca_1996_Fase2");
            if (car != null)
            {
                _fusca = car.transform;
                _fuscaDeparture = car.GetComponent<FuscaDepartureAnimation>();
                _fuscaRenderer = car.GetComponent<SpriteRenderer>();
            }

            _students = FindObjectsByType<VarginhaStudentHostage>(FindObjectsInactive.Include);
            _encounterReady = _students.Length >= StudentNames.Length &&
                              FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Include).Length >= 4;
            PrepareEdelzioForPhase2();
            StartCoroutine(ArrivalCinematic());
        }

        private void OnDestroy()
        {
            if (Active == this) Active = null;
        }

        private void PrepareEdelzioForPhase2()
        {
            if (_player == null) return;
            // Tudo que foi levado no Fusca na Fase 1 permanece disponível na investigação.
            _player.HasBackpack = true;
            _player.HasFuscaKey = true;
            _player.HasResearchNotebook = true;
            _player.HasDecodedData = true;
            _player.HasHistoricalDocument = true;
            _player.EquipBackpack();
            _player.CompleteHouseInventory();
            _player.SetInputLocked(true);
            _player.SetCombatLocked(true);
        }

        private IEnumerator ArrivalCinematic()
        {
            if (_player == null || _fusca == null)
            {
                _arrivalFinished = true;
                yield break;
            }

            var playerRenderer = _player.GetComponent<SpriteRenderer>();
            var playerAnimation = _player.GetComponent<VarginhaPlayerSpriteAnimation>();
            var arrivalBody = _player.GetComponent<Rigidbody2D>();
            if (arrivalBody != null) arrivalBody.simulated = false;
            if (playerAnimation != null) playerAnimation.enabled = false;
            _player.SetCarriedItemsVisible(false);
            Vector3 parking = _fusca.position;
            _fusca.position = parking + Vector3.left * 12f;
            if (_fuscaRenderer != null) _fuscaRenderer.enabled = true;
            _player.transform.position = _fusca.position + Vector3.left * .22f;
            if (playerRenderer != null) playerRenderer.sortingOrder = 3;

            VarginhaGameHUD.Instance?.ShowDialogue("Edelzio", "A escola está cercada... ainda bem que trouxe o Fusca e tudo o que encontrei em casa.");
            yield return new WaitForSecondsRealtime(.35f);

            float elapsed = 0f;
            while (elapsed < 1.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                _fusca.position = Vector3.Lerp(parking + Vector3.left * 12f, parking, 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / 1.2f), 3f));
                _player.transform.position = _fusca.position;
                yield return null;
            }

            VarginhaGameHUD.Instance?.ShowDialogue("Edelzio", "Subordinados ETs enjaularam meus alunos e estão atacando. Vou quebrar essas jaulas e tirá-los daqui!");
            if (playerAnimation != null) playerAnimation.enabled = true;
            Vector3 exit = _fusca.position + Vector3.right * 1.05f;
            Vector3 start = _player.transform.position;
            elapsed = 0f;
            _player.IsScriptedMotion = true;
            while (elapsed < .55f)
            {
                elapsed += Time.unscaledDeltaTime;
                _player.transform.position = Vector3.Lerp(start, exit, Mathf.SmoothStep(0f, 1f, elapsed / .55f));
                yield return null;
            }

            _player.IsScriptedMotion = false;
            // Só mostra mochila e notebook depois que Edelzio saiu da cabine;
            // durante a tomada eles não podem cobrir o carro.
            _player.SetCarriedItemsVisible(true);
            if (arrivalBody != null) arrivalBody.simulated = true;
            if (playerRenderer != null) playerRenderer.sortingOrder = 5;
            _player.SetCombatLocked(false);
            _player.SetInputLocked(false);
            _arrivalFinished = true;
            VarginhaGameHUD.Instance?.CloseDialogue();
            VarginhaGameHUD.Instance?.ShowRodrigoHint("Rodrigo: 'Cuidado com as rajadas verdes dos ETs. Derrote-os e depois leve os nove alunos, ainda enjaulados, até o Fusca.'");
        }

        private void Update()
        {
            if (_departureStarted) return;
            if (!_encounterReady)
            {
                // Auto-reparo para uma execução que ficou viva enquanto os scripts
                // recompilavam ou para uma cena antiga que criou somente a Yasmin.
                VarginhaPhase2RuntimeFactory.EnsurePopulation(transform);
                _students = FindObjectsByType<VarginhaStudentHostage>(FindObjectsInactive.Include);
                _encounterReady = _students.Length >= StudentNames.Length &&
                                  FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Include).Length >= 4;
            }
            if (!_arrivalFinished || !_encounterReady || _rescueStarted) return;
            var enemies = FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Include);
            if (enemies.Length < 4) return;
            bool allDefeated = true;
            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                var target = enemy.GetComponent<VarginhaCombatTarget>();
                if (target != null && !target.IsDead)
                {
                    allDefeated = false;
                    break;
                }
            }

            if (allDefeated) BeginRescue();
        }

        private void BeginRescue()
        {
            if (_rescueStarted || _students.Length < StudentNames.Length || _fusca == null) return;
            _rescueStarted = true;
            VarginhaGameHUD.Instance?.ShowDialogue("Edelzio", "Consegui! A escola está livre. Todos para o Fusca, rápido!");
            VarginhaGameHUD.Instance?.ShowRodrigoHint("Rodrigo: 'Os alunos estão livres. Fique perto deles e conduza a turma até o Fusca.'");
            for (int i = 0; i < _students.Length; i++)
                _students[i].ReleaseTo(_fusca, i, _player != null ? _player.transform : null);
        }

        public static void NotifyStudentAtFusca(VarginhaStudentHostage student)
        {
            if (Active == null || student == null || !Active._arrivedStudents.Add(student)) return;
            Active._studentsAtFusca = Active._arrivedStudents.Count;
        }

        private void LateUpdate()
        {
            if (!_rescueStarted || _departureStarted || _fusca == null || _player == null) return;
            if (_studentsAtFusca < _students.Length) return;
            if (Vector2.Distance(_player.transform.position, _fusca.position) > 3.4f) return;
            _departureStarted = true;
            StartCoroutine(DepartureRoutine());
        }

        private IEnumerator DepartureRoutine()
        {
            _player.SetInputLocked(true);
            _player.SetCombatLocked(true);
            _player.IsScriptedMotion = true;
            Vector3 start = _player.transform.position;
            Vector3 seat = _fusca.position + Vector3.left * .10f;
            float elapsed = 0f;
            while (elapsed < .5f)
            {
                elapsed += Time.deltaTime;
                _player.transform.position = Vector3.Lerp(start, seat, Mathf.SmoothStep(0f, 1f, elapsed / .5f));
                yield return null;
            }

            foreach (var student in _students)
            {
                var renderer = student.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            var playerRenderer = _player.GetComponent<SpriteRenderer>();
            if (playerRenderer != null) playerRenderer.enabled = false;
            var spriteAnimation = _player.GetComponent<VarginhaPlayerSpriteAnimation>();
            if (spriteAnimation != null) spriteAnimation.enabled = false;
            _player.SetCarriedItemsVisible(false);
            _player.transform.SetParent(_fusca, true);
            _player.IsScriptedMotion = false;

            bool finished = false;
            if (_fuscaDeparture != null)
            {
                // A cinematics de viagem continua o trajeto; não deixe o carro
                // percorrer 15 unidades e desaparecer da fase antes do fade.
                _fuscaDeparture.Depart(() => finished = true, 4.2f);
                yield return new WaitUntil(() => finished);
            }
            else yield return new WaitForSeconds(.8f);

            ScoreManager.Instance?.AddScore(1500);
            VarginhaTravelCinematic.Begin(true);
        }
    }

    /// <summary>Fábrica mínima usada pela cena serializada; mantém a Fase 2 jogável em qualquer build.</summary>
    internal static class VarginhaPhase2RuntimeFactory
    {
        private static readonly Vector3[] StudentPositions =
        {
            // Espaçamento maior para a turma continuar legível mesmo com a jaula sobreposta.
            new(-2.2f, 1.8f, 0f), new(0f, 1.8f, 0f), new(2.2f, 1.8f, 0f),
            new(-2.2f, 0f, 0f), new(0f, 0f, 0f), new(2.2f, 0f, 0f),
            new(-2.2f, -1.8f, 0f), new(0f, -1.8f, 0f), new(2.2f, -1.8f, 0f)
        };

        private static readonly Color[] StudentShirts =
        {
            new(.25f, .52f, .88f), new(.88f, .36f, .30f), new(.28f, .68f, .45f),
            new(.78f, .44f, .18f), new(.58f, .34f, .82f), new(.85f, .52f, .68f),
            new(.22f, .66f, .70f), new(.78f, .68f, .22f), new(.45f, .52f, .58f)
        };

        public static void Build(Transform root)
        {
            CreateManagers(root);
            var camera = CreateCamera(root);
            var player = CreatePlayer(root, new Vector3(-6.1f, -1.2f));
            camera.GetComponent<CameraFollow2D>().Target = player.transform;
            BuildSchool(root);
            CreateFusca(root, VarginhaEnvironmentArt.FuscaParkingPosition);
            CreateSubordinate(root, "ET_Subordinado_1", new Vector3(2.8f, 2.6f));
            CreateSubordinate(root, "ET_Subordinado_2", new Vector3(5.1f, -2.4f));
            CreateSubordinate(root, "ET_Subordinado_3", new Vector3(-.3f, 3.1f));
            CreateSubordinate(root, "ET_Subordinado_4", new Vector3(6.6f, 2.9f));
            var students = new GameObject("Refens_3_Sistema").transform;
            students.SetParent(root);
            for (int i = 0; i < VarginhaPhase2Controller.StudentNames.Length; i++)
                CreateStudent(students, VarginhaPhase2Controller.StudentNames[i], StudentPositions[i], StudentShirts[i]);
        }

        /// <summary>
        /// Repara cenas da Fase 2 que foram salvas com somente parte dos objetos runtime.
        /// O método é idempotente: reutiliza o que já existe e cria apenas o que falta.
        /// </summary>
        public static void EnsurePopulation(Transform root)
        {
            CreateManagers(root);
            BuildSchool(root);

            var player = Object.FindAnyObjectByType<EdelzioTopDownController>();
            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera == null)
                camera = CreateCamera(root).GetComponent<Camera>();
            var follow = camera.GetComponent<CameraFollow2D>() ?? camera.gameObject.AddComponent<CameraFollow2D>();
            if (player != null) follow.Target = player.transform;

            var existingCar = GameObject.Find("Fusca_1996_Fase2");
            if (existingCar == null)
                CreateFusca(root, VarginhaEnvironmentArt.FuscaParkingPosition);
            else
                // Corrige cenas antigas que ainda tinham o Fusca na posição anterior.
                existingCar.transform.position = VarginhaEnvironmentArt.FuscaParkingPosition;

            var enemyPositions = new[]
            {
                new Vector3(2.8f, 2.6f), new Vector3(5.1f, -2.4f),
                new Vector3(-.3f, 3.1f), new Vector3(6.6f, 2.9f)
            };
            int enemyCount = Object.FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Include).Length;
            for (int i = enemyCount; i < enemyPositions.Length; i++)
                CreateSubordinate(root, "ET_Subordinado_" + (i + 1), enemyPositions[i]);

            var parentObject = GameObject.Find("Refens_3_Sistema");
            var studentsParent = parentObject != null ? parentObject.transform : new GameObject("Refens_3_Sistema").transform;
            if (parentObject == null) studentsParent.SetParent(root);

            var existing = Object.FindObjectsByType<VarginhaStudentHostage>(FindObjectsInactive.Include);
            var existingNames = new System.Collections.Generic.HashSet<string>();
            foreach (var student in existing)
            {
                if (student == null) continue;
                string key = student.StudentName;
                if (string.IsNullOrWhiteSpace(key) || key == "Aluno")
                    key = student.gameObject.name.Replace("Refem_", string.Empty).Replace("_", " ");
                int rosterIndex = -1;
                for (int i = 0; i < VarginhaPhase2Controller.StudentNames.Length; i++)
                {
                    if (VarginhaPhase2Controller.StudentNames[i] == key) { rosterIndex = i; break; }
                }
                if (student.StudentName == "Aluno" && rosterIndex >= 0)
                    student.Configure(key, StudentShirts[rosterIndex]);
                existingNames.Add(key);
                student.EnsurePresentation(8 + Mathf.Clamp(rosterIndex, 0, 8));
                if (!student.IsReleased)
                {
                    student.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                    if (rosterIndex >= 0) student.transform.position = StudentPositions[rosterIndex];
                }
            }

            for (int i = 0; i < VarginhaPhase2Controller.StudentNames.Length; i++)
            {
                string name = VarginhaPhase2Controller.StudentNames[i];
                if (!existingNames.Contains(name))
                {
                    CreateStudent(studentsParent, name, StudentPositions[i], StudentShirts[i]);
                    var created = studentsParent.Find("Refem_" + name.Replace(" ", "_"));
                    created?.GetComponent<VarginhaStudentHostage>()?.EnsurePresentation(8 + i);
                }
            }
        }

        private static void CreateManagers(Transform root)
        {
            if (Object.FindAnyObjectByType<VarginhaGameHUD>() != null) return;
            var managers = new GameObject("GameManagers");
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
            camera.backgroundColor = new Color(.025f, .035f, .06f);
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
            return go;
        }

        private static void BuildSchool(Transform root)
        {
            VarginhaEnvironmentArt.EnsureSchool(root);
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Wall_" + name, color);
            renderer.sortingOrder = 3;
            go.AddComponent<BoxCollider2D>();
        }

        private static void CreateSubordinate(Transform root, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("ET_Subordinate_" + name, new Color(.68f, .36f, .18f));
            renderer.sortingOrder = 5;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = .42f;
            var health = go.AddComponent<HealthSystem>();
            health.SetMaxHealth(35f, false);
            var target = go.AddComponent<VarginhaCombatTarget>();
            target.SetKind(VarginhaCombatTarget.EnemyKind.MinorManifestation);
            go.AddComponent<VarginhaCombatEnemy>();
        }

        private static void CreateStudent(Transform parent, string name, Vector3 position, Color shirt)
        {
            var go = new GameObject("Refem_" + name.Replace(" ", "_"));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Student_" + name, shirt);
            renderer.sortingOrder = 8;
            go.AddComponent<VarginhaStudentHostage>().Configure(name, shirt);
        }

        private static void CreateFusca(Transform root, Vector3 position)
        {
            var go = new GameObject("Fusca_1996_Fase2");
            go.transform.SetParent(root);
            go.transform.position = position;
            go.transform.localScale = new Vector3(2.6f, 2.6f, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            var texture = Resources.Load<Texture2D>("Varginha/fusca-sprite-sheet");
            var frames = new Sprite[3];
            if (texture != null)
            {
                int width = texture.width / frames.Length;
                for (int i = 0; i < frames.Length; i++)
                    frames[i] = Sprite.Create(texture, new Rect(i * width, 0, width, texture.height), new Vector2(.5f, .5f), texture.height);
            }
            else
            {
                frames[0] = VarginhaPixelArtSprites.Create("Fusca_Fallback", new Color(.2f, .6f, .85f));
                frames[1] = frames[0]; frames[2] = frames[0];
            }
            renderer.sprite = frames[0];
            renderer.sortingOrder = 4;
            go.AddComponent<FuscaDepartureAnimation>().Configure(frames);
            go.AddComponent<AudioSource>();
            go.AddComponent<FuscaEngineSound>();
        }
    }
}
