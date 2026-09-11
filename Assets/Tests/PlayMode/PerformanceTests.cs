using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Testes de performance medindo tempo de execução, física e estabilidade de frames.
    /// Utiliza System.Diagnostics.Stopwatch e NUnit sem dependência de pacotes externos.
    /// </summary>
    public class PerformanceTests
    {
        // ─── Instantiation ────────────────────────────────────────────────────

        [Test]
        public void Performance_Instantiate100Objects()
        {
            var prefab = new GameObject("Prefab");
            var objects = new GameObject[100];
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                objects[i] = Object.Instantiate(prefab);
            }

            sw.Stop();
            UnityEngine.Debug.Log($"[Performance] Instantiate 100 objects: {sw.ElapsedMilliseconds} ms");

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    Object.DestroyImmediate(objects[i]);
                }
            }
            Object.DestroyImmediate(prefab);

            Assert.Less(sw.ElapsedMilliseconds, 2000, "Instanciação de 100 objetos demorou mais que o esperado.");
        }

        [Test]
        public void Performance_Destroy100Objects()
        {
            var prefab = new GameObject("Prefab");
            var objects = new GameObject[100];
            for (int i = 0; i < 100; i++)
            {
                objects[i] = Object.Instantiate(prefab);
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    Object.DestroyImmediate(objects[i]);
                }
            }
            sw.Stop();
            UnityEngine.Debug.Log($"[Performance] Destroy 100 objects: {sw.ElapsedMilliseconds} ms");

            Object.DestroyImmediate(prefab);
            Assert.Less(sw.ElapsedMilliseconds, 2000, "Destruição de 100 objetos demorou mais que o esperado.");
        }

        // ─── Vector Operations ────────────────────────────────────────────────

        [Test]
        public void Performance_VectorOperations_ExecutionSpeed()
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                Vector2 a = new Vector2(1f, 2f);
                Vector2 b = new Vector2(3f, 4f);
                float dist = Vector2.Distance(a, b);
                Vector2 norm = (b - a).normalized;
                _ = dist;
                _ = norm;
            }
            sw.Stop();
            UnityEngine.Debug.Log($"[Performance] 100k Vector2 ops: {sw.ElapsedMilliseconds} ms");

            Assert.Less(sw.ElapsedMilliseconds, 1000, "100k operações de Vector2 demoraram mais que o esperado.");
        }

        // ─── Physics2D ────────────────────────────────────────────────────────

        [Test]
        public void Performance_Physics2D_OverlapCircle_1000Times()
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                Physics2D.OverlapCircle(Vector2.zero, 1f);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"[Performance] 1000 OverlapCircle calls: {sw.ElapsedMilliseconds} ms");

            Assert.Less(sw.ElapsedMilliseconds, 1000, "1000 OverlapCircle demorou mais que o esperado.");
        }

        // ─── Frame Time ───────────────────────────────────────────────────────

        [UnityTest]
        public IEnumerator Performance_EmptyScene_FrameTime()
        {
            float totalDelta = 0f;
            int frames = 30;

            for (int i = 0; i < frames; i++)
            {
                yield return null;
                totalDelta += Time.deltaTime;
            }

            float avgFrameTime = (totalDelta / frames) * 1000f;
            UnityEngine.Debug.Log($"[Performance] Empty scene avg frame time: {avgFrameTime:F2} ms");
            Assert.Less(avgFrameTime, 100f, "Frame time médio excedeu o limite tolerável.");
        }

        [UnityTest]
        public IEnumerator Performance_50DynamicObjects_FrameTime()
        {
            var objects = new GameObject[50];
            for (int i = 0; i < 50; i++)
            {
                objects[i] = new GameObject($"Dyn_{i}");
                var rb = objects[i].AddComponent<Rigidbody2D>();
                objects[i].AddComponent<CircleCollider2D>();
                objects[i].transform.position = new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(0f, 10f), 0f);
            }

            float totalDelta = 0f;
            int frames = 30;

            for (int i = 0; i < frames; i++)
            {
                yield return null;
                totalDelta += Time.deltaTime;
            }

            float avgFrameTime = (totalDelta / frames) * 1000f;
            UnityEngine.Debug.Log($"[Performance] 50 dynamic objects avg frame time: {avgFrameTime:F2} ms");

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    Object.Destroy(objects[i]);
                }
            }

            Assert.Less(avgFrameTime, 100f, "Frame time com 50 objetos dinâmicos excedeu o limite.");
        }

        // ─── Find Operations ──────────────────────────────────────────────────

        [Test]
        public void Performance_FindObjectsByType_100Times()
        {
            var objs = new GameObject[10];
            for (int i = 0; i < 10; i++)
            {
                objs[i] = new GameObject($"Obj_{i}");
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude);
            }
            sw.Stop();
            UnityEngine.Debug.Log($"[Performance] 100x FindObjectsByType: {sw.ElapsedMilliseconds} ms");

            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] != null)
                {
                    Object.DestroyImmediate(objs[i]);
                }
            }

            Assert.Less(sw.ElapsedMilliseconds, 2000, "100x FindObjectsByType demorou mais que o esperado.");
        }

        // ─── String Operations ────────────────────────────────────────────────

        [Test]
        public void Performance_StringConcat_VsStringBuilder()
        {
            var swSb = Stopwatch.StartNew();
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                sb.Append("Test").Append(i);
            }
            _ = sb.ToString();
            swSb.Stop();

            UnityEngine.Debug.Log($"[Performance] StringBuilder 1000 appends: {swSb.ElapsedMilliseconds} ms");
            Assert.Less(swSb.ElapsedMilliseconds, 1000, "StringBuilder demorou mais que o esperado.");
        }
    }
}
