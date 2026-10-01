if (!UnityEngine.Application.isPlaying) throw new System.Exception("Enter Play Mode first.");
var player = UnityEngine.Object.FindAnyObjectByType<Game.Varginha.EdelzioTopDownController>();
var body = player.GetComponent<UnityEngine.Rigidbody2D>();
var pose = player.GetComponent<Game.Varginha.VarginhaPlayerSpriteAnimation>();
var action = player.GetComponent<Game.Varginha.VarginhaPlayerActionAnimation>();
var coffee = UnityEngine.GameObject.Find("Coffee_Cup");
var notebook = UnityEngine.GameObject.Find("Notebook_TI");
var notes = new System.Collections.Generic.List<string>();
System.Action<string, UnityEngine.Vector2, float, int, int> render = (name, center, zoom, width, height) =>
{
    var go = new UnityEngine.GameObject("Temporary_Runtime_Preview");
    var camera = go.AddComponent<UnityEngine.Camera>();
    camera.orthographic = true;
    camera.orthographicSize = zoom;
    camera.transform.position = new UnityEngine.Vector3(center.x, center.y, -10);
    camera.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
    camera.backgroundColor = new UnityEngine.Color(.09f, .05f, .05f);
    camera.allowHDR = false;
    camera.allowMSAA = false;
    var target = new UnityEngine.RenderTexture(width, height, 24);
    var texture = new UnityEngine.Texture2D(width, height, UnityEngine.TextureFormat.RGBA32, false);
    var previous = UnityEngine.RenderTexture.active;
    try
    {
        camera.targetTexture = target;
        camera.Render();
        UnityEngine.RenderTexture.active = target;
        texture.ReadPixels(new UnityEngine.Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        System.IO.File.WriteAllBytes("Docs/Previews/" + name + ".png", texture.EncodeToPNG());
    }
    finally
    {
        camera.targetTexture = null;
        UnityEngine.RenderTexture.active = previous;
        UnityEngine.Object.DestroyImmediate(texture);
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(go);
    }
};
render("CasaEQuintal512", new UnityEngine.Vector2(9, 0), 9.65f, 2200, 1100);
var coffeeStart = new UnityEngine.Vector2(6.4f, -2.2f);
player.transform.position = new UnityEngine.Vector3(coffeeStart.x, coffeeStart.y, 0);
body.position = coffeeStart;
body.linearVelocity = UnityEngine.Vector2.zero;
UnityEngine.Physics2D.SyncTransforms();
Game.Varginha.VarginhaGameHUD.Instance?.CloseDialogue();
coffee.GetComponent<Game.Varginha.InteractableProp>().Interact(player);
int stage = 0;
bool sawCoffee = false;
double began = UnityEditor.EditorApplication.timeSinceStartup;
UnityEditor.EditorApplication.CallbackFunction poll = null;
poll = () =>
{
    try
    {
        if (!UnityEngine.Application.isPlaying || player == null || UnityEditor.EditorApplication.timeSinceStartup - began > 25)
            throw new System.Exception("Runtime review timed out or was interrupted.");
        if (stage == 0)
        {
            if (!sawCoffee && pose.IsDrinking && player.GetComponent<UnityEngine.SpriteRenderer>().sprite.name.Contains("_0_2"))
            {
                render("EdelzioCafeAtual512", new UnityEngine.Vector2(4.4f, -3.3f), 2.7f, 1100, 800);
                notes.Add("Coffee seated at " + body.position + "; frame " + player.GetComponent<UnityEngine.SpriteRenderer>().sprite.name);
                sawCoffee = true;
            }
            if (!player.IsInputLocked)
            {
                if (!sawCoffee) throw new System.Exception("Coffee did not reach its seated sip frame.");
                if (UnityEngine.Vector2.Distance(body.position, coffeeStart) > .16f) throw new System.Exception("Coffee did not return to the starting point.");
                if (coffee.GetComponent<UnityEngine.SpriteRenderer>().sprite.name != "House512_CupEmpty") throw new System.Exception("Coffee did not finish with the empty cup.");
                notes.Add("Coffee returned to start, restored movement and left an empty cup.");
                Game.Varginha.VarginhaGameHUD.Instance?.CloseDialogue();
                player.transform.position = new UnityEngine.Vector3(-6.92f, -3.61f, 0);
                body.position = new UnityEngine.Vector2(-6.92f, -3.61f);
                notes.Add("Notebook approach starts at the west doorway shown in the obstruction report.");
                UnityEngine.Physics2D.SyncTransforms();
                stage = 1;
                action.PlayNotebookSession(notebook.transform, () =>
                {
                    render("EdelzioNotebookAtual512", new UnityEngine.Vector2(-5f, -3.6f), 2.8f, 1100, 800);
                    notes.Add("Notebook seated at " + body.position + "; frame " + player.GetComponent<UnityEngine.SpriteRenderer>().sprite.name);
                    action.FinishNotebookSession();
                    stage = 2;
                });
            }
        }
        else if (stage == 2 && !player.IsInputLocked)
        {
            if (pose.IsSeated || player.IsScriptedMotion) throw new System.Exception("Notebook movement was not restored.");
            notes.Add("Notebook returned to standing with movement restored.");
            System.IO.File.WriteAllLines("scratch/house_runtime_review.txt", notes);
            UnityEditor.EditorApplication.update -= poll;
        }
    }
    catch (System.Exception error)
    {
        notes.Add("ERROR: " + error.Message);
        System.IO.File.WriteAllLines("scratch/house_runtime_review.txt", notes);
        UnityEditor.EditorApplication.update -= poll;
    }
};
UnityEditor.EditorApplication.update += poll;
return "Runtime review started; coffee and notebook snapshots will be saved automatically.";
