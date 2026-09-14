using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Varginha;
using Game.Managers;
using Game.Level;

namespace Game.Editor.Testing
{
    /// <summary>
    /// Construtor da fase Top-Down estilo Pokémon / RPG clássico de investigação
    /// baseado no GDD de "O Segredo de Varginha".
    /// </summary>
    public static class VarginhaLevelBuilderTool
    {
        [InitializeOnLoadMethod]
        private static void RunPendingCommandLineBuildAfterCompilation()
        {
            if (!System.Environment.CommandLine.Contains("BuildAndSaveSceneSilently")) return;
            EditorApplication.delayCall += BuildAndSaveSceneSilently;
        }
        [MenuItem("Tools/Varginha/🛸 Construir Fase Top-Down (Na Cena Aberta)", false, 0)]
        public static void BuildInActiveScene()
        {
            if (EditorUtility.DisplayDialog("O Segredo de Varginha",
                "Isso construirá a fase Top-Down investigativa (Ato II: A Casa de Edelzio e a Fuga no Fusca) na cena aberta.\n\nDeseja continuar?",
                "Sim, Construir", "Cancelar"))
            {
                BuildVarginhaLevel();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("🛸 [Varginha] Fase Top-Down construída com sucesso!");
                EditorUtility.DisplayDialog("Fase Top-Down Pronta!",
                    "A fase estilo Pokémon / RPG investigativo de 'O Segredo de Varginha' foi gerada!\n\n" +
                    "Controles:\n- WASD ou Setas para andar em 4/8 direções\n- Shift para correr\n- E ou Espaço para examinar objetos e pistas\n\n" +
                    "Aperte PLAY para jogar!", "OK");
            }
        }

        [MenuItem("Tools/Varginha/💾 Salvar Cena Top-Down (FaseTopView_Varginha)", false, 1)]
        public static void BuildAndSaveScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildVarginhaLevel();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/FaseTopView_Varginha.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ [Varginha] Cena salva em: Assets/Scenes/FaseTopView_Varginha.unity");
            EditorUtility.DisplayDialog("Cena Salva!", "Cena salva com sucesso em 'Assets/Scenes/FaseTopView_Varginha.unity'!", "OK");
        }

        // Entry point used by automated Unity verification; it intentionally avoids editor dialogs.
        public static void BuildAndSaveSceneSilently()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildVarginhaLevel();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/FaseTopView_Varginha.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void BuildVarginhaLevel()
        {
            var oldRoot = GameObject.Find("[VARGINHA_TOPVIEW]");
            if (oldRoot != null) Object.DestroyImmediate(oldRoot);

            var oldRoot2 = GameObject.Find("[FASE_COMPLETA]");
            if (oldRoot2 != null) Object.DestroyImmediate(oldRoot2);

            var root = new GameObject("[VARGINHA_TOPVIEW]");

            // 1. Managers & HUD
            CreateManagers(root.transform);

            // 2. Camera
            var cam = CreateTopDownCamera(root.transform);

            // 3. Player: Edelzio
            var player = CreateEdelzio(root.transform, new Vector3(-5f, 2f, 0f));

            // Conecta Camera ao Player
            var follow = cam.GetComponent<CameraFollow2D>();
            if (follow != null)
            {
                follow.Target = player.transform;
                var soCam = new SerializedObject(follow);
                soCam.FindProperty("offset").vector3Value = new Vector3(0f, 0f, -10f);
                soCam.FindProperty("limitMinY").boolValue = false;
                soCam.ApplyModifiedProperties();
            }

            // 4. Cenário Top-Down (Paredes, Cômodos, Móveis e Quintal)
            var envRoot = new GameObject("House_And_Yard").transform;
            envRoot.SetParent(root.transform);
            BuildHouseAndYard(envRoot);

            // 5. Objetos Interativos & Pistas
            var propsRoot = new GameObject("Investigation_Props").transform;
            propsRoot.SetParent(root.transform);
            BuildProps(propsRoot);

            // 6. Inimigo: Manifestação da Entidade Ancestral
            var entityRoot = new GameObject("Supernatural_Entity").transform;
            entityRoot.SetParent(root.transform);
            BuildEntity(entityRoot);

            // 7. O Fusca de Edelzio (Fim de fase / Fuga)
            BuildFusca(root.transform, new Vector3(20f, 0f, 0f));
            VarginhaEnvironmentPolish.EnsureHouse(envRoot);

            Selection.activeGameObject = player;
        }

        private static void CreateManagers(Transform parent)
        {
            var existing = Object.FindAnyObjectByType<GameManager>();
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject("GameManagers");
                go.AddComponent<GameManager>();
                go.AddComponent<ScoreManager>();
                go.AddComponent<LevelManager>();
            }

            if (go.GetComponent<VarginhaGameHUD>() == null)
            {
                go.AddComponent<VarginhaGameHUD>();
            }
            if (go.GetComponent<VarginhaNotebookQuiz>() == null)
            {
                go.AddComponent<VarginhaNotebookQuiz>();
            }
        }

        private static GameObject CreateTopDownCamera(Transform parent)
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
                var c = camGo.AddComponent<Camera>();
                c.tag = "MainCamera";
                c.orthographic = true;
                c.orthographicSize = 6.5f;
                c.backgroundColor = new Color(0.05f, 0.07f, 0.12f);
                camGo.AddComponent<AudioListener>();
            }

            camGo.transform.SetParent(parent);
            camGo.transform.position = new Vector3(-5f, 2f, -10f);

            if (camGo.GetComponent<CameraFollow2D>() == null)
            {
                camGo.AddComponent<CameraFollow2D>();
            }

            return camGo;
        }

        private static GameObject CreateEdelzio(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Edelzio_Protagonist");
            go.tag = "Player";
            go.transform.SetParent(parent);
            go.transform.position = pos;
            // Edelzio deve ter presença de protagonista, não parecer um ícone do cenário.
            go.transform.localScale = new Vector3(1.08f, 1.08f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create("Edelzio", new Color(.62f, .82f, .38f));
            sr.color = Color.white; // A jaqueta já faz parte do sprite; evita escurecer a arte duas vezes.
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            go.AddComponent<EdelzioTopDownController>();
            go.AddComponent<VarginhaPlayerSpriteAnimation>();
            go.AddComponent<VarginhaPlayerActionAnimation>();

            return go;
        }

        private static void BuildHouseAndYard(Transform parent)
        {
            Color floorHouseColor = new Color(0.48f, 0.27f, 0.14f);
            Color floorYardColor = new Color(0.16f, 0.35f, 0.18f);
            Color wallColor = new Color(0.36f, 0.22f, 0.15f);

            // Piso modular mantém o pixel do tamanho certo: cada quadrado é um tile de 32 px,
            // em vez de uma textura única esticada por toda a casa.
            CreateTiledFloor(parent, "Floor_House", new Vector2Int(-9, -7), new Vector2Int(18, 14), floorHouseColor, 0);
            CreateTiledFloor(parent, "Floor_Yard", new Vector2Int(9, -8), new Vector2Int(18, 16), floorYardColor, 0);
            CreateTiledFloor(parent, "Driveway_Stone", new Vector2Int(10, -2), new Vector2Int(9, 4), new Color(0.34f, 0.35f, 0.34f), 1);
            CreateTiledFloor(parent, "Street_Road", new Vector2Int(20, -3), new Vector2Int(8, 6), new Color(0.12f, 0.16f, 0.20f), 1);

            // Paredes Externas da Casa
            CreateWall(parent, "Wall_North", new Vector3(0f, 7f, 0f), new Vector3(18f, 0.8f, 1f), wallColor);
            CreateWall(parent, "Wall_South", new Vector3(0f, -7f, 0f), new Vector3(18f, 0.8f, 1f), wallColor);
            CreateWall(parent, "Wall_West", new Vector3(-9f, 0f, 0f), new Vector3(0.8f, 14f, 1f), wallColor);
            CreateWall(parent, "Wall_East_Top", new Vector3(9f, 4.2f, 0f), new Vector3(0.8f, 5.6f, 1f), wallColor);
            CreateWall(parent, "Wall_East_Bottom", new Vector3(9f, -4.2f, 0f), new Vector3(0.8f, 5.6f, 1f), wallColor);
            // Porta aberta para o quintal em x: 9, y: 0

            // Divisórias internas (Quarto, Escritório e Sala)
            CreateWall(parent, "Wall_Bedroom_H", new Vector3(-4.5f, 0f, 0f), new Vector3(9f, 0.6f, 1f), wallColor);
            // Divisória do escritório dividida em duas partes: abre um vão real para a porta.
            CreateWall(parent, "Wall_OfficeDivider_Bottom", new Vector3(0f, -5.5f, 0f), new Vector3(0.6f, 3f, 1f), wallColor);
            CreateWall(parent, "Wall_OfficeDivider_Top", new Vector3(0f, -1f, 0f), new Vector3(0.6f, 2f, 1f), wallColor);
            CreateWall(parent, "Wall_Corridor_V2", new Vector3(0f, 4.5f, 0f), new Vector3(0.6f, 5f, 1f), wallColor);

            // Soleiras e detalhes estruturais separam os ambientes sem comprometer a leitura top-down.
            CreateDecoration(parent, "Doorway_Bedroom", new Vector3(-4.2f, 0.05f, 0f), new Vector3(1.55f, 0.42f, 1f), new Color(0.18f, 0.11f, 0.08f), 4);
            CreateDecoration(parent, "Door_Office_Accessible", new Vector3(-0.02f, -3.25f, 0f), new Vector3(0.62f, 1.8f, 1f), new Color(0.50f, 0.28f, 0.12f), 4);
            CreateDecoration(parent, "Doorway_Yard", new Vector3(8.96f, 0f, 0f), new Vector3(0.35f, 2.25f, 1f), new Color(0.64f, 0.36f, 0.16f), 4);

            // Muros do Quintal
            Color fenceColor = new Color(0.2f, 0.25f, 0.32f);
            CreateWall(parent, "Fence_North", new Vector3(18f, 8f, 0f), new Vector3(18f, 0.8f, 1f), fenceColor);
            CreateWall(parent, "Fence_South", new Vector3(18f, -8f, 0f), new Vector3(18f, 0.8f, 1f), fenceColor);
            CreateWall(parent, "Fence_East", new Vector3(27f, 0f, 0f), new Vector3(0.8f, 16f, 1f), fenceColor);

            BuildYardLandscaping(parent);

            // Árvores misteriosas no quintal
            CreateTree(parent, "Tree_Anomalous1", new Vector3(14f, 5f, 0f));
            CreateTree(parent, "Tree_Anomalous2", new Vector3(14f, -5f, 0f));
            CreateTree(parent, "Tree_Anomalous3", new Vector3(24f, 5.5f, 0f));
            CreateTree(parent, "Tree_Anomalous4", new Vector3(24f, -5.5f, 0f));
            BuildHouseDecor(parent);
        }

        private static void BuildYardLandscaping(Transform parent)
        {
            // Contorno de jardim e caminho até o Fusca dão escala e um destino visual claro.
            CreateDecoration(parent, "Porch_Wood", new Vector3(10.15f, 0f, 0f), new Vector3(1.7f, 3.0f, 1f), new Color(0.43f, 0.25f, 0.13f), 2);
            CreateDecoration(parent, "GardenBorder_North", new Vector3(16.8f, 6.6f, 0f), new Vector3(4.6f, 0.34f, 1f), new Color(0.10f, 0.23f, 0.13f), 2);
            CreateDecoration(parent, "GardenBorder_South", new Vector3(16.8f, -6.6f, 0f), new Vector3(4.6f, 0.34f, 1f), new Color(0.10f, 0.23f, 0.13f), 2);
            CreateDecoration(parent, "Shrub_NorthWest", new Vector3(11.7f, 5.8f, 0f), new Vector3(1.3f, 1.15f, 1f), new Color(0.13f, 0.38f, 0.18f), 2);
            CreateDecoration(parent, "Shrub_SouthWest", new Vector3(11.7f, -5.8f, 0f), new Vector3(1.3f, 1.15f, 1f), new Color(0.13f, 0.38f, 0.18f), 2);
            CreateDecoration(parent, "Shrub_NorthEast", new Vector3(25.6f, 6.3f, 0f), new Vector3(1.35f, 1.15f, 1f), new Color(0.13f, 0.38f, 0.18f), 2);
            CreateDecoration(parent, "Shrub_SouthEast", new Vector3(25.6f, -6.3f, 0f), new Vector3(1.35f, 1.15f, 1f), new Color(0.13f, 0.38f, 0.18f), 2);
            CreateDecoration(parent, "RoadMarking", new Vector3(23.5f, 0f, 0f), new Vector3(5.6f, 0.16f, 1f), new Color(0.88f, 0.72f, 0.31f), 2);
        }

        private static void BuildHouseDecor(Transform parent)
        {
            // Interior details inspired by cozy top-down pixel-art rooms.
            CreateDecoration(parent, "BedroomRug", new Vector3(-5.25f, 2.1f, 0f), new Vector3(3.0f, 1.35f, 1f), new Color(0.35f, 0.12f, 0.10f), 1);
            CreateDecoration(parent, "Rug_LivingRoom", new Vector3(4.5f, 2.45f, 0f), new Vector3(4.7f, 2.8f, 1f), new Color(0.54f, 0.15f, 0.12f), 1);
            CreateFurniture(parent, "CoffeeTable_Living", new Vector3(4.5f, 1.55f, 0f), new Vector3(1.8f, 1f, 1f), new Color(0.34f, 0.18f, 0.09f));
            CreateFurniture(parent, "Bookshelf_Office", new Vector3(-7.3f, -5.3f, 0f), new Vector3(1.2f, 2.4f, 1f), new Color(0.32f, 0.18f, 0.10f));
            // Cozinha: eletrodomésticos encostados na parede sul, com corredor central livre.
            CreateFurniture(parent, "Kitchen_Cabinet", new Vector3(4.55f, -5.15f, 0f), new Vector3(2.45f, 1.25f, 1f), new Color(0.52f, 0.34f, 0.20f));
            CreateDecoration(parent, "Nightstand_Bedroom", new Vector3(-3.2f, 5.15f, 0f), new Vector3(0.85f, 1.05f, 1f), new Color(0.38f, 0.23f, 0.12f), 3);
            CreateDecoration(parent, "Dresser_Bedroom", new Vector3(-7.65f, 1.15f, 0f), new Vector3(1.15f, 1.15f, 1f), new Color(0.42f, 0.25f, 0.13f), 3);
            // Assento alinhado à mesa: a sequência do notebook usa esta cadeira de verdade.
            CreateFurniture(parent, "Chair_Office", new Vector3(-5.0f, -2.25f, 0f), new Vector3(1.25f, 1.15f, 1f), new Color(0.22f, 0.30f, 0.34f));
            CreateDecoration(parent, "Radio_Office", new Vector3(-6.9f, -3.35f, 0f), new Vector3(0.8f, 0.6f, 1f), new Color(0.31f, 0.38f, 0.40f), 4);
            CreateDecoration(parent, "Stove_Kitchen", new Vector3(7.4f, -5.0f, 0f), new Vector3(1.15f, 1.45f, 1f), new Color(0.38f, 0.43f, 0.45f), 3);
            CreateDecoration(parent, "Fridge_Kitchen", new Vector3(1.45f, -4.95f, 0f), new Vector3(1.0f, 1.65f, 1f), new Color(0.55f, 0.65f, 0.68f), 3);
            CreateDecoration(parent, "CoffeeTable_Kitchen", new Vector3(4.6f, -2.55f, 0f), new Vector3(1.45f, 0.85f, 1f), new Color(0.39f, 0.22f, 0.11f), 2);
            CreateDecoration(parent, "Clock_Living", new Vector3(7.75f, 5.75f, 0f), new Vector3(0.65f, 0.7f, 1f), new Color(0.67f, 0.44f, 0.18f), 4);
            CreateDecoration(parent, "Plant_Indoor", new Vector3(7.35f, 2.4f, 0f), new Vector3(1.1f, 1.4f, 1f), new Color(0.18f, 0.48f, 0.23f), 3);
            CreateDecoration(parent, "Lamp_Desk", new Vector3(-3.7f, -3.3f, 0f), new Vector3(0.75f, 0.9f, 1f), new Color(0.95f, 0.73f, 0.26f), 4);
            CreateDecoration(parent, "WallPicture_Office", new Vector3(-7.0f, -1.25f, 0f), new Vector3(1.4f, 0.42f, 1f), new Color(0.18f, 0.47f, 0.55f), 4);
            CreateDecoration(parent, "WallPicture_Living", new Vector3(6.7f, 5.9f, 0f), new Vector3(1.3f, 0.42f, 1f), new Color(0.68f, 0.29f, 0.17f), 4);
            CreateDecoration(parent, "KitchenRunner", new Vector3(4.55f, -3.85f, 0f), new Vector3(3.4f, 0.7f, 1f), new Color(0.16f, 0.32f, 0.38f), 1);

            // Exterior pieces establish a readable garden, driveway and street.
            CreateDecoration(parent, "Mailbox_Yard", new Vector3(11.1f, 2.1f, 0f), new Vector3(0.9f, 1.1f, 1f), new Color(0.65f, 0.17f, 0.12f), 3);
            CreateDecoration(parent, "StreetLamp_Yard", new Vector3(25.7f, 3.1f, 0f), new Vector3(0.9f, 2.4f, 1f), new Color(0.85f, 0.68f, 0.30f), 3);
            CreateDecoration(parent, "FlowerPatch_Left", new Vector3(12.4f, -3.6f, 0f), new Vector3(1.6f, 1.0f, 1f), new Color(0.93f, 0.37f, 0.48f), 2);
            CreateDecoration(parent, "FlowerPatch_Right", new Vector3(25.0f, -3.9f, 0f), new Vector3(1.6f, 1.0f, 1f), new Color(0.94f, 0.74f, 0.23f), 2);
        }

        private static void BuildProps(Transform parent)
        {
            // 1. Quarto de Edelzio
            // Cama
            CreateFurniture(parent, "Bed_Edelzio", new Vector3(-6.5f, 5f, 0f), new Vector3(2.8f, 3.4f, 1f), new Color(0.24f, 0.42f, 0.61f));
            // Caixa de brinquedos debaixo da cama (Chave do Fusca + Caderno de 1996)
            var toyBox = CreateInteractable(parent, "ToyBox_UnderBed", new Vector3(-6.5f, 3.2f, 0f), new Vector3(1f, 0.8f, 1f),
                new Color(0.95f, 0.65f, 0.15f), PropType.ToyBoxUnderBed, "Caixa de Brinquedos Antiga",
                "Uma antiga caixa de madeira com brinquedos de infância de 1996...");
            toyBox.AddComponent<ToyBoxOpenAnimation>().Configure(new Color(0.95f, 0.65f, 0.15f));

            // Mochila de Edelzio
            var backpack = CreateInteractable(parent, "Backpack_Prop", new Vector3(-3.0f, 2.0f, 0f), new Vector3(0.9f, 0.9f, 1f),
                new Color(0.2f, 0.75f, 0.4f), PropType.Backpack, "Mochila do Professor",
                "Sua mochila com materiais e anotações das aulas.");
            backpack.AddComponent<BackpackPickupAnimation>();

            // 2. Escritório de TI
            // Mesa de trabalho
            CreateFurniture(parent, "Desk_Office", new Vector3(-5f, -3.5f, 0f), new Vector3(3.7f, 1.8f, 1f), new Color(0.36f, 0.22f, 0.12f));
            // Notebook de Edelzio
            CreateInteractable(parent, "Notebook_TI", new Vector3(-5f, -3.5f, 0f), new Vector3(1.1f, 0.8f, 1f),
                new Color(0.3f, 0.9f, 1f), PropType.NotebookLaptop, "Notebook de Programação",
                "O computador onde você analisa códigos e anomalias de Varginha.");

            // Documento de 1898
            CreateInteractable(parent, "Doc_Historical", new Vector3(-2f, -5.5f, 0f), new Vector3(0.7f, 0.7f, 1f),
                new Color(0.9f, 0.85f, 0.7f), PropType.OldDocument, "Recorte de Jornal Antigo",
                "Notícias sobre Zé Gomes e as primeiras escavações em 1898.");

            // 3. Sala & Cozinha
            // Sofá
            CreateFurniture(parent, "Sofa_LivingRoom", new Vector3(4.5f, 3.55f, 0f), new Vector3(3.8f, 1.9f, 1f), new Color(0.20f, 0.42f, 0.37f));
            // TV Chiando com Estática de 1996
            CreateFurniture(parent, "TV_StaticNoise", new Vector3(4.5f, 5.45f, 0f), new Vector3(2.7f, 1.55f, 1f), new Color(0.24f, 0.31f, 0.35f));
            // Mesa com Café (Restaura Sanidade)
            CreateInteractable(parent, "Coffee_Cup", new Vector3(4.6f, -2.55f, 0f), new Vector3(0.8f, 0.8f, 1f),
                new Color(0.8f, 0.4f, 0.2f), PropType.CoffeeOrFood, "Café Quente",
                "Uma xícara de café fresco para acalmar a tensão.");

            // Caixa de Força (Eletricidade Falhando)
            CreateInteractable(parent, "FuseBox_Prop", new Vector3(1f, 6.2f, 0f), new Vector3(1f, 1.2f, 1f),
                new Color(0.9f, 0.3f, 0.2f), PropType.FuseBox, "Caixa de Força",
                "Pulsos eletromagnéticos anômalos estão fazendo os fusíveis chiarem.");
        }

        private static void BuildEntity(Transform parent)
        {
            var go = new GameObject("Entity_Manifestation");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(14f, 0f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            // A entidade ancestral mantém a silhueta própria, mas compartilha a
            // leitura visual dos inimigos: pele marrom e olhos vermelhos.
            sr.sprite = VarginhaPixelArtSprites.Create("ET_Subordinate_Entity", new Color(.68f, .36f, .18f));
            sr.color = new Color(.68f, .36f, .18f, .85f);
            sr.sortingOrder = 6;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.8f;

            var ai = go.AddComponent<EntityManifestationAI>();

            // Waypoints de patrulha
            var p1 = new GameObject("Waypoint_1").transform;
            p1.SetParent(go.transform);
            p1.position = new Vector3(12f, 3.5f, 0f);

            var p2 = new GameObject("Waypoint_2").transform;
            p2.SetParent(go.transform);
            p2.position = new Vector3(12f, -3.5f, 0f);

            var p3 = new GameObject("Waypoint_3").transform;
            p3.SetParent(go.transform);
            p3.position = new Vector3(17f, 0f, 0f);

            var so = new SerializedObject(ai);
            var wpProp = so.FindProperty("patrolWaypoints");
            wpProp.arraySize = 3;
            wpProp.GetArrayElementAtIndex(0).objectReferenceValue = p1;
            wpProp.GetArrayElementAtIndex(1).objectReferenceValue = p2;
            wpProp.GetArrayElementAtIndex(2).objectReferenceValue = p3;
            so.ApplyModifiedProperties();
        }

        private static void BuildFusca(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Fusca_1996_Exit");
            go.transform.SetParent(parent);
            go.transform.position = pos;
            // O spritesheet tem PPU próprio; esta escala o deixa proporcional ao jogador.
            go.transform.localScale = new Vector3(2.75f, 2.75f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            var fuscaFrames = GetFuscaFrames();
            sr.sprite = fuscaFrames[0];
            sr.color = Color.white;
            sr.sortingOrder = 4;

            go.AddComponent<FuscaDepartureAnimation>().Configure(fuscaFrames);
            go.AddComponent<AudioSource>();
            go.AddComponent<FuscaEngineSound>();

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            // Faróis amarelos no Fusca
            // Scripts
            var prop = go.AddComponent<InteractableProp>();
            var so = new SerializedObject(prop);
            so.FindProperty("propType").enumValueIndex = (int)PropType.FuscaVehicle;
            so.FindProperty("propName").stringValue = "Fusca 1996 (Veículo de Fuga)";
            so.FindProperty("canInteractMultipleTimes").boolValue = true;
            so.ApplyModifiedProperties();

            go.AddComponent<FuscaLevelExit>();
        }

        private static void CreateFloor(Transform parent, string name, Vector3 pos, Vector3 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create(name, color);
            sr.color = Color.white;
            sr.sortingOrder = 0;
        }

        private static void CreateTiledFloor(Transform parent, string name, Vector2Int origin, Vector2Int dimensions, Color color, int sortingOrder)
        {
            for (int y = 0; y < dimensions.y; y++)
            for (int x = 0; x < dimensions.x; x++)
            {
                // Três variações sutis quebram a repetição sem perder a paleta coesa.
                int variation = (x * 7 + y * 13) % 7;
                Color tileColor = variation == 0 ? Color.Lerp(color, Color.white, .075f) :
                    variation == 1 ? Color.Lerp(color, Color.black, .07f) : color;
                var tile = new GameObject(name + "_Tile_" + x + "_" + y);
                tile.transform.SetParent(parent);
                tile.transform.position = new Vector3(origin.x + x + .5f, origin.y + y + .5f, 0f);
                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = VarginhaPixelArtSprites.Create(name, tileColor);
                sr.sortingOrder = sortingOrder;
            }
        }

        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create(name, color);
            sr.color = Color.white;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        private static void CreateFurniture(Transform parent, string name, Vector3 pos, Vector3 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create(name, color);
            sr.color = Color.white;
            sr.sortingOrder = 2;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        private static void CreateDecoration(Transform parent, string name, Vector3 pos, Vector3 size, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create(name, color);
            sr.color = Color.white;
            sr.sortingOrder = sortingOrder;
        }

        private static void CreateTree(Transform parent, string name, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetTreeSprite();
            sr.color = Color.white;
            sr.sortingOrder = 4;
            go.transform.localScale = new Vector3(2.9f, 2.9f, 1f);

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
        }

        private static GameObject CreateInteractable(Transform parent, string name, Vector3 pos, Vector3 size, Color color, PropType type, string propName, string message)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create(name, color);
            sr.color = Color.white;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            var prop = go.AddComponent<InteractableProp>();
            var so = new SerializedObject(prop);
            so.FindProperty("propType").enumValueIndex = (int)type;
            so.FindProperty("propName").stringValue = propName;
            so.FindProperty("inspectMessage").stringValue = message;
            // O notebook abre um minigame: o jogador pode fechar e tentar novamente antes de acertar.
            so.FindProperty("canInteractMultipleTimes").boolValue = type == PropType.NotebookLaptop;
            so.ApplyModifiedProperties();
            return go;
        }

        private static Sprite GetBoxSprite()
        {
            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png");
            if (loaded != null) return loaded;
            return CreateFallbackSprite(32, 32);
        }

        private static Sprite GetCircleSprite()
        {
            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            if (loaded != null) return loaded;
            return CreateFallbackSprite(32, 32);
        }

        private static Sprite GetTreeSprite()
        {
            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Varginha/tree.png");
            return loaded != null ? loaded : GetCircleSprite();
        }

        private static Sprite[] GetFuscaFrames()
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Varginha/fusca-sprite-sheet.png");
            if (texture == null) return new[] { GetBoxSprite() };

            const int frameCount = 3;
            int frameWidth = texture.width / frameCount;
            var frames = new Sprite[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                frames[i] = Sprite.Create(texture, new Rect(i * frameWidth, 0, frameWidth, texture.height),
                    new Vector2(0.5f, 0.5f), texture.height);
            }
            return frames;
        }

        private static Sprite CreateFallbackSprite(int w, int h)
        {
            var tex = new Texture2D(w, h);
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
