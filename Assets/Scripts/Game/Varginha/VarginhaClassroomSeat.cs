using UnityEngine;

namespace Game.Varginha
{
    /// <summary>One occupant per chair; students stand into the aisle before rescue movement.</summary>
    [DisallowMultipleComponent]
    public sealed class VarginhaClassroomSeat : MonoBehaviour
    {
        private Component _occupant;
        private Collider2D _studentCollider;
        private bool _wasIgnored;
        private SpriteRenderer _backrest;
        public bool IsOccupied => _occupant != null;
        public Vector3 SeatedPosition => transform.position + Vector3.up * .25f;
        public Vector3 ExitPosition => transform.position + Vector3.down * .75f;
        public Collider2D DeskCollider => transform.parent?.Find(name.Replace("Cadeira_", "Carteira_"))?.GetComponent<Collider2D>();

        public bool Reserve(Component actor)
        {
            if (actor == null || (IsOccupied && _occupant != actor)) return false;
            _occupant = actor;
            return true;
        }

        public bool SitStudent(VarginhaStudentHostage student)
        {
            if (student == null || student.IsReleased || !Reserve(student)) return false;
            _studentCollider = student.GetComponent<Collider2D>();
            var chair = GetComponent<Collider2D>();
            if (_studentCollider != null && chair != null)
            {
                _wasIgnored = Physics2D.GetIgnoreCollision(_studentCollider, chair);
                Physics2D.IgnoreCollision(_studentCollider, chair, true);
            }
            var position = transform.position + Vector3.up * .05f;
            student.transform.position = position;
            var body = student.GetComponent<Rigidbody2D>();
            if (body != null) { body.position = position; body.linearVelocity = Vector2.zero; }
            student.GetComponent<VarginhaStudentAnimation>()?.SetSeated(true);
            ShowOccupiedBackrest(student.GetComponent<SpriteRenderer>());
            return true;
        }

        public void ShowOccupiedBackrest(SpriteRenderer actor)
        {
            var chair = GetComponent<SpriteRenderer>();
            if (chair == null || chair.sprite == null || actor == null) return;
            if (_backrest == null)
            {
                var go = new GameObject("Encosto_Frente_Aluno");
                go.transform.SetParent(transform, false);
                _backrest = go.AddComponent<SpriteRenderer>();
            }
            // Only the lower, near side of the rear-view chair covers the seated legs.
            var source = chair.sprite;
            var rect = source.rect;
            rect.height *= .66f;
            if (_backrest.sprite == null || _backrest.sprite.texture != source.texture)
                _backrest.sprite = Sprite.Create(source.texture, rect,
                    new Vector2(source.pivot.x / source.rect.width, source.pivot.y / rect.height),
                    source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            _backrest.sortingLayerID = actor.sortingLayerID;
            _backrest.sortingOrder = actor.sortingOrder + 1;
            _backrest.enabled = true;
        }

        public void Vacate(Component actor)
        {
            if (_occupant != actor) return;
            var student = actor as VarginhaStudentHostage;
            if (student != null)
            {
                student.GetComponent<VarginhaStudentAnimation>()?.SetSeated(false);
                var body = student.GetComponent<Rigidbody2D>();
                if (body != null) { body.position = ExitPosition; body.linearVelocity = Vector2.zero; }
                student.transform.position = ExitPosition;
            }
            var chair = GetComponent<Collider2D>();
            if (_studentCollider != null && chair != null) Physics2D.IgnoreCollision(_studentCollider, chair, _wasIgnored);
            _studentCollider = null;
            _occupant = null;
            if (_backrest != null) _backrest.enabled = false;
        }

        private void OnDisable() { if (_occupant != null) Vacate(_occupant); }
        private void OnDestroy()
        {
            if (_backrest == null || _backrest.sprite == null) return;
            if (Application.isPlaying) Destroy(_backrest.sprite); else DestroyImmediate(_backrest.sprite);
        }
    }
}
