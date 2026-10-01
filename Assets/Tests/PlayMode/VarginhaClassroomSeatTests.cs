using System.Collections;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public class VarginhaClassroomSeatTests
    {
        private GameObject _root;
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = 1;
            Object.Destroy(_root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SitAndStandAtDeskRestoresMovementAndFurnitureCollision()
        {
            Time.timeScale = 1;
            _root = new GameObject("ClassroomSeatTest");
            var school = VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            var seat = school.Find("CenarioV2_Cadeira_1").GetComponent<VarginhaClassroomSeat>();
            var go = new GameObject("SeatedPlayer"); go.transform.SetParent(_root.transform);
            go.transform.position = seat.ExitPosition;
            go.AddComponent<SpriteRenderer>(); go.AddComponent<CircleCollider2D>().radius = .45f;
            var player = go.AddComponent<EdelzioTopDownController>();
            var animation = go.AddComponent<VarginhaPlayerSpriteAnimation>();
            var action = go.AddComponent<VarginhaPlayerActionAnimation>();
            Vector3 standing = go.transform.position;
            var collider = go.GetComponent<Collider2D>();
            action.PlayChurchSeat(seat.transform);
            yield return new WaitForSeconds(1.5f);
            Assert.IsTrue(animation.IsSeated, "The player must reach the chair beside the desk.");
            Assert.IsTrue(seat.IsOccupied);
            Assert.That(Vector2.Distance(go.transform.position, seat.SeatedPosition), Is.LessThan(.16f));
            action.FinishSeatSession();
            yield return new WaitForSeconds(1.5f);
            Assert.IsFalse(animation.IsSeated);
            Assert.IsFalse(seat.IsOccupied);
            Assert.IsFalse(player.IsInputLocked);
            Assert.IsFalse(Physics2D.GetIgnoreCollision(collider, seat.GetComponent<Collider2D>()));
            Assert.IsFalse(Physics2D.GetIgnoreCollision(collider, seat.DeskCollider));
            Assert.That(Vector2.Distance(go.transform.position, standing), Is.LessThan(.16f));
        }
    }
}
