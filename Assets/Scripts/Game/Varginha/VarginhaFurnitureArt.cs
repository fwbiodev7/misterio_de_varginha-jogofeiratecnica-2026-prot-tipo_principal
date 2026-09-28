using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Shared walnut/teal furniture set, independent of legacy per-object tints.</summary>
    public static class VarginhaFurnitureArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new();
        private static readonly string[] Names = { "Desk", "Chair", "Sofa", "CoffeeTable", "Bookshelf", "Nightstand",
            "Dresser", "Kitchen", "Bed", "SchoolDesk", "Pew", "Altar" };
        private static readonly string[] PropNames = { "TV", "Stove", "Fridge", "Clock", "Lamp", "Radio",
            "WallPicture", "Plant", "Mailbox", "FlowerPatch", "Shrub", "Porch", "Rug", "FuseBox", "Noticeboard", "Door" };

        public static Sprite ForId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string motif = id.StartsWith("SchoolDesk") ? "SchoolDesk"
                : id.StartsWith("CoffeeTable") ? "CoffeeTable"
                : id.StartsWith("Desk") ? "Desk"
                : id.StartsWith("Chair") || id.StartsWith("SchoolChair") ? "Chair"
                : id.StartsWith("Sofa") ? "Sofa"
                : id.StartsWith("Bookshelf") ? "Bookshelf"
                : id.StartsWith("Nightstand") ? "Nightstand"
                : id.StartsWith("Dresser") ? "Dresser"
                : id.StartsWith("Kitchen_Cabinet") ? "Kitchen"
                : id.StartsWith("Bed_") ? "Bed"
                : id.StartsWith("ChurchPew") ? "Pew"
                : id.StartsWith("ChurchAltar") ? "Altar" : null;
            if (motif == null)
            {
                foreach (var name in PropNames)
                    if (id.StartsWith(name + "_") || id == name) { motif = name; break; }
                if (id == "BedroomRug" || id == "KitchenRunner") motif = "Rug";
            }
            return motif == null ? null : Create(motif, Vector2.one);
        }

        public static Sprite Create(string motif, Vector2 size)
        {
            int index = System.Array.IndexOf(Names, motif);
            bool prop = index < 0;
            if (prop) index = System.Array.IndexOf(PropNames, motif);
            if (index < 0) return null;
            // Unit sprites are stretched by legacy scene transforms. Keep 64px of source detail.
            int density = size == Vector2.one ? 64 : 32;
            int width = Mathf.Max(4, Mathf.RoundToInt(size.x * density));
            int height = Mathf.Max(4, Mathf.RoundToInt(size.y * density));
            string key = motif + "_" + width + "x" + height;
            if (Cache.TryGetValue(key, out var cached) && cached != null && cached.texture != null) return cached;
            var source = Resources.Load<Texture2D>(prop ? "Varginha/HousePropsV1" : "Varginha/FurnitureV1");
            if (source == null) return null;
            var colors = source.GetPixels(index % 4 * 64, ((prop ? 3 : 2) - index / 4) * 64, 64, 64);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                pixels[y * width + x] = colors[Mathf.Min(63, (int)((y + .5f) * 64 / height)) * 64
                    + Mathf.Min(63, (int)((x + .5f) * 64 / width))];
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            { name = "Furniture_" + key, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels(pixels);
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * .5f, density,
                0, SpriteMeshType.FullRect);
            sprite.name = texture.name;
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Upgrade only original builder dimensions; authored custom layouts stay untouched.</summary>
        public static void CorrectLegacyProportions(Transform item)
        {
            Vector2 oldSize, newSize;
            switch (item.name)
            {
                case "Desk_Office": oldSize = new(3.7f, 1.8f); newSize = new(2.9f, 1.85f); break;
                case "Bookshelf_Office": oldSize = new(1.2f, 2.4f); newSize = new(1.55f, 1.8f); break;
                case "Chair_Office": oldSize = new(1.25f, 1.15f); newSize = new(.85f, 1.35f); break;
                case "Sofa_LivingRoom": oldSize = new(3.8f, 1.9f); newSize = new(3.2f, 2.1f); break;
                case "Bed_Edelzio": oldSize = new(2.8f, 3.4f); newSize = new(2.4f, 2.8f); break;
                case "TV_StaticNoise": oldSize = new(2.7f, 1.55f); newSize = new(1.9f, 2f); break;
                case "Lamp_Desk": oldSize = new(.75f, .9f); newSize = new(.55f, .6f); break;
                case "WallPicture_Office": oldSize = new(1.4f, .42f); newSize = new(1.3f, 1f); break;
                case "WallPicture_Living": oldSize = new(1.3f, .42f); newSize = new(1.3f, 1f); break;
                default: return;
            }
            if (((Vector2)item.localScale - oldSize).sqrMagnitude > .001f) return;
            item.localScale = new Vector3(newSize.x, newSize.y, item.localScale.z);
            if (item.name == "Lamp_Desk") item.localPosition += new Vector3(-.3f, .3f, 0);
        }
    }
}
