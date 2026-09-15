using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Varginha.Prologue
{
    // Um Collider2D trigger por objeto; evite areas de interacao sobrepostas.
    [RequireComponent(typeof(Collider2D))]
    public sealed class PrologueInteractable : MonoBehaviour
    {
        [SerializeField] private TMP_Text subtitle;
        [SerializeField, TextArea] private string message;
        [SerializeField] private bool singleUse = true;
        [SerializeField] private UnityEvent onInteracted = new UnityEvent();
        private PrologueChildController _nearby;
        private bool _used;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var child = other.GetComponentInParent<PrologueChildController>();
            if (child != null) _nearby = child;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PrologueChildController>() == _nearby)
                _nearby = null;
        }

        private void Update()
        {
            if (_nearby == null || !_nearby.CanInteract || (singleUse && _used)) return;
            if (!VarginhaInputBindings.WasPressedThisFrame(VarginhaInputAction.Interact)) return;
            _used = true;
            if (subtitle != null) subtitle.text = message;
            onInteracted.Invoke();
        }

        private void OnDisable() => _nearby = null;
    }
}
