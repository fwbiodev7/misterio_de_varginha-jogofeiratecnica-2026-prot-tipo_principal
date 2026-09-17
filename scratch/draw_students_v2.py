from PIL import Image
import math
W,H=384,216
INK=(12,15,28,255)
SEAT_MID=(145,95,62,255)
SEAT_DARK=(100,62,40,255)
G_FRAME=(22,26,42,255)
G_LENS=(180,225,248,255)
SKIN=dict(tavares=(195,135,90,255),fabio=(238,190,148,255),sabia=(235,178,130,255),marcos=(120,76,48,255),martins=(245,210,178,255),yasmin=(248,225,210,255),pedro=(228,175,130,255),matias=(210,152,108,255),messias=(205,148,102,255))
SHIRT=dict(tavares=((28,168,168,255),(18,120,125,255)),fabio=((200,45,55,255),(150,30,40,255)),sabia=((185,80,140,255),(140,55,108,255)),marcos=((42,162,88,255),(28,115,62,255)),martins=((95,118,148,255),(68,88,118,255)),yasmin=((148,62,182,255),(108,42,138,255)),pedro=((210,118,38,255),(165,88,28,255)),matias=((42,122,228,255),(28,88,175,255)),messias=((195,172,42,255),(148,128,30,255)))
HAIR=dict(tavares=((32,22,18,255),(68,45,34,255)),fabio=((58,38,24,255),(118,78,48,255)),sabia=((42,28,20,255),(88,58,40,255)),marcos=((18,15,18,255),(42,36,42,255)),martins=((52,38,28,255),(105,78,55,255)),yasmin=((14,11,18,255),(38,32,46,255)),pedro=((50,32,20,255),(105,68,42,255)),matias=((35,25,25,255),(78,60,52,255)),messias=((28,20,16,255),(60,44,36,255)))
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
    for i in range(9):
        a=math.pi+(math.pi/8)*i
        bx=int(cx+(rx-2)*math.cos(a));by=int(cy+(ry-1)*math.sin(a))
        disk(img,bx,by,3,2,l);disk(img,bx,by,2,1,d)
def hair_afro(img,cx,cy,rx,ry,d,l):
    disk(img,cx,cy-2,rx,ry,d)
    for i in range(7):
        a=math.pi+(math.pi/6)*i
        bx=int(cx+(rx-1)*math.cos(a));by=int(cy+(ry-1)*math.sin(a))
        disk(img,bx,by,2,2,l)
    for i in range(5):
        bx=cx-6+i*3;by=cy-ry+1
        disk(img,bx,by,2,2,l);disk(img,bx,by,1,1,d)
def hair_side(img,cx,cy,rx,ry,d,l):
    disk(img,cx,cy-1,rx,ry,d)
    rc(img,cx-rx+1,cy-ry,rx+3,4,l)
    rc(img,cx-rx+1,cy-ry,2,ry+2,d)
def hair_bangs(img,cx,cy,rx,ry,d,l):
    disk(img,cx,cy,rx,ry,d)
    rc(img,cx-rx+1,cy-ry+2,rx*2-1,5,d)
    rc(img,cx-rx+2,cy-ry+2,rx*2-3,1,l)
def hair_buzz(img,cx,cy,rx,d,l):
    disk(img,cx,cy-2,rx,4,d)
    rc(img,cx-rx+1,cy-5,rx*2-1,2,l)
def hair_cpart(img,cx,cy,rx,ry,d,l):
    disk(img,cx,cy-1,rx,ry,d)
    rc(img,cx-rx+1,cy-ry,rx-1,5,l)
    rc(img,cx+1,cy-ry,rx-1,5,l)
    rc(img,cx-1,cy-ry-1,2,ry+2,d)
    rc(img,cx-rx,cy-1,2,8,d)
    rc(img,cx+rx-1,cy-1,2,8,d)
def face(img,cx,cy,sk,mc=(195,90,85,255)):
    disk(img,cx,cy,8,9,INK);disk(img,cx,cy,7,8,sk)
    pt(img,cx-3,cy-1,INK);pt(img,cx-2,cy-1,(240,240,240,255))
    pt(img,cx+2,cy-1,INK);pt(img,cx+3,cy-1,(240,240,240,255))
    pt(img,cx-1,cy+4,mc);pt(img,cx,cy+4,mc);pt(img,cx+1,cy+4,mc)
def gls(img,cx,cy):
    rc(img,cx-6,cy-2,5,4,G_FRAME);rc(img,cx+1,cy-2,5,4,G_FRAME)
    for dx in(-5,-4,-3,2,3,4):pt(img,cx+dx,cy-1,G_LENS)
    pt(img,cx-1,cy-1,G_FRAME);pt(img,cx,cy-1,G_FRAME)
def body(img,cx,cy,w,h,sl,sd):
    rc(img,cx-w//2,cy,w,h//2,sl)
    rc(img,cx-w//2,cy+h//2,w,h-h//2,sd)
def beard(img,cx,cy):
    b=(22,16,12,255)
    rc(img,cx-3,cy+3,7,2,b);rc(img,cx-6,cy+1,2,5,b)
    rc(img,cx+5,cy+1,2,5,b);rc(img,cx-5,cy+5,11,4,b)
def hband(img,cx,cy,cm,cl):
    rc(img,cx-8,cy-3,17,3,cm)
    pt(img,cx-4,cy-2,cl);pt(img,cx+2,cy-2,cl)
    rc(img,cx-9,cy-3,2,4,cm);pt(img,cx-9,cy-2,cl)
def earring(img,x,y):
    pt(img,x,y,(248,210,50,255));pt(img,x,y+1,(228,185,30,255))
MC=dict(tavares=(195,90,80,255),sabia=(195,88,88,255),marcos=(155,72,60,255),martins=(210,90,88,255),yasmin=(218,88,98,255),matias=(188,84,72,255))
def draw(img,cx,cy,nm,sty,wide=False):
    d,l=HAIR[nm];sk=SKIN[nm];sl,sd=SHIRT[nm]
    w=22 if wide else 18
    if sty=='curly':hair_curly(img,cx,cy-10,11,13,d,l)
    elif sty=='curly_tall':hair_curly(img,cx,cy-11,13,15,d,l)
    elif sty=='afro':hair_afro(img,cx,cy-8,9,10,d,l)
    elif sty=='side':hair_side(img,cx,cy-8,9,10,d,l)
    elif sty=='bangs':hair_bangs(img,cx,cy-9,11,12,d,l)
    elif sty=='buzz':hair_buzz(img,cx,cy-8,9,d,l)
    elif sty=='cpart':hair_cpart(img,cx,cy-8,9,9,d,l)
    elif sty=='curly_hband':
        hair_curly(img,cx,cy-10,11,12,d,l)
        hband(img,cx,cy-8,(28,100,238,255),(95,168,255,255))
    mc=MC.get(nm,(195,90,85,255))
    face(img,cx,cy,sk,mc)
    if nm in('fabio','pedro'):gls(img,cx,cy-1)
    if nm=='marcos':
        fd=(92,60,38,255)
        rc(img,cx-9,cy-2,2,6,fd);rc(img,cx+8,cy-2,2,6,fd)
    if nm=='messias':beard(img,cx,cy)
    if nm=='tavares':earring(img,cx-8,cy+2)
    body(img,cx,cy+10,w,18,sl,sd)
def compose():
    img=Image.open('scratch/cabin_cleaned.png').convert('RGBA')
    rc(img,118,60,265,145,SEAT_MID)
    rc(img,118,60,265,2,SEAT_DARK)
    rc(img,118,145,265,3,SEAT_DARK)
    draw(img,140,88,'tavares','curly')
    draw(img,178,88,'fabio','cpart')
    draw(img,218,88,'sabia','curly',wide=True)
    draw(img,258,88,'marcos','afro')
    draw(img,298,88,'martins','side')
    draw(img,148,118,'yasmin','bangs')
    draw(img,192,118,'pedro','curly_tall')
    draw(img,238,118,'matias','curly_hband')
    draw(img,282,118,'messias','buzz')
    rc(img,118,58,2,95,(38,58,75,255))
    rc(img,381,58,2,95,(38,58,75,255))
    preview=img.resize((768,432),Image.Resampling.NEAREST)
    preview.save('scratch/cabin_v2_preview.png')
    export=img.resize((1672,941),Image.Resampling.NEAREST)
    export.save('Assets/Resources/Varginha/TravelPixel/CabinStudents.png')
    print('Done!')
compose()
