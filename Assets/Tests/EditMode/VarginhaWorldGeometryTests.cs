using Game.Varginha;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Tests.EditMode
{
    public class VarginhaWorldGeometryTests
    {
        private GameObject _root;
        [SetUp] public void Setup() => _root = new GameObject("WorldGeometryTest");
        [TearDown] public void Cleanup() => Object.DestroyImmediate(_root);

        private SpriteRenderer Prop(string name, Vector2 size, Vector2 position = default)
        {
            var go = new GameObject(name); go.transform.SetParent(_root.transform);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create("Dust", Color.white);
            sr.drawMode = SpriteDrawMode.Sliced; sr.size = size; sr.sortingOrder = 3;
            return sr;
        }

        [Test] public void PostsStayBehindPlayerAndOnlyTheirBaseBlocksWalking()
        {
            var post = Prop("Poste_Noturno_0", new(.65f, 2.3f));
            var player = Prop("Player", Vector2.one);
            VarginhaWorldDepth.Ensure(player, true);
            VarginhaWorldGeometry.Ensure(_root.transform);
            var box = post.GetComponentInChildren<BoxCollider2D>();
            Assert.IsNotNull(box); Assert.IsFalse(box.isTrigger);
            Assert.Less(box.bounds.size.y, .3f);
            Assert.Less(box.bounds.max.y, post.bounds.center.y);
            Assert.Less(post.GetComponent<SortingGroup>().sortingOrder, player.GetComponent<SortingGroup>().sortingOrder);
            Assert.IsNotNull(Physics2D.OverlapPoint(box.bounds.center));
            Assert.IsNull(Physics2D.OverlapPoint(post.bounds.center + Vector3.up * .6f));
        }

        [Test] public void FurnitureChangesDepthWhenPlayerWalksAroundItAndAttachmentsStayTogether()
        {
            var desk = Prop("Desk_Office", new(2.85f, 1.7f));
            var player = Prop("Player", Vector2.one);
            var depth = VarginhaWorldDepth.Ensure(player, true);
            var hand = new GameObject("HeldItem"); hand.transform.SetParent(player.transform);
            hand.AddComponent<SpriteRenderer>().sortingOrder = player.sortingOrder + 2;
            VarginhaWorldGeometry.Ensure(_root.transform);
            Assert.IsNotNull(desk.GetComponentInChildren<BoxCollider2D>());
            player.transform.position = Vector3.up; depth.Refresh();
            Assert.Less(player.GetComponent<SortingGroup>().sortingOrder, desk.GetComponent<SortingGroup>().sortingOrder);
            player.transform.position = Vector3.down; depth.Refresh();
            Assert.Greater(player.GetComponent<SortingGroup>().sortingOrder, desk.GetComponent<SortingGroup>().sortingOrder);
            Assert.AreSame(player.GetComponent<SortingGroup>(), hand.GetComponentInParent<SortingGroup>());
        }

        [Test] public void TableAccessoriesUseTheTablesDepthAndMigrationIsIdempotent()
        {
            var desk = Prop("Desk_Office", new(2.85f,1.7f));
            var cup = Prop("Coffee_Cup", new(.18f,.18f), new(0,.3f));
            var floor = Prop("Floor_Yard", new(10,10)); floor.sortingOrder = 0;
            VarginhaWorldGeometry.Ensure(_root.transform);
            int colliders = _root.GetComponentsInChildren<Collider2D>().Length;
            int groups = _root.GetComponentsInChildren<SortingGroup>().Length;
            VarginhaWorldGeometry.Ensure(_root.transform);
            Assert.AreEqual(colliders,_root.GetComponentsInChildren<Collider2D>().Length);
            Assert.AreEqual(groups,_root.GetComponentsInChildren<SortingGroup>().Length);
            Assert.Greater(cup.GetComponent<SortingGroup>().sortingOrder,desk.GetComponent<SortingGroup>().sortingOrder);
            Assert.IsNull(cup.GetComponent<Collider2D>());
            Assert.IsNull(floor.GetComponent<VarginhaWorldDepth>());
        }

        [Test] public void SolidPropsInHouseSchoolAndChurchReceiveFootprints()
        {
            string[] names = { "Nightstand_Bedroom", "Dresser_Bedroom", "Fridge_Kitchen", "Plant_Indoor",
                "Livros_Quarto", "Samambaia_Sala", "Mailbox_Yard", "Shrub_NorthWest", "CenarioV2_Carteira_0",
                "CenarioV2_Estante_Informatica", "Banco_Jardim", "CenarioV2_Altar_Visual", "Arquivo_Sacristia" };
            for(int i=0;i<names.Length;i++) Prop(names[i],new(1,1.5f),new(i*2,0));
            VarginhaWorldGeometry.Ensure(_root.transform);
            foreach(string name in names)
            {
                var prop=_root.transform.Find(name);
                Assert.IsNotNull(prop.GetComponentInChildren<Collider2D>(),name);
                Assert.IsNotNull(prop.GetComponent<SortingGroup>(),name);
            }
        }

        [Test] public void IndustrialFacadeIsPackagedAndLeavesTheSchoolEntranceClear()
        {
            var school=VarginhaEnvironmentArt.EnsureSchool(_root.transform);
            VarginhaWorldGeometry.Ensure(_root.transform);
            var facade=school.GetComponentInChildren<VarginhaIndustrialSchoolFacade>();
            Assert.IsNotNull(facade);
            var panels=facade.GetComponentsInChildren<SpriteRenderer>();
            Assert.AreEqual(2,panels.Length);
            foreach(var panel in panels) { Assert.AreEqual(FilterMode.Point,panel.sprite.texture.filterMode); Assert.AreEqual(0,panel.sprite.pivot.y); }
            Vector2 entrance=VarginhaSchoolExterior.Entrance;
            Assert.IsEmpty(Physics2D.CircleCastAll(entrance+Vector2.down*2,.49f,Vector2.up,4));
            Assert.IsNotEmpty(Physics2D.CircleCastAll(new(-4,-7),.49f,Vector2.up,3));
            facade.ShowInterior(true);
            foreach(var panel in panels)Assert.IsFalse(panel.enabled);
            facade.ShowInterior(false);
            foreach(var panel in panels)Assert.IsTrue(panel.enabled);
        }
    }
}
