UnityEditor.AssetDatabase.Refresh();
var path = "Assets/Resources/Varginha/SchoolChairRearV1.png";
var image = new UnityEngine.Texture2D(2,2,UnityEngine.TextureFormat.RGBA32,false);
image.LoadImage(System.IO.File.ReadAllBytes(path));
int left=image.width, right=0, bottom=image.height, top=0;
for(int y=0;y<image.height;y++) for(int x=0;x<image.width;x++)
 if(image.GetPixel(x,y).a>.5f) {left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
var trimmed = new UnityEngine.Texture2D(right-left+1,top-bottom+1,UnityEngine.TextureFormat.RGBA32,false);
trimmed.SetPixels(image.GetPixels(left,bottom,trimmed.width,trimmed.height)); trimmed.Apply();
UnityEditor.AssetDatabase.ReleaseCachedFileHandles();
System.IO.File.WriteAllBytes(path,trimmed.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(trimmed);
UnityEditor.AssetDatabase.ImportAsset(path,UnityEditor.ImportAssetOptions.ForceUpdate);
Game.Editor.Testing.EdelzioPunchAtlasBuilder.Build();
foreach(var asset in new[]{path,"Assets/Resources/Varginha/EdelzioPunchV3.png"}) {
 var importer=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(asset);
 importer.textureType=UnityEditor.TextureImporterType.Sprite;
 importer.spriteImportMode=UnityEditor.SpriteImportMode.Single;
 importer.spritePixelsPerUnit=128;
 importer.filterMode=UnityEngine.FilterMode.Point;
 importer.mipmapEnabled=false; importer.isReadable=true; importer.alphaIsTransparency=true;
 importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;
 importer.npotScale=UnityEditor.TextureImporterNPOTScale.None;
 importer.maxTextureSize=2048; importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;
 var settings=new UnityEditor.TextureImporterSettings(); importer.ReadTextureSettings(settings);
 settings.spriteMeshType=UnityEngine.SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape=false;
 importer.SetTextureSettings(settings); importer.SaveAndReimport();
}
Game.Varginha.VarginhaReferenceSprites.ClearCache();
var report = new System.Text.StringBuilder();
var walk = Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames();
var attack = UnityEngine.Resources.Load<UnityEngine.Texture2D>("Varginha/EdelzioPunchV3");
for(int d=0;d<4;d++) {
 foreach(bool idle in new[]{true,false}) {
  var p= idle ? walk[d][0].texture.GetPixels((int)walk[d][0].rect.x,(int)walk[d][0].rect.y,64,64) : attack.GetPixels(3*64,(3-d)*64,64,64);
  report.AppendLine("d="+d+" idle="+idle);
  for(int y=6;y<53;y++) {
   int a=64,b=0,shirt=0;
   for(int x=0;x<64;x++) {var c=p[y*64+x];if(c.a<.5f)continue;a=Mathf.Min(a,x);b=Mathf.Max(b,x);if(c.r>.43f && c.g>.27f && c.r>c.g*1.15f && c.b<c.g*.58f)shirt++;}
   if(b>=a)report.AppendLine(y+":"+a+"-"+b+" shirt="+shirt);
  }
 }
}
return report.ToString();
