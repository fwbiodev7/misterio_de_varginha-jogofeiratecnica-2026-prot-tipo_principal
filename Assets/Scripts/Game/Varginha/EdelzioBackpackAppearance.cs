using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>
    /// Gera visual de mochila de trilha cinza integrada aos sprites do Edelzio.
    /// Respeita a silhueta do personagem em todas as 4 direções, sem vazamentos ou artefatos.
    /// </summary>
    public sealed class EdelzioBackpackAppearance : IDisposable
    {
        private readonly Dictionary<(Sprite, int), Sprite> _frames = new();

        // Paleta oficial de mochila de trilha cinza
        private static readonly Color32 Outline = new(28, 32, 38, 255);
        private static readonly Color32 Fabric = new(115, 122, 130, 255);
        private static readonly Color32 FabricLight = new(135, 142, 150, 255);
        private static readonly Color32 Shadow = new(72, 78, 86, 255);
        private static readonly Color32 Highlight = new(165, 172, 180, 255);
        private static readonly Color32 Roll = new(140, 148, 155, 255);
        private static readonly Color32 RollHi = new(185, 192, 198, 255);
        private static readonly Color32 RollSh = new(58, 64, 72, 255);
        private static readonly Color32 Buckle = new(215, 222, 228, 255);
        private static readonly Color32 Strap = new(50, 55, 62, 255);

        public Sprite GetFrame(Sprite body, int direction)
        {
            if (body == null || body.texture == null || !body.texture.isReadable) return body;
            var key = (body, direction);
            if (_frames.TryGetValue(key, out var cached) && cached != null) return cached;

            Rect rect = body.rect;
            int width = Mathf.RoundToInt(rect.width), height = Mathf.RoundToInt(rect.height);
            Color[] source = body.texture.GetPixels((int)rect.x, (int)rect.y, width, height);
            var original = new Color32[source.Length];
            for (int i = 0; i < source.Length; i++) original[i] = source[i];
            var pixels = (Color32[])original.Clone();

            float scaleX = width / 64f;
            float scaleY = height / 64f;

            int ToX(int x) => Mathf.Clamp(Mathf.RoundToInt(x * scaleX), 0, width - 1);
            int ToUnityY(int py) => Mathf.Clamp(Mathf.RoundToInt((63 - py) * scaleY), 0, height - 1);

            void SetPixel(int x, int py, Color32 color)
            {
                int ux = ToX(x);
                int uy = ToUnityY(py);
                pixels[uy * width + ux] = color;
            }

            // Direção 0: Olhando para frente / baixo.
            // A mochila está atrás das costas; desenha-se apenas as alças frontais e a fivela peitoral sobre a camisa.
            if (direction == 0)
            {
                // Sutis pontas do rolo superior atrás dos ombros
                int[] rollXs = { 25, 26, 37, 38 };
                for (int i = 0; i < rollXs.Length; i++)
                {
                    int ux = ToX(rollXs[i]);
                    int uy = ToUnityY(30);
                    if (original[uy * width + ux].a == 0)
                        pixels[uy * width + ux] = RollSh;
                }

                for (int py = 32; py <= 42; py++)
                {
                    // Alça esquerda (peito do Edelzio)
                    for (int x = 26; x <= 27; x++)
                    {
                        int ux = ToX(x);
                        int uy = ToUnityY(py);
                        if (IsShirt(original[uy * width + ux]))
                            pixels[uy * width + ux] = x == 26 ? Strap : (py == 33 || py == 37) ? Highlight : Fabric;
                    }

                    // Alça direita
                    for (int x = 36; x <= 37; x++)
                    {
                        int ux = ToX(x);
                        int uy = ToUnityY(py);
                        if (IsShirt(original[uy * width + ux]))
                            pixels[uy * width + ux] = x == 37 ? Strap : (py == 33 || py == 37) ? Highlight : Fabric;
                    }

                    // Fivela / tira peitoral na altura py = 36
                    if (py == 36)
                    {
                        for (int x = 28; x <= 35; x++)
                        {
                            int ux = ToX(x);
                            int uy = ToUnityY(py);
                            if (IsShirt(original[uy * width + ux]))
                                pixels[uy * width + ux] = (x == 31 || x == 32) ? Buckle : Strap;
                        }
                    }
                }
            }
            // Direção 3: De costas / subindo.
            // Mochila de trilha cinza centralizada nas costas, com rolo de dormir, fivelas e bolsos.
            else if (direction == 3)
            {
                for (int py = 28; py <= 42; py++)
                for (int px = 25; px <= 39; px++)
                {
                    // Cantos arredondados
                    if ((px == 25 || px == 39) && (py == 28 || py == 42 || py == 29))
                        continue;

                    bool isEdge = (px == 25 && py > 29 && py < 42) || (px == 39 && py > 29 && py < 42) ||
                                  (py == 28 && px > 25 && px < 39) || (py == 42 && px > 25 && px < 39) ||
                                  (px == 26 && (py == 29 || py == 42)) || (px == 38 && (py == 29 || py == 42));

                    if (isEdge)
                    {
                        SetPixel(px, py, Outline);
                    }
                    else if (py >= 29 && py <= 30)
                    {
                        // Rolo superior / isolante térmico
                        if (px == 28 || px == 36) SetPixel(px, py, RollSh);
                        else if (py == 29) SetPixel(px, py, RollHi);
                        else SetPixel(px, py, Roll);
                    }
                    else if (py == 31)
                    {
                        SetPixel(px, py, Outline);
                    }
                    else if (py >= 32 && py <= 34)
                    {
                        // Aba superior
                        if (px == 28 || px == 36) SetPixel(px, py, Strap);
                        else if (px > 34) SetPixel(px, py, Shadow);
                        else if (py == 32) SetPixel(px, py, Highlight);
                        else SetPixel(px, py, px < 30 ? FabricLight : Fabric);
                    }
                    else if (py == 35)
                    {
                        SetPixel(px, py, (px == 28 || px == 36) ? Buckle : Outline);
                    }
                    else
                    {
                        // Corpo principal e bolso inferior
                        if (px == 28 || px == 36) SetPixel(px, py, Strap);
                        else if ((px >= 31 && px <= 33) && (py == 38 || py == 39)) SetPixel(px, py, py == 38 ? Buckle : Shadow);
                        else if (px > 35 || py == 41) SetPixel(px, py, Shadow);
                        else SetPixel(px, py, Fabric);
                    }
                }
            }
            // Direção 1: Andando para a Esquerda.
            // Costas estão à direita do torso (x=38..47, py=29..42).
            else if (direction == 1)
            {
                for (int py = 29; py <= 42; py++)
                for (int px = 38; px <= 47; px++)
                {
                    if ((px == 47 && (py == 29 || py == 42 || py == 30))) continue;
                    bool isEdge = (px == 47 && py > 30 && py < 42) || (py == 29 && px >= 39 && px < 47) ||
                                  (py == 42 && px >= 39 && px < 47) || (px == 46 && py == 30);

                    if (isEdge) SetPixel(px, py, Outline);
                    else if (py >= 30 && py <= 31)
                    {
                        if (px == 44 || px == 45) SetPixel(px, py, RollSh);
                        else if (py == 30) SetPixel(px, py, RollHi);
                        else SetPixel(px, py, Roll);
                    }
                    else if (py == 32) SetPixel(px, py, Outline);
                    else if (py >= 33 && py <= 35)
                    {
                        if (px == 43) SetPixel(px, py, Strap);
                        else if (px >= 44) SetPixel(px, py, Shadow);
                        else if (py == 33) SetPixel(px, py, Highlight);
                        else SetPixel(px, py, Fabric);
                    }
                    else if (py == 36) SetPixel(px, py, px == 43 ? Buckle : Outline);
                    else
                    {
                        if (px == 43) SetPixel(px, py, Strap);
                        else if (px >= 45 || py >= 41) SetPixel(px, py, Shadow);
                        else SetPixel(px, py, Fabric);
                    }
                }

                // Braço e frente do Edelzio permanecem sempre em primeiro plano
                for (int uy = 0; uy < height; uy++)
                for (int ux = 0; ux < width; ux++)
                {
                    Color32 orig = original[uy * width + ux];
                    if (orig.a > 0 && (IsSkin(orig) || ux <= ToX(38)))
                        pixels[uy * width + ux] = orig;
                }
            }
            // Direção 2: Andando para a Direita.
            // Costas estão à esquerda do torso (x=17..26, py=29..42).
            else if (direction == 2)
            {
                for (int py = 29; py <= 42; py++)
                for (int px = 17; px <= 26; px++)
                {
                    if ((px == 17 && (py == 29 || py == 42 || py == 30))) continue;
                    bool isEdge = (px == 17 && py > 30 && py < 42) || (py == 29 && px > 17 && px <= 25) ||
                                  (py == 42 && px > 17 && px <= 25) || (px == 18 && py == 30);

                    if (isEdge) SetPixel(px, py, Outline);
                    else if (py >= 30 && py <= 31)
                    {
                        if (px == 19 || px == 20) SetPixel(px, py, RollSh);
                        else if (py == 30) SetPixel(px, py, RollHi);
                        else SetPixel(px, py, Roll);
                    }
                    else if (py == 32) SetPixel(px, py, Outline);
                    else if (py >= 33 && py <= 35)
                    {
                        if (px == 21) SetPixel(px, py, Strap);
                        else if (px <= 19) SetPixel(px, py, Shadow);
                        else if (py == 33) SetPixel(px, py, Highlight);
                        else SetPixel(px, py, Fabric);
                    }
                    else if (py == 36) SetPixel(px, py, px == 21 ? Buckle : Outline);
                    else
                    {
                        if (px == 21) SetPixel(px, py, Strap);
                        else if (px <= 19 || py >= 41) SetPixel(px, py, Shadow);
                        else SetPixel(px, py, Fabric);
                    }
                }

                // Braço e frente do Edelzio permanecem sempre em primeiro plano
                for (int uy = 0; uy < height; uy++)
                for (int ux = 0; ux < width; ux++)
                {
                    Color32 orig = original[uy * width + ux];
                    if (orig.a > 0 && (IsSkin(orig) || ux >= ToX(26)))
                        pixels[uy * width + ux] = orig;
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = body.name + "_ComMochila_" + direction,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(body.pivot.x / width, body.pivot.y / height), body.pixelsPerUnit,
                0, SpriteMeshType.FullRect, body.border);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.DontSave;
            _frames[key] = sprite;
            return sprite;
        }

        private static bool IsShirt(Color32 c) => c.a > 128 && c.r > 120 && c.g > 75
            && c.g < c.r * .83f && c.b < c.g * .45f;

        private static bool IsSkin(Color32 c) => c.a > 128 && c.r > 140 && c.g > 85 && c.b >= c.g * .45f;

        public void Dispose()
        {
            foreach (var sprite in _frames.Values)
            {
                if (sprite == null) continue;
                Destroy(sprite.texture);
                Destroy(sprite);
            }
            _frames.Clear();
        }

        private static void Destroy(UnityEngine.Object item)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(item);
            else UnityEngine.Object.DestroyImmediate(item);
        }
    }
}
