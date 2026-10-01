if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Render outside Play");
var scene=UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Additive);
var root=new UnityEngine.GameObject("IndustrialPreview");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);
var school=Game.Varginha.VarginhaEnvironmentArt.EnsureSchool(root.transform);
foreach(var t in root.GetComponentsInChildren<UnityEngine.Transform>(true))t.gameObject.layer=31;
var cam=new UnityEngine.GameObject("IndustrialPreviewCamera").AddComponent<UnityEngine.Camera>();
if(UnityEngine.Camera.main!=null)cam.CopyFrom(UnityEngine.Camera.main);
cam.enabled=false;cam.orthographic=true;cam.orthographicSize=4.6f;cam.aspect=2;
cam.clearFlags=UnityEngine.CameraClearFlags.SolidColor;cam.backgroundColor=new UnityEngine.Color(.04f,.06f,.08f,1);cam.cullingMask=1<<31;
cam.transform.position=new UnityEngine.Vector3(.4f,-6.2f,-10);
var rt=new UnityEngine.RenderTexture(1536,768,24,UnityEngine.RenderTextureFormat.ARGB32);
var texture=new UnityEngine.Texture2D(1536,768,UnityEngine.TextureFormat.RGBA32,false);
try {
 cam.targetTexture=rt;cam.Render();var previous=UnityEngine.RenderTexture.active;
 UnityEngine.RenderTexture.active=rt;texture.ReadPixels(new UnityEngine.Rect(0,0,1536,768),0,0);texture.Apply();UnityEngine.RenderTexture.active=previous;
 System.IO.File.WriteAllBytes("Docs/Previews/EscolaIndustrialEntrada.png",texture.EncodeToPNG());
 return "Industrial entrance preview saved from the game's imported pixel art and environment";
} finally {
 UnityEngine.Object.DestroyImmediate(cam.gameObject);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);
 UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);
}
