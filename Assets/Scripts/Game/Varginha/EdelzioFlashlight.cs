using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Warm, occluded torch cone. Vertices stop at scenery and never illuminate through walls.</summary>
    [DisallowMultipleComponent]
    public sealed class EdelzioFlashlight : MonoBehaviour
    {
        [SerializeField, Range(1f, 8f)] private float reach = 4.2f;
        [SerializeField, Range(0f, 1f)] private float baseIntensity = .48f;
        [SerializeField, Range(15f, 80f)] private float beamAngle = 48f;
        [SerializeField] private bool followCursor = true;
        [SerializeField] private LayerMask occluderLayers = ~0;
        private const int Rays = 80, Rings = 8;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[64];
        private readonly Vector3[] _vertices = new Vector3[(Rays + 1) * (Rings + 1)];
        private readonly Color[] _colors = new Color[(Rays + 1) * (Rings + 1)];
        private readonly List<SpriteRenderer> _furniture = new List<SpriteRenderer>(32);
        private readonly List<Rect> _footprints = new List<Rect>(32);
        private float _nextFurnitureScan = -1f;
        private EdelzioTopDownController _controller;
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private Material _material;
        private GameObject _light;
        private float _angle;
        private bool _hasDirection;
        public float Reach => reach;
        public Vector2 BeamOrigin { get; private set; }
        public Vector2 BeamDirection { get; private set; } = Vector2.down;

        private void Awake()
        {
            _controller = GetComponent<EdelzioTopDownController>();
            _light = new GameObject("Edelzio_Lanterna_Oclusao", typeof(MeshFilter), typeof(MeshRenderer));
            _light.transform.SetParent(transform, false);
            _mesh = new Mesh { name = "Lanterna_Malha_Ocluida" };
            _mesh.MarkDynamic();
            _light.GetComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = _light.GetComponent<MeshRenderer>();
            _renderer.sortingOrder = 4;
            var body = GetComponent<SpriteRenderer>();
            if (body != null) _renderer.sortingLayerID = body.sortingLayerID;
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            var shader = Resources.Load<Shader>("Varginha/FlashlightBeam");
            if (shader == null) { Debug.LogError("Shader da lanterna ausente.", this); enabled = false; return; }
            _material = new Material(shader) { name = "Lanterna_Luz_Quente" };
            _renderer.sharedMaterial = _material;
            var triangles = new int[Rays * Rings * 6];
            int index = 0;
            for (int ray = 0; ray < Rays; ray++) for (int ring = 0; ring < Rings; ring++)
            {
                int a = ray * (Rings + 1) + ring, b = a + Rings + 1;
                triangles[index++] = a; triangles[index++] = b; triangles[index++] = a + 1;
                triangles[index++] = a + 1; triangles[index++] = b; triangles[index++] = b + 1;
            }
            _mesh.vertices = _vertices;
            _mesh.triangles = triangles;
        }

        private void LateUpdate()
        {
            if (_renderer == null) return;
            bool visible = !VarginhaTravelCinematic.IsTravelling
                && _controller != null && _controller.CurrentSanity > 0
                && _controller.HasFlashlight && _controller.FlashlightActive
                && _controller.IsFlashlightEquippedInHotbar;
            _renderer.enabled = visible;
            if (!visible || (Time.timeScale <= 0f && _hasDirection)) return;
            Vector2 direction = _controller.FacingDirection;
            var cursor = VarginhaCombatCursor.Instance;
            if (followCursor && cursor != null && cursor.isActiveAndEnabled && !VarginhaWorldFeedback.IsHidden)
            {
                var aim = cursor.WorldAimPosition - (Vector2)transform.position;
                if (aim.sqrMagnitude > .12f) direction = aim.normalized;
            }
            if (direction.sqrMagnitude < .01f) direction = Vector2.down;
            float desired = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _angle = _hasDirection ? Mathf.LerpAngle(_angle, desired, 1f - Mathf.Exp(-22f * Time.deltaTime)) : desired;
            _hasDirection = true;
            BeamDirection = new Vector2(Mathf.Cos(_angle * Mathf.Deg2Rad), Mathf.Sin(_angle * Mathf.Deg2Rad));
            Vector2 hand = new Vector2(BeamDirection.x * .23f, BeamDirection.y * .16f - .08f);
            Vector2 centre = transform.position;
            // The emitter itself cannot be placed across a thin wall when the player hugs it.
            float arm = TraceDistance(centre, hand.normalized, hand.magnitude);
            BeamOrigin = centre + hand.normalized * arm;
            float glow = baseIntensity * (1f + Mathf.Sin(Time.time * 3.1f) * .018f);
            BuildBeam(BeamOrigin, BeamDirection, glow);
        }

        public float TraceDistance(Vector2 origin, Vector2 direction, float distance)
        {
            RefreshFurnitureFootprints();
            return TraceDistanceCached(origin, direction, distance);
        }

        private float TraceDistanceCached(Vector2 origin, Vector2 direction, float distance)
        {
            if (distance <= 0f) return 0f;
            direction.Normalize();
            if (direction.sqrMagnitude < .001f) return 0f;
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = occluderLayers };
            int count = Physics2D.Raycast(origin, direction, filter, _hits, distance);
            float nearest = distance;
            for (int i = 0; i < count; i++)
            {
                var collider = _hits[i].collider;
                if (collider == null || collider.isTrigger || collider.transform == transform
                    || collider.transform.IsChildOf(transform)
                    || collider.GetComponentInParent<EdelzioTopDownController>() != null
                    || collider.GetComponentInParent<VarginhaStudentAlly>() != null
                    || collider.GetComponentInParent<VarginhaStudentHostage>() != null
                    || collider.GetComponentInParent<VarginhaCombatTarget>() != null) continue;
                nearest = Mathf.Min(nearest, Mathf.Max(0f, _hits[i].distance - .018f));
            }
            foreach (var footprint in _footprints)
            {
                // These props deliberately have no gameplay collision. If Edelzio walks
                // inside one, it must not extinguish the entire beam around its emitter.
                if (footprint.Contains(origin)) continue;
                float hit = RayFootprintDistance(origin, direction, footprint, nearest);
                if (hit < nearest) nearest = Mathf.Max(0f, hit - .018f);
            }
            return nearest;
        }

        private void RefreshFurnitureFootprints()
        {
            // Discover late-built/replaced scenery at most once a second, not per ray.
            // Keep disabled candidates so re-enabling a prop takes effect immediately.
            if (Time.unscaledTime >= _nextFurnitureScan)
            {
                _nextFurnitureScan = Time.unscaledTime + 1f;
                _furniture.Clear();
                foreach (var candidate in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
                    if (VarginhaSoftLighting.IsFurniture(candidate.name)) _furniture.Add(candidate);
            }
            _footprints.Clear();
            foreach (var furniture in _furniture)
            {
                if (furniture == null || !furniture.enabled || !furniture.gameObject.activeInHierarchy
                    || furniture.sprite == null || (occluderLayers.value & (1 << furniture.gameObject.layer)) == 0) continue;
                var bounds = furniture.bounds;
                if (bounds.size.x <= 0f || bounds.size.y <= 0f) continue;
                // Match the floor footprint used by VarginhaSoftLighting's static shadows.
                _footprints.Add(new Rect(bounds.min.x + bounds.size.x * .07f, bounds.min.y,
                    bounds.size.x * .86f, bounds.size.y * .42f));
            }
        }

        private static float RayFootprintDistance(Vector2 origin, Vector2 direction, Rect footprint, float maximum)
        {
            float near = 0f, far = maximum;
            for (int axis = 0; axis < 2; axis++)
            {
                float min = axis == 0 ? footprint.xMin : footprint.yMin;
                float max = axis == 0 ? footprint.xMax : footprint.yMax;
                if (Mathf.Abs(direction[axis]) < .00001f)
                {
                    if (origin[axis] < min || origin[axis] > max) return maximum;
                    continue;
                }
                float a = (min - origin[axis]) / direction[axis];
                float b = (max - origin[axis]) / direction[axis];
                near = Mathf.Max(near, Mathf.Min(a, b));
                far = Mathf.Min(far, Mathf.Max(a, b));
                if (far < near) return maximum;
            }
            return near;
        }

        public void BuildBeam(Vector2 origin, Vector2 direction, float intensity)
        {
            RefreshFurnitureFootprints();
            float heading = Mathf.Atan2(direction.y, direction.x);
            float half = beamAngle * .5f * Mathf.Deg2Rad;
            float safeReach = Mathf.Clamp(reach, 1f, 8f);
            for (int ray = 0; ray <= Rays; ray++)
            {
                float across = ray / (float)Rays;
                float angle = heading + Mathf.Lerp(-half, half, across);
                Vector2 outward = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float clipped = TraceDistanceCached(origin, outward, safeReach);
                float edge = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - Mathf.Abs(across * 2f - 1f)) / .32f));
                for (int ring = 0; ring <= Rings; ring++)
                {
                    float radial = ring / (float)Rings;
                    float d = radial * clipped;
                    float falloff = Mathf.Pow(1f - d / safeReach, 1.8f);
                    // Fade the last strip at an obstacle; there is no bright hard end cap.
                    float rim = ring == Rings ? 0f : 1f;
                    int vertex = ray * (Rings + 1) + ring;
                    _vertices[vertex] = _light.transform.InverseTransformPoint((Vector3)(origin + outward * d));
                    _colors[vertex] = new Color(1f, .88f, .61f, intensity * edge * falloff * rim);
                }
            }
            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.RecalculateBounds();
        }

        private void OnDisable() { if (_renderer != null) _renderer.enabled = false; }
        private void OnDestroy()
        {
            if (_light != null) Destroy(_light);
            if (_mesh != null) Destroy(_mesh);
            if (_material != null) Destroy(_material);
        }
    }
}
