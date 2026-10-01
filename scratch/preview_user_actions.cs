using var appearance = new Game.Varginha.EdelzioBackpackAppearance();
const int cell=144, scale=2;
var atlas=new UnityEngine.Texture2D(cell*4*scale,cell*4*scale,UnityEngine.TextureFormat.RGBA32,false);
var pixels=new UnityEngine.Color32[atlas.width*atlas.height];
int[] directions={2,3,0,2};
for(int r=0;r<4;r++)for(int f=0;f<4;f++)
{
    var body=Game.Varginha.VarginhaInteractionSprites.Frame(r,f);
    var frame=appearance.ComposeFrame(body,directions[r]);
    int w=(int)frame.rect.width,h=(int)frame.rect.height;
    var input=frame.texture.GetPixels((int)frame.rect.x,(int)frame.rect.y,w,h);
    for(int y=0;y<h*scale;y++)for(int x=0;x<w*scale;x++)
        pixels[((3-r)*cell*scale+y)*atlas.width+f*cell*scale+(cell-w)*scale/2+x]=input[y/scale*w+x/scale];
}
atlas.SetPixels32(pixels);atlas.Apply();
System.IO.File.WriteAllBytes("Docs/Previews/MochilaUsuarioInteracoes.png",atlas.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(atlas);
return "Coffee, back seating/typing, webcam, side seating: four frames each";

