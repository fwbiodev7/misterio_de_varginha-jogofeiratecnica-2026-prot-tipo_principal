using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Gerencia um SpriteRenderer filho que exibe a mochila equipada nas costas do Edelzio.
    /// Abordagem de filho de cena (igual ao notebook) em vez de composição pixel a pixel,
    /// garantindo qualidade visual perfeita em qualquer resolução.
    /// Direções: 0 = Sul (frente), 1 = Oeste, 2 = Leste, 3 = Norte (costas).
    /// </summary>
    public sealed class EdelzioBackpackAppearance : System.IDisposable
    {
        private SpriteRenderer _sr;
        // Sprite compartilhado entre todas as instâncias — gerado uma única vez.
        private static Sprite _sprite;

        // ── Paleta pixel art ────────────────────────────────────────────────────
        private static readonly Color32 Body    = new(88,  95, 105, 255); // corpo cinza grafite
        private static readonly Color32 Dark    = new(52,  57,  64, 255); // sombra / contorno
        private static readonly Color32 Mid     = new(68,  74,  83, 255); // meio-tom
        private static readonly Color32 Light   = new(128,136, 148, 255); // destaque topo
        private static readonly Color32 Strap   = new(42,  45,  50, 255); // alça quase preta
        private static readonly Color32 Buckle  = new(188,194, 202, 255); // fivela prateada
        private static readonly Color32 Pocket  = new(72,  78,  88, 255); // bolso frontal
        private static readonly Color32 Clear   = new(0, 0, 0, 0);

        // ── Textura 14 × 18 px (pivot na base central) ──────────────────────────
        private const int W = 14, H = 18;

        private static void Px(Color32[] p, int x, int y, Color32 c)
        {
            if (x < 0 || x >= W || y < 0 || y >= H) return;
            p[y * W + x] = c;
        }

        private static Sprite BuildSprite()
        {
            var p = new Color32[W * H];
            for (int i = 0; i < p.Length; i++) p[i] = Clear;

            // ── Corpo principal (cols 2-11, rows 2-15) ──
            for (int y = 2; y <= 15; y++)
            for (int x = 2; x <= 11; x++)
                Px(p, x, y, Body);

            // Aba superior arredondada (rows 16-17)
            for (int x = 3; x <= 10; x++) { Px(p, x, 16, Dark); Px(p, x, 17, Dark); }

            // Contorno escuro — lados e fundo
            for (int y = 2; y <= 15; y++) { Px(p, 2, y, Dark); Px(p, 11, y, Dark); }
            for (int x = 2; x <= 11; x++)  Px(p, x, 2, Dark);

            // Meio-tom direito (sombra lateral)
            for (int y = 3; y <= 14; y++) Px(p, 10, y, Mid);

            // Destaque topo-esquerdo
            for (int y = 13; y <= 15; y++) Px(p, 3, y, Light);
            for (int x = 3; x <=  8; x++) Px(p, x, 15, Light);

            // ── Bolso frontal (rows 4-9, cols 4-9) ──
            for (int x = 4; x <= 9; x++) { Px(p, x, 4, Dark); Px(p, x, 9, Dark); }
            for (int y = 4; y <= 9; y++) { Px(p, 4, y, Dark); Px(p, 9, y, Dark); }
            for (int y = 5; y <= 8; y++)
            for (int x = 5; x <= 8; x++) Px(p, x, y, Pocket);
            // zipper do bolso
            Px(p, 6, 6, Buckle); Px(p, 7, 6, Buckle);

            // ── Alças (cols 4 e 9, rows 0-2) ──
            for (int y = 0; y <= 2; y++) { Px(p, 4, y, Strap); Px(p, 9, y, Strap); }
            // fivela central
            Px(p, 6, 1, Buckle); Px(p, 7, 1, Buckle);

            // ── Costura horizontal do meio ──
            for (int x = 3; x <= 10; x++) Px(p, x, 11, Mid);

            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp,
                hideFlags  = HideFlags.DontSave,
                name       = "Backpack_Equipped_Tex"
            };
            tex.SetPixels32(p);
            tex.Apply(false, false);

            // Pivot na base central para facilitar o posicionamento
            var s = Sprite.Create(tex, new Rect(0, 0, W, H),
                new Vector2(.5f, 0f), 16f, 0, SpriteMeshType.FullRect);
            s.name       = "Backpack_Equipped";
            s.hideFlags  = HideFlags.DontSave;
            return s;
        }

        // ── API pública ─────────────────────────────────────────────────────────

        /// <summary>
        /// Atualiza visibilidade e pose da mochila filho com base na direção de Edelzio.
        /// Deve ser chamado em PresentPose / RefreshEquipmentAppearance.
        /// </summary>
        public void UpdatePose(Transform player, SpriteRenderer playerSr, int direction, bool visible)
        {
            EnsureChild(player, playerSr);
            if (!visible) { _sr.enabled = false; return; }
            if (_sprite == null) _sprite = BuildSprite();
            _sr.sprite = _sprite;
            _sr.enabled = true;
            _sr.sortingLayerID = playerSr.sortingLayerID;

            // ── Ajusta posição, escala e ordem por direção ──────────────────────
            //   direction 3 = Norte (costas para câmera) — mochila totalmente visível
            //   direction 0 = Sul  (frente para câmera) — só alças discretas
            //   direction 1 = Oeste / direction 2 = Leste — perfil lateral
            switch (direction)
            {
                case 3: // Costas — mochila exposta na frente do sprite
                    _sr.sortingOrder   = playerSr.sortingOrder + 1;
                    _sr.transform.localPosition = new Vector3(0f, -.08f, 0f);
                    _sr.transform.localScale    = new Vector3(.50f, .50f, 1f);
                    _sr.flipX = false;
                    break;

                case 1: // Facing west — pack visível atrás do ombro direito
                    _sr.sortingOrder   = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(.13f, -.10f, 0f);
                    _sr.transform.localScale    = new Vector3(.30f, .42f, 1f);
                    _sr.flipX = false;
                    break;

                case 2: // Facing east — pack visível atrás do ombro esquerdo
                    _sr.sortingOrder   = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(-.13f, -.10f, 0f);
                    _sr.transform.localScale    = new Vector3(.30f, .42f, 1f);
                    _sr.flipX = true;
                    break;

                default: // Sul (frente) — mochila quase oculta, só as alças
                    _sr.sortingOrder   = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(0f, -.06f, 0f);
                    _sr.transform.localScale    = new Vector3(.44f, .44f, 1f);
                    _sr.flipX = false;
                    break;
            }
        }

        // ── Internos ────────────────────────────────────────────────────────────

        private void EnsureChild(Transform player, SpriteRenderer playerSr)
        {
            if (_sr != null && _sr.gameObject != null) return;
            var found = player.Find("Mochila_Equipada");
            if (found == null)
            {
                found = new GameObject("Mochila_Equipada").transform;
                found.SetParent(player, false);
                found.gameObject.hideFlags = HideFlags.DontSave;
            }
            _sr = found.GetComponent<SpriteRenderer>()
                  ?? found.gameObject.AddComponent<SpriteRenderer>();
            _sr.hideFlags = HideFlags.DontSave;
        }

        public void Dispose()
        {
            if (_sr != null && _sr.gameObject != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(_sr.gameObject);
                else                       UnityEngine.Object.DestroyImmediate(_sr.gameObject);
            }
            _sr = null;
            // O sprite estático é compartilhado; deixa o GC coletar em Play.
            // Em modo Editor, o DontSave garante limpeza automática.
        }
    }
}
