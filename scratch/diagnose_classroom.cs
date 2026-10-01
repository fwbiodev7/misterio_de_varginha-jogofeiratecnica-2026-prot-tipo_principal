var report = new System.Text.StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
report.AppendLine("STATE " + scene.path + " play=" + EditorApplication.isPlaying + " dirty=" + scene.isDirty + " pipeline=" + (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.GetType().Name ?? "BuiltIn"));
foreach(var c in UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include)) {
 if(c.name.Contains("Parede") || c.name.Contains("Wall") || c.name.Contains("Cadeira") || c.name.Contains("Carteira") || c.name.Contains("Lamp") || c.name.Contains("Port") || c.name.Contains("Colisor"))
 report.AppendLine("COL " + c.name + " active=" + c.gameObject.activeInHierarchy + " enabled=" + c.enabled + " trigger=" + c.isTrigger + " center=" + c.bounds.center + " size=" + c.bounds.size + " parent=" + c.transform.parent?.name);
}
foreach(var s in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include).Where(s=>s.name.Contains("Lamp") || s.name.Contains("Poste") || s.name.Contains("Fachada")))
 report.AppendLine("ART " + s.name + " pos=" + s.transform.position + " scale=" + s.transform.lossyScale + " sprite=" + s.sprite?.name + " bounds=" + s.bounds + " order=" + s.sortingOrder + " parent=" + s.transform.parent?.name);


return report.ToString();
