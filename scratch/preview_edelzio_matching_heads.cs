Game.Varginha.VarginhaReferenceSprites.ClearCache();
var idle = Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames();
var attack = Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames();
using var pack = new Game.Varginha.EdelzioBackpackAppearance();
const int zoom = 3, cell = 64 * zoom;
var texture = new UnityEngine.Texture2D(cell * 4, cell * 4, UnityEngine.TextureFormat.RGBA32, false);
var pixels = new UnityEngine.Color32[texture.width * texture.height];
for (int d = 0; d < 4; d++)
for (int c = 0; c < 4; c++)
{
    var body = c == 0 ? idle[d][0] : attack[d][new[] { 1, 3, 5 }[c - 1]];
    var sprite = pack.GetFrame(body, d);
    var source = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, 64, 64);
    for (int y = 0; y < cell; y++)
    for (int x = 0; x < cell; x++)
        pixels[((3 - d) * cell + y) * texture.width + c * cell + x] = source[y / zoom * 64 + x / zoom];
}
texture.SetPixels32(pixels); texture.Apply();
System.IO.File.WriteAllBytes("Docs/Previews/EdelzioCabecaComparacao.png", texture.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(texture);
return "Idle, windup, contact and return: four directions with identical heads and stable backpack";
