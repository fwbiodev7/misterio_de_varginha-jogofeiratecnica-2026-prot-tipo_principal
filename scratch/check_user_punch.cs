var authored=UnityEngine.Resources.Load<UnityEngine.Texture2D>("Varginha/Equipment/EdelzioBackpackUserPunchV3");
using var appearance=new Game.Varginha.EdelzioBackpackAppearance();
int[] samples={-1,3,9,15};var report=new System.Text.StringBuilder();
for(int d=0;d<4;d++)for(int s=0;s<4;s++) {
 var body=s==0?Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames()[d][0]:Game.Varginha.VarginhaReferenceSprites.EdelzioAttackFrames()[d][samples[s]];
 var baseline=appearance.ComposeFrame(body,d);var pixels=baseline.texture.GetPixels32();var edited=authored.GetPixels(s*64,(3-d)*64,64,64);
 for(int i=0;i<4096;i++) { UnityEngine.Color32 c=edited[i],p=pixels[i];
 if((c.a>127||p.a>127) && (System.Math.Abs(c.r-p.r)>3||System.Math.Abs(c.g-p.g)>3||System.Math.Abs(c.b-p.b)>3||System.Math.Abs(c.a-p.a)>3)) report.AppendLine($"{d},{s}: {i%64},{i/64} {p} -> {c}"); }
}
return report.ToString();
