using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>512px authored house art. Applies only to the first-stage house scope.</summary>
    public static class VarginhaHouseReferenceArt
    {
        private const string Resource = "Varginha/HouseReference512/";
        private static readonly Dictionary<string, Sprite> Sprites = new();
        private static readonly string[] Motifs = { "Desk", "Chair", "Sofa", "CoffeeTable", "Bookshelf",
            "Nightstand", "Dresser", "Kitchen", "Bed", "TV", "Stove", "Fridge", "Lamp", "Plant", "Rug", "Door" };

        private static Sprite Load(string sheet, string name)
        {
            if (Sprites.TryGetValue(name, out var sprite) && sprite != null) return sprite;
            foreach (var item in Resources.LoadAll<Sprite>(Resource + sheet)) Sprites[item.name] = item;
            return Sprites.TryGetValue(name, out sprite) ? sprite : null;
        }

        // Animation frames share the same atlas as the saved house, including after domain reload.
        public static Sprite PropSprite(Component owner, string id)
        {
            if (owner == null) return null;
            bool inHouse = owner.gameObject.scene.name == "FaseTopView_Varginha";
            for (var parent = owner.transform; parent != null && !inHouse; parent = parent.parent)
                inHouse = parent.name == "House_And_Yard" || parent.Find("House_And_Yard") != null;
            return inHouse ? Prop(id) : null;
        }

        public static Sprite ArchitectureSprite(string motif) => Load("Architecture", "House512_" + motif);
        public static Sprite FloorSprite(int variant) => Load("Floor", "House512_Floor_" + variant);
        public static Sprite EntranceSprite() => Load("Furniture", "House512_Door");
        public static Sprite YardSprite(string motif) => Load("Yard", "House512_Yard" + motif);

        private static Sprite Prop(string id)
        {
            string motif = null;
            if (id.StartsWith("Janela_Casa_") || id.StartsWith("Window_House")) motif = "Window";
            else if (id == "ToyBox_Open") motif = "ChestOpen";
            else if (id == "ToyBox_Opening") motif = "ChestOpening";
            else if (id.StartsWith("ToyBox_")) motif = "ChestClosed";
            else if (id == "Coffee_Empty") motif = "CupEmpty";
            else if (id.StartsWith("Coffee_")) motif = "CupFull";
            else if (id.StartsWith("Backpack_")) motif = "Backpack";
            else if (id == "Notebook_TI") motif = "Notebook";
            else if (id.StartsWith("Radio_")) motif = "Radio";
            else if (id.StartsWith("Clock_")) motif = "Clock";
            else if (id.StartsWith("WallPicture_")) motif = "WallPicture";
            else if (id.StartsWith("FuseBox_")) motif = "FuseBox";
            else if (id == "Memorias_1996") motif = "Noticeboard";
            else if (id.StartsWith("Doc_")) motif = "Document";
            return motif != null ? Load("Props", "House512_" + motif) : null;
        }

        public static void Apply(Transform house)
        {
            if (house == null) return;
            foreach (var renderer in VarginhaHouseComposition.Scope(house).GetComponentsInChildren<SpriteRenderer>(true))
            {
                string id = renderer.name;
                if (id.StartsWith("House512_")) continue;
                if (renderer.sprite != null && (renderer.sprite.name == "House512_ChestOpen"
                    || renderer.sprite.name == "House512_ChestOpening" || renderer.sprite.name == "House512_CupEmpty")) continue;
                Sprite replacement = null;
                if (id.StartsWith("Floor_House"))
                {
                    // Four staggered board crops in each direction avoid one identical floor stamp.
                    int x = Mathf.FloorToInt(renderer.transform.position.x);
                    int y = Mathf.FloorToInt(renderer.transform.position.y);
                    int variant = ((x % 4 + 4) % 4) + ((y % 4 + 4) % 4) * 4;
                    replacement = Load("Floor", "House512_Floor_" + variant);
                }
                else if (id.StartsWith("Wall_"))
                {
                    if (house.Find("House512_Architecture") != null) continue;
                    ApplyWall(renderer);
                    continue;
                }
                else
                {
                    string motif = Motif(id);
                    replacement = Prop(id);
                    if (id == "CoffeeTable_Living") replacement = Load("Furniture", "House512_CoffeeTable");
                    else if (id.StartsWith("Balcao_Sala") || id == "CoffeeTable_Kitchen") replacement = Load("Props", "House512_DiningTable");
                    else if (id.StartsWith("CoffeeTable_")) replacement = Load("Furniture", "House512_CoffeeTable");
                    if (motif == "Sofa") replacement = Load("Furniture", "House512_Sofa");
                    if (id.StartsWith("Banco_Balcao_") || id.StartsWith("Cadeira_Sala_") || id.StartsWith("Cadeira_Mesa_"))
                        replacement = Load("Props", "House512_DiningChair");
                    if (id == "Chair_Office") replacement = Load("Chairs", "House512_ChairNorth");
                    if (id == "Cadeira_Cafe") replacement = Load("Chairs", "House512_ChairEast");
                    if (replacement == null && motif != null) replacement = Load("Furniture", "House512_" + motif);
                }
                if (replacement == null || renderer.sprite == replacement) continue;
                // Preserve the existing visual footprint, including decor authored at scale one.
                Vector2 oldBounds = renderer.drawMode != SpriteDrawMode.Simple ? renderer.size
                    : renderer.sprite != null ? renderer.sprite.bounds.size : Vector2.one;
                Vector3 authoredScale = renderer.transform.localScale;
                renderer.sprite = replacement;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = oldBounds;
                // Unity compensates transform scale when switching draw modes. Keep collider geometry intact.
                renderer.transform.localScale = authoredScale;
                renderer.color = Color.white;
            }
        }

        private static string Motif(string id)
        {
            if (id == "BedroomRug" || id == "KitchenRunner" || id.StartsWith("Tapete_")) return "Rug";
            if (id == "Abajur_Cabeceira") return "Lamp";
            if (id == "Livros_Quarto") return "Bookshelf";
            if (id == "Aparador_Arquivo") return "Dresser";
            if (id == "Cadeira_Cafe" || id == "Cadeira_Notebook") return "Chair";
            if (id.StartsWith("Samambaia_") || id.StartsWith("Planta_")) return "Plant";
            foreach (string motif in Motifs)
                if (id == motif || id.StartsWith(motif + "_")) return motif;
            return null;
        }

        private static void ApplyWall(SpriteRenderer original)
        {
            var source = Load("Wall", "House512_Wall");
            if (source == null) return;
            var wall = original.transform;
            if (wall.Find("House512_WallSurface") != null) { original.enabled = false; return; }
            Vector2 dimensions = Vector2.Scale(original.sprite.bounds.size, wall.localScale);
            bool vertical = dimensions.y > dimensions.x;
            // The new child carries only the art; the existing wall and its collider keep their dimensions.
            var child = new GameObject("House512_WallSurface").transform;
            child.SetParent(wall, false);
            var renderer = child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = vertical ? Load("Floor", "House512_Floor_0") : source;
            if (renderer.sprite == null) renderer.sprite = source;
            renderer.sortingLayerID = original.sortingLayerID;
            renderer.sortingOrder = original.sortingOrder;
            renderer.sharedMaterial = original.sharedMaterial;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            child.localScale = vertical
                ? new Vector3(dimensions.x / wall.localScale.x, 1f / wall.localScale.y, 1)
                : new Vector3(1f / wall.localScale.x, dimensions.y / wall.localScale.y, 1);
            renderer.size = vertical ? new Vector2(1, dimensions.y) : new Vector2(dimensions.x, 1);
            renderer.color = vertical ? new Color(.55f, .48f, .40f) : Color.white;
            original.enabled = false;
        }
    }
}
