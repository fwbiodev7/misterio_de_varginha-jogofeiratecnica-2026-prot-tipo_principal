using System;
using System.Collections.Generic;

namespace Game.Varginha
{
    /// <summary>Composição inteira em 384x216. O mesmo rasterizador produz o jogo e as prévias de revisão.</summary>
    public sealed class VarginhaTravelPixelArt
    {
        public const int Width = 384, Height = 216;
        public sealed class Picture
        {
            public readonly int Width, Height;
            public readonly uint[] Pixels;
            public Picture(int width,int height,uint[] pixels) { Width=width;Height=height;Pixels=pixels; }
        }
        private readonly Picture _night,_car,_tree,_cabin;
        private readonly uint[] _pixels=new uint[Width*Height];
        private const uint Ink=0xff080f20, Gold=0xffeed48b, Cyan=0xff71cbe0, Muted=0xff8ea9b9;
        private static readonly Dictionary<char,string> Glyphs=BuildFont();
        public VarginhaTravelPixelArt(Picture night,Picture car,Picture tree,Picture cabin)
        { _night=night;_car=car;_tree=tree;_cabin=cabin; }

        public uint[] Render(float seconds,bool students,float progress,string difficulty)
        {
            int tick=(int)(seconds*12); // Exposição em doze quadros por segundo.
            // A estrada abre a viagem. Quando há alunos, a tomada interna entra uma
            // única vez e permanece até a cena terminar, sem voltar para a composição
            // lateral que podia recortar os personagens nas janelas.
            bool inside=students && seconds>=3.6f;
            Array.Fill(_pixels,Ink);
            if(inside) Cabin(tick); else Road(seconds,tick,students);
            Fill(0,0,Width,25,Ink);
            Fill(0,24,Width,1,0xff284758);
            Text("MISTERIO DE VARGINHA / "+difficulty,Width/2,4,Muted,true);
            Text(students ? "RUMO A DIOCESE" : "RUMO A ESCOLA",Width/2,14,Gold,true);
            Fill(0,185,Width,31,Ink); Fill(0,185,Width,1,0xff284758);
            string caption=inside ? "TODOS A BORDO. NINGUEM FICA PARA TRAS."
                : students ? "A TURMA SEGUE JUNTO PELA NOITE." : "O RADIO CHIA. A ESCOLA ESPERA.";
            Text(caption,Width/2,190,Gold,true);
            Fill(50,204,284,3,0xff243848);
            Fill(50,204,(int)(284*Math.Clamp(progress,0,1)),3,Cyan);
            for(int i=1;i<28;i++) Fill(50+i*10,204,1,3,Ink);
            DrawLoadingStatus(progress, tick);
            Text("CASA > ESCOLA > DIOCESE",Width/2,211,Muted,true);
            // Cortina em degraus entre tomadas: nenhum esticamento ou interpolação da imagem.
            float cut = students ? Math.Abs(seconds-3.6f) : 2f;
            if(cut<.12f) Fill(0,25,Width,160,Ink);
            return _pixels;
        }
        private void Road(float time, int tick, bool students)
        {
            // Fundo desenhado em pixels, sempre filtrado por nearest-neighbour.
            Blit(_night, 0, 0, Width, Height);
            DrawRoadMotion(time, tick);

            // Deslocamento cinematográfico suave do Fusca
            float arrival = Math.Clamp(time / 2.2f, 0f, 1f);
            arrival = arrival * arrival * (3f - 2f * arrival);
            int carX = (int)Math.Round(-190f + arrival * 275f);
            float bounce = (float)Math.Sin(time * 14f) * 0.85f;
            int carY = (int)Math.Round(78f + bounce);
            int carW = 168, carH = 94;

            // Feixe volumétrico dos faróis iluminando a pista à frente
            DrawHeadlights(carX + carW - 25, carY + 60);

            // Fusca avançando na pista (alterna quadros para calotas girando)
            int frame = (tick / 3) % 2;
            Blit(_car, carX, carY, carW, carH, frame * 100, 10, 100, 56);

            // Fumaça sutil saindo do escapamento
            for (int i = 0; i < 4; i++)
            {
                int phase = (tick + i * 4) % 16;
                Fill(carX - 4 - phase * 3, carY + 76 - phase / 3, 2 + phase / 4, 2, 0x8898a8b8);
            }
        }

        private void DrawRoadMotion(float time, int tick)
        {
            // Marcas de pista em dois planos e reflexos curtos dão sensação de velocidade
            // sem suavizar nem deslocar a arte de base em pixels inteiros.
            int dashShift = (int)(time * 150f) % 48;
            for (int dx = -48; dx < Width + 48; dx += 48)
            {
                Fill(dx - dashShift, 160, 22, 2, Gold);
                Fill(dx - dashShift + 5, 163, 12, 1, 0x776f5c37);
            }

            int reflectorShift = (int)(time * 92f) % 64;
            for (int dx = -64; dx < Width + 64; dx += 64)
            {
                int x = dx - reflectorShift;
                Fill(x, 151, 2, 1, 0xffd5e8df);
                Fill(x + 31, 177, 3, 1, 0xff395469);
            }

            // Vaga-lumes e luzes distantes piscam em um ritmo discreto no acostamento.
            for (int i = 0; i < 9; i++)
            {
                int x = (i * 43 + 17) % Width;
                int y = 119 + (i * 17) % 24;
                if ((tick + i * 3) % 11 < 4) Fill(x, y, 1, 1, 0xffe6cb75);
            }
        }

        private void DrawLoadingStatus(float progress, int tick)
        {
            int percent = (int)Math.Round(Math.Clamp(progress, 0f, 1f) * 100f);
            Text("CARREGANDO " + percent + "%", Width / 2 - 7, 197, Gold, true);
            int left = Width / 2 + 55;
            for (int dot = 0; dot < 4; dot++)
            {
                uint color = dot == tick % 4 ? Cyan : 0xff284758;
                Fill(left + dot * 5, 198, 3, 3, color);
            }
        }

        private void DrawHeadlights(int originX, int originY)
        {
            for (int hx = Math.Max(0, originX); hx < Width; hx++)
            {
                float prog = (hx - originX) / (float)Math.Max(1, Width - originX);
                float spread = 4f + 26f * (float)Math.Pow(prog, 0.82f);
                float beamY = originY + 8f * prog;
                int minY = Math.Max(0, (int)(beamY - spread));
                int maxY = Math.Min(Height - 1, (int)(beamY + spread));
                for (int hy = minY; hy <= maxY; hy++)
                {
                    float d = Math.Abs(hy - beamY) / Math.Max(1f, spread);
                    float alpha = Math.Max(0f, (1f - prog * 0.70f) * (1f - d * d));
                    if (alpha <= 0.02f) continue;
                    uint aByte = (uint)Math.Clamp((int)(alpha * 70f), 0, 255);
                    uint color = (aByte << 24) | 0x00ffe890u;
                    Fill(hx, hy, 1, 1, color);
                }
            }
        }

        private void Cabin(int tick)
        {
            // Balanço sutil de suspensão e vibração do motor do Fusca
            int sway = (tick / 3) % 4;
            int offset = sway == 0 ? -1 : sway == 2 ? 1 : 0;
            if (_cabin != null)
            {
                Blit(_cabin, -1, 25 + offset, Width + 2, 160);
            }
            else
            {
                Fill(0, 25, Width, 160, 0xff0a1220);
            }

            // Reflexos atmosféricos discretos nos vidros laterais
            int shine = (tick * 3) % 90;
            for (int i = 0; i < 3; i++)
            {
                int ry = 40 + shine + i * 8;
                if (ry < 130) { Fill(7, ry, 5, 1, 0x33365c69); Fill(Width - 10, ry + 5, 4, 1, 0x33365c69); }
            }
            // Reflexo suave no retrovisor oscilando com o movimento
            Fill(Width / 2 - 20 + offset, 37, 40, 1, 0x44516675);
            // Pequenos pulsos do painel e dos postes que passam pelo vidro mantêm
            // a tomada interna viva enquanto a próxima fase carrega.
            int panelGlow = 82 + (tick % 5) * 4;
            Fill(142, 177, 100, 2, 0x553b6680);
            Fill(176, 179, 32, 1, 0xff3b6680);
            Fill(panelGlow, 171, 2, 1, 0xffe0b45f);
        }
        private void Fill(int x,int y,int w,int h,uint color)
        {
            for(int yy=Math.Max(0,y);yy<Math.Min(Height,y+h);yy++)
                for(int xx=Math.Max(0,x);xx<Math.Min(Width,x+w);xx++)
                {
                    int index=yy*Width+xx;
                    int alpha=(int)(color>>24);
                    if(alpha>=255) { _pixels[index]=color; continue; }
                    if(alpha<=0) continue;
                    uint background=_pixels[index];
                    int inverse=255-alpha;
                    uint r=(((color>>16)&255u)*((uint)alpha)+((background>>16)&255u)*(uint)inverse)/255u;
                    uint g=(((color>>8)&255u)*((uint)alpha)+((background>>8)&255u)*(uint)inverse)/255u;
                    uint b=((color&255u)*((uint)alpha)+(background&255u)*(uint)inverse)/255u;
                    _pixels[index]=0xff000000u|(r<<16)|(g<<8)|b;
                }
        }
        private void Blit(Picture p,int x,int y,int w,int h) => Blit(p,x,y,w,h,0,0,p.Width,p.Height);
        private void Blit(Picture p,int x,int y,int w,int h,int sx,int sy,int sw,int sh,uint tint=0xffffffff)
        {
            for(int yy=Math.Max(0,y);yy<Math.Min(Height,y+h);yy++)
            {
                int py=Math.Clamp(sy+(yy-y)*sh/h,0,p.Height-1);
                for(int xx=Math.Max(0,x);xx<Math.Min(Width,x+w);xx++)
                {
                    int px=Math.Clamp(sx+(xx-x)*sw/w,0,p.Width-1);
                    uint c=p.Pixels[py*p.Width+px];
                    int a=(int)(c>>24); if(a<128)continue;
                    uint r=((c>>16)&255)*((tint>>16)&255)/255;
                    uint g=((c>>8)&255)*((tint>>8)&255)/255;
                    uint b=(c&255)*(tint&255)/255;
                    _pixels[yy*Width+xx]=0xff000000|(r<<16)|(g<<8)|b;
                }
            }
        }
        private void Text(string value,int x,int y,uint color,bool centered=false)
        {
            value=value.ToUpperInvariant();
            if(centered)x-=value.Length*4/2;
            foreach(char c in value)
            {
                if(Glyphs.TryGetValue(c,out var pattern))
                    for(int i=0;i<15;i++)if(pattern[i]=='1')Fill(x+i%3,y+i/3,1,1,color);
                x+=4;
            }
        }
        private static Dictionary<char,string> BuildFont()
        {
            string[] rows={
                "A:010101111101101","B:110101110101110","C:011100100100011","D:110101101101110",
                "E:111100110100111","F:111100110100100","G:011100101101011","H:101101111101101",
                "I:111010010010111","J:001001001101010","K:101101110101101","L:100100100100111",
                "M:101111111101101","N:101111111111101","O:010101101101010","P:110101110100100",
                "Q:010101101111011","R:110101110101101","S:011100010001110","T:111010010010010",
                "U:101101101101111","V:101101101101010","W:101101111111101","X:101101010101101",
                "Y:101101010010010","Z:111001010100111","0:111101101101111","1:010110010010111",
                "2:110001010100111","3:110001010001110","4:101101111001001","5:111100110001110",
                "6:011100111101111","7:111001010010010","8:111101111101111","9:111101111001110",
                ">:100010001010100","/:001001010100100",".:000000000000010","-:000000111000000",
                "%:101001010010101"
            };
            var result=new Dictionary<char,string>();foreach(var row in rows)result[row[0]]=row.Substring(2);return result;
        }
    }
}
