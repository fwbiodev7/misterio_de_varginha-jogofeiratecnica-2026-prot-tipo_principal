import os
from PIL import Image
import numpy as np

BASE = r"c:\Users\Usuario\misterio_de_varginha-jogofeiratecnica-2026-prot-tipo_principal-2026-09-22-10-19-30"
RESOURCES = os.path.join(BASE, r"Assets\Resources\Varginha")
ALLIES = os.path.join(RESOURCES, "Allies")

CLEAR = np.array([0, 0, 0, 0], dtype=np.uint8)

# -------------------------------------------------------------------------
# 1. Ana Tavares: fix legs in all 16 frames
# -------------------------------------------------------------------------
def process_ana_tavares():
    path = os.path.join(ALLIES, "AnaTavares.png")
    im = Image.open(path)
    arr = np.array(im)
    
    SKIN = np.array([220, 151, 102, 255], dtype=np.uint8)
    SKIN_SHADOW = np.array([195, 130, 85, 255], dtype=np.uint8)
    SOCK_WHITE = np.array([236, 235, 226, 255], dtype=np.uint8)
    SOCK_SHADOW = np.array([210, 209, 200, 255], dtype=np.uint8)
    SHOE_BLUE = np.array([48, 104, 178, 255], dtype=np.uint8)
    SHOE_WHITE = np.array([235, 238, 235, 255], dtype=np.uint8)
    
    for d in [0, 3]: # Front (dir 0) and Back (dir 3)
        for f in range(4):
            cell = arr[d*64:(d+1)*64, f*64:(f+1)*64]
            # Clear below shorts y=42..58
            cell[42:58, :] = CLEAR
            # Clean shorts inseam notch at y=41, x=30..31
            cell[41, 30:32] = CLEAR
            
            # Leg X positions per walk frame
            if f == 0:
                lx0, lx1 = 24, 28
                rx0, rx1 = 33, 37
            elif f == 1:
                lx0, lx1 = 25, 29
                rx0, rx1 = 33, 37
            elif f == 2:
                lx0, lx1 = 24, 28
                rx0, rx1 = 33, 38
            else: # f == 3
                lx0, lx1 = 24, 28
                rx0, rx1 = 32, 36
            
            # Thighs / skin: y=42..48
            for y in range(42, 49):
                cell[y, lx0:lx1+1] = SKIN
                cell[y, lx0] = SKIN_SHADOW
                cell[y, rx0:rx1+1] = SKIN
                cell[y, rx1] = SKIN_SHADOW
                
            # Socks: y=49..53
            for y in range(49, 54):
                cell[y, lx0:lx1+1] = SOCK_WHITE
                cell[y, lx0] = SOCK_SHADOW
                cell[y, rx0:rx1+1] = SOCK_WHITE
                cell[y, rx1] = SOCK_SHADOW
                
            # Shoes: y=54..57
            for y in range(54, 57):
                cell[y, lx0:lx1+1] = SHOE_BLUE
                cell[y, rx0:rx1+1] = SHOE_BLUE
            # White soles at y=57
            cell[57, lx0:lx1+1] = SHOE_WHITE
            cell[57, rx0:rx1+1] = SHOE_WHITE
            
            # Toe shaping per frame
            if f == 0:
                cell[56:58, lx0-1] = SHOE_BLUE
                cell[56:58, rx1+1] = SHOE_BLUE
            elif f == 1:
                cell[56:58, lx1+1] = SHOE_BLUE
                cell[57, rx0:rx1+1] = SHOE_WHITE # planted
            elif f == 2:
                cell[56:58, lx0-1] = SHOE_BLUE
                cell[56:58, rx1+1] = SHOE_BLUE
            elif f == 3:
                cell[56:58, rx1+1] = SHOE_BLUE
                cell[57, lx0:lx1+1] = SHOE_WHITE
                
    # Direction 1 (Left) - ground the lead foot in f0 and f2
    for f in [0, 2]:
        cell = arr[64:128, f*64:(f+1)*64]
        # Plant forward foot to y=57
        cell[56, 22:27] = SHOE_BLUE
        cell[57, 22:27] = SHOE_WHITE
        
    out = Image.fromarray(arr)
    out.save(path)
    print("AnaTavares.png updated successfully.")

# -------------------------------------------------------------------------
# 2. Luis Miguel Messias: fix bugged shirt (clean stripes, collar, sleeves)
# -------------------------------------------------------------------------
def process_luis_messias():
    path = os.path.join(ALLIES, "LuisMiguelMessias.png")
    im = Image.open(path)
    arr = np.array(im)
    
    WHITE = np.array([236, 233, 222, 255], dtype=np.uint8)
    BLACK = np.array([20, 21, 26, 255], dtype=np.uint8)
    JEANS = np.array([23, 41, 68, 255], dtype=np.uint8)
    
    for d in [0, 3]: # Front and Back
        for f in range(4):
            cell = arr[d*64:(d+1)*64, f*64:(f+1)*64]
            # Collar at y=30..31
            for x in range(25, 39):
                if cell[30, x, 3] > 128: cell[30, x] = BLACK
                if cell[31, x, 3] > 128: cell[31, x] = BLACK
            
            # Torso stripes (y=32..43, x=24..39)
            for y in range(32, 44):
                for x in range(24, 40):
                    if cell[y, x, 3] > 128:
                        if (27 <= x <= 29) or (34 <= x <= 36):
                            cell[y, x] = WHITE
                        else:
                            cell[y, x] = BLACK
                # Short sleeves (x=18..23 and x=40..45, y<=38)
                for x in range(18, 24):
                    if cell[y, x, 3] > 128 and y <= 38:
                        cell[y, x] = BLACK
                for x in range(40, 46):
                    if cell[y, x, 3] > 128 and y <= 38:
                        cell[y, x] = BLACK
            # Clean hemline meeting navy jeans at y=44
            for y in range(44, 46):
                for x in range(23, 41):
                    if cell[y, x, 3] > 128 and cell[y, x, 0] < 50 and cell[y, x, 1] < 50:
                        cell[y, x] = JEANS
                        
    # Direction 1 & 2 (side views): clean up collar and shirt boundary
    for d in [1, 2]:
        for f in range(4):
            cell = arr[d*64:(d+1)*64, f*64:(f+1)*64]
            for y in range(32, 44):
                for x in range(22, 42):
                    if cell[y, x, 3] > 128 and cell[y, x, 0] > 180 and cell[y, x, 1] > 180:
                        cell[y, x] = WHITE
            for y in range(44, 46):
                for x in range(22, 42):
                    if cell[y, x, 3] > 128 and cell[y, x, 0] < 50 and cell[y, x, 1] < 50:
                        cell[y, x] = JEANS
                        
    out = Image.fromarray(arr)
    out.save(path)
    print("LuisMiguelMessias.png updated successfully.")

# -------------------------------------------------------------------------
# 3. Edelzio: consistent head and mustard yellow shirt across walk frames
# -------------------------------------------------------------------------
def process_edelzio():
    path = os.path.join(ALLIES, "Edelzio.png")
    im = Image.open(path)
    arr = np.array(im)
    
    # In each direction d, frame 0 is the canonical standing idle ("Edelzio parado").
    # For walking frames 1, 2, 3: preserve the clean head (y=15..34) from f0 with natural bob.
    # Also ensure yellow shirt colors (y=35..45) match the vibrant mustard yellow.
    for d in range(4):
        f0 = arr[d*64:(d+1)*64, 0:64]
        head_f0 = f0[14:35, 18:46].copy()
        
        # Bobs per frame in walk cycle: f0: 0, f1: -1, f2: 0, f3: 1
        bobs = [0, 0, 0, 0] # Keep head position steady to eliminate flickering
        for f in range(1, 4):
            cell = arr[d*64:(d+1)*64, f*64:(f+1)*64]
            # Replace head area with canonical f0 head
            for y in range(14, 35):
                for x in range(18, 46):
                    src_p = head_f0[y-14, x-18]
                    if src_p[3] > 128:
                        cell[y, x] = src_p
                    elif cell[y, x, 3] > 128 and y < 32:
                        cell[y, x] = CLEAR
            
            # Normalize yellow shirt pixels across y=35..45
            for y in range(35, 46):
                for x in range(18, 46):
                    p = cell[y, x]
                    if p[3] > 128:
                        # If pixel is yellow/shirt
                        if p[0] > 180 and p[1] > 110 and p[2] < 90:
                            # match mustard yellow
                            cell[y, x, 0] = min(255, int(p[0] * 1.05))
                            cell[y, x, 1] = max(130, min(165, int(p[1])))
                            cell[y, x, 2] = min(55, p[2])
                            
    out = Image.fromarray(arr)
    out.save(path)
    print("Edelzio.png updated successfully.")

# -------------------------------------------------------------------------
# 4. EdelzioPunchV2: match Edelzio parado for idle/guard frames & texture
# -------------------------------------------------------------------------
def process_edelzio_punch():
    punch_path = os.path.join(RESOURCES, "EdelzioPunchV2.png")
    edelzio_path = os.path.join(ALLIES, "Edelzio.png")
    
    p_im = Image.open(punch_path)
    p_arr = np.array(p_im)
    
    e_im = Image.open(edelzio_path)
    e_arr = np.array(e_im)
    
    # In EdelzioPunchV2: 4 rows (directions 0..3), 18 cols each.
    # Combos:
    # Combo 0: cols 0..5. (col 0: idle guard, col 5: return to guard)
    # Combo 1: cols 6..11. (col 6: idle guard, col 11: return to guard)
    # Combo 2: cols 12..17. (col 12: idle guard, col 17: return to guard)
    for d in range(4):
        # Canonical standing frame of Edelzio
        standing = e_arr[d*64:(d+1)*64, 0:64]
        
        # Idle/guard frames in punch sheet
        for col in [0, 5, 6, 11, 12, 17]:
            p_arr[d*64:(d+1)*64, col*64:(col+1)*64] = standing
            
        # For the active punch frames (windup, extension, contact, followthrough):
        # Harmonize shirt and pants palette to match standing Edelzio:
        # shirt: mustard yellow [228, 145, 52]
        # pants: dark charcoal [32, 31, 33]
        for col in range(18):
            if col in [0, 5, 6, 11, 12, 17]:
                continue
            cell = p_arr[d*64:(d+1)*64, col*64:(col+1)*64]
            for y in range(64):
                for x in range(64):
                    if cell[y, x, 3] > 128:
                        r, g, b = cell[y, x, :3]
                        # Shirt tones
                        if r > 160 and g > 90 and b < 90:
                            cell[y, x, 0] = min(255, max(190, r))
                            cell[y, x, 1] = min(170, max(120, g))
                            cell[y, x, 2] = min(60, b)
                        # Dark pants tones
                        elif r < 60 and g < 60 and b < 60 and y >= 42:
                            cell[y, x, 0] = 32
                            cell[y, x, 1] = 31
                            cell[y, x, 2] = 33
                            
    out = Image.fromarray(p_arr)
    out.save(punch_path)
    print("EdelzioPunchV2.png updated successfully.")

if __name__ == "__main__":
    process_ana_tavares()
    process_luis_messias()
    process_edelzio()
    process_edelzio_punch()
