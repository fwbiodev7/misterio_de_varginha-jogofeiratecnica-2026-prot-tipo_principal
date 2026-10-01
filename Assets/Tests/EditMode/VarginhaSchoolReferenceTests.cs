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
            Assert.AreEqual(12, art.Count(r => r.sprite != null && r.sprite.name == "Chair"));
            Physics2D.SyncTransforms();
            for (int i = 0; i < 9; i++)
            {
                var position = VarginhaSchoolClassroomLayout.StudentPosition(i);
                Assert.IsFalse(school.GetComponentsInChildren<Collider2D>().Any(c => !c.isTrigger && c.OverlapPoint(position)), "Blocked cage " + i);
            }
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
            Assert.That(Sample(new Vector2(-7.8f, -9)).a, Is.LessThan(.15f));
        }
    }
}
