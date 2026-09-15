using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Continuous projection of one rigid door about its front hinge (no sprite-frame jumps).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FuscaDoorMotion : MonoBehaviour
    {
        private Mesh _mesh;
        private Material _material;
        private MeshRenderer _door;
        private SpriteRenderer _car;
        private readonly Vector3[] _vertices = new Vector3[4];
        public float OpenAmount { get; private set; }
        public Vector3 EntryPosition => transform.TransformPoint(Hinge + new Vector3(-Direction * Width * .65f, -Height * .64f, 0));
        public Vector3 SeatPosition => transform.TransformPoint(Hinge + new Vector3(-Direction * Width * .46f, 0, 0));
        private float Direction => _car != null && _car.flipX ? -1 : 1;
        private Bounds CarBounds => _car != null && _car.sprite != null ? _car.sprite.bounds : new Bounds(Vector3.zero, new Vector3(2.2f, 1.1f, 0));
        // The 100px car frame has substantial transparent padding below the body.
        // Fit the painted cabin aperture, not the height of the sprite canvas.
        private float Width => CarBounds.size.x * .255f;
        private float Height => CarBounds.size.y * .265f;
        private Vector3 Hinge => CarBounds.center + new Vector3(Direction * CarBounds.size.x * .105f, CarBounds.size.y * .07f, 0);

        public static Vector2 ProjectFreeEdge(float openness, float width)
        {
            float angle = Mathf.Clamp01(openness) * 68f * Mathf.Deg2Rad;
            return new Vector2(-Mathf.Cos(angle) * width, -Mathf.Sin(angle) * width * .53f);
        }

        public void SetOpenAmount(float amount)
        {
            EnsureGeometry();
            OpenAmount = Mathf.Clamp01(amount);
            Vector2 edge = ProjectFreeEdge(OpenAmount, Width);
            Vector3 free = new Vector3(edge.x * Direction, edge.y, 0);
            Vector3 hinge = Hinge;
            _vertices[0] = hinge + Vector3.down * Height * .5f;
            _vertices[1] = _vertices[0] + free;
            _vertices[2] = hinge + Vector3.up * Height * .5f + free;
            _vertices[3] = hinge + Vector3.up * Height * .5f;
            _mesh.vertices = _vertices; _mesh.RecalculateBounds();
            _door.sortingLayerID = _car.sortingLayerID; _door.sortingOrder = _car.sortingOrder + 2;
            _door.enabled = OpenAmount > .001f;
            _material.color = Color.Lerp(Color.white, new Color(.77f, .84f, .91f), OpenAmount * .5f);
        }

        private void EnsureGeometry()
        {
            if (_mesh != null) return;
            _car = GetComponent<SpriteRenderer>();
            var go = new GameObject("Porta_Fusca_Dobradica_Dianteira", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            var sprite = VarginhaPixelArtSprites.Create("FuscaDoor_Curved", new Color(.20f, .65f, .88f));
            _mesh = new Mesh { name = "Fusca_Porta_Articulada" };
            _mesh.MarkDynamic(); _mesh.vertices = _vertices;
            _mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            // Crop the procedural canvas to the leaf; forward edge is on the right of the artwork.
            _mesh.uv = new[] { new Vector2(.8125f, .1875f), new Vector2(.1875f, .1875f),
                new Vector2(.1875f, .9375f), new Vector2(.8125f, .9375f) };
            _mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            go.GetComponent<MeshFilter>().sharedMesh = _mesh;
            _material = new Material(Shader.Find("Sprites/Default")) { name = "Fusca_Porta", mainTexture = sprite.texture };
            _door = go.GetComponent<MeshRenderer>(); _door.sharedMaterial = _material;
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
            if (_material != null) Destroy(_material);
        }
    }
}
