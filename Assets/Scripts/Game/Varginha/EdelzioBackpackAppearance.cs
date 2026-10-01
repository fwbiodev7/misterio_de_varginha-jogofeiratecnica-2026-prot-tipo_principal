using System.IO;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Gerencia a exibição da mochila nas costas do Edelzio utilizando o modelo de pixel art
    /// fornecido pelo usuário em todas as 8 posições (Frente, Costas, Lado Esq, Lado Dir, e diagonais).
    /// Utiliza duas camadas complementares:
    /// - Camada base (_sr em "Mochila_Equipada"): desenha o corpo da mochila (nas costas, sortingOrder - 1 em perfil).
    /// - Camada de alça (_srStrap em "Mochila_Alca"): desenha as alças contornando os ombros (sortingOrder + 1).
    /// </summary>
    public sealed class EdelzioBackpackAppearance : System.IDisposable
    {
        private SpriteRenderer _sr;
        private SpriteRenderer _srStrap;

        // Sprites da mochila nas costas e alças nos ombros
        private static Sprite _spriteCostasNorte;    // Face externa (bolsos/zíper) vista nas costas do player
        private static Sprite _spriteFrenteSulStrap; // Alças frontais descendo pelos ombros/peito
        private static Sprite _spriteLadoOestePack;  // Corpo da mochila nas costas (perfil oeste)
        private static Sprite _spriteLadoOesteStrap; // Alça contornando o ombro esquerdo (perfil oeste)
        private static Sprite _spriteLadoLestePack;  // Corpo da mochila nas costas (perfil leste)
        private static Sprite _spriteLadoLesteStrap; // Alça contornando o ombro direito (perfil leste)
        private static Sprite _spriteDiagCostasEsq;  // Diagonal costas esquerda
        private static Sprite _spriteDiagCostasDir;  // Diagonal costas direita
        private static Sprite _spriteDiagFrenteEsq;  // Diagonal frente esquerda
        private static Sprite _spriteDiagFrenteDir;  // Diagonal frente direita

        public void UpdatePose(Transform player, SpriteRenderer playerSr, int direction, bool visible)
        {
            EnsureChild(player, playerSr);
            if (_sr == null || _srStrap == null) return;

            if (!visible)
            {
                _sr.enabled = false;
                _srStrap.enabled = false;
                return;
            }

            EnsureSpritesLoaded();

            _sr.sortingLayerID = playerSr.sortingLayerID;
            _sr.transform.localScale = Vector3.one;
            _sr.flipX = false;

            _srStrap.sortingLayerID = playerSr.sortingLayerID;
            _srStrap.transform.localScale = Vector3.one;
            _srStrap.flipX = false;

            // Mapeamento das 8 direções:
            // 3 = Norte (Costas do player para câmera — exibe a mochila completa nas costas)
            // 1 = Oeste (Player virado à esquerda — mochila colada nas costas + alça contornando o ombro)
            // 2 = Leste (Player virado à direita — mochila colada nas costas + alça contornando o ombro)
            // 0 = Sul (Player de frente para câmera — alças frontais sobre o peito)
            // 6 = Diagonal Norte-Oeste, 7 = Diagonal Norte-Leste, 4 = Sul-Oeste, 5 = Sul-Leste
            switch (direction)
            {
                case 3: // Norte (costas — usuário aprovou essa posição)
                    _sr.sprite = _spriteCostasNorte;
                    _sr.enabled = _spriteCostasNorte != null;
                    _sr.sortingOrder = playerSr.sortingOrder + 1;
                    _sr.transform.localPosition = new Vector3(0f, .05f, 0f);

                    _srStrap.enabled = false;
                    break;

                case 1: // Oeste (olhando para a esquerda)
                    // Corpo da mochila colado nas costas (à direita do torso)
                    _sr.sprite = _spriteLadoOestePack;
                    _sr.enabled = _spriteLadoOestePack != null;
                    _sr.sortingOrder = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(.035f, -.015f, 0f);

                    // Alça contornando o ombro em primeiro plano (à frente da camisa)
                    _srStrap.sprite = _spriteLadoOesteStrap;
                    _srStrap.enabled = _spriteLadoOesteStrap != null;
                    _srStrap.sortingOrder = playerSr.sortingOrder + 1;
                    _srStrap.transform.localPosition = new Vector3(.035f, -.015f, 0f);
                    break;

                case 2: // Leste (olhando para a direita)
                    // Corpo da mochila colado nas costas (à esquerda do torso)
                    _sr.sprite = _spriteLadoLestePack;
                    _sr.enabled = _spriteLadoLestePack != null;
                    _sr.sortingOrder = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(-.035f, -.015f, 0f);

                    // Alça contornando o ombro em primeiro plano (à frente da camisa)
                    _srStrap.sprite = _spriteLadoLesteStrap;
                    _srStrap.enabled = _spriteLadoLesteStrap != null;
                    _srStrap.sortingOrder = playerSr.sortingOrder + 1;
                    _srStrap.transform.localPosition = new Vector3(-.035f, -.015f, 0f);
                    break;

                case 6: // Diagonal Norte-Oeste
                    _sr.sprite = _spriteDiagCostasEsq ?? _spriteCostasNorte;
                    _sr.enabled = _sr.sprite != null;
                    _sr.sortingOrder = playerSr.sortingOrder + 1;
                    _sr.transform.localPosition = new Vector3(.04f, .03f, 0f);

                    _srStrap.enabled = false;
                    break;

                case 7: // Diagonal Norte-Leste
                    _sr.sprite = _spriteDiagCostasDir ?? _spriteCostasNorte;
                    _sr.enabled = _sr.sprite != null;
                    _sr.sortingOrder = playerSr.sortingOrder + 1;
                    _sr.transform.localPosition = new Vector3(-.04f, .03f, 0f);

                    _srStrap.enabled = false;
                    break;

                case 4: // Diagonal Sul-Oeste
                    _sr.sprite = _spriteDiagFrenteEsq;
                    _sr.enabled = _spriteDiagFrenteEsq != null;
                    _sr.sortingOrder = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(.035f, -.01f, 0f);

                    _srStrap.sprite = _spriteLadoOesteStrap;
                    _srStrap.enabled = _spriteLadoOesteStrap != null;
                    _srStrap.sortingOrder = playerSr.sortingOrder + 1;
                    _srStrap.transform.localPosition = new Vector3(.035f, -.01f, 0f);
                    break;

                case 5: // Diagonal Sul-Leste
                    _sr.sprite = _spriteDiagFrenteDir;
                    _sr.enabled = _spriteDiagFrenteDir != null;
                    _sr.sortingOrder = playerSr.sortingOrder - 1;
                    _sr.transform.localPosition = new Vector3(-.035f, -.01f, 0f);

                    _srStrap.sprite = _spriteLadoLesteStrap;
                    _srStrap.enabled = _spriteLadoLesteStrap != null;
                    _srStrap.sortingOrder = playerSr.sortingOrder + 1;
                    _srStrap.transform.localPosition = new Vector3(-.035f, -.01f, 0f);
                    break;

                default: // 0 = Sul (frente para câmera — alças nos ombros visíveis na frente da camisa)
                    _sr.enabled = false;

                    _srStrap.sprite = _spriteFrenteSulStrap;
                    _srStrap.enabled = _spriteFrenteSulStrap != null;
                    _srStrap.sortingOrder = playerSr.sortingOrder + 1;
                    _srStrap.transform.localPosition = new Vector3(0f, -.01f, 0f);
                    break;
            }
        }

        private static void EnsureSpritesLoaded()
        {
            if (_spriteCostasNorte != null) return;

            _spriteCostasNorte   = LoadEquipmentSprite("Backpack_Frente");
            _spriteDiagCostasEsq = LoadEquipmentSprite("Backpack_DiagCostasEsq");
            _spriteDiagCostasDir = LoadEquipmentSprite("Backpack_DiagCostasDir");
            _spriteDiagFrenteEsq = LoadEquipmentSprite("Backpack_DiagFrenteEsq");
            _spriteDiagFrenteDir = LoadEquipmentSprite("Backpack_DiagFrenteDir");

            // Divide as imagens de perfil em corpo da mochila (Pack) e alça do ombro (Strap)
            LoadSplitSideSprites("Backpack_LadoDir", splitX: 17, isLeftStrap: true,
                out _spriteLadoOestePack, out _spriteLadoOesteStrap);

            LoadSplitSideSprites("Backpack_LadoEsq", splitX: 32, isLeftStrap: false,
                out _spriteLadoLestePack, out _spriteLadoLesteStrap);

            // Alças frontais para visão frontal (Sul)
            _spriteFrenteSulStrap = LoadFrontStraps("Backpack_Costas");
        }

        private static void LoadSplitSideSprites(string name, int splitX, bool isLeftStrap,
            out Sprite packSprite, out Sprite strapSprite)
        {
            packSprite = null;
            strapSprite = null;

            try
            {
                string path = Path.Combine(Application.dataPath, "Resources", "Varginha", "Equipment", name + ".png");
                if (!File.Exists(path)) return;

                byte[] bytes = File.ReadAllBytes(path);
                if (bytes == null || bytes.Length == 0) return;

                var baseTex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                if (!ImageConversion.LoadImage(baseTex, bytes)) return;

                int w = baseTex.width;
                int h = baseTex.height;
                var raw = baseTex.GetPixels32();

                var packPixels = new Color32[w * h];
                var strapPixels = new Color32[w * h];

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int idx = y * w + x;
                        Color32 px = raw[idx];
                        bool isStrap = isLeftStrap ? (x < splitX) : (x >= splitX);

                        if (isStrap)
                        {
                            strapPixels[idx] = px;
                            packPixels[idx] = new Color32(0, 0, 0, 0);
                        }
                        else
                        {
                            packPixels[idx] = px;
                            strapPixels[idx] = new Color32(0, 0, 0, 0);
                        }
                    }
                }

                float targetPixelHeight = 18f;
                float ppu = (h / targetPixelHeight) * VarginhaReferenceSprites.EdelzioPixelsPerUnit;

                var packTex = new Texture2D(w, h, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    name = name + "_Pack"
                };
                packTex.SetPixels32(packPixels);
                packTex.Apply(false, false);
                packSprite = Sprite.Create(packTex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
                packSprite.name = name + "_Pack";
                packSprite.hideFlags = HideFlags.DontSave;

                var strapTex = new Texture2D(w, h, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    name = name + "_Strap"
                };
                strapTex.SetPixels32(strapPixels);
                strapTex.Apply(false, false);
                strapSprite = Sprite.Create(strapTex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
                strapSprite.name = name + "_Strap";
                strapSprite.hideFlags = HideFlags.DontSave;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EdelzioBackpackAppearance] Erro ao separar alça/pack de {name}: {ex.Message}");
            }
        }

        private static Sprite LoadFrontStraps(string name)
        {
            try
            {
                string path = Path.Combine(Application.dataPath, "Resources", "Varginha", "Equipment", name + ".png");
                if (!File.Exists(path)) return null;

                byte[] bytes = File.ReadAllBytes(path);
                if (bytes == null || bytes.Length == 0) return null;

                var baseTex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                if (!ImageConversion.LoadImage(baseTex, bytes)) return null;

                int w = baseTex.width;
                int h = baseTex.height;
                var raw = baseTex.GetPixels32();
                var strapPixels = new Color32[w * h];

                // Isola as duas alças laterais que passam sobre os ombros
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int idx = y * w + x;
                        Color32 px = raw[idx];
                        // Alças nas laterais externas (x < 18 e x > w - 19)
                        bool isStrap = (x < 18 || x >= w - 18) && px.a > 30;
                        strapPixels[idx] = isStrap ? px : new Color32(0, 0, 0, 0);
                    }
                }

                float targetPixelHeight = 18f;
                float ppu = (h / targetPixelHeight) * VarginhaReferenceSprites.EdelzioPixelsPerUnit;

                var strapTex = new Texture2D(w, h, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    name = name + "_FrontStraps"
                };
                strapTex.SetPixels32(strapPixels);
                strapTex.Apply(false, false);
                var s = Sprite.Create(strapTex, new Rect(0, 0, w, h), new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
                s.name = name + "_FrontStraps";
                s.hideFlags = HideFlags.DontSave;
                return s;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EdelzioBackpackAppearance] Erro ao extrair alças frontais: {ex.Message}");
                return null;
            }
        }

        private static Sprite LoadEquipmentSprite(string name)
        {
            try
            {
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

        public Sprite GetFrame(Sprite body, int direction) => body;

        private void EnsureChild(Transform player, SpriteRenderer playerSr)
        {
            if (player == null) return;

            if (_sr == null)
            {
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

            if (_srStrap == null)
            {
                var foundStrap = player.Find("Mochila_Alca");
                if (foundStrap == null)
                {
                    var go = new GameObject("Mochila_Alca");
                    go.transform.SetParent(player, false);
                    go.hideFlags = HideFlags.DontSave;
                    _srStrap = go.AddComponent<SpriteRenderer>();
                }
                else
                {
                    var sr = foundStrap.GetComponent<SpriteRenderer>();
                    _srStrap = sr != null ? sr : foundStrap.gameObject.AddComponent<SpriteRenderer>();
                }
                if (_srStrap != null) _srStrap.hideFlags = HideFlags.DontSave;
            }
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
                _sr = null;
            }

            if (_srStrap != null)
            {
                var go = _srStrap.gameObject;
                if (go != null)
                {
                    if (Application.isPlaying) Object.Destroy(go);
                    else Object.DestroyImmediate(go);
                }
                _srStrap = null;
            }
        }
    }
}
