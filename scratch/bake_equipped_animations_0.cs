int group = 0;
var poses = new System.Collections.Generic.List<(UnityEngine.Sprite body, int direction)>();
string sheet = group == 0 ? "EdelzioEquippedWalkV1" : group == 1 ? "EdelzioEquippedPunchV1" : "EdelzioEquippedActionsV1";
int size = group == 2 ? 144 : 64, columns = group == 0 ? 4 : group == 1 ? 18 : 8;
for (int d = 0; d < 8; d++)
{
    int cardinal = d < 4 ? d : d < 6 ? 0 : 3;
    if (group < 2)
    {
        var frames = group == 0 ? Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames()[cardinal]
            : Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames()[cardinal];
        foreach (var body in frames) poses.Add((body,d));
    }
    else
        for (int r = 0; r < 4; r++) for (int f = 0; f < 4; f++)
            poses.Add((Game.Varginha.VarginhaInteractionSprites.Frame(r,f),d));
}
int rows = (poses.Count+columns-1)/columns;
var texture = new UnityEngine.Texture2D(columns*size,rows*size,UnityEngine.TextureFormat.RGBA32,false);
texture.SetPixels(new UnityEngine.Color[texture.width*texture.height]);
var entries = new System.Collections.Generic.List<Game.Varginha.EdelzioBackpackFrames.Entry>();
using var appearance = new Game.Varginha.EdelzioBackpackAppearance();
for (int i=0;i<poses.Count;i++)
{
    var body=poses[i].body;
    var frame=appearance.ComposeFrame(body,poses[i].direction);
    int x=i%columns*size, y=(rows-1-i/columns)*size, w=(int)body.rect.width, h=(int)body.rect.height;
    if(w>size || h>size) throw new System.Exception("Pose exceeds atlas cell: "+body.name);
    texture.SetPixels(x,y,w,h,frame.texture.GetPixels((int)frame.rect.x,(int)frame.rect.y,w,h));
    entries.Add(new Game.Varginha.EdelzioBackpackFrames.Entry
    {
        source=body.name,sheet=sheet,direction=poses[i].direction,x=x,y=y,width=w,height=h,
        pivotX=body.pivot.x/w,pivotY=body.pivot.y/h,pixelsPerUnit=body.pixelsPerUnit
    });
}
texture.Apply();
string path="Assets/Resources/Varginha/Equipment/"+sheet+".png";
UnityEditor.AssetDatabase.ReleaseCachedFileHandles();
System.IO.File.WriteAllBytes(path,texture.EncodeToPNG());
UnityEditor.AssetDatabase.ImportAsset(path,UnityEditor.ImportAssetOptions.ForceSynchronousImport);
var importer=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
importer.textureType=UnityEditor.TextureImporterType.Default;
importer.isReadable=true;importer.mipmapEnabled=false;importer.filterMode=UnityEngine.FilterMode.Point;
importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;
importer.npotScale=UnityEditor.TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;importer.SaveAndReimport();
System.IO.File.WriteAllText("scratch/equipped-catalog-"+group+".json",UnityEngine.JsonUtility.ToJson(new Game.Varginha.EdelzioBackpackFrames.Catalog{frames=entries.ToArray()},true));
UnityEngine.Object.DestroyImmediate(texture);
return sheet+": "+poses.Count+" complete equipped animation frames";

