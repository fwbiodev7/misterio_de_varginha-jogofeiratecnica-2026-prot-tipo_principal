using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Editor.Testing
{
    /// <summary>One-shot refresh used to persist the authored Varginha map after source changes.</summary>
    [InitializeOnLoad]
    internal static class VarginhaMapRefreshOnce
    {
        private const string RefreshKey = "VarginhaMapRefreshOnce_20260911_EdelzioDirectionSpritesV2";

        static VarginhaMapRefreshOnce()
        {
            if (EditorPrefs.GetBool(RefreshKey, false)) return;
            EditorApplication.delayCall += Refresh;
        }

        private static void Refresh()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += Refresh;
                return;
            }

            EditorPrefs.SetBool(RefreshKey, true);
            VarginhaLevelBuilderTool.BuildAndSaveSceneSilently();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
