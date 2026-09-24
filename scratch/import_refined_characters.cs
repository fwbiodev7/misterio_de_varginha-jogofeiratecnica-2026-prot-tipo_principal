System.IO.File.Copy(@"C:\Users\Usuario\.codex\generated_images\01a0d405-4341-7041-bd67-f73ec6058a81\exec-cc226e27-6829-48cd-9476-b93f2e7e90af.png", "Assets/ArtSource/Allies/Fabio.png", true);
System.IO.File.Copy(@"C:\Users\Usuario\.codex\generated_images\01a0d405-4341-7041-bd67-f73ec6058a81\exec-019c9779-4398-4b1e-b0ea-2d99c96ac296.png", "Assets/ArtSource/Allies/Edelzio.png", true);
UnityEditor.AssetDatabase.ImportAsset("Assets/ArtSource/Allies/Fabio.png", UnityEditor.ImportAssetOptions.ForceSynchronousImport);
UnityEditor.AssetDatabase.ImportAsset("Assets/ArtSource/Allies/Edelzio.png", UnityEditor.ImportAssetOptions.ForceSynchronousImport);
return "Imported both source sheets; production importer queued.";
