using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Bakes the static scenery lights once per environment. One transparent floor layer avoids
    /// stacked beam seams, while finite-width sources cast penumbras behind solid scenery.
    /// No render-pipeline dependency, post-process blur of the scene, or per-frame texture uploads.
    /// </summary>
    public sealed class VarginhaSoftLighting : MonoBehaviour
    {
        public const string LayerName = "Luz_Difusa_Composta_V5";
        private Texture2D _texture;
        private Sprite _sprite;
        public int SourceCount { get; private set; }
        public int OccluderCount { get; private set; }

        public static bool IsFurniture(string name) => name.Contains("Carteira_") || name.Contains("Armario_")
            || name.Contains("Banco_Igreja_") || name.Contains("Altar_Visual") || name.Contains("Leitor")
            || name.StartsWith("Bookshelf") || name.StartsWith("Kitchen_") || name.StartsWith("Sofa_")
            || name.StartsWith("Dresser_") || name.StartsWith("Bed_") || name.Contains("Pilastra_")
            || name.Contains("Arquivo_") || name.Contains("Biblioteca_");

        public static void Build(Transform environment, Transform decor)
        {
            var sources = new List<SpriteRenderer>();
            var occluders = new List<Rect>();
            Bounds bounds = default;
            foreach (var renderer in decor.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (!(renderer.name.StartsWith("Luz_") || renderer.name.StartsWith("Luar_") || renderer.name.StartsWith("Reflexo_Vitral_"))) continue;
                if (renderer.sprite == null) continue;
                if (sources.Count == 0) bounds = renderer.bounds; else bounds.Encapsulate(renderer.bounds);
                sources.Add(renderer);
            }
            if (sources.Count == 0) return;
            foreach (var renderer in environment.GetComponentsInChildren<SpriteRenderer>(true))
            {
                bool wall = renderer.name.Contains("Parede_") || renderer.name.StartsWith("Wall_");
                if (!IsFurniture(renderer.name) && !wall) continue;
                renderer.sortingOrder = Mathf.Max(2, renderer.sortingOrder);
                var b = renderer.bounds;
                if (b.size.x <= 0 || b.size.y <= 0) continue;
                // A furniture footprint, not its entire upright sprite, casts the shadow.
                occluders.Add(wall ? new Rect(b.min.x, b.min.y, b.size.x, b.size.y)
                    : new Rect(b.min.x + b.size.x * .07f, b.min.y, b.size.x * .86f, b.size.y * .42f));
            }
            const float ppu = 32;
            int width = Mathf.CeilToInt(bounds.size.x * ppu) + 2;
            int height = Mathf.CeilToInt(bounds.size.y * ppu) + 2;
            Vector2 origin = (Vector2)bounds.min - Vector2.one / ppu;
            var rgb = new Vector3[width * height];
            var energy = new float[rgb.Length];
            var weights = new float[rgb.Length];
            foreach (var source in sources)
            {
                var b = source.bounds;
                var sb = source.sprite.bounds;
                var inverse = source.transform.worldToLocalMatrix;
                bool beam = source.name.StartsWith("Reflexo_") || source.name.StartsWith("Luar_") || source.name.StartsWith("Luz_Janela_");
                var localOrigin = beam ? new Vector3(source.flipX ? sb.max.x : sb.min.x, sb.center.y, 0) : sb.center;
                Vector2 emitter = source.transform.TransformPoint(localOrigin);
                int minX = Mathf.Clamp(Mathf.FloorToInt((b.min.x - origin.x) * ppu), 0, width - 1);
                int maxX = Mathf.Clamp(Mathf.CeilToInt((b.max.x - origin.x) * ppu), 0, width - 1);
                int minY = Mathf.Clamp(Mathf.FloorToInt((b.min.y - origin.y) * ppu), 0, height - 1);
                int maxY = Mathf.Clamp(Mathf.CeilToInt((b.max.y - origin.y) * ppu), 0, height - 1);
                for (int y = minY; y <= maxY; y++) for (int x = minX; x <= maxX; x++)
                {
                    Vector2 point = origin + new Vector2((x + .5f) / ppu, (y + .5f) / ppu);
                    var local = inverse.MultiplyPoint3x4(point);
                    float u = (local.x - sb.min.x) / sb.size.x, v = (local.y - sb.min.y) / sb.size.y;
                    if (u < 0 || u > 1 || v < 0 || v > 1) continue;
                    if (source.flipX) u = 1 - u;
                    if (source.flipY) v = 1 - v;
                    Color sample = source.sprite.texture.GetPixelBilinear(u, v) * source.color;
                    if (sample.a < .001f) continue;
                    float visibility = Visibility(emitter, point, occluders, beam ? Mathf.Clamp(sb.size.y * .30f, .16f, .8f) : .24f);
                    float a = sample.a * visibility;
                    int i = y * width + x;
                    float weight = a * a;
                    rgb[i] += new Vector3(sample.r, sample.g, sample.b) * weight;
                    weights[i] += weight;
                    energy[i] += weight * weight;
                }
                source.enabled = false; // Retain source transforms for authored inspection and repair.
                var ambient = source.GetComponent<VarginhaAmbientPixelEffect>();
                if (ambient != null) ambient.enabled = false;
            }
            var pixels = new Color32[rgb.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Vector3 color = weights[i] > 0 ? rgb[i] / weights[i] : Vector3.zero;
                float alpha = Mathf.Min(.38f, Mathf.Pow(energy[i], .25f));
                pixels[i] = new Color(color.x, color.y, color.z, alpha);
            }
            var go = new GameObject(LayerName);
            go.transform.SetParent(decor, false);
            go.transform.position = new Vector3(origin.x + width / ppu * .5f, origin.y + height / ppu * .5f, .01f);
            var baked = go.AddComponent<VarginhaSoftLighting>();
            baked.SourceCount = sources.Count; baked.OccluderCount = occluders.Count;
            baked._texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            { name = LayerName, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            baked._texture.SetPixels32(pixels); baked._texture.Apply(false, false);
            baked._sprite = Sprite.Create(baked._texture, new Rect(0, 0, width, height), Vector2.one * .5f, ppu, 0, SpriteMeshType.FullRect);
            var output = go.AddComponent<SpriteRenderer>(); output.sprite = baked._sprite; output.sortingOrder = 1;
        }

        private static float Visibility(Vector2 emitter, Vector2 point, List<Rect> blockers, float sourceHalfWidth)
        {
            Vector2 direction = point - emitter;
            if (direction.sqrMagnitude < .01f) return 1;
            Vector2 side = new Vector2(-direction.y, direction.x).normalized * sourceHalfWidth;
            float visible = 0;
            // Nine samples across an area source produce graduated, non-hard shadow edges.
            for (int tap = -4; tap <= 4; tap++)
            {
                Vector2 from = emitter + side * (tap / 4f);
                bool blocked = false;
                foreach (var box in blockers)
                {
                    if (box.Contains(from)) continue;
                    if (SegmentIntersects(from, point, box)) { blocked = true; break; }
                }
                if (!blocked) visible += 1f / 9;
            }
            return .16f + .84f * visible; // ambient bounce remains in the umbra.
        }

        private static bool SegmentIntersects(Vector2 from, Vector2 to, Rect box)
        {
            float near = 0, far = 1;
            Vector2 direction = to - from;
            for (int axis = 0; axis < 2; axis++)
            {
                float d = direction[axis], start = from[axis];
                float min = axis == 0 ? box.xMin : box.yMin, max = axis == 0 ? box.xMax : box.yMax;
                if (Mathf.Abs(d) < .00001f) { if (start < min || start > max) return false; continue; }
                float a = (min - start) / d, b = (max - start) / d;
                near = Mathf.Max(near, Mathf.Min(a, b)); far = Mathf.Min(far, Mathf.Max(a, b));
                if (far < near) return false;
            }
            return near < .995f && far > .005f;
        }

        private void OnDestroy()
        {
            if (Application.isPlaying) { if (_sprite != null) Destroy(_sprite); if (_texture != null) Destroy(_texture); }
            else { if (_sprite != null) DestroyImmediate(_sprite); if (_texture != null) DestroyImmediate(_texture); }
        }
    }
}
