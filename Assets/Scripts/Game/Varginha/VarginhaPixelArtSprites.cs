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

        public static Sprite Create(string id, Color color)
        {
            string key = id + color;
            if (Cache.TryGetValue(key, out var cached)) return cached;

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
            else if (id.StartsWith("Backpack")) DrawBackpack(texture, mid, dark, light);
            else if (id.StartsWith("Notebook")) DrawNotebook(texture, mid, dark, light);
            else if (id.StartsWith("Coffee_Empty")) DrawCoffeeEmpty(texture, mid, dark, light);
            else if (id.StartsWith("Coffee")) DrawCoffee(texture, mid, dark, light);
            else if (id.StartsWith("Fuse")) DrawFuseBox(texture, mid, dark, light);
            else if (id.StartsWith("Doc")) DrawDocument(texture, mid, dark, light);
            else if (id.StartsWith("FuscaDoor_Open")) DrawFuscaDoor(texture, mid, dark, light, true);
            else if (id.StartsWith("FuscaDoor")) DrawFuscaDoor(texture, mid, dark, light, false);
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
            Fill(t, 11, 19, 10, 10, dark); Fill(t, 12, 20, 8, 8, new Color(.95f, .65f, .43f));
            Fill(t, 12, 26, 8, 3, new Color(.06f, .07f, .08f));
            Fill(t, 12, 23, 3, 2, new Color(.05f, .05f, .06f)); Fill(t, 17, 23, 3, 2, new Color(.05f, .05f, .06f));
            Pixel(t, 15, 24, new Color(.05f, .05f, .06f)); Pixel(t, 16, 24, new Color(.05f, .05f, .06f));
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

        private static void DrawFuscaDoor(Texture2D t, Color mid, Color dark, Color light, bool open)
        {
            Fill(t, 7, 5, 18, 23, dark); Fill(t, 9, 7, 14, 19, mid); Fill(t, 11, 16, 10, 7, light);
            Fill(t, 18, 10, 3, 2, dark); Pixel(t, 21, 10, light);
            if (open) Fill(t, 3, 7, 4, 18, Color.Lerp(dark, Color.black, .4f));
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
