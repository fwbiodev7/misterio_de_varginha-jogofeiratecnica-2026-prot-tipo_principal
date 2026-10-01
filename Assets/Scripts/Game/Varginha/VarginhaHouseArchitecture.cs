using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Stepped house outline inspired by the reference, with a clear route to the yard.</summary>
    public static class VarginhaHouseArchitecture
    {
        public static Rect CameraBounds => Rect.MinMaxRect(-10.25f, -9.25f, 28.25f, 9.25f);

        public static void Apply(Transform house)
        {
            if (house == null || VarginhaHouseReferenceArt.ArchitectureSprite("WallFace") == null) return;
            foreach (string legacy in new[] { "Doorway_Bedroom", "Door_Office_Accessible", "Doorway_Yard" })
                VarginhaOpenPassage.Remove(house, legacy);
            var previous = house.Find("House512_Architecture");
            if (previous != null && previous.Find("RoomsV2") != null)
            {
                ApplyPassages(previous);
                return;
            }
            if (previous != null)
            {
                previous.name += "_Replacing";
                previous.gameObject.SetActive(false);
                if (Application.isPlaying) Object.Destroy(previous.gameObject);
                else Object.DestroyImmediate(previous.gameObject);
            }
            foreach (var renderer in house.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.name.StartsWith("Floor_House")) renderer.enabled = false;
                if (!renderer.name.StartsWith("Wall_") && renderer.name != "Doorway_Bedroom"
                    && renderer.name != "Door_Office_Accessible" && renderer.name != "Doorway_Yard") continue;
                foreach (var visual in renderer.GetComponentsInChildren<SpriteRenderer>(true)) visual.enabled = false;
                foreach (var collider in renderer.GetComponentsInChildren<Collider2D>(true)) collider.enabled = false;
            }
            var root = new GameObject("House512_Architecture").transform;
            root.SetParent(house, false);
            new GameObject("RoomsV2").transform.SetParent(root, false);
            // West wing stops below the taller north wing; the lower center projects as a vestibule.
            for (int x = -9; x < 9; x++)
            for (int y = -8; y < 8; y++)
            {
                if (y >= 6 && x < -2 || y < -6 && (x < -2 || x >= 4)) continue;
                int variant = ((x % 4 + 4) % 4) + ((y % 4 + 4) % 4) * 4;
                Visual(root, "Floor_" + x + "_" + y, VarginhaHouseReferenceArt.FloorSprite(variant),
                    new Vector2(x + .5f, y + .5f), Vector2.one, 0);
            }
            Wall(root, "NorthWest", -5.5f, 6, 7, .7f, "WallFace");
            Wall(root, "NorthStep", -2, 7, .32f, 2, "WallEdge");
            Wall(root, "North", 3.5f, 8, 11, .7f, "WallFace");
            Wall(root, "West", -9, 0, .32f, 12, "WallEdge");
            Wall(root, "EastNorth", 9, 4.65f, .32f, 6.7f, "WallEdge");
            Wall(root, "EastSouth", 9, -3.65f, .32f, 4.7f, "WallEdge");
            Wall(root, "SouthWest", -5.5f, -6, 7, .32f, "WallCap");
            Wall(root, "SouthEast", 6.5f, -6, 5, .32f, "WallCap");
            Wall(root, "VestibuleWest", -2, -7, .32f, 2, "WallEdge");
            Wall(root, "VestibuleEast", 4, -7, .32f, 2, "WallEdge");
            Wall(root, "VestibuleSouth", 1, -8, 6, .32f, "WallCap");
            // Two wall sections leave a real two-meter opening between bedroom and office.
            Wall(root, "BedroomWest", -6.75f, 0, 4.5f, .85f, "WallFace");
            Wall(root, "BedroomEast", -2.25f, 0, .5f, .85f, "WallFace");
            Wall(root, "BedroomSideNorth", -2, 3.975f, .28f, 4.05f, "WallEdge");
            Wall(root, "BedroomSideSouth", -2, .225f, .28f, .45f, "WallEdge");
            Wall(root, "OfficeSideNorth", -2, -1.1f, .28f, 2.2f, "WallEdge");
            Wall(root, "OfficeSideSouth", -2, -4.9f, .28f, 2.2f, "WallEdge");
            Wall(root, "KitchenNorthWest", 1.25f, -1.25f, 2.5f, .65f, "WallFace");
            Wall(root, "KitchenNorthEast", 7.75f, -1.25f, 2.5f, .65f, "WallFace");
            Wall(root, "KitchenSide", 0, -3.625f, .28f, 4.75f, "WallEdge");
            ApplyPassages(root);
            UnityEngine.Physics2D.SyncTransforms();
        }

        private static void ApplyPassages(Transform root)
        {
            VarginhaOpenPassage.Remove(root, "House512_BedroomDoor");
            VarginhaOpenPassage.Remove(root, "House512_OfficeSideDoor");
            VarginhaOpenPassage.Remove(root, "House512_BedroomSideDoor");
            VarginhaOpenPassage.Remove(root, "House512_YardDoor");
            VarginhaOpenPassage.Remove(root, "House512_VestibuleDoor");
            VarginhaOpenPassage.Remove(root, "House512_KitchenThreshold");
            var side = VarginhaHouseReferenceArt.ArchitectureSprite("WallEdge");
            var cap = VarginhaHouseReferenceArt.ArchitectureSprite("WallCap");
            VarginhaOpenPassage.Ensure(root, "Quarto", new(-3.5f, 0), 2, .85f, true, side, cap);
            VarginhaOpenPassage.Ensure(root, "Quarto_Sala", new(-2, 1.2f), 1.5f, .28f, false, cap, side);
            VarginhaOpenPassage.Ensure(root, "Escritorio", new(-2, -3), 1.6f, .28f, false, cap, side);
            VarginhaOpenPassage.Ensure(root, "Cozinha", new(4.5f, -1.25f), 4, .65f, true, side, cap);
            VarginhaOpenPassage.Ensure(root, "Quintal", new(9, 0), 2.6f, .32f, false, cap, side);
            VarginhaOpenPassage.Ensure(root, "Vestibulo", new(1, -6), 6, .32f, true, side, cap);
        }

        private static void Wall(Transform parent, string name, float x, float y, float width, float height, string motif)
        {
            var renderer = Visual(parent, name, VarginhaHouseReferenceArt.ArchitectureSprite(motif),
                new Vector2(x, y), new Vector2(width, height), 3);
            var collider = renderer.gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width / renderer.transform.localScale.x, height / renderer.transform.localScale.y);
        }

        private static SpriteRenderer Visual(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, int order)
        {
            var go = new GameObject("House512_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            bool wall = !name.Contains("Door") && !name.Contains("Floor_") && !name.Contains("Threshold");
            renderer.drawMode = wall ? SpriteDrawMode.Tiled : SpriteDrawMode.Sliced;
            renderer.tileMode = SpriteTileMode.Continuous;
            Vector2 scale = Vector2.one;
            if (wall && size.x > size.y)
                scale = new Vector2(size.y > .4f ? 2 : 1, size.y / sprite.bounds.size.y);
            else if (wall)
                scale = new Vector2(size.x / sprite.bounds.size.x, 2);
            renderer.size = new Vector2(size.x / scale.x, size.y / scale.y);
            go.transform.localScale = new Vector3(scale.x, scale.y, 1);
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
