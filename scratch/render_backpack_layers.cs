var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
var atlas = new UnityEngine.Texture2D(1024,512,UnityEngine.TextureFormat.RGBA32,false);
var output = new UnityEngine.Color[1024*512];
var rt = new UnityEngine.RenderTexture(64,64,0,UnityEngine.RenderTextureFormat.ARGB32);
var read = new UnityEngine.Texture2D(64,64,UnityEngine.TextureFormat.RGBA32,false);
using var equipment = new Game.Varginha.EdelzioBackpackAppearance();
try
{
    var go = new UnityEngine.GameObject("BackpackVisualQA");
    go.layer=31;
    var sr = go.AddComponent<UnityEngine.SpriteRenderer>();
    var cam = new UnityEngine.GameObject("BackpackQACamera").AddComponent<UnityEngine.Camera>();
    cam.enabled=false;cam.orthographic=true;cam.clearFlags=UnityEngine.CameraClearFlags.SolidColor;
    cam.backgroundColor=UnityEngine.Color.clear;cam.cullingMask=1<<31;cam.targetTexture=rt;
    var walk=Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames();
    for(int d=0;d<8;d++)
    {
        sr.sprite=equipment.GetFrame(walk[d<4?d:d<6?0:3][0],d);
        cam.orthographicSize=32/sr.sprite.pixelsPerUnit;
        cam.transform.position=sr.bounds.center+new UnityEngine.Vector3(0,0,-10);
        cam.Render();
        var oldRt=UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active=rt;
        read.ReadPixels(new UnityEngine.Rect(0,0,64,64),0,0);read.Apply();
        UnityEngine.RenderTexture.active=oldRt;
        var input=read.GetPixels();
        for(int y=0;y<256;y++)for(int x=0;x<256;x++)
            output[((1-d/4)*256+y)*1024+d%4*256+x]=input[y/4*64+x/4];
    }
    atlas.SetPixels(output);atlas.Apply();
    System.IO.File.WriteAllBytes("Docs/Previews/MochilaRemasterizada8Direcoes.png",atlas.EncodeToPNG());
}
finally
{
    equipment.Dispose();
    UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(read);UnityEngine.Object.DestroyImmediate(atlas);
    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);
    UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
}
return "Eight complete equipped sprites rendered; QA scene removed";
