using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Testa validação de Tilemap no modo Editor.
    /// Cobre: bounds, densidade de tiles, detecção de tilemap vazio.
    /// </summary>
    public class TilemapValidatorTests
    {
        private GameObject _gridGo;
        private Grid _grid;
        private Tilemap _tilemap;
        private TilemapRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _gridGo = new GameObject("Grid");
            _grid = _gridGo.AddComponent<Grid>();

            var tmGo = new GameObject("Tilemap");
            tmGo.transform.SetParent(_gridGo.transform);
            _tilemap = tmGo.AddComponent<Tilemap>();
            _renderer = tmGo.AddComponent<TilemapRenderer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gridGo);
        }

        // ─── Empty Tilemap ────────────────────────────────────────────────────

        [Test]
        public void Tilemap_IsEmpty_Initially()
        {
            Assert.AreEqual(0, _tilemap.GetUsedTilesCount(),
                "Tilemap recém-criado deve ter 0 tiles.");
        }

        [Test]
        public void Tilemap_HasNoTiles_ByDefault()
        {
            _tilemap.CompressBounds();
            var bounds = _tilemap.cellBounds;
            int count = 0;
            foreach (var pos in bounds.allPositionsWithin)
                if (_tilemap.HasTile(pos)) count++;
            Assert.AreEqual(0, count, "Tilemap vazio não deve ter tiles.");
        }

        // ─── Adding Tiles ─────────────────────────────────────────────────────

        [Test]
        public void Tilemap_AfterSetTile_HasTile()
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            _tilemap.SetTile(Vector3Int.zero, tile);
            Assert.IsTrue(_tilemap.HasTile(Vector3Int.zero),
                "Tilemap deve ter o tile na posição definida.");
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void Tilemap_TileCount_IncreasesAfterSet()
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            _tilemap.SetTile(Vector3Int.zero, tile);
            _tilemap.SetTile(new Vector3Int(1, 0, 0), tile);
            _tilemap.SetTile(new Vector3Int(2, 0, 0), tile);
            Assert.AreEqual(3, _tilemap.GetUsedTilesCount(),
                "Contagem de tiles deve aumentar.");
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void Tilemap_SetTile_Null_RemovesTile()
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            _tilemap.SetTile(Vector3Int.zero, tile);
            _tilemap.SetTile(Vector3Int.zero, null);
            Assert.IsFalse(_tilemap.HasTile(Vector3Int.zero),
                "Definir tile como null deve remover o tile.");
            Object.DestroyImmediate(tile);
        }

        // ─── Bounds ───────────────────────────────────────────────────────────

        [Test]
        public void Tilemap_Bounds_UpdateAfterAddingTile()
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            _tilemap.SetTile(new Vector3Int(5, 3, 0), tile);
            _tilemap.CompressBounds();
            Assert.IsTrue(_tilemap.cellBounds.Contains(new Vector3Int(5, 3, 0)),
                "Bounds do tilemap deve conter a posição do tile adicionado.");
            Object.DestroyImmediate(tile);
        }

        // ─── CellToWorld / WorldToCell ────────────────────────────────────────

        [Test]
        public void Grid_CellToWorld_ConvertsCorrectly()
        {
            Vector3 world = _grid.CellToWorld(Vector3Int.zero);
            Assert.AreEqual(Vector3.zero, world, "Célula (0,0,0) deve mapear para world origin.");
        }

        [Test]
        public void Grid_WorldToCell_RoundsCorrectly()
        {
            Vector3Int cell = _grid.WorldToCell(new Vector3(0.4f, 0.4f, 0f));
            Assert.AreEqual(Vector3Int.zero, cell,
                "WorldToCell deve arredondar corretamente para a célula.");
        }

        // ─── Renderer ─────────────────────────────────────────────────────────

        [Test]
        public void TilemapRenderer_ExistsOnTilemapObject()
        {
            Assert.IsNotNull(_renderer,
                "TilemapRenderer deve existir no objeto do Tilemap.");
        }

        [Test]
        public void TilemapRenderer_IsEnabled_ByDefault()
        {
            Assert.IsTrue(_renderer.enabled,
                "TilemapRenderer deve estar habilitado por padrão.");
        }

        // ─── Collision / TilemapCollider2D ────────────────────────────────────

        [Test]
        public void TilemapCollider2D_CanBeAdded()
        {
            var col = _tilemap.gameObject.AddComponent<TilemapCollider2D>();
            Assert.IsNotNull(col, "TilemapCollider2D deve poder ser adicionado ao tilemap.");
        }
    }
}
