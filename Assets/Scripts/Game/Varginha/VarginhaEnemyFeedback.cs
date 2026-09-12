using System.Collections.Generic;
using Game.Player;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Barra de vida e números de dano pixelados sobre cada ET combatível.</summary>
    [RequireComponent(typeof(HealthSystem))]
    public sealed class VarginhaEnemyFeedback : MonoBehaviour
    {
        private sealed class DamagePopup
        {
            public float Amount;
            public float Age;
            public Vector3 Offset;
        }

        private HealthSystem _health;
        private SpriteRenderer _renderer;
        private readonly List<DamagePopup> _popups = new();
        private GUIStyle _damageStyle;
        private GUIStyle _barStyle;

        private void Awake()
        {
            _health = GetComponent<HealthSystem>();
            _renderer = GetComponent<SpriteRenderer>();
            _health.OnDamageReceived += ShowDamage;
            _health.OnDeath += ClearFeedback;
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.OnDamageReceived -= ShowDamage;
            _health.OnDeath -= ClearFeedback;
        }

        private void Update()
        {
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                _popups[i].Age += Time.unscaledDeltaTime;
                if (_popups[i].Age >= .82f) _popups.RemoveAt(i);
            }
        }

        private void ShowDamage(float amount)
        {
            if (amount <= 0f || _health.IsDead) return;
            _popups.Add(new DamagePopup
            {
                Amount = amount,
                Age = 0f,
                Offset = new Vector3(Random.Range(-.12f, .12f), Random.Range(-.02f, .10f), 0f)
            });
        }

        private void ClearFeedback()
        {
            _popups.Clear();
        }

        private void OnGUI()
        {
            if (_health == null || _health.IsDead || VarginhaTravelCinematic.IsTravelling) return;
            Camera camera = Camera.main;
            if (camera == null) return;

            float height = _renderer != null ? _renderer.bounds.size.y : .8f;
            Vector3 world = transform.position + Vector3.up * (height * .55f + .24f);
            Vector3 point = camera.WorldToScreenPoint(world);
            if (point.z <= 0f) return;
            point.y = Screen.height - point.y;

            DrawHealthBar(point);
            DrawDamagePopups(camera);
        }

        private void DrawHealthBar(Vector3 point)
        {
            const float width = 58f;
            const float height = 7f;
            Rect outer = new Rect(Mathf.Floor(point.x - width * .5f), Mathf.Floor(point.y), width, height);
            GUI.color = new Color(.025f, .035f, .055f, .95f);
            GUI.DrawTexture(outer, Texture2D.whiteTexture);
            float percent = Mathf.Clamp01(_health.HealthPercent);
            Color fill = Color.Lerp(new Color(.92f, .12f, .10f), new Color(.28f, .92f, .38f), percent);
            GUI.color = fill;
            GUI.DrawTexture(new Rect(outer.x + 1f, outer.y + 1f, (width - 2f) * percent, height - 2f), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawDamagePopups(Camera camera)
        {
            if (_popups.Count == 0) return;
            if (_damageStyle == null)
            {
                _damageStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    wordWrap = false
                };
                _damageStyle.normal.textColor = new Color(1f, .88f, .28f);
            }

            float height = _renderer != null ? _renderer.bounds.size.y : .8f;
            for (int i = 0; i < _popups.Count; i++)
            {
                DamagePopup popup = _popups[i];
                float t = Mathf.Clamp01(popup.Age / .82f);
                Vector3 world = transform.position + Vector3.up * (height * .55f + .42f + t * .72f) + popup.Offset;
                Vector3 point = camera.WorldToScreenPoint(world);
                if (point.z <= 0f) continue;
                point.y = Screen.height - point.y;
                Color previous = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 1f - t);
                Rect rect = new Rect(point.x - 42f, point.y - 16f, 84f, 28f);
                GUI.Label(rect, "-" + Mathf.CeilToInt(popup.Amount), _damageStyle);
                GUI.color = previous;
            }
        }
    }
}
