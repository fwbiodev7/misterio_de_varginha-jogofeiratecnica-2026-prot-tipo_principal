using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Player;
using Game.Enemies;
using Game.Managers;
using Game.Level;
using Game.UI;

namespace Game.Editor.Testing
{
    /// <summary>
    /// Constrói uma fase completa e jogável com:
    /// - Player configurado (controles, vida, colisão, física)
    /// - Inimigos com IA de patrulha e perseguição
    /// - Plataformas, buracos e obstáculos (espinhos/abismo)
    /// - Moedas e itens coletáveis
    /// - Bandeira de chegada para finalizar a fase
    /// - Câmera inteligente que segue o jogador
    /// - HUD em tempo real com barra de vida, pontuação e telas de vitória/derrota.
    /// </summary>
    public static class LevelBuilderTool
    {
        [MenuItem("Tools/Game Testing/Construir Fase Completa (Na Cena Aberta)", false, 1)]
        public static void BuildInActiveScene()
        {
            if (EditorUtility.DisplayDialog("Construir Fase Completa",
                "Isso irá gerar a fase completa (Player, Inimigos, Plataformas, Coletáveis, Câmera e Chegada) na cena atual.\n\nDeseja continuar?",
                "Sim, Construir", "Cancelar"))
            {
                BuildLevelHierarchy();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("✅ [LevelBuilder] Fase completa construída com sucesso na cena aberta!");
                EditorUtility.DisplayDialog("Fase Pronta!", "A fase completa foi gerada!\n\nPressione PLAY no Unity para jogar com WASD / Setas e Barra de Espaço!", "OK");
            }
        }

        [MenuItem("Tools/Game Testing/Gerar Cenas Salvas (Fase1 e FaseCompleta)", false, 2)]
        public static void BuildAndSaveScenes()
        {
            BuildAndSaveScene("Assets/Scenes/FaseCompleta.unity");
            BuildAndSaveScene("Assets/Scenes/Fase1_deTestes.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ [LevelBuilder] Cenas 'FaseCompleta.unity' e 'Fase1_deTestes.unity' geradas e salvas!");
            EditorUtility.DisplayDialog("Cenas Salvas", "Cenas 'FaseCompleta.unity' e 'Fase1_deTestes.unity' foram criadas e salvas com sucesso!", "OK");
        }

        public static void BuildAndSaveScene(string scenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildLevelHierarchy();
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        public static void BuildLevelHierarchy()
        {
            // Limpa objetos anteriores com tag ou nome de teste
            var existingRoot = GameObject.Find("[FASE_COMPLETA]");
            if (existingRoot != null)
            {
                Object.DestroyImmediate(existingRoot);
            }

            var root = new GameObject("[FASE_COMPLETA]");

            // 1. Managers & HUD
            CreateManagers(root.transform);

            // 2. Câmera
            var camera = CreateCamera(root.transform);

            // 3. Player
            var player = CreatePlayer(root.transform, new Vector3(0f, 1f, 0f));

            // Conecta câmera ao player
            var camFollow = camera.GetComponent<CameraFollow2D>();
            if (camFollow != null)
            {
                camFollow.Target = player.transform;
            }

            // 4. Cenário (Chão, Plataformas, Espinhos)
            var envRoot = new GameObject("Environment").transform;
            envRoot.SetParent(root.transform);

            CreateGroundAndPlatforms(envRoot);

            // 5. Inimigos
            var enemiesRoot = new GameObject("Enemies").transform;
            enemiesRoot.SetParent(root.transform);

            CreateEnemies(enemiesRoot);

            // 6. Coletáveis (Moedas e Estrelas)
            var collRoot = new GameObject("Collectibles").transform;
            collRoot.SetParent(root.transform);

            CreateCollectibles(collRoot);

            // 7. Chegada / Final da Fase
            CreateGoal(root.transform, new Vector3(70f, 0.8f, 0f));

            // 8. Zona da Morte (Abismo)
            CreateDeathZone(root.transform, new Vector3(35f, -8f, 0f), 120f);

            Selection.activeGameObject = player;
        }

        private static void CreateManagers(Transform parent)
        {
            var existing = Object.FindAnyObjectByType<GameManager>();
            if (existing != null) return;

            var go = new GameObject("GameManagers");
            go.transform.SetParent(parent);
            go.AddComponent<GameManager>();
            go.AddComponent<ScoreManager>();
            go.AddComponent<LevelManager>();
            go.AddComponent<GameHUD>();
        }

        private static GameObject CreateCamera(Transform parent)
        {
            Camera existingCam = Camera.main;
            GameObject camGo;
            if (existingCam != null)
            {
                camGo = existingCam.gameObject;
            }
            else
            {
                camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.orthographic = true;
                cam.orthographicSize = 7f;
                cam.backgroundColor = new Color(0.08f, 0.11f, 0.18f);
                camGo.AddComponent<AudioListener>();
            }

            camGo.transform.SetParent(parent);
            camGo.transform.position = new Vector3(0f, 2f, -10f);

            if (camGo.GetComponent<CameraFollow2D>() == null)
            {
                camGo.AddComponent<CameraFollow2D>();
            }

            return camGo;
        }

        private static GameObject CreatePlayer(Transform parent, Vector3 position)
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.transform.SetParent(parent);
            go.transform.position = position;

            // Visual
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetBoxSprite();
            sr.color = new Color(0.2f, 0.85f, 1f); // Ciano vibrante
            sr.sortingOrder = 5;

            // Física
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3.5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.8f, 1.8f);

            // GroundCheck
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(go.transform);
            gc.transform.localPosition = new Vector3(0f, -0.9f, 0f);

            // Scripts
            go.AddComponent<HealthSystem>();
            var pc = go.AddComponent<PlayerController>();

            // Configura campo groundCheck via SerializedObject
            var so = new SerializedObject(pc);
            var prop = so.FindProperty("groundCheck");
            if (prop != null)
            {
                prop.objectReferenceValue = gc.transform;
                so.ApplyModifiedProperties();
            }

            return go;
        }

        private static void CreateGroundAndPlatforms(Transform parent)
        {
            Color groundColor = new Color(0.18f, 0.22f, 0.32f);
            Color platformColor = new Color(0.25f, 0.35f, 0.5f);

            // 1. Chão Inicial (Start Zone)
            CreateBlock(parent, "Ground_Start", new Vector3(5f, -1f, 0f), new Vector3(16f, 1.2f, 1f), groundColor);

            // 2. Espinhos no primeiro vão (Buraco 1)
            CreateSpikes(parent, "Spikes_Pit1", new Vector3(16f, -1.8f, 0f), new Vector3(4f, 0.6f, 1f));

            // 3. Plataforma sobre o buraco
            CreateBlock(parent, "Platform_Jump1", new Vector3(16f, 1.2f, 0f), new Vector3(3.5f, 0.6f, 1f), platformColor);

            // 4. Chão do Primeiro Inimigo
            CreateBlock(parent, "Ground_Zone2", new Vector3(26f, -1f, 0f), new Vector3(14f, 1.2f, 1f), groundColor);

            // 5. Escadaria de Plataformas
            CreateBlock(parent, "Platform_Stairs1", new Vector3(36f, 1.0f, 0f), new Vector3(3.5f, 0.6f, 1f), platformColor);
            CreateBlock(parent, "Platform_Stairs2", new Vector3(41f, 3.0f, 0f), new Vector3(3.5f, 0.6f, 1f), platformColor);

            // Plataforma secreta alta com estrela
            CreateBlock(parent, "Platform_SecretHigh", new Vector3(36f, 5.2f, 0f), new Vector3(3f, 0.6f, 1f), new Color(0.4f, 0.25f, 0.55f));

            // 6. Plataforma Elevada do Segundo Inimigo (Guarda)
            CreateBlock(parent, "Platform_Zone3_Enemy2", new Vector3(47f, 4.5f, 0f), new Vector3(7f, 0.6f, 1f), platformColor);

            // 7. Descida e Ponte Suspensa
            CreateBlock(parent, "Platform_Drop1", new Vector3(53f, 2.5f, 0f), new Vector3(3.5f, 0.6f, 1f), platformColor);
            CreateBlock(parent, "Platform_Drop2", new Vector3(58f, 0.5f, 0f), new Vector3(3.5f, 0.6f, 1f), platformColor);

            // 8. Chão Final da Vitória
            CreateBlock(parent, "Ground_VictoryZone", new Vector3(67f, -1f, 0f), new Vector3(14f, 1.2f, 1f), new Color(0.15f, 0.3f, 0.25f));
        }

        private static GameObject CreateBlock(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetBoxSprite();
            sr.color = color;
            sr.sortingOrder = 1;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            return go;
        }

        private static void CreateSpikes(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            var go = CreateBlock(parent, name, pos, scale, new Color(0.9f, 0.2f, 0.2f));
            var hazard = go.AddComponent<Hazard2D>();

            // Configura dano
            var col = go.GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        private static void CreateEnemies(Transform parent)
        {
            // Inimigo 1 (Patrulheiro do Chão Intermediário)
            CreatePatrolEnemy(parent, "Enemy_Patrol1",
                new Vector3(26f, 0.2f, 0f),
                new Vector3(21f, 0.2f, 0f),
                new Vector3(31f, 0.2f, 0f),
                new Color(0.95f, 0.35f, 0.2f),
                50f, 20f);

            // Inimigo 2 (Guarda da Plataforma Alta)
            CreatePatrolEnemy(parent, "Enemy_Guard2",
                new Vector3(47f, 5.7f, 0f),
                new Vector3(45f, 5.7f, 0f),
                new Vector3(49f, 5.7f, 0f),
                new Color(0.85f, 0.15f, 0.35f),
                70f, 25f);
        }

        private static GameObject CreatePatrolEnemy(Transform parent, string name, Vector3 spawnPos, Vector3 pointA, Vector3 pointB, Color color, float hp, float dmg)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = spawnPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetBoxSprite();
            sr.color = color;
            sr.sortingOrder = 4;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3.5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.9f, 1.2f);

            var health = go.AddComponent<HealthSystem>();
            var enemy = go.AddComponent<EnemyAI>();

            // Pontos de patrulha
            var ptA = new GameObject($"{name}_PointA").transform;
            ptA.SetParent(go.transform.parent);
            ptA.position = pointA;

            var ptB = new GameObject($"{name}_PointB").transform;
            ptB.SetParent(go.transform.parent);
            ptB.position = pointB;

            var so = new SerializedObject(enemy);
            so.FindProperty("patrolPointA").objectReferenceValue = ptA;
            so.FindProperty("patrolPointB").objectReferenceValue = ptB;
            so.FindProperty("patrolSpeed").floatValue = 2.5f;
            so.FindProperty("chaseSpeed").floatValue = 4.5f;
            so.FindProperty("attackDamage").floatValue = dmg;
            so.ApplyModifiedProperties();

            return go;
        }

        private static void CreateCollectibles(Transform parent)
        {
            // Moedas no caminho
            CreateCoin(parent, "Coin_1", new Vector3(8f, 0.6f, 0f));
            CreateCoin(parent, "Coin_2", new Vector3(16f, 2.2f, 0f));
            CreateCoin(parent, "Coin_3", new Vector3(23f, 0.6f, 0f));
            CreateCoin(parent, "Coin_4", new Vector3(36f, 2.0f, 0f));
            CreateCoin(parent, "Coin_5", new Vector3(41f, 4.0f, 0f));
            CreateCoin(parent, "Coin_6", new Vector3(53f, 3.5f, 0f));
            CreateCoin(parent, "Coin_7", new Vector3(58f, 1.5f, 0f));

            // Estrela Secreta (Super Bônus)
            CreateStar(parent, "Star_Secret", new Vector3(36f, 6.4f, 0f));

            // Coração de Cura antes do trecho final
            CreateHeart(parent, "Heart_Heal", new Vector3(50f, 3.5f, 0f));
        }

        private static GameObject CreateCoin(Transform parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = new Color(1f, 0.85f, 0.1f); // Dourado
            sr.sortingOrder = 3;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var c = go.AddComponent<Collectible>();
            var so = new SerializedObject(c);
            so.FindProperty("type").enumValueIndex = (int)Collectible.CollectibleType.Coin;
            so.FindProperty("scoreValue").intValue = 50;
            so.ApplyModifiedProperties();

            return go;
        }

        private static GameObject CreateStar(Transform parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetBoxSprite();
            sr.color = new Color(0.4f, 0.9f, 1f); // Estrela Azul brilhante
            sr.sortingOrder = 3;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            var c = go.AddComponent<Collectible>();
            var so = new SerializedObject(c);
            so.FindProperty("type").enumValueIndex = (int)Collectible.CollectibleType.Star;
            so.FindProperty("scoreValue").intValue = 250;
            so.ApplyModifiedProperties();

            return go;
        }

        private static GameObject CreateHeart(Transform parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = new Color(1f, 0.25f, 0.4f); // Vermelho coração
            sr.sortingOrder = 3;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var c = go.AddComponent<Collectible>();
            var so = new SerializedObject(c);
            so.FindProperty("type").enumValueIndex = (int)Collectible.CollectibleType.Heart;
            so.FindProperty("healValue").floatValue = 35f;
            so.ApplyModifiedProperties();

            return go;
        }

        private static GameObject CreateGoal(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Goal_FinishFlag");
            go.transform.SetParent(parent);
            go.transform.position = pos;

            // Visual da bandeira/portal
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetBoxSprite();
            sr.color = new Color(0.2f, 0.95f, 0.45f); // Verde Esmeralda Vitória
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 2.5f);

            go.AddComponent<FinishGoal>();

            // Marcador no topo da bandeira
            var top = new GameObject("GoalTop");
            top.transform.SetParent(go.transform);
            top.transform.localPosition = new Vector3(0.4f, 0.8f, 0f);
            var topSr = top.AddComponent<SpriteRenderer>();
            topSr.sprite = GetBoxSprite();
            topSr.color = Color.yellow;
            topSr.sortingOrder = 4;

            return go;
        }

        private static void CreateDeathZone(Transform parent, Vector3 pos, float width)
        {
            var go = new GameObject("Abyss_DeathZone");
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(width, 4f);

            var hazard = go.AddComponent<Hazard2D>();
            var so = new SerializedObject(hazard);
            so.FindProperty("instantKill").boolValue = true;
            so.ApplyModifiedProperties();
        }

        private static Sprite GetBoxSprite()
        {
            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png");
            if (loaded != null) return loaded;
            return CreateBoxSprite(32, 32, Color.white);
        }

        private static Sprite GetCircleSprite()
        {
            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            if (loaded != null) return loaded;
            return CreateBoxSprite(32, 32, Color.white);
        }

        private static Sprite CreateBoxSprite(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = color;

            texture.SetPixels(colors);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                32f);
        }
    }
}
