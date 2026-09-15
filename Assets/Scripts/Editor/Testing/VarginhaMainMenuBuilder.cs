using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Varginha;

namespace Game.Editor.Testing
{
    /// <summary>Gera a cena inicial do protótipo sem exigir assets de interface externos.</summary>
    public static class VarginhaMainMenuBuilder
    {
        [MenuItem("Tools/Varginha/Criar Menu Principal (Mistério de Varginha)", false, 2)]
        public static void BuildAndSaveScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("[MISTERIO_DE_VARGINHA_MENU]");
            root.AddComponent<VarginhaMainMenu>();
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<Camera>().backgroundColor = new Color(.005f, .015f, .04f);
            cameraObject.AddComponent<AudioListener>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Menu_MisterioDeVarginha.unity");
            EnsureScenesInBuildSettings();
            SetMenuAsPlayModeStartScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Mistério de Varginha] Menu salvo em Assets/Scenes/Menu_MisterioDeVarginha.unity");
        }

        // Ponto de entrada para verificação sem diálogos no Unity batch mode.
        public static void BuildAndSaveSceneSilently() => BuildAndSaveScene();

        private static void EnsureScenesInBuildSettings()
        {
            const string menuPath = "Assets/Scenes/Menu_MisterioDeVarginha.unity";
            const string investigationPath = "Assets/Scenes/FaseTopView_Varginha.unity";
            var updated = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var item in EditorBuildSettings.scenes)
                if (item.path != menuPath && item.path != investigationPath) updated.Add(item);

            // O índice 0 é sempre o menu; a investigação só começa pelo botão INICIAR.
            updated.Insert(0, new EditorBuildSettingsScene(menuPath, true));
            updated.Add(new EditorBuildSettingsScene(investigationPath, true));
            EditorBuildSettings.scenes = updated.ToArray();
        }

        private static void SetMenuAsPlayModeStartScene()
        {
            const string menuPath = "Assets/Scenes/Menu_MisterioDeVarginha.unity";
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(menuPath);
        }
    }

    /// <summary>Gera uma única vez a cena ao término da próxima compilação do Editor.</summary>
    [InitializeOnLoad]
    internal static class VarginhaMainMenuRefreshOnce
    {
        private const string RefreshKey = "VarginhaMainMenuRefreshOnce_20260911_MenuFirstAndFurniture";

        static VarginhaMainMenuRefreshOnce()
        {
            if (EditorPrefs.GetBool(RefreshKey, false)) return;
            EditorApplication.delayCall += Refresh;
        }

        private static void Refresh()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Refresh;
                return;
            }

            EditorPrefs.SetBool(RefreshKey, true);
            VarginhaMainMenuBuilder.BuildAndSaveSceneSilently();
        }
    }

    /// <summary>Garante que Play sempre inicie pelo menu, mesmo com outra cena aberta no Editor.</summary>
    [InitializeOnLoad]
    internal static class VarginhaPlayModeStartScene
    {
        private const string MenuPath = "Assets/Scenes/Menu_MisterioDeVarginha.unity";

        static VarginhaPlayModeStartScene()
        {
            EditorApplication.playModeStateChanged -= EnsureMenuBeforePlay;
            EditorApplication.playModeStateChanged += EnsureMenuBeforePlay;
            EditorApplication.delayCall += Apply;
        }

        private static void EnsureMenuBeforePlay(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) Apply();
        }

        private static void Apply()
        {
            // Session-only opt-out for test/visual QA runs; survives assembly reloads,
            // unlike removing the callback. Normal Play still always starts at the menu.
            if (SessionState.GetBool("Varginha.SuppressMenuForTests", false)) return;
            var menuScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath);
            if (menuScene != null) EditorSceneManager.playModeStartScene = menuScene;
        }
    }
}
