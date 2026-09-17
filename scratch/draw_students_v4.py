from PIL import Image
import math

W,H=384,216
INK=(12,15,28,255)
G_FRAME=(22,26,42,255)
G_LENS=(160,215,245,255)
SEAT_DARK=(88,55,35,255)
SEAT_MID=(138,88,58,255)
SEAT_LIGHT=(168,112,78,255)

SKIN=dict(
    tavares=(195,135,90,255),fabio=(238,190,148,255),sabia=(232,175,128,255),
    marcos=(115,72,44,255),martins=(245,212,180,255),yasmin=(248,226,212,255),
    pedro=(225,172,128,255),matias=(208,150,106,255),messias=(202,145,100,255))

SHIRT=dict(
    tavares=((32,175,175,255),(20,125,130,255)),
    fabio=((205,48,58,255),(155,32,42,255)),
    sabia=((190,82,148,255),(145,58,115,255)),
    marcos=((45,168,92,255),(30,118,65,255)),
    martins=((98,122,155,255),(72,92,125,255)),
    yasmin=((152,65,188,255),(112,45,145,255)),
    pedro=((215,122,40,255),(168,92,28,255)),
    matias=((45,128,235,255),(30,92,182,255)),
    messias=((198,175,45,255),(152,132,32,255)))

HAIR=dict(
    tavares=((30,20,16,255),(72,48,36,255)),
    fabio=((55,36,22,255),(115,75,48,255)),
    sabia=((40,26,18,255),(85,56,38,255)),
    marcos=((16,14,16,255),(40,34,40,255)),
    martins=((50,36,26,255),(102,76,52,255)),
    yasmin=((12,10,16,255),(35,28,42,255)),
    pedro=((48,30,18,255),(102,65,40,255)),
    matias=((33,24,24,255),(75,58,50,255)),
    messias=((26,18,14,255),(58,42,34,255)))

def disk(img,cx,cy,rx,ry,col):
    p=img.load()
    for dy in range(-ry,ry+1):
        y=cy+dy
        if not(0<=y<H):continue
        frac=dy/max(1,ry)
        rxi=int(rx*(1.0-frac*frac)**0.5+0.5)
        for dx in range(-rxi,rxi+1):
            x=cx+dx
            if 0<=x<W:p[x,y]=col

def rc(img,x,y,w,h,col):
    p=img.load()
    for yy in range(max(0,y),min(H,y+h)):
        for xx in range(max(0,x),min(W,x+w)):p[xx,yy]=col

def pt(img,x,y,col):
    if 0<=x<W and 0<=y<H:img.load()[x,y]=col

def hair_curly(img,cx,cy,rx,ry,d,l):
    disk(img,cx,cy,rx,ry,d)
    for i in range(10):
        a=math.pi+(math.pi/9)*i
        bx=int(cx+(rx-1)*math.cos(a))
        by=int(cy+(ry-1)*math.sin(a))
        disk(img,bx,by,3,3,l)
        disk(img,bx,by,2,2,d)

def hair_afro(img,cx,cy,rx,ry,d,l):
    disk(img,cx,cy-2,rx,ry,d)
    for i in range(8):
        a=math.pi+(math.pi/7)*i
        bx=int(cx+rx*math.cos(a))
        by=int(cy+(ry-1)*math.sin(a))
        disk(img,bx,by,3,3,l)
        disk(img,bx,by,1,1,d)
    for i in range(5):
        bx=cx-6+i*3;by=cy-ry
        disk(img,bx,by,3,2,l)
        disk(img,bx,by,2,1,d)

def hair_side(img,cx,cy,rx,ry,d,l):
    # base oval
    disk(img,cx-1,cy-1,rx,ry,d)
    # lado esquerdo tem inclinacao -- risca lateral
    rc(img,cx-rx-1,cy-ry,rx+5,5,l)
    # franja lateral caindo sobre a testa
    rc(img,cx-rx-1,cy-ry+4,5,ry+2,l)
    rc(img,cx-rx-1,cy-ry,2,ry+3,d)
    # lateral direita curta
    rc(img,cx+rx-2,cy-ry+3,3,ry+2,d)

def hair_bangs(img,cx,cy,rx,ry,d,l):
    # volumoso por baixo
    disk(img,cx,cy+2,rx,ry,d)
    # franja reta horizontal sobre a testa
    rc(img,cx-rx+1,cy-ry+3,rx*2-1,6,d)
    # brilho no topo da franja
    rc(img,cx-rx+3,cy-ry+3,rx*2-5,2,l)
    # laterais
    rc(img,cx-rx,cy-ry+2,2,ry+4,d)
    rc(img,cx+rx-1,cy-ry+2,2,ry+4,d)

def hair_buzz(img,cx,cy,rx,d,l):
    disk(img,cx,cy-1,rx,5,d)
    rc(img,cx-rx+2,cy-4,rx*2-3,3,l)
    # linhas de raspagem simulando navalha
    for xx in range(cx-rx+1,cx+rx-1,2):
        pt(img,xx,cy-1,d)

def hair_cpart(img,cx,cy,rx,ry,d,l):
    disk(img,cx,cy-1,rx,ry,d)
    rc(img,cx-rx+1,cy-ry,rx-2,6,l)
    rc(img,cx+2,cy-ry,rx-2,6,l)
    # linha da risca ao meio (mais escura)
    rc(img,cx-1,cy-ry-1,3,ry+4,INK)
    # queda lateral
    rc(img,cx-rx,cy-1,3,10,d)
    rc(img,cx+rx-2,cy-1,3,10,d)

def face(img,cx,cy,sk,mc=(195,90,85,255)):
    disk(img,cx,cy,8,9,INK)
    disk(img,cx,cy,7,8,sk)
    sh=(max(0,sk[0]-40),max(0,sk[1]-35),max(0,sk[2]-28),255)
    pt(img,cx-3,cy-1,INK)
    pt(img,cx+2,cy-1,INK)
    pt(img,cx-2,cy-2,(245,245,255,255))
    pt(img,cx+3,cy-2,(245,245,255,255))
    pt(img,cx,cy+2,sh)
    rc(img,cx-2,cy+4,5,2,mc)
    pt(img,cx-1,cy+5,(max(0,mc[0]-50),max(0,mc[1]-40),max(0,mc[2]-35),255))
    pt(img,cx+1,cy+5,(max(0,mc[0]-50),max(0,mc[1]-40),max(0,mc[2]-35),255))

def gls(img,cx,cy):
    rc(img,cx-6,cy-2,5,4,G_FRAME)
    rc(img,cx+1,cy-2,5,4,G_FRAME)
    p=img.load()
    for dx in(-5,-4,-3,2,3,4):
        p[cx+dx,cy-1]=G_LENS
        p[cx+dx,cy]=G_LENS
    pt(img,cx-1,cy-1,G_FRAME)
    pt(img,cx,cy-1,G_FRAME)

def body(img,cx,cy,w,h,sl,sd):
    rc(img,cx-w//2,cy,w,h//2+1,sl)
    rc(img,cx-w//2,cy+h//2,w,h-h//2,sd)
    hi=(min(255,sl[0]+45),min(255,sl[1]+45),min(255,sl[2]+45),255)
    rc(img,cx-w//2+1,cy,w-2,2,hi)

def beard(img,cx,cy):
    b=(20,14,10,255)
    sk=SKIN['messias']
    rc(img,cx-4,cy+2,9,3,b)
    rc(img,cx-7,cy,2,8,b)
    rc(img,cx+5,cy,2,8,b)
    rc(img,cx-6,cy+5,13,5,b)
    pt(img,cx-1,cy+3,(int(sk[0]*0.75),int(sk[1]*0.75),int(sk[2]*0.75),255))
    pt(img,cx+1,cy+3,(int(sk[0]*0.75),int(sk[1]*0.75),int(sk[2]*0.75),255))

def hband(img,cx,cy,cm,cl):
    rc(img,cx-9,cy-2,19,4,cm)
    pt(img,cx-5,cy-1,cl);pt(img,cx+3,cy-1,cl);pt(img,cx-1,cy,cl)
    rc(img,cx-10,cy-2,2,5,cm)
    pt(img,cx-10,cy-1,cl)

def earring(img,x,y):
    pt(img,x,y,(252,215,55,255))
    pt(img,x,y+1,(235,192,38,255))

def draw_seat_bg(img):
    for y in range(55,205):
        t=(y-55)/150.0
        r=int(SEAT_LIGHT[0]*(1-t)+SEAT_DARK[0]*t)
        g=int(SEAT_LIGHT[1]*(1-t)+SEAT_DARK[1]*t)
        b=int(SEAT_LIGHT[2]*(1-t)+SEAT_DARK[2]*t)
        rc(img,120,y,263,1,(r,g,b,255))
    rc(img,120,55,263,3,SEAT_DARK)
    rc(img,120,144,263,5,SEAT_DARK)
    rc(img,120,146,263,2,SEAT_LIGHT)
    # Bordas de metal das janelas
    rc(img,120,55,3,148,(28,45,58,255))
    rc(img,380,55,3,148,(28,45,58,255))

MC=dict(tavares=(195,90,80,255),sabia=(195,88,88,255),marcos=(148,65,52,255),
        martins=(208,92,88,255),yasmin=(215,85,95,255),matias=(185,82,70,255))

def draw(img,cx,cy,nm,sty,wide=False):
    d,l=HAIR[nm];sk=SKIN[nm];sl,sd=SHIRT[nm]
    w=23 if wide else 19
    if sty=='curly':hair_curly(img,cx,cy-10,11,13,d,l)
    elif sty=='curly_tall':hair_curly(img,cx,cy-11,13,15,d,l)
    elif sty=='afro':hair_afro(img,cx,cy-8,10,11,d,l)
    elif sty=='side':hair_side(img,cx,cy-8,9,10,d,l)
    elif sty=='bangs':hair_bangs(img,cx,cy-9,11,12,d,l)
    elif sty=='buzz':hair_buzz(img,cx,cy-8,10,d,l)
    elif sty=='cpart':hair_cpart(img,cx,cy-8,9,9,d,l)
    elif sty=='curly_hband':
        hair_curly(img,cx,cy-10,11,12,d,l)
        hband(img,cx,cy-8,(28,100,238,255),(95,168,255,255))
    mc=MC.get(nm,(195,90,85,255))
    face(img,cx,cy,sk,mc)
    if nm in('fabio','pedro'):gls(img,cx,cy-1)
    if nm=='marcos':
        fd=(85,55,32,255)
        rc(img,cx-11,cy-4,3,9,fd);rc(img,cx+9,cy-4,3,9,fd)
    if nm=='messias':beard(img,cx,cy)
    if nm=='tavares':earring(img,cx-9,cy+1)
    body(img,cx,cy+10,w,18,sl,sd)

def compose():
    img=Image.open('scratch/cabin_cleaned.png').convert('RGBA')
    draw_seat_bg(img)
    draw(img,140,85,'tavares','curly')
    draw(img,178,85,'fabio','cpart')
    draw(img,218,85,'sabia','curly',wide=True)
    draw(img,258,85,'marcos','afro')
    draw(img,298,85,'martins','side')
    draw(img,148,118,'yasmin','bangs')
    draw(img,195,118,'pedro','curly_tall')
    draw(img,240,118,'matias','curly_hband')
    draw(img,284,118,'messias','buzz')
    preview=img.resize((768,432),Image.Resampling.NEAREST)
    preview.save('scratch/cabin_v2_preview.png')
    export=img.resize((1672,941),Image.Resampling.NEAREST)
    export.save('Assets/Resources/Varginha/TravelPixel/CabinStudents.png')
    print('Done! v4')

compose()
