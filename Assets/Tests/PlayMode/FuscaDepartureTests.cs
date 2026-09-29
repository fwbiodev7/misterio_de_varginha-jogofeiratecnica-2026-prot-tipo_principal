using System.Collections;
using System.Reflection;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class FuscaDepartureTests
    {
        [Test]
        public void SchoolBoardingStopsPhysicsBeforeMovingIntoTheSeat()
        {
            var root = new GameObject("Boarding_Test");
            IEnumerator routine = null;
            try
            {
                var car = new GameObject("Car");
                car.transform.SetParent(root.transform);
                var actor = new GameObject("Player");
                actor.transform.SetParent(root.transform);
                actor.AddComponent<SpriteRenderer>();
                var body = actor.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                actor.AddComponent<CircleCollider2D>();
                var player = actor.AddComponent<EdelzioTopDownController>();
                var controller = root.AddComponent<VarginhaPhase2Controller>();
                controller.enabled = false;
                const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                typeof(VarginhaPhase2Controller).GetField("_player", flags).SetValue(controller, player);
                typeof(VarginhaPhase2Controller).GetField("_fusca", flags).SetValue(controller, car.transform);
                body.linearVelocity = Vector2.right * 5f;
                routine = (IEnumerator)typeof(VarginhaPhase2Controller).GetMethod("DepartureRoutine", flags).Invoke(controller, null);
                Assert.IsTrue(routine.MoveNext());
                Assert.IsFalse(body.simulated);
                Assert.AreEqual(Vector2.zero, body.linearVelocity);
                Assert.IsTrue(player.IsScriptedMotion);
            }
            finally
            {
                (routine as System.IDisposable)?.Dispose();
                Object.DestroyImmediate(root);
            }
        }

        [UnityTest]
        public IEnumerator DepartureKeepsLaneAndEndsAtExactDistance()
        {
            foreach (bool flipped in new[] { false, true })
            {
                var car = new GameObject("Fusca_Test");
                try
                {
                    car.transform.position = new Vector3(3f, -4f, 2f);
                    car.AddComponent<SpriteRenderer>().flipX = flipped;
                    var animation = car.AddComponent<FuscaDepartureAnimation>();
                    Set(animation, "startDuration", .05f);
                    Set(animation, "driveSpeed", 30f);
                    Set(animation, "exitDistance", 1f);
                    int completions = 0;
                    animation.Depart(() => completions++);
                    animation.Depart(() => completions++); // Interações repetidas não iniciam outra saída.
                    float previous = car.transform.position.x;
                    float deadline = Time.realtimeSinceStartup + 3f;
                    while (completions == 0 && Time.realtimeSinceStartup < deadline)
                    {
                        yield return null;
                        var position = car.transform.position;
                        Assert.AreEqual(-4f, position.y, "O Fusca deve permanecer na mesma faixa.");
                        Assert.AreEqual(2f, position.z);
                        Assert.GreaterOrEqual((position.x - previous) * (flipped ? -1f : 1f), 0f);
                        previous = position.x;
                    }
                    Assert.AreEqual(1, completions);
                    Assert.AreEqual(flipped ? 2f : 4f, car.transform.position.x, .0001f);
                }
                finally { Object.DestroyImmediate(car); }
            }
        }

        private static void Set(FuscaDepartureAnimation animation, string field, float value)
        {
            typeof(FuscaDepartureAnimation).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(animation, value);
        }
    }
}
