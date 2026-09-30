using Game.Level;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class VarginhaCameraFramingTests
    {
        [TestCase("FaseTopView_Varginha", -10.25f, -9.25f, 28.25f, 9.25f, 3.8f)]
        [TestCase("Fase2_Escola_Resgate", -12.3f, -14f, 12.3f, 6f, 4.2f)]
        [TestCase("Fase3_Igreja_Guardiao", -8.5f, -6.5f, 9.5f, 6.5f, 4f)]
        public void ViewportStaysInsideMapAtCornersAndAfterResizing(string scene, float left, float bottom,
            float right, float top, float zoom)
        {
            var go = new GameObject("CameraBoundsTest");
            var target = new GameObject("Target");
            try
            {
                var camera = go.AddComponent<Camera>();
                var follow = go.AddComponent<CameraFollow2D>();
                follow.Target = target.transform;
                Assert.IsTrue(VarginhaCameraFraming.Configure(follow, scene));
                foreach (float aspect in new[] { 4f / 3f, 16f / 9f, 21f / 9f, 32f / 9f, 9f / 16f })
                {
                    camera.aspect = aspect;
                    foreach (float x in new[] { left - 20f, (left + right) / 2f, right + 20f })
                    foreach (float y in new[] { bottom - 20f, (bottom + top) / 2f, top + 20f })
                    {
                        target.transform.position = new Vector3(x, y);
                        // Native lifecycle dispatch is unavailable on ordinary behaviours in Edit Mode.
                        typeof(CameraFollow2D).GetMethod("LateUpdate",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                            .Invoke(follow, null);
                        var position = go.transform.position;
                        float h = camera.orthographicSize, w = h * aspect;
                        Assert.LessOrEqual(h, zoom);
                        Assert.GreaterOrEqual(position.x - w, left - .001f);
                        Assert.LessOrEqual(position.x + w, right + .001f);
                        Assert.GreaterOrEqual(position.y - h, bottom - .001f);
                        Assert.LessOrEqual(position.y + h, top + .001f);
                        Assert.AreEqual(-10f, position.z, .001f);
                        var shake = follow.ConstrainPosition(position + new Vector3(3f, -3f));
                        Assert.LessOrEqual(shake.x + w, right + .001f);
                        Assert.GreaterOrEqual(shake.y - h, bottom - .001f);
                    }
                }
            }
            finally { Object.DestroyImmediate(go); Object.DestroyImmediate(target); }
        }
    }
}
