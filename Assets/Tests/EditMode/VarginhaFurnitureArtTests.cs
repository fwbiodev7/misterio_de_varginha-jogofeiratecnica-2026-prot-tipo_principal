using System.Linq;
using Game.Varginha;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class VarginhaFurnitureArtTests
    {
        [Test]
        public void CompositionIncludesSiblingPropsAndDoesNotMoveThemTwice()
        {
            var root = new GameObject("HouseCompositionTest");
            try
            {
                var house = new GameObject("House_And_Yard").transform;
                house.SetParent(root.transform);
                var props = new GameObject("Investigation_Props").transform;
                props.SetParent(root.transform);
                var desk = new GameObject("Desk_Office", typeof(SpriteRenderer), typeof(BoxCollider2D));
                desk.transform.SetParent(props);
                desk.transform.position = new Vector3(-5, -3.5f);
                desk.transform.localScale = new Vector3(3.7f, 1.8f, 1);
                var notebook = new GameObject("Notebook_TI", typeof(BoxCollider2D));
                notebook.transform.SetParent(props);
                notebook.transform.position = desk.transform.position;
                notebook.GetComponent<BoxCollider2D>().isTrigger = true;
                var custom = new GameObject("Sofa_LivingRoom");
                custom.transform.SetParent(props);
                custom.transform.position = new Vector3(20, 20);

                VarginhaEnvironmentPolish.EnsureHouse(house);
                var positions = root.GetComponentsInChildren<Transform>().Select(t => t.position).ToArray();
                VarginhaEnvironmentPolish.EnsureHouse(house);
                CollectionAssert.AreEqual(positions, root.GetComponentsInChildren<Transform>().Select(t => t.position).ToArray());
                Assert.Greater(notebook.transform.position.y, desk.transform.position.y);
                Assert.Less(Mathf.Abs(notebook.transform.position.x - desk.transform.position.x), .1f);
                Assert.IsTrue(notebook.GetComponent<BoxCollider2D>().isTrigger);
                Assert.IsFalse(desk.GetComponent<BoxCollider2D>().isTrigger);
                Assert.AreEqual(new Vector3(20, 20), custom.transform.position);
                Assert.AreEqual(props, notebook.transform.parent);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void SchoolFurnitureClearsPartitionAndChairsStayPaired()
        {
            var root = new GameObject("SchoolCompositionTest");
            try
            {
                var school = VarginhaEnvironmentArt.EnsureSchool(root.transform);
                var wall = school.Find("CenarioV2_Divisoria_Sala").GetComponent<SpriteRenderer>().bounds;
                foreach (int index in new[] { 4, 5 })
                {
                    var desk = school.Find("CenarioV2_Carteira_" + index).GetComponent<SpriteRenderer>();
                    var chair = school.Find("CenarioV2_Cadeira_" + index).GetComponent<SpriteRenderer>();
                    Assert.Greater(desk.bounds.min.y, wall.max.y);
                    Assert.Greater(chair.bounds.min.y, wall.max.y);
                    Assert.AreEqual(desk.transform.position.x, chair.transform.position.x);
                }
                var board = school.Find("CenarioV2_Lousa_Fundo").GetComponent<SpriteRenderer>();
                var partition = school.Find("CenarioV2_Divisoria_Fundo").GetComponent<SpriteRenderer>();
                Assert.Greater(board.sortingOrder, partition.sortingOrder, "A lousa deve aparecer na frente da divisória.");
                foreach (int index in new[] { 8, 9, 10 })
                    Assert.Greater(board.bounds.min.y,
                        school.Find("CenarioV2_Carteira_" + index).GetComponent<SpriteRenderer>().bounds.max.y);
                var position = board.transform.position;
                VarginhaEnvironmentPolish.EnsureSchool(school);
                Assert.AreEqual(position, board.transform.position);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [TestCase("Desk_Office")][TestCase("Chair_Office")][TestCase("Sofa_LivingRoom")]
        [TestCase("CoffeeTable_Living")][TestCase("Bookshelf_Office")][TestCase("Nightstand_Bedroom")]
        [TestCase("Dresser_Bedroom")][TestCase("Kitchen_Cabinet")][TestCase("Bed_Edelzio")]
        [TestCase("SchoolDesk")][TestCase("ChurchPew")][TestCase("ChurchAltar")]
        [TestCase("TV_StaticNoise")][TestCase("Stove_Kitchen")][TestCase("Fridge_Kitchen")]
        [TestCase("WallPicture_Office")][TestCase("Mailbox_Yard")][TestCase("Porch_Wood")]
        [TestCase("FlowerPatch_Left")][TestCase("Shrub_NorthWest")][TestCase("Lamp_Desk")]
        public void FurnitureUsesSharedPaletteAndRetainsUnitGeometry(string id)
        {
            var sprite = VarginhaPixelArtSprites.Create(id, Color.magenta);
            Assert.That(sprite.name, Does.StartWith("Furniture_"));
            Assert.AreEqual(Vector2.one, (Vector2)sprite.bounds.size);
            Assert.AreEqual(FilterMode.Point, sprite.texture.filterMode);
            Assert.AreSame(sprite, VarginhaPixelArtSprites.Create(id, Color.green));
            var pixels = sprite.texture.GetPixels32();
            Assert.Greater(pixels.Count(p => p.a > 128), 50);
            Assert.Greater(pixels.Count(p => p.a == 0), 100);
            Assert.AreEqual(0, pixels[0].a);
            Assert.AreEqual(0, pixels[pixels.Length - 1].a);
        }

        [Test]
        public void RefreshReplacesSavedHouseFurnitureWithoutChangingPhysics()
        {
            var root = new GameObject("House_And_Yard");
            try
            {
                foreach (var id in new[] { "Desk_Office", "Chair_Office", "Sofa_LivingRoom", "Bed_Edelzio" })
                {
                    var go = new GameObject(id, typeof(SpriteRenderer), typeof(BoxCollider2D));
                    go.transform.SetParent(root.transform);
                    go.transform.localScale = new Vector3(2, 1.3f, 1);
                    go.GetComponent<BoxCollider2D>().size = new Vector2(.8f, .5f);
                }
                VarginhaEnvironmentPolish.EnsureHouse(root.transform);
                foreach (Transform child in root.transform)
                {
                    var collider = child.GetComponent<BoxCollider2D>();
                    if (collider == null) continue;
                    Assert.AreEqual(new Vector3(2, 1.3f, 1), child.localScale);
                    Assert.AreEqual(new Vector2(.8f, .5f), collider.size);
                    Assert.That(child.GetComponent<SpriteRenderer>().sprite.name, Does.StartWith("Furniture_"));
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void LegacyProportionsAreCorrectedOnceAndCollidersFollowTheFurniture()
        {
            var desk = new GameObject("Desk_Office", typeof(BoxCollider2D));
            try
            {
                desk.transform.localScale = new Vector3(3.7f, 1.8f, 1);
                desk.GetComponent<BoxCollider2D>().size = new Vector2(.8f, .6f);
                VarginhaFurnitureArt.CorrectLegacyProportions(desk.transform);
                Assert.AreEqual(new Vector3(2.9f, 1.85f, 1), desk.transform.localScale);
                Assert.AreEqual(new Vector2(.8f, .6f), desk.GetComponent<BoxCollider2D>().size);
                VarginhaFurnitureArt.CorrectLegacyProportions(desk.transform);
                Assert.AreEqual(new Vector3(2.9f, 1.85f, 1), desk.transform.localScale);
            }
            finally { Object.DestroyImmediate(desk); }
        }
    }
}
