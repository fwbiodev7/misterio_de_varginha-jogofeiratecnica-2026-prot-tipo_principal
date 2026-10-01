if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Play required");
UnityEngine.Time.timeScale = 1;
UnityEngine.Application.runInBackground = true;
foreach (var controller in UnityEngine.Object.FindObjectsByType<Game.Varginha.VarginhaPhase2Controller>()) controller.enabled = false;
foreach (var enemy in UnityEngine.Object.FindObjectsByType<Game.Varginha.VarginhaCombatEnemy>()) enemy.enabled = false;
var leader = new UnityEngine.GameObject("QA_LeaderAtFusca").transform;
var car = new UnityEngine.GameObject("QA_ExternalFusca").transform;
car.position = Game.Varginha.VarginhaEnvironmentArt.FuscaParkingPosition;
leader.position = car.position + UnityEngine.Vector3.right * 3.8f;
var students = UnityEngine.Object.FindObjectsByType<Game.Varginha.VarginhaStudentHostage>(UnityEngine.FindObjectsInactive.Include);
int seated = 0;
for (int i = 0; i < students.Length; i++)
{
    if (students[i].GetComponent<Game.Varginha.VarginhaStudentAnimation>().IsSeated) seated++;
    students[i].ReleaseTo(car, i, leader);
}
return new { students = students.Length, seatedBeforeRelease = seated, destination = car.position };
