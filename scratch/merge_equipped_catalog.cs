var entries=new System.Collections.Generic.List<Game.Varginha.EdelzioBackpackFrames.Entry>();
for(int i=0;i<3;i++)entries.AddRange(UnityEngine.JsonUtility.FromJson<Game.Varginha.EdelzioBackpackFrames.Catalog>(System.IO.File.ReadAllText("scratch/equipped-catalog-"+i+".json")).frames);
if(entries.Count!=304)throw new System.Exception("Expected 304 frames");
const string path="Assets/Resources/Varginha/Equipment/EdelzioEquippedV1.json";
UnityEditor.AssetDatabase.ReleaseCachedFileHandles();
System.IO.File.WriteAllText(path,UnityEngine.JsonUtility.ToJson(new Game.Varginha.EdelzioBackpackFrames.Catalog{frames=entries.ToArray()},true));
UnityEditor.AssetDatabase.ImportAsset(path,UnityEditor.ImportAssetOptions.ForceSynchronousImport);
Game.Varginha.EdelzioBackpackFrames.ClearCache();
return "Catalog rebuilt: 304 equipped frames";
