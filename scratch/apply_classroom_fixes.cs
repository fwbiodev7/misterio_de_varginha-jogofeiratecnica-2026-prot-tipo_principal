var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Stop play before saving fixes");
var school=UnityEngine.GameObject.Find("Escola_3_Sistema_Ambiente").transform;
UnityEditor.Undo.RegisterFullObjectHierarchyUndo(school.gameObject,"Corrigir laboratório e postes");
Game.Varginha.VarginhaSchoolClassroomLayout.RefreshSprites(school);
Game.Varginha.VarginhaOutdoorNight.EnsureSchool(school);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
Game.Varginha.VarginhaReferenceSprites.ClearCache();
var frames=Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames();
var walk=Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames();
var atlas=new UnityEngine.Texture2D(7*64,4*64,UnityEngine.TextureFormat.RGBA32,false);
for(int d=0;d<4;d++)for(int c=0;c<7;c++){
 var sprite=c==0?walk[d][0]:frames[d][new[]{0,1,2,3,8,14}[c-1]];
 atlas.SetPixels(c*64,(3-d)*64,64,64,sprite.texture.GetPixels((int)sprite.rect.x,(int)sprite.rect.y,64,64));
}
atlas.Apply();System.IO.File.WriteAllBytes("Docs/Previews/EdelzioSocosMesmoCorpo.png",atlas.EncodeToPNG());
UnityEngine.Object.DestroyImmediate(atlas);
return "Saved " + scene.path + "; 12 corrected chairs; idle/punch comparison exported.";

