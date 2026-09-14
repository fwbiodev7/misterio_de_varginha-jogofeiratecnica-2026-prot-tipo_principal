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
            else if (id == "SchoolFloor") DrawSchoolFloor(texture, mid, dark, light);
            else if (id.StartsWith("ChurchFloor")) DrawChurchFloor(texture, mid, dark, light);
            else if (id == "Floor_Yard") DrawGrass(texture, mid, dark, light);
            else if (id.StartsWith("Street")) DrawStreet(texture, mid, dark, light);
            else if (id.StartsWith("Driveway")) DrawStone(texture, mid, dark, light);
            else if (id.StartsWith("Wall")) DrawWall(texture, mid, dark, light);
            else if (id == "SchoolWall") DrawSchoolWall(texture, mid, dark, light);
            else if (id == "ChurchWall") DrawChurchWall(texture, mid, dark, light);
            else if (id.StartsWith("SchoolBlackboard")) DrawSchoolBlackboard(texture, mid, dark, light);
            else if (id.StartsWith("SchoolDesk")) DrawSchoolDesk(texture, mid, dark, light);
            else if (id.StartsWith("SchoolChair")) DrawSchoolChair(texture, mid, dark, light);
            else if (id.StartsWith("SchoolLocker")) DrawSchoolLocker(texture, mid, dark, light);
            else if (id.StartsWith("SchoolWindow")) DrawSchoolWindow(texture, mid, dark, light);
            else if (id.StartsWith("SchoolCurtain")) DrawSchoolCurtain(texture, mid, dark, light);
            else if (id.StartsWith("SchoolPoster")) DrawSchoolPoster(texture, mid, dark, light);
            else if (id.StartsWith("SchoolClock")) DrawSchoolClock(texture, mid, dark, light);
            else if (id.StartsWith("SchoolExitSign")) DrawSchoolExitSign(texture, mid, dark, light);
            else if (id.StartsWith("FuscaParking")) DrawFuscaParking(texture, mid, dark, light);
            else if (id.StartsWith("ChurchWindow")) DrawChurchWindow(texture, mid, dark, light, id.Contains("Top"));
            else if (id.StartsWith("ChurchPew")) DrawChurchPew(texture, mid, dark, light);
            else if (id.StartsWith("ChurchRug")) DrawChurchRug(texture, mid, dark, light);
            else if (id.StartsWith("ChurchDais")) DrawChurchDais(texture, mid, dark, light);
            else if (id.StartsWith("ChurchCandle")) DrawChurchCandle(texture, mid, dark, light);
            else if (id.StartsWith("ChurchLectern")) DrawChurchLectern(texture, mid, dark, light);
            else if (id.StartsWith("ChurchDoor")) DrawChurchDoor(texture, mid, dark, light);
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
            else if (id == "Attack_Slash" || id == "Attack_HeavySlash") DrawAttackSlash(texture, mid, dark, light, id.Contains("Heavy"));
            else if (id == "Attack_CrossSlash") DrawCrossSlash(texture, mid, dark, light);
            else if (id == "Attack_Impact") DrawAttackImpact(texture, mid, dark, light);
            else if (id == "Dodge_Dust") DrawDodgeDust(texture, mid, dark, light);
            else if (id.StartsWith("HostageCage")) DrawHostageCage(texture, mid, dark, light);
            else if (id == "ET_Attack") DrawETAttack(texture, mid, dark, light);
            else if (id == "ET_Shockwave") DrawShockwave(texture, mid, false);
            else if (id.StartsWith("StudentAttackTrail_")) DrawStudentTrail(texture, mid, id);
            else if (id.StartsWith("StudentImpact_")) DrawStudentImpact(texture, mid, id);
            else if (id.StartsWith("StudentAttack_")) DrawStudentAttack(texture, mid, dark, light, id);
            else if (id.StartsWith("StudentHead")) DrawStudentHead(texture, mid, dark, light, id);
            else if (id.StartsWith("Student")) DrawStudent(texture, mid, dark, light, id);
            else if (id.StartsWith("ET_Subordinate")) DrawETSubordinate(texture, mid, dark, light, id);
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
            texture.anisoLevel = 0;
            texture.Apply(false, false);
            var sprite = Sprite.Create(texture, new Rect(0, 0, CanvasSize, CanvasSize), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = "Pixel_" + id;
            Cache[key] = sprite;
            return sprite;
        }

        private static void DrawWoodFloor(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 0, 0, 32, 32, mid);
            for (int y = 0; y < 32; y += 8)
            {
                Fill(t, 0, y, 32, 1, Color.Lerp(mid, dark, .42f));
                int seam = ((y / 8) & 1) == 0 ? 9 : 25;
                Fill(t, seam, y + 1, 1, 7, Color.Lerp(mid, dark, .35f));
                Fill(t, 0, y + 1, 32, 1, Color.Lerp(mid, light, .16f));
                Fill(t, 3, y + 4, 16, 1, Color.Lerp(mid, dark, .10f));
                Fill(t, 19, y + 5, 10, 1, Color.Lerp(mid, light, .08f));
            }
        }

        private static void DrawSchoolFloor(Texture2D t, Color mid, Color dark, Color light)
        {
            // Woven parquet: four blocks with perpendicular wood grain, quiet
            // contrast so characters and floor lighting remain easy to read.
            Color plank = new Color(.52f, .46f, .35f);
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                bool vertical = ((x / 16 + y / 16) & 1) == 0;
                int cross = (vertical ? x : y) % 4, along = (vertical ? y : x) % 16;
                Color shade = cross == 0 || along == 0 ? Color.Lerp(plank, dark, .28f)
                    : cross == 1 ? Color.Lerp(plank, light, .18f) : plank;
                if ((x * 7 + y * 3) % 29 == 0) shade = Color.Lerp(shade, dark, .08f);
                Pixel(t, x, y, shade);
            }
        }

        private static void DrawSchoolWall(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 0, 0, 32, 32, new Color(.66f, .60f, .46f));
            Fill(t, 0, 0, 32, 10, new Color(.30f, .39f, .36f));
            for (int x = 1; x < 32; x += 8) Fill(t, x, 1, 1, 8, new Color(.23f, .31f, .30f));
            Fill(t, 0, 10, 32, 2, new Color(.76f, .64f, .42f));
            Fill(t, 0, 12, 32, 1, new Color(.40f, .35f, .29f));
            Fill(t, 0, 28, 32, 2, new Color(.84f, .77f, .59f));
            Fill(t, 0, 30, 32, 2, new Color(.25f, .28f, .29f));
        }

        private static void DrawChurchFloor(Texture2D t, Color mid, Color dark, Color light)
        {
            Color stone = new Color(.225f, .235f, .255f);
            Color seam = new Color(.125f, .14f, .17f);
            Fill(t, 0, 0, 32, 32, stone);
            // Large dressed stone slabs replace the wall-like brick pattern.
            for (int by = 0; by < 2; by++) for (int bx = 0; bx < 2; bx++)
            {
                int x = bx * 16, y = by * 16;
                Fill(t, x, y, 16, 1, seam); Fill(t, x, y, 1, 16, seam);
                Fill(t, x + 1, y + 1, 14, 1, new Color(.285f, .29f, .30f));
                Fill(t, x + 1, y + 2, 1, 13, new Color(.26f, .27f, .29f));
                Fill(t, x + 4, y + 11, 4, 1, new Color(.245f, .255f, .275f));
                Pixel(t, x + 11, y + 5, new Color(.20f, .215f, .235f));
            }
        }

        private static void DrawChurchWall(Texture2D t, Color mid, Color dark, Color light)
        {
            Color stone = Color.Lerp(mid, new Color(.19f, .20f, .24f), .35f);
            Color seam = new Color(.055f, .065f, .09f);
            Color edge = new Color(.30f, .27f, .28f);
            Fill(t, 0, 0, 32, 32, stone);
            for (int y = 2; y < 32; y += 8)
            {
                Fill(t, 0, y, 32, 1, seam);
                int offset = ((y / 8) & 1) == 0 ? 4 : 12;
                for (int x = offset; x < 32; x += 16) Fill(t, x, y + 1, 1, 7, seam);
            }
            Fill(t, 0, 28, 32, 2, edge);
            Fill(t, 0, 30, 32, 2, new Color(.035f, .045f, .065f));
        }

        private static void DrawSchoolBlackboard(Texture2D t, Color mid, Color dark, Color light)
        {
            Color frame = new Color(.32f, .20f, .10f);
            Color board = new Color(.08f, .20f, .19f);
            Fill(t, 2, 3, 28, 26, frame); Fill(t, 4, 5, 24, 22, board);
            Fill(t, 6, 9, 12, 1, new Color(.75f, .82f, .72f));
            Fill(t, 7, 15, 18, 1, new Color(.60f, .70f, .64f));
            Fill(t, 10, 20, 10, 1, new Color(.84f, .74f, .48f));
            Fill(t, 5, 26, 22, 2, Color.Lerp(frame, dark, .25f));
        }

        private static void DrawSchoolDesk(Texture2D t, Color mid, Color dark, Color light)
        {
            Color wood = Color.Lerp(mid, new Color(.42f, .22f, .11f), .45f);
            Fill(t, 3, 7, 26, 13, dark); Fill(t, 5, 8, 22, 10, wood);
            Fill(t, 7, 9, 18, 2, light); Fill(t, 6, 19, 4, 8, dark); Fill(t, 22, 19, 4, 8, dark);
            Fill(t, 12, 13, 8, 2, Color.Lerp(wood, dark, .3f));
        }

        private static void DrawSchoolChair(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 7, 5, 18, 11, dark); Fill(t, 9, 7, 14, 7, mid);
            Fill(t, 10, 16, 12, 4, dark); Fill(t, 11, 20, 3, 7, dark); Fill(t, 19, 20, 3, 7, dark);
            Fill(t, 11, 7, 10, 1, light);
        }

        private static void DrawSchoolLocker(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 4, 2, 24, 28, dark); Fill(t, 6, 4, 20, 24, mid);
            Fill(t, 7, 5, 18, 1, light); Fill(t, 15, 4, 2, 24, dark);
            Fill(t, 10, 12, 2, 3, light); Fill(t, 20, 12, 2, 3, light);
            Fill(t, 7, 26, 18, 2, Color.Lerp(dark, Color.black, .25f));
        }

        private static void DrawSchoolWindow(Texture2D t, Color mid, Color dark, Color light)
        {
            Color frame = new Color(.23f, .27f, .29f);
            Color sky = new Color(.22f, .52f, .68f);
            Fill(t, 3, 3, 26, 26, frame); Fill(t, 6, 6, 20, 20, sky);
            Fill(t, 15, 6, 2, 20, frame); Fill(t, 6, 15, 20, 2, frame);
            Fill(t, 8, 8, 5, 3, light); Fill(t, 19, 18, 5, 3, Color.Lerp(sky, Color.white, .28f));
        }

        private static void DrawSchoolCurtain(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 6, 3, 18, 25, dark); Fill(t, 8, 5, 14, 20, mid);
            for (int x = 9; x < 22; x += 4) Fill(t, x, 6, 2, 18, light);
            Fill(t, 5, 26, 22, 3, dark);
        }

        private static void DrawSchoolPoster(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 5, 3, 22, 26, dark); Fill(t, 7, 5, 18, 22, new Color(.88f, .78f, .54f));
            Fill(t, 9, 7, 14, 5, mid); Fill(t, 9, 15, 6, 8, light); Fill(t, 17, 15, 6, 8, new Color(.38f, .64f, .55f));
            Fill(t, 9, 25, 14, 1, dark);
        }

        private static void DrawSchoolClock(Texture2D t, Color mid, Color dark, Color light)
        {
            Ellipse(t, 16, 16, 12, 12, dark); Ellipse(t, 16, 16, 9, 9, mid);
            Fill(t, 15, 8, 2, 8, dark); Fill(t, 16, 15, 6, 2, dark); Pixel(t, 15, 15, light);
        }

        private static void DrawSchoolExitSign(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 3, 8, 26, 16, dark); Fill(t, 5, 10, 22, 12, new Color(.16f, .56f, .28f));
            Fill(t, 8, 13, 6, 2, Color.white); Fill(t, 17, 13, 7, 2, Color.white);
            Fill(t, 13, 11, 2, 9, Color.white);
        }

        private static void DrawFuscaParking(Texture2D t, Color mid, Color dark, Color light)
        {
            Color asphalt = new Color(.09f, .13f, .16f);
            Color edge = new Color(.82f, .61f, .23f);
            Color reflection = new Color(.22f, .38f, .42f);
            Fill(t, 1, 4, 62, 56, dark);
            Fill(t, 4, 7, 56, 50, asphalt);
            Fill(t, 5, 8, 54, 2, edge);
            Fill(t, 5, 54, 54, 2, edge);
            Fill(t, 5, 9, 2, 45, edge);
            Fill(t, 57, 9, 2, 45, edge);
            for (int x = 10; x < 55; x += 12) Fill(t, x, 12, 5, 1, reflection);
            Fill(t, 30, 17, 4, 30, Color.Lerp(edge, asphalt, .32f));
            Fill(t, 14, 29, 36, 2, Color.Lerp(light, edge, .45f));
        }

        private static void DrawChurchWindow(Texture2D t, Color mid, Color dark, Color light, bool top)
        {
            Color frame = new Color(.08f, .08f, .12f);
            Color glassA = new Color(.35f, .20f, .55f);
            Color glassB = new Color(.82f, .34f, .30f);
            Color glassC = new Color(.20f, .55f, .65f);
            Fill(t, 3, 2, 26, 28, frame); Fill(t, 6, 5, 20, 22, glassA);
            Fill(t, 8, 6, 5, 9, glassB); Fill(t, 14, 6, 5, 9, glassC); Fill(t, 20, 6, 4, 9, new Color(.78f, .62f, .26f));
            Fill(t, 8, 17, 5, 8, glassC); Fill(t, 14, 17, 5, 8, glassB); Fill(t, 20, 17, 4, 8, glassA);
            Fill(t, 14, 4, 4, 24, frame); Fill(t, 6, 15, 20, 2, frame);
            Fill(t, 8, 6, 4, 2, light); Fill(t, 20, 19, 3, 2, light);
            if (top) Fill(t, 4, 28, 24, 2, new Color(.34f, .24f, .16f));
        }

        private static void DrawChurchPew(Texture2D t, Color mid, Color dark, Color light)
        {
            Color wood = new Color(.27f, .13f, .08f);
            Fill(t, 2, 8, 28, 9, dark); Fill(t, 4, 9, 24, 6, wood); Fill(t, 5, 10, 22, 2, light);
            Fill(t, 5, 18, 4, 10, dark); Fill(t, 23, 18, 4, 10, dark);
            Fill(t, 11, 13, 10, 1, Color.Lerp(wood, Color.black, .3f));
        }

        private static void DrawChurchRug(Texture2D t, Color mid, Color dark, Color light)
        {
            Color red = new Color(.36f, .08f, .12f);
            Color gold = new Color(.67f, .40f, .17f);
            Fill(t, 2, 5, 28, 22, dark); Fill(t, 4, 7, 24, 18, red);
            Fill(t, 5, 8, 22, 2, gold); Fill(t, 5, 22, 22, 2, gold);
            for (int x = 8; x < 25; x += 6) Fill(t, x, 13, 2, 6, gold);
        }

        private static void DrawChurchDais(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 3, 6, 26, 22, dark); Fill(t, 5, 8, 22, 17, new Color(.35f, .30f, .29f));
            Fill(t, 7, 10, 18, 2, light); Fill(t, 7, 22, 18, 2, dark); Fill(t, 14, 12, 4, 10, new Color(.18f, .16f, .17f));
        }

        private static void DrawChurchCandle(Texture2D t, Color mid, Color dark, Color light)
        {
            Color flame = new Color(1f, .72f, .25f);
            Fill(t, 14, 10, 4, 16, dark); Fill(t, 11, 25, 10, 3, mid); Fill(t, 15, 5, 2, 6, flame);
            Pixel(t, 15, 4, Color.white); Pixel(t, 16, 4, flame); Fill(t, 10, 28, 12, 2, dark);
        }

        private static void DrawChurchLectern(Texture2D t, Color mid, Color dark, Color light)
        {
            Color wood = new Color(.32f, .17f, .10f);
            Fill(t, 7, 5, 18, 7, dark); Fill(t, 9, 7, 14, 4, wood); Fill(t, 14, 12, 4, 14, dark);
            Fill(t, 8, 26, 16, 3, wood); Fill(t, 11, 8, 10, 1, light);
        }

        private static void DrawChurchDoor(Texture2D t, Color mid, Color dark, Color light)
        {
            Fill(t, 4, 4, 24, 25, dark); Fill(t, 7, 7, 18, 22, new Color(.22f, .12f, .13f));
            Fill(t, 15, 8, 2, 20, dark); Fill(t, 10, 16, 2, 2, light); Fill(t, 20, 16, 2, 2, light);
            Fill(t, 7, 6, 18, 2, Color.Lerp(light, dark, .25f));
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
            for (int i = 0; i < 7; i++)
            {
                int x = (i * 13 + 3) % 30, y = (i * 17 + 7) % 30;
                Pixel(t, x, y, Color.Lerp(mid, light, .16f));
                Pixel(t, x + 1, y + 1, Color.Lerp(mid, dark, .16f));
                Pixel(t, x + 2, y, Color.Lerp(mid, light, .11f));
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
                Fill(t, x + 1, y + 4, 5, 1, Color.Lerp(mid, light, .22f));
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
            // Folha compacta em pixel art: contorno, janela, chapa, friso e
            // maçaneta cabem dentro da proporção real da porta do Fusca.
            Fill(t, 7, 7, 18, 22, dark);
            Fill(t, 8, 8, 16, 20, mid);
            Fill(t, 9, 9, 14, 7, dark);
            Fill(t, 10, 10, 12, 5, glass);
            Fill(t, 11, 11, 6, 2, glassLight);
            Fill(t, 18, 11, 3, 1, Color.Lerp(glass, glassLight, .35f));
            Fill(t, 9, 16, 14, 2, Color.Lerp(mid, light, .35f));
            Fill(t, 10, 19, 12, 7, mid);
            Fill(t, 11, 21, 10, 1, light);
            Fill(t, 19, 18, 3, 2, dark); Fill(t, 20, 18, 2, 1, glassLight);
            Fill(t, 10, 26, 3, 2, dark); Fill(t, 19, 26, 3, 2, dark);
            if (open) Fill(t, 5, 9, 3, 16, Color.Lerp(dark, Color.black, .4f));
            else if (ajar)
            {
                // Quadro intermediário: o vão aparece, mas a folha ainda não
                // atravessa a carroceria como acontecia na troca instantânea.
                Fill(t, 7, 9, 2, 16, Color.Lerp(dark, Color.black, .22f));
                Fill(t, 8, 11, 1, 11, light);
            }
        }

        private static void DrawAttackSlash(Texture2D t, Color mid, Color dark, Color light, bool heavy)
        {
            // Crescente completo dentro da grade: a versão antiga ultrapassava 32px e cortava o arco.
            for (int y = 3; y < 29; y++)
            for (int x = 8; x < 31; x++)
            {
                float dx = x - 15.5f, dy = y - 15.5f;
                float radius = Mathf.Sqrt(dx * dx + dy * dy);
                float inner = heavy ? 8.3f : 10.2f;
                if (radius > 14f || radius < inner || dx < Mathf.Abs(dy) * .30f) continue;
                Pixel(t, x, y, radius > 12.9f ? Color.white : radius > 11.3f ? light : mid);
            }
            Fill(t, 10, 5, 5, 1, light); Fill(t, 11, 26, 5, 1, light);
            if (heavy)
            {
                Fill(t, 4, 15, 6, 2, light); Fill(t, 7, 9, 4, 1, mid);
                Fill(t, 6, 22, 4, 1, mid); Pixel(t, 29, 4, Color.white); Pixel(t, 29, 27, Color.white);
            }
        }

        private static void DrawAttackImpact(Texture2D t, Color mid, Color dark, Color light)
        {
            for (int y = 3; y < 29; y++)
            for (int x = 3; x < 29; x++)
            {
                int dx = Mathf.Abs(x - 16), dy = Mathf.Abs(y - 16);
                if (dx + dy < 10 || (dx < 2 && dy < 14) || (dy < 2 && dx < 14))
                    Pixel(t, x, y, dx + dy < 7 ? Color.white : light);
            }
            Fill(t, 5, 5, 3, 3, mid); Fill(t, 24, 24, 3, 3, mid);
            Fill(t, 5, 25, 2, 2, light); Fill(t, 25, 5, 2, 2, light);
        }

        private static void DrawCrossSlash(Texture2D t, Color mid, Color dark, Color light)
        {
            // Segundo golpe do combo: duas lâminas cruzadas, visualmente distinto
            // do arco inicial e menor que o finalizador pesado.
            for (int i = 0; i < 24; i++)
            {
                int a = 5 + i;
                int b = 27 - i;
                Fill(t, a, 7 + Mathf.Clamp(i / 5, 0, 4), 2, 2, i % 4 == 0 ? Color.white : light);
                Fill(t, b, 23 - Mathf.Clamp(i / 5, 0, 4), 2, 2, i % 4 == 1 ? Color.white : mid);
            }
            Fill(t, 13, 14, 6, 4, dark);
            Fill(t, 14, 15, 4, 2, Color.white);
        }

        private static void DrawDodgeDust(Texture2D t, Color mid, Color dark, Color light)
        {
            Color dust = new Color(.73f, .77f, .80f, .60f);
            Ellipse(t, 9, 12, 6, 3, dust); Ellipse(t, 19, 17, 7, 4, dust);
            Ellipse(t, 25, 23, 4, 3, new Color(.87f, .89f, .91f, .78f));
            Fill(t, 4, 11, 5, 1, light); Fill(t, 14, 17, 4, 1, light);
            Pixel(t, 6, 21, dust); Pixel(t, 11, 25, dust); Pixel(t, 28, 9, dust);
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

        private static void DrawStudent(Texture2D t, Color mid, Color dark, Color light, string id)
        {
            Color ink = new Color(.06f, .07f, .12f);
            Color denim = new Color(.17f, .22f, .34f);
            Ellipse(t, 16, 3, 11, 2, new Color(.025f, .035f, .07f, .42f));
            // Pés embaixo e cabeça acima, na mesma orientação do Edelzio.
            Fill(t, 10, 4, 5, 7, ink); Fill(t, 18, 4, 5, 7, ink);
            Fill(t, 11, 6, 3, 5, denim); Fill(t, 19, 6, 3, 5, denim);
            Fill(t, 10, 4, 5, 1, new Color(.72f, .77f, .82f));
            Fill(t, 18, 4, 5, 1, new Color(.72f, .77f, .82f));
            Fill(t, 8, 10, 17, 10, ink); Fill(t, 9, 11, 15, 8, mid);
            Fill(t, 9, 11, 3, 6, dark); Fill(t, 12, 17, 9, 2, light);
            Fill(t, 7, 11, 2, 6, ink); Fill(t, 24, 11, 2, 6, ink);
            Fill(t, 7, 10, 2, 3, new Color(.82f, .56f, .40f));
            Fill(t, 24, 10, 2, 3, new Color(.82f, .56f, .40f));
            Fill(t, 14, 10, 5, 1, light);
            if (id.Contains("Marcos"))
            {
                Fill(t, 10, 14, 13, 1, Color.white);
                Fill(t, 15, 11, 3, 3, Color.white); Pixel(t, 16, 12, mid);
            }
            else if (id.Contains("Matias"))
            {
                Fill(t, 13, 14, 2, 5, light); Fill(t, 16, 11, 2, 6, light);
                Fill(t, 9, 12, 15, 2, ink); Fill(t, 16, 9, 2, 5, ink);
            }
            else if (id.Contains("Yasmin"))
            {
                Fill(t, 10, 11, 13, 3, ink);
                for (int x = 11; x < 23; x += 3) Fill(t, x, 11, 2, 2, light);
            }
            else if (id.Contains("Luis"))
            {
                Fill(t, 11, 13, 3, 3, light); Pixel(t, 12, 14, mid);
            }
            DrawStudentFace(t, mid, id, 0);
        }

        private static void DrawStudentHead(Texture2D t, Color mid, Color dark, Color light, string id)
        {
            // Retrato da mesma pessoa, conservando corte de cabelo e acessórios.
            Fill(t, 7, 5, 19, 8, new Color(.06f, .07f, .12f));
            Fill(t, 9, 6, 15, 6, mid); Fill(t, 11, 10, 11, 2, light);
            Fill(t, 10, 6, 3, 3, dark);
            DrawStudentFace(t, mid, id, -6);
        }

        private static void DrawStudentFace(Texture2D t, Color shirt, string id, int dy)
        {
            Color ink = new Color(.065f, .045f, .085f);
            Color hair = new Color(.17f, .10f, .09f);
            Color hairLight = new Color(.32f, .21f, .15f);
            Color skin = new Color(.86f, .61f, .43f);
            bool longHair = id.Contains("Yasmin") || id.Contains("Anna") || id.Contains("Tavares");
            if (id.Contains("Pedro")) { hair = new Color(.25f, .12f, .06f); hairLight = new Color(.48f, .28f, .12f); }
            if (longHair) { Fill(t, 8, 17 + dy, 17, 11, ink); Fill(t, 9, 17 + dy, 15, 11, hair); }
            Fill(t, 10, 18 + dy, 13, 11, ink); Fill(t, 11, 19 + dy, 11, 9, skin);
            Fill(t, 10, 25 + dy, 13, 4, hair); Fill(t, 12, 29 + dy, 9, 1, ink);
            Fill(t, 11, 28 + dy, 8, 1, hairLight); Fill(t, 11, 24 + dy, 2, 3, hair);
            Fill(t, 19, 25 + dy, 3, 2, hair); Fill(t, 21, 20 + dy, 1, 5, new Color(.66f, .39f, .28f));
            Pixel(t, 13, 23 + dy, ink); Pixel(t, 19, 23 + dy, ink);
            Pixel(t, 12, 24 + dy, Color.Lerp(skin, Color.white, .35f));
            Fill(t, 15, 20 + dy, 3, 1, new Color(.49f, .20f, .18f));
            if (id.Contains("Anna"))
            {
                Fill(t, 23, 19 + dy, 3, 7, hair); Fill(t, 23, 25 + dy, 3, 2, shirt);
                Pixel(t, 25, 19 + dy, hairLight);
            }
            else if (id.Contains("Tavares"))
            {
                Fill(t, 9, 26 + dy, 14, 1, shirt); Fill(t, 23, 24 + dy, 3, 3, shirt);
            }
            else if (id.Contains("Yasmin"))
            {
                Fill(t, 9, 18 + dy, 2, 7, hairLight); Fill(t, 20, 26 + dy, 3, 2, shirt);
            }
            else if (id.Contains("Marcos"))
            {
                Fill(t, 10, 26 + dy, 13, 2, new Color(.94f, .96f, .91f));
                Fill(t, 21, 26 + dy, 2, 2, shirt);
            }
            else if (id.Contains("Fabio"))
            {
                Fill(t, 10, 26 + dy, 13, 1, new Color(.79f, .12f, .17f));
                Fill(t, 23, 22 + dy, 3, 1, new Color(.79f, .12f, .17f));
            }
            else if (id.Contains("Martins"))
            {
                Fill(t, 9, 27 + dy, 15, 3, new Color(.25f, .31f, .46f));
                Fill(t, 10, 29 + dy, 10, 1, new Color(.44f, .54f, .65f));
                Fill(t, 23, 19 + dy, 1, 5, new Color(.91f, .65f, .24f));
            }
            else if (id.Contains("Messias"))
            {
                Fill(t, 9, 21 + dy, 2, 5, shirt); Fill(t, 22, 21 + dy, 2, 5, shirt);
                Fill(t, 11, 23 + dy, 5, 2, ink); Fill(t, 17, 23 + dy, 5, 2, ink);
                Pixel(t, 16, 24 + dy, ink); Pixel(t, 12, 24 + dy, new Color(.52f, .79f, .90f));
            }
            else if (id.Contains("Matias"))
            {
                Fill(t, 11, 28 + dy, 11, 1, hairLight); Fill(t, 12, 29 + dy, 3, 1, hair);
            }
            else if (id.Contains("Pedro"))
            {
                Fill(t, 10, 26 + dy, 3, 4, hair); Fill(t, 13, 29 + dy, 3, 2, hair);
                Fill(t, 17, 28 + dy, 3, 2, hair); Pixel(t, 22, 21 + dy, new Color(.88f, .72f, .29f));
            }
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
            // Projétil apontado à direita: a rotação do combate acompanha a trajetória.
            Color outline = new Color(.035f, .13f, .15f);
            Color energy = new Color(.18f, .89f, .72f);
            Color hot = new Color(.75f, 1f, .85f);
            for (int y = 9; y < 24; y++)
            for (int x = 7; x < 30; x++)
            {
                int dy = Mathf.Abs(y - 16);
                if (dy > 6 || x + dy > 30 || x - dy < 7) continue;
                Pixel(t, x, y, dy > 4 || x + dy > 28 ? outline : dy > 2 ? energy : hot);
            }
            Fill(t, 2, 15, 9, 2, energy); Fill(t, 6, 20, 7, 1, energy);
            Fill(t, 4, 10, 6, 1, energy); Fill(t, 20, 15, 7, 2, Color.white);
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

            if (id.Contains("MarcosChute"))
            {
                DrawMarcosChute(t, outline, spark);
                return;
            }
            if (id.Contains("MarcosCotovelo"))
            {
                DrawMarcosCotovelo(t, outline, spark);
                return;
            }
            if (id.Contains("Volleyball"))
            {
                DrawVolleyball(t);
                return;
            }

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

        private static void DrawVolleyball(Texture2D t)
        {
            Color ink = new Color(.065f, .11f, .22f);
            Color blue = new Color(.12f, .35f, .78f);
            Color gold = new Color(1f, .74f, .13f);
            Ellipse(t, 16, 16, 12, 12, ink);
            Ellipse(t, 16, 16, 11, 11, new Color(.94f, .96f, .88f));
            // Painéis curvos azul/amarelo com costuras, legíveis durante o giro da bola.
            for (int y = 6; y <= 26; y++)
            for (int x = 6; x <= 26; x++)
            {
                float dx = x - 16, dy = y - 16;
                if (dx * dx + dy * dy > 107f) continue;
                float bend = x + (y - 16) * (y - 16) * .055f;
                if (bend > 17 && bend < 22) Pixel(t, x, y, blue);
                else if (bend >= 22) Pixel(t, x, y, gold);
                else if (y < 14 && x < 17) Pixel(t, x, y, gold);
                if ((y == 14 && x < 17) || (Mathf.Abs(bend - 17) < .7f && y < 24)) Pixel(t, x, y, ink);
            }
            Fill(t, 10, 22, 4, 2, Color.white); Pixel(t, 9, 21, Color.white);
            Fill(t, 18, 7, 4, 1, new Color(.69f, .52f, .10f));
        }

        private static void DrawMarcosChute(Texture2D t, Color outline, Color spark)
        {
            // Silhueta inclinada com a perna estendida, diferente da bola e da
            // cotovelada, mas ainda legível em uma escala pequena.
            Fill(t, 9, 10, 12, 9, outline);
            Fill(t, 11, 11, 8, 6, spark);
            Fill(t, 18, 15, 12, 4, outline);
            Fill(t, 24, 16, 7, 3, spark);
            Fill(t, 5, 19, 10, 4, outline);
            Fill(t, 7, 20, 7, 2, spark);
            Fill(t, 12, 5, 5, 5, outline);
            Fill(t, 13, 4, 3, 3, Color.white);
            Pixel(t, 27, 12, spark); Pixel(t, 29, 9, Color.white);
        }

        private static void DrawMarcosCotovelo(Texture2D t, Color outline, Color spark)
        {
            Fill(t, 8, 9, 15, 13, outline);
            Fill(t, 10, 11, 11, 9, spark);
            Fill(t, 18, 8, 6, 8, outline);
            Fill(t, 21, 6, 8, 5, spark);
            Fill(t, 24, 9, 6, 4, Color.white);
            Fill(t, 9, 22, 12, 3, outline);
            Pixel(t, 5, 8, spark); Pixel(t, 4, 24, spark); Pixel(t, 28, 22, Color.white);
        }

        private static void DrawStudentTrail(Texture2D t, Color mid, string id)
        {
            Color hot = Color.Lerp(mid, Color.white, .55f);
            if (id.Contains("Guitar") || id.Contains("Microphone") || id.Contains("FallingPiano"))
            {
                // Notas musicais, sem replicar o instrumento inteiro em cada partícula.
                Fill(t, 12, 10, 3, 14, mid); Fill(t, 15, 21, 8, 3, hot); Fill(t, 22, 9, 2, 14, mid);
                Ellipse(t, 10, 10, 4, 3, hot); Ellipse(t, 20, 9, 4, 3, mid);
            }
            else if (id.Contains("Art"))
            {
                Ellipse(t, 16, 16, 7, 6, mid); Ellipse(t, 23, 21, 3, 4, new Color(1f, .37f, .65f));
                Fill(t, 9, 18, 4, 2, hot); Fill(t, 21, 7, 3, 3, new Color(.99f, .82f, .23f));
                Pixel(t, 6, 24, mid); Pixel(t, 25, 28, hot);
            }
            else if (id.Contains("Katana")) DrawAttackSlash(t, mid, mid, hot, false);
            else
            {
                Fill(t, 3, 13, 22, 3, mid); Fill(t, 9, 17, 20, 2, hot);
                Fill(t, 5, 21, 14, 1, mid); Fill(t, 12, 10, 15, 1, hot);
                Fill(t, 24, 13, 5, 5, hot);
                if (id.Contains("Volleyball")) { Fill(t, 2, 17, 5, 1, Color.white); Fill(t, 7, 7, 9, 2, new Color(1f, .8f, .2f)); }
            }
        }

        private static void DrawStudentImpact(Texture2D t, Color mid, string id)
        {
            if (id.Contains("Art"))
            {
                Ellipse(t, 16, 15, 11, 6, new Color(.15f, .10f, .23f, .85f));
                Ellipse(t, 15, 16, 10, 6, mid); Ellipse(t, 22, 20, 5, 4, new Color(.91f, .27f, .63f));
                Ellipse(t, 9, 11, 4, 3, new Color(1f, .77f, .21f));
                Fill(t, 10, 17, 7, 1, Color.Lerp(mid, Color.white, .65f));
                Fill(t, 4, 24, 3, 3, mid); Fill(t, 26, 10, 3, 3, new Color(.91f, .27f, .63f));
                Fill(t, 19, 4, 3, 2, new Color(1f, .77f, .21f));
            }
            else if (id.Contains("Guitar") || id.Contains("Microphone")) DrawShockwave(t, mid, true);
            else
            {
                DrawAttackImpact(t, mid, mid, Color.Lerp(mid, Color.white, .45f));
                Fill(t, 3, 12, 3, 1, new Color(.20f, .57f, 1f));
                Fill(t, 26, 21, 3, 1, new Color(1f, .77f, .15f));
            }
        }

        private static void DrawShockwave(Texture2D t, Color mid, bool musical)
        {
            Color hot = Color.Lerp(mid, Color.white, .6f);
            for (int y = 2; y < 30; y++)
            for (int x = 2; x < 30; x++)
            {
                float dx = x - 15.5f, dy = y - 15.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d >= 12 && d <= 14) Pixel(t, x, y, d > 13 ? hot : mid);
                else if (d >= 7 && d <= 8 && (x + y) % 4 < 3) Pixel(t, x, y, mid);
            }
            if (musical) { Fill(t, 15, 11, 2, 10, hot); Fill(t, 17, 18, 4, 2, hot); Ellipse(t, 13, 11, 3, 2, hot); }
            else { Fill(t, 14, 13, 4, 5, hot); Pixel(t, 9, 8, hot); Pixel(t, 23, 23, hot); }
        }

        private static void DrawETSubordinate(Texture2D t, Color mid, Color dark, Color light, string id)
        {
            bool sentinel = id.Contains("Sentinel");
            bool skirmisher = id.Contains("Skirmisher");
            Color outline = new Color(.065f, .035f, .09f);
            Color skin = new Color(.68f, .36f, .18f);
            Color shade = Color.Lerp(skin, outline, .50f);
            Color rim = Color.Lerp(skin, new Color(1f, .88f, .56f), .48f);
            Color energy = new Color(.95f, .08f, .045f);
            Ellipse(t, 16, 3, sentinel ? 13 : 10, 2, new Color(.035f, .025f, .08f, .5f));
            // O crânio de três saliências e os olhos de brasa mantêm a identidade de Varginha.
            Fill(t, 10, 4, 5, 9, outline); Fill(t, 18, 4, 5, 9, outline);
            Fill(t, 11, 5, 3, 7, shade); Fill(t, 19, 5, 3, 7, skin);
            Fill(t, 8, 4, 6, 2, outline); Fill(t, 19, 4, 6, 2, outline);
            Fill(t, 10, 10, 13, 8, outline); Fill(t, 11, 11, 11, 6, shade);
            Fill(t, 13, 13, 7, 3, skin); Fill(t, 15, 12, 3, 1, energy);
            Fill(t, 6, 8, 4, 9, outline); Fill(t, 23, 8, 4, 9, outline);
            Fill(t, 7, 9, 2, 7, skin); Fill(t, 24, 9, 2, 7, shade);
            Fill(t, 9, 18, 15, 9, outline); Fill(t, 11, 16, 11, 4, outline);
            Fill(t, 10, 20, 13, 6, skin); Fill(t, 11, 18, 11, 3, skin);
            Fill(t, 12, 17, 9, 2, shade);
            Fill(t, 10, 26, 4, 3, outline); Fill(t, 15, 27, 3, 4, outline); Fill(t, 20, 26, 4, 3, outline);
            Fill(t, 11, 26, 2, 2, skin); Fill(t, 16, 27, 1, 3, rim); Fill(t, 21, 26, 2, 2, skin);
            Fill(t, 11, 25, 4, 1, rim); Fill(t, 10, 21, 1, 3, rim);
            Fill(t, 10, 21, 6, 3, outline); Fill(t, 18, 21, 6, 3, outline);
            Fill(t, 11, 21, 4, 2, energy); Fill(t, 19, 21, 4, 2, energy);
            Pixel(t, 12, 22, new Color(1f, .91f, .66f)); Pixel(t, 20, 22, new Color(1f, .91f, .66f));
            Fill(t, 15, 19, 3, 1, shade); Fill(t, 14, 17, 5, 1, outline);
            if (sentinel)
            {
                // Ombreiras e manoplas fazem o tanque reconhecível mesmo sem depender da cor.
                Fill(t, 3, 11, 7, 8, outline); Fill(t, 23, 11, 7, 8, outline);
                Fill(t, 4, 13, 5, 5, shade); Fill(t, 24, 13, 5, 5, shade);
                Fill(t, 4, 17, 5, 1, rim); Fill(t, 24, 17, 5, 1, rim);
                Fill(t, 3, 7, 6, 5, outline); Fill(t, 24, 7, 6, 5, outline);
                Fill(t, 4, 8, 4, 3, skin); Fill(t, 25, 8, 4, 3, skin);
                Fill(t, 11, 13, 11, 2, outline); Fill(t, 15, 12, 3, 4, energy);
            }
            else if (skirmisher)
            {
                Fill(t, 7, 24, 2, 6, outline); Pixel(t, 7, 29, energy);
                Fill(t, 25, 24, 2, 6, outline); Pixel(t, 26, 29, energy);
                Fill(t, 24, 7, 5, 5, outline); Fill(t, 25, 8, 3, 3, energy);
                Pixel(t, 26, 10, Color.white); Fill(t, 15, 14, 3, 1, energy);
            }
            else
            {
                Fill(t, 4, 6, 6, 2, outline); Fill(t, 24, 6, 6, 2, outline);
                Pixel(t, 4, 5, rim); Pixel(t, 7, 5, rim); Pixel(t, 26, 5, rim); Pixel(t, 29, 5, rim);
                Fill(t, 10, 24, 5, 1, shade); Fill(t, 19, 24, 5, 1, shade);
                Fill(t, 13, 10, 2, 3, energy); Fill(t, 19, 10, 2, 3, energy);
            }
        }

        private static void Ellipse(Texture2D t, int centerX, int centerY, int radiusX, int radiusY, Color color)
        {
            for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
            for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
            {
                float dx = (x - centerX) / (float)radiusX, dy = (y - centerY) / (float)radiusY;
                if (dx * dx + dy * dy <= 1f) Pixel(t, x, y, color);
            }
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
            // Rostos, armas e efeitos usam grupos de pixels desenhados à mão. Ruído
            // aleatório ali apagava olhos/costuras e enfraquecia a silhueta em movimento.
            if (id.StartsWith("Student") || id.StartsWith("ET_") || id.StartsWith("Attack_")
                || id.StartsWith("Dodge_") || id.StartsWith("Edelzio") || id.StartsWith("Floor_")
                || id == "SchoolFloor" || id == "ChurchFloor") return;
            uint seed = 17;
            for (int i = 0; i < id.Length; i++) seed = unchecked(seed * 31u + id[i]);

            int count = id.StartsWith("Floor") || id.StartsWith("Wall") || id.StartsWith("Street") || id.StartsWith("Driveway") ? 16 : 6;
            for (int i = 0; i < count; i++)
            {
                int x = 3 + (int)(unchecked(seed + (uint)i * 17u) % 58u);
                int y = 3 + (int)(unchecked(seed / 7u + (uint)i * 23u) % 58u);
                Color baseColor = t.GetPixel(x, y);
                if (baseColor.a < .5f) continue;

                // Pixels individuais dão desgaste, reflexos e granulação sem borrar a silhueta.
                t.SetPixel(x, y, Color.Lerp(baseColor, (i & 1) == 0 ? light : dark, .25f));
            }
        }
    }
}
