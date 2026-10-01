if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Wait for tests to exit");
const string temporary="Assets/InitTestScene5482103f-88ea-43f4-a158-238a94b4694e.unity";
if(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path==temporary)
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/FaseTopView_Varginha.unity");
if(System.IO.File.Exists(temporary))UnityEditor.AssetDatabase.DeleteAsset(temporary);
UnityEditor.SessionState.EraseBool("Varginha.PlayCurrentSceneOnce");
UnityEditor.SessionState.EraseBool("Varginha.SuppressMenuForTests");
UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>("Assets/Scenes/Menu_MisterioDeVarginha.unity");
Game.Varginha.EdelzioBackpackFrames.ClearCache();
return new{activeScene=UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path,startScene=UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene?.name};
