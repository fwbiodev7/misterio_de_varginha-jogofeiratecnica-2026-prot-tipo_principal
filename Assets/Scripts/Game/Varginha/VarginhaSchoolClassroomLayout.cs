using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>The open computer classroom reconstructed from the September 30 reference.</summary>
    public static class VarginhaSchoolClassroomLayout
    {
        public const string Marker = "CenarioVisual_Escola_Laboratorio20261001_V2";
        private static readonly Dictionary<string, Sprite> Sprites = new();
        public static Vector3 StudentPosition(int index) => new(-4.45f + index % 3 * 4.8f, 1.55f - index / 3 * 2.65f);
        public static readonly Vector3[] EnemyPositions =
        {
            new(-4.45f, 3.15f), new(5.15f, -1.85f), new(.35f, 4.25f), new(5.15f, 3.15f)
        };

        public static void Build(Transform school)
        {
            if (school.Find(Marker) != null) return;
            // Remove only the old school's scenery; its exterior and gameplay actors are preserved.
            var obsolete = new List<GameObject>();
            foreach (Transform child in school)
                if (child.name.StartsWith("CenarioV2_") || child.name.StartsWith("CenarioVisual_Escola_")
                    || child.name.StartsWith("Cenario_Acabamento_")) obsolete.Add(child.gameObject);
            foreach (var go in obsolete)
            {
                go.SetActive(false);
                go.name += "_Replacing";
                if (Application.isPlaying) Object.Destroy(go); else Object.DestroyImmediate(go);
            }
            new GameObject(Marker).transform.SetParent(school, false);
            for (int y = -6; y < 6; y++)
            for (int x = -7; x < 9; x++)
                Surface(school, "CenarioV2_Piso_Escola_" + x + "_" + y, "SchoolFloor",
                    new(x + .5f, y + .5f), Vector2.one, new(.57f, .54f, .46f), 0);
            Wall(school, "CenarioV2_Parede_Norte", new(.5f, 5.7f), new(16.4f, .7f));
            Wall(school, "CenarioV2_Parede_Sul", new(.5f, -5.7f), new(16.4f, .7f));
            Wall(school, "CenarioV2_Parede_Oeste", new(-7.7f, 0), new(.7f, 11.4f));
            Wall(school, "CenarioV2_Parede_Leste", new(8.7f, 0), new(.7f, 11.4f));
            float[] columns = { -5.85f, -3.05f, 3.75f, 6.55f };
            for (int row = 0; row < 3; row++)
            for (int column = 0; column < columns.Length; column++)
            {
                int index = row * 4 + column;
                Vector2 position = new(columns[column], 3.15f - row * 2.4f);
                bool computer = column != 1;
                var table = Art(school, "CenarioV2_Carteira_" + index, computer ? "ComputerDesk" : "Desk",
                    position + Vector2.up * (computer ? .16f : 0), new(1.72f, computer ? 1.35f : .95f), 3);
                Blocker(table, new(1.55f, .4f), new(0, -.15f - (computer ? .16f : 0)));
                var chair = Art(school, "CenarioV2_Cadeira_" + index, "Chair", position + Vector2.down * .86f,
                    new(.76f, .9f), 3);
                Blocker(chair, new(.44f, .32f), new(0, -.1f));
            }
            for (int i = 0; i < 3; i++)
                Art(school, "CenarioV2_Janela_" + i, "Window", new(-5.25f + i * 3.15f, 5.2f), new(2.7f, .92f), 4);
            Art(school, "CenarioV2_Lousa_Direita", "Whiteboard", new(6.55f, 5.18f), new(2.15f, .82f), 4);
            Art(school, "CenarioV2_Projetor", "Projector", new(3.25f, 5.2f), new(.75f, .42f), 4);
            var teacher = Art(school, "CenarioV2_Mesa_Professor", "TeacherDesk", new(.8f, 4.64f), new(1.85f, .65f), 3);
            Blocker(teacher, new(1.65f, .3f), new(0, -.12f));
            var shelf = Art(school, "CenarioV2_Estante_Informatica", "Shelf", new(-6.65f, -4.67f), new(.7f, 1.3f), 3);
            Blocker(shelf, new(.62f, .45f), new(0, -.35f));
            Surface(school, "CenarioV2_Quadro_Mural", "SchoolPoster", new(8f, -4.55f), new(.5f, .9f),
                new(.74f, .48f, .18f), 4);
            var parking = Surface(school, "CenarioV2_Vaga_Fusca", "FuscaParking", VarginhaEnvironmentArt.FuscaParkingPosition,
                VarginhaEnvironmentArt.FuscaParkingSize, new(.22f, .34f, .39f), 1);
            parking.transform.localScale = Vector3.one;
            parking.sprite = VarginhaSceneryArt.Create("Parking", VarginhaEnvironmentArt.FuscaParkingSize);
        }

        public static void RefreshSprites(Transform school)
        {
            foreach (var renderer in school.GetComponentsInChildren<SpriteRenderer>(true))
            {
                string id = renderer.name;
                if (id.StartsWith("CenarioV2_Carteira_"))
                {
                    int index = int.Parse(id.Substring("CenarioV2_Carteira_".Length));
                    ApplySprite(renderer, index % 4 == 1 ? "Desk" : "ComputerDesk");
                }
                else if (id.StartsWith("CenarioV2_Cadeira_")) ApplySprite(renderer, "Chair");
                else if (id.StartsWith("CenarioV2_Janela_")) ApplySprite(renderer, "Window");
                else if (id == "CenarioV2_Lousa_Direita") ApplySprite(renderer, "Whiteboard");
                else if (id == "CenarioV2_Projetor") ApplySprite(renderer, "Projector");
                else if (id == "CenarioV2_Mesa_Professor") ApplySprite(renderer, "TeacherDesk");
                else if (id == "CenarioV2_Estante_Informatica") ApplySprite(renderer, "Shelf");
            }
        }

        private static Sprite Load(string motif)
        {
            if (Sprites.TryGetValue(motif, out var cached) && cached != null) return cached;
            foreach (var sprite in Resources.LoadAll<Sprite>("Varginha/SchoolComputerLab")) Sprites[sprite.name] = sprite;
            return Sprites.TryGetValue(motif, out var found) ? found : null;
        }
        private static void ApplySprite(SpriteRenderer renderer, string motif)
        {
            var sprite = Load(motif);
            if (sprite != null && renderer.sprite != sprite)
            {
                Vector3 size = renderer.bounds.size;
                var scale = renderer.transform.lossyScale;
                var collider = renderer.GetComponent<BoxCollider2D>();
                Vector2 collisionSize = collider != null ? Vector2.Scale(collider.size, scale) : Vector2.zero;
                Vector2 collisionOffset = collider != null ? Vector2.Scale(collider.offset, scale) : Vector2.zero;
                renderer.sprite = sprite;
                renderer.transform.localScale = new(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1);
                if (collider != null)
                {
                    scale = renderer.transform.lossyScale;
                    collider.size = new(collisionSize.x / scale.x, collisionSize.y / scale.y);
                    collider.offset = new(collisionOffset.x / scale.x, collisionOffset.y / scale.y);
                }
            }
        }
        private static SpriteRenderer Art(Transform parent, string name, string motif, Vector2 position, Vector2 size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Load(motif) ?? VarginhaSceneryArt.Create(motif == "ComputerDesk" ? "Desk" : motif, size);
            sr.transform.localScale = new(size.x / sr.sprite.bounds.size.x, size.y / sr.sprite.bounds.size.y, 1);
            sr.sortingOrder = order;
            return sr;
        }
        private static SpriteRenderer Surface(Transform parent, string name, string id, Vector2 position, Vector2 size, Color tint, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new(size.x, size.y, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create(id, tint);
            sr.sortingOrder = order;
            return sr;
        }
        private static void Wall(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var sr = Surface(parent, name, "SchoolWall", position, size, new(.18f, .24f, .29f), 3);
            sr.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
        }
        private static void Blocker(SpriteRenderer renderer, Vector2 worldSize, Vector2 worldOffset)
        {
            var scale = renderer.transform.lossyScale;
            var collider = renderer.gameObject.AddComponent<BoxCollider2D>();
            collider.size = new(worldSize.x / scale.x, worldSize.y / scale.y);
            collider.offset = new(worldOffset.x / scale.x, worldOffset.y / scale.y);
        }
    }
}
