using System.Collections.Generic;
using UnityEngine;

namespace Game.Varginha
{
    /// <summary>Builds crisp, low-resolution sprites for the Varginha top-down map.</summary>
    public static class VarginhaPixelArtSprites
    {
        // A arte é desenhada em uma grade lógica de 32 e ampliada para 64px com detalhes finos.
        // O PPU também sobe para 64, preservando o tamanho físico de cada objeto na fase.
        private const int DetailScale = 2;
        private const int CanvasSize = 64;
        private const float PixelsPerUnit = 64f;
        private static readonly Dictionary<string, Sprite> Cache = new();

        /// <summary>Camada de barba falhada alinhada às folhas de Edelzio.</summary>
        public static Sprite CreateEdelzioBeard(bool attackSheet, int direction)
        {
            string key = "Edelzio_Barba_" + (attackSheet ? "Ataque_" : "Andar_") + direction;
            if (Cache.TryGetValue(key, out var cached))
            {
                if (cached != null && cached.texture != null) return cached;
                Cache.Remove(key);
            }

            int size = attackSheet ? 64 : 96;
            int height = attackSheet ? 64 : 43;
            float ppu = attackSheet ? 44.1379f : 66.2f;
            var texture = new Texture2D(size, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = key
            };
            var pixels = new Color[size * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            texture.SetPixels(pixels);

            // A barba não aparece na nuca. Nos perfis ela fica um pouco deslocada
            // para o lado do nariz para acompanhar as quatro direções da folha.
            if (direction != 3)
            {
                Color beard = new Color(.22f, .13f, .10f, .86f);
                Color highlight = new Color(.34f, .20f, .15f, .72f);
                int center = attackSheet ? 32 : 48;
                int baseY = attackSheet ? 22 : 7;
                int side = direction == 1 ? -2 : direction == 2 ? 2 : 0;
                int[][] marks =
                {
                    new[] { -7, 1 }, new[] { -4, 0 }, new[] { -1, 1 }, new[] { 3, 0 },
                    new[] { 6, 1 }, new[] { -5, 3 }, new[] { 0, 3 }, new[] { 4, 3 }
                };
                for (int i = 0; i < marks.Length; i++)
                {
                    int x = center + side + marks[i][0];
                    int y = baseY + marks[i][1];
                    FillRaw(texture, x, y, attackSheet ? 2 : 2, attackSheet ? 2 : 1,
                        (i & 1) == 0 ? beard : highlight);
                }
            }

            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, height), new Vector2(.5f, .5f), ppu);
            sprite.name = key;
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite Create(string id, Color color)
        {
            if (id.StartsWith("Backpack")) color = new Color(.46f, .48f, .50f);
            string key = id + color;
            if (Cache.TryGetValue(key, out var cached))
            {
                // Sprites procedurais pertencem à cena. Ao trocar de fase o Unity
                // destrói a textura, mas a referência estática ainda pode existir.
                // Descartar referências mortas evita itens, jaulas e ícones invisíveis.
                if (cached != null && cached.texture != null) return cached;
                Cache.Remove(key);
            }

            var texture = new Texture2D(CanvasSize, CanvasSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "Pixel_" + id
            };
            var pixels = new Color[CanvasSize * CanvasSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            texture.SetPixels(pixels);

            Color dark = Color.Lerp(color, Color.black, 0.45f);
            Color mid = color;
            Color light = Color.Lerp(color, Color.white, 0.35f);
            Color accent = Color.Lerp(color, new Color(0.2f, 0.9f, 0.85f), 0.24f);

            if (id == "Floor_House") DrawWoodFloor(texture, mid, dark, light);
            else if (id.StartsWith("ChurchFloor")) DrawChurchFloor(texture, mid, dark, light);
            else if (id == "Floor_Yard") DrawGrass(texture, mid, dark, light);
            else if (id.StartsWith("Street")) DrawStreet(texture, mid, dark, light);
            else if (id.StartsWith("Driveway")) DrawStone(texture, mid, dark, light);
            else if (id.StartsWith("Wall")) DrawWall(texture, mid, dark, light);
            else if (id.StartsWith("Fence")) DrawFence(texture, mid, dark, light);
            else if (id.StartsWith("Rug")) DrawRug(texture, mid, dark, light);
            else if (id.StartsWith("Bed")) DrawBed(texture, mid, dark, light);
            else if (id.StartsWith("Desk")) DrawDesk(texture, mid, dark, light);
            else if (id.StartsWith("Sofa")) DrawSofa(texture, mid, dark, light);
            else if (id.StartsWith("TV")) DrawTelevision(texture, mid, dark, light);
            else if (id.StartsWith("CoffeeTable")) DrawCoffeeTable(texture, mid, dark, light);
            else if (id.StartsWith("Bookshelf")) DrawBookshelf(texture, mid, dark, light);
            else if (id.StartsWith("Kitchen")) DrawKitchenCabinet(texture, mid, dark, light);
            else if (id.StartsWith("Nightstand")) DrawNightstand(texture, mid, dark, light);
            else if (id.StartsWith("Dresser")) DrawDresser(texture, mid, dark, light);
            else if (id.StartsWith("Chair")) DrawChair(texture, mid, dark, light);
            else if (id.StartsWith("Radio")) DrawRadio(texture, mid, dark, light);
            else if (id.StartsWith("Stove")) DrawStove(texture, mid, dark, light);
            else if (id.StartsWith("Fridge")) DrawFridge(texture, mid, dark, light);
            else if (id.StartsWith("Clock")) DrawClock(texture, mid, dark, light);
            else if (id.StartsWith("Plant")) DrawPlant(texture, mid, dark, light);
            else if (id.StartsWith("Lamp")) DrawLamp(texture, mid, dark, light);
            else if (id.StartsWith("StreetLamp")) DrawStreetLamp(texture, mid, dark, light);
            else if (id.StartsWith("Mailbox")) DrawMailbox(texture, mid, dark, light);
            else if (id.StartsWith("FlowerPatch")) DrawFlowerPatch(texture, mid, dark, light);
            else if (id.StartsWith("Shrub")) DrawShrub(texture, mid, dark, light);
            else if (id.StartsWith("GardenBorder")) DrawGardenBorder(texture, mid, dark, light);
            else if (id.StartsWith("Porch")) DrawPorch(texture, mid, dark, light);
            else if (id.StartsWith("RoadMarking")) DrawRoadMarking(texture, mid, dark, light);
            else if (id.StartsWith("Doorway")) DrawDoorway(texture, mid, dark, light);
            else if (id.StartsWith("Door_")) DrawDoor(texture, mid, dark, light);
            else if (id.StartsWith("BedroomRug") || id.StartsWith("KitchenRunner")) DrawRunner(texture, mid, dark, light);
            else if (id.StartsWith("WallPicture")) DrawWallPicture(texture, mid, dark, light);
            else if (id.StartsWith("ToyBox_Open")) DrawToyBoxOpen(texture, mid, dark, light);
            else if (id.StartsWith("ToyBox_Opening")) DrawToyBoxOpening(texture, mid, dark, light);
            else if (id.StartsWith("ToyBox")) DrawToyBox(texture, mid, dark, light);
            else if (id == "Backpack_Straps") DrawBackpackStraps(texture, mid, dark, light);
            else if (id.StartsWith("Backpack")) DrawBackpack(texture, mid, dark, light);
            else if (id == "Inventory_Key") DrawInventoryKey(texture);
            else if (id == "Inventory_Journal") DrawInventoryJournal(texture);
            else if (id.StartsWith("Notebook")) DrawNotebook(texture, mid, dark, light);
            else if (id.StartsWith("Coffee_Empty")) DrawCoffeeEmpty(texture, mid, dark, light);
            else if (id.StartsWith("Coffee")) DrawCoffee(texture, mid, dark, light);
            else if (id.StartsWith("Fuse")) DrawFuseBox(texture, mid, dark, light);
            else if (id.StartsWith("Doc")) DrawDocument(texture, mid, dark, light);
            else if (id.StartsWith("FuscaDoor_Open")) DrawFuscaDoor(texture, mid, dark, light, true, false);
            else if (id.StartsWith("FuscaDoor_Ajar")) DrawFuscaDoor(texture, mid, dark, light, false, true);
            else if (id.StartsWith("FuscaDoor")) DrawFuscaDoor(texture, mid, dark, light, false, false);
            else if (id.StartsWith("Padre_Fabio")) DrawPadre(texture, mid, dark, light);
            else if (id.StartsWith("Tome_Prop")) DrawTome(texture, mid, dark, light);
            else if (id.StartsWith("ChurchAltar")) DrawChurchAltar(texture, mid, dark, light);
            else if (id.StartsWith("SealSymbol")) DrawSealSymbol(texture, mid, dark, light);
            else if (id == "Attack_Slash") DrawAttackSlash(texture, mid, dark, light);
            else if (id == "Attack_Impact") DrawAttackImpact(texture, mid, dark, light);
            else if (id.StartsWith("HostageCage")) DrawHostageCage(texture, mid, dark, light);
            else if (id == "ET_Attack") DrawETAttack(texture, mid, dark, light);
            else if (id.StartsWith("StudentAttack_") || id.StartsWith("StudentAttackTrail_")) DrawStudentAttack(texture, mid, dark, light, id);
            else if (id.StartsWith("StudentHead")) DrawStudentHead(texture, mid, dark, light);
            else if (id.StartsWith("Student")) DrawStudent(texture, mid, dark, light);
            else if (id.StartsWith("ET_Subordinate")) DrawETSubordinate(texture, mid, dark, light);
            else if (id.StartsWith("Edelzio_Crouch")) DrawCharacterCrouch(texture, mid, dark, light, accent);
            else if (id.StartsWith("Edelzio_Reach")) DrawCharacterReach(texture, mid, dark, light, accent);
            else if (id.StartsWith("Edelzio_Sit")) DrawCharacterSit(texture, mid, dark, light, accent, false);
            else if (id.StartsWith("Edelzio_UseNotebook")) DrawCharacterSit(texture, mid, dark, light, accent, true);
            else if (id.StartsWith("Edelzio_DrinkCoffee")) DrawCharacterDrinkCoffee(texture, mid, dark, light, accent);
            else if (id.StartsWith("Edelzio_RunA")) DrawCharacterRun(texture, mid, dark, light, accent, true);
            else if (id.StartsWith("Edelzio_RunB")) DrawCharacterRun(texture, mid, dark, light, accent, false);
            else if (id.StartsWith("Edelzio_IdleB")) DrawCharacterIdle(texture, mid, dark, light, accent, true);
            else if (id.StartsWith("Edelzio")) DrawCharacterIdle(texture, mid, dark, light, accent, false);
            else if (id.StartsWith("Entity")) DrawEntity(texture, mid, dark, light);
            else DrawCrate(texture, mid, dark, light);

            AddMicroDetails(texture, id, dark, light);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, CanvasSize, CanvasSize), new Vector2(0.5f, 0.5f), PixelsPerUnit);
            sprite.name = "Pixel_" + id;
            Cache[key] = sprite;
            return sprite;
        }

        private static void DrawWoodFloor(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 0, 0, 32, 32, mid);
            for (int y = 0; y < 32; y += 5)
            {
                Fill(t, 0, y, 32, 1, dark);
                int seam = ((y / 5) & 1) == 0 ? 7 : 17;
                Fill(t, seam, y + 1, 1, 4, dark);
                Fill(t, (seam + 12) % 32, y + 1, 1, 4, dark);
                Fill(t, 2, y + 2, 4, 1, light);
                Fill(t, 23, y + 3, 5, 1, Color.Lerp(mid, light, 0.25f));
                Pixel(t, 12, y + 2, Color.Lerp(mid, dark, .18f));
                Pixel(t, 13, y + 2, Color.Lerp(mid, dark, .18f));
                Pixel(t, 28, y + 1, light);
            }
        }

        private static void DrawChurchFloor(Texture2D t, Color mid, Color dark, Color light)
        {
            Color stone = new Color(.18f, .20f, .24f);
            Color seam = new Color(.08f, .10f, .14f);
            Color glint = new Color(.30f, .34f, .39f);
            Fill(t, 0, 0, 32, 32, stone);
            for (int y = 1; y < 32; y += 8)
            {
                Fill(t, 0, y, 32, 1, seam);
                int offset = ((y / 8) & 1) == 0 ? 4 : 12;
                for (int x = offset; x < 32; x += 16) Fill(t, x, y + 1, 1, 7, seam);
            }
            Fill(t, 3, 3, 5, 1, glint); Fill(t, 21, 5, 7, 1, glint);
            Pixel(t, 13, 12, light); Pixel(t, 25, 26, light);
        }

        private static void DrawPadre(Texture2D t, Color mid, Color dark, Color light)
        {
            Color robe = new Color(.16f, .13f, .18f);
            Color robeLight = new Color(.30f, .25f, .34f);
            Color skin = new Color(.76f, .48f, .32f);
            Color collar = new Color(.92f, .90f, .78f);
            Color hair = new Color(.10f, .07f, .06f);
            Fill(t, 9, 3, 14, 3, hair); Fill(t, 7, 6, 18, 9, hair);
            Fill(t, 10, 8, 12, 7, skin); Fill(t, 11, 13, 10, 2, skin);
            Fill(t, 8, 15, 16, 13, robe); Fill(t, 10, 16, 12, 10, robeLight);
            Fill(t, 14, 15, 4, 11, collar); Fill(t, 15, 16, 2, 8, new Color(.55f, .42f, .16f));
            Fill(t, 7, 26, 7, 3, dark); Fill(t, 18, 26, 7, 3, dark);
            Pixel(t, 12, 10, Color.black); Pixel(t, 19, 10, Color.black);
            Fill(t, 13, 13, 6, 1, hair);
        }

        private static void DrawTome(Texture2D t, Color mid, Color dark, Color light)
        {
            Color cover = new Color(.34f, .10f, .08f);
            Color coverLight = new Color(.62f, .25f, .13f);
            Color page = new Color(.88f, .76f, .52f);
            Fill(t, 5, 7, 22, 19, dark); Fill(t, 7, 6, 18, 20, cover);
            Fill(t, 9, 8, 14, 16, coverLight); Fill(t, 10, 10, 12, 12, page);
            Fill(t, 15, 10, 2, 12, cover); Fill(t, 11, 14, 10, 1, cover);
            Pixel(t, 12, 12, new Color(.75f, .64f, .34f)); Pixel(t, 20, 18, new Color(.75f, .64f, .34f));
        }

        private static void DrawChurchAltar(Texture2D t, Color mid, Color dark, Color light)
        {
            Color wood = new Color(.27f, .15f, .10f);
            Color trim = new Color(.65f, .42f, .20f);
            Fill(t, 4, 7, 24, 5, trim); Fill(t, 6, 12, 20, 15, wood);
            Fill(t, 9, 15, 14, 3, trim); Fill(t, 10, 19, 12, 2, dark);
            Fill(t, 8, 25, 5, 4, dark); Fill(t, 19, 25, 5, 4, dark);
        }

        private static void DrawSealSymbol(Texture2D t, Color mid, Color dark, Color light)
        {
            Color glow = new Color(.36f, .86f, .92f);
            Color dim = new Color(.08f, .28f, .34f);
            Fill(t, 15, 3, 2, 26, dim); Fill(t, 3, 15, 26, 2, dim);
            Fill(t, 8, 8, 16, 2, glow); Fill(t, 8, 22, 16, 2, glow);
            Fill(t, 8, 10, 2, 12, glow); Fill(t, 22, 10, 2, 12, glow);
            Fill(t, 13, 13, 6, 6, glow); Pixel(t, 15, 15, Color.white);
        }

        private static void DrawGrass(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 0, 0, 32, 32, mid);
            for (int y = 2; y < 32; y += 5)
            for (int x = (y & 2); x < 32; x += 6)
            {
                Pixel(t, x, y, light);
                Pixel(t, x + 1, y + 1, dark);
                Pixel(t, x + 2, y, Color.Lerp(mid, dark, .2f));
            }
        }

        private static void DrawStreet(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 0, 0, 32, 32, mid);
            for (int y = 4; y < 32; y += 9) Fill(t, 2, y, 28, 1, dark);
            for (int x = 2; x < 32; x += 10) Fill(t, x, 15, 5, 2, light);
            Pixel(t, 7, 7, light); Pixel(t, 25, 25, light); Pixel(t, 18, 4, dark);
        }

        private static void DrawStone(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 0, 0, 32, 32, dark);
            for (int y = 1; y < 32; y += 8)
            for (int x = ((y / 8) & 1) == 0 ? 1 : 5; x < 32; x += 10)
            {
                Fill(t, x, y, 8, 6, mid);
                Fill(t, x + 1, y + 4, 5, 1, light);
            }
        }

        private static void DrawWall(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 0, 0, 32, 32, mid);
            for (int y = 2; y < 32; y += 7)
            {
                Fill(t, 0, y, 32, 2, dark);
                int offset = ((y / 7) & 1) == 0 ? 4 : 9;
                for (int x = offset; x < 32; x += 10) Fill(t, x, y + 2, 1, 5, Color.Lerp(dark, mid, .45f));
            }
            Fill(t, 0, 28, 32, 3, light);
            Fill(t, 2, 29, 28, 1, Color.Lerp(light, Color.white, .15f));
        }

        private static void DrawFence(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 1, 5, 30, 5, dark);
            Fill(t, 1, 12, 30, 4, mid);
            for (int x = 3; x < 32; x += 7)
            {
                Fill(t, x, 1, 4, 30, dark);
                Fill(t, x + 1, 2, 2, 28, light);
            }
        }

        private static void DrawRug(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 3, 28, 26, dark); Fill(t, 4, 5, 24, 22, mid);
            Fill(t, 6, 7, 20, 2, light); Fill(t, 6, 23, 20, 2, light);
            Fill(t, 6, 9, 2, 14, light); Fill(t, 24, 9, 2, 14, light);
            Fill(t, 12, 11, 8, 10, dark); Fill(t, 14, 13, 4, 6, light);
            Pixel(t, 9, 10, light); Pixel(t, 22, 21, light); Pixel(t, 10, 22, light); Pixel(t, 21, 10, light);
        }

        private static void DrawCoffeeTable(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 8, 28, 16, dark); Fill(t, 4, 10, 24, 12, mid);
            Fill(t, 5, 19, 22, 2, light); Fill(t, 6, 11, 20, 2, Color.Lerp(light, Color.white, .2f));
            Fill(t, 4, 3, 5, 7, dark); Fill(t, 23, 3, 5, 7, dark); Fill(t, 4, 22, 5, 7, dark); Fill(t, 23, 22, 5, 7, dark);
            Fill(t, 5, 3, 2, 7, Color.Lerp(mid, light, .35f)); Fill(t, 24, 3, 2, 7, Color.Lerp(mid, light, .35f));
            Fill(t, 11, 14, 10, 5, Color.Lerp(light, Color.white, .35f)); Fill(t, 13, 15, 6, 1, mid);
        }

        private static void DrawBookshelf(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 1, 28, 30, dark); Fill(t, 4, 3, 24, 26, mid); Fill(t, 5, 4, 22, 2, light);
            for (int y = 7; y < 28; y += 6)
            {
                Fill(t, 4, y, 24, 2, dark);
                Fill(t, 6, y + 2, 3, 3, light); Fill(t, 10, y + 2, 2, 3, new Color(.78f, .18f, .17f));
                Fill(t, 13, y + 2, 2, 3, new Color(.25f, .50f, .82f)); Fill(t, 16, y + 2, 3, 3, new Color(.35f, .72f, .42f));
                Fill(t, 20, y + 2, 2, 3, new Color(.94f, .75f, .25f)); Fill(t, 23, y + 2, 2, 3, Color.Lerp(mid, dark, .25f));
            }
        }

        private static void DrawKitchenCabinet(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 4, 28, 24, dark); Fill(t, 3, 23, 26, 4, light); Fill(t, 4, 7, 24, 16, mid);
            Fill(t, 4, 5, 24, 3, Color.Lerp(light, Color.white, .15f)); Fill(t, 5, 9, 10, 12, Color.Lerp(mid, dark, .22f));
            Fill(t, 17, 9, 9, 12, Color.Lerp(mid, dark, .22f)); Fill(t, 6, 10, 8, 1, light); Fill(t, 18, 10, 7, 1, light);
            Fill(t, 9, 14, 2, 2, light); Fill(t, 21, 14, 2, 2, light); Fill(t, 14, 8, 2, 15, dark);
        }

        private static void DrawNightstand(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 5, 4, 22, 24, dark); Fill(t, 7, 6, 18, 19, mid); Fill(t, 8, 7, 16, 2, light);
            Fill(t, 9, 12, 14, 5, Color.Lerp(mid, dark, .2f)); Fill(t, 9, 19, 14, 5, Color.Lerp(mid, dark, .28f));
            Fill(t, 15, 14, 2, 1, light); Fill(t, 15, 21, 2, 1, light); Fill(t, 8, 25, 3, 4, dark); Fill(t, 21, 25, 3, 4, dark);
        }

        private static void DrawDresser(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 3, 4, 26, 24, dark); Fill(t, 5, 6, 22, 20, mid); Fill(t, 6, 7, 20, 2, light);
            for (int y = 11; y < 25; y += 6)
            {
                Fill(t, 7, y, 18, 4, Color.Lerp(mid, dark, .23f)); Fill(t, 10, y + 1, 2, 1, light); Fill(t, 20, y + 1, 2, 1, light);
            }
            Fill(t, 6, 26, 3, 3, dark); Fill(t, 23, 26, 3, 3, dark);
        }

        private static void DrawChair(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 8, 14, 16, 11, dark); Fill(t, 10, 16, 12, 7, mid); Fill(t, 11, 17, 10, 2, light);
            Fill(t, 8, 4, 5, 12, dark); Fill(t, 10, 5, 3, 10, mid); Fill(t, 19, 4, 5, 12, dark); Fill(t, 19, 5, 3, 10, mid);
            Fill(t, 9, 24, 3, 5, dark); Fill(t, 20, 24, 3, 5, dark);
        }

        private static void DrawRadio(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 3, 10, 26, 14, dark); Fill(t, 5, 12, 22, 10, mid); Fill(t, 7, 14, 7, 6, dark); Fill(t, 8, 15, 5, 4, light);
            Fill(t, 17, 14, 7, 6, dark); Fill(t, 18, 15, 5, 4, Color.Lerp(light, Color.white, .3f));
            Fill(t, 8, 24, 3, 3, dark); Fill(t, 21, 24, 3, 3, dark); Fill(t, 12, 5, 2, 6, dark); Fill(t, 19, 5, 2, 6, dark);
        }

        private static void DrawStove(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 4, 3, 24, 26, dark); Fill(t, 6, 5, 20, 22, mid); Fill(t, 7, 6, 18, 7, Color.Lerp(light, Color.white, .2f));
            Fill(t, 9, 8, 4, 3, dark); Fill(t, 19, 8, 4, 3, dark); Fill(t, 8, 15, 16, 9, Color.Lerp(mid, dark, .38f));
            Fill(t, 10, 17, 12, 5, new Color(.08f, .12f, .15f)); Fill(t, 15, 25, 3, 1, light); Fill(t, 6, 27, 3, 3, dark); Fill(t, 23, 27, 3, 3, dark);
        }

        private static void DrawFridge(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 6, 2, 20, 29, dark); Fill(t, 8, 4, 16, 24, mid); Fill(t, 9, 5, 14, 10, Color.Lerp(mid, light, .12f));
            Fill(t, 8, 16, 16, 2, dark); Fill(t, 9, 19, 14, 8, Color.Lerp(mid, dark, .12f)); Fill(t, 21, 7, 1, 6, light); Fill(t, 21, 21, 1, 4, light);
            Fill(t, 10, 6, 3, 3, new Color(.85f, .20f, .17f)); Fill(t, 14, 6, 3, 3, Color.white); Fill(t, 9, 28, 3, 3, dark); Fill(t, 20, 28, 3, 3, dark);
        }

        private static void DrawClock(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 9, 4, 14, 24, dark); Fill(t, 7, 9, 18, 14, dark); Fill(t, 9, 10, 14, 12, light); Fill(t, 10, 11, 12, 10, Color.Lerp(light, Color.white, .2f));
            Fill(t, 15, 16, 1, 4, dark); Fill(t, 15, 16, 4, 1, dark); Pixel(t, 15, 12, dark); Pixel(t, 15, 20, dark); Pixel(t, 11, 16, dark); Pixel(t, 20, 16, dark);
            Fill(t, 13, 3, 6, 2, mid); Fill(t, 12, 27, 8, 3, mid);
        }

        private static void DrawPlant(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 11, 3, 10, 8, dark); Fill(t, 10, 5, 12, 5, mid); Fill(t, 7, 9, 18, 5, mid);
            Fill(t, 12, 13, 8, 3, light); Fill(t, 9, 15, 14, 11, dark); Fill(t, 10, 16, 12, 8, new Color(.48f, .25f, .12f));
        }

        private static void DrawLamp(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 8, 18, 16, 7, dark); Fill(t, 10, 19, 12, 5, light); Fill(t, 14, 5, 4, 14, dark);
            Fill(t, 11, 2, 10, 3, mid); Fill(t, 8, 1, 16, 2, dark);
        }

        private static void DrawStreetLamp(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 13, 1, 6, 25, dark); Fill(t, 14, 2, 4, 23, mid); Fill(t, 9, 25, 14, 4, dark);
            Fill(t, 9, 26, 14, 2, mid); Fill(t, 9, 24, 14, 2, light); Fill(t, 10, 29, 12, 2, dark);
        }

        private static void DrawMailbox(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 10, 3, 12, 9, dark); Fill(t, 11, 5, 10, 6, mid); Fill(t, 10, 10, 12, 2, light);
            Fill(t, 14, 12, 4, 15, dark); Fill(t, 15, 13, 2, 14, mid); Fill(t, 10, 27, 12, 3, dark);
        }

        private static void DrawFlowerPatch(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 3, 8, 26, 15, dark); Fill(t, 5, 9, 22, 13, new Color(.14f, .38f, .15f));
            for (int x = 7; x < 27; x += 6)
            {
                Fill(t, x, 13 + (x & 2), 3, 3, mid); Pixel(t, x + 1, 14 + (x & 2), light);
            }
        }

        private static void DrawShrub(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 4, 6, 24, 17, dark); Fill(t, 6, 5, 20, 19, mid);
            Fill(t, 9, 23, 14, 5, dark); Fill(t, 7, 10, 4, 8, light); Fill(t, 18, 13, 5, 7, light);
            Pixel(t, 13, 7, light); Pixel(t, 22, 9, light);
        }

        private static void DrawGardenBorder(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 0, 9, 32, 14, dark); Fill(t, 0, 12, 32, 8, mid);
            for (int x = 2; x < 32; x += 8) { Fill(t, x, 8, 2, 16, light); Pixel(t, x + 1, 7, light); }
        }

        private static void DrawPorch(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 1, 1, 30, 30, dark);
            for (int y = 3; y < 30; y += 6) { Fill(t, 3, y, 26, 4, mid); Fill(t, 4, y + 1, 22, 1, light); }
        }

        private static void DrawRoadMarking(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 12, 28, 8, dark); Fill(t, 3, 13, 26, 6, mid); Fill(t, 5, 15, 22, 2, light);
        }

        private static void DrawDoorway(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 2, 28, 28, dark); Fill(t, 5, 5, 22, 22, mid); Fill(t, 7, 7, 18, 3, light);
            Fill(t, 22, 14, 2, 2, light);
        }

        private static void DrawDoor(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 1, 28, 30, dark);
            Fill(t, 5, 3, 22, 27, mid);
            Fill(t, 7, 5, 18, 2, light);
            Fill(t, 8, 10, 7, 9, Color.Lerp(mid, dark, .25f));
            Fill(t, 17, 10, 6, 9, Color.Lerp(mid, dark, .25f));
            Fill(t, 21, 15, 2, 2, light);
        }

        private static void DrawRunner(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 1, 3, 30, 26, dark); Fill(t, 3, 5, 26, 22, mid); Fill(t, 5, 7, 22, 2, light); Fill(t, 5, 23, 22, 2, light);
            for (int x = 7; x < 26; x += 6) Fill(t, x, 11, 3, 10, Color.Lerp(light, Color.white, .15f));
        }

        private static void DrawWallPicture(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 4, 28, 24, dark); Fill(t, 4, 6, 24, 20, light); Fill(t, 6, 8, 20, 16, mid);
            Fill(t, 7, 9, 18, 4, Color.Lerp(mid, light, .25f)); Fill(t, 10, 13, 5, 8, dark); Fill(t, 16, 16, 7, 5, dark);
        }

        private static void DrawBed(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 1, 1, 30, 30, dark); Fill(t, 3, 3, 26, 26, Color.Lerp(mid, dark, .2f));
            Fill(t, 4, 5, 24, 9, Color.Lerp(light, Color.white, .18f)); Fill(t, 5, 6, 10, 6, Color.Lerp(light, Color.white, .45f));
            Fill(t, 17, 6, 10, 6, Color.Lerp(light, Color.white, .36f)); Fill(t, 5, 14, 22, 12, mid);
            Fill(t, 5, 22, 22, 4, light); Fill(t, 8, 15, 2, 6, Color.Lerp(mid, dark, .28f)); Fill(t, 22, 15, 2, 6, Color.Lerp(mid, dark, .28f));
            Fill(t, 1, 1, 3, 30, Color.Lerp(dark, Color.black, .25f)); Fill(t, 28, 1, 3, 30, Color.Lerp(dark, Color.black, .25f));
        }

        private static void DrawDesk(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 8, 28, 17, dark); Fill(t, 3, 11, 26, 12, mid); Fill(t, 4, 20, 24, 3, light);
            Fill(t, 4, 2, 5, 10, dark); Fill(t, 23, 2, 5, 10, dark); Fill(t, 5, 3, 2, 8, Color.Lerp(mid, light, .3f));
            Fill(t, 24, 3, 2, 8, Color.Lerp(mid, light, .3f)); Fill(t, 12, 13, 8, 4, Color.Lerp(mid, dark, .22f));
            Fill(t, 13, 14, 6, 1, light); Fill(t, 14, 17, 4, 2, dark);
        }

        private static void DrawSofa(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 5, 28, 21, dark); Fill(t, 4, 7, 24, 17, mid); Fill(t, 5, 16, 22, 7, light);
            Fill(t, 2, 4, 4, 22, Color.Lerp(dark, Color.black, .15f)); Fill(t, 26, 4, 4, 22, Color.Lerp(dark, Color.black, .15f));
            Fill(t, 6, 2, 3, 5, dark); Fill(t, 23, 2, 3, 5, dark); Fill(t, 6, 9, 9, 6, Color.Lerp(mid, light, .2f));
            Fill(t, 17, 9, 9, 6, Color.Lerp(mid, light, .2f)); Fill(t, 14, 8, 2, 15, dark);
        }

        private static void DrawTelevision(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 2, 7, 28, 19, dark); Fill(t, 4, 9, 22, 15, Color.Lerp(mid, Color.black, .55f));
            Fill(t, 6, 11, 18, 11, light); for (int y = 12; y < 22; y += 3) Fill(t, 6, y, 18, 1, mid);
            Fill(t, 7, 14, 16, 1, Color.white); Fill(t, 25, 12, 3, 3, mid); Fill(t, 11, 3, 3, 5, dark); Fill(t, 20, 3, 3, 5, dark);
            Fill(t, 8, 4, 7, 1, light); Fill(t, 18, 5, 7, 1, light);
        }

        private static void DrawToyBox(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 3, 4, 26, 22, dark); Fill(t, 5, 6, 22, 16, mid); Fill(t, 4, 21, 24, 5, light);
            Fill(t, 4, 3, 24, 5, Color.Lerp(mid, light, .25f)); Fill(t, 5, 4, 22, 1, Color.Lerp(light, Color.white, .3f));
            Fill(t, 14, 8, 4, 14, dark); Fill(t, 5, 13, 22, 2, dark); Fill(t, 14, 12, 4, 4, light);
            Fill(t, 7, 17, 5, 2, Color.Lerp(mid, dark, .25f)); Fill(t, 20, 17, 5, 2, Color.Lerp(mid, dark, .25f));
        }

        private static void DrawToyBoxOpening(Texture2D t, Color mid, Color dark, Color light)
        {
            // A tampa levanta no segundo quadro, revelando uma faixa interna brilhante.
            Fill(t, 3, 4, 26, 18, dark); Fill(t, 5, 6, 22, 14, mid); Fill(t, 4, 19, 24, 6, light);
            Fill(t, 5, 18, 22, 3, new Color(.18f, .10f, .05f)); Fill(t, 14, 8, 4, 12, dark); Fill(t, 14, 12, 4, 4, light);
            Fill(t, 5, 2, 22, 4, Color.Lerp(mid, light, .28f)); Fill(t, 7, 0, 18, 2, light);
        }

        private static void DrawToyBoxOpen(Texture2D t, Color mid, Color dark, Color light)
        {
            // Quadro final: tampa acima da caixa, caderno e chave visíveis no interior.
            Fill(t, 3, 5, 26, 17, dark); Fill(t, 5, 7, 22, 13, mid); Fill(t, 4, 19, 24, 6, light);
            Fill(t, 6, 16, 20, 4, new Color(.12f, .07f, .04f)); Fill(t, 8, 16, 7, 3, new Color(.82f, .78f, .58f));
            Fill(t, 18, 16, 5, 2, new Color(.95f, .74f, .20f)); Pixel(t, 23, 17, new Color(.95f, .74f, .20f));
            Fill(t, 4, 0, 24, 5, dark); Fill(t, 6, 1, 20, 3, Color.Lerp(mid, light, .30f)); Fill(t, 7, 1, 18, 1, Color.Lerp(light, Color.white, .3f));
        }

        private static void DrawBackpack(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 8, 5, 16, 22, dark); Fill(t, 9, 7, 14, 18, mid); Fill(t, 10, 18, 12, 7, Color.Lerp(mid, dark, .18f));
            Fill(t, 11, 20, 10, 4, light); Fill(t, 12, 21, 8, 1, Color.Lerp(light, Color.white, .3f));
            Fill(t, 11, 25, 4, 3, dark); Fill(t, 17, 25, 4, 3, dark); Fill(t, 11, 2, 10, 5, light);
            Fill(t, 13, 1, 6, 2, dark); Fill(t, 7, 10, 2, 10, dark); Fill(t, 23, 10, 2, 10, dark);
        }

        private static void DrawBackpackStraps(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 8, 9, 4, 14, dark); Fill(t, 20, 9, 4, 14, dark);
            Fill(t, 9, 10, 2, 12, light); Fill(t, 21, 10, 2, 12, light);
            Fill(t, 8, 11, 4, 2, mid); Fill(t, 20, 11, 4, 2, mid);
        }

        private static void DrawInventoryKey(Texture2D t)
        {
            var edge = new Color(.17f, .13f, .09f);
            var gold = new Color(.92f, .68f, .25f);
            Fill(t, 5, 17, 12, 11, edge); Fill(t, 7, 19, 8, 7, gold);
            Fill(t, 9, 21, 4, 3, edge); Fill(t, 10, 5, 5, 14, edge);
            Fill(t, 11, 6, 3, 14, gold); Fill(t, 14, 5, 6, 4, edge);
            Fill(t, 14, 6, 5, 2, gold); Fill(t, 14, 11, 5, 3, gold);
        }

        private static void DrawInventoryJournal(Texture2D t)
        {
            Fill(t, 6, 4, 21, 25, new Color(.12f, .09f, .07f));
            Fill(t, 8, 6, 17, 21, new Color(.57f, .30f, .13f));
            Fill(t, 8, 6, 3, 21, new Color(.31f, .16f, .10f));
            Fill(t, 13, 17, 10, 6, new Color(.90f, .80f, .57f));
            Fill(t, 12, 7, 12, 2, new Color(.91f, .86f, .69f));
            Fill(t, 20, 4, 2, 7, new Color(.65f, .15f, .12f));
        }

        private static void DrawNotebook(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 3, 9, 26, 16, dark); Fill(t, 5, 11, 22, 12, mid); Fill(t, 7, 13, 18, 8, new Color(.20f, .78f, .86f));
            Fill(t, 9, 15, 14, 4, Color.Lerp(light, Color.white, .45f)); Fill(t, 10, 16, 12, 1, Color.white);
            Fill(t, 1, 5, 30, 4, dark); Fill(t, 3, 6, 26, 2, light); Fill(t, 4, 23, 24, 2, Color.Lerp(mid, dark, .3f));
            Pixel(t, 25, 12, light); Pixel(t, 25, 13, light);
        }

        private static void DrawCoffee(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 9, 8, 14, 14, dark); Fill(t, 10, 9, 12, 12, mid); Fill(t, 12, 12, 8, 7, new Color(.20f, .09f, .04f));
            Fill(t, 11, 10, 10, 1, light); Fill(t, 22, 11, 5, 7, dark); Fill(t, 23, 12, 3, 5, light);
            Fill(t, 8, 5, 2, 4, light); Fill(t, 16, 4, 2, 5, light); Pixel(t, 19, 7, light);
        }

        private static void DrawCoffeeEmpty(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 9, 8, 14, 14, dark); Fill(t, 10, 9, 12, 12, mid); Fill(t, 12, 12, 8, 3, Color.Lerp(mid, dark, .3f));
            Fill(t, 11, 10, 10, 1, light); Fill(t, 22, 11, 5, 7, dark); Fill(t, 23, 12, 3, 5, light);
            // Vapor desapareceu: a xícara vazia fica sobre a mesa como pista visual da interação concluída.
        }

        private static void DrawFuseBox(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 5, 4, 22, 24, dark); Fill(t, 7, 6, 18, 20, mid); Fill(t, 9, 8, 14, 7, light);
            Fill(t, 11, 9, 10, 2, Color.Lerp(light, Color.white, .3f)); Fill(t, 9, 18, 3, 4, dark);
            Fill(t, 15, 18, 3, 4, dark); Fill(t, 21, 18, 2, 4, dark); Fill(t, 10, 19, 1, 2, light); Fill(t, 16, 19, 1, 2, light);
        }

        private static void DrawDocument(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 6, 3, 20, 26, dark); Fill(t, 8, 5, 16, 22, light); Fill(t, 20, 5, 4, 4, Color.Lerp(light, dark, .25f));
            for (int y = 10; y < 24; y += 4) Fill(t, 10, y, 11 + ((y * 3) % 4), 1, mid);
            Fill(t, 10, 8, 7, 1, dark); Fill(t, 19, 23, 3, 2, new Color(.55f, .12f, .10f));
        }

        private static void DrawCharacter(Texture2D t, Color mid, Color dark, Color light, Color accent)
        {
            // Cabelo escuro, óculos quadrados e polo verde remetem ao sprite clássico do Edelzio.
            Color stubble = new Color(.25f, .16f, .12f);
            Fill(t, 11, 19, 10, 10, dark); Fill(t, 12, 20, 8, 8, new Color(.95f, .65f, .43f));
            Fill(t, 12, 26, 8, 3, new Color(.06f, .07f, .08f));
            Fill(t, 12, 23, 3, 2, new Color(.05f, .05f, .06f)); Fill(t, 17, 23, 3, 2, new Color(.05f, .05f, .06f));
            Pixel(t, 15, 24, new Color(.05f, .05f, .06f)); Pixel(t, 16, 24, new Color(.05f, .05f, .06f));
            // Barba falhada: pontos separados no maxilar e abaixo da boca, sem
            // transformar o rosto em uma mancha sólida.
            Pixel(t, 13, 21, stubble); Pixel(t, 15, 20, stubble); Pixel(t, 18, 20, stubble);
            Pixel(t, 20, 21, stubble); Pixel(t, 12, 22, stubble); Pixel(t, 19, 22, stubble);
            Fill(t, 8, 8, 16, 13, dark); Fill(t, 10, 9, 12, 11, mid); Fill(t, 11, 14, 10, 4, accent);
            Fill(t, 9, 3, 5, 7, dark); Fill(t, 18, 3, 5, 7, dark); Fill(t, 13, 20, 2, 3, light); Fill(t, 18, 20, 2, 3, light);
        }

        private static void DrawCharacterIdle(Texture2D t, Color mid, Color dark, Color light, Color accent, bool breathingFrame)
        {
            DrawCharacter(t, mid, dark, light, accent);
            if (!breathingFrame) return;
            // Segundo quadro de idle: ombros e casaco sobem um pixel para simular respiração.
            Fill(t, 10, 10, 12, 1, Color.Lerp(mid, light, .12f));
            Fill(t, 11, 18, 10, 1, accent);
        }

        private static void DrawCharacterRun(Texture2D t, Color mid, Color dark, Color light, Color accent, bool leftStep)
        {
            DrawCharacter(t, mid, dark, light, accent);
            if (leftStep)
            {
                Fill(t, 7, 2, 7, 3, dark); Fill(t, 19, 5, 6, 3, dark);
                Fill(t, 8, 1, 4, 1, Color.Lerp(dark, light, .3f)); Fill(t, 22, 4, 2, 1, Color.Lerp(dark, light, .25f));
            }
            else
            {
                Fill(t, 8, 5, 6, 3, dark); Fill(t, 18, 2, 7, 3, dark);
                Fill(t, 9, 4, 2, 1, Color.Lerp(dark, light, .25f)); Fill(t, 21, 1, 4, 1, Color.Lerp(dark, light, .3f));
            }
        }

        private static void DrawCharacterCrouch(Texture2D t, Color mid, Color dark, Color light, Color accent)
        {
            Fill(t, 11, 14, 10, 10, dark); Fill(t, 12, 15, 8, 8, new Color(.95f, .65f, .43f));
            Fill(t, 8, 6, 16, 9, dark); Fill(t, 10, 7, 12, 7, mid); Fill(t, 11, 10, 10, 3, accent);
            Fill(t, 7, 3, 9, 3, dark); Fill(t, 16, 3, 9, 3, dark); Fill(t, 6, 8, 5, 2, light);
        }

        private static void DrawFuscaDoor(Texture2D t, Color mid, Color dark, Color light, bool open, bool ajar)
        {
            Color glass = new Color(.46f, .78f, .86f);
            Color glassLight = new Color(.75f, .93f, .94f);
            // Porta completa em baixa resolução: contorno, janela, chapa, friso
            // e maçaneta seguem a mesma linguagem quadriculada do Fusca.
            Fill(t, 5, 3, 22, 26, dark);
            Fill(t, 7, 5, 18, 22, mid);
            Fill(t, 8, 6, 16, 9, dark);
            Fill(t, 9, 7, 14, 7, glass);
            Fill(t, 10, 8, 7, 2, glassLight);
            Fill(t, 18, 8, 4, 1, Color.Lerp(glass, glassLight, .35f));
            Fill(t, 8, 15, 16, 2, Color.Lerp(mid, light, .35f));
            Fill(t, 9, 18, 14, 7, mid);
            Fill(t, 10, 20, 12, 1, light);
            Fill(t, 19, 17, 3, 2, dark); Fill(t, 20, 17, 2, 1, glassLight);
            Fill(t, 9, 25, 4, 2, dark); Fill(t, 19, 25, 4, 2, dark);
            if (open) Fill(t, 3, 7, 4, 18, Color.Lerp(dark, Color.black, .4f));
            else if (ajar)
            {
                // Quadro intermediário: o vão aparece, mas a folha ainda não
                // atravessa a carroceria como acontecia na troca instantânea.
                Fill(t, 5, 7, 2, 18, Color.Lerp(dark, Color.black, .22f));
                Fill(t, 6, 9, 1, 13, light);
            }
        }

        private static void DrawAttackSlash(Texture2D t, Color mid, Color dark, Color light)
        {
            // Arco frontal apontado para a direita; o controlador gira o efeito para cada direção.
            Fill(t, 22, 5, 3, 4, dark); Fill(t, 26, 8, 3, 4, mid); Fill(t, 29, 12, 3, 4, light);
            Fill(t, 31, 17, 3, 9, light); Fill(t, 29, 27, 3, 4, light); Fill(t, 25, 31, 3, 4, mid); Fill(t, 21, 34, 3, 4, dark);
            Fill(t, 18, 9, 2, 3, Color.Lerp(light, Color.white, .35f)); Fill(t, 23, 36, 5, 2, dark);
        }

        private static void DrawAttackImpact(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 19, 3, 4, 26, light); Fill(t, 8, 14, 26, 4, light);
            Fill(t, 13, 8, 4, 16, Color.white); Fill(t, 25, 8, 4, 16, Color.white);
            Fill(t, 17, 17, 10, 4, Color.white); Fill(t, 3, 7, 4, 3, light); Fill(t, 31, 28, 4, 3, light);
        }

        private static void DrawCharacterReach(Texture2D t, Color mid, Color dark, Color light, Color accent)
        {
            DrawCharacter(t, mid, dark, light, accent);
            Fill(t, 21, 12, 7, 3, dark); Fill(t, 25, 11, 4, 3, new Color(.95f, .65f, .43f));
            Fill(t, 7, 5, 6, 3, dark);
        }

        private static void DrawCharacterSit(Texture2D t, Color mid, Color dark, Color light, Color accent, bool usingNotebook)
        {
            Fill(t, 11, 17, 10, 10, dark); Fill(t, 12, 18, 8, 8, new Color(.95f, .65f, .43f));
            Fill(t, 8, 9, 16, 9, dark); Fill(t, 10, 10, 12, 7, mid); Fill(t, 11, 13, 10, 3, accent);
            Fill(t, 8, 5, 14, 4, dark); Fill(t, 18, 4, 8, 3, dark);
            if (!usingNotebook) return;
            Fill(t, 20, 11, 10, 6, dark); Fill(t, 21, 12, 8, 4, new Color(.20f, .78f, .86f));
            Fill(t, 9, 14, 6, 2, new Color(.95f, .65f, .43f)); Fill(t, 20, 14, 5, 2, new Color(.95f, .65f, .43f));
        }

        private static void DrawCharacterDrinkCoffee(Texture2D t, Color mid, Color dark, Color light, Color accent)
        {
            DrawCharacter(t, mid, dark, light, accent);
            Fill(t, 20, 17, 6, 3, dark); Fill(t, 24, 18, 4, 3, new Color(.95f, .65f, .43f));
            Fill(t, 25, 20, 4, 5, dark); Fill(t, 26, 21, 3, 3, mid); Fill(t, 28, 21, 2, 3, light);
        }

        private static void DrawEntity(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 10, 6, 12, 20, dark); Fill(t, 7, 10, 18, 13, mid); Fill(t, 4, 12, 4, 5, dark); Fill(t, 24, 12, 4, 5, dark);
            Fill(t, 11, 19, 3, 3, light); Fill(t, 18, 19, 3, 3, light); Fill(t, 12, 7, 2, 3, dark); Fill(t, 18, 7, 2, 3, dark);
        }

        private static void DrawStudent(Texture2D t, Color mid, Color dark, Color light)
        {
            // Silhueta simples, legível de cima, com roupa colorida para diferenciar os reféns.
            Fill(t, 10, 5, 12, 10, dark); Fill(t, 12, 7, 8, 7, new Color(.86f, .62f, .44f));
            Fill(t, 8, 14, 16, 11, dark); Fill(t, 10, 15, 12, 8, mid);
            Fill(t, 11, 16, 10, 2, light); Fill(t, 10, 24, 5, 4, dark); Fill(t, 19, 24, 5, 4, dark);
            Fill(t, 8, 5, 16, 3, dark); Fill(t, 10, 3, 12, 3, dark);
            Pixel(t, 13, 10, new Color(.07f, .07f, .08f)); Pixel(t, 18, 10, new Color(.07f, .07f, .08f));
            Fill(t, 14, 12, 4, 1, new Color(.45f, .12f, .12f));
        }

        private static void DrawStudentHead(Texture2D t, Color mid, Color dark, Color light)
        {
            // Ícone compacto usado na Fase 3 para manter a turma legível sem
            // movimentar nove corpos sobrepostos pelo cenário.
            Color skin = new Color(.86f, .62f, .44f);
            Color hair = new Color(.08f, .07f, .10f);
            Fill(t, 9, 5, 14, 3, hair); Fill(t, 7, 8, 18, 10, hair);
            Fill(t, 10, 8, 12, 10, skin); Fill(t, 8, 12, 16, 7, skin);
            Fill(t, 10, 18, 12, 3, mid); Fill(t, 12, 20, 8, 2, dark);
            Pixel(t, 12, 12, Color.black); Pixel(t, 19, 12, Color.black);
            Fill(t, 14, 16, 5, 1, new Color(.45f, .12f, .12f));
            Fill(t, 6, 10, 2, 6, light); Fill(t, 24, 10, 2, 6, light);
        }

        private static void DrawHostageCage(Texture2D t, Color mid, Color dark, Color light)
        {
            // Jaula simples em primeiro plano: barras verdes luminosas e contorno quase preto.
            // O centro fica transparente para que o aluno continue legível por baixo.
            Color frame = new Color(.035f, .09f, .045f);
            Color bars = new Color(.20f, .78f, .34f);
            Color glow = new Color(.47f, .96f, .40f);
            Fill(t, 5, 5, 22, 2, frame);
            Fill(t, 5, 25, 22, 2, frame);
            Fill(t, 5, 7, 2, 18, frame);
            Fill(t, 25, 7, 2, 18, frame);
            Fill(t, 7, 7, 1, 18, bars);
            Fill(t, 12, 7, 1, 18, bars);
            Fill(t, 18, 7, 1, 18, bars);
            Fill(t, 23, 7, 1, 18, bars);
            Fill(t, 6, 7, 20, 1, glow);
            Fill(t, 6, 24, 20, 1, bars);
            Pixel(t, 8, 6, glow); Pixel(t, 23, 6, glow);
            Pixel(t, 8, 26, bars); Pixel(t, 23, 26, bars);
        }

        private static void DrawETAttack(Texture2D t, Color mid, Color dark, Color light)
        {
            // Rajada curta de energia esmeralda usada pelo subordinado contra Edelzio.
            Color outline = new Color(.04f, .12f, .05f);
            Color energy = new Color(.24f, .85f, .28f);
            Color hot = new Color(.72f, 1f, .42f);
            Fill(t, 14, 5, 4, 4, outline);
            Fill(t, 11, 9, 10, 14, outline);
            Fill(t, 8, 13, 16, 6, outline);
            Fill(t, 13, 8, 6, 16, energy);
            Fill(t, 10, 14, 12, 4, energy);
            Fill(t, 14, 10, 4, 12, hot);
            Fill(t, 12, 15, 8, 2, hot);
            Pixel(t, 15, 6, hot); Pixel(t, 16, 23, energy);
        }

        private static void DrawStudentAttack(Texture2D t, Color mid, Color dark, Color light, string id)
        {
            Color outline = new Color(.06f, .04f, .08f);
            Color spark = Color.Lerp(mid, Color.white, .45f);
            bool katana = id.Contains("Katana");
            bool piano = id.Contains("FallingPiano");
            bool paddle = id.Contains("PingPong");
            bool guitar = id.Contains("Guitar");
            bool microphone = id.Contains("Microphone");
            bool art = id.Contains("Art");
            bool grab = id.Contains("JiuJitsu");

            if (katana)
            {
                // Katana horizontal com lâmina clara e arco de corte; o objeto é
                // girado pelo aliado para acompanhar a direção do alvo.
                Fill(t, 3, 13, 6, 8, outline);
                Fill(t, 4, 15, 4, 5, mid);
                Fill(t, 8, 12, 3, 10, outline);
                Fill(t, 9, 13, 2, 8, spark);
                Fill(t, 11, 14, 18, 5, outline);
                Fill(t, 12, 15, 16, 2, spark);
                Fill(t, 27, 14, 3, 3, light);
                Pixel(t, 13, 12, spark); Pixel(t, 17, 10, spark); Pixel(t, 21, 8, spark);
                Pixel(t, 25, 6, spark); Pixel(t, 28, 5, spark);
                return;
            }
            if (piano)
            {
                // Piano compacto, com teclado visível e pernas; ele nasce acima
                // do ET e desce até o ponto de impacto.
                Color pianoBlack = new Color(.015f, .018f, .025f);
                Color pianoBody = new Color(.10f, .12f, .18f);
                Color pianoEdge = new Color(.32f, .36f, .46f);
                Color key = new Color(.92f, .92f, .82f);
                Fill(t, 4, 6, 24, 16, pianoBlack);
                Fill(t, 6, 8, 20, 12, pianoBody);
                Fill(t, 7, 9, 18, 3, pianoEdge);
                Fill(t, 7, 13, 18, 6, key);
                Fill(t, 8, 13, 2, 6, pianoBlack); Fill(t, 12, 13, 2, 6, pianoBlack);
                Fill(t, 16, 13, 2, 6, pianoBlack); Fill(t, 20, 13, 2, 6, pianoBlack);
                Fill(t, 8, 20, 3, 8, pianoBlack); Fill(t, 21, 20, 3, 8, pianoBlack);
                Fill(t, 7, 20, 18, 2, pianoEdge);
                return;
            }
            if (paddle)
            {
                Fill(t, 12, 4, 8, 15, outline); Fill(t, 14, 6, 4, 12, mid);
                Fill(t, 9, 17, 14, 9, outline); Fill(t, 11, 18, 10, 7, mid);
                Fill(t, 24, 8, 4, 4, spark); Pixel(t, 26, 6, spark);
                return;
            }
            if (guitar)
            {
                Fill(t, 7, 11, 18, 13, outline); Fill(t, 10, 13, 12, 9, mid);
                Fill(t, 20, 6, 4, 10, outline); Fill(t, 22, 4, 2, 10, spark);
                Fill(t, 12, 15, 8, 2, spark); Pixel(t, 14, 19, outline);
                return;
            }
            if (microphone)
            {
                Fill(t, 10, 5, 12, 12, outline); Fill(t, 12, 7, 8, 8, spark);
                Fill(t, 14, 16, 4, 11, outline); Fill(t, 8, 27, 16, 2, outline);
                Fill(t, 9, 28, 14, 1, mid);
                return;
            }
            if (art)
            {
                Fill(t, 6, 21, 20, 4, outline); Fill(t, 9, 18, 14, 3, mid);
                Fill(t, 12, 10, 4, 10, spark); Fill(t, 16, 7, 4, 10, mid);
                Pixel(t, 22, 5, spark); Pixel(t, 25, 8, spark); Pixel(t, 7, 14, mid);
                return;
            }
            if (grab)
            {
                Fill(t, 5, 10, 22, 4, outline); Fill(t, 8, 14, 16, 5, mid);
                Fill(t, 10, 19, 5, 8, outline); Fill(t, 17, 19, 5, 8, spark);
                Pixel(t, 6, 8, spark); Pixel(t, 25, 8, spark);
                return;
            }

            // Tapão/empurrão: clarão em estrela para os perfis restantes.
            Fill(t, 14, 3, 4, 26, outline); Fill(t, 3, 14, 26, 4, outline);
            Fill(t, 15, 5, 2, 22, spark); Fill(t, 5, 15, 22, 2, spark);
            Pixel(t, 8, 8, mid); Pixel(t, 23, 8, mid); Pixel(t, 8, 23, mid); Pixel(t, 23, 23, mid);
        }

        private static void DrawETSubordinate(Texture2D t, Color mid, Color dark, Color light)
        {
            // ET inspirado no desenho de Varginha: crânio comprido, pele ocre,
            // olhos vermelhos brilhantes e corpo estreito com braços longos.
            Color outline = new Color(.045f, .025f, .025f);
            Color shadow = new Color(.19f, .085f, .045f);
            Color skin = new Color(.62f, .34f, .17f);
            Color skinLight = new Color(.86f, .61f, .32f);
            Color eyeDark = new Color(.025f, .012f, .014f);
            Color eyeRed = new Color(.90f, .10f, .055f);
            Color eyeHot = new Color(1f, .36f, .16f);
            Color mouth = new Color(.20f, .025f, .025f);

            // Corpo e pescoço ficam atrás da cabeça para formar a silhueta fina.
            Fill(t, 12, 24, 8, 5, outline);
            Fill(t, 13, 24, 6, 5, shadow);
            Fill(t, 9, 26, 4, 2, outline); Fill(t, 19, 26, 4, 2, outline);
            Fill(t, 10, 27, 4, 2, skin); Fill(t, 18, 27, 4, 2, skin);
            Fill(t, 5, 22, 7, 2, outline); Fill(t, 20, 22, 7, 2, outline);
            Fill(t, 6, 21, 7, 2, skin); Fill(t, 19, 21, 7, 2, skin);

            // Crânio alongado com contorno em degraus de pixel art.
            Fill(t, 13, 1, 6, 2, outline);
            Fill(t, 10, 3, 12, 3, outline);
            Fill(t, 8, 6, 16, 5, outline);
            Fill(t, 6, 10, 20, 12, outline);
            Fill(t, 8, 22, 16, 5, outline);
            Fill(t, 11, 26, 10, 2, outline);
            Fill(t, 13, 3, 6, 2, shadow);
            Fill(t, 11, 6, 10, 4, skin);
            Fill(t, 9, 10, 14, 11, skin);
            Fill(t, 10, 20, 12, 5, shadow);
            Fill(t, 12, 6, 4, 2, skinLight); Fill(t, 9, 11, 3, 4, skinLight);
            Pixel(t, 20, 9, skinLight); Pixel(t, 22, 11, skinLight);

            // Olhos grandes e oblíquos, com reflexo vivo.
            Fill(t, 7, 13, 7, 5, eyeDark); Fill(t, 18, 13, 7, 5, eyeDark);
            Fill(t, 9, 14, 4, 3, eyeRed); Fill(t, 19, 14, 4, 3, eyeRed);
            Pixel(t, 10, 14, eyeHot); Pixel(t, 20, 14, eyeHot);
            Pixel(t, 12, 17, shadow); Pixel(t, 19, 17, shadow);

            // Nariz curto, rugas e sorriso escuro do desenho de referência.
            Fill(t, 15, 18, 2, 3, shadow); Pixel(t, 14, 20, skinLight);
            Fill(t, 11, 21, 10, 3, mouth); Pixel(t, 14, 21, skinLight); Pixel(t, 18, 21, skinLight);
            Pixel(t, 8, 19, shadow); Pixel(t, 23, 19, shadow);
            Pixel(t, 12, 8, shadow); Pixel(t, 21, 7, shadow); Pixel(t, 8, 16, skinLight);
        }

        private static void DrawCrate(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 3, 3, 26, 26, dark); Fill(t, 5, 5, 22, 22, mid); Fill(t, 7, 7, 18, 3, light); Fill(t, 14, 5, 4, 22, dark);
        }

        private static void Pixel(Texture2D t, int x, int y, Color c)
        {
            int pixelX = x * DetailScale;
            int pixelY = y * DetailScale;
            for (int yOffset = 0; yOffset < DetailScale; yOffset++)
            for (int xOffset = 0; xOffset < DetailScale; xOffset++)
            {
                int targetX = pixelX + xOffset;
                int targetY = pixelY + yOffset;
                if (targetX >= 0 && targetX < CanvasSize && targetY >= 0 && targetY < CanvasSize)
                    t.SetPixel(targetX, targetY, c);
            }
        }

        private static void Fill(Texture2D t, int x, int y, int width, int height, Color c)
        {
            for (int iy = y; iy < y + height; iy++)
            for (int ix = x; ix < x + width; ix++) Pixel(t, ix, iy, c);
        }

        private static void FillRaw(Texture2D t, int x, int y, int width, int height, Color c)
        {
            for (int iy = y; iy < y + height; iy++)
            for (int ix = x; ix < x + width; ix++)
                if (ix >= 0 && ix < t.width && iy >= 0 && iy < t.height) t.SetPixel(ix, iy, c);
        }

        private static void AddMicroDetails(Texture2D t, string id, Color dark, Color light)
        {
            int seed = 17;
            for (int i = 0; i < id.Length; i++) seed = seed * 31 + id[i];

            int count = id.StartsWith("Floor") || id.StartsWith("Wall") || id.StartsWith("Street") || id.StartsWith("Driveway") ? 16 : 6;
            for (int i = 0; i < count; i++)
            {
                int x = 3 + Mathf.Abs(seed + i * 17) % 58;
                int y = 3 + Mathf.Abs(seed / 7 + i * 23) % 58;
                Color baseColor = t.GetPixel(x, y);
                if (baseColor.a < .5f) continue;

                // Pixels individuais dão desgaste, reflexos e granulação sem borrar a silhueta.
                t.SetPixel(x, y, Color.Lerp(baseColor, (i & 1) == 0 ? light : dark, .25f));
            }
        }
    }
}
