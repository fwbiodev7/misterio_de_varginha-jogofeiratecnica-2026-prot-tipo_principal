var source=new UnityEngine.Texture2D(2,2,UnityEngine.TextureFormat.RGBA32,false);
source.LoadImage(System.IO.File.ReadAllBytes("Assets/ArtSource/SchoolIndustrialFacadeV1.png"));
int w=source.width/4,h=source.height/4;
var atlas=new UnityEngine.Texture2D(w,h,UnityEngine.TextureFormat.RGBA32,false);
for(int y=0;y<h;y++)for(int x=0;x<w;x++)atlas.SetPixel(x,y,source.GetPixel(x*4+2,y*4+2));
atlas.Apply();const string path="Assets/Resources/Varginha/SchoolIndustrialFacadeV1.png";
UnityEditor.AssetDatabase.ReleaseCachedFileHandles();System.IO.File.WriteAllBytes(path,atlas.EncodeToPNG());
UnityEditor.AssetDatabase.ImportAsset(path,UnityEditor.ImportAssetOptions.ForceSynchronousImport);
var importer=(UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
importer.textureType=UnityEditor.TextureImporterType.Default;importer.isReadable=true;importer.filterMode=UnityEngine.FilterMode.Point;
importer.mipmapEnabled=false;importer.textureCompression=UnityEditor.TextureImporterCompression.Uncompressed;
importer.npotScale=UnityEditor.TextureImporterNPOTScale.None;importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;importer.SaveAndReimport();
UnityEditor.AssetDatabase.ImportAsset("Assets/ArtSource/SchoolIndustrialFacadeV1.png");
int left=w,right=-1,bottom=h,top=-1;
for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(atlas.GetPixel(x,y).a>.5f){left=System.Math.Min(left,x);right=System.Math.Max(right,x);bottom=System.Math.Min(bottom,y);top=System.Math.Max(top,y);}
int line=bottom+(top-bottom)/4,bestStart=-1,bestWidth=0,runStart=-1;
for(int x=w*35/100;x<w*70/100;x++){
 if(atlas.GetPixel(x,line).a<.5f){if(runStart<0)runStart=x;int length=x-runStart+1;if(length>bestWidth){bestStart=runStart;bestWidth=length;}}
 else runStart=-1;
}
if(bestWidth<20)throw new System.Exception("Missing transparent open gate");
var rects=new[]{new UnityEngine.RectInt(left,bottom,bestStart-left,top-bottom+1),new UnityEngine.RectInt(bestStart+bestWidth,bottom,right-(bestStart+bestWidth)+1,top-bottom+1)};
System.IO.File.WriteAllText("scratch/school-facade-crops.json",UnityEngine.JsonUtility.ToJson(new Game.Varginha.EdelzioBackpackFrames.Catalog{frames=new[]{new Game.Varginha.EdelzioBackpackFrames.Entry{x=rects[0].x,y=rects[0].y,width=rects[0].width,height=rects[0].height},new Game.Varginha.EdelzioBackpackFrames.Entry{x=rects[1].x,y=rects[1].y,width=rects[1].width,height=rects[1].height}}},true));
UnityEngine.Object.DestroyImmediate(atlas);UnityEngine.Object.DestroyImmediate(source);
return new{w,h,left,right,bottom,top,line,bestStart,bestWidth,rects};
