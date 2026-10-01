using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Continuous school entrance and exterior parking, shared by saved scenes and runtime.</summary>
    public static class VarginhaSchoolExterior
    {
        public static readonly Vector2 Entrance = new(.75f, -5.7f);
        public const string RootName = "Escola_Estacionamento_Externo";

        public static void Ensure(Transform school)
        {
            // A continuous base covers the side gardens as the camera follows the player
            // between the narrower school and the wider parking lot.
            if (school.Find("Terreno_Continuo_Escola") == null)
                Tile(school, "Terreno_Continuo_Escola", "Floor_Yard", new(0, -4.1f),
                    new(25, 20.4f), new(.13f, .25f, .21f), -3);
            // Migrate the old unbroken south wall, leaving a 2.5-unit doorway.
            var south = school.Find("CenarioV2_Parede_Sul");
            if (south != null)
            {
                south.position = new Vector3(-4.25f, -5.7f, 0);
                south.localScale = new Vector3(7.5f, .7f, 1);
            }
            var old = school.Find(RootName);
            if (old != null)
            {
                bool valid = true;
                foreach (var sr in old.GetComponentsInChildren<SpriteRenderer>())
                    if (sr.sprite == null || sr.sprite.texture == null) { valid = false; break; }
                if (valid) { ApplyPassage(old); VarginhaIndustrialSchoolFacade.Ensure(school); return; }
                old.name += "_Replacing";
                old.gameObject.SetActive(false);
                if (Application.isPlaying) Object.Destroy(old.gameObject); else Object.DestroyImmediate(old.gameObject);
            }
            var root = new GameObject(RootName).transform;
            root.SetParent(school, false);
            Wall(root, "Parede_Sul_Direita", new(5.55f, -5.7f), new(7.1f, .7f));

            // Grounds, apron and driving lane: all connected without scene transitions.
            Tile(root, "Jardim", "Floor_Yard", new(0, -10), new(25, 8.6f), new(.13f, .25f, .21f), -2);
            Tile(root, "Asfalto", "Street_Parking", new(0, -11.25f), new(25, 5.5f), new(.19f, .23f, .26f), -1);
            Tile(root, "Calcada_Fachada", "Driveway_Stone", new(.5f, -7.1f), new(18, 2), new(.42f, .43f, .39f), 0);
            Tile(root, "Caminho_Entrada", "Driveway_Stone", new(.75f, -8.5f), new(2.5f, 4.9f), new(.47f, .47f, .42f), 0);
            Tile(root, "Soleira_Porta", "Driveway_Stone", Entrance, new(2.5f, .8f), new(.57f, .52f, .40f), 1);
            ApplyPassage(root);
            Art(root, "Tapete_Entrada", "Rug", new(.75f, -6.65f), new(2, .8f), 1);

            Art(root, "Vaga_Visitantes", "Parking", new(6.1f, -10.4f), new(5.5f, 3), 1);
            for (int i = 0; i < 9; i++)
                Stripe(root, "Faixa_Via_" + i, new(-10 + i * 2.5f, -12.85f), new(1.25f, .09f), new(.77f, .67f, .40f));
            for (int i = 0; i < 5; i++)
                Stripe(root, "Travessia_" + i, new(.75f, -9.55f - i * .38f), new(2.05f, .18f), new(.85f, .82f, .68f));

            Art(root, "Banco_Jardim", "Pew", new(5.7f, -7.1f), new(2.2f, .75f), 3);
            foreach (float x in new[] { -9.9f, 10.3f })
            {
                Art(root, "Canteiro_" + x, "FlowerPatch", new(x, -7.6f), new(2, 1.3f), 1);
                Art(root, "Arbusto_" + x, "Shrub", new(x, -6.5f), new(1.6f, 1.3f), 2);
            }
            Sign(root, "Placa_Escola", "ESCOLA", new(3.4f, -5.7f), new(2.1f, .48f));
            Sign(root, "Placa_Estacionamento", "ESTACIONAMENTO", new(-5.5f, -8.25f), new(4.5f, .48f));
            // Visible perimeter curbs also keep walking actors inside the expanded map.
            foreach (float x in new[] { -12.4f, 12.4f })
            {
                Wall(root, "Mureta_Superior_" + x, new(x, -7.3f), new(.3f, 3.2f));
                Wall(root, "Mureta_Inferior_" + x, new(x, -13.1f), new(.3f, 2.4f));
                Tile(root, "Acesso_Veiculos_" + x, "Street_Parking", new(Mathf.Sign(x) * 17.4f, -10.4f), new(10, 3), new(.19f, .23f, .26f), -1);
                var boundary = new GameObject("Limite_Via_" + x);
                boundary.transform.SetParent(root, false);
                boundary.transform.position = new Vector3(x, -10.4f);
                boundary.AddComponent<BoxCollider2D>().size = new Vector2(.3f, 3);
            }
            Wall(root, "Mureta_Sul", new(0, -14.15f), new(25, .3f));
            Wall(root, "Mureta_Noroeste", new(-10.15f, -5.85f), new(4.2f, .3f));
            Wall(root, "Mureta_Nordeste", new(10.85f, -5.85f), new(3.1f, .3f));
            VarginhaIndustrialSchoolFacade.Ensure(school);
        }

        private static void ApplyPassage(Transform root)
        {
            VarginhaOpenPassage.Remove(root, "Porta_Aberta_Esquerda");
            VarginhaOpenPassage.Remove(root, "Porta_Aberta_Direita");
            var wall = VarginhaPixelArtSprites.Create("SchoolWall", new Color(.35f, .39f, .35f));
            VarginhaOpenPassage.Ensure(root, "Entrada_Escola", Entrance, 2.5f, .7f, true, wall, wall);
        }

        private static SpriteRenderer Art(Transform root, string name, string motif, Vector2 position, Vector2 size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaSceneryArt.Create(motif, size);
            sr.sortingOrder = order;
            return sr;
        }

        private static SpriteRenderer Tile(Transform root, string name, string motif, Vector2 position, Vector2 size, Color tint, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = VarginhaPixelArtSprites.Create(motif, tint);
            sr.sortingOrder = order;
            if (motif != "SchoolWall")
            {
                go.transform.localScale = Vector3.one;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = size;
            }
            return sr;
        }

        private static void Wall(Transform root, string name, Vector2 position, Vector2 size)
        {
            var sr = Tile(root, name, "SchoolWall", position, size, new(.35f, .39f, .35f), 3);
            sr.gameObject.AddComponent<BoxCollider2D>().size = Vector2.one;
        }

        private static void Stripe(Transform root, string name, Vector2 position, Vector2 size, Color color)
        {
            var sr = Art(root, name, "Dust", position, size, 1);
            sr.color = color;
        }

        private static void Sign(Transform root, string name, string text, Vector2 position, Vector2 size)
        {
            Stripe(root, name + "_Fundo", position, size, new(.10f, .27f, .29f));
            root.Find(name + "_Fundo").GetComponent<SpriteRenderer>().sortingOrder = 4;
            var sign = new GameObject(name);
            sign.transform.SetParent(root, false);
            sign.transform.position = position;
            var label = sign.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 48;
            label.characterSize = .075f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(.92f, .87f, .70f);
            sign.GetComponent<MeshRenderer>().sortingOrder = 5;
        }
    }
}
