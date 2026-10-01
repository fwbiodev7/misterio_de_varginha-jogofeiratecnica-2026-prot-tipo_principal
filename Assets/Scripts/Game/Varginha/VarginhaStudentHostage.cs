using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Aluno mantido como refém até que os subordinados ETs sejam derrotados.</summary>
    public sealed class VarginhaStudentHostage : MonoBehaviour
    {
        [SerializeField] private string studentName = "Aluno";
        [SerializeField] private float followSpeed = 2.6f;

        private Transform _fusca;
        private Rigidbody2D _body;
        private CircleCollider2D _bodyCollider;
        private Vector3 _carOffset;
        private SpriteRenderer _renderer;
        private SpriteRenderer _cageRenderer;
        private bool _released;
        private bool _arrived;
        private Transform _school;
        private readonly List<Vector2> _path = new();
        private Vector2 _pathDestination;
        private float _nextPathTime;
        private Vector2 _arrivalPoint;
        private Vector2 _movementTarget;
        private bool _hasMovementTarget;
        private static PhysicsMaterial2D _movementMaterial;

        public string StudentName => studentName;
        public bool IsReleased => _released;
        public bool IsCaged => !_released;
        public bool IsAtFusca => _arrived;

        private void Awake()
        {
            EnsurePhysicsBody();
        }

        private void EnsurePhysicsBody()
        {
            _body = GetComponent<Rigidbody2D>();
            if (_body == null) _body = gameObject.AddComponent<Rigidbody2D>();
            _body.bodyType = RigidbodyType2D.Dynamic;
            _body.gravityScale = 0f;
            _body.constraints = RigidbodyConstraints2D.FreezeRotation;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;

            _bodyCollider = GetComponent<CircleCollider2D>();
            if (_bodyCollider == null) _bodyCollider = gameObject.AddComponent<CircleCollider2D>();
            _bodyCollider.radius = .22f;
            _bodyCollider.isTrigger = false;
            if (_movementMaterial == null)
                _movementMaterial = new PhysicsMaterial2D("StudentHostage_NoFriction") { friction = 0f, bounciness = 0f };
            _bodyCollider.sharedMaterial = _movementMaterial;

            foreach (var other in FindObjectsByType<VarginhaStudentHostage>(FindObjectsInactive.Include))
            {
                if (other == null || other == this) continue;
                foreach (var otherCollider in other.GetComponentsInChildren<Collider2D>(true))
                    if (otherCollider != _bodyCollider) Physics2D.IgnoreCollision(_bodyCollider, otherCollider, true);
            }
        }

        /// <summary>
        /// Reaplica a apresentação visual quando uma cena antiga foi salva sem a jaula
        /// ou com o SpriteRenderer desativado. Não altera o estado de resgate.
        /// </summary>
        public void EnsurePresentation(int sortingOrder = 8)
        {
            var animation = GetComponent<VarginhaStudentAnimation>();
            if (animation == null) animation = gameObject.AddComponent<VarginhaStudentAnimation>();
            animation.Configure(studentName);
            _renderer = _renderer != null ? _renderer : GetComponent<SpriteRenderer>();
            if (_renderer != null)
            {
                if (_renderer.sprite == null || studentName == "Fabio")
                    _renderer.sprite = VarginhaPixelArtSprites.Create("Student_" + studentName, new Color(.25f, .52f, .88f));
                _renderer.sortingOrder = sortingOrder;
                _renderer.enabled = true;
            }

            EnsureCage();
            if (_cageRenderer != null)
            {
                _cageRenderer.sortingOrder = sortingOrder + 2;
                _cageRenderer.enabled = !_released;
            }
        }

        public void Configure(string name, Color shirtColor)
        {
            studentName = name;
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null)
                _renderer.sprite = VarginhaPixelArtSprites.Create("Student_" + name, shirtColor);
            var animation = GetComponent<VarginhaStudentAnimation>();
            if (animation == null) animation = gameObject.AddComponent<VarginhaStudentAnimation>();
            animation.Configure(studentName);
            EnsureCage();
        }

        public void ReleaseTo(Transform fusca, int index)
        {
            ReleaseTo(fusca, index, null);
        }

        public void ReleaseTo(Transform fusca, int index, Transform leader)
        {
            if (_released || fusca == null) return;
            foreach (var seat in FindObjectsByType<VarginhaClassroomSeat>(FindObjectsInactive.Include)) seat.Vacate(this);
            _released = true;
            _fusca = fusca;
            _school = GameObject.Find("Escola_3_Sistema_Ambiente")?.transform;
            // A fila do Fusca usa um espaço compacto; durante o acompanhamento eles
            // mantêm uma formação 3x3 atrás do Edelzio, evitando que um sprite cubra os outros.
            int column = index % 3;
            int row = index / 3;
            _carOffset = new Vector3(-1.35f - column * .70f, (row - 1) * .82f, 0f);
            if (_renderer != null) _renderer.sortingOrder = 8 + Mathf.Clamp(index, 0, 8);
            if (_renderer != null) _renderer.color = Color.white;
            if (_cageRenderer != null) _cageRenderer.enabled = false;
        }

        private void Update()
        {
            _hasMovementTarget = false;
            if (!_released && _cageRenderer != null)
            {
                // O brilho pulsa para comunicar que os alunos ainda estão presos.
                float pulse = .82f + Mathf.Sin(Time.unscaledTime * 3.5f) * .12f;
                _cageRenderer.color = new Color(1f, 1f, 1f, pulse);
            }
            if (!_released || _fusca == null || _arrived) return;
            Vector3 destination = _fusca.position + _carOffset;
            // A posição interpolada do desenho fica atrasada em relação à física.
            // Planejar a partir dela pode cortar quinas antes de chegar ao waypoint.
            Vector2 current = _body != null ? _body.position : (Vector2)transform.position;

            Vector2 next = destination;
            if (_school == null) _arrivalPoint = destination;
            if (_school != null)
            {
                if (Time.time >= _nextPathTime && (_path.Count == 0 ||
                    ((Vector2)destination - _pathDestination).sqrMagnitude > .36f))
                {
                    VarginhaSchoolNavigation.FindPath(_school, current, destination, _path);
                    _pathDestination = destination;
                    _nextPathTime = Time.time + .65f;
                    // Navigation can end beside an obstructed slot. Accept that reachable
                    // point, rather than waiting forever for an exact point inside a collider.
                    _arrivalPoint = _path.Count > 0 ? _path[_path.Count - 1] : (Vector2)destination;
                }
                while (_path.Count > 0 && Vector2.Distance(current, _path[0]) < .001f)
                    _path.RemoveAt(0);
                // Take the farthest visible waypoint, keeping every movement segment clear of walls.
                while (_path.Count > 1 && VarginhaSchoolNavigation.CanNavigateSegment(current, _path[1])) _path.RemoveAt(0);
                if (_path.Count == 0 && Vector2.Distance(current, _arrivalPoint) >= .2f) return;
                if (_path.Count == 0) next = _arrivalPoint;
                if (_path.Count > 0) next = _path[0];
            }
            _movementTarget = next;
            _hasMovementTarget = true;

            transform.localScale = Vector3.one;

            if (Vector2.Distance(current, destination) < .3f ||
                (_school != null && _path.Count == 0 && Vector2.Distance(current, _arrivalPoint) < .2f &&
                 Vector2.Distance(current, destination) < 1.5f))
            {
                _arrived = true;
                _hasMovementTarget = false;
                if (_body != null)
                {
                    _body.linearVelocity = Vector2.zero;
                    // Finaliza também a interpolação visual antes de anunciar a chegada.
                    _body.interpolation = RigidbodyInterpolation2D.None;
                    transform.position = new Vector3(current.x, current.y, transform.position.z);
                }
                VarginhaPhase2Controller.NotifyStudentAtFusca(this);
            }
        }

        private void FixedUpdate()
        {
            if (_body == null) EnsurePhysicsBody();
            if (_body == null) return;
            if (!_released || !_hasMovementTarget)
            {
                _body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 next = Vector2.MoveTowards(_body.position, _movementTarget, followSpeed * Time.fixedDeltaTime);
            if (_school != null && !VarginhaSchoolNavigation.CanWalkSegment(_body.position, next))
            {
                _body.linearVelocity = Vector2.zero;
                _hasMovementTarget = false;
                _path.Clear();
                _nextPathTime = 0f;
                return;
            }
            _body.MovePosition(next);
        }

        private void OnGUI()
        {
            if (VarginhaWorldFeedback.IsHidden) return;
            if (_renderer == null || !_renderer.enabled) return;
            var camera = Camera.main;
            if (camera == null) return;
            Vector3 screen = camera.WorldToScreenPoint(transform.position + Vector3.up * .62f);
            if (screen.z <= 0f) return;
            screen.y = Screen.height - screen.y;
            var old = GUI.color;
            GUI.color = _released ? new Color(.65f, 1f, .74f) : new Color(1f, .76f, .66f);
            GUI.Label(new Rect(screen.x - 52f, screen.y - 10f, 104f, 20f), studentName, GUI.skin.label);
            GUI.color = old;
        }

        private void EnsureCage()
        {
            if (_cageRenderer != null) return;
            var existing = transform.Find("Jaula_ET");
            var cage = existing != null ? existing.gameObject : new GameObject("Jaula_ET");
            cage.transform.SetParent(transform, false);
            cage.transform.localPosition = Vector3.zero;
            cage.transform.localScale = Vector3.one;
            // Não use ?? aqui: componentes destruídos pelo Unity continuam sendo
            // referências não nulas para o C#, mas são nulos para a engine.
            _cageRenderer = cage.GetComponent<SpriteRenderer>();
            if (_cageRenderer == null)
                _cageRenderer = cage.AddComponent<SpriteRenderer>();
            if (_cageRenderer == null) return;
            int studentOrder = _renderer != null ? _renderer.sortingOrder : 5;
            _cageRenderer.sortingOrder = studentOrder + 2;
            _cageRenderer.sprite = VarginhaPixelArtSprites.Create("HostageCage_" + studentName, new Color(.2f, .78f, .34f));
            _cageRenderer.color = new Color(1f, 1f, 1f, .94f);
            _cageRenderer.enabled = !_released;
        }
    }
}
