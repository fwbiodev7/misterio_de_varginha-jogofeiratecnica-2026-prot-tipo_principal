var report = new System.Text.StringBuilder();
var walk = Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames();
var attack = UnityEngine.Resources.Load<UnityEngine.Texture2D>("Varginha/EdelzioPunchV3");
for(int d=0;d<4;d++) {
 foreach(bool idle in new[]{true,false}) {
  var p= idle ? walk[d][0].texture.GetPixels((int)walk[d][0].rect.x,(int)walk[d][0].rect.y,64,64) : attack.GetPixels(3*64,(3-d)*64,64,64);
  report.AppendLine("d="+d+" idle="+idle);
  for(int y=6;y<53;y++) {
   int a=64,b=0,shirt=0;
   for(int x=0;x<64;x++) {var c=p[y*64+x];if(c.a<.5f)continue;a=Mathf.Min(a,x);b=Mathf.Max(b,x);if(c.r>.43f && c.g>.27f && c.r>c.g*1.15f && c.b<c.g*.58f)shirt++;}
   if(b>=a)report.AppendLine(y+":"+a+"-"+b+" shirt="+shirt);
  }
 }
}
return report.ToString();

