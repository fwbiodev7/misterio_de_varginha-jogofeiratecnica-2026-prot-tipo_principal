using System.Collections;
using System.Linq;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaSchoolRescueTests
    {
        private GameObject _root;

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            Object.Destroy(_root);
            yield return null;
        }

        [UnityTest] public IEnumerator NineReleasedStudentsWalkOutTheDoorAndReachExternalFusca()
        {
            Time.timeScale = 1;
            _root = new GameObject("SchoolRescueTest");
            VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            var car = new GameObject("RescueCar").transform;
            car.SetParent(_root.transform);
            car.position = VarginhaEnvironmentArt.FuscaParkingPosition;
            var students = new VarginhaStudentHostage[9];
            var positions = new Vector2[9];
            for (int i = 0; i < 9; i++)
            {
                var go = new GameObject("RescueStudent_" + i);
                go.transform.SetParent(_root.transform);
                go.transform.position = new Vector3(-2.2f + i % 3 * 2.2f, 1.8f - i / 3 * 1.8f);
                go.AddComponent<SpriteRenderer>();
                students[i] = go.AddComponent<VarginhaStudentHostage>();
                students[i].ReleaseTo(car, i, car);
                positions[i] = go.transform.position;
            }
            float deadline = Time.realtimeSinceStartup + 25;
            while (students.Any(student => !student.IsAtFusca) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                for (int i = 0; i < students.Length; i++)
                {
                    Vector2 next = students[i].transform.position;
                    Assert.IsTrue(VarginhaSchoolNavigation.CanWalkSegment(positions[i], next), "Aluno atravessou uma parede.");
                    positions[i] = next;
                }
            }
            Assert.AreEqual(9, students.Count(student => student.IsAtFusca));
        }
    }
}
