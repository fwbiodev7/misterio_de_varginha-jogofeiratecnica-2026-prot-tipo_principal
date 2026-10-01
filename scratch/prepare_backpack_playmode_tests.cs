if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Wait for test cancellation to exit Play Mode");
UnityEditor.SessionState.SetBool("Varginha.PlayCurrentSceneOnce",true);
UnityEditor.SessionState.SetBool("Varginha.SuppressMenuForTests",true);
UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=null;
return "Use the generated test scene for this test run; normal menu configuration restores on exit";
