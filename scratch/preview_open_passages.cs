if(!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run with the investigation scene playing");
var house=System.Linq.Enumerable.First(UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(),t=>t.name=="House_And_Yard");
Game.Varginha.VarginhaHouseArchitecture.Apply(house);
var architecture=house.Find("House512_Architecture");
int before=architecture.childCount;
Game.Varginha.VarginhaHouseArchitecture.Apply(house);
if(before!=architecture.childCount) throw new System.Exception("Applying passage migration twice duplicated the frame");
foreach(var renderer in architecture.GetComponentsInChildren<UnityEngine.SpriteRenderer>())
    if(renderer.sprite!=null && renderer.sprite.name.StartsWith("House512_Door"))
        throw new System.Exception("An old house door remains: "+renderer.name);
foreach(var t in architecture.GetComponentsInChildren<UnityEngine.Transform>())
    if(t.name.StartsWith("Passagem_") && t.GetComponentsInChildren<UnityEngine.Collider2D>().Length>0)
        throw new System.Exception("Portal blocks the walkable opening");
UnityEngine.Physics2D.SyncTransforms();
var passageCenters=new[]{new UnityEngine.Vector2(-3.5f,0),new UnityEngine.Vector2(-2,-3),new UnityEngine.Vector2(9,0)};
for(int i=0;i<passageCenters.Length;i++)
{
    var direction=i==0?UnityEngine.Vector2.up:UnityEngine.Vector2.right;
    foreach(var hit in UnityEngine.Physics2D.CircleCastAll(passageCenters[i]-direction, .49f, direction, 2))
        if(hit.collider.transform.IsChildOf(architecture))throw new System.Exception("House wall blocks passage "+i+": "+hit.collider.name);
}
var temporary=UnityEngine.SceneManagement.SceneManager.CreateScene("Portais_Visual_QA",new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var schoolRoot=new UnityEngine.GameObject("SchoolPortalQA");
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(schoolRoot,temporary);
var school=Game.Varginha.VarginhaEnvironmentArt.EnsureSchool(schoolRoot.transform);
Game.Varginha.VarginhaSchoolExterior.Ensure(school);
var exterior=school.Find(Game.Varginha.VarginhaSchoolExterior.RootName);
if(exterior.Find("Porta_Aberta_Esquerda")!=null || exterior.Find("Porta_Aberta_Direita")!=null)
    throw new System.Exception("A school door leaf remains");
if(exterior.Find("Passagem_Entrada_Escola")==null)throw new System.Exception("Missing school portal");
foreach(var t in schoolRoot.GetComponentsInChildren<UnityEngine.Transform>())t.gameObject.layer=31;
const int size=384;
var cam=new UnityEngine.GameObject("PortalPreviewCamera").AddComponent<UnityEngine.Camera>();
cam.CopyFrom(UnityEngine.Camera.main);cam.enabled=false;cam.aspect=1;cam.orthographic=true;cam.orthographicSize=1.85f;
var rt=new UnityEngine.RenderTexture(size,size,24,UnityEngine.RenderTextureFormat.ARGB32);
var tile=new UnityEngine.Texture2D(size,size,UnityEngine.TextureFormat.RGBA32,false);
var montage=new UnityEngine.Texture2D(size*4,size,UnityEngine.TextureFormat.RGBA32,false);
var positions=new[]{new UnityEngine.Vector2(-3.5f,0),new UnityEngine.Vector2(-2,-3),new UnityEngine.Vector2(9,0),Game.Varginha.VarginhaSchoolExterior.Entrance};
try
{
    UnityEngine.Physics2D.SyncTransforms();
    var physics=UnityEngine.PhysicsSceneExtensions2D.GetPhysicsScene2D(temporary);
    var entry=Game.Varginha.VarginhaSchoolExterior.Entrance;
    var hit=physics.CircleCast(entry+UnityEngine.Vector2.down*2,.49f,UnityEngine.Vector2.up,4);
    if(hit.collider!=null)throw new System.Exception("School entrance blocked: "+hit.collider.name);
    if(physics.CircleCast(new UnityEngine.Vector2(-4,-7),.49f,UnityEngine.Vector2.up,3).collider==null)
        throw new System.Exception("Solid school facade lost its collision");
    cam.targetTexture=rt;
    for(int i=0;i<positions.Length;i++)
    {
        cam.cullingMask=i==3?1<<31:~(1<<31);
        cam.transform.position=new UnityEngine.Vector3(positions[i].x,positions[i].y,-10);
        cam.Render();
        var previous=UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active=rt;tile.ReadPixels(new UnityEngine.Rect(0,0,size,size),0,0);tile.Apply();
        UnityEngine.RenderTexture.active=previous;
        montage.SetPixels(i*size,0,size,size,tile.GetPixels());
    }
    montage.Apply();System.IO.Directory.CreateDirectory("Docs/Previews");
    System.IO.File.WriteAllBytes("Docs/Previews/PortaisAbertos.png",montage.EncodeToPNG());
}
finally
{
    UnityEngine.Object.DestroyImmediate(cam.gameObject);
    UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tile);UnityEngine.Object.DestroyImmediate(montage);
    UnityEngine.Object.DestroyImmediate(schoolRoot);
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(temporary);
}
return "Bedroom, office, yard and school open passages verified with player radius 0.49; solid school facade preserved; no old door sprites, added colliders or duplicate frames; preview saved";
