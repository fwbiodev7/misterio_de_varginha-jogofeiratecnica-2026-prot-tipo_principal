if(UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Leave Play before saving scene assets");
const string path="Assets/Scenes/Fase2_Escola_Resgate.unity";
var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
bool opened=!scene.IsValid() || !scene.isLoaded;
if(opened) scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path,UnityEditor.SceneManagement.OpenSceneMode.Additive);
try {
 UnityEngine.Transform school=null;
 foreach(var go in scene.GetRootGameObjects())foreach(var t in go.GetComponentsInChildren<UnityEngine.Transform>(true))if(t.name=="Escola_3_Sistema_Ambiente")school=t;
 if(school==null)throw new System.Exception("Saved school environment not found");
 Game.Varginha.VarginhaIndustrialSchoolFacade.Ensure(school);
 if(school.GetComponentInChildren<Game.Varginha.VarginhaIndustrialSchoolFacade>()==null)throw new System.Exception("Facade not installed");
 UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
 if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene))throw new System.Exception("Scene save failed");
 return "Industrial facade saved in phase 2; existing map retained";
} finally { if(opened)UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true); }
