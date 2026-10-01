using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Game.Editor.Testing
{
    public static class VarginhaHouseReferenceImporter
    {
        private const string Folder = "Assets/Resources/Varginha/HouseReference512/";
        private static readonly string[] Names = { "Desk", "Chair", "Sofa", "CoffeeTable", "Bookshelf",
            "Nightstand", "Dresser", "Kitchen", "Bed", "TV", "Stove", "Fridge", "Lamp", "Plant", "Rug", "Door" };
        private static readonly string[] PropNames = { "Window", "ChestClosed", "ChestOpening", "ChestOpen",
            "CupFull", "CupEmpty", "Backpack", "Notebook", "Radio", "Clock", "WallPicture", "FuseBox",
            "Noticeboard", "DiningTable", "Document", "DiningChair" };
        private static readonly string[] YardNames = { "Fence", "FenceVertical", "Gate", "Bench", "Paving", "Grass",
            "Deck", "Lantern", "Mailbox", "Planter", "Flowers", "Barrel", "Tree", "Shrub", "Leaves", "Steps" };

        public static void ConfigureAll()
        {
            foreach (string file in new[] { "Furniture", "Floor", "Wall", "SofaNorth", "Chairs", "Props", "Architecture", "Yard", "EdelzioInteractions" }) Configure(file);
            ConfigureOriginalTree();
        }

        private static void ConfigureOriginalTree()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Varginha/TravelPixel/TreeReference.png");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            const string materialPath = "Assets/Resources/Varginha/OriginalTreeTone.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Varginha/OriginalSceneryTone")) { name = "OriginalTreeTone" };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = Shader.Find("Varginha/OriginalSceneryTone");
            material.SetColor("_LeafTint", new Color(.61f, .69f, .55f, 1));
            material.SetColor("_TrunkTint", new Color(.92f, .87f, .82f, 1));
            material.SetFloat("_FoliageOnly", 1);
            material.SetFloat("_Saturation", .96f);
            EditorUtility.SetDirty(material);
            const string carPath = "Assets/Resources/Varginha/OriginalFuscaTone.mat";
            var car = AssetDatabase.LoadAssetAtPath<Material>(carPath);
            if (car == null)
            {
                car = new Material(material.shader) { name = "OriginalFuscaTone" };
                AssetDatabase.CreateAsset(car, carPath);
            }
            car.shader = material.shader;
            car.SetColor("_TrunkTint", new Color(.94f, .96f, .93f, 1));
            car.SetFloat("_FoliageOnly", 0);
            car.SetFloat("_Saturation", .93f);
            EditorUtility.SetDirty(car);
            AssetDatabase.SaveAssets();
        }

        private static void Configure(string file)
        {
            string assetPath = Folder + file + ".png";
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Texture not imported: " + assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 512;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.anisoLevel = 0;
            importer.spritePixelsPerUnit = assetPath.EndsWith("Wall.png") || assetPath.EndsWith("SofaNorth.png") ? 512
                : file == "Chairs" || file == "Architecture" ? 256 : 128;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null) throw new System.InvalidOperationException("Sprite data provider unavailable.");
            provider.InitSpriteEditorDataProvider();
            var edit = provider.GetDataProvider<ISpriteFrameEditCapability>();
            if (edit == null || !edit.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite))
                throw new System.InvalidOperationException("Importer does not support sprite slicing.");
            var slices = new List<SpriteRect>();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (assetPath.EndsWith("Furniture.png"))
                for (int i = 0; i < Names.Length; i++)
                    slices.Add(Slice("House512_" + Names[i], new Rect(i % 4 * 128, (3 - i / 4) * 128, 128, 128)));
            else if (file == "Props")
                for (int i = 0; i < PropNames.Length; i++)
                {
                    // The wide dining table deliberately occupies extra transparent space in the last row.
                    Rect rect = i == 13 ? new Rect(132, 24, 170, 110)
                        : i == 14 ? new Rect(305, 24, 94, 108)
                        : new Rect(i % 4 * 128, (3 - i / 4) * 128, 128, 128);
                    slices.Add(Slice("House512_" + PropNames[i], rect));
                }
            else if (assetPath.EndsWith("Floor.png"))
                for (int i = 0; i < 16; i++)
                    slices.Add(Slice("House512_Floor_" + i, new Rect(i % 4 * 128, i / 4 * 128, 128, 128)));
            else if (file == "Chairs")
            {
                string[] directions = { "North", "East", "South", "West" };
                for (int i = 0; i < 4; i++)
                    slices.Add(Slice("House512_Chair" + directions[i], new Rect(i % 2 * 256, (1 - i / 2) * 256, 256, 256)));
            }
            else if (file == "Architecture")
            {
                slices.Add(Slice("House512_WallFace", new Rect(8, 272, 253, 207)));
                slices.Add(Slice("House512_WallEdge", new Rect(272, 272, 212, 234)));
                slices.Add(Slice("House512_WallCap", new Rect(8, 447, 253, 30)));
                slices.Add(Slice("House512_DoorOpenFront", new Rect(0, 0, 256, 256)));
                slices.Add(Slice("House512_DoorOpenSide", new Rect(256, 0, 256, 256)));
            }
            else if (file == "Yard")
            {
                var rects = new[] { new Rect(4, 391, 140, 85), new Rect(164, 390, 64, 120), new Rect(262, 390, 106, 108),
                    new Rect(380, 390, 124, 86), new Rect(10, 273, 113, 110), new Rect(135, 274, 112, 110),
                    new Rect(259, 275, 110, 108), new Rect(384, 256, 128, 128), new Rect(0, 161, 115, 109),
                    new Rect(116, 160, 136, 112), new Rect(256, 148, 128, 108), new Rect(384, 146, 128, 122),
                    new Rect(3, 16, 138, 144), new Rect(128, 0, 128, 128), new Rect(257, 10, 116, 127),
                    new Rect(380, 32, 119, 84) };
                for (int i = 0; i < rects.Length; i++)
                    slices.Add(Slice("House512_Yard" + YardNames[i], i >= 4 && i <= 6 ? rects[i] : Trim(texture, rects[i])));
            }
            else if (file == "EdelzioInteractions")
            {
                var walk = Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames()[0][0];
                var visibleWalk = Trim(walk.texture, walk.rect);
                float footY = (visibleWalk.yMin - walk.rect.yMin - walk.pivot.y) / walk.pixelsPerUnit;
                float ppu = Trim(texture, new Rect(0, 256, 128, 128)).height / (visibleWalk.height / walk.pixelsPerUnit);
                importer.spritePixelsPerUnit = ppu;
                for (int row = 0; row < 4; row++)
                for (int frame = 0; frame < 4; frame++)
                {
                    var cell = new Rect(frame * 128, (3 - row) * 128, 128, 128);
                    if (row == 2) cell = new Rect(frame * 128, 146, 128, 110);
                    if (row == 3) cell = new Rect(frame * 128, 0, 128, 146);
                    var rect = Trim(texture, cell);
                    var slice = Slice("Edelzio_Interaction_" + row + "_" + frame, rect);
                    slice.alignment = SpriteAlignment.Custom;
                    slice.pivot = row == 2 ? Vector2.one * .5f
                        : new Vector2((cell.center.x - rect.xMin) / rect.width, -footY * ppu / rect.height);
                    // Lift the pelvis onto the seat while keeping the player and collider at the chair anchor.
                    float seatLift = row == 0 ? .34f : row == 2 ? 0 : frame == 0 ? 0 : frame == 1 ? .17f : .34f;
                    slice.pivot -= new Vector2(0, seatLift * ppu / rect.height);
                    slices.Add(slice);
                }
            }
            else
                slices.Add(Slice(assetPath.EndsWith("SofaNorth.png") ? "House512_SofaNorth" : "House512_Wall",
                    assetPath.EndsWith("SofaNorth.png") ? new Rect(0, 128, 512, 256) : new Rect(0, 0, 512, 512)));
            // Preserve sprite IDs when refreshing an existing import.
            var existing = provider.GetSpriteRects();
            foreach (var slice in slices)
                foreach (var previous in existing)
                    if (previous.name == slice.name) slice.spriteID = previous.spriteID;
            provider.SetSpriteRects(slices.ToArray());
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if (names != null)
            {
                var pairs = new List<SpriteNameFileIdPair>();
                foreach (var slice in slices) pairs.Add(new SpriteNameFileIdPair(slice.name, slice.spriteID));
                names.SetNameFileIdPairs(pairs);
            }
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static SpriteRect Slice(string name, Rect rect) => new SpriteRect
        {
            name = name, rect = rect, alignment = SpriteAlignment.Center, pivot = Vector2.one * .5f,
            spriteID = GUID.Generate()
        };

        private static Rect Trim(Texture2D texture, Rect cell)
        {
            var pixels = texture.GetPixels32();
            int left = (int)cell.xMax, right = (int)cell.xMin, bottom = (int)cell.yMax, top = (int)cell.yMin;
            for (int y = (int)cell.yMin; y < cell.yMax; y++)
            for (int x = (int)cell.xMin; x < cell.xMax; x++)
            {
                if (pixels[y * texture.width + x].a < 64) continue;
                left = Mathf.Min(left, x); right = Mathf.Max(right, x + 1);
                bottom = Mathf.Min(bottom, y); top = Mathf.Max(top, y + 1);
            }
            return right > left && top > bottom ? Rect.MinMaxRect(left, bottom, right, top) : cell;
        }
    }
}
