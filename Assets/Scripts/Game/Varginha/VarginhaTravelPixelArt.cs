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
        private readonly Picture _night,_cabin,_car,_tree,_edelzio;
        private readonly uint[] _pixels=new uint[Width*Height];
        private const uint Ink=0xff080f20, Gold=0xffeed48b, Cyan=0xff71cbe0, Muted=0xff8ea9b9;
        private static readonly Dictionary<char,string> Glyphs=BuildFont();
        public VarginhaTravelPixelArt(Picture night,Picture cabin,Picture car,Picture tree,Picture edelzio)
        { _night=night;_cabin=cabin;_car=car;_tree=tree;_edelzio=edelzio; }

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
            Text("CASA > ESCOLA > DIOCESE",Width/2,211,Muted,true);
            // Cortina em degraus entre tomadas: nenhum esticamento ou interpolação da imagem.
            float cut = students ? Math.Abs(seconds-3.6f) : 2f;
            if(cut<.12f) Fill(0,25,Width,160,Ink);
            return _pixels;
        }
        private void Road(float time,int tick,bool students)
        {
            // A ilustração nova é o plano distante. Estrada, árvore original e Fusca são camadas independentes.
            Blit(_night,0,0,Width,Height);
            int roadShift=(int)(time*75)%Width;
            for(int i=-1;i<2;i++)
                Blit(_night,i*Width-roadShift,148,Width,39,0,(int)(_night.Height*.685f),_night.Width,(int)(_night.Height*.181f));
            for(int i=-1;i<4;i++)
            {
                int x=i*175-((int)(time*25)%175);
                Blit(_tree,x,61,100,100,0,0,_tree.Width,_tree.Height,0xff607966);
            }
            // O Fusca entra, freia e estaciona exatamente no centro da tomada. O
            // quadro alternado é mais lento para a suspensão parecer uma animação,
            // não uma troca nervosa de sprites.
            float arrival=Math.Clamp(time/2.35f,0,1);arrival=arrival*arrival*(3-2*arrival);
            int carX=(int)Math.Round(-210+arrival*302);
            float settle=Math.Clamp((time-2.35f)/.55f,0,1);
            float suspension=(1f-settle)*(float)Math.Sin(Math.Max(0f,time-2.35f)*18f)*1.25f;
            int carY=(int)Math.Round(66+suspension);
            int frame=(tick/3)%2;
            Blit(_car,carX,carY,200,112,frame*100,10,100,56);
            // A passagem lateral fica limpa: os nove passageiros e o motorista são
            // revelados na tomada interna final, onde podem ser vistos com detalhe.
            // Poeira em pixels quadrados; sem elipses vetoriais e sem suavização.
            for(int i=0;i<5;i++)
            {
                int phase=(tick+i*3)%15;
                Fill(carX-8-phase*3,carY+97-phase/3,2+phase/5,2,0xff607077);
            }
            // Vegetação próxima atravessa a borda e reforça a velocidade.
            int frontShift=(int)(time*120)%Width;
            for(int i=-1;i<2;i++)
                Blit(_night,i*Width-frontShift,179,Width,37,0,(int)(_night.Height*.828f),_night.Width,(int)(_night.Height*.172f));
        }
        private void DrawDriver(int x,int y,int frame,int tick)
        {
            uint outline=0xff111727, hair=0xff32231f, skin=0xffd98955;
            uint shirt=0xffd7a52a, shirtLight=0xfff1c85f, glass=0x779dd1d7;
            int bob=(tick%6==0 || tick%6==1) ? 1 : 0;
            // Corpo sentado, ombros baixos e braços estendidos até o volante.
            Fill(x+2,y+11+bob,11,6,outline); Fill(x+4,y+11+bob,8,6,shirt);
            Fill(x+5,y+14+bob,2,3,shirtLight); Fill(x+12,y+13+bob,5,2,skin);
            Fill(x+3,y+3+bob,10,9,outline); Fill(x+5,y+4+bob,7,7,skin);
            Fill(x+4,y+2+bob,9,4,hair); Fill(x+5,y+1+bob,6,2,hair);
            Fill(x+5,y+7+bob,2,1,outline); Fill(x+10,y+7+bob,2,1,outline);
            Fill(x+7,y+9+bob,4,1,hair);
            // Reflexo transparente do para-brisa passa sobre a pose, mas não a apaga.
            Fill(x+1,y+1,1,17,glass); Fill(x+14,y+1,1,17,glass);
        }
        private void DrawSidePassengers(int carX,int carY,int frame,int tick)
        {
            uint[] shirts={0xff8147b8,0xffd34d4d,0xff3c84c5};
            uint[] hair={0xff2d211d,0xff5b3325,0xff1b1a25};
            int baseX=frame==0 ? carX+18 : carX+118;
            for(int i=0;i<3;i++)
            {
                int x=baseX+i*13, y=carY+30+(i==1 ? 1 : 0);
                Fill(x,y+7,9,5,0xff111727); Fill(x+2,y+7,5,5,shirts[i]);
                Fill(x+1,y,8,8,0xff111727); Fill(x+2,y+1,6,6,0xffd98955);
                Fill(x+1,y,8,3,hair[i]); Fill(x+3,y+4,1,1,0xff111727); Fill(x+6,y+4,1,1,0xff111727);
                if((tick+i)%7==0) Fill(x+8,y+4,2,1,0xffeecb6c);
            }
        }
        private void DrawSteeringWheel(int x,int y,int direction)
        {
            uint rim=0xff182b3b, shine=0xffb9d7dc;
            Fill(x,y,1,7,rim); Fill(x+direction*5,y,1,7,rim);
            Fill(x+direction*1,y-1,4,1,rim); Fill(x+direction*1,y+7,4,1,rim);
            Fill(x+direction*2,y+2,2,3,shine); Fill(x+direction*1,y+3,4,1,rim);
        }
        private void DrawWindshieldReflection(int carX,int carY,int frame)
        {
            int x=frame==0 ? carX+54 : carX+154;
            uint reflection=0x8898d0dc;
            Fill(x,carY+25,2,15,reflection);
            Fill(x+4,carY+22,1,10,reflection);
            Fill(x+7,carY+19,1,7,reflection);
        }
        private void Cabin(int tick)
        {
            int sway=(tick/3)%4;
            int offset=sway==0?-1:sway==2?1:0;
            Blit(_cabin,-1,offset,386,216);
            // Reflexos discretos cruzam apenas as bordas dos vidros, preservando rostos e figurinos.
            int shine=(tick*3)%90;
            for(int i=0;i<3;i++)
            {
                int y=40+shine+i*8;
                if(y<130) { Fill(7,y,5,1,0xff365c69);Fill(374,y+5,4,1,0xff365c69); }
            }
            // Pequeno reflexo no retrovisor oscila junto da suspensão.
            Fill(180+offset,37,13,1,0xff516675);
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
                ">:100010001010100","/:001001010100100",".:000000000000010","-:000000111000000"
            };
            var result=new Dictionary<char,string>();foreach(var row in rows)result[row[0]]=row.Substring(2);return result;
        }
    }
}
