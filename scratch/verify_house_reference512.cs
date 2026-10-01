UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/FaseTopView_Varginha.unity");
var house = UnityEngine.GameObject.Find("House_And_Yard").transform;
Game.Varginha.VarginhaEnvironmentPolish.EnsureHouse(house);
UnityEngine.Physics2D.SyncTransforms();
var player = UnityEngine.Object.FindAnyObjectByType<Game.Varginha.EdelzioTopDownController>();
var playerCollider = player.GetComponent<UnityEngine.Collider2D>();
float radius = UnityEngine.Mathf.Max(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y) + .03f;
const float step = .25f;
var origin = (UnityEngine.Vector2)player.transform.position;
var visited = new System.Collections.Generic.HashSet<UnityEngine.Vector2Int>();
var queue = new System.Collections.Generic.Queue<UnityEngine.Vector2Int>();
var start = new UnityEngine.Vector2Int(UnityEngine.Mathf.RoundToInt(origin.x / step), UnityEngine.Mathf.RoundToInt(origin.y / step));
visited.Add(start); queue.Enqueue(start);
var directions = new[] { UnityEngine.Vector2Int.up, UnityEngine.Vector2Int.down, UnityEngine.Vector2Int.left, UnityEngine.Vector2Int.right };
while (queue.Count > 0)
{
    var cell = queue.Dequeue();
    foreach (var direction in directions)
    {
        var next = cell + direction;
        var point = (UnityEngine.Vector2)next * step;
        if (point.x < -9.4f || point.x > 27.4f || point.y < -8.4f || point.y > 8.4f || visited.Contains(next)) continue;
        bool blocked = false;
        foreach (var collider in UnityEngine.Physics2D.OverlapCircleAll(point, radius))
            if (!collider.isTrigger && collider.GetComponentInParent<Game.Varginha.EdelzioTopDownController>() == null) { blocked = true; break; }
        if (blocked) continue;
        visited.Add(next); queue.Enqueue(next);
    }
}
var accessibility = new System.Collections.Generic.List<object>();
foreach (var name in new[] { "ToyBox_UnderBed", "Backpack_Prop", "Notebook_TI", "Coffee_Cup", "Doc_Historical", "FuseBox_Prop", "Chair_Office", "Fusca_1996_Exit" })
{
    var target = UnityEngine.GameObject.Find(name);
    if (target == null) throw new System.Exception("Missing target: " + name);
    var targetCollider = target.GetComponent<UnityEngine.Collider2D>();
    float nearest = float.MaxValue;
    foreach (var point in visited)
    {
        var position = (UnityEngine.Vector2)point * step;
        var interactionPoint = targetCollider != null ? targetCollider.ClosestPoint(position) : (UnityEngine.Vector2)target.transform.position;
        nearest = UnityEngine.Mathf.Min(nearest, UnityEngine.Vector2.Distance(position, interactionPoint));
    }
    accessibility.Add(new { name, nearest, accessible = nearest <= 1.8f });
    if (nearest > 1.8f) throw new System.Exception("Blocked target: " + name);
}
var textures = new System.Collections.Generic.List<object>();
var yardCell = new UnityEngine.Vector2Int(39, 0);
if (!visited.Contains(yardCell)) throw new System.Exception("The yard doorway is blocked.");
foreach (var file in new[] { "Furniture", "Floor", "Wall", "SofaNorth", "Chairs", "Props", "Architecture", "Yard", "EdelzioInteractions" })
{
    var texture = UnityEngine.Resources.Load<UnityEngine.Texture2D>("Varginha/HouseReference512/" + file);
    if (texture.width != 512 || texture.height != 512 || texture.filterMode != UnityEngine.FilterMode.Point || texture.mipmapCount != 1)
        throw new System.Exception("Incorrect texture settings: " + file);
    textures.Add(new { file, texture.width, texture.height, filter = texture.filterMode.ToString() });
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(house.gameObject.scene);
if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(house.gameObject.scene)) throw new System.Exception("Scene save failed.");
UnityEditor.AssetDatabase.SaveAssets();
var seating = new System.Collections.Generic.List<object>();
var route = new System.Collections.Generic.List<UnityEngine.Vector2>();
foreach (var pair in new[] { new[] { "Chair_Office", "Notebook_TI" }, new[] { "Cadeira_Cafe", "Coffee_Cup" } })
{
    var chair = UnityEngine.GameObject.Find(pair[0]);
    var prop = UnityEngine.GameObject.Find(pair[1]);
    bool canApproach = false;
    foreach (var cell in visited)
    {
        var startPoint = (UnityEngine.Vector2)cell * step;
        if (UnityEngine.Vector2.Distance(startPoint, prop.transform.position) > 1.8f) continue;
        if (!Game.Varginha.VarginhaInteractionApproach.FindPath(startPoint, chair.transform.position,
            playerCollider, chair.GetComponent<UnityEngine.Collider2D>(), route)) continue;
        canApproach = true;
        break;
    }
    if (!canApproach) throw new System.Exception("Cannot sit from an interactable position: " + pair[0]);
    seating.Add(new { chair = pair[0], reachable = true });
}
int count = house.GetComponentsInChildren<UnityEngine.Transform>(true).Length;
Game.Varginha.VarginhaEnvironmentPolish.EnsureHouse(house);
if (count != house.GetComponentsInChildren<UnityEngine.Transform>(true).Length) throw new System.Exception("Duplicated layout after refresh.");
return new { visited = visited.Count, radius, accessibility, textures, seating, stableObjects = count, saved = true };
