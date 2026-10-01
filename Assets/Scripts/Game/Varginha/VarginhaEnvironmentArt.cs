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
        private const string SchoolMarker = VarginhaSchoolClassroomLayout.Marker;
        private const string DioceseMarker = "CenarioVisual_Diocese_V2";

        // Uma única referência evita que o marcador, o carro de runtime e o
        // builder acabem usando vagas diferentes.
        public static readonly Vector3 FuscaParkingPosition = new(-5.5f, -10.4f, 0f);
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
            VarginhaSchoolExterior.Ensure(school);
            VarginhaEnvironmentPolish.EnsureSchool(school);
            VarginhaOutdoorNight.EnsureSchool(school);
            VarginhaWorldGeometry.Ensure(school);
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
            EnsureDioceseGround(diocese);
            VarginhaEnvironmentPolish.EnsureDiocese(diocese);
            VarginhaWorldGeometry.Ensure(diocese);
            return diocese;
        }

        public static void BuildSchool(Transform parent)
        {
            if (parent == null) return;
            VarginhaSchoolClassroomLayout.Build(parent);
            EnsureFuscaParking(parent);
            VarginhaSchoolExterior.Ensure(parent);
            VarginhaEnvironmentPolish.EnsureSchool(parent);
            VarginhaOutdoorNight.EnsureSchool(parent);
            VarginhaWorldGeometry.Ensure(parent);
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
            EnsureDioceseGround(parent);
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

        private static void EnsureDioceseGround(Transform parent)
        {
            if (parent.Find("Piso_Continuo_Diocese") != null) return;
            var ground = CreateSprite(parent, "Piso_Continuo_Diocese", new Vector3(.5f, 0f),
                Vector3.one, "ChurchFloor", new Color(.25f, .26f, .29f), -1);
            var renderer = ground.GetComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(18f, 13f);
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
