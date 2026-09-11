using UnityEngine;
using UnityEditor;
using Game.Player;
using Game.Enemies;
using Game.Managers;

namespace Game.Editor.Testing
{
    /// <summary>
    /// Spawna objetos de teste na cena (Player, Enemies, Collectibles).
    /// Todos os objetos criados recebem a tag "TestObject" para fácil limpeza.
    /// </summary>
    public static class SpawnTestTool
    {
        private const string TestTag = "EditorTest";

        // ─── Spawn ────────────────────────────────────────────────────────────

        public static void SpawnPlayer()
        {
            var go = new GameObject("TestPlayer");
            go.name = "[TEST] Player";

            // Componentes físicos
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.8f, 1.8f);

            // Renderer (quadrado colorido)
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(Color.cyan);
            sr.color = new Color(0.2f, 0.8f, 1f);

            // Lógica
            go.AddComponent<HealthSystem>();
            go.AddComponent<PlayerController>();

            // GroundCheck filho
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(go.transform);
            gc.transform.localPosition = new Vector3(0f, -0.9f, 0f);

            // Posição no centro da cena
            go.transform.position = GetSpawnPosition();

            Undo.RegisterCreatedObjectUndo(go, "Spawn Test Player");
            Selection.activeGameObject = go;
            Debug.Log($"[SpawnTool] Player de teste criado: '{go.name}'");
        }

        public static void SpawnEnemy()
        {
            var go = new GameObject("[TEST] Enemy");

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(Color.red);
            sr.color = new Color(1f, 0.3f, 0.3f);

            go.AddComponent<HealthSystem>();
            go.AddComponent<EnemyAI>();

            // Pontos de patrulha
            var ptA = new GameObject("[TEST] PatrolA");
            ptA.transform.position = GetSpawnPosition() + Vector3.left * 3f;
            var ptB = new GameObject("[TEST] PatrolB");
            ptB.transform.position = GetSpawnPosition() + Vector3.right * 3f;

            go.transform.position = GetSpawnPosition() + Vector3.right;

            Undo.RegisterCreatedObjectUndo(go, "Spawn Test Enemy");
            Undo.RegisterCreatedObjectUndo(ptA, "Spawn PatrolA");
            Undo.RegisterCreatedObjectUndo(ptB, "Spawn PatrolB");

            Selection.activeGameObject = go;
            Debug.Log($"[SpawnTool] Inimigo de teste criado: '{go.name}'");
        }

        public static void SpawnCollectible()
        {
            var go = new GameObject("[TEST] Collectible");

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(Color.yellow);
            sr.color = new Color(1f, 0.9f, 0.2f);
            go.transform.localScale = Vector3.one * 0.6f;

            go.AddComponent<Collectible>();
            go.transform.position = GetSpawnPosition() + Vector3.up * 1f;

            Undo.RegisterCreatedObjectUndo(go, "Spawn Test Collectible");
            Selection.activeGameObject = go;
            Debug.Log($"[SpawnTool] Collectible de teste criado: '{go.name}'");
        }

        // ─── Clear ────────────────────────────────────────────────────────────

        public static void ClearTestObjects()
        {
            var all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude);
            int count = 0;
            foreach (var go in all)
            {
                if (go.name.StartsWith("[TEST]"))
                {
                    Undo.DestroyObjectImmediate(go);
                    count++;
                }
            }
            Debug.Log($"[SpawnTool] {count} objetos de teste removidos.");
            EditorUtility.DisplayDialog("Clear Test Objects", $"✅ {count} objetos removidos.", "OK");
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static Vector3 GetSpawnPosition()
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv != null)
                return sv.pivot;
            return Vector3.zero;
        }

        private static Sprite CreateSquareSprite(Color color)
        {
            // Cria sprite branco 32x32 (tintado pelo SpriteRenderer.color)
            var tex = new Texture2D(32, 32);
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
