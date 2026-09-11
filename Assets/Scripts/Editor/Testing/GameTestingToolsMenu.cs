using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace Game.Editor.Testing
{
    /// <summary>
    /// Menu central de Testing Tools.
    /// Acessível via: Tools > Game Testing
    /// </summary>
    public static class GameTestingToolsMenu
    {
        private const string MenuRoot = "Tools/Game Testing/";

        // ─── Level Builder ────────────────────────────────────────────────────

        [MenuItem(MenuRoot + "🏰 Construir Fase Completa (Na Cena Aberta)", false, 0)]
        public static void BuildLevelInActiveScene()
        {
            LevelBuilderTool.BuildInActiveScene();
        }

        [MenuItem(MenuRoot + "💾 Salvar Cenas (FaseCompleta e Fase1)", false, 1)]
        public static void SaveLevelScenes()
        {
            LevelBuilderTool.BuildAndSaveScenes();
        }

        // ─── Scene Health ─────────────────────────────────────────────────────

        [MenuItem(MenuRoot + "🩺 Scene Health Check", false, 10)]
        public static void RunSceneHealthCheck()
        {
            SceneHealthChecker.RunCheck();
        }

        [MenuItem(MenuRoot + "🗺️ Tilemap Validator")]
        public static void RunTilemapValidator()
        {
            TilemapValidatorTool.RunValidation();
        }

        // ─── Performance ──────────────────────────────────────────────────────

        [MenuItem(MenuRoot + "📊 Performance Profiler")]
        public static void RunPerformanceProfiler()
        {
            PerformanceProfilerTool.RunProfile();
        }

        // ─── Spawn ────────────────────────────────────────────────────────────

        [MenuItem(MenuRoot + "👾 Spawn/Spawn Test Enemy")]
        public static void SpawnTestEnemy()
        {
            SpawnTestTool.SpawnEnemy();
        }

        [MenuItem(MenuRoot + "👾 Spawn/Spawn Test Collectible")]
        public static void SpawnTestCollectible()
        {
            SpawnTestTool.SpawnCollectible();
        }

        [MenuItem(MenuRoot + "👾 Spawn/Spawn Player Test Prefab")]
        public static void SpawnTestPlayer()
        {
            SpawnTestTool.SpawnPlayer();
        }

        [MenuItem(MenuRoot + "👾 Spawn/Clear All Test Objects")]
        public static void ClearAllTestObjects()
        {
            SpawnTestTool.ClearTestObjects();
        }

        // ─── Input ────────────────────────────────────────────────────────────

        [MenuItem(MenuRoot + "🎮 Input/Simulate Move Right")]
        public static void SimulateMoveRight()
        {
            InputSimulatorTool.SimulateMove(Vector2.right);
        }

        [MenuItem(MenuRoot + "🎮 Input/Simulate Move Left")]
        public static void SimulateMoveLeft()
        {
            InputSimulatorTool.SimulateMove(Vector2.left);
        }

        [MenuItem(MenuRoot + "🎮 Input/Simulate Jump")]
        public static void SimulateJump()
        {
            InputSimulatorTool.SimulateJump();
        }

        [MenuItem(MenuRoot + "🎮 Input/Simulate Dash")]
        public static void SimulateDash()
        {
            InputSimulatorTool.SimulateDash();
        }

        // ─── Separators ───────────────────────────────────────────────────────

        [MenuItem(MenuRoot + "Open Test Runner")]
        public static void OpenTestRunner()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
        }

        [MenuItem(MenuRoot + "Open Profiler")]
        public static void OpenProfiler()
        {
            EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
        }
    }
}
