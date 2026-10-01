if (System.IO.File.Exists("Assets/Resources/Varginha/Equipment/EdelzioBackpackPunchBaselineV3.png"))
    return "Original baseline already preserved; do not regenerate it from retouched frames";
using var appearance=new Game.Varginha.EdelzioBackpackAppearance();
var baseline=new UnityEngine.Texture2D(256,256,UnityEngine.TextureFormat.RGBA32,false);
int[] samples={-1,3,9,15};
for(int d=0;d<4;d++)for(int s=0;s<4;s++) {
 var body=s==0?Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames()[d][0]:Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames()[d][samples[s]];
 var frame=appearance.ComposeFrame(body,d);
 baseline.SetPixels(s*64,(3-d)*64,64,64,frame.texture.GetPixels((int)frame.rect.x,(int)frame.rect.y,64,64));
}
baseline.Apply();const string path="Assets/Resources/Varginha/Equipment/EdelzioBackpackPunchBaselineV3.png";
UnityEditor.AssetDatabase.ReleaseCachedFileHandles();System.IO.File.WriteAllBytes(path,baseline.EncodeToPNG());
UnityEditor.AssetDatabase.ImportAsset(path,UnityEditor.ImportAssetOptions.ForceSynchronousImport);
var importer=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
importer.textureType=UnityEditor.TextureImporterType.Default;importer.isReadable=true;importer.filterMode=UnityEngine.FilterMode.Point;
importer.mipmapEnabled=false;importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;
importer.npotScale=UnityEditor.TextureImporterNPOTScale.None;importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;importer.SaveAndReimport();
UnityEngine.Object.DestroyImmediate(baseline);return "Original punch preview baseline preserved to isolate manual edits";
