var player = UnityEngine.Object.FindAnyObjectByType<Game.Varginha.EdelzioTopDownController>();
var playerRenderer = player.GetComponent<UnityEngine.SpriteRenderer>();
var originalSprite = playerRenderer.sprite;
var originalColor = playerRenderer.color;
var entity = UnityEngine.Object.FindAnyObjectByType<Game.Varginha.EntityManifestationAI>();
var entityRenderer = entity != null ? entity.GetComponent<UnityEngine.SpriteRenderer>() : null;
bool entityVisible = entityRenderer != null && entityRenderer.enabled;
// The entity stays hidden in the playable scene until the chest awakens it.
if (entityRenderer != null) entityRenderer.enabled = false;
playerRenderer.sprite = Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames()[0][0];
playerRenderer.color = UnityEngine.Color.white;
try
{
    foreach (bool fullMap in new[] { false, true })
    {
        int width = fullMap ? 2200 : 1200, height = fullMap ? 1100 : 1000;
        var go = new UnityEngine.GameObject("Temporary_House_Preview");
        var camera = go.AddComponent<UnityEngine.Camera>();
        camera.orthographic = true;
        camera.orthographicSize = fullMap ? 9.65f : 9.3f;
        camera.transform.position = new UnityEngine.Vector3(fullMap ? 9 : 0, 0, -10);
        camera.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
        camera.backgroundColor = new UnityEngine.Color(.09f, .05f, .05f);
        camera.allowHDR = false;
        camera.allowMSAA = false;
        var target = new UnityEngine.RenderTexture(width, height, 24);
        var texture = new UnityEngine.Texture2D(width, height, UnityEngine.TextureFormat.RGBA32, false);
        var previous = UnityEngine.RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            UnityEngine.RenderTexture.active = target;
            texture.ReadPixels(new UnityEngine.Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            string name = fullMap ? "CasaEQuintal512" : "CasaReferencia512";
            System.IO.File.WriteAllBytes("Docs/Previews/" + name + ".png", texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null;
            UnityEngine.RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
finally
{
    playerRenderer.sprite = originalSprite;
    playerRenderer.color = originalColor;
    if (entityRenderer != null) entityRenderer.enabled = entityVisible;
}
return "Saved final house and yard previews.";
