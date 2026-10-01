var go=new UnityEngine.GameObject("QA");var sr=go.AddComponent<UnityEngine.SpriteRenderer>();using var appearance=new Game.Varginha.EdelzioBackpackAppearance();var report=new System.Text.StringBuilder();
try{for(int d=0;d<8;d++)for(int f=0;f<4;f++) {
 int c=d<4?d:d<6?0:3;sr.sprite=Game.Varginha.VarginhaReferenceSprites.EdelzioWalkFrames()[c][f];appearance.UpdatePose(go.transform,sr,d,true);
 var overlay=go.transform.Find("Mochila_Alca").GetComponent<UnityEngine.SpriteRenderer>().sprite.texture.GetPixels();var original=sr.sprite.texture.GetPixels((int)sr.sprite.rect.x,(int)sr.sprite.rect.y,64,64);
 for(int i=0;i<4096;i++)if(overlay[i].a>.5f && (i/64<17||i/64>38))report.AppendLine($"d={d},f={f},x={i%64},y={i/64},body={(UnityEngine.Color32)original[i]},overlay={(UnityEngine.Color32)overlay[i]}");
}}finally{UnityEngine.Object.DestroyImmediate(go);}return report.ToString();
