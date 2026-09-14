using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Cenografia compartilhada das fases 2 e 3. Os mapas continuam sendo montados
    /// em runtime, mas cada fase recebe uma identidade visual própria e legível.
    /// </summary>
    public static class VarginhaEnvironmentArt
    {
        private const string SchoolName = "Escola_3_Sistema_Ambiente";
        private const string DioceseName = "Igreja_Diocese_Ato_III";
        private const string SchoolMarker = "CenarioVisual_Escola_V2";
        private const string DioceseMarker = "CenarioVisual_Diocese_V2";

        // Uma única referência evita que o marcador, o carro de runtime e o
        // builder acabem usando vagas diferentes.
        public static readonly Vector3 FuscaParkingPosition = new(-4.75f, -1.2f, 0f);
        public static readonly Vector3 FuscaParkingSize = new(5.5f, 3f, 1f);

        public static Transform EnsureSchool(Transform root)
        {
            if (root == null) return null;
            var school = root.Find(SchoolName);
            if (school == null)
            {
                var schoolObject = new GameObject(SchoolName);
                schoolObject.transform.SetParent(root, false);
                school = schoolObject.transform;
            }
            if (school.Find(SchoolMarker) == null) BuildSchool(school);
            EnsureFuscaParking(school);
            VarginhaEnvironmentPolish.EnsureSchool(school);
            return school;
        }

        public static Transform EnsureDiocese(Transform root)
        {
            if (root == null) return null;
            var diocese = root.Find(DioceseName);
            if (diocese == null)
            {
                var dioceseObject = new GameObject(DioceseName);
                dioceseObject.transform.SetParent(root, false);
                diocese = dioceseObject.transform;
            }
            if (diocese.Find(DioceseMarker) == null) BuildDiocese(diocese);
            VarginhaEnvironmentPolish.EnsureDiocese(diocese);
            return diocese;
        }

        public static void BuildSchool(Transform parent)
        {
            if (parent == null) return;
            if (parent.Find(SchoolMarker) != null)
            {
                EnsureFuscaParking(parent);
                VarginhaEnvironmentPolish.EnsureSchool(parent);
                return;
            }
            var marker = new GameObject(SchoolMarker).transform;
            marker.SetParent(parent, false);

            Color floor = new(.57f, .54f, .46f);
            Color wall = new(.18f, .24f, .29f);
            for (int y = -5; y < 6; y++)
            for (int x = -7; x < 9; x++)
                CreateSprite(parent, "CenarioV2_Piso_Escola_" + x + "_" + y,
                    new Vector3(x + .5f, y + .5f, 0f), Vector3.one,
                    "SchoolFloor", ((x + y) & 1) == 0 ? floor : Color.Lerp(floor, Color.white, .045f), 0);

            CreateWall(parent, "CenarioV2_Parede_Norte", new Vector3(0f, 5.7f), new Vector3(16f, .7f, 1f), wall);
            CreateWall(parent, "CenarioV2_Parede_Sul", new Vector3(0f, -5.7f), new Vector3(16f, .7f, 1f), wall);
            CreateWall(parent, "CenarioV2_Parede_Oeste", new Vector3(-7.7f, 0f), new Vector3(.7f, 11f, 1f), wall);
            CreateWall(parent, "CenarioV2_Parede_Leste", new Vector3(8.7f, 0f), new Vector3(.7f, 11f, 1f), wall);
            CreateWall(parent, "CenarioV2_Divisoria_Sala", new Vector3(3.7f, 3.3f), new Vector3(7.5f, .45f, 1f), wall);
            CreateWall(parent, "CenarioV2_Divisoria_Fundo", new Vector3(-4.1f, -3.4f), new Vector3(7.2f, .45f, 1f), wall);

            // Vaga interna para o Fusca: longe da parede oeste, com margem para a
            // câmera e para a fila dos alunos durante a saída da fase.
            CreateSprite(parent, "CenarioV2_Vaga_Fusca", FuscaParkingPosition,
                FuscaParkingSize, "FuscaParking", new Color(.22f, .34f, .39f), 1);

            CreateSprite(parent, "CenarioV2_Lousa_Esquerda", new Vector3(-5.3f, 4.72f), new Vector3(2.25f, .56f, 1f),
                "SchoolBlackboard", new Color(.14f, .29f, .28f), 2);
            CreateSprite(parent, "CenarioV2_Lousa_Direita", new Vector3(5.55f, 4.72f), new Vector3(2.15f, .56f, 1f),
                "SchoolBlackboard", new Color(.12f, .27f, .25f), 2);
            CreateSprite(parent, "CenarioV2_Lousa_Fundo", new Vector3(-1.35f, -4.63f), new Vector3(2.1f, .5f, 1f),
                "SchoolBlackboard", new Color(.14f, .29f, .28f), 2);

            Vector3[] desks =
            {
                new(-5.5f, 3.55f), new(-3.25f, 3.55f), new(-5.5f, 2.35f), new(-3.25f, 2.35f),
                new(4.25f, 3.45f), new(6.45f, 3.45f), new(4.25f, 2.25f), new(6.45f, 2.25f),
                new(-5.55f, -4.45f), new(-3.25f, -4.45f), new(-.95f, -4.45f)
            };
            for (int i = 0; i < desks.Length; i++)
            {
                CreateSprite(parent, "CenarioV2_Carteira_" + i, desks[i], new Vector3(.88f, .62f, 1f),
                    "SchoolDesk", new Color(.47f, .27f, .14f), 2);
                CreateSprite(parent, "CenarioV2_Cadeira_" + i, desks[i] + Vector3.down * .47f, new Vector3(.42f, .36f, 1f),
                    "SchoolChair", new Color(.28f, .20f, .17f), 2);
            }

            Vector3[] lockers = { new(2.4f, -4.7f), new(3.35f, -4.7f), new(4.3f, -4.7f), new(5.25f, -4.7f), new(6.2f, -4.7f) };
            for (int i = 0; i < lockers.Length; i++)
                CreateSprite(parent, "CenarioV2_Armario_" + i, lockers[i], new Vector3(.7f, 1.05f, 1f),
                    "SchoolLocker", new Color(.30f, .42f, .49f), 2);

            Vector3[] windows = { new(-3.8f, 5.27f), new(0f, 5.27f), new(3.7f, 5.27f) };
            for (int i = 0; i < windows.Length; i++)
            {
                CreateSprite(parent, "CenarioV2_Janela_" + i, windows[i], new Vector3(1.15f, .42f, 1f),
                    "SchoolWindow", new Color(.28f, .55f, .72f), 2);
                CreateSprite(parent, "CenarioV2_Cortina_" + i, windows[i] + Vector3.left * .72f,
                    new Vector3(.42f, .72f, 1f), "SchoolCurtain", new Color(.62f, .18f, .22f), 3);
                CreateSprite(parent, "CenarioV2_Cortina_Dir_" + i, windows[i] + Vector3.right * .72f,
                    new Vector3(.42f, .72f, 1f), "SchoolCurtain", new Color(.62f, .18f, .22f), 3);
            }

            CreateSprite(parent, "CenarioV2_Relógio", new Vector3(1.65f, 5.2f), Vector3.one * .48f,
                "SchoolClock", new Color(.82f, .78f, .62f), 3);
            CreateSprite(parent, "CenarioV2_Quadro_Mural", new Vector3(-6.5f, .9f), new Vector3(.55f, 1.45f, 1f),
                "SchoolPoster", new Color(.74f, .48f, .18f), 3);
            CreateSprite(parent, "CenarioV2_Quadro_Mural_2", new Vector3(7.85f, 1.3f), new Vector3(.55f, 1.45f, 1f),
                "SchoolPoster", new Color(.24f, .52f, .64f), 3);
            CreateSprite(parent, "CenarioV2_Saida", new Vector3(7.88f, 4.72f), new Vector3(.85f, .3f, 1f),
                "SchoolExitSign", new Color(.24f, .75f, .42f), 4);
            EnsureFuscaParking(parent);
            VarginhaEnvironmentPolish.EnsureSchool(parent);
        }

        private static void EnsureFuscaParking(Transform parent)
        {
            if (parent == null) return;
            var parking = parent.Find("CenarioV2_Vaga_Fusca");
            if (parking == null)
                parking = CreateSprite(parent, "CenarioV2_Vaga_Fusca", FuscaParkingPosition,
                    Vector3.one, "FuscaParking", new Color(.22f, .34f, .39f), 1).transform;

            // Corrige mapas já serializados sem exigir que o usuário reconstrua a cena.
            parking.position = FuscaParkingPosition;
            parking.localScale = Vector3.one;
            var renderer = parking.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sprite = VarginhaSceneryArt.Create("Parking", FuscaParkingSize);
                renderer.sortingOrder = 1;
            }
        }

        public static void BuildDiocese(Transform parent)
        {
            if (parent == null) return;
            if (parent.Find(DioceseMarker) != null)
            {
                VarginhaEnvironmentPolish.EnsureDiocese(parent);
                return;
            }
            var marker = new GameObject(DioceseMarker).transform;
            marker.SetParent(parent, false);

            Color floor = new(.13f, .15f, .19f);
            for (int y = -6; y <= 6; y++)
            for (int x = -8; x <= 9; x++)
                CreateSprite(parent, "CenarioV2_Piso_Diocese_" + x + "_" + y,
                    new Vector3(x + .5f, y + .5f, 0f), Vector3.one,
                    "ChurchFloor", ((x + y) & 1) == 0 ? floor : Color.Lerp(floor, Color.white, .035f), 0);

            Color wall = new(.12f, .14f, .18f);
            CreateWall(parent, "CenarioV2_Parede_Norte_Diocese", new Vector3(.5f, 6.8f), new Vector3(18f, .7f, 1f), wall, "ChurchWall");
            CreateWall(parent, "CenarioV2_Parede_Sul_Diocese", new Vector3(.5f, -6.8f), new Vector3(18f, .7f, 1f), wall, "ChurchWall");
            CreateWall(parent, "CenarioV2_Parede_Oeste_Diocese", new Vector3(-8.8f, 0f), new Vector3(.7f, 13f, 1f), wall, "ChurchWall");
            CreateWall(parent, "CenarioV2_Parede_Leste_Diocese", new Vector3(9.8f, 0f), new Vector3(.7f, 13f, 1f), wall, "ChurchWall");

            Vector3[] sideWindows = { new(-8.35f, 4.15f), new(-8.35f, 1.25f), new(-8.35f, -1.65f), new(9.35f, 4.15f), new(9.35f, 1.25f), new(9.35f, -1.65f) };
            for (int i = 0; i < sideWindows.Length; i++)
                CreateSprite(parent, "CenarioV2_Vitral_Lateral_" + i, sideWindows[i], new Vector3(.46f, 1.45f, 1f),
                    "ChurchWindow_" + i, new Color(.28f, .48f, .74f), 2);
            Vector3[] frontWindows = { new(-5.1f, 6.28f), new(-1.7f, 6.28f), new(1.7f, 6.28f), new(5.1f, 6.28f) };
            for (int i = 0; i < frontWindows.Length; i++)
                CreateSprite(parent, "CenarioV2_Vitral_Fundo_" + i, frontWindows[i], new Vector3(1.28f, .45f, 1f),
                    "ChurchWindow_Top_" + i, new Color(.65f, .30f, .48f), 2);

            Vector3[] pews =
            {
                new(-5.6f, 3.4f), new(-2.9f, 3.4f), new(-5.6f, 1.75f), new(-2.9f, 1.75f),
                new(-5.6f, .1f), new(-2.9f, .1f), new(-5.6f, -1.55f), new(-2.9f, -1.55f),
                new(-5.6f, -3.2f), new(-2.9f, -3.2f)
            };
            for (int i = 0; i < pews.Length; i++)
                CreateSprite(parent, "CenarioV2_Banco_Igreja_" + i, pews[i], new Vector3(2.15f, .42f, 1f),
                    "ChurchPew", new Color(.29f, .15f, .10f), 2);

            CreateSprite(parent, "CenarioV2_Tapete_Altar", new Vector3(2.3f, .25f), new Vector3(4.8f, .78f, 1f),
                "ChurchRug", new Color(.45f, .12f, .15f), 1);
            CreateSprite(parent, "CenarioV2_Dais_Altar", new Vector3(5.2f, .25f), new Vector3(2.15f, 1.32f, 1f),
                "ChurchDais", new Color(.34f, .27f, .24f), 2);
            CreateSprite(parent, "CenarioV2_Altar_Visual", new Vector3(5.2f, .25f), new Vector3(1.55f, 1.35f, 1f),
                "ChurchAltar", new Color(.40f, .22f, .12f), 3);
            CreateSprite(parent, "CenarioV2_Selo", new Vector3(-1.1f, .25f), Vector3.one * 1.2f,
                "SealSymbol", new Color(.18f, .55f, .64f), 1);

            Vector3[] candles = { new(3.95f, 1.45f), new(6.45f, 1.45f), new(3.95f, -1.0f), new(6.45f, -1.0f) };
            for (int i = 0; i < candles.Length; i++)
                CreateSprite(parent, "CenarioV2_Vela_" + i, candles[i], Vector3.one * .48f,
                    "ChurchCandle", new Color(.83f, .56f, .20f), 4);
            CreateSprite(parent, "CenarioV2_Leitor", new Vector3(2.15f, 3.7f), new Vector3(.75f, 1.05f, 1f),
                "ChurchLectern", new Color(.32f, .17f, .11f), 3);
            CreateSprite(parent, "CenarioV2_Porta_Sacristia", new Vector3(-6.9f, -6.28f), new Vector3(1.1f, .4f, 1f),
                "ChurchDoor", new Color(.20f, .12f, .12f), 3);
            VarginhaEnvironmentPolish.EnsureDiocese(parent);
        }

        private static GameObject CreateSprite(Transform parent, string name, Vector3 position, Vector3 scale,
            string spriteId, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaPixelArtSprites.Create(spriteId, color);
            renderer.sortingOrder = sortingOrder;
            return go;
        }

        private static GameObject CreateWall(Transform parent, string name, Vector3 position, Vector3 scale,
            Color color, string spriteId = "SchoolWall")
        {
            var go = CreateSprite(parent, name, position, scale, spriteId, color, 3);
            go.AddComponent<BoxCollider2D>();
            return go;
        }
    }
}
