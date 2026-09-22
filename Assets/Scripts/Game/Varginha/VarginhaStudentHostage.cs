using System;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Aluno mantido como refém até que os subordinados ETs sejam derrotados.</summary>
    public sealed class VarginhaStudentHostage : MonoBehaviour
    {
        [SerializeField] private string studentName = "Aluno";
        [SerializeField] private float followSpeed = 2.6f;

        private Transform _fusca;
        private Transform _leader;
        private Vector3 _carOffset;
        private Vector3 _followOffset;
        private SpriteRenderer _renderer;
        private SpriteRenderer _cageRenderer;
        private bool _released;
        private bool _arrived;

        public string StudentName => studentName;
        public bool IsReleased => _released;
        public bool IsCaged => !_released;
        public bool IsAtFusca => _arrived;

        /// <summary>
        /// Reaplica a apresentação visual quando uma cena antiga foi salva sem a jaula
        /// ou com o SpriteRenderer desativado. Não altera o estado de resgate.
        /// </summary>
        public void EnsurePresentation(int sortingOrder = 8)
        {
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
            _released = true;
            _fusca = fusca;
            _leader = leader;
            // A fila do Fusca usa um espaço compacto; durante o acompanhamento eles
            // mantêm uma formação 3x3 atrás do Edelzio, evitando que um sprite cubra os outros.
            int column = index % 3;
            int row = index / 3;
            _carOffset = new Vector3(-1.35f - column * .70f, (row - 1) * .82f, 0f);
            _followOffset = new Vector3(-1.75f - column * 1.05f, (1 - row) * 1.12f, 0f);
            if (_renderer != null) _renderer.sortingOrder = 8 + Mathf.Clamp(index, 0, 8);
            if (_renderer != null) _renderer.color = Color.white;
            if (_cageRenderer != null) _cageRenderer.enabled = false;
        }

        private void Update()
        {
            if (!_released && _cageRenderer != null)
            {
                // O brilho pulsa para comunicar que os alunos ainda estão presos.
                float pulse = .82f + Mathf.Sin(Time.unscaledTime * 3.5f) * .12f;
                _cageRenderer.color = new Color(1f, 1f, 1f, pulse);
            }
            if (!_released || _fusca == null || _arrived) return;
            bool headingToCar = _leader == null || Vector2.Distance(_leader.position, _fusca.position) <= 3.4f;
            Vector3 destination = _fusca.position + _carOffset;
            // Enquanto Edelzio ainda está na escola, a turma o acompanha; quando ele volta ao Fusca,
            // cada aluno entra na fila do carro para concluir a fase.
            if (!headingToCar)
                destination = _leader.position + _followOffset;

            Vector3 prev = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, destination, followSpeed * Time.deltaTime);

            transform.localScale = Vector3.one;

            if (headingToCar && Vector2.Distance(transform.position, destination) < .06f)
            {
                transform.position = destination;
                _arrived = true;
                VarginhaPhase2Controller.NotifyStudentAtFusca(this);
            }
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
            _cageRenderer.color = Color.white;
            _cageRenderer.enabled = !_released;
        }
    }
}
