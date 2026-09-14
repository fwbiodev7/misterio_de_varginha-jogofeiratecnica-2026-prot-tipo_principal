using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Level;
using Game.Managers;
using Game.Player;
using Game.Varginha;

namespace Game.Editor.Testing
{
    /// <summary>Cria a Fase 2 enxuta da escola, com reféns e subordinados ETs.</summary>
    public static class VarginhaPhase2Builder
    {
        private const string ScenePath = "Assets/Scenes/Fase2_Escola_Resgate.unity";

        [InitializeOnLoadMethod]
        private static void RunPendingCommandLineBuildAfterCompilation()
        {
            if (!System.Environment.CommandLine.Contains("BuildAndSavePhase2Silently")) return;
            EditorApplication.delayCall += BuildAndSavePhase2Silently;
        }

        [MenuItem("Tools/Varginha/🚨 Construir Fase 2 - Escola e Resgate", false, 5)]
        public static void BuildAndSavePhase2()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildPhase2(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsurePhase2InBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Varginha] Fase 2 salva em " + ScenePath);
            EditorUtility.DisplayDialog("Fase 2 pronta", "A escola, os subordinados ETs, os nove reféns e a saída pelo Fusca foram criados.", "OK");
        }

        public static void BuildAndSavePhase2Silently()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildPhase2(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsurePhase2InBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>Gera a cena sem substituir a cena que o desenvolvedor está editando.</summary>
        public static void BuildAndSavePhase2FromCurrentEditor()
        {
            var previous = EditorSceneManager.GetActiveScene();
            var generated = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            BuildPhase2(generated);
            EditorSceneManager.SaveScene(generated, ScenePath);
            EnsurePhase2InBuildSettings();
            EditorSceneManager.CloseScene(generated, true);
            if (previous.IsValid()) EditorSceneManager.SetActiveScene(previous);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Varginha] Fase 2 gerada em segundo plano: " + ScenePath);
        }

        private static void BuildPhase2(UnityEngine.SceneManagement.Scene scene)
        {
            var root = new GameObject("[VARGINHA_FASE_2_ESCOLA]");
            CreateManagers(root.transform);

            var camera = CreateCamera(root.transform);
            var player = CreatePlayer(root.transform, new Vector3(-6.1f, -1.2f, 0f));
            var follow = camera.GetComponent<CameraFollow2D>();
            follow.Target = player.transform;
            var soFollow = new SerializedObject(follow);
            soFollow.FindProperty("offset").vector3Value = new Vector3(0f, 0f, -10f);
            soFollow.FindProperty("limitMinY").boolValue = false;
            soFollow.ApplyModifiedProperties();

            var environment = new GameObject("Escola_3_Sistema_Ambiente").transform;
            environment.SetParent(root.transform);
            BuildSchool(environment);

            // A vaga fica dentro da área visível e alinhada ao marcador desenhado pelo ambiente.
            var car = CreateFusca(root.transform, VarginhaEnvironmentArt.FuscaParkingPosition);
            CreateSubordinate(root.transform, "ET_Subordinado_1", new Vector3(2.8f, 2.6f, 0f));
            CreateSubordinate(root.transform, "ET_Subordinado_2", new Vector3(5.1f, -2.4f, 0f));
            CreateSubordinate(root.transform, "ET_Subordinado_3", new Vector3(-.3f, 3.1f, 0f));
            CreateSubordinate(root.transform, "ET_Subordinado_4", new Vector3(6.6f, 2.9f, 0f));

            var studentsRoot = new GameObject("Refens_3_Sistema").transform;
            studentsRoot.SetParent(root.transform);
            string[] names = VarginhaPhase2Controller.StudentNames;
            Vector3[] positions =
            {
                new(-2.2f, 1.8f, 0f), new(0f, 1.8f, 0f), new(2.2f, 1.8f, 0f),
                new(-2.2f, 0f, 0f), new(0f, 0f, 0f), new(2.2f, 0f, 0f),
                new(-2.2f, -1.8f, 0f), new(0f, -1.8f, 0f), new(2.2f, -1.8f, 0f)
            };
            Color[] shirts =
            {
                new(.25f, .52f, .88f), new(.88f, .36f, .30f), new(.28f, .68f, .45f),
                new(.78f, .44f, .18f), new(.58f, .34f, .82f), new(.85f, .52f, .68f),
                new(.22f, .66f, .70f), new(.78f, .68f, .22f), new(.45f, .52f, .58f)
            };
            for (int i = 0; i < names.Length; i++) CreateStudent(studentsRoot, names[i], positions[i], shirts[i]);

            root.AddComponent<VarginhaPhase2Controller>();
            Selection.activeGameObject = player;
        }

        private static void CreateManagers(Transform parent)
        {
            var managers = new GameObject("GameManagers");
            managers.transform.SetParent(parent);
            managers.AddComponent<GameManager>();
            managers.AddComponent<ScoreManager>();
            managers.AddComponent<LevelManager>();
            managers.AddComponent<VarginhaGameHUD>();
            managers.AddComponent<VarginhaNotebookQuiz>();
        }

        private static GameObject CreateCamera(Transform parent)
        {
            var go = new GameObject("Main Camera");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 7.2f;
            camera.backgroundColor = new Color(.025f, .035f, .06f);
            go.AddComponent<AudioListener>();
            go.AddComponent<CameraFollow2D>();
            return go;
        }

        private static GameObject CreatePlayer(Transform parent, Vector3 position)
        {
            var go = new GameObject("Edelzio_Protagonist");
            go.tag = "Player";
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.08f, 1.08f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create("Edelzio", new Color(.62f, .82f, .38f));
            sr.sortingOrder = 5;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = .45f;
            var health = go.AddComponent<HealthSystem>();
            health.SetMaxHealth(100f, false);
            go.AddComponent<EdelzioTopDownController>();
            go.AddComponent<VarginhaPlayerSpriteAnimation>();
            go.AddComponent<VarginhaPlayerActionAnimation>();
            return go;
        }

        private static void BuildSchool(Transform parent)
        {
            VarginhaEnvironmentArt.BuildSchool(parent);
        }

        private static GameObject CreateSubordinate(Transform parent, string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("ET_Subordinate_" + name, new Color(.68f, .36f, .18f));
            renderer.sortingOrder = 8;
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = .42f;
            var health = go.AddComponent<HealthSystem>();
            var healthObject = new SerializedObject(health);
            healthObject.FindProperty("maxHealth").floatValue = 35f;
            healthObject.FindProperty("currentHealth").floatValue = 35f;
            healthObject.ApplyModifiedPropertiesWithoutUndo();
            var target = go.AddComponent<VarginhaCombatTarget>();
            target.SetKind(VarginhaCombatTarget.EnemyKind.MinorManifestation);
            go.AddComponent<VarginhaCombatEnemy>();
            return go;
        }

        private static void CreateStudent(Transform parent, string name, Vector3 position, Color shirtColor)
        {
            var go = new GameObject("Refem_" + name.Replace(" ", "_"));
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create("Student_" + name, shirtColor);
            renderer.sortingOrder = 5;
            go.AddComponent<VarginhaStudentHostage>().Configure(name, shirtColor);
        }

        private static GameObject CreateFusca(Transform parent, Vector3 position)
        {
            var go = new GameObject("Fusca_1996_Fase2");
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(2.6f, 2.6f, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            var frames = GetFuscaFrames();
            renderer.sprite = frames[0];
            renderer.sortingOrder = 4;
            go.AddComponent<FuscaDepartureAnimation>().Configure(frames);
            go.AddComponent<AudioSource>();
            go.AddComponent<FuscaEngineSound>();
            return go;
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
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }

        private static void CreateDecoration(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create(name, color);
            renderer.sortingOrder = 4;
        }

        private static Sprite[] GetFuscaFrames()
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Varginha/fusca-sprite-sheet.png");
            if (texture == null) return new[] { VarginhaPixelArtSprites.Create("Fusca_Fallback", new Color(.2f, .6f, .85f)) };
            int frameWidth = texture.width / 3;
            var frames = new Sprite[3];
            for (int i = 0; i < frames.Length; i++)
                frames[i] = Sprite.Create(texture, new Rect(i * frameWidth, 0, frameWidth, texture.height), new Vector2(.5f, .5f), texture.height);
            return frames;
        }

        private static void EnsurePhase2InBuildSettings()
        {
            var paths = new List<EditorBuildSettingsScene>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.path != ScenePath) paths.Add(scene);
            paths.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = paths.ToArray();
        }
    }
}
