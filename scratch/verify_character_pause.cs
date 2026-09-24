var results = new System.Collections.Generic.List<string>();
var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
System.Type studentTests = null, referenceTests = null;
foreach (var assembly in assemblies)
{
    studentTests = studentTests ?? assembly.GetType("Game.Tests.EditMode.VarginhaStudentSpriteTests");
    referenceTests = referenceTests ?? assembly.GetType("Game.Tests.EditMode.VarginhaReferenceArtTests");
}
if (studentTests == null || referenceTests == null) throw new System.Exception("Art test assemblies unavailable");
var tests = System.Activator.CreateInstance(studentTests);
foreach (string name in new[] { "Yasmin", "Pedro", "Matias", "Fabio", "Marcos", "Anna Sabia", "Ana Tavares", "Luis Miguel Messias", "Luis Martins", "Edelzio" })
{
    studentTests.GetMethod("EveryStudentUsesTheCanonicalAtlasForWalkingAndPortraits").Invoke(tests, new object[] { name });
    results.Add(name + ": all 16 frames distinct, feet aligned, point filtering, portrait uses front row.");
}
referenceTests.GetMethod("EdelzioAndFabioSheetsGenerateValidDirectionalFrames").Invoke(System.Activator.CreateInstance(referenceTests), null);
results.Add("Edelzio/Fabio production animation references: PASS");
var hud = Game.Varginha.VarginhaGameHUD.Instance;
bool wasPaused = hud.IsPaused;
float previousTimeScale = UnityEngine.Time.timeScale;
try
{
    if (!hud.IsPaused) hud.TogglePause();
    if (UnityEngine.Time.timeScale != 0f) throw new System.Exception("Pause did not freeze time");
    hud.ResumePause();
    if (UnityEngine.Time.timeScale != 1f || hud.IsPaused) throw new System.Exception("Resume failed");
    hud.TogglePause();
    results.Add("Pause/resume state and game time: PASS");
}
finally
{
    if (hud.IsPaused != wasPaused) hud.TogglePause();
    UnityEngine.Time.timeScale = previousTimeScale;
}
System.IO.File.WriteAllLines("Logs/CharacterPauseQA/validation.txt", results);
return results;
