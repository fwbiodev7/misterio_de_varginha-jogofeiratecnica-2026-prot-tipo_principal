import os
from PIL import Image
import numpy as np

BASE = r"c:\Users\Usuario\misterio_de_varginha-jogofeiratecnica-2026-prot-tipo_principal-2026-09-22-10-19-30"
RESOURCES_ALLIES = os.path.join(BASE, r"Assets\Resources\Varginha\Allies")
ARTSOURCE_ALLIES = os.path.join(BASE, r"Assets\ArtSource\Allies")

def remove_glasses_marcos():
    path = os.path.join(RESOURCES_ALLIES, "Marcos.png")
    im = Image.open(path)
    arr = np.array(im)
    
    SKIN = np.array([232, 151, 85, 255], dtype=np.uint8)
    LID = np.array([160, 95, 55, 255], dtype=np.uint8)
    SCLERA = np.array([240, 235, 230, 255], dtype=np.uint8)
    PUPIL = np.array([35, 25, 20, 255], dtype=np.uint8)
    
    # 1. Direction 0 (Front) - all 4 walk frames
    for f in range(4):
        cell = arr[0:64, f*64:(f+1)*64]
        bob = 1 if f == 3 else 0
        y_top = 24 + bob
        
        # Upper eyelid line
        cell[y_top, 25] = SKIN
        cell[y_top, 26:29] = LID
        cell[y_top, 29:33] = SKIN
        cell[y_top, 33:36] = LID
        cell[y_top, 36] = SKIN
        
        # Eyes lines
        for y in [y_top + 1, y_top + 2]:
            cell[y, 25] = SKIN
            cell[y, 26] = SCLERA
            cell[y, 27:29] = PUPIL
            cell[y, 29:33] = SKIN
            cell[y, 33:35] = PUPIL
            cell[y, 35] = SCLERA
            cell[y, 36] = SKIN
            
        # Lower eye / cheeks (remove glasses lower rim and reflections)
        cell[y_top + 3, 25:37] = SKIN
        cell[y_top + 3, 27] = LID
        cell[y_top + 3, 34] = LID
        cell[y_top + 4, 25:37] = SKIN
        
    # 2. Direction 1 (Left profile) - remove glasses temple/rim
    for f in range(4):
        cell = arr[64:128, f*64:(f+1)*64]
        bob = 0
        y_top = 24 + bob
        for y in [y_top, y_top + 1]:
            cell[y, 22] = PUPIL
            cell[y, 23] = SCLERA
            cell[y, 24:28] = SKIN
        cell[y_top + 2, 22:28] = SKIN
        cell[y_top + 3, 22:28] = SKIN
        
    # 3. Direction 2 (Right profile) - remove glasses temple/rim
    for f in range(4):
        cell = arr[128:192, f*64:(f+1)*64]
        bob = 1 if f in [1, 3] else 0
        y_top = 24 + bob
        for y in [y_top, y_top + 1]:
            cell[y, 36:40] = SKIN
            cell[y, 40] = SCLERA
            cell[y, 41] = PUPIL
        cell[y_top + 2, 36:42] = SKIN
        cell[y_top + 3, 36:42] = SKIN
        
    out = Image.fromarray(arr)
    out.save(path)
    print("Marcos.png in Resources updated without glasses.")
    sync_artsource("Marcos.png", out)

def remove_glasses_luis_martins():
    path = os.path.join(RESOURCES_ALLIES, "LuisMartins.png")
    im = Image.open(path)
    arr = np.array(im)
    
    SKIN = np.array([251, 175, 110, 255], dtype=np.uint8)
    LID = np.array([200, 130, 80, 255], dtype=np.uint8)
    SCLERA = np.array([245, 240, 238, 255], dtype=np.uint8)
    PUPIL = np.array([35, 20, 15, 255], dtype=np.uint8)
    
    # 1. Direction 0 (Front) - all 4 walk frames
    for f in range(4):
        cell = arr[0:64, f*64:(f+1)*64]
        bob = 0
        y_top = 24 + bob
        
        # Upper eyelid line
        cell[y_top, 25] = SKIN
        cell[y_top, 26:29] = LID
        cell[y_top, 29:33] = SKIN
        cell[y_top, 33:36] = LID
        cell[y_top, 36] = SKIN
        
        # Eyes lines
        for y in [y_top + 1, y_top + 2]:
            cell[y, 25] = SKIN
            cell[y, 26] = SCLERA
            cell[y, 27:29] = PUPIL
            cell[y, 29:33] = SKIN
            cell[y, 33:35] = PUPIL
            cell[y, 35] = SCLERA
            cell[y, 36] = SKIN
            
        # Lower eye / cheeks (remove glasses lower rim and reflections)
        cell[y_top + 3, 25:37] = SKIN
        cell[y_top + 3, 27] = LID
        cell[y_top + 3, 34] = LID
        cell[y_top + 4, 25:37] = SKIN
        
    # 2. Direction 1 (Left profile) - remove glasses temple/rim
    for f in range(4):
        cell = arr[64:128, f*64:(f+1)*64]
        bob = 1 if f in [1, 3] else 0
        y_top = 24 + bob
        for y in [y_top, y_top + 1]:
            cell[y, 22] = PUPIL
            cell[y, 23] = SCLERA
            cell[y, 24:28] = SKIN
        cell[y_top + 2, 22:28] = SKIN
        cell[y_top + 3, 22:28] = SKIN
        
    # 3. Direction 2 (Right profile) - remove glasses temple/rim
    for f in range(4):
        cell = arr[128:192, f*64:(f+1)*64]
        bob = 0
        y_top = 24 + bob
        for y in [y_top, y_top + 1]:
            cell[y, 36:40] = SKIN
            cell[y, 40] = SCLERA
            cell[y, 41] = PUPIL
        cell[y_top + 2, 36:42] = SKIN
        cell[y_top + 3, 36:42] = SKIN
        
    out = Image.fromarray(arr)
    out.save(path)
    print("LuisMartins.png in Resources updated without glasses.")
    sync_artsource("LuisMartins.png", out)

def sync_artsource(filename, res_im):
    art_path = os.path.join(ARTSOURCE_ALLIES, filename)
    if not os.path.exists(art_path):
        return
    art_im = Image.new('RGBA', (1254, 1254))
    for row in range(4):
        for col in range(4):
            cell_64 = res_im.crop((col*64, row*64, (col+1)*64, (row+1)*64))
            bbox = cell_64.getbbox()
            if not bbox: continue
            cropped = cell_64.crop(bbox)
            scale = 260 / cropped.height
            new_w = round(cropped.width * scale)
            new_h = round(cropped.height * scale)
            scaled = cropped.resize((new_w, new_h), Image.Resampling.NEAREST)
            cell_x = col * 1254 // 4 + (313 - new_w) // 2
            cell_y = row * 1254 // 4 + (313 - new_h) // 2
            art_im.alpha_composite(scaled, (cell_x, cell_y))
    art_im.save(art_path)
    print(f"{filename} in ArtSource updated.")

if __name__ == "__main__":
    remove_glasses_marcos()
    remove_glasses_luis_martins()
