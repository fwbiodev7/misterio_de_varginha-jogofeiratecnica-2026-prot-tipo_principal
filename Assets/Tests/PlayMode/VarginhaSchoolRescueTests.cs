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

        [UnityTest]
        public IEnumerator ReleasedStudentGoesToTheCarAndWaitsWhileLeaderIsAway()
        {
            Time.timeScale = 1;
            _root = new GameObject("InteriorDoorFollowTest");
            VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            var car = new GameObject("TestCar").transform;
            car.SetParent(_root.transform);
            car.position = VarginhaEnvironmentArt.FuscaParkingPosition;
            var leader = new GameObject("TestLeader").transform;
            leader.SetParent(_root.transform);
            leader.position = car.position + Vector3.right * 9f;
            var studentObject = new GameObject("TestStudent");
            studentObject.transform.SetParent(_root.transform);
            studentObject.transform.position = new Vector3(-7f, -4.8f);
            var student = studentObject.AddComponent<VarginhaStudentHostage>();
            student.ReleaseTo(car, 0, leader);
            Assert.AreEqual(RigidbodyType2D.Dynamic, student.GetComponent<Rigidbody2D>().bodyType);
            Assert.IsFalse(student.GetComponent<CircleCollider2D>().isTrigger);

            float deadline = Time.realtimeSinceStartup + 15f;
            while (!student.IsAtFusca && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(student.IsAtFusca, "O aluno deve ir até o Fusca sem esperar Edelzio.");
            Vector3 parked = student.transform.position;

            yield return new WaitForSeconds(1f);

            Assert.That(Vector2.Distance(student.transform.position, parked), Is.LessThan(.05f),
                "O aluno deve esperar no Fusca até Edelzio chegar.");
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
                go.transform.position = VarginhaSchoolClassroomLayout.StudentPosition(i);
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
