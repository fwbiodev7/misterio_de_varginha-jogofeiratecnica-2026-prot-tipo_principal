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
            Place(pieces, "Bed_Edelzio", -6.5f, 5, -6.3f, 4.9f, 2.4f, 2.8f);
            Place(pieces, "Nightstand_Bedroom", -3.2f, 5.15f, -4.55f, 5.35f, .95f, 1.15f);
            Place(pieces, "Dresser_Bedroom", -7.65f, 1.15f, -7.5f, 1.65f, 1.5f, 1.45f);
            Place(pieces, "BedroomRug", -5.25f, 2.1f, -5.45f, 2.15f, 3.4f, 1.8f);
            Place(pieces, "ToyBox_UnderBed", -6.5f, 3.2f, -6.3f, 3.15f, 1, .8f);
            Place(pieces, "Backpack_Prop", -3, 2, -3.35f, 2.65f, .9f, .9f);

            // Desk accessories sit on the rear half of the tabletop, above its apron.
            Place(pieces, "Desk_Office", -5, -3.5f, -5, -3.8f, 3.4f, 2.1f);
            Place(pieces, "Chair_Office", -5, -2.25f, -5, -2.3f, .85f, 1.2f);
            Place(pieces, "Notebook_TI", -5, -3.5f, -5, -3.3f, .85f, .65f);
            Place(pieces, "Radio_Office", -6.9f, -3.35f, -6.02f, -3.3f, .65f, .48f);
            Place(pieces, "Lamp_Desk", -4, -3, -3.95f, -3.16f, .52f, .62f);
            Place(pieces, "Bookshelf_Office", -7.3f, -5.3f, -7.35f, -5.35f, 1.7f, 2);
            Place(pieces, "Doc_Historical", -2, -5.5f, -2.25f, -5.02f, .5f, .5f);
            Place(pieces, "WallPicture_Office", -7, -1.25f, -7.2f, -.85f, 1.3f, 1);

            Place(pieces, "Sofa_LivingRoom", 4.5f, 3.55f, 4.6f, 3.55f, 3.2f, 2.1f);
            Place(pieces, "TV_StaticNoise", 4.5f, 5.45f, 4.6f, 5.55f, 1.9f, 2);
            Place(pieces, "CoffeeTable_Living", 4.5f, 1.55f, 4.6f, 1.85f, 1.9f, 1.1f);
            Place(pieces, "Rug_LivingRoom", 4.5f, 2.45f, 4.6f, 2.55f, 4.65f, 3.3f);
            Place(pieces, "Plant_Indoor", 7.35f, 2.4f, 7.3f, 3.65f, .95f, 1.3f);
            Place(pieces, "Clock_Living", 7.75f, 5.75f, 7.7f, 6.15f, .6f, .65f);
            Place(pieces, "WallPicture_Living", 6.7f, 5.9f, 6.65f, 6.05f, 1.25f, .95f);

            // One fitted kitchen run: matching toe line and narrow joins between appliances.
            Place(pieces, "Kitchen_Cabinet", 4.55f, -5.15f, 4.6f, -5.35f, 3.1f, 1.55f);
            Place(pieces, "Fridge_Kitchen", 1.45f, -4.95f, 2.75f, -5.05f, 1.15f, 2.15f);
            Place(pieces, "Stove_Kitchen", 7.4f, -5, 6.6f, -5.2f, 1.25f, 1.85f);
            Place(pieces, "KitchenRunner", 4.55f, -3.85f, 4.65f, -3.85f, 4.5f, .85f);
            Place(pieces, "CoffeeTable_Kitchen", 4.6f, -2.55f, 4.6f, -2.05f, 1.8f, 1.05f);
            Place(pieces, "Coffee_Cup", 4.6f, -2.55f, 4.6f, -1.86f, .48f, .48f);
        }

        private static void Place(Dictionary<string, Transform> pieces, string name,
            float oldX, float oldY, float x, float y, float width, float height)
        {
            if (!pieces.TryGetValue(name, out var item)) return;
            if (((Vector2)item.position - new Vector2(oldX, oldY)).sqrMagnitude > .0025f) return;
            item.position = new Vector3(Mathf.Round(x * 32) / 32, Mathf.Round(y * 32) / 32, item.position.z);
            item.localScale = new Vector3(width, height, item.localScale.z);
        }
    }
}
