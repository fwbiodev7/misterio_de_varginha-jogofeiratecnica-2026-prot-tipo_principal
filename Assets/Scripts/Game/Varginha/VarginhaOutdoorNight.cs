using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Static exterior night shade with warm, readable pools around working streetlights.</summary>
    public sealed class VarginhaOutdoorNight : MonoBehaviour
    {
        public const string RootName = "Noite_Exterior_Postes";
        private Texture2D _shadeTexture;
        private Sprite _shadeSprite;
        public int LampCount { get; private set; }

        public static void EnsureSchool(Transform school) => Ensure(school,
            Rect.MinMaxRect(-23, -15, 23, 7), Rect.MinMaxRect(-7.35f, -5.35f, 8.35f, 5.35f),
            new[] { new Vector2(-9.1f, -8.25f), new Vector2(9.8f, -8.25f),
                new Vector2(-8f, -12.5f), new Vector2(8f, -12.5f) });

        public static void EnsureHouse(Transform house) => Ensure(house,
            Rect.MinMaxRect(-11, -10, 30, 11), Rect.MinMaxRect(-8.6f, -6.6f, 8.6f, 8.6f),
            new[] { new Vector2(13.8f, 3.3f), new Vector2(16.6f, -4.3f), new Vector2(25.7f, 3.1f) });

        public static void RefreshHouseLamp(SpriteRenderer renderer)
        {
            var sprite = Resources.Load<Sprite>("Varginha/OutdoorLanternPost");
            if (renderer == null || sprite == null) return;
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Simple;
            float scale = 2.3f / sprite.bounds.size.y;
            renderer.transform.localScale = new Vector3(scale, scale, 1);
            renderer.color = Color.white;
        }

        private static void Ensure(Transform environment, Rect area, Rect indoor, Vector2[] posts)
        {
            if (environment == null) return;
            foreach (var lamp in environment.GetComponentsInChildren<SpriteRenderer>(true))
                if (lamp.name == "StreetLamp_Yard" || lamp.name.StartsWith("Poste_Noturno_")) RefreshHouseLamp(lamp);
            var previous = environment.Find(RootName);
            if (previous != null)
            {
                var shade = previous.Find("Sombra_Noturna");
                var renderer = shade != null ? shade.GetComponent<SpriteRenderer>() : null;
                if (renderer != null && renderer.sprite != null && renderer.sprite.texture != null
                    && previous.Find("Lanternas_Proporcionais_V3") != null) return;
                previous.gameObject.SetActive(false);
                previous.name += "_Replacing";
                if (Application.isPlaying) Destroy(previous.gameObject); else DestroyImmediate(previous.gameObject);
            }
            var go = new GameObject(RootName);
            go.transform.SetParent(environment, false);
            var night = go.AddComponent<VarginhaOutdoorNight>();
            new GameObject("Lanternas_Proporcionais_V3").transform.SetParent(go.transform, false);
            night.LampCount = posts.Length;
            for (int i = 0; i < posts.Length; i++)
            {
                // The original house lamp is already baked into the house hierarchy.
                if (environment.Find("StreetLamp_Yard") == null || i != posts.Length - 1)
                {
                    var post = Part(go.transform, "Poste_Noturno_" + i, "StreetLamp", posts[i], new(.65f, 2.3f), 6);
                    RefreshHouseLamp(post);
                }
                var pool = Part(go.transform, "Luz_Poste_Noturno_" + i, "Glow", posts[i] + Vector2.down * .75f,
                    new(6.8f, 5.3f), 2);
                pool.color = new Color(1, .72f, .33f, .72f);
                var halo = Part(go.transform, "Halo_Lampada_" + i, "Glow", posts[i] + new Vector2(.22f, .08f),
                    new(.7f, .55f), 25001);
                halo.color = new Color(1, .86f, .48f, .85f);
            }
            const float ppu = 20;
            int width = Mathf.CeilToInt(area.width * ppu), height = Mathf.CeilToInt(area.height * ppu);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector2 point = area.min + new Vector2((x + .5f) / ppu, (y + .5f) / ppu);
                Vector2 nearest = new(Mathf.Clamp(point.x, indoor.xMin, indoor.xMax), Mathf.Clamp(point.y, indoor.yMin, indoor.yMax));
                float exterior = Mathf.SmoothStep(0, 1, Vector2.Distance(point, nearest) / .55f);
                float illumination = 0;
                foreach (var post in posts)
                {
                    Vector2 delta = point - (post + Vector2.down * .75f);
                    float radius = new Vector2(delta.x / 3.4f, delta.y / 2.65f).magnitude;
                    illumination = Mathf.Max(illumination, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, 1, radius)));
                }
                pixels[y * width + x] = new Color(.015f, .025f, .085f, exterior * Mathf.Lerp(.64f, .1f, illumination));
            }
            night._shadeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            { name = "Noite_Exterior", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            night._shadeTexture.SetPixels32(pixels);
            night._shadeTexture.Apply(false, false);
            night._shadeSprite = Sprite.Create(night._shadeTexture, new Rect(0, 0, width, height), Vector2.one * .5f, ppu);
            var shadeObject = new GameObject("Sombra_Noturna");
            shadeObject.transform.SetParent(go.transform, false);
            shadeObject.transform.position = area.center;
            var shadeRenderer = shadeObject.AddComponent<SpriteRenderer>();
            shadeRenderer.sprite = night._shadeSprite;
            shadeRenderer.sortingOrder = 25000;
        }

        private static SpriteRenderer Part(Transform parent, string name, string motif, Vector2 position, Vector2 size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = VarginhaSceneryArt.Create(motif, size);
            renderer.sortingOrder = order;
            return renderer;
        }

        private void OnDestroy()
        {
            if (_shadeSprite != null) { if (Application.isPlaying) Destroy(_shadeSprite); else DestroyImmediate(_shadeSprite); }
            if (_shadeTexture != null) { if (Application.isPlaying) Destroy(_shadeTexture); else DestroyImmediate(_shadeTexture); }
        }
    }
}
