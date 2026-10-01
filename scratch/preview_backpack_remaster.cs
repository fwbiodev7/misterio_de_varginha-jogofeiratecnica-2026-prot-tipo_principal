var walk = Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames();
var attack = Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames();
using var appearance = new Game.Varginha.EdelzioBackpackAppearance();
const int scale = 4, cell = 64*scale;
var atlas = new UnityEngine.Texture2D(cell*4, cell*2, UnityEngine.TextureFormat.RGBA32, false);
var pixels = new UnityEngine.Color32[atlas.width*atlas.height];
for(int d=0;d<8;d++)
{
    int cardinal = d<4? d : d<6? 0 : 3;
    var body = appearance.GetFrame(walk[cardinal][0],d);
    var input = body.texture.GetPixels((int)body.rect.x,(int)body.rect.y,64,64);
    for(int y=0;y<cell;y++) for(int x=0;x<cell;x++)
        pixels[((1-d/4)*cell+y)*atlas.width+d%4*cell+x] = input[y/scale*64+x/scale];
}
atlas.SetPixels32(pixels);atlas.Apply();
System.IO.Directory.CreateDirectory("Docs/Previews");
System.IO.File.WriteAllBytes("Docs/Previews/MochilaRemasterizada8Direcoes.png",atlas.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(atlas);
return "Eight views: S, W, E, N / SW, SE, NW, NE";
