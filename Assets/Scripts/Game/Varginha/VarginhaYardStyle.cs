using UnityEngine;

namespace Game.Varginha
{
    /// <summary>The house's walnut, brass and forest-green palette continues into its garden.</summary>
    public static class VarginhaYardStyle
    {
        public static void Apply(Transform house)
        {
            if (house == null || VarginhaHouseReferenceArt.YardSprite("Grass") == null) return;
            foreach (var renderer in house.GetComponentsInChildren<SpriteRenderer>(true))
            {
                string id = renderer.name;
                if (id.StartsWith("Tree_"))
                {
                    var original = Resources.Load<Sprite>("Varginha/TravelPixel/TreeReference");
                    if (original != null)
                    {
                        renderer.sprite = original;
                        renderer.drawMode = SpriteDrawMode.Simple;
                        renderer.color = Color.white;
                        renderer.sharedMaterial = Resources.Load<Material>("Varginha/OriginalTreeTone");
                    }
                    continue;
                }
                string motif = null;
                if (id.StartsWith("Floor_Yard")) motif = "Grass";
                else if (id.StartsWith("Driveway_")) motif = renderer.transform.position.x < 12.5f ? "Deck" : "Paving";
                else if (id == "Porch_Wood") motif = "Deck";
                else if (id.StartsWith("Shrub_")) motif = "Shrub";
                else if (id.StartsWith("FlowerPatch_")) motif = "Flowers";
                else if (id.StartsWith("GardenBorder_")) motif = "Planter";
                else if (id.StartsWith("Folhas_Quintal_")) motif = "Leaves";
                else if (id == "Mailbox_Yard") motif = "Mailbox";
                else if (id == "StreetLamp_Yard") motif = "Lantern";
                if (motif == null) continue;
                var sprite = VarginhaHouseReferenceArt.YardSprite(motif);
                if (sprite == null) continue;
                Vector2 size = renderer.drawMode != SpriteDrawMode.Simple ? renderer.size
                    : renderer.sprite != null ? renderer.sprite.bounds.size : Vector2.one;
                Vector3 scale = renderer.transform.localScale;
                // Legacy garden borders were thin ground strips; a fern planter needs its full height.
                if (motif == "Planter" && size.y * Mathf.Abs(scale.y) < .55f)
                    size = new Vector2(3.2f / Mathf.Max(.01f, Mathf.Abs(scale.x)),
                        1.45f / Mathf.Max(.01f, Mathf.Abs(scale.y)));
                if (renderer.sprite == sprite && renderer.size == size) continue;
                renderer.sprite = sprite;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = size;
                renderer.transform.localScale = scale;
                renderer.color = motif == "Grass" ? new Color(.83f, .9f, .8f) : Color.white;
            }
            var scope = house.parent != null ? house.parent : house;
            foreach (var car in scope.GetComponentsInChildren<FuscaLevelExit>(true))
            {
                var renderer = car.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = Resources.Load<Material>("Varginha/OriginalFuscaTone");
                    renderer.color = Color.white;
                }
            }
            if (house.Find("House512_Yard") != null) return;
            var root = new GameObject("House512_Yard").transform;
            root.SetParent(house, false);
            foreach (var name in new[] { "Fence_North", "Fence_South", "Fence_East" })
            {
                var fence = house.Find(name);
                if (fence == null) continue;
                var original = fence.GetComponent<SpriteRenderer>();
                if (original == null) continue;
                original.enabled = false;
                bool vertical = name == "Fence_East";
                var sprite = VarginhaHouseReferenceArt.YardSprite(vertical ? "FenceVertical" : "Fence");
                Vector2 size = vertical ? new Vector2(.6f, 16) : new Vector2(18, 1.05f);
                var visual = Part(root, name, sprite, fence.position, size, 3);
                visual.drawMode = SpriteDrawMode.Tiled;
                var scale = vertical ? new Vector2(size.x / sprite.bounds.size.x, 2 / sprite.bounds.size.y)
                    : new Vector2(2.6f / sprite.bounds.size.x, size.y / sprite.bounds.size.y);
                visual.size = new Vector2(size.x / scale.x, size.y / scale.y);
                visual.transform.localScale = new Vector3(scale.x, scale.y, 1);
            }
            var bench = Part(root, "GardenBench", VarginhaHouseReferenceArt.YardSprite("Bench"),
                new Vector2(11.5f, 3.7f), new Vector2(2.3f, 1.4f), 3);
            var benchCollider = bench.gameObject.AddComponent<BoxCollider2D>();
            benchCollider.size = new Vector2(2.1f, .5f);
            benchCollider.offset = Vector2.down * .35f;
            var barrel = Part(root, "RainBarrel", VarginhaHouseReferenceArt.YardSprite("Barrel"),
                new Vector2(11.15f, -3.7f), new Vector2(.95f, 1.2f), 3);
            var barrelCollider = barrel.gameObject.AddComponent<BoxCollider2D>();
            barrelCollider.size = new Vector2(.75f, .65f);
            barrelCollider.offset = Vector2.down * .25f;
            Part(root, "PorchLantern", VarginhaHouseReferenceArt.YardSprite("Lantern"),
                new Vector2(9.85f, 1.85f), new Vector2(.6f, 1.05f), 4);
            var steps = Part(root, "PorchSteps", VarginhaHouseReferenceArt.YardSprite("Steps"),
                new Vector2(12.6f, 0), new Vector2(2.9f, .65f), 2);
            steps.transform.rotation = Quaternion.Euler(0, 0, 90);
            Part(root, "GardenGate", VarginhaHouseReferenceArt.YardSprite("Gate"),
                new Vector2(22.5f, 8), new Vector2(2.2f, 1.25f), 4);
        }

        private static SpriteRenderer Part(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, int order)
        {
            var go = new GameObject("House512_" + name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            go.transform.localScale = Vector3.one;
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
