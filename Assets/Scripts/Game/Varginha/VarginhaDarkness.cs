using UnityEngine;

namespace Game.Varginha
{
    /// <summary>World-only darkness; the flashlight cuts an occluded opening through it.</summary>
    [DefaultExecutionOrder(500)]
    public sealed class VarginhaDarkness : MonoBehaviour
    {
        public const float ChurchOpacity = .90f;
        private EdelzioTopDownController _player;
        private bool _church;
        private Material _material;
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private float _surgeStarted = -1;
        private float _nextEyeScan;
        public float Opacity { get; private set; }
        public bool PowerSurgesActive => _surgeStarted >= 0;

        public static VarginhaDarkness Ensure(EdelzioTopDownController player, bool church)
        {
            if (player == null) return null;
            var effect = player.GetComponent<VarginhaDarkness>();
            if (effect == null) effect = player.gameObject.AddComponent<VarginhaDarkness>();
            effect._player = player;
            effect._church = church;
            return effect;
        }

        public void TriggerPowerSurges() => _surgeStarted = Time.time;

        private void Awake()
        {
            var layer = new GameObject("Escuridao_Mundo", typeof(MeshFilter), typeof(MeshRenderer));
            layer.transform.SetParent(transform, false);
            _mesh = new Mesh { name = "Escuridao_Quad" };
            _mesh.vertices = new[] { new Vector3(-.5f,-.5f), new Vector3(.5f,-.5f), new Vector3(.5f,.5f), new Vector3(-.5f,.5f) };
            _mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            layer.GetComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = layer.GetComponent<MeshRenderer>();
            _renderer.sortingOrder = 30000;
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            var shader = Resources.Load<Shader>("Varginha/WorldDarkness");
            if (shader == null) { Debug.LogError("Shader de escuridao ausente."); enabled = false; return; }
            _material = new Material(shader);
            _renderer.sharedMaterial = _material;
        }

        public static float SurgeOpacity(float elapsed)
        {
            // A short initial failure followed by spaced, irregular brownouts; no rapid strobe.
            if (elapsed < 0) return 0;
            float t = elapsed < 4 ? elapsed : (elapsed - 4) % 13.7f;
            if (elapsed >= 4 && t > 3.2f) return .08f;
            if (t < .7f) return Mathf.SmoothStep(.08f, .88f, t / .7f);
            if (t < 1.6f) return .88f;
            if (t < 2.3f) return Mathf.SmoothStep(.88f, .15f, (t - 1.6f) / .7f);
            return Mathf.Lerp(.15f, .72f, Mathf.Sin((t - 2.3f) * 2f) * .5f + .5f);
        }

        private void LateUpdate()
        {
            if (_renderer == null || _material == null) return;
            var camera = Camera.main;
            if (camera == null) return;
            Opacity = _church ? ChurchOpacity : SurgeOpacity(_surgeStarted < 0 ? -1 : Time.time - _surgeStarted);
            _renderer.enabled = Opacity > 0 && !VarginhaTravelCinematic.IsTravelling;
            var layer = _renderer.transform;
            layer.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 0);
            layer.rotation = Quaternion.identity;
            var parentScale = transform.lossyScale;
            layer.localScale = new Vector3(camera.orthographicSize * camera.aspect * 2.02f / parentScale.x,
                camera.orthographicSize * 2.02f / parentScale.y, 1);
            _material.SetFloat("_Darkness", Opacity);
            _material.SetFloat("_HouseOnly", _church ? 0 : 1);
            var beam = _player != null ? _player.GetComponent<EdelzioFlashlight>() : null;
            bool on = beam != null && beam.IsIlluminating;
            _material.SetFloat("_BeamOn", on ? 1 : 0);
            if (on)
            {
                _material.SetVector("_Origin", beam.BeamOrigin);
                _material.SetVector("_Direction", beam.BeamDirection);
                _material.SetFloat("_HalfAngle", beam.HalfAngleRadians);
                _material.SetFloatArray("_Distances", beam.OccludedReach);
            }
            if (_church && Time.time >= _nextEyeScan)
            {
                _nextEyeScan = Time.time + 1;
                foreach (var enemy in Object.FindObjectsByType<VarginhaCombatEnemy>(FindObjectsInactive.Exclude))
                    if (enemy.GetComponent<VarginhaGlowingEyes>() == null) enemy.gameObject.AddComponent<VarginhaGlowingEyes>();
            }
        }

        private void OnDisable() { if (_renderer != null) _renderer.enabled = false; }
        private void OnDestroy()
        {
            if (_renderer != null) Destroy(_renderer.gameObject);
            if (_mesh != null) Destroy(_mesh);
            if (_material != null) Destroy(_material);
        }
    }
}
