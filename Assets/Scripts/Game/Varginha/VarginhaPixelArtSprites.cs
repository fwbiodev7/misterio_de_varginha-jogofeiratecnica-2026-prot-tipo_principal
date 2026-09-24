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
            var reference = VarginhaReferenceSprites.ForId(id);
            if (reference != null) return reference;
            // Roster art has one source of truth in the authored, directional atlases.
            if (id.StartsWith("StudentHead_"))
            {
                var portrait = VarginhaStudentSprites.Portrait(id.Substring("StudentHead_".Length));
                if (portrait != null) return portrait;
            }
            else if (id.StartsWith("Student_"))
            {
                var student = VarginhaStudentSprites.Frame(id.Substring("Student_".Length));
                if (student != null) return student;
            }
            if (id.StartsWith("Backpack") || id.StartsWith("Notebook")) color = new Color(.48f, .48f, .48f);
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
            else if (id == "Backpack_Side") DrawBackpackSide(texture, mid, dark, light);
            else if (id.StartsWith("Backpack")) DrawBackpack(texture, mid, dark, light);
            else if (id == "Inventory_Key") DrawInventoryKey(texture);
            else if (id == "Inventory_Journal") DrawInventoryJournal(texture);
            else if (id == "Notebook_Held") DrawNotebookClosed(texture, mid, dark, light);
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
            else if (id == "Alien_Ichor") DrawAlienIchor(texture);
            else if (id == "Dodge_Dust") DrawDodgeDust(texture, mid, dark, light);
            else if (id.StartsWith("HostageCage")) DrawHostageCage(texture, mid, dark, light);
            else if (id == "ET_Shockwave") DrawShockwave(texture, mid, false);
            else if (id.StartsWith("Flashlight_Cone")) DrawFlashlightCone(texture, mid);
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
            AddSurfaceFinish(texture, id);
            texture.anisoLevel = 0;
            if (id.StartsWith("Flashlight_Cone")) texture.filterMode = FilterMode.Bilinear;
            texture.Apply(false, false);
            Vector2 pivot = id.StartsWith("Flashlight_Cone") ? new Vector2(0.03125f, 0.5f) : new Vector2(0.5f, 0.5f);
            var sprite = Sprite.Create(texture, new Rect(0, 0, CanvasSize, CanvasSize), pivot, PixelsPerUnit, 0, SpriteMeshType.FullRect);
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
            Fill(t, 14, 10, 4, 16, dark); Fill(t, 11, 25, 10, 3, mid);
            Fill(t, 10, 28, 12, 2, dark);
            // The animated reference flame is a separate decorative child.
            if (VarginhaReferenceSprites.Fire(0) == null)
            {
                Color flame = new Color(1f, .72f, .25f);
                Fill(t, 15, 5, 2, 6, flame);
                Pixel(t, 15, 4, Color.white); Pixel(t, 16, 4, flame);
            }
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
            // Small olive leaf clusters echo the reference canopy, keeping the night palette.
            for (int i = 0; i < 37; i++)
            {
                int x = (i * 13 + 3) % 30, y = (i * 17 + 7) % 30;
                Pixel(t, x, y, Color.Lerp(mid, new Color(.39f, .46f, .24f), .22f));
                Pixel(t, x + 1, y + 1, Color.Lerp(mid, dark, .16f));
                Pixel(t, x + 2, y, Color.Lerp(mid, light, .11f));
            }
        }

        private static void AddSurfaceFinish(Texture2D texture, string id)
        {
            bool wood = id == "Floor_House" || id == "SchoolFloor";
            bool grass = id == "Floor_Yard";
            bool stone = id.StartsWith("ChurchFloor") || id.StartsWith("Wall") || id.EndsWith("Wall")
                || id.StartsWith("Driveway") || id.StartsWith("Street");
            bool furnishing = id.StartsWith("Bed_") || id.StartsWith("Sofa_") || id.StartsWith("Kitchen_")
                || id.StartsWith("Bookshelf") || id.StartsWith("SchoolLocker");
            if (!wood && !grass && !stone && !furnishing) return;
            Color[] pixels = texture.GetPixels();
            int width = texture.width;
            for (int y = 0; y < texture.height; y++) for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color color = pixels[index];
                if (color.a < .5f) continue;
                // Deterministic clustered grain, not random noise on characters, items or effects.
                int cluster = ((x / (wood ? 8 : 3)) * 31 + (y / (wood ? 2 : 3)) * 17) % 13;
                int fine = (x * 13 + y * 7 + x * y) % 11;
                float shade = (cluster - 6) * .004f + (fine < 2 ? -.025f : fine > 8 ? .018f : 0f);
                if (wood && y % 16 > 3 && (x + (y / 16) * 11) % 27 < 9 && y % 5 == 0) shade -= .025f;
                Color tint = shade > 0 ? new Color(.83f, .79f, .63f) : new Color(.07f, .10f, .12f);
                Color result = Color.Lerp(color, tint, Mathf.Abs(shade));
                result.a = color.a;
                pixels[index] = result;
            }
            texture.SetPixels(pixels);
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
            Color edge = new Color(.12f, .12f, .12f);
            // Rounded satchel, top handle, flap, twin buckles and gusseted pockets.
            Fill(t, 13, 27, 6, 4, edge); Fill(t, 14, 28, 4, 2, light); Fill(t, 15, 27, 2, 2, Color.clear);
            Fill(t, 8, 3, 16, 25, edge); Fill(t, 6, 5, 20, 21, edge);
            Fill(t, 7, 6, 18, 19, dark); Fill(t, 9, 4, 14, 23, mid);
            Fill(t, 10, 6, 12, 14, Color.Lerp(mid, dark, .18f));
            Fill(t, 4, 7, 4, 10, edge); Fill(t, 5, 8, 2, 7, mid); Fill(t, 5, 14, 2, 2, light);
            Fill(t, 24, 7, 4, 10, edge); Fill(t, 25, 8, 2, 7, dark); Fill(t, 25, 14, 2, 2, mid);
            Fill(t, 8, 19, 16, 8, edge); Fill(t, 9, 20, 14, 7, mid);
            Fill(t, 10, 26, 12, 1, light); Fill(t, 8, 21, 1, 4, light);
            Fill(t, 10, 18, 12, 2, edge); Fill(t, 10, 19, 12, 1, dark);
            for (int x = 10; x <= 19; x += 9)
            {
                Fill(t, x, 12, 3, 10, edge); Fill(t, x + 1, 13, 1, 9, dark);
                Fill(t, x, 16, 3, 3, light); Pixel(t, x + 1, 17, edge);
            }
            Fill(t, 10, 5, 12, 1, dark); Fill(t, 8, 7, 1, 8, light);
        }

        private static void DrawBackpackSide(Texture2D t, Color mid, Color dark, Color light)
        {
            Color edge = new Color(.12f, .12f, .12f);
            Fill(t, 12, 27, 5, 3, edge); Fill(t, 13, 28, 3, 1, light);
            Fill(t, 9, 5, 11, 21, edge); Fill(t, 7, 8, 13, 15, edge);
            Fill(t, 10, 6, 9, 20, dark); Fill(t, 8, 9, 9, 14, mid);
            Fill(t, 10, 23, 8, 3, mid); Fill(t, 10, 25, 7, 1, light);
            Fill(t, 8, 19, 10, 2, edge); Fill(t, 9, 20, 9, 1, light);
            Fill(t, 7, 7, 8, 9, edge); Fill(t, 8, 8, 6, 7, dark); Fill(t, 8, 14, 6, 1, light);
            Fill(t, 19, 9, 3, 17, edge); Fill(t, 20, 10, 1, 14, light);
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
            Color edge = new Color(.08f, .08f, .08f);
            Fill(t, 5, 12, 22, 17, edge); Fill(t, 6, 13, 20, 15, mid);
            Fill(t, 7, 14, 18, 13, dark); Fill(t, 8, 15, 16, 11, new Color(.14f,.14f,.14f));
            Fill(t, 8, 23, 15, 3, new Color(.18f,.18f,.18f));
            Fill(t, 9, 21, 1, 4, light); Fill(t, 10, 20, 1, 1, mid);
            Fill(t, 6, 27, 19, 1, light); Fill(t, 12, 12, 8, 1, dark);
            Fill(t, 4, 10, 24, 3, edge); Fill(t, 3, 8, 26, 3, edge); Fill(t, 2, 5, 28, 4, edge);
            Fill(t, 5, 10, 22, 2, light); Fill(t, 4, 8, 24, 2, light); Fill(t, 3, 6, 26, 2, mid);
            for (int x = 6; x < 26; x += 4) { Fill(t, x, 10, 2, 1, dark); Fill(t, x - 1, 8, 3, 1, dark); }
            Fill(t, 13, 6, 6, 2, light); Fill(t, 4, 5, 24, 1, dark);
        }

        private static void DrawNotebookClosed(Texture2D t, Color mid, Color dark, Color light)
        {
            var edge = new Color(.10f,.10f,.10f);
            Fill(t, 5, 5, 22, 22, edge); Fill(t, 6, 7, 19, 19, mid);
            Fill(t, 7, 25, 17, 1, light); Fill(t, 6, 8, 1, 17, light);
            Fill(t, 25, 7, 1, 18, dark); Fill(t, 6, 6, 19, 1, dark);
            Fill(t, 7, 5, 17, 1, light); Fill(t, 13, 16, 6, 2, dark);
        }

        private static void DrawCoffee(Texture2D t, Color mid, Color dark, Color light)
        {
            DrawCoffeeEmpty(t, mid, dark, light);
            Fill(t, 11, 20, 10, 2, new Color(.24f, .12f, .06f)); Pixel(t, 12, 21, new Color(.48f,.28f,.12f));
            Fill(t, 13, 25, 1, 3, new Color(.9f,.9f,.9f,.4f)); Pixel(t, 14, 28, new Color(.9f,.9f,.9f,.25f));
        }

        private static void DrawCoffeeEmpty(Texture2D t, Color mid, Color dark, Color light)
        {
            var ceramic = new Color(.80f,.79f,.74f); var edge = new Color(.23f,.22f,.21f);
            Fill(t, 22, 12, 5, 9, edge); Fill(t, 23, 13, 3, 7, ceramic); Fill(t, 23, 15, 2, 4, Color.clear);
            Fill(t, 9, 11, 14, 12, edge); Fill(t, 10, 9, 12, 14, edge);
            Fill(t, 11, 10, 10, 12, ceramic); Fill(t, 10, 13, 2, 8, light);
            Fill(t, 20, 12, 2, 9, mid); Fill(t, 12, 10, 8, 1, dark);
            Fill(t, 10, 20, 12, 3, ceramic); Fill(t, 11, 20, 10, 2, dark);
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
            // Paleta oficial de 7 cores extraída do spritesheet do Fusca 1996
            Color outline = new Color32(39, 44, 46, 255);
            Color darkBlue = new Color32(24, 105, 143, 255);
            Color midBlue = new Color32(49, 137, 204, 255);
            Color baseBlue = new Color32(69, 179, 230, 255);
            Color lightBlue = new Color32(89, 210, 247, 255);
            Color chrome = new Color32(176, 199, 209, 255);
            Color glass = new Color32(200, 242, 247, 255);
            Color glassDark = new Color32(105, 168, 188, 240);

            // Folha da porta preenche com precisão a área UV (x=6..25, y=6..29)
            for (int y = 6; y <= 29; y++)
            {
                int left = 6;
                int right = y <= 24 ? 25 : 25 - (y - 24);

                // Contorno e vedação de borracha
                Fill(t, left, y, right - left + 1, 1, outline);

                if (y > 6 && y < 29)
                {
                    int innerLeft = left + 1;
                    int innerRight = right - 1;
                    int innerWidth = innerRight - innerLeft + 1;

                    if (y == 16)
                    {
                        // Friso cromado da cintura (beltline)
                        Fill(t, innerLeft, y, innerWidth, 1, chrome);
                    }
                    else if (y > 16)
                    {
                        // Janela do motorista (proporcional à cabine, ~48% da altura)
                        for (int x = innerLeft; x <= innerRight; x++)
                        {
                            if (x == 20)
                            {
                                // Coluna do quebra-vento clássico do Fusca
                                Pixel(t, x, y, chrome);
                            }
                            else if ((x - y) % 5 == 0 || (x - y) % 5 == 1)
                            {
                                // Reflexo diagonal vítreo
                                Pixel(t, x, y, glass);
                            }
                            else
                            {
                                Pixel(t, x, y, glassDark);
                            }
                        }
                    }
                    else
                    {
                        // Painel de lataria inferior
                        Color bodyColor = y <= 8 ? darkBlue : y == 15 ? lightBlue : baseBlue;
                        Fill(t, innerLeft, y, innerWidth, 1, bodyColor);

                        // Sombra na borda posterior (abertura)
                        Pixel(t, innerLeft, y, midBlue);
                    }
                }
            }

            // Maçaneta clássica horizontal (x=8..12, y=13..14) com gatilho cromado
            Fill(t, 8, 13, 5, 2, outline);
            Pixel(t, 9, 14, chrome);
            Pixel(t, 10, 14, chrome);
            Pixel(t, 8, 12, darkBlue);
            Pixel(t, 12, 12, darkBlue);

            // Moldura superior da calha / teto
            Fill(t, 7, 28, 14, 1, chrome);
            // Trinco do quebra-vento
            Pixel(t, 20, 17, Color.white);
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

        private static void DrawAlienIchor(Texture2D t)
        {
            Color glow = new Color(.30f, 1f, .42f);
            Color core = new Color(.78f, 1f, .85f);
            Color deep = new Color(.08f, .55f, .22f);
            for (int y = 4; y < 28; y++)
            for (int x = 4; x < 28; x++)
            {
                int dx = Mathf.Abs(x - 16), dy = Mathf.Abs(y - 16);
                if (dx * dx + dy * dy < 18) Pixel(t, x, y, dx + dy < 4 ? core : glow);
            }
            Fill(t, 6, 7, 3, 2, glow); Fill(t, 23, 22, 3, 3, deep);
            Fill(t, 7, 24, 2, 3, deep); Fill(t, 24, 8, 3, 2, glow);
            Pixel(t, 16, 5, core); Pixel(t, 16, 27, deep);
            Pixel(t, 5, 16, deep); Pixel(t, 27, 16, glow);
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

        private static void DrawFlashlightCone(Texture2D t, Color color)
        {
            // Cone de luz âmbar/incandescente atmosférico anos 90: lente quente no bocal,
            // facho central brilhante e penumbra suave com partículas de poeira.
            Color lensCore = new Color(1f, 1f, .98f, 1f);
            Color hotCore = new Color(1f, .97f, .85f, .82f);
            Color warmBeam = new Color(1f, .90f, .60f, .55f);
            Color softEdge = new Color(.98f, .72f, .30f, .28f);
            Color faintEdge = new Color(.90f, .60f, .20f, .10f);

            for (int x = 1; x < 64; x++)
            {
                float progress = (x - 1f) / 63f;
                // Spread cresce de forma mais orgânica: lento no início, abre no final
                float halfSpread = Mathf.Lerp(2f, 28f, Mathf.Pow(progress, 0.68f));
                float softFringe = halfSpread * 1.28f; // penumbra além do cone duro
                int minY = Mathf.Clamp(Mathf.RoundToInt(32f - softFringe), 0, 63);
                int maxY = Mathf.Clamp(Mathf.RoundToInt(32f + softFringe), 0, 63);
                for (int y = minY; y <= maxY; y++)
                {
                    float absY = Mathf.Abs(y - 32f);
                    float tightDist = absY / Mathf.Max(1f, halfSpread);
                    float fringeDist = absY / Mathf.Max(1f, softFringe);

                    // Atenuação radial: mais intensa perto da lâmpada, suave no fim
                    float radialFalloff = Mathf.Pow(Mathf.Max(0f, 1f - progress * 0.65f), 1.2f);
                    // Atenuação angular em duas zonas: cone interno e penumbra
                    float innerAngle = Mathf.Max(0f, 1f - tightDist * tightDist * 1.4f);
                    float fringeAngle = Mathf.Max(0f, 1f - fringeDist * fringeDist);
                    float combined = Mathf.Max(innerAngle * radialFalloff, fringeAngle * radialFalloff * .35f);
                    if (combined <= 0.008f) continue;

                    Color col;
                    if (tightDist < 0.28f) col = hotCore;
                    else if (tightDist < 0.62f) col = warmBeam;
                    else if (tightDist < 1f) col = softEdge;
                    else col = faintEdge;
                    col.a *= combined;

                    // Brilho extra da lente na origem do bocal (ponto focal)
                    if (x <= 4 && absY <= 2f)
                    {
                        col = Color.Lerp(col, lensCore, 0.85f);
                        col.a = Mathf.Min(1f, col.a + 0.50f);
                    }
                    else if (x <= 8 && absY <= 4f)
                    {
                        col = Color.Lerp(col, lensCore, 0.40f);
                        col.a = Mathf.Min(1f, col.a + 0.20f);
                    }

                    // Ruído sutil de partículas de poeira na borda do feixe
                    if (tightDist > 0.7f && progress > 0.4f)
                    {
                        uint dustSeed = (uint)(x * 73 + y * 137);
                        if ((dustSeed % 11u) == 0u) col.a *= 1.6f;
                    }

                    Pixel(t, x, y, col);
                }
            }
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
            Color ink = new Color(.06f, .06f, .10f);
            Color denim = new Color(.16f, .21f, .36f);
            StudentPalette(id, out var skin, out _, out _);
            bool isFair = (skin.r + skin.g + skin.b) / 3f > 0.75f;
            Color skinShade = isFair
                ? Color.Lerp(skin, new Color(.58f, .52f, .68f), .30f)
                : Color.Lerp(skin, new Color(.30f, .14f, .12f), .34f);
            Color skinLight = isFair
                ? Color.Lerp(skin, new Color(1f, .96f, .90f), .28f)
                : Color.Lerp(skin, new Color(1f, .88f, .72f), .22f);

            // Sombra do personagem no chão
            Ellipse(t, 16, 2, 9, 2, new Color(.02f, .03f, .06f, .38f));

            bool isTall = id.Contains("Tavares") || id.Contains("Martins");
            bool isWide = id.Contains("Anna") || id.Contains("Sabia");

            // --- PERNAS ---
            Color shoeSole = new Color(.08f, .07f, .10f);
            Color shoeTop = new Color(.15f, .14f, .18f);
            Color denimLight = new Color(.28f, .36f, .54f);
            Color denimShadow = new Color(.10f, .14f, .26f);
            int legY = isTall ? 4 : 3;
            int legH = isTall ? 10 : 9;
            // Perna esquerda
            Fill(t, 11, legY, 5, legH, denim);
            Fill(t, 11, legY, 1, legH, denimShadow); // Costura lateral
            Fill(t, 12, legY + 5, 1, 3, denimLight);  // Destaque central
            Fill(t, 15, legY, 1, legH, denimShadow);
            Fill(t, 11, legY, 5, 1, shoeSole); Fill(t, 11, legY + 1, 5, 1, shoeTop); // Bota/tênis
            // Perna direita
            Fill(t, 17, legY, 5, legH, denim);
            Fill(t, 17, legY, 1, legH, denimShadow);
            Fill(t, 18, legY + 5, 1, 3, denimLight);
            Fill(t, 21, legY, 1, legH, denimShadow);
            Fill(t, 17, legY, 5, 1, shoeSole); Fill(t, 17, legY + 1, 5, 1, shoeTop);
            // Sombra entre as pernas
            Fill(t, 16, legY + 2, 1, legH - 2, new Color(.04f, .04f, .08f, .6f));

            // --- TORSO ---
            int torsoX = isWide ? 7 : 9;
            int torsoW = isWide ? 19 : 15;
            int torsoY = isTall ? 13 : 12;
            Color torsoShadow = Color.Lerp(mid, new Color(.04f, .04f, .08f), .42f);
            Color torsoHighlight = Color.Lerp(mid, Color.white, .32f);
            // Silhueta com contorno
            Fill(t, torsoX - 1, torsoY - 1, torsoW + 2, 12, ink);
            Fill(t, torsoX, torsoY, torsoW, 10, mid);
            // Shading: sombra esquerda, highlight direita-superior
            Fill(t, torsoX, torsoY, 2, 9, torsoShadow);
            Fill(t, torsoX + torsoW - 2, torsoY, 2, 7, torsoShadow);
            Fill(t, torsoX + 1, torsoY + 8, torsoW - 3, 2, torsoHighlight);
            Fill(t, torsoX + 2, torsoY + 9, torsoW - 5, 1, mid);

            // --- MANGAS ---
            int armLeft = isWide ? 4 : 6;
            int armRight = isWide ? torsoX + torsoW + 1 : 24;
            int armW = isWide ? 5 : 4;
            // Manga esquerda
            Fill(t, armLeft - 1, torsoY, armW + 2, 8, ink);
            Fill(t, armLeft, torsoY + 1, armW, 6, mid);
            Fill(t, armLeft, torsoY + 1, 1, 5, torsoShadow);
            // Manga direita
            Fill(t, armRight - 1, torsoY, armW + 2, 8, ink);
            Fill(t, armRight, torsoY + 1, armW, 6, mid);
            Fill(t, armRight + armW - 1, torsoY + 1, 1, 5, torsoShadow);
            // Mãos
            Fill(t, armLeft, torsoY + 7, armW, 2, ink);
            Fill(t, armLeft + 1, torsoY + 7, armW - 2, 1, skin);
            Fill(t, armRight, torsoY + 7, armW, 2, ink);
            Fill(t, armRight + 1, torsoY + 7, armW - 2, 1, skinShade);

            // --- ACESSÓRIOS DE CORPO POR PERSONAGEM ---
            if (id.Contains("Marcos"))
            {
                // Marcos: moreno atlético - luvas de boxe vermelhas e faixa esportiva
                Color gloveRed = new Color(.82f, .18f, .18f);
                Color gloveDark = new Color(.55f, .08f, .08f);
                Fill(t, armLeft - 1, torsoY + 5, armW + 2, 4, gloveDark);
                Fill(t, armLeft, torsoY + 6, armW, 2, gloveRed);
                Fill(t, armRight - 1, torsoY + 5, armW + 2, 4, gloveDark);
                Fill(t, armRight, torsoY + 6, armW, 2, gloveRed);
                // Faixa abdominal branca
                Fill(t, torsoX + 1, torsoY + 4, torsoW - 2, 1, new Color(.9f, .9f, .9f));
                Pixel(t, 15, torsoY + 4, new Color(.7f, .7f, .7f));
            }
            else if (id.Contains("Matias"))
            {
                // Matias: faixa azul royal amarrada na cintura, pontas longas caindo
                Color beltBlue = new Color(.10f, .38f, .90f);
                Color beltShine = new Color(.48f, .74f, 1f);
                Fill(t, torsoX, torsoY + 1, torsoW, 3, beltBlue);
                Fill(t, torsoX + 1, torsoY + 2, torsoW - 2, 1, beltShine);
                // Pontas da faixa caindo entre as pernas
                Fill(t, 13, torsoY - 2, 2, 3, beltBlue);
                Fill(t, 17, torsoY - 3, 2, 4, beltBlue);
                Pixel(t, 13, torsoY - 1, beltShine); Pixel(t, 17, torsoY - 2, beltShine);
            }
            else if (id.Contains("Yasmin"))
            {
                // Yasmin: pasta de arte branca sob o braço
                Fill(t, 12, torsoY + 2, 10, 4, ink);
                Fill(t, 13, torsoY + 3, 8, 2, new Color(.93f, .95f, .92f));
                for (int bx = 14; bx < 20; bx += 3)
                    Fill(t, bx, torsoY + 3, 1, 2, new Color(.80f, .82f, .80f));
                // Alça tiracolo roxa
                Fill(t, torsoX + torsoW - 1, torsoY, 2, 10, new Color(.35f, .20f, .50f));
                Pixel(t, torsoX + torsoW, torsoY + 5, new Color(.55f, .38f, .70f));
            }
            else if (id.Contains("Pedro"))
            {
                // Pedro: munhequeiras brancas (estilo esportivo)
                Fill(t, armLeft, torsoY + 5, armW, 2, Color.white);
                Fill(t, armRight, torsoY + 5, armW, 2, Color.white);
                Pixel(t, armLeft + 1, torsoY + 6, new Color(.85f, .85f, .85f));
                Pixel(t, armRight + 1, torsoY + 6, new Color(.85f, .85f, .85f));
            }
            else if (id.Contains("Fabio"))
            {
                // Fabio: camisa social com botões centrais e colarinho
                Fill(t, 15, torsoY + 1, 3, 8, Color.Lerp(mid, Color.white, .18f));
                Pixel(t, 16, torsoY + 2, ink); Pixel(t, 16, torsoY + 4, ink);
                Pixel(t, 16, torsoY + 6, ink);
                // Colarinho
                Fill(t, 14, torsoY + 9, 5, 1, Color.Lerp(mid, Color.white, .35f));
            }
            else if (id.Contains("Anna") || id.Contains("Sabia"))
            {
                // Anna Sabia: roupa larga oversized - extende mangas
                Fill(t, armLeft - 2, torsoY, armW + 4, 9, mid);
                Fill(t, armLeft - 1, torsoY + 1, armW + 2, 7, mid);
                Fill(t, armRight - 2, torsoY, armW + 4, 9, mid);
                Fill(t, torsoX, torsoY + 8, torsoW, 2, torsoHighlight);
                Fill(t, torsoX, torsoY + 9, torsoW, 1, mid);
            }
            else if (id.Contains("Tavares"))
            {
                // Tavares: cachecol de lã encorpado em tons quentes
                Color scarf1 = new Color(.82f, .74f, .58f);
                Color scarf2 = new Color(.68f, .56f, .40f);
                Color scarf3 = new Color(.92f, .84f, .70f);
                Fill(t, 12, torsoY + 6, 9, 4, scarf1);
                Fill(t, 11, torsoY + 7, 11, 2, scarf2);
                Fill(t, 13, torsoY + 8, 7, 1, scarf3);
                Pixel(t, 12, torsoY + 8, new Color(.90f, .34f, .24f)); // Detalhe cor
            }
            else if (id.Contains("Messias"))
            {
                // Luis Messias: alça de mochila transversal no peito
                Fill(t, 15, torsoY + 1, 2, 9, new Color(.14f, .10f, .08f));
                Fill(t, 13, torsoY + 8, 5, 1, new Color(.14f, .10f, .08f));
                Pixel(t, 15, torsoY + 5, new Color(.42f, .30f, .22f));
                // Bolso lateral
                Fill(t, torsoX + 1, torsoY + 5, 3, 4, Color.Lerp(mid, dark, .35f));
                Fill(t, torsoX + 1, torsoY + 9, 3, 1, ink);
            }
            else if (id.Contains("Martins"))
            {
                // Martins: fone de ouvido de estúdio no pescoço + camisa com logo
                Color headband = new Color(.08f, .08f, .10f);
                Color earpad = new Color(.22f, .22f, .26f);
                Fill(t, 8, torsoY + 7, 4, 4, headband); Fill(t, 21, torsoY + 7, 4, 4, headband);
                Fill(t, 9, torsoY + 8, 2, 2, earpad); Fill(t, 22, torsoY + 8, 2, 2, earpad);
                // Cordão do fone entre ombros
                Fill(t, 12, torsoY + 10, 9, 1, new Color(.18f, .18f, .22f));
                // Logo minimalista no peito
                Fill(t, 14, torsoY + 5, 5, 3, Color.Lerp(mid, Color.white, .22f));
                Pixel(t, 16, torsoY + 6, ink);
            }

            int headDy = isTall ? 2 : 0;
            DrawStudentFace(t, mid, id, headDy);
        }

        private static void DrawStudentHead(Texture2D t, Color mid, Color dark, Color light, string id)
        {
            // Retrato do aliado no HUD: rosto grande e expressivo ocupando o canvas inteiro.
            // Paleta do personagem para fundo e moldura.
            StudentPalette(id, out var skin, out var hair, out var hairLight);
            bool isFair = (skin.r + skin.g + skin.b) / 3f > 0.75f;
            Color skinShade = isFair
                ? Color.Lerp(skin, new Color(.58f, .52f, .68f), .30f)
                : Color.Lerp(skin, new Color(.30f, .14f, .12f), .34f);
            Color skinLt = isFair
                ? Color.Lerp(skin, new Color(1f, .96f, .90f), .30f)
                : Color.Lerp(skin, new Color(1f, .88f, .72f), .22f);
            Color panelBg = new Color(.06f, .07f, .12f);
            Color panelBorder = Color.Lerp(mid, new Color(.28f, .40f, .44f), .6f);

            // Fundo escuro com moldura colorida sutil
            Fill(t, 0, 0, 32, 32, panelBg);
            // Moldura
            Fill(t, 1, 1, 30, 1, panelBorder); Fill(t, 1, 30, 30, 1, panelBorder);
            Fill(t, 1, 1, 1, 30, panelBorder); Fill(t, 30, 1, 1, 30, panelBorder);
            // Cantos decorativos
            Fill(t, 2, 2, 2, 2, panelBorder); Fill(t, 28, 2, 2, 2, panelBorder);
            Fill(t, 2, 28, 2, 2, panelBorder); Fill(t, 28, 28, 2, 2, panelBorder);

            // Pescoço e ombros (parte inferior do retrato)
            Fill(t, 11, 2, 10, 5, skin);
            Fill(t, 11, 2, 2, 4, skinShade); // Sombra lateral esquerda do pescoço
            Fill(t, 19, 2, 2, 4, skinShade);
            // Ombros - cor da camisa
            Fill(t, 4, 2, 8, 6, Color.Lerp(mid, panelBg, .3f));
            Fill(t, 20, 2, 8, 6, Color.Lerp(mid, panelBg, .3f));
            Fill(t, 5, 6, 22, 2, mid);

            // --- ROSTO BASE ---
            // Silhueta do rosto com contorno limpo
            Fill(t, 8, 7, 16, 18, new Color(.04f, .04f, .08f)); // Contorno
            Fill(t, 9, 8, 14, 16, skin); // Base da pele
            // Shading 3D
            Fill(t, 9, 8, 2, 15, skinShade);  // Sombra lado esquerdo
            Fill(t, 21, 8, 2, 14, skinShade); // Sombra lado direito
            Fill(t, 10, 22, 12, 2, skinShade); // Sombra queixo
            Fill(t, 11, 8, 10, 3, skinLt);    // Highlight testa

            // Nariz (centro do rosto)
            Pixel(t, 16, 14, skinShade); Pixel(t, 15, 15, skinShade);
            Pixel(t, 17, 15, skinShade); Pixel(t, 16, 16, Color.Lerp(skinShade, skin, .5f));

            // Boca
            bool isFemale2 = id.Contains("Yasmin") || id.Contains("Anna") || id.Contains("Sabia") || id.Contains("Tavares");
            Color lipCol = isFemale2
                ? Color.Lerp(skin, new Color(.88f, .22f, .28f), isFair ? .60f : .50f)
                : Color.Lerp(skin, new Color(.65f, .20f, .20f), .60f);
            Fill(t, 13, 12, 6, 2, lipCol);
            Fill(t, 14, 11, 4, 1, Color.Lerp(lipCol, new Color(.20f, .04f, .04f), .5f)); // Abertura
            if (isFemale2) Pixel(t, 16, 12, Color.Lerp(lipCol, Color.white, .45f)); // Brilho lábio
            Pixel(t, 16, 11, skinLt); // Sulco filtrum

            // Orelhas
            Fill(t, 8, 14, 2, 4, skin);
            Fill(t, 22, 14, 2, 4, skin);
            Pixel(t, 8, 15, skinShade); Pixel(t, 23, 15, skinShade);

            // --- OLHOS EXPRESSIVOS ---
            Color eyeWhite = new Color(.97f, .97f, .95f);
            Color eyeDark = new Color(.06f, .05f, .10f);
            Color iris1 = id.Contains("Yasmin") ? new Color(.15f, .12f, .18f)
                : id.Contains("Matias") || id.Contains("Marcos") ? new Color(.22f, .14f, .08f)
                : new Color(.24f, .18f, .12f);
            // Sobrancelhas
            Fill(t, 11, 19, 4, 2, hair);
            Fill(t, 17, 19, 4, 2, hair);
            // Área dos olhos
            Fill(t, 10, 16, 6, 4, eyeDark); Fill(t, 16, 16, 7, 4, eyeDark);
            Fill(t, 11, 17, 4, 2, eyeWhite); Fill(t, 17, 17, 4, 2, eyeWhite);
            // Íris e pupila
            Fill(t, 12, 17, 2, 2, iris1); Pixel(t, 12, 17, eyeDark);
            Fill(t, 18, 17, 2, 2, iris1); Pixel(t, 18, 17, eyeDark);
            // Brilho do olho
            Pixel(t, 14, 18, eyeWhite); Pixel(t, 20, 18, eyeWhite);

            // --- CABELO FRONTAL ESPECÍFICO ---
            DrawStudentHairPortrait(t, id, hair, hairLight, skin);
        }

        private static void DrawStudentHairPortrait(Texture2D t, string id, Color hair, Color hairLight, Color skin)
        {
            // Cabelo frontal de alta qualidade para o retrato HUD.
            if (id.Contains("Yasmin"))
            {
                // Yasmin: franja preta reta sobre a testa, mechas laterais longas
                Fill(t, 8, 25, 16, 7, hair);   // Volume superior
                Fill(t, 9, 24, 14, 3, hair);   // Topo da cabeça
                Fill(t, 8, 21, 2, 8, hair); Fill(t, 22, 21, 2, 8, hair); // Mechas laterais
                Fill(t, 7, 18, 2, 5, hair); Fill(t, 23, 18, 2, 5, hair);
                // Franja
                Fill(t, 9, 20, 14, 2, hair);
                Pixel(t, 10, 21, hairLight); Pixel(t, 18, 21, hairLight); // Brilho franja
                // Presilha vermelha
                Fill(t, 8, 23, 3, 2, new Color(.86f, .14f, .18f));
                Pixel(t, 9, 24, new Color(1f, .55f, .58f));
            }
            else if (id.Contains("Anna") || id.Contains("Sabia"))
            {
                // Anna Sabia: cabelo cacheado afro volumoso
                Fill(t, 7, 22, 18, 10, hair);
                Fill(t, 6, 20, 20, 6, hair);
                Fill(t, 5, 18, 4, 4, hair); Fill(t, 23, 18, 4, 4, hair);
                Fill(t, 9, 30, 14, 2, hair);
                // Brilhos nos cachos
                Pixel(t, 9, 26, hairLight); Pixel(t, 13, 28, hairLight);
                Pixel(t, 18, 27, hairLight); Pixel(t, 22, 25, hairLight);
                Pixel(t, 7, 22, hairLight); Pixel(t, 24, 22, hairLight);
            }
            else if (id.Contains("Tavares"))
            {
                // Tavares: cachos longos e encorpados, brincos de argola dourados
                Fill(t, 7, 20, 18, 12, hair);
                Fill(t, 6, 18, 20, 6, hair);
                Fill(t, 5, 16, 3, 5, hair); Fill(t, 24, 16, 3, 5, hair);
                Fill(t, 9, 30, 14, 2, hair);
                // Brilhos dos cachos
                Pixel(t, 10, 27, hairLight); Pixel(t, 16, 29, hairLight);
                Pixel(t, 21, 27, hairLight); Pixel(t, 8, 23, hairLight);
                // Brinco dourado
                Pixel(t, 6, 16, new Color(1f, .84f, .20f));
                Pixel(t, 6, 17, new Color(.94f, .70f, .12f));
                Pixel(t, 7, 16, new Color(1f, .92f, .48f));
            }
            else if (id.Contains("Pedro"))
            {
                // Pedro: juba cacheada longa e volumosa + óculos redondos
                Fill(t, 7, 21, 18, 11, hair);
                Fill(t, 6, 19, 20, 5, hair);
                Fill(t, 5, 17, 3, 4, hair); Fill(t, 24, 17, 3, 4, hair);
                Fill(t, 9, 30, 14, 2, hair);
                Pixel(t, 9, 26, hairLight); Pixel(t, 16, 28, hairLight); Pixel(t, 21, 25, hairLight);
                // Óculos redondos
                Color gf = new Color(.12f, .14f, .20f);
                Color gs = new Color(.78f, .92f, 1f);
                Fill(t, 10, 16, 5, 4, gf); Fill(t, 17, 16, 5, 4, gf);
                Pixel(t, 15, 17, gf); Pixel(t, 16, 17, gf); // Ponte dos óculos
                Fill(t, 11, 17, 3, 2, gs); Fill(t, 18, 17, 3, 2, gs);
                Pixel(t, 11, 18, skin); Pixel(t, 18, 18, skin); // Reflexo pele na lente
            }
            else if (id.Contains("Fabio"))
            {
                // Fabio: cabelo dividido ao meio anos 90 + óculos quadrados
                Fill(t, 9, 24, 14, 8, hair);
                Fill(t, 8, 22, 16, 4, hair);
                Fill(t, 7, 18, 3, 6, hair); Fill(t, 22, 18, 3, 6, hair);
                // Risca ao meio
                Fill(t, 15, 24, 2, 4, skin);
                // Brilho do cabelo
                Pixel(t, 11, 26, hairLight); Fill(t, 12, 27, 3, 1, hairLight);
                Pixel(t, 19, 26, hairLight); Fill(t, 18, 27, 3, 1, hairLight);
                // Óculos quadrados
                Color gf = new Color(.12f, .14f, .20f);
                Color gs = new Color(.78f, .92f, 1f);
                Fill(t, 10, 16, 5, 4, gf); Fill(t, 17, 16, 5, 4, gf);
                Pixel(t, 15, 17, gf); Pixel(t, 16, 17, gf);
                Fill(t, 11, 17, 3, 2, gs); Fill(t, 18, 17, 3, 2, gs);
            }
            else if (id.Contains("Matias"))
            {
                // Matias: cacheado volumoso com faixa azul na testa
                Fill(t, 9, 24, 14, 8, hair);
                Fill(t, 8, 22, 16, 5, hair);
                Fill(t, 7, 19, 3, 5, hair); Fill(t, 22, 19, 3, 5, hair);
                Pixel(t, 11, 27, hairLight); Pixel(t, 16, 28, hairLight); Pixel(t, 20, 27, hairLight);
                // Faixa azul na testa
                Color hb = new Color(.10f, .38f, .90f);
                Color hbL = new Color(.45f, .72f, 1f);
                Fill(t, 8, 20, 16, 3, hb);
                Fill(t, 9, 21, 14, 1, hbL);
                // Nó lateral da faixa
                Fill(t, 6, 19, 3, 4, hb);
                Pixel(t, 7, 20, hbL);
            }
            else if (id.Contains("Marcos"))
            {
                // Marcos: cacheado compacto com burst fade nas têmporas
                Fill(t, 10, 25, 12, 7, hair);
                Fill(t, 9, 23, 14, 4, hair);
                Fill(t, 8, 21, 3, 4, hair); Fill(t, 21, 21, 3, 4, hair);
                Pixel(t, 11, 27, hairLight); Pixel(t, 16, 28, hairLight);
                // Burst fade: degradê de pele para cabelo nas têmporas
                Color fade1 = Color.Lerp(skin, hair, .25f);
                Color fade2 = Color.Lerp(skin, hair, .55f);
                Fill(t, 7, 18, 2, 4, fade1); Fill(t, 9, 19, 1, 3, fade2);
                Fill(t, 23, 18, 2, 4, fade1); Fill(t, 22, 19, 1, 3, fade2);
            }
            else if (id.Contains("Messias"))
            {
                // Luis Messias: buzzcut quase raspado + barba e cavanhaque cheio
                Fill(t, 9, 26, 14, 6, hair);
                Fill(t, 10, 25, 12, 2, hair);
                // Buzzcut rente (textura)
                Pixel(t, 10, 28, hairLight); Pixel(t, 15, 29, hairLight);
                // Barba
                Color beard = Color.Lerp(hair, new Color(.08f, .06f, .05f), .3f);
                Fill(t, 11, 8, 10, 4, beard);   // Barba/cavanhaque
                Fill(t, 9, 10, 3, 5, beard);   // Costeleta esquerda
                Fill(t, 20, 10, 3, 5, beard);  // Costeleta direita
                Fill(t, 13, 9, 6, 2, beard);   // Bigode
                Pixel(t, 13, 11, Color.Lerp(beard, skin, .4f));
                Pixel(t, 18, 11, Color.Lerp(beard, skin, .4f));
            }
            else if (id.Contains("Martins"))
            {
                // Martins: cabelo liso jogado pro lado com volume
                Fill(t, 8, 24, 16, 8, hair);
                Fill(t, 7, 22, 14, 4, hair); // Mecha jogada para esquerda
                Fill(t, 6, 20, 5, 5, hair);
                Fill(t, 22, 22, 3, 5, hair); // Lateral direita
                // Brilho do cabelo liso
                Fill(t, 9, 27, 8, 2, hairLight);
                Pixel(t, 8, 25, hairLight);
                // Headphone pendurado no pescoço
                Color hpBand = new Color(.08f, .08f, .10f);
                Color hpPad = new Color(.20f, .20f, .24f);
                Fill(t, 6, 8, 4, 5, hpBand); Fill(t, 22, 8, 4, 5, hpBand);
                Fill(t, 7, 9, 2, 3, hpPad); Fill(t, 23, 9, 2, 3, hpPad);
            }
        }

        private static void StudentPalette(string id, out Color skin, out Color hair, out Color hairLight)
        {
            skin = new Color(.86f, .61f, .43f);
            hair = new Color(.17f, .10f, .09f);
            hairLight = new Color(.32f, .21f, .15f);

            if (id.Contains("Yasmin"))
            {
                // yasmin: cabelo preto com franja e branca
                skin = new Color(.96f, .87f, .82f);
                hair = new Color(.07f, .06f, .08f);
                hairLight = new Color(.19f, .17f, .23f);
            }
            else if (id.Contains("Pedro"))
            {
                // pedro: cabeludo cacheado e de oculos — branco
                skin = new Color(.96f, .83f, .75f);
                hair = new Color(.20f, .13f, .08f);
                hairLight = new Color(.44f, .27f, .17f);
            }
            else if (id.Contains("Matias"))
            {
                // matias: cabelo cacheado e faixa azul
                skin = new Color(.78f, .56f, .40f);
                hair = new Color(.12f, .10f, .10f);
                hairLight = new Color(.28f, .23f, .20f);
            }
            else if (id.Contains("Marcos"))
            {
                // marcos: moreno de cabelo cacheado com burst fade
                skin = new Color(.48f, .30f, .20f);
                hair = new Color(.08f, .07f, .08f);
                hairLight = new Color(.20f, .17f, .18f);
            }
            else if (id.Contains("Anna") || id.Contains("Sabia"))
            {
                // sabia: cabelo cacheado e roupa larga — branca
                skin = new Color(.96f, .83f, .75f);
                hair = new Color(.16f, .11f, .08f);
                hairLight = new Color(.38f, .24f, .16f);
            }
            else if (id.Contains("Tavares"))
            {
                // tavares: cabelo cacheado e alta — branca
                skin = new Color(.96f, .84f, .76f);
                hair = new Color(.20f, .13f, .10f);
                hairLight = new Color(.42f, .26f, .18f);
            }
            else if (id.Contains("Messias"))
            {
                // luis messias: buzzcut e barba
                skin = new Color(.80f, .55f, .40f);
                hair = new Color(.12f, .09f, .08f);
                hairLight = new Color(.26f, .19f, .16f);
            }
            else if (id.Contains("Martins"))
            {
                // martins: cabelo liso pro lado e alto e branco
                skin = new Color(.95f, .84f, .76f);
                hair = new Color(.18f, .13f, .10f);
                hairLight = new Color(.36f, .25f, .18f);
            }
            else if (id.Contains("Fabio"))
            {
                // fabio: cabelo dividido ao meio e de oculos
                skin = new Color(.89f, .70f, .54f);
                hair = new Color(.22f, .14f, .09f);
                hairLight = new Color(.45f, .28f, .18f);
            }
        }

        private static void DrawStudentFace(Texture2D t, Color shirt, string id, int dy)
        {
            // NOTA: dy é usado para ajustar a posição vertical do rosto (para alunos altos, dy=+2)
            Color ink = new Color(.05f, .04f, .08f);
            StudentPalette(id, out var skin, out var hair, out var hairLight);
            bool isFairFace = (skin.r + skin.g + skin.b) / 3f > 0.75f;
            Color skinShade = isFairFace
                ? Color.Lerp(skin, new Color(.58f, .52f, .68f), .30f)
                : Color.Lerp(skin, new Color(.30f, .14f, .12f), .34f);
            Color skinLight = isFairFace
                ? Color.Lerp(skin, new Color(1f, .96f, .90f), .30f)
                : Color.Lerp(skin, new Color(1f, .88f, .72f), .22f);

            // 1. Cabelo traseiro volumoso (base atrás do rosto)
            if (id.Contains("Yasmin"))
            {
                // Cabelo preto liso com caimento nas costas e mechas
                Fill(t, 9, 17 + dy, 3, 9, hair); Fill(t, 21, 17 + dy, 3, 9, hair);
                Fill(t, 8, 19 + dy, 2, 7, hair); Fill(t, 23, 19 + dy, 2, 7, hair);
                Pixel(t, 9, 19 + dy, hairLight); Pixel(t, 22, 19 + dy, hairLight);
            }
            else if (id.Contains("Anna") || id.Contains("Sabia"))
            {
                // Cabelo cacheado volumoso afro
                Fill(t, 8, 18 + dy, 4, 8, hair); Fill(t, 21, 18 + dy, 4, 8, hair);
                Fill(t, 7, 20 + dy, 2, 7, hair); Fill(t, 24, 20 + dy, 2, 7, hair);
                Pixel(t, 7, 22 + dy, hairLight); Pixel(t, 24, 22 + dy, hairLight);
            }
            else if (id.Contains("Tavares"))
            {
                // Cachos longos e volumosos caindo
                Fill(t, 7, 16 + dy, 4, 12, hair); Fill(t, 22, 16 + dy, 4, 12, hair);
                Fill(t, 6, 18 + dy, 2, 9, hair); Fill(t, 25, 18 + dy, 2, 9, hair);
                Pixel(t, 8, 17 + dy, hairLight); Pixel(t, 24, 17 + dy, hairLight);
                // Brinco dourado
                Pixel(t, 7, 18 + dy, new Color(1f, .84f, .20f));
                Pixel(t, 7, 17 + dy, new Color(.94f, .70f, .12f));
            }
            else if (id.Contains("Pedro"))
            {
                // Juba cacheada longa e densa
                Fill(t, 7, 17 + dy, 4, 10, hair); Fill(t, 22, 17 + dy, 4, 10, hair);
                Fill(t, 6, 20 + dy, 2, 7, hair); Fill(t, 25, 20 + dy, 2, 7, hair);
                Pixel(t, 8, 20 + dy, hairLight); Pixel(t, 24, 20 + dy, hairLight);
            }
            else if (id.Contains("Matias"))
            {
                // Cacheado volumoso com faixa azul
                Fill(t, 9, 18 + dy, 3, 7, hair); Fill(t, 21, 18 + dy, 3, 7, hair);
                Fill(t, 8, 20 + dy, 2, 5, hair); Fill(t, 23, 20 + dy, 2, 5, hair);
            }
            else
            {
                // Cabelo curto masculino genérico
                Fill(t, 10, 19 + dy, 13, 4, ink);
                Fill(t, 9, 20 + dy, 2, 3, ink); Fill(t, 22, 20 + dy, 2, 3, ink);
            }

            // 2. Silhueta do rosto com contorno firme
            Fill(t, 10, 18 + dy, 13, 11, ink); // Contorno
            Fill(t, 11, 19 + dy, 11, 9, skin); // Base da pele
            // Shading 3D no rosto
            Fill(t, 11, 19 + dy, 2, 8, skinShade); // Sombra esquerda
            Fill(t, 20, 19 + dy, 2, 7, skinShade); // Sombra direita
            Fill(t, 12, 19 + dy, 9, 2, skinLight);  // Highlight na testa
            Fill(t, 12, 26 + dy, 9, 2, skinShade);  // Sombra queixo

            // 3. Orelhas
            Fill(t, 9, 21 + dy, 2, 4, skin);
            Fill(t, 22, 21 + dy, 2, 4, skin);
            Pixel(t, 9, 22 + dy, skinShade); Pixel(t, 23, 22 + dy, skinShade);

            // 4. Olhos com brilho e íris colorida
            Color eyeWhite = new Color(.97f, .97f, .95f);
            Color eyeDark = new Color(.06f, .05f, .10f);
            Color irisCol = id.Contains("Yasmin") ? new Color(.14f, .11f, .16f)
                : id.Contains("Marcos") ? new Color(.28f, .16f, .09f)
                : new Color(.22f, .16f, .10f);
            // Sobrancelhas expressivas
            Fill(t, 12, 25 + dy, 4, 1, hair); Fill(t, 17, 25 + dy, 4, 1, hair);
            if (id.Contains("Messias") || id.Contains("Marcos"))
            { // Sobrancelhas mais grossas e expressivas
                Fill(t, 12, 24 + dy, 4, 2, hair); Fill(t, 17, 24 + dy, 4, 2, hair);
            }
            // Área dos olhos
            Fill(t, 12, 22 + dy, 5, 3, eyeDark); Fill(t, 16, 22 + dy, 5, 3, eyeDark);
            Fill(t, 12, 23 + dy, 4, 2, eyeWhite); Fill(t, 17, 23 + dy, 4, 2, eyeWhite);
            // Íris e pupila
            Fill(t, 13, 23 + dy, 2, 2, irisCol); Pixel(t, 13, 23 + dy, eyeDark);
            Fill(t, 18, 23 + dy, 2, 2, irisCol); Pixel(t, 18, 23 + dy, eyeDark);
            // Brilho dos olhos
            Pixel(t, 15, 24 + dy, eyeWhite); Pixel(t, 20, 24 + dy, eyeWhite);

            // 5. Nariz
            Pixel(t, 16, 22 + dy, skinShade);
            Pixel(t, 15, 21 + dy, skinShade); Pixel(t, 17, 21 + dy, skinShade);

            // 6. Boca com expressão
            bool isFemale = id.Contains("Yasmin") || id.Contains("Anna") || id.Contains("Sabia") || id.Contains("Tavares");
            Color lipColor = isFemale
                ? Color.Lerp(skin, new Color(.88f, .22f, .28f), isFairFace ? .58f : .48f)
                : Color.Lerp(skin, new Color(.62f, .20f, .20f), .52f);
            Fill(t, 13, 19 + dy, 7, 2, lipColor);
            Fill(t, 14, 19 + dy, 5, 1, Color.Lerp(lipColor, new Color(.12f, .02f, .02f), .55f));
            if (isFemale) Pixel(t, 16, 20 + dy, Color.Lerp(lipColor, Color.white, .40f));
            Pixel(t, 16, 18 + dy, skinLight); // Filtrum/sulco

            // 7. CABELO FRONTAL ESPECÍFICO DE CADA ALUNO
            if (id.Contains("Yasmin"))
            {
                // Franja preta reta e bem definida cobrindo a testa
                Fill(t, 10, 26 + dy, 13, 3, hair); // Franja compacta
                Fill(t, 11, 28 + dy, 11, 2, hair);
                Fill(t, 11, 25 + dy, 11, 2, hair); // Segunda camada de franja
                // Mechas laterais encaixando no pescoço
                Fill(t, 9, 21 + dy, 2, 7, hair); Fill(t, 22, 21 + dy, 2, 7, hair);
                // Brilho na franja
                Fill(t, 12, 28 + dy, 8, 1, hairLight);
                // Presilha vermelha cintilante
                Fill(t, 10, 27 + dy, 2, 2, new Color(.86f, .14f, .18f));
                Pixel(t, 11, 28 + dy, new Color(1f, .58f, .62f));
            }
            else if (id.Contains("Anna") || id.Contains("Sabia"))
            {
                // Coroa cacheada arredondada e volumosa
                Fill(t, 9, 27 + dy, 15, 5, hair);
                Fill(t, 10, 30 + dy, 13, 2, hair);
                Fill(t, 8, 22 + dy, 3, 7, hair); Fill(t, 22, 22 + dy, 3, 7, hair);
                Pixel(t, 10, 29 + dy, hairLight); Pixel(t, 15, 31 + dy, hairLight);
                Pixel(t, 19, 30 + dy, hairLight); Pixel(t, 23, 28 + dy, hairLight);
                Pixel(t, 8, 25 + dy, hairLight); Pixel(t, 24, 25 + dy, hairLight);
            }
            else if (id.Contains("Tavares"))
            {
                // Cachos longos encorpados, volume escultural
                Fill(t, 8, 26 + dy, 16, 6, hair);
                Fill(t, 9, 30 + dy, 14, 2, hair);
                Fill(t, 7, 21 + dy, 3, 9, hair); Fill(t, 23, 21 + dy, 3, 9, hair);
                Pixel(t, 10, 29 + dy, hairLight); Pixel(t, 16, 31 + dy, hairLight);
                Pixel(t, 21, 29 + dy, hairLight); Pixel(t, 8, 24 + dy, hairLight);
            }
            else if (id.Contains("Fabio"))
            {
                // Cabelo anos 90 dividido ao meio com risca central
                Fill(t, 10, 26 + dy, 13, 5, hair);
                Fill(t, 9, 24 + dy, 2, 5, hair); Fill(t, 22, 24 + dy, 2, 5, hair);
                // Risca ao meio
                Pixel(t, 16, 26 + dy, skin); Pixel(t, 16, 27 + dy, skin); Pixel(t, 16, 28 + dy, skin);
                Fill(t, 12, 29 + dy, 4, 1, hairLight); Fill(t, 18, 29 + dy, 3, 1, hairLight);
                // Óculos quadrados
                Color gf = new Color(.12f, .14f, .20f); Color gs = new Color(.78f, .92f, 1f);
                Fill(t, 12, 22 + dy, 4, 3, gf); Fill(t, 17, 22 + dy, 4, 3, gf);
                Pixel(t, 16, 23 + dy, gf); Pixel(t, 13, 23 + dy, gs); Pixel(t, 18, 23 + dy, gs);
            }
            else if (id.Contains("Matias"))
            {
                // Cacheado com faixa azul royal na testa
                Fill(t, 10, 27 + dy, 13, 5, hair);
                Fill(t, 9, 26 + dy, 3, 4, hair); Fill(t, 21, 26 + dy, 3, 4, hair);
                Pixel(t, 12, 29 + dy, hairLight); Pixel(t, 17, 30 + dy, hairLight);
                Color hb = new Color(.10f, .40f, .94f); Color hbL = new Color(.48f, .74f, 1f);
                Fill(t, 10, 25 + dy, 13, 2, hb); Fill(t, 11, 26 + dy, 11, 1, hbL);
                // Pontas laterais da faixa
                Fill(t, 8, 24 + dy, 2, 3, hb); Pixel(t, 8, 25 + dy, hbL);
            }
            else if (id.Contains("Marcos"))
            {
                // Cacheado compacto topo com burst fade nas têmporas
                Fill(t, 11, 28 + dy, 11, 4, hair);
                Fill(t, 10, 27 + dy, 13, 2, hair);
                Pixel(t, 13, 30 + dy, hairLight); Pixel(t, 17, 31 + dy, hairLight);
                // Burst fade degradê
                Color fade1 = Color.Lerp(skin, hair, .22f);
                Color fade2 = Color.Lerp(skin, hair, .52f);
                Fill(t, 9, 22 + dy, 2, 5, fade1); Fill(t, 10, 23 + dy, 1, 4, fade2);
                Fill(t, 22, 22 + dy, 2, 5, fade1); Fill(t, 21, 23 + dy, 1, 4, fade2);
            }
            else if (id.Contains("Pedro"))
            {
                // Juba cacheada longa e densa
                Fill(t, 8, 26 + dy, 16, 6, hair);
                Fill(t, 7, 22 + dy, 3, 8, hair); Fill(t, 23, 22 + dy, 3, 8, hair);
                Pixel(t, 10, 29 + dy, hairLight); Pixel(t, 18, 30 + dy, hairLight);
                // Óculos redondos
                Color gf = new Color(.12f, .14f, .20f); Color gs = new Color(.78f, .92f, 1f);
                Fill(t, 12, 22 + dy, 4, 3, gf); Fill(t, 17, 22 + dy, 4, 3, gf);
                Pixel(t, 16, 23 + dy, gf); Pixel(t, 13, 23 + dy, gs); Pixel(t, 18, 23 + dy, gs);
            }
            else if (id.Contains("Messias"))
            {
                // Buzzcut rente: apenas uma camada fina
                Fill(t, 11, 27 + dy, 11, 3, hair);
                Fill(t, 10, 26 + dy, 13, 2, hair);
                Pixel(t, 12, 29 + dy, hairLight); Pixel(t, 18, 29 + dy, hairLight);
                // Barba e cavanhaque cheio
                Color beard = Color.Lerp(hair, new Color(.08f, .06f, .05f), .35f);
                Fill(t, 11, 18 + dy, 2, 5, beard); Fill(t, 20, 18 + dy, 2, 5, beard); // Costeletas
                Fill(t, 13, 18 + dy, 7, 2, beard); // Queixo
                Fill(t, 13, 20 + dy, 7, 1, beard); // Bigode
                Pixel(t, 12, 20 + dy, Color.Lerp(beard, skin, .4f));
                Pixel(t, 20, 20 + dy, Color.Lerp(beard, skin, .4f));
            }
            else if (id.Contains("Martins"))
            {
                // Cabelo liso e volumoso jogado para um lado
                Fill(t, 8, 26 + dy, 16, 6, hair); // Volume principal
                Fill(t, 7, 24 + dy, 8, 4, hair);  // Mecha lateral grande
                Fill(t, 6, 22 + dy, 4, 4, hair);
                Fill(t, 22, 24 + dy, 3, 5, hair); // Lateral direita
                // Brilho do cabelo liso
                Fill(t, 9, 28 + dy, 9, 2, hairLight);
                Pixel(t, 8, 26 + dy, hairLight);
            }
        }

        private static void DrawHostageCage(Texture2D t, Color mid, Color dark, Color light)
        {
            // Modelo de jaula metálica com pilares laterais, barras de aço e placa de fechadura central.
            Color outline = new Color(.08f, .10f, .14f);
            Color steelDark = new Color(.38f, .44f, .55f);
            Color steelMid = new Color(.56f, .64f, .75f);
            Color steelLight = new Color(.78f, .84f, .92f);
            Color lockPlate = new Color(.48f, .54f, .65f);

            // Vigas e pilares externos
            Fill(t, 4, 4, 24, 2, outline);
            Fill(t, 4, 26, 24, 2, outline);
            Fill(t, 4, 6, 4, 20, outline);
            Fill(t, 24, 6, 4, 20, outline);

            Fill(t, 5, 5, 22, 1, steelLight);
            Fill(t, 5, 26, 22, 1, steelDark);
            Fill(t, 5, 6, 2, 20, steelMid);
            Fill(t, 25, 6, 2, 20, steelMid);

            // Barras verticais de aço
            int[] barXs = { 9, 13, 18, 22 };
            foreach (int bx in barXs)
            {
                Fill(t, bx - 1, 6, 3, 20, outline);
                Fill(t, bx, 6, 1, 20, steelLight);
                Fill(t, bx + 1, 6, 1, 20, steelDark);
            }

            // Travessa horizontal central
            Fill(t, 5, 15, 22, 3, outline);
            Fill(t, 5, 16, 22, 1, steelMid);

            // Placa central de fechadura
            Fill(t, 13, 13, 6, 7, outline);
            Fill(t, 14, 14, 4, 5, lockPlate);
            Fill(t, 14, 14, 4, 1, steelLight);
            Pixel(t, 15, 16, outline);
            Pixel(t, 15, 17, outline);
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
