var go = new UnityEngine.GameObject("House512_PreviewCamera");
var camera = go.AddComponent<UnityEngine.Camera>();
camera.orthographic = true;
camera.orthographicSize = 9.3f;
camera.transform.position = new UnityEngine.Vector3(0, 0, -10);
camera.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
camera.backgroundColor = new UnityEngine.Color(.09f, .05f, .05f);
camera.allowHDR = false;
camera.allowMSAA = false;
var target = new UnityEngine.RenderTexture(1200, 1000, 24);
var previous = UnityEngine.RenderTexture.active;
var texture = new UnityEngine.Texture2D(1200, 1000, UnityEngine.TextureFormat.RGBA32, false);
try
{
    camera.targetTexture = target;
    camera.Render();
    UnityEngine.RenderTexture.active = target;
    texture.ReadPixels(new UnityEngine.Rect(0, 0, 1200, 1000), 0, 0);
    texture.Apply();
    System.IO.File.WriteAllBytes("Docs/Previews/CasaReferencia512.png", texture.EncodeToPNG());
}
finally
{
    camera.targetTexture = null;
    UnityEngine.RenderTexture.active = previous;
    UnityEngine.Object.DestroyImmediate(texture);
    UnityEngine.Object.DestroyImmediate(target);
    UnityEngine.Object.DestroyImmediate(go);
}
return "Docs/Previews/CasaReferencia512.png";
