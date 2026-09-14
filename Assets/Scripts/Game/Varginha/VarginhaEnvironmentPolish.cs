using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Decorative layers shared by saved scenes and runtime builders; never adds obstacles.</summary>
    public static class VarginhaEnvironmentPolish
    {
        private const string Marker = "Cenario_Acabamento_V3";

        public static void EnsureSchool(Transform school)
        {
            if (!NeedsRefresh(school)) return;
            RefreshSurfaces(school, true);
            var decor = Root(school);

            // Window openings cast the same grid onto the classroom floor.
            for (int i = 0; i < 3; i++)
            {
                float x = new[] { -3.8f, 0f, 3.7f }[i];
                var window = school.Find("CenarioV2_Janela_" + i);
                if (window != null) Replace(window, "Window", new Vector2(1.15f, .66f));
                var light = Part(decor, "Luz_Janela_" + i, "WindowLight", new Vector2(x + .28f, 4.27f), new Vector2(1.6f, 2.45f), 1);
                light.transform.rotation = Quaternion.Euler(0, 0, -90);
            }
            Part(decor, "Mural_Trabalhos", "Noticeboard", new Vector2(1.1f, 4.1f), new Vector2(1.65f, .78f), 3);
            Part(decor, "Biblioteca_Sala", "Books", new Vector2(-6.66f, 3.95f), new Vector2(.65f, 1.6f), 3);
            Part(decor, "Livros_Corredor", "Books", new Vector2(7.75f, -4.7f), new Vector2(.85f, 1.15f), 3);
            Part(decor, "Planta_Entrada", "Plant", new Vector2(7.55f, -3.8f), new Vector2(.55f, .85f), 3);
            Part(decor, "Planta_Sala", "Plant", new Vector2(-6.65f, 1.55f), new Vector2(.5f, .75f), 3);
            foreach (var renderer in school.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.name.StartsWith("CenarioV2_Carteira_")) Replace(renderer.transform, "Desk", new Vector2(.88f, .62f));
                else if (renderer.name.StartsWith("CenarioV2_Lousa_")) Replace(renderer.transform, "Blackboard", new Vector2(2.1f, .65f));
            }
            // A restrained runner and ceiling light pools organize the open corridor.
            Part(decor, "Passadeira_Corredor", "Rug", new Vector2(4.8f, -.55f), new Vector2(3.4f, .9f), 1).color = new Color(.72f, .83f, .86f);
            Pool(decor, "Luz_Sala_Esquerda", new Vector2(-4.2f, 3.4f), new Vector2(3.8f, 2.35f), .42f);
            Pool(decor, "Luz_Sala_Direita", new Vector2(5.5f, 3.5f), new Vector2(3.8f, 2.25f), .42f);
            Pool(decor, "Luz_Corredor", new Vector2(1.9f, -2.8f), new Vector2(3.2f, 1.8f), .3f);
            ContactShadows(school, decor);
            Dust(decor, new Vector2(-4.1f, 3.85f), new Vector2(2.5f, 1.2f), 8, new Color(.88f, .88f, .7f, .42f));
        }

        public static void EnsureDiocese(Transform church)
        {
            if (!NeedsRefresh(church)) return;
            RefreshSurfaces(church, false);
            var decor = Root(church);
            // Lateral beams point into the nave, away from the wall. Rays and
            // reflected panes are floor layers, behind pews, actors and the altar.
            float[] rows = { 4.15f, 1.25f, -1.65f };
            for (int i = 0; i < 6; i++)
            {
                bool right = i >= 3;
                float y = rows[i % 3];
                var window = church.Find("CenarioV2_Vitral_Lateral_" + i);
                if (window != null) Replace(window, "Glass", new Vector2(.66f, 1.85f));
                var ray = Part(decor, "Reflexo_Vitral_Lateral_" + i, "GlassLight",
                    new Vector2(right ? 7.06f : -6.06f, y - .3f), new Vector2(4.3f, 2.2f), 1);
                ray.flipX = right;
                ray.transform.position += Vector3.forward * -.025f;
                var column = Part(decor, "Pilastra_Lateral_" + i, "Column",
                    new Vector2(right ? 9.1f : -8.1f, y - 1.46f), new Vector2(.48f, 1.15f), 3);
                Shadow(decor, column.transform.position + Vector3.down * .48f, new Vector2(.8f, .4f));
                Dust(decor, new Vector2(right ? 7.5f : -6.5f, y - .2f), new Vector2(2.25f, .95f), 5, new Color(.84f, .79f, .95f, .4f));
            }
            float[] north = { -5.1f, -1.7f, 1.7f, 5.1f };
            for (int i = 0; i < north.Length; i++)
            {
                var window = church.Find("CenarioV2_Vitral_Fundo_" + i);
                if (window != null) Replace(window, "Glass", new Vector2(1.15f, 1.2f));
                var ray = Part(decor, "Reflexo_Vitral_Fundo_" + i, "GlassLight",
                    new Vector2(north[i] + .2f, 4.47f), new Vector2(3.1f, 1.7f), 1);
                ray.transform.rotation = Quaternion.Euler(0, 0, -90);
                ray.transform.position += Vector3.forward * -.025f;
            }
            Part(decor, "Rosacea_Piso", "Mosaic", new Vector2(-.2f, -3.4f), new Vector2(2.4f, 2.1f), 1);
            var carpet = church.Find("CenarioV2_Tapete_Altar");
            if (carpet != null) Replace(carpet, "Rug", new Vector2(4.8f, 1.15f));
            foreach (var renderer in church.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.name.StartsWith("CenarioV2_Banco_Igreja_")) Replace(renderer.transform, "Pew", new Vector2(2.15f, .65f));
                else if (renderer.name == "CenarioV2_Altar_Visual") Replace(renderer.transform, "Altar", new Vector2(1.6f, 1.3f));
            }
            Part(decor, "Arquivo_Sacristia", "Books", new Vector2(7.55f, 4.6f), new Vector2(1.45f, 1.2f), 3);
            Part(decor, "Planta_Sacristia", "Plant", new Vector2(7.75f, -5.4f), new Vector2(.8f, 1.2f), 3);
            Part(decor, "Planta_Entrada", "Plant", new Vector2(-7.85f, -5.65f), new Vector2(.7f, 1f), 3);
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = new Vector2(i % 2 == 0 ? 3.95f : 6.45f, i < 2 ? 1.45f : -1f);
                Pool(decor, "Luz_Vela_Altar_" + i, pos + Vector2.down * .18f, new Vector2(2.3f, 1.55f), 1f, true);
            }
            for (int i = 0; i < 3; i++)
            {
                var pos = new Vector2(-3.4f + i * 3.4f, 5.9f);
                Part(decor, "Arandela_" + i, "Sconce", pos, new Vector2(.45f, .8f), 4);
                Pool(decor, "Luz_Arandela_" + i, pos + Vector2.down * .5f, new Vector2(2.3f, 1.45f), .85f, true);
            }
            ContactShadows(church, decor);
        }

        public static void EnsureHouse(Transform house)
        {
            if (!NeedsRefresh(house)) return;
            var decor = Root(house);
            foreach (var renderer in house.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.name.StartsWith("Floor_House"))
                {
                    renderer.sprite = VarginhaPixelArtSprites.Create("Floor_House", new Color(.39f, .29f, .23f));
                    renderer.color = Color.white;
                }
                else if (renderer.name.StartsWith("Floor_Yard"))
                {
                    renderer.sprite = VarginhaPixelArtSprites.Create("Floor_Yard", new Color(.12f, .24f, .19f));
                    renderer.color = Color.white;
                }
                else if (renderer.name.StartsWith("Tree_")) renderer.color = new Color(.66f, .77f, .79f);
                else if (renderer.name == "StreetLamp_Yard") Replace(renderer.transform, "StreetLamp", new Vector2(.62f, 2.25f));
                else if (renderer.name.StartsWith("Driveway_"))
                    renderer.sprite = VarginhaPixelArtSprites.Create("Driveway_Stone", new Color(.30f, .32f, .32f));
            }
            Vector2[] windows = { new Vector2(-4.25f, 6.35f), new Vector2(3f, 6.35f), new Vector2(7.3f, -6.35f) };
            for (int i = 0; i < windows.Length; i++)
            {
                Part(decor, "Janela_Casa_" + i, "Window", windows[i], new Vector2(1.45f, .72f), 4);
                var ray = Part(decor, "Luar_Casa_" + i, "WindowLight", windows[i] + Vector2.down * (i == 2 ? -1.5f : 1.5f), new Vector2(2.8f, 1.8f), 1);
                ray.transform.rotation = Quaternion.Euler(0, 0, i == 2 ? 90 : -90);
            }
            Pool(decor, "Luz_Abajur", new Vector2(-3.25f, 4.65f), new Vector2(3.3f, 2.8f), .85f, true);
            Pool(decor, "Luz_Escritorio", new Vector2(-3.7f, -3.15f), new Vector2(3.6f, 2.8f), .85f, true);
            Pool(decor, "Luz_Cozinha", new Vector2(4.55f, -4.8f), new Vector2(4.6f, 2.45f), .6f);
            Pool(decor, "Luz_TV", new Vector2(4.5f, 4.6f), new Vector2(3.5f, 2.1f), .6f, true).color = new Color(.35f, .72f, 1f, .55f);
            Part(decor, "Memorias_1996", "Noticeboard", new Vector2(-8.1f, -1.7f), new Vector2(.95f, 1.2f), 3);
            Part(decor, "Livros_Quarto", "Books", new Vector2(-1.3f, 5.9f), new Vector2(.9f, 1.2f), 3);
            Part(decor, "Samambaia_Sala", "Plant", new Vector2(1f, 1.55f), new Vector2(.6f, .95f), 3);
            Part(decor, "Planta_Varanda", "Plant", new Vector2(10.6f, -1.45f), new Vector2(.7f, 1.1f), 3);
            Part(decor, "Tapete_Porta", "Rug", new Vector2(7.8f, 0), new Vector2(1.3f, 1.55f), 1);
            Pool(decor, "Luz_Poste", new Vector2(25.7f, 2.1f), new Vector2(4.5f, 2.85f), .95f, true, 2);
            Pool(decor, "Luz_Varanda", new Vector2(10.2f, .25f), new Vector2(3.2f, 2.75f), .85f, false, 2);
            Part(decor, "Poca_Rua", "Puddle", new Vector2(24.5f, 1.6f), new Vector2(2.2f, .65f), 2);
            Part(decor, "Poca_Caminho", "Puddle", new Vector2(16.8f, -1.4f), new Vector2(1.5f, .5f), 2);
            Vector2[] foliage = { new Vector2(12.5f, 5.1f), new Vector2(15.1f, -4.8f), new Vector2(24.5f, 5.4f), new Vector2(22.9f, -5.3f) };
            for (int i = 0; i < foliage.Length; i++)
            {
                Part(decor, "Folhas_Quintal_" + i, "Leaves", foliage[i], new Vector2(2.5f, 1.6f), 1);
                Shadow(decor, foliage[i] + Vector2.down * .55f, new Vector2(3.2f, 1.25f));
            }
            // Keep the road and the escape interaction clear of new collision geometry.
            ContactShadows(house, decor);
            Dust(decor, new Vector2(-4.1f, 4.7f), new Vector2(2.1f, 1.2f), 6, new Color(.64f, .82f, .95f, .45f));
            Dust(decor, new Vector2(14.3f, -4.8f), new Vector2(3.5f, 2.6f), 7, new Color(.75f, .94f, .58f, .65f));
        }

        private static bool NeedsRefresh(Transform parent)
        {
            if (parent == null) return false;
            var previous = parent.Find(Marker);
            if (previous == null) return true;
            // Procedural sprites are not persistent assets. A saved hierarchy can
            // survive a scene reload while its textures do not; repair that case.
            var renderers = previous.GetComponentsInChildren<SpriteRenderer>(true);
            bool valid = renderers.Length > 0;
            foreach (var renderer in renderers)
                if (renderer.sprite == null || renderer.sprite.texture == null) { valid = false; break; }
            if (valid) return false;
            previous.name = Marker + "_Replacing";
            previous.gameObject.SetActive(false);
            if (Application.isPlaying) Object.Destroy(previous.gameObject);
            else Object.DestroyImmediate(previous.gameObject);
            return true;
        }

        private static void RefreshSurfaces(Transform parent, bool school)
        {
            foreach (var renderer in parent.GetComponentsInChildren<SpriteRenderer>(true))
            {
                string name = renderer.name;
                if (name.StartsWith("CenarioV2_Piso_"))
                {
                    renderer.sprite = VarginhaPixelArtSprites.Create(school ? "SchoolFloor" : "ChurchFloor", school ? new Color(.61f, .54f, .4f) : new Color(.25f, .26f, .29f));
                    renderer.color = Color.white;
                }
                else if (school && (name.StartsWith("CenarioV2_Parede_") || name.StartsWith("CenarioV2_Divisoria_")))
                    renderer.sprite = VarginhaPixelArtSprites.Create("SchoolWall", new Color(.67f, .60f, .44f));
            }
        }

        private static Transform Root(Transform parent)
        { var root = new GameObject(Marker).transform; root.SetParent(parent, false); return root; }

        private static SpriteRenderer Part(Transform parent, string name, string motif, Vector2 position, Vector2 size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(Mathf.Round(position.x * 32f) / 32f, Mathf.Round(position.y * 32f) / 32f, 0f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaSceneryArt.Create(motif, size);
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void Replace(Transform target, string motif, Vector2 size)
        {
            var renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null) return;
            // Sprite dimensions already encode world size. Do not stretch the texture again.
            renderer.sprite = VarginhaSceneryArt.Create(motif, size);
            target.localScale = Vector3.one;
            renderer.color = Color.white;
        }

        private static SpriteRenderer Pool(Transform parent, string name, Vector2 position, Vector2 size, float opacity, bool flicker = false, int order = 1)
        {
            var renderer = Part(parent, name, "Glow", position, size, order);
            renderer.color = new Color(1, 1, 1, opacity);
            if (flicker) renderer.gameObject.AddComponent<VarginhaAmbientPixelEffect>().Configure(false, .065f);
            return renderer;
        }

        private static void Shadow(Transform parent, Vector2 position, Vector2 size)
            => Part(parent, "Sombra_Contato_" + parent.childCount, "Shadow", position, size, 1);

        private static void ContactShadows(Transform environment, Transform decor)
        {
            foreach (var renderer in environment.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer.transform.IsChildOf(decor)) continue;
                string n = renderer.name;
                if (!(n.Contains("Carteira_") || n.Contains("Armario_") || n.Contains("Banco_Igreja_") || n.Contains("Altar_Visual")
                    || n.StartsWith("Bookshelf") || n.StartsWith("Kitchen_") || n.StartsWith("Sofa_") || n.StartsWith("Dresser_"))) continue;
                var b = renderer.bounds;
                Shadow(decor, new Vector2(b.center.x + .13f, b.min.y + .04f), new Vector2(b.size.x + .22f, Mathf.Max(.25f, b.size.y * .35f)));
            }
        }

        private static void Dust(Transform parent, Vector2 center, Vector2 spread, int count, Color color)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2(((i * 37 % 101) / 100f - .5f) * spread.x, ((i * 61 % 97) / 96f - .5f) * spread.y);
                var mote = Part(parent, "Poeira_Luz_" + parent.childCount, "Dust", center + offset, Vector2.one * .0625f, 2);
                mote.transform.localScale = Vector3.one * .5f;
                mote.color = color;
                mote.gameObject.AddComponent<VarginhaAmbientPixelEffect>().Configure(true, i * .73f);
            }
        }
    }
}
