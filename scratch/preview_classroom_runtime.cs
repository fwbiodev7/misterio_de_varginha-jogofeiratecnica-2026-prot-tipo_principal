if(!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Play required");
foreach(var enemy in UnityEngine.Object.FindObjectsByType<Game.Varginha.VarginhaCombatEnemy>(UnityEngine.FindObjectsInactive.Include)) enemy.enabled=false;
var player=UnityEngine.Object.FindAnyObjectByType<Game.Varginha.EdelzioTopDownController>();
player.SetInputLocked(true);
var camera=UnityEngine.Camera.main;
camera.GetComponent<Game.Level.CameraFollow2D>().enabled=false;
camera.transform.position=new UnityEngine.Vector3(.5f,-3.8f,-10);
camera.orthographicSize=10.8f;
UnityEngine.Time.timeScale=0;
var report=new System.Text.StringBuilder();
foreach(var student in UnityEngine.Object.FindObjectsByType<Game.Varginha.VarginhaStudentHostage>(UnityEngine.FindObjectsInactive.Include))
 report.AppendLine(student.StudentName+" seated="+student.GetComponent<Game.Varginha.VarginhaStudentAnimation>().IsSeated+" pos="+student.transform.position);
foreach(var c in UnityEngine.Object.FindObjectsByType<UnityEngine.BoxCollider2D>())
 if(c.name.StartsWith("CenarioV2_Carteira_")) report.AppendLine(c.name+" width="+c.bounds.size.x);
return report.ToString();
