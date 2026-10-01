var walk = Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames();
var attacks = Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames();
using var appearance = new Game.Varginha.EdelzioBackpackAppearance();
var atlas = new UnityEngine.Texture2D(1024,1024,UnityEngine.TextureFormat.RGBA32,false);
var pixels = new UnityEngine.Color32[1024*1024];
int[] frames={-1,3,9,15};
for(int d=0;d<4;d++)for(int f=0;f<4;f++)
{
    var body=frames[f]<0?walk[d][0]:attacks[d][frames[f]];
    var equipped=appearance.GetFrame(body,d);
    var input=equipped.texture.GetPixels((int)equipped.rect.x,(int)equipped.rect.y,64,64);
    for(int y=0;y<256;y++)for(int x=0;x<256;x++)
        pixels[((3-d)*256+y)*1024+f*256+x]=input[y/4*64+x/4];
}
atlas.SetPixels32(pixels);atlas.Apply();
System.IO.File.WriteAllBytes("Docs/Previews/MochilaUsuarioSocosAplicada.png",atlas.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(atlas);
return "Punch preview rendered from packaged game sprites";
