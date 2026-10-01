Game.Varginha.EdelzioBackpackFrames.ClearCache();
var texture=UnityEngine.Resources.Load<UnityEngine.Texture2D>(Game.Varginha.EdelzioBackpackAppearance.UserDirectionsPath);
if(texture==null || texture.width!=256 || texture.height!=128 || !texture.isReadable
    || texture.filterMode!=UnityEngine.FilterMode.Point || texture.mipmapCount!=1)
    throw new System.Exception("User directions were not imported for crisp native pixels");
var results=new System.Collections.Generic.List<string>();
var type=System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.Select(System.AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Game.Tests.EditMode.VarginhaBackpackRemasterTests")),t=>t!=null);
if(type==null)throw new System.Exception("Backpack test assembly unavailable");
var fixture=System.Activator.CreateInstance(type);
foreach(var method in type.GetMethods())
{
    if(!System.Linq.Enumerable.Any(method.GetCustomAttributes(false),a=>a.GetType().FullName=="NUnit.Framework.TestAttribute"))continue;
    try {method.Invoke(fixture,null); results.Add("PASS "+method.Name);}
    catch(System.Reflection.TargetInvocationException error){results.Add("FAIL "+method.Name+": "+error.InnerException.Message);}
}
foreach(var animation in UnityEngine.Object.FindObjectsByType<Game.Varginha.VarginhaPlayerSpriteAnimation>())
    animation.RefreshEquipmentAppearance();
System.IO.File.WriteAllLines("scratch/user-backpack-checks.txt",results);
if(System.Linq.Enumerable.Any(results,line=>line.StartsWith("FAIL")))throw new System.Exception(string.Join("\n",results));
return string.Join("\n",results);
