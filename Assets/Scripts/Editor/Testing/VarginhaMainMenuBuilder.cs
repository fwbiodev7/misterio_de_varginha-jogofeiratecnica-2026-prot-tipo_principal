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

    /// <summary>Garante que Play sempre inicie pelo menu, mesmo com outra cena aberta no Editor.</summary>
    [InitializeOnLoad]
    internal static class VarginhaPlayModeStartScene
    {
        private const string MenuPath = "Assets/Scenes/Menu_MisterioDeVarginha.unity";
        private const string PlayCurrentSceneOnce = "Varginha.PlayCurrentSceneOnce";

        static VarginhaPlayModeStartScene()
        {
            EditorApplication.playModeStateChanged -= EnsureMenuBeforePlay;
            EditorApplication.playModeStateChanged += EnsureMenuBeforePlay;
            EditorApplication.delayCall += Apply;
        }

        private static void EnsureMenuBeforePlay(PlayModeStateChange state)
        {
            if (Application.isBatchMode) return;
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                bool testCurrentScene = SessionState.GetBool(PlayCurrentSceneOnce, false);
                SessionState.EraseBool(PlayCurrentSceneOnce);
                if (testCurrentScene) EditorSceneManager.playModeStartScene = null;
                else Apply();
            }
            if (state == PlayModeStateChange.EnteredEditMode) Apply();
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Keyboard input belongs to Game View; a focused Inspector/Scene view does not feed it.
                var gameView = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                if (gameView != null) EditorWindow.GetWindow(gameView).Focus();
            }
        }

        private static void Apply()
        {
            // The old QA flag could leak into every subsequent Play in the session.
            SessionState.EraseBool("Varginha.SuppressMenuForTests");
            if (Application.isBatchMode || EditorApplication.isPlaying) return;
            var menuScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath);
            if (menuScene != null) EditorSceneManager.playModeStartScene = menuScene;
        }

        [MenuItem("Tools/Varginha/Testar cena atual uma vez")]
        private static void PlayCurrentScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SessionState.SetBool(PlayCurrentSceneOnce, true);
            EditorApplication.isPlaying = true;
        }
    }
}
