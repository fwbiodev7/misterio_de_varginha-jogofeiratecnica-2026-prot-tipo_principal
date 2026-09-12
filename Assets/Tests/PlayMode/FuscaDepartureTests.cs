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
