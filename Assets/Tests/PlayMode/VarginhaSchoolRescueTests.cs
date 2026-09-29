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

        [Test] public void ConfirmedEncounterRescuesStudentsWhenDefeatedEnemiesWereRemoved()
        {
            _root = new GameObject("CompletedEncounterTest");
            var controller = _root.AddComponent<VarginhaPhase2Controller>();
            var car = new GameObject("Car").transform;
            car.SetParent(_root.transform);
            var students = new VarginhaStudentHostage[9];
            for (int i = 0; i < students.Length; i++)
            {
                var go = new GameObject("Student_" + i);
                go.transform.SetParent(_root.transform);
                students[i] = go.AddComponent<VarginhaStudentHostage>();
            }
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var type = typeof(VarginhaPhase2Controller);
            type.GetField("_fusca", flags).SetValue(controller, car);
            type.GetField("_students", flags).SetValue(controller, students);
            type.GetField("_arrivalFinished", flags).SetValue(controller, true);
            type.GetField("_encounterReady", flags).SetValue(controller, true);
            type.GetMethod("Update", flags).Invoke(controller, null);
            Assert.IsTrue(controller.RescueStarted);
            Assert.IsTrue(students.All(student => student.IsReleased));
            controller.enabled = false;
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            Object.Destroy(_root);
            yield return null;
        }

        [UnityTest] public IEnumerator NineReleasedStudentsWalkOutTheDoorAndReachExternalFusca()
        {
            yield return Rescue(false);
        }

        [UnityTest] public IEnumerator BoardingFinishesWithBlockedSlotAndLeaderLeavingTheRadius()
        {
            yield return Rescue(true);
        }

        private IEnumerator Rescue(bool obstructSlot)
        {
            Time.timeScale = 1;
            _root = new GameObject("SchoolRescueTest");
            VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            var car = new GameObject("RescueCar").transform;
            car.SetParent(_root.transform);
            car.position = VarginhaEnvironmentArt.FuscaParkingPosition;
            var leader = new GameObject("EscortLeader").transform;
            leader.SetParent(_root.transform);
            leader.position = car.position + Vector3.right * 3.8f;
            if (obstructSlot)
            {
                var obstacle = new GameObject("BlockedBoardingSlot");
                obstacle.transform.SetParent(_root.transform);
                obstacle.transform.position = car.position + new Vector3(-1.35f, -.82f);
                obstacle.AddComponent<BoxCollider2D>().size = Vector2.one * .4f;
            }
            Physics2D.SyncTransforms();
            var students = new VarginhaStudentHostage[9];
            var positions = new Vector2[9];
            for (int i = 0; i < 9; i++)
            {
                var go = new GameObject("RescueStudent_" + i);
                go.transform.SetParent(_root.transform);
                go.transform.position = new Vector3(-2.2f + i % 3 * 2.2f, 1.8f - i / 3 * 1.8f);
                go.AddComponent<SpriteRenderer>();
                students[i] = go.AddComponent<VarginhaStudentHostage>();
                students[i].ReleaseTo(car, i, leader);
                positions[i] = go.transform.position;
            }
            float deadline = Time.realtimeSinceStartup + 25;
            yield return null;
            if (obstructSlot) leader.position = car.position + Vector3.right * 7f;
            while (students.Any(student => !student.IsAtFusca) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                for (int i = 0; i < students.Length; i++)
                {
                    Vector2 next = students[i].transform.position;
                    Assert.IsTrue(VarginhaSchoolNavigation.CanWalkSegment(positions[i], next), $"Aluno {i} atravessou uma parede: {positions[i]} -> {next}.");
                    positions[i] = next;
                }
            }
            Assert.AreEqual(9, students.Count(student => student.IsAtFusca));
        }
    }
}
