using System.Collections.Generic;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class VarginhaSchoolExteriorTests
    {
        private GameObject _root;
        private Transform _school;

        [SetUp] public void Setup()
        {
            _root = new GameObject("ExteriorTest");
            _school = VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            Physics2D.SyncTransforms();
        }

        [TearDown] public void Cleanup() => Object.DestroyImmediate(_root);

        [Test] public void ParkingIsOutsideAndFullSizePlayerCanWalkThroughDoor()
        {
            var parking = _school.Find("CenarioV2_Vaga_Fusca").GetComponent<SpriteRenderer>().bounds;
            var south = _school.Find("CenarioV2_Parede_Sul").GetComponent<Collider2D>().bounds;
            Assert.Less(parking.max.y, south.min.y);
            Vector2 door = VarginhaSchoolExterior.Entrance;
            Assert.IsEmpty(Physics2D.CircleCastAll(door + Vector2.down * 2, .49f, Vector2.up, 4),
                "A passagem deve acomodar o colisor real do jogador.");
            Assert.IsNotEmpty(Physics2D.CircleCastAll(new Vector2(-4, -7), .49f, Vector2.up, 3),
                "A fachada permanece sólida fora da porta.");
        }

        [Test] public void AllNineStudentsHaveWallFreeRoutesThroughTheEntrance()
        {
            for (int i = 0; i < 9; i++)
            {
                Vector2 start = new(-2.2f + i % 3 * 2.2f, 1.8f - i / 3 * 1.8f);
                Vector2 finish = (Vector2)VarginhaEnvironmentArt.FuscaParkingPosition + new Vector2(-1.35f - i % 3 * .7f, (i / 3 - 1) * .82f);
                var path = new List<Vector2>();
                VarginhaSchoolNavigation.FindPath(_school, start, finish, path);
                Assert.IsNotEmpty(path, "Aluno " + i);
                Vector2 previous = start;
                bool crossedDoor = false;
                foreach (var point in path)
                {
                    Assert.IsTrue(VarginhaSchoolNavigation.CanWalkSegment(previous, point), "Rota atravessa parede: " + i);
                    if (previous.y >= -5.7f && point.y < -5.7f)
                    {
                        Assert.That(point.x, Is.InRange(-.2f, 1.7f));
                        crossedDoor = true;
                    }
                    previous = point;
                }
                Assert.IsTrue(crossedDoor);
                Assert.That(Vector2.Distance(previous, finish), Is.LessThan(.01f));
            }
        }
    }
}
