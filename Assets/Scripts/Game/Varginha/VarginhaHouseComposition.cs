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

            // ── Quarto ─────────────────────────────────────────────────────────
            Set(pieces, "Bed_Edelzio",         -6.35f,  4.10f, 2.25f, 2.50f);
            Set(pieces, "Nightstand_Bedroom",   -4.65f,  5.10f,  .82f,  .92f);
            Set(pieces, "Dresser_Bedroom",      -7.50f,  1.50f, 1.38f, 1.32f);
            Set(pieces, "BedroomRug",           -5.65f,  2.15f, 3.60f, 1.85f);
            Set(pieces, "ToyBox_UnderBed",      -6.35f,  2.60f, 1.32f, 1.06f);
            // Mochila no chão: prop compacto e legível sem ser enorme
            Set(pieces, "Backpack_Prop",         -3.60f,  2.40f,  .52f,  .52f);

            // ── Escritório ─────────────────────────────────────────────────────
            Set(pieces, "Desk_Office",          -5.00f, -3.10f, 2.85f, 1.70f);
            Set(pieces, "Chair_Office",         -5.00f, -4.45f,  .78f, 1.06f);
            // Acessórios na metade traseira da mesa (y mais alto = mais ao norte)
            Set(pieces, "Notebook_TI",          -5.00f, -2.72f,  .96f,  .74f);
            Set(pieces, "Radio_Office",         -5.88f, -2.72f,  .72f,  .56f);
            Set(pieces, "Lamp_Desk",            -4.18f, -2.58f,  .58f,  .70f);
            Set(pieces, "Bookshelf_Office",     -8.06f, -4.88f, 1.52f, 1.82f);
            Set(pieces, "Doc_Historical",       -3.00f, -4.95f,  .55f,  .55f);
            Set(pieces, "WallPicture_Office",   -7.20f,  -.10f, 1.30f,  .72f);

            // ── Sala de Estar ──────────────────────────────────────────────────
            // Sofá apontado para o sul (encostado na parede norte, virado para a TV)
            Set(pieces, "Sofa_LivingRoom",       4.60f,  2.05f, 2.80f, 1.58f);
            Set(pieces, "TV_StaticNoise",        4.60f,  6.75f, 1.58f, 1.58f);
            Set(pieces, "CoffeeTable_Living",    4.60f,  3.80f, 2.18f, 1.28f);
            Set(pieces, "Rug_LivingRoom",        4.60f,  3.10f, 4.20f, 3.80f);
            Set(pieces, "Plant_Indoor",          7.35f,  3.75f, 1.06f, 1.52f);
            Set(pieces, "Clock_Living",          7.70f,  7.65f,  .70f,  .76f);
            Set(pieces, "WallPicture_Living",    6.65f,  7.65f, 1.42f, 1.06f);

            // ── Cozinha ────────────────────────────────────────────────────────
            Set(pieces, "Kitchen_Cabinet",       4.60f, -5.35f, 3.05f, 1.38f);
            Set(pieces, "Fridge_Kitchen",        2.40f, -5.05f, 1.12f, 1.82f);
            Set(pieces, "Stove_Kitchen",         6.90f, -5.15f, 1.22f, 1.58f);
            Set(pieces, "KitchenRunner",         4.80f, -4.20f, 5.20f,  .96f);
            // Mesa de jantar: scale proporcional para o sprite oval não esticar demais
            Set(pieces, "CoffeeTable_Kitchen",   4.80f, -3.05f, 1.65f, 1.10f);
            // Xícara de café: pequenina, ~18 cm em cima da mesa
            Set(pieces, "Coffee_Cup",            4.80f, -2.78f,  .18f,  .18f);

            // ── Outros ─────────────────────────────────────────────────────────
            Set(pieces, "FuseBox_Prop",           .25f,  7.65f,  .90f,  .90f);

            // ── Colisores ajustados ────────────────────────────────────────────
            if (pieces.TryGetValue("Desk_Office", out var officeDesk))
            {
                var col = officeDesk.GetComponent<BoxCollider2D>();
                if (col != null) { col.size = new Vector2(.82f, .50f); col.offset = Vector2.down * .12f; }
            }
            if (pieces.TryGetValue("Chair_Office", out var officeChair))
            {
                var col = officeChair.GetComponent<BoxCollider2D>();
                if (col != null) { col.size = new Vector2(.78f, .42f); col.offset = Vector2.down * .025f; }
            }
            if (pieces.TryGetValue("CoffeeTable_Kitchen", out var coffeeTable))
            {
                var col = coffeeTable.GetComponent<BoxCollider2D>();
                if (col == null) col = coffeeTable.gameObject.AddComponent<BoxCollider2D>();
                col.size   = new Vector2(.72f, .42f);
                col.offset = Vector2.down * .10f;
            }
            if (pieces.TryGetValue("Cadeira_Cafe", out var coffeeChair))
                if (((Vector2)coffeeChair.position - new Vector2(3.125f, -3f)).sqrMagnitude > .01f)
                    coffeeChair.position = new Vector3(3.125f, -3f, coffeeChair.position.z);

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

        /// <summary>Unconditionally places a furniture piece at the given world position and scale.</summary>
        private static void Set(Dictionary<string, Transform> pieces, string name,
            float x, float y, float width, float height)
        {
            if (!pieces.TryGetValue(name, out var item)) return;
            item.position   = new Vector3(Mathf.Round(x * 32) / 32, Mathf.Round(y * 32) / 32, item.position.z);
            item.localScale = new Vector3(width, height, item.localScale.z);
        }

        private static void Place(Dictionary<string, Transform> pieces, string name,
            float oldX, float oldY, float x, float y, float width, float height)
        {
            if (!pieces.TryGetValue(name, out var item)) return;
            if (((Vector2)item.position - new Vector2(oldX, oldY)).sqrMagnitude > .0025f &&
                ((Vector2)item.position - new Vector2(x, y)).sqrMagnitude > .0025f) return;
            item.position   = new Vector3(Mathf.Round(x * 32) / 32, Mathf.Round(y * 32) / 32, item.position.z);
            item.localScale = new Vector3(width, height, item.localScale.z);
        }
    }
}
