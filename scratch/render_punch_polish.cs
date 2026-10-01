var attacks = Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames();
var frames = new[] { 2, 3, 8, 9, 14, 15 };
var target = new UnityEngine.RenderTexture(768, 512, 0);
var output = new UnityEngine.Texture2D(768, 512, UnityEngine.TextureFormat.RGBA32, false);
var material = new UnityEngine.Material(UnityEngine.Shader.Find("Unlit/Transparent"));
var previous = UnityEngine.RenderTexture.active;
try
{
    UnityEngine.RenderTexture.active = target;
    UnityEngine.GL.Clear(true, true, new UnityEngine.Color(.13f, .105f, .09f));
    UnityEngine.GL.PushMatrix();
    UnityEngine.GL.LoadPixelMatrix(0, 768, 512, 0);
    for (int direction = 0; direction < 4; direction++)
    for (int column = 0; column < frames.Length; column++)
    {
        var sprite = attacks[direction][frames[column]];
        UnityEngine.Graphics.DrawTexture(new UnityEngine.Rect(column * 128, direction * 128, 128, 128),
            sprite.texture, new UnityEngine.Rect(0, 0, 1, 1), 0, 0, 0, 0, UnityEngine.Color.white, material);
    }
    UnityEngine.GL.PopMatrix();
    output.ReadPixels(new UnityEngine.Rect(0, 0, 768, 512), 0, 0); output.Apply();
    System.IO.File.WriteAllBytes("Docs/Previews/EdelzioSocosAtual.png", output.EncodeToPNG());
}
finally
{
    UnityEngine.RenderTexture.active = previous;
    UnityEngine.Object.DestroyImmediate(material);
    UnityEngine.Object.DestroyImmediate(output);
    UnityEngine.Object.DestroyImmediate(target);
}
return "Saved all three combos in four directions.";
