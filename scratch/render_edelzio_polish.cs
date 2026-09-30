var appearance = new Game.Varginha.EdelzioBackpackAppearance();
var walk = Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames();
var target = new UnityEngine.RenderTexture(1000, 600, 0);
var output = new UnityEngine.Texture2D(1000, 600, UnityEngine.TextureFormat.RGBA32, false);
var material = new UnityEngine.Material(UnityEngine.Shader.Find("Unlit/Transparent"));
var previous = UnityEngine.RenderTexture.active;
try
{
    UnityEngine.RenderTexture.active = target;
    UnityEngine.GL.Clear(true, true, new UnityEngine.Color(.13f, .105f, .09f));
    UnityEngine.GL.PushMatrix();
    UnityEngine.GL.LoadPixelMatrix(0, 1000, 600, 0);
    for (int row = 0; row < 2; row++)
    for (int direction = 0; direction < 4; direction++)
    {
        var s = row == 0 ? walk[direction][0] : appearance.GetFrame(walk[direction][0], direction);
        var uv = new UnityEngine.Rect(s.rect.x / s.texture.width, s.rect.y / s.texture.height,
            s.rect.width / s.texture.width, s.rect.height / s.texture.height);
        UnityEngine.Graphics.DrawTexture(new UnityEngine.Rect(25 + direction * 250, 30 + row * 275, 256, 256),
            s.texture, uv, 0, 0, 0, 0, UnityEngine.Color.white, material);
    }
    UnityEngine.GL.PopMatrix();
    output.ReadPixels(new UnityEngine.Rect(0, 0, 1000, 600), 0, 0); output.Apply();
    System.IO.File.WriteAllBytes("Docs/Previews/EdelzioDetalhesFoto.png", output.EncodeToPNG());
}
finally
{
    UnityEngine.RenderTexture.active = previous;
    appearance.Dispose();
    UnityEngine.Object.DestroyImmediate(material);
    UnityEngine.Object.DestroyImmediate(output);
    UnityEngine.Object.DestroyImmediate(target);
}
return "Exported four directions with and without the collected backpack.";
