var source = new UnityEngine.Texture2D(2, 2, UnityEngine.TextureFormat.RGBA32, false);
source.LoadImage(System.IO.File.ReadAllBytes("Assets/ArtSource/EdelzioBackpackRemasterSourceV1.png"));
const int cell = 32;
var atlas = new UnityEngine.Texture2D(cell * 4, cell * 2, UnityEngine.TextureFormat.RGBA32, false);
atlas.SetPixels(new UnityEngine.Color[cell * 4 * cell * 2]);
for(int i = 0; i < 8; i++)
{
    int x0 = i % 4 * source.width / 4, x1 = (i % 4 + 1) * source.width / 4;
    int y0 = (1 - i / 4) * source.height / 2, y1 = (2 - i / 4) * source.height / 2;
    int left = x1, right = x0, bottom = y1, top = y0;
    for(int y = y0; y < y1; y++) for(int x = x0; x < x1; x++)
        if(source.GetPixel(x,y).a >= .5f) {left = Mathf.Min(left,x); right = Mathf.Max(right,x); bottom = Mathf.Min(bottom,y); top = Mathf.Max(top,y);}
    // The independent shoulder layer supplies the on-character strap; omit the
    // floating loop from the isolated side-view equipment art.
    if(i == 2) right = left + Mathf.RoundToInt((right-left+1)*.68f);
    if(i == 3) left = right - Mathf.RoundToInt((right-left+1)*.68f);
    int h = 28, w = Mathf.Min(28, Mathf.RoundToInt(h * (right-left+1f)/(top-bottom+1f)));
    for(int y=0;y<h;y++) for(int x=0;x<w;x++)
    {
        var color = source.GetPixel(left + Mathf.FloorToInt((x+.5f)*(right-left+1)/w), bottom + Mathf.FloorToInt((y+.5f)*(top-bottom+1)/h));
        if(color.a<.5f) color=UnityEngine.Color.clear; else color.a=1;
        atlas.SetPixel(i%4*cell+(cell-w)/2+x,(1-i/4)*cell+2+y,color);
    }
}
atlas.Apply();
const string path = "Assets/Resources/Varginha/Equipment/BackpackRemasterAtlasV1.png";
UnityEditor.AssetDatabase.ReleaseCachedFileHandles();
System.IO.File.WriteAllBytes(path, atlas.EncodeToPNG());
UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceSynchronousImport);
var importer = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
importer.textureType = UnityEditor.TextureImporterType.Default;
importer.isReadable = true;
importer.filterMode = UnityEngine.FilterMode.Point;
importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
importer.mipmapEnabled = false;
importer.npotScale = UnityEditor.TextureImporterNPOTScale.None;
importer.maxTextureSize = 256;
importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
importer.SaveAndReimport();
UnityEngine.Object.DestroyImmediate(source); UnityEngine.Object.DestroyImmediate(atlas);
return "Eight remastered backpack views imported into Resources, 128x64 point-filtered atlas";
