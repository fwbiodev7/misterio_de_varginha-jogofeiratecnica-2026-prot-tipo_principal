using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Varginha
{
    /// <summary>Shared ground footprints and depth rules for the house, school and church.</summary>
    public static class VarginhaWorldGeometry
    {
        private static bool Has(string name, params string[] parts) => Array.Exists(parts,
            part => name.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0);

        public static bool IsPost(string name) => Has(name, "StreetLamp", "Poste_Noturno", "LampPost");

        public static bool IsSolidProp(string name) => name.StartsWith("Mesa_", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Balcao_", StringComparison.OrdinalIgnoreCase) || IsPost(name) || Has(name,
            "Tree_", "Shrub_", "Arbusto_", "GardenBorder", "Planter", "Mailbox", "RainBarrel", "GardenBench",
            "Bed_", "Desk_", "CoffeeTable", "Bookshelf", "Kitchen_Cabinet", "Nightstand", "Dresser", "Chair_",
            "Stove_", "Fridge_", "Sofa_", "TV_Static", "Livros_Quarto", "Biblioteca_", "Livros_Corredor",
            "Arquivo_Sacristia", "Aparador_", "Samambaia_", "Planta_", "Plant_Indoor", "Banco_", "Carteira_",
            "Cadeira_", "Mesa_Professor", "Estante_", "Altar_Visual", "Leitor", "Fusca_1996", "FuscaVehicle");

        private static bool IsGround(string name) => Has(name, "Floor", "Piso", "Rug", "Tapete", "Passadeira",
            "Runner", "Driveway", "Street_Road", "RoadMarking", "Parking", "Vaga_", "Sombra", "Shadow",
            "Glow", "Luz_", "Halo_", "Reflexo", "Luar_", "Folhas", "Folhagem", "Leaves", "FlowerPatch",
            "Canteiro_Florido", "Mosaic", "Rosacea", "Puddle", "Dust", "Poeira", "Terreno_", "Jardim",
            "Asfalto", "Calcada", "Caminho_", "Soleira", "Faixa_", "Travessia", "Porch_Wood", "PorchSteps");

        private static bool IsActor(SpriteRenderer sr) => sr.GetComponent<EdelzioTopDownController>() != null
            || sr.GetComponent<VarginhaCombatTarget>() != null || sr.GetComponent<VarginhaStudentHostage>() != null
            || sr.GetComponent<VarginhaStudentAlly>() != null;

        public static void Ensure(Transform root)
        {
            if (root == null) return;
            var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                if (!sr.gameObject.activeInHierarchy || !sr.enabled || sr.sprite == null) continue;
                if (IsActor(sr)) { VarginhaWorldDepth.Ensure(sr, true); continue; }
                if (sr.GetComponentInParent<VarginhaWorldDepth>() != null && sr.GetComponent<VarginhaWorldDepth>() == null) continue;
                if (!IsSolidProp(sr.name)) continue;
                var footprint = SolidFootprint(sr);
                VarginhaWorldDepth.Ensure(sr, false, IsPost(sr.name), footprint);
            }
            foreach (var sr in renderers)
            {
                if (!sr.gameObject.activeInHierarchy || !sr.enabled || sr.sprite == null || sr.sortingOrder > 24000) continue;
                if (sr.GetComponent<VarginhaWorldDepth>() != null || sr.GetComponentInParent<VarginhaWorldDepth>() != null) continue;
                if (IsGround(sr.name)) continue;
                // Portal jambs are purely decorative: never close a doorway with a new collider.
                bool passage = Has(sr.name, "Passagem", "Montante", "Verga") || Has(sr.transform.parent?.name ?? "", "Passagem_");
                if (sr.sortingOrder < 3 && !passage && sr.GetComponent<InteractableProp>() == null) continue;
                var support = FindSupport(sr, renderers);
                VarginhaWorldDepth.Ensure(sr, false, false, sr.GetComponent<Collider2D>(), support, support != null ? 2 : 0);
            }
            Physics2D.SyncTransforms();
            VarginhaSchoolNavigation.Invalidate();
        }

        private static Collider2D SolidFootprint(SpriteRenderer sr)
        {
            var existing = sr.GetComponents<Collider2D>();
            foreach (var collider in existing) if (!collider.isTrigger && !IsPost(sr.name) && !sr.name.StartsWith("Tree_")) return collider;
            var child = sr.transform.Find("Colisao_Base_Cenario");
            if (child == null)
            {
                child = new GameObject("Colisao_Base_Cenario").transform;
                child.SetParent(sr.transform, false);
            }
            var box = child.GetComponent<BoxCollider2D>();
            if (box == null) box = child.gameObject.AddComponent<BoxCollider2D>();
            var bounds = sr.bounds;
            bool post = IsPost(sr.name), tree = sr.name.StartsWith("Tree_");
            float width = bounds.size.x * (post ? .22f : tree ? .22f : .82f);
            float height = bounds.size.y * (post ? .09f : tree ? .10f : .38f);
            width = Mathf.Max(.12f, width); height = Mathf.Max(.12f, height);
            Vector3 center = new(bounds.center.x, bounds.min.y + bounds.size.y * (post || tree ? .10f : .24f), sr.transform.position.z);
            child.position = center;
            child.gameObject.layer = sr.gameObject.layer;
            var scale = child.lossyScale;
            box.size = new(width / Mathf.Max(.001f, Mathf.Abs(scale.x)), height / Mathf.Max(.001f, Mathf.Abs(scale.y)));
            box.offset = Vector2.zero; box.isTrigger = false;
            if (post || tree) foreach (var collider in existing) if (!collider.isTrigger) collider.enabled = false;
            return box;
        }

        private static Transform FindSupport(SpriteRenderer sr, SpriteRenderer[] renderers)
        {
            bool accessory = Has(sr.name, "Coffee_Cup", "Notebook_TI", "Radio_Office", "Lamp_Desk", "Abajur_",
                "Caneca_", "Jornal_Mesa", "Documento_Mesa", "Vaso_Mesa", "Prato_Mesa");
            if (!accessory) return null;
            Transform best = null; float area = float.MaxValue;
            foreach (var other in renderers)
            {
                if (other == sr || !other.enabled || !IsSolidProp(other.name)
                    || !Has(other.name, "Desk_", "CoffeeTable", "Nightstand", "Mesa_", "Balcao_")) continue;
                var b = other.bounds;
                if (!b.Contains(new Vector3(sr.bounds.center.x, sr.bounds.center.y, b.center.z))) continue;
                float candidate = b.size.x * b.size.y;
                if (candidate < area) { best = other.transform; area = candidate; }
            }
            return best;
        }

        public static void EnsureScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects()) Ensure(root.transform);
        }
    }
}
