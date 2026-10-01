using System.Linq;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class VarginhaSchoolReferenceTests
    {
        private GameObject _root;
        [TearDown] public void Cleanup() { if (_root != null) Object.DestroyImmediate(_root); }

        [Test]
        public void ImportedAtlasHasNamedFurnitureInsteadOfAutomaticSlices()
        {
            CollectionAssert.AreEquivalent(new[] { "Desk", "ComputerDesk", "Chair", "Whiteboard", "Window", "Shelf", "Projector", "TeacherDesk" },
                Resources.LoadAll<Sprite>("Varginha/SchoolComputerLab").Select(s => s.name));
        }

        [Test]
        public void ReferenceLayoutKeepsComputersAndRescueAisles()
        {
            _root = new GameObject("SchoolReferenceTest");
            var school = VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            var art = school.GetComponentsInChildren<SpriteRenderer>();
            Assert.AreEqual(9, art.Count(r => r.sprite != null && r.sprite.name == "ComputerDesk"));
            Assert.AreEqual(3, art.Count(r => r.sprite != null && r.sprite.name == "Desk"));
            Assert.AreEqual(12, art.Count(r => r.sprite != null && r.sprite.name == "SchoolChairRearV1"));
            Physics2D.SyncTransforms();
            for (int i = 0; i < 9; i++)
            {
                var position = VarginhaSchoolClassroomLayout.StudentPosition(i);
                Assert.IsFalse(school.GetComponentsInChildren<Collider2D>().Any(c => !c.isTrigger && c.OverlapPoint(position)), "Blocked cage " + i);
            }
        }

        [Test]
        public void GlobalFurnitureRefreshNeverEnlargesComputerLabFootprints()
        {
            _root = new GameObject("SchoolCollisionRegression");
            var school = VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            for (int repeat = 0; repeat < 3; repeat++)
            {
                VarginhaEnvironmentPolish.RefreshReferenceFurniture(_root.transform);
                VarginhaSchoolClassroomLayout.RefreshSprites(school);
                Physics2D.SyncTransforms();
                foreach (var renderer in school.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (!renderer.name.StartsWith("CenarioV2_Carteira_")) continue;
                    Assert.That(renderer.GetComponent<BoxCollider2D>().bounds.size.x, Is.EqualTo(1.55f).Within(.001f));
                    Assert.That(renderer.bounds.size.x, Is.EqualTo(1.72f).Within(.001f));
                }
                // The former oversized boxes joined neighboring desks across this aisle.
                Assert.IsTrue(VarginhaSchoolNavigation.CanWalkSegment(new Vector2(-4.45f, -4.3f), new Vector2(-4.45f, 4.1f)));
                Assert.IsTrue(VarginhaSchoolNavigation.CanWalkSegment(new Vector2(.75f, -10), new Vector2(.75f, 3.9f)));
            }
        }

        [Test]
        public void ClassroomChairSeatsOneStudentAndReleasesThemIntoClearAisle()
        {
            _root = new GameObject("SchoolSeatRegression");
            var school = VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            var seat = school.Find("CenarioV2_Cadeira_0").GetComponent<VarginhaClassroomSeat>();
            var go = new GameObject("Student"); go.transform.SetParent(_root.transform);
            go.AddComponent<SpriteRenderer>();
            var student = go.AddComponent<VarginhaStudentHostage>(); student.Configure("Matias", Color.white);
            Assert.IsTrue(seat.SitStudent(student));
            Assert.IsTrue(go.GetComponent<VarginhaStudentAnimation>().IsSeated);
            Assert.AreEqual(3, go.GetComponent<VarginhaStudentAnimation>().Facing);
            Assert.IsFalse(seat.GetComponent<InteractableProp>().CanInteract);
            var another = new GameObject("Other"); another.transform.SetParent(_root.transform);
            Assert.IsFalse(seat.Reserve(another.transform));
            seat.Vacate(student);
            Assert.IsFalse(seat.IsOccupied);
            Assert.IsFalse(go.GetComponent<VarginhaStudentAnimation>().IsSeated);
            Physics2D.SyncTransforms();
            Assert.IsFalse(seat.GetComponent<Collider2D>().OverlapPoint(go.transform.position));
            Assert.IsTrue(seat.GetComponent<InteractableProp>().CanInteract);
        }

        [Test]
        public void NightLeavesInteriorClearAndPreservesLampProportions()
        {
            _root = new GameObject("NightReferenceTest");
            VarginhaOutdoorNight.EnsureSchool(_root.transform);
            var night = _root.transform.Find(VarginhaOutdoorNight.RootName);
            var lamps = night.GetComponentsInChildren<SpriteRenderer>().Where(r => r.name.StartsWith("Poste_Noturno_")).ToArray();
            Assert.AreEqual(4, lamps.Length);
            foreach (var lamp in lamps)
            {
                Assert.AreEqual("OutdoorLanternPost", lamp.sprite.name);
                Assert.That(lamp.transform.localScale.x, Is.EqualTo(lamp.transform.localScale.y).Within(.0001f));
            }
            var mask = night.Find("Sombra_Noturna").GetComponent<SpriteRenderer>().sprite;
            Color Sample(Vector2 world) => mask.texture.GetPixel(Mathf.FloorToInt((world.x + 23) * 20), Mathf.FloorToInt((world.y + 15) * 20));
            Assert.That(Sample(Vector2.zero).a, Is.LessThan(.01f));
            Assert.That(Sample(new Vector2(20, -12)).a, Is.GreaterThan(.6f));
            Assert.That(Sample(new Vector2(-9.1f, -9)).a, Is.LessThan(.15f));
        }
    }
}
