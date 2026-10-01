var source=new UnityEngine.Texture2D(2,2,UnityEngine.TextureFormat.RGBA32,false);
source.LoadImage(System.IO.File.ReadAllBytes("Assets/ArtSource/EdelzioBackpackUserPunchV3.png"));
if(source.width!=1024 || source.height!=1024) throw new System.Exception("Expected 4x4 punch preview at 4x scale");
var atlas=new UnityEngine.Texture2D(256,256,UnityEngine.TextureFormat.RGBA32,false);
for(int y=0;y<256;y++)for(int x=0;x<256;x++)atlas.SetPixel(x,y,source.GetPixel(x*4+2,y*4+2));
atlas.Apply();
const string path="Assets/Resources/Varginha/Equipment/EdelzioBackpackUserPunchV3.png";
UnityEditor.AssetDatabase.ReleaseCachedFileHandles();
System.IO.File.WriteAllBytes(path,atlas.EncodeToPNG());
UnityEditor.AssetDatabase.ImportAsset(path,UnityEditor.ImportAssetOptions.ForceSynchronousImport);
var importer=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
importer.textureType=UnityEditor.TextureImporterType.Default;importer.isReadable=true;
importer.filterMode=UnityEngine.FilterMode.Point;importer.mipmapEnabled=false;
importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;
importer.npotScale=UnityEditor.TextureImporterNPOTScale.None;importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;
importer.SaveAndReimport();
UnityEditor.AssetDatabase.ImportAsset("Assets/ArtSource/EdelzioBackpackUserPunchV3.png");
using var appearance=new Game.Varginha.EdelzioBackpackAppearance();
int[] samples={-1,3,9,15};
var report=new System.Text.StringBuilder();
for(int d=0;d<4;d++)for(int s=0;s<4;s++) {
 var body=s==0?Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames()[d][0]:Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames()[d][samples[s]];
 var old=appearance.ComposeFrame(body,d);
 var input=old.texture.GetPixels((int)old.rect.x,(int)old.rect.y,64,64);
 var bare=body.texture.GetPixels((int)body.rect.x,(int)body.rect.y,64,64);
 var edited=atlas.GetPixels(s*64,(3-d)*64,64,64);
 int changed=0,head=0,feet=0,bareChanges=0,removed=0;
 for(int i=0;i<4096;i++) {
  bool different(UnityEngine.Color a,UnityEngine.Color b)=> (a.a>.5f || b.a>.5f) && (UnityEngine.Mathf.Abs(a.r-b.r)>3f/255 || UnityEngine.Mathf.Abs(a.g-b.g)>3f/255 || UnityEngine.Mathf.Abs(a.b-b.b)>3f/255 || UnityEngine.Mathf.Abs(a.a-b.a)>3f/255);
  if(different(edited[i],bare[i]))bareChanges++;
  if(!different(edited[i],input[i])) continue;
  changed++;if(i/64>=39)head++;if(i/64<17)feet++;
  if(edited[i].a<.5f && input[i].a>.5f)removed++;
 }
 report.AppendLine($"d={d} sample={samples[s]} edits={changed},head={head},feet={feet},equipmentVsBare={bareChanges},removed={removed}");
}
UnityEngine.Object.DestroyImmediate(atlas);UnityEngine.Object.DestroyImmediate(source);
return report.ToString();
