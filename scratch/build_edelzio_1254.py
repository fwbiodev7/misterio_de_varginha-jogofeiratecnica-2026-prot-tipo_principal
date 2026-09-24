"""
Generates production 1254x1254 pixel art sheets for Edelzio and Fabio matching the exact
chibi anatomy and scale of Yasmin, Pedro, Marcos, and Matias.
"""
from PIL import Image, ImageDraw, ImageFilter
import numpy as np

def create_edelzio():
    orig = Image.open('scratch/fabio_original.png').convert('RGBA')
    arr = np.array(orig)
    
    # Edelzio has:
    # 1. Warm brown hair (#5c3a22, #7a4e2f, #3d2414)
    # 2. Black glasses
    # 3. Goatee / beard (cavanhaque e bigode) around mouth
    # 4. Mustard-yellow / golden t-shirt (#d49830, #ebb042, #a06c1c, outline #4a300a)
    # 5. Black watch on left wrist (#1e1e24 with cyan dot)
    # 6. Dark charcoal jeans (#282a30, #363942)
    # 7. Black sneakers with crisp white sole trim (#ffffff)
    
    # Let's inspect original Fabio colors to map them cleanly:
    # The original jacket was black/dark gray (#151515 .. #353535)
    # Let's transform the torso (jacket/shirt) into a solid mustard-yellow t-shirt.
    # In each cell, torso is roughly y in [148..225] in cell-local coords.
    
    h, w = arr.shape[:2]
    cw, ch = w // 4, h // 4
    
    for row in range(4):
        for col in range(4):
            y_base = row * ch
            x_base = col * cw
            cell = arr[y_base:y_base+ch, x_base:x_base+cw]
            
            # --- 1. TORSO: Mustard yellow t-shirt ---
            # Torso region: local y from 148 to 226
            for y in range(148, 226):
                for x in range(cw):
                    r, g, b, a = cell[y, x]
                    if a < 128:
                        continue
                    
                    # Distinguish skin (hands, neck) from clothing
                    # Skin in original has r > 180, g in [120..180], b in [90..150], r > g > b
                    is_skin = (r > 160 and g > 110 and b > 80 and r > g and g >= b and (r - b) > 30)
                    is_black_outline = (r < 25 and g < 25 and b < 25)
                    
                    # If it's clothing (dark grayscale jacket/shirt):
                    if not is_skin:
                        lum = (int(r) + int(g) + int(b)) / 3.0
                        if is_black_outline:
                            # Keep dark outline, slightly warmer dark brown
                            cell[y, x] = [35, 22, 10, a]
                        else:
                            # Map luminosity (typically 20..100) to mustard yellow
                            # Shadow: [160, 108, 28], Mid: [212, 152, 48], High: [238, 178, 68]
                            t = np.clip((lum - 20) / 60.0, 0.0, 1.0)
                            yr = int(160 + t * 78)
                            yg = int(108 + t * 70)
                            yb = int(28 + t * 40)
                            cell[y, x] = [yr, yg, yb, a]
            
            # --- 2. HAIR: Warm chestnut brown ---
            # Hair is mainly in y in [35..140]
            for y in range(35, 140):
                for x in range(cw):
                    r, g, b, a = cell[y, x]
                    if a < 128:
                        continue
                    is_skin = (r > 160 and g > 110 and b > 80 and r > g and g >= b and (r - b) > 30)
                    is_glasses = (r < 30 and g < 30 and b < 30 and y in range(92, 120))
                    
                    # Hair in original is dark brownish-gray
                    if not is_skin and not is_glasses:
                        lum = (int(r) + int(g) + int(b)) / 3.0
                        if lum < 25:
                            cell[y, x] = [40, 24, 15, a]
                        else:
                            t = np.clip((lum - 25) / 55.0, 0.0, 1.0)
                            hr = int(75 + t * 45)
                            hg = int(45 + t * 30)
                            hb = int(25 + t * 20)
                            cell[y, x] = [hr, hg, hb, a]
            
            # --- 3. FACE & BEARD: Add goatee & mustache on Front (row 0) and Side (row 1, 2) ---
            if row == 0:
                # Front view goatee: around mouth y in [130..148], x centered around 156
                # Center is around x=156
                # Mouth is around y=134, chin is at y=144
                for gy in range(131, 148):
                    for gx in range(144, 169):
                        r, g, b, a = cell[gy, gx]
                        if a > 128:
                            # Mustache above mouth (y 131..134, x 147..165)
                            is_mustache = (gy in (131, 132, 133) and abs(gx - 156) <= 9)
                            # Goatee below mouth / chin (y 138..147, x 148..164)
                            is_chin_beard = (gy >= 137 and abs(gx - 156) <= (6 if gy < 144 else 8))
                            # Soul patch (y 135..137, x 154..158)
                            is_soul_patch = (gy in (135, 136) and abs(gx - 156) <= 2)
                            
                            if is_mustache or is_chin_beard or is_soul_patch:
                                # Warm dark brown goatee
                                cell[gy, gx] = [62, 38, 22, 255]
                                if gy == 147 or abs(gx - 156) >= 7:
                                    cell[gy, gx] = [40, 24, 14, 255] # border
            elif row in (1, 2):
                # Side view goatee: near the chin profile
                # Row 1 is left, row 2 is right
                # Find profile edge around y in [132..147]
                for gy in range(133, 147):
                    vis_x = np.where(cell[gy, :, 3] > 128)[0]
                    if len(vis_x) > 0:
                        edge_x = vis_x.min() if row == 1 else vis_x.max()
                        dx = 1 if row == 1 else -1
                        # Place beard pixels along chin profile
                        for offset in range(1, 7):
                            bx = edge_x + offset * dx
                            if 0 <= bx < cw and cell[gy, bx, 3] > 128:
                                cell[gy, bx] = [58, 36, 20, 255]
            
            # --- 4. SNEAKERS: White sole trim ---
            # Soles are at bottom of shoes: local y in [292..302]
            for y in range(292, min(303, ch)):
                for x in range(cw):
                    r, g, b, a = cell[y, x]
                    if a > 128:
                        # Check if this pixel is near the bottom edge of the foot
                        # Bottom-most 3-4 pixels of visible foot
                        below_alpha = cell[min(y+2, ch-1), x, 3]
                        if below_alpha < 128 and y >= 293:
                            cell[y, x] = [235, 238, 242, 255]
            
            arr[y_base:y_base+ch, x_base:x_base+cw] = cell
            
    out_img = Image.fromarray(arr, 'RGBA')
    out_img.save('Assets/ArtSource/Allies/Edelzio.png')
    print('Generated Assets/ArtSource/Allies/Edelzio.png')
    
    # Save a small preview of cell 0 (front) and cell 12 (back)
    preview = Image.new('RGBA', (320, 160), (30, 35, 45, 255))
    c_front = out_img.crop((0, 0, cw, ch)).resize((140, 140), Image.NEAREST)
    c_back  = out_img.crop((0, 3*ch, cw, 4*ch)).resize((140, 140), Image.NEAREST)
    preview.paste(c_front, (12, 10), c_front)
    preview.paste(c_back, (168, 10), c_back)
    preview.save('scratch/preview_edelzio.png')
    print('Saved scratch/preview_edelzio.png')

if __name__ == '__main__':
    create_edelzio()
