using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Composes the original house as functional furniture groups, including sibling investigation props.</summary>
    public static class VarginhaHouseComposition
    {
        public static Transform Scope(Transform house) => house.parent != null &&
            house.parent.Find("Investigation_Props") != null ? house.parent : house;

        public static void Apply(Transform house)
        {
            if (house == null) return;
            var pieces = new Dictionary<string, Transform>();
            foreach (var item in Scope(house).GetComponentsInChildren<Transform>(true))
                pieces.TryAdd(item.name, item);

            // Only migrate the shipped positions. Custom placements and collected props stay intact.
            Place(pieces, "Bed_Edelzio", -6.5f, 5, -6.3f, 4.9f, 1.9f, 2.2f);
            Place(pieces, "Nightstand_Bedroom", -3.2f, 5.15f, -4.55f, 5.35f, .7f, .8f);
            Place(pieces, "Dresser_Bedroom", -7.65f, 1.15f, -7.5f, 1.65f, 1.2f, 1.15f);
            Place(pieces, "BedroomRug", -5.25f, 2.1f, -5.45f, 2.15f, 3.4f, 1.8f);
            Place(pieces, "ToyBox_UnderBed", -6.5f, 3.2f, -6.3f, 3.15f, 1, .8f);
            Place(pieces, "Backpack_Prop", -3, 2, -3.35f, 2.65f, .9f, .9f);

            // Desk accessories sit on the rear half of the tabletop, above its apron.
            Place(pieces, "Desk_Office", -5, -3.5f, -5, -3.8f, 2.5f, 1.5f);
            Place(pieces, "Chair_Office", -5, -2.25f, -5, -2.3f, .65f, .9f);
            Place(pieces, "Notebook_TI", -5, -3.5f, -5, -3.3f, .85f, .65f);
            Place(pieces, "Radio_Office", -6.9f, -3.35f, -6.02f, -3.3f, .65f, .48f);
            Place(pieces, "Lamp_Desk", -4, -3, -3.95f, -3.16f, .52f, .62f);
            Place(pieces, "Bookshelf_Office", -7.3f, -5.3f, -7.35f, -5.35f, 1.35f, 1.6f);
            Place(pieces, "Doc_Historical", -2, -5.5f, -2.25f, -5.02f, .5f, .5f);
            Place(pieces, "WallPicture_Office", -7, -1.25f, -7.2f, -.85f, 1.3f, 1);

            Place(pieces, "Sofa_LivingRoom", 4.5f, 3.55f, 4.6f, 3.55f, 2.55f, 1.45f);
            Place(pieces, "TV_StaticNoise", 4.5f, 5.45f, 4.6f, 5.55f, 1.35f, 1.35f);
            Place(pieces, "CoffeeTable_Living", 4.5f, 1.55f, 4.6f, 1.85f, 1.5f, .8f);
            Place(pieces, "Rug_LivingRoom", 4.5f, 2.45f, 4.6f, 2.55f, 4.65f, 3.3f);
            Place(pieces, "Plant_Indoor", 7.35f, 2.4f, 7.3f, 3.65f, .95f, 1.3f);
            Place(pieces, "Clock_Living", 7.75f, 5.75f, 7.7f, 6.15f, .6f, .65f);
            Place(pieces, "WallPicture_Living", 6.7f, 5.9f, 6.65f, 6.05f, 1.25f, .95f);

            // One fitted kitchen run: matching toe line and narrow joins between appliances.
            Place(pieces, "Kitchen_Cabinet", 4.55f, -5.15f, 4.6f, -5.35f, 2.65f, 1.2f);
            Place(pieces, "Fridge_Kitchen", 1.45f, -4.95f, 2.75f, -5.05f, .95f, 1.7f);
            Place(pieces, "Stove_Kitchen", 7.4f, -5, 6.6f, -5.2f, 1.05f, 1.4f);
            Place(pieces, "KitchenRunner", 4.55f, -3.85f, 4.65f, -3.85f, 4.5f, .85f);
            Place(pieces, "CoffeeTable_Kitchen", 4.6f, -2.55f, 4.6f, -2.05f, 1.45f, .85f);
            Place(pieces, "Coffee_Cup", 4.6f, -2.55f, 4.6f, -1.86f, .48f, .48f);
            // Face the sofa toward the north-wall TV, with the coffee table between them.
            bool movingSofa = At(pieces, "Sofa_LivingRoom", new Vector2(4.6f, 3.55f));
            bool movingTable = At(pieces, "CoffeeTable_Living", new Vector2(4.6f, 1.85f));
            bool movingDesk = At(pieces, "Desk_Office", new Vector2(-5f, -3.8f));
            Place(pieces, "Sofa_LivingRoom", 4.6f, 3.55f, 4.6f, 2.15f, 2.55f, 1.45f);
            Place(pieces, "CoffeeTable_Living", 4.6f, 1.85f, 4.6f, 3.85f, 1.5f, .8f);
            Place(pieces, "Rug_LivingRoom", 4.6f, 2.55f, 4.6f, 3.5f, 4.65f, 4.5f);

            // The writing desk opens south; its chair and notebook now share that working axis.
            Place(pieces, "Desk_Office", -5f, -3.8f, -5f, -3.1f, 2.5f, 1.5f);
            Place(pieces, "Chair_Office", -5f, -2.3f, -5f, -4.45f, .65f, .9f);
            Place(pieces, "Notebook_TI", -5f, -3.3f, -5f, -2.8f, .85f, .65f);
            Place(pieces, "Radio_Office", -6.02f, -3.3f, -6.02f, -2.8f, .65f, .48f);
            Place(pieces, "Lamp_Desk", -3.95f, -3.16f, -3.95f, -2.66f, .52f, .62f);
            // A dining-sized coffee table, with a small cup on the rear half of the tabletop.
            Place(pieces, "CoffeeTable_Kitchen", 4.6f, -2.05f, 4.6f, -2.05f, 2.15f, 1.25f);
            Place(pieces, "Coffee_Cup", 4.6f, -1.86f, 4.6f, -1.86f, .32f, .32f);
            Place(pieces, "CoffeeTable_Living", 4.6f, 3.85f, 4.6f, 3.85f, 1.8f, 1f);
            Place(pieces, "ToyBox_UnderBed", -6.3f, 3.15f, -6.3f, 3.15f, 1.15f, 1f);
            Place(pieces, "Backpack_Prop", -3.35f, 2.65f, -3.35f, 2.65f, .72f, .92f);
            // Fit the shortened west wing and the taller north wing of the reference floor plan.
            Place(pieces, "Bed_Edelzio", -6.3f, 4.9f, -6.3f, 4.45f, 1.9f, 2.2f);
            Place(pieces, "TV_StaticNoise", 4.6f, 5.55f, 4.6f, 6.75f, 1.35f, 1.35f);
            Place(pieces, "WallPicture_Living", 6.65f, 6.05f, 6.65f, 7.65f, 1.25f, .95f);
            Place(pieces, "Clock_Living", 7.7f, 6.15f, 7.7f, 7.65f, .6f, .65f);
            Place(pieces, "Bookshelf_Office", -7.35f, -5.35f, -7.35f, -5.1f, 1.35f, 1.6f);
            Place(pieces, "FuseBox_Prop", 1, 6.2f, .25f, 7.65f, .9f, .9f);
            Place(pieces, "WallPicture_Office", -7.2f, -.85f, -7.2f, -.1f, 1.3f, .7f);
            // Increase furniture moderately while keeping the authored circulation and table accessories aligned.
            Place(pieces, "Bed_Edelzio", -6.3f, 4.45f, -6.35f, 4.1f, 2.15f, 2.45f);
            Place(pieces, "Nightstand_Bedroom", -4.55f, 5.35f, -4.65f, 5.1f, .8f, .9f);
            Place(pieces, "Dresser_Bedroom", -7.5f, 1.65f, -7.5f, 1.5f, 1.35f, 1.3f);
            Place(pieces, "BedroomRug", -5.45f, 2.15f, -5.65f, 2, 3.8f, 2.1f);
            Place(pieces, "ToyBox_UnderBed", -6.3f, 3.15f, -6.35f, 2.65f, 1.3f, 1.05f);
            Place(pieces, "Backpack_Prop", -3.35f, 2.65f, -3.6f, 2.4f, .58f, .58f);
            Place(pieces, "Desk_Office", -5, -3.1f, -5, -3.1f, 2.85f, 1.7f);
            Place(pieces, "Chair_Office", -5, -4.45f, -5, -4.3125f, .76f, 1.05f);
            Place(pieces, "Notebook_TI", -5, -2.8f, -5, -2.72f, .95f, .72f);
            Place(pieces, "Radio_Office", -6.02f, -2.8f, -5.85f, -2.72f, .72f, .55f);
            Place(pieces, "Lamp_Desk", -3.95f, -2.66f, -4.18f, -2.58f, .58f, .7f);
            Place(pieces, "Bookshelf_Office", -7.35f, -5.1f, -7.5f, -5, 1.5f, 1.8f);
            Place(pieces, "Doc_Historical", -2.25f, -5.02f, -3, -4.95f, .55f, .55f);
            Place(pieces, "Sofa_LivingRoom", 4.6f, 2.15f, 4.6f, 1.9f, 2.75f, 1.55f);
            Place(pieces, "TV_StaticNoise", 4.6f, 6.75f, 4.6f, 6.75f, 1.55f, 1.55f);
            Place(pieces, "CoffeeTable_Living", 4.6f, 3.85f, 4.6f, 3.75f, 2.15f, 1.25f);
            Place(pieces, "Rug_LivingRoom", 4.6f, 3.5f, 4.6f, 2.75f, 4.4f, 4.6f);
            Place(pieces, "Plant_Indoor", 7.3f, 3.65f, 7.35f, 3.75f, 1.05f, 1.5f);
            Place(pieces, "Clock_Living", 7.7f, 7.65f, 7.7f, 7.65f, .7f, .75f);
            Place(pieces, "WallPicture_Living", 6.65f, 7.65f, 6.65f, 7.65f, 1.4f, 1.05f);
            Place(pieces, "Kitchen_Cabinet", 4.6f, -5.35f, 4.6f, -5.35f, 3, 1.35f);
            Place(pieces, "Fridge_Kitchen", 2.75f, -5.05f, 2.4f, -5.05f, 1.1f, 1.8f);
            Place(pieces, "Stove_Kitchen", 6.6f, -5.2f, 6.9f, -5.15f, 1.2f, 1.55f);
            Place(pieces, "KitchenRunner", 4.65f, -3.85f, 4.8f, -4.2f, 5.15f, .95f);
            Place(pieces, "CoffeeTable_Kitchen", 4.6f, -2.05f, 4.8f, -3.05f, 2.4f, 1.4f);
            Place(pieces, "Coffee_Cup", 4.6f, -1.86f, 4.8f, -2.78f, .35f, .35f);
            // A full player-width route from the west doorway to both sides of the office chair.
            Place(pieces, "Bookshelf_Office", -7.5f, -5, -8.0625f, -4.875f, 1.5f, 1.8f);
            Place(pieces, "Chair_Office", -5, -4.5f, -5, -4.3125f, .76f, 1.05f);
            if (At(pieces, "Desk_Office", new Vector2(-5, -3.1f)) && pieces.TryGetValue("Desk_Office", out var officeDesk))
            {
                var collider = officeDesk.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.size = new Vector2(.82f, .5f);
                    collider.offset = Vector2.down * .12f;
                }
            }
            if (At(pieces, "Chair_Office", new Vector2(-5, -4.3125f)) && pieces.TryGetValue("Chair_Office", out var officeChair))
            {
                var collider = officeChair.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    collider.size = new Vector2(.82f, .42f);
                    collider.offset = Vector2.down * .025f;
                }
            }
            if (pieces.TryGetValue("CoffeeTable_Kitchen", out var coffeeTable))
            {
                var collider = coffeeTable.GetComponent<BoxCollider2D>();
                if (collider == null) collider = coffeeTable.gameObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(.86f, .45f);
                collider.offset = Vector2.down * .1f;
            }
            if (pieces.TryGetValue("Cadeira_Cafe", out var coffeeChair)
                && (At(pieces, "Cadeira_Cafe", new Vector2(3.25f, -2f)) || At(pieces, "Cadeira_Cafe", new Vector2(3.1f, -2f))))
                coffeeChair.position = new Vector3(3.125f, -3, coffeeChair.position.z);
            // Move the previously baked contact shadows with the migrated furniture.
            foreach (var item in pieces.Values)
            {
                if (!item.name.StartsWith("Sombra_Contato_")) continue;
                Vector2 previous = item.position;
                if (movingSofa && (previous - new Vector2(4.73f, 2.865f)).sqrMagnitude < .003f)
                    item.position += Vector3.down * 1.4f;
                else if (movingTable && (previous - new Vector2(4.73f, 1.49f)).sqrMagnitude < .003f)
                    item.position += Vector3.up * 2f;
                else if (movingDesk && (previous - new Vector2(-4.87f, -4.51f)).sqrMagnitude < .003f)
                    item.position += Vector3.up * .7f;
            }
            if (pieces.TryGetValue("Notebook_TI", out var notebook)) EnsureNotebookChair(notebook);
        }

        public static GameObject EnsureNotebookChair(Transform notebook)
        {
            if (notebook == null) return null;
            GameObject chair = null;
            foreach (var item in notebook.root.GetComponentsInChildren<Transform>(true))
                if (item.name == "Chair_Office") { chair = item.gameObject; break; }
            if (chair == null)
            {
                chair = new GameObject("Chair_Office");
                chair.transform.SetParent(notebook.parent, true);
                chair.transform.position = notebook.position + Vector3.down * 1.35f;
                var renderer = chair.AddComponent<SpriteRenderer>();
                renderer.sprite = VarginhaSceneryArt.Create("Chair", new Vector2(.85f, 1.2f));
                renderer.sortingOrder = 3;
                chair.AddComponent<BoxCollider2D>().size = new Vector2(.6f, .55f);
            }
            return chair;
        }

        private static bool At(Dictionary<string, Transform> pieces, string name, Vector2 position) =>
            pieces.TryGetValue(name, out var item) && ((Vector2)item.position - position).sqrMagnitude < .0025f;

        private static void Place(Dictionary<string, Transform> pieces, string name,
            float oldX, float oldY, float x, float y, float width, float height)
        {
            if (!pieces.TryGetValue(name, out var item)) return;
            if (((Vector2)item.position - new Vector2(oldX, oldY)).sqrMagnitude > .0025f &&
                ((Vector2)item.position - new Vector2(x, y)).sqrMagnitude > .0025f) return;
            item.position = new Vector3(Mathf.Round(x * 32) / 32, Mathf.Round(y * 32) / 32, item.position.z);
            item.localScale = new Vector3(width, height, item.localScale.z);
        }
    }
}
