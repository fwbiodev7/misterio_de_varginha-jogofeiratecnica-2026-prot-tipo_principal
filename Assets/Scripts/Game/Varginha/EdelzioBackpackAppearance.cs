using System.IO;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Gerencia a exibição da mochila nas costas do Edelzio utilizando o modelo de pixel art
    /// fornecido pelo usuário em todas as 8 posições (Frente, Costas, Lado Esq, Lado Dir, e diagonais).
    /// As texturas são carregadas de Resources/Varginha/Equipment/ com fallback para leitura de bytes em disco.
    /// A escala é ajustada para proporção 1:1 anatômica com o torso do Edelzio (altura de 18 pixels no corpo).
    /// </summary>
    public sealed class EdelzioBackpackAppearance : System.IDisposable
    {
        private SpriteRenderer _sr;

        // Cache estático dos 8 sprites extraídos do modelo de IA do usuário
        private static Sprite _spriteCostasNorte;    // Face externa (bolsos/zíper) vista nas costas do player
        private static Sprite _spriteFrenteSul;      // Face interna com alças
        private static Sprite _spriteLadoOeste;      // Perfil direito da mochila (visível atrás do player virado a oeste)
        private static Sprite _spriteLadoLeste;      // Perfil esquerdo da mochila (visível atrás do player virado a leste)
        private static Sprite _spriteDiagCostasEsq;  // Diagonal costas esquerda
        private static Sprite _spriteDiagCostasDir;  // Diagonal costas direita
        private static Sprite _spriteDiagFrenteEsq;  // Diagonal frente esquerda
        private static Sprite _spriteDiagFrenteDir;  // Diagonal frente direita

        public void UpdatePose(Transform player, SpriteRenderer playerSr, int direction, bool visible)
        {
            EnsureChild(player, playerSr);
            if (_sr == null) return;
            if (!visible)
            {
                _sr.enabled = false;
                return;
            }

            EnsureSpritesLoaded();

            _sr.sortingLayerID = playerSr.sortingLayerID;
            _sr.transform.localScale = Vector3.one;
            _sr.flipX = false;

            // Mapeamento das direções com base no sprite do Edelzio:
            // 3 = Norte (Costas do player para câmera — exibe a mochila com bolso e zíper no centro das costas)
            // 1 = Oeste (Player olhando para a esquerda — mochila projeta perfil para a direita)
            // 2 = Leste (Player olhando para a direita — mochila projeta perfil para a esquerda)
            // 0 = Sul (Player de frente para câmera — mochila oculta atrás do tronco)
            // 6 = Diagonal Norte-Oeste, 7 = Diagonal Norte-Leste, 4 = Sul-Oeste, 5 = Sul-Leste
            switch (direction)
            {
                case 3: // Norte (costas)
                    _sr.sprite = _spriteCostasNorte;
                    _sr.enabled = _spriteCostasNorte != null;
                    _sr.sortingOrder = playerSr.sortingOrder + 1;
                    _sr.transform.localPosition = new Vector3(0f, .05f, 0f);
                    break;

                case 1: // Oeste (olhando para a esquerda)
                    _sr.sprite = _spriteLadoOeste;
                    _sr.enabled = _spriteLadoOeste != null;
                    _sr.sortingOrder = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(.12f, .05f, 0f);
                    break;

                case 2: // Leste (olhando para a direita)
                    _sr.sprite = _spriteLadoLeste;
                    _sr.enabled = _spriteLadoLeste != null;
                    _sr.sortingOrder = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(-.12f, .05f, 0f);
                    break;

                case 6: // Diagonal Norte-Oeste
                    _sr.sprite = _spriteDiagCostasEsq ?? _spriteCostasNorte;
                    _sr.enabled = _sr.sprite != null;
                    _sr.sortingOrder = playerSr.sortingOrder + 1;
                    _sr.transform.localPosition = new Vector3(.06f, .05f, 0f);
                    break;

                case 7: // Diagonal Norte-Leste
                    _sr.sprite = _spriteDiagCostasDir ?? _spriteCostasNorte;
                    _sr.enabled = _sr.sprite != null;
                    _sr.sortingOrder = playerSr.sortingOrder + 1;
                    _sr.transform.localPosition = new Vector3(-.06f, .05f, 0f);
                    break;

                case 4: // Diagonal Sul-Oeste
                    _sr.sprite = _spriteDiagFrenteEsq;
                    _sr.enabled = _spriteDiagFrenteEsq != null;
                    _sr.sortingOrder = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(.08f, .04f, 0f);
                    break;

                case 5: // Diagonal Sul-Leste
                    _sr.sprite = _spriteDiagFrenteDir;
                    _sr.enabled = _spriteDiagFrenteDir != null;
                    _sr.sortingOrder = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(-.08f, .04f, 0f);
                    break;

                default: // 0 = Sul (frente para câmera — mochila oculta atrás do corpo)
                    _sr.enabled = false;
                    break;
            }
        }

        private static void EnsureSpritesLoaded()
        {
            if (_spriteCostasNorte != null) return;

            // O modelo fornecido pelo usuário rotula a face com zíper/bolsos de "Frente",
            // que quando usada nas costas de um personagem é a face externa visível olhando de trás (Norte).
            _spriteCostasNorte   = LoadEquipmentSprite("Backpack_Frente");
            _spriteFrenteSul     = LoadEquipmentSprite("Backpack_Costas");
            _spriteLadoOeste     = LoadEquipmentSprite("Backpack_LadoDir");
            _spriteLadoLeste     = LoadEquipmentSprite("Backpack_LadoEsq");
            _spriteDiagCostasEsq = LoadEquipmentSprite("Backpack_DiagCostasEsq");
            _spriteDiagCostasDir = LoadEquipmentSprite("Backpack_DiagCostasDir");
            _spriteDiagFrenteEsq = LoadEquipmentSprite("Backpack_DiagFrenteEsq");
            _spriteDiagFrenteDir = LoadEquipmentSprite("Backpack_DiagFrenteDir");
        }

        private static Sprite LoadEquipmentSprite(string name)
        {
            try
            {
                // 1. Tenta ler direto do disco via ImageConversion: garante Texture2D legível (readable) em runtime
                string path = Path.Combine(Application.dataPath, "Resources", "Varginha", "Equipment", name + ".png");
                if (File.Exists(path))
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    if (bytes != null && bytes.Length > 0)
                    {
                        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                        {
                            filterMode = FilterMode.Point,
                            wrapMode = TextureWrapMode.Clamp,
                            name = name
                        };
                        if (ImageConversion.LoadImage(tex, bytes))
                        {
                            tex.filterMode = FilterMode.Point;
                            float targetPixelHeight = 18f;
                            float ppu = (tex.height / targetPixelHeight) * VarginhaReferenceSprites.EdelzioPixelsPerUnit;
                            var s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
                            s.name = name;
                            s.hideFlags = HideFlags.DontSave;
                            return s;
                        }
                    }
                }

                // 2. Fallback para Resources caso executando em build standalone
                var directSprites = Resources.LoadAll<Sprite>("Varginha/Equipment/" + name);
                if (directSprites != null && directSprites.Length > 0 && directSprites[0] != null)
                {
                    return directSprites[0];
                }

                var directSprite = Resources.Load<Sprite>("Varginha/Equipment/" + name);
                if (directSprite != null) return directSprite;

                var resTex = Resources.Load<Texture2D>("Varginha/Equipment/" + name);
                if (resTex != null)
                {
                    float targetPixelHeight = 18f;
                    float ppu = (resTex.height / targetPixelHeight) * VarginhaReferenceSprites.EdelzioPixelsPerUnit;
                    var s = Sprite.Create(resTex, new Rect(0, 0, resTex.width, resTex.height),
                        new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
                    s.name = name;
                    s.hideFlags = HideFlags.DontSave;
                    return s;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EdelzioBackpackAppearance] Não foi possível carregar o sprite {name}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Compatibilidade retroativa com código de teste.
        /// </summary>
        public Sprite GetFrame(Sprite body, int direction) => body;

        private void EnsureChild(Transform player, SpriteRenderer playerSr)
        {
            if (_sr != null) return;
            if (player == null) return;
            var found = player.Find("Mochila_Equipada");
            if (found == null)
            {
                var go = new GameObject("Mochila_Equipada");
                go.transform.SetParent(player, false);
                go.hideFlags = HideFlags.DontSave;
                _sr = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                var sr = found.GetComponent<SpriteRenderer>();
                _sr = sr != null ? sr : found.gameObject.AddComponent<SpriteRenderer>();
            }
            if (_sr != null) _sr.hideFlags = HideFlags.DontSave;
        }

        public void Dispose()
        {
            if (_sr != null)
            {
                var go = _sr.gameObject;
                if (go != null)
                {
                    if (Application.isPlaying) Object.Destroy(go);
                    else Object.DestroyImmediate(go);
                }
            }
            _sr = null;
        }
    }
}
