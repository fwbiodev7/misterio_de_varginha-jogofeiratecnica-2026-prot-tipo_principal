"""
Generates production 1254x1254 pixel art sheet for Fabio matching the chibi aesthetic
and proportions of Yasmin, Pedro, Marcos, and Matias, incorporating the user's reference:
- Long wavy dark emo/metal hair
- No glasses, clear expressive chibi eyes
- Black t-shirt with Metallica Ride the Lightning graphic (electric blue lightning)
- Studded black wristbands
- Silver chain necklace
- Black cargo pants with silver chain loop
"""
from PIL import Image, ImageDraw
import numpy as np

def create_fabio():
    orig = Image.open('scratch/fabio_original.png').convert('RGBA')
    pedro = Image.open('Assets/ArtSource/Allies/Pedro.png').convert('RGBA')
    
    o_arr = np.array(orig)
    p_arr = np.array(pedro)
    
    h, w = o_arr.shape[:2]
    cw, ch = w // 4, h // 4
    
    res = np.zeros_like(o_arr)
    
    for row in range(4):
        for col in range(4):
            yb = row * ch
            xb = col * cw
            o_cell = o_arr[yb:yb+ch, xb:xb+cw].copy()
            p_cell = p_arr[yb:yb+ch, xb:xb+cw].copy()
            
            # --- 1. REMOVE GLASSES (Row 0: Front) ---
            if row == 0:
                # The rectangular frame lines are in y in [92..120]
                # Replace black glasses frames with skin/eye colors
                for y in range(92, 120):
                    for x in range(100, 215):
                        r, g, b, a = o_cell[y, x]
                        if a > 128:
                            # If it's a black glasses frame pixel (not eyebrow, not pupil)
                            # Pupils are at x in [124..136] and [176..188], y in [103..114]
                            is_pupil = (y in range(103, 115) and (abs(x - 130) <= 6 or abs(x - 182) <= 6))
                            is_eyebrow = (y in range(92, 100) and (abs(x - 130) <= 10 or abs(x - 182) <= 10))
                            is_bridge = (y in range(102, 108) and abs(x - 156) <= 6)
                            is_outer_frame = ((abs(x - 116) <= 3 or abs(x - 144) <= 3 or abs(x - 168) <= 3 or abs(x - 196) <= 3) and y in range(100, 118))
                            is_bottom_frame = (y in range(115, 119) and (abs(x - 130) <= 14 or abs(x - 182) <= 14))
                            is_top_frame = (y in range(100, 104) and (abs(x - 130) <= 14 or abs(x - 182) <= 14) and not is_eyebrow)
                            
                            if is_bridge or is_outer_frame or is_bottom_frame or is_top_frame:
                                if not is_pupil and not is_eyebrow:
                                    # Replace with smooth skin tone
                                    o_cell[y, x] = [248, 172, 114, 255]
            
            # --- 2. HAIR: Long wavy metalhead hair (combine and adapt Pedro's wavy volume recolored to dark charcoal/black) ---
            # Pedro's hair pixels: in Pedro cell, hair is in y in [30..170]
            # Dark charcoal palette:
            # Highlight: [62, 56, 68]
            # Midtone:   [40, 36, 46]
            # Shadow:    [26, 23, 30]
            # Outline:   [14, 12, 16]
            for y in range(25, 175):
                for x in range(cw):
                    pr, pg, pb, pa = p_cell[y, x]
                    if pa > 128:
                        # Check if Pedro pixel is hair
                        p_is_skin = (pr > 160 and pg > 110 and pb > 80 and pr > pg and pg >= pb and (pr - pb) > 30)
                        p_is_glasses = (pr < 30 and pg < 30 and pb < 30 and y in range(90, 122) and row != 3)
                        p_is_clothing = (y >= 148 and (pb > pr or pr > 120)) # Pedro's blue jacket or burgundy shirt
                        
                        if not p_is_skin and not p_is_glasses and not p_is_clothing:
                            # It's Pedro's hair! Recolor to Fabio's dark wavy hair:
                            lum = (int(pr) + int(pg) + int(pb)) / 3.0
                            if lum < 30:
                                hr, hg, hb = 16, 14, 18
                            elif lum < 55:
                                hr, hg, hb = 28, 25, 32
                            elif lum < 85:
                                hr, hg, hb = 42, 38, 48
                            else:
                                hr, hg, hb = 62, 56, 70
                            
                            # Blend/apply hair over Fabio
                            # On front view, only cover if not eye/face skin
                            if row == 0:
                                o_skin = (o_cell[y, x, 0] > 160 and o_cell[y, x, 1] > 110 and o_cell[y, x, 2] > 80 and o_cell[y, x, 3] > 128)
                                o_eye = (y in range(100, 118) and abs(x - 156) < 35)
                                # Keep forehead and face clear, let wavy hair frame the sides
                                if o_eye:
                                    continue
                                if o_skin and y in range(80, 145) and abs(x - 156) < 26:
                                    continue
                            o_cell[y, x] = [hr, hg, hb, pa]
            
            # --- 3. SHIRT: Black rock t-shirt with Metallica Ride the Lightning graphic ---
            # Torso region: y in [148..225]
            for y in range(148, 226):
                for x in range(cw):
                    r, g, b, a = o_cell[y, x]
                    if a < 128:
                        continue
                    is_skin = (r > 160 and g > 110 and b > 80 and r > g and g >= b and (r - b) > 30)
                    if not is_skin:
                        # Make base shirt a deep charcoal black (#18181c / #24242a)
                        lum = (int(r) + int(g) + int(b)) / 3.0
                        if lum < 20:
                            o_cell[y, x] = [14, 14, 16, a]
                        else:
                            sr = int(22 + lum * 0.15)
                            sg = int(22 + lum * 0.15)
                            sb = int(26 + lum * 0.18)
                            o_cell[y, x] = [sr, sg, sb, a]
            
            # Chest graphic on Front (row 0):
            if row == 0:
                # Center is x=156, chest is y in [165..195], width around 24px (x in [144..168])
                for gy in range(166, 195):
                    for gx in range(145, 168):
                        if o_cell[gy, gx, 3] > 128:
                            # Check if not outer edge
                            # Draw electric blue lightning / rock graphic
                            rel_y = gy - 166
                            rel_x = gx - 145
                            # Lightning bolt pattern
                            is_lightning = ((rel_x + rel_y // 2) % 6 in (0, 1) and rel_y > 4) or (rel_y in (1, 2) and rel_x in range(4, 19))
                            is_glow = ((rel_x + rel_y // 2) % 6 in (2, 5))
                            
                            if is_lightning:
                                # Bright electric blue / white lightning
                                o_cell[gy, gx] = [170, 225, 255, 255]
                            elif is_glow:
                                # Electric blue midtone
                                o_cell[gy, gx] = [60, 140, 230, 255]
                            else:
                                # Dark blue album art background
                                o_cell[gy, gx] = [20, 50, 95, 255]
                
                # Silver chain necklace around neck (y in [152..158], x in [148..164])
                for nx in range(149, 164):
                    ny = 153 + abs(nx - 156) // 2
                    if o_cell[ny, nx, 3] > 128:
                        o_cell[ny, nx] = [205, 215, 228, 255]
                # Cross pendant
                o_cell[158, 156] = [220, 230, 245, 255]
                o_cell[159, 156] = [220, 230, 245, 255]
                o_cell[160, 156] = [220, 230, 245, 255]
                o_cell[159, 155] = [220, 230, 245, 255]
                o_cell[159, 157] = [220, 230, 245, 255]
                
                # Spiked wristbands on hands/wrists (y in [202..214], wrists are at edges of hands)
                # Left hand (screen left): x in [95..108]
                # Right hand (screen right): x in [204..217]
                for wy in range(200, 214):
                    for wx in list(range(94, 108)) + list(range(204, 218)):
                        if o_cell[wy, wx, 3] > 128:
                            # Check if boundary between sleeve and hand
                            r, g, b, a = o_cell[wy, wx]
                            is_skin = (r > 160 and g > 110 and b > 80)
                            if is_skin and wy in range(201, 207):
                                # Studded wristband
                                if wx % 3 == 0:
                                    o_cell[wy, wx] = [215, 225, 235, 255] # silver stud
                                else:
                                    o_cell[wy, wx] = [18, 18, 20, 255] # black leather band
                
                # Silver chain on right hip (x in [168..178], y in [224..238])
                for cy in range(225, 238):
                    cx = 170 + int(np.sin((cy - 225) / 2.0) * 3)
                    if 0 <= cx < cw and o_cell[cy, cx, 3] > 128:
                        o_cell[cy, cx] = [200, 210, 225, 255]
            
            # --- 4. PANTS: Dark black cargo pants ---
            for y in range(226, 285):
                for x in range(cw):
                    r, g, b, a = o_cell[y, x]
                    if a > 128:
                        lum = (int(r) + int(g) + int(b)) / 3.0
                        if lum < 18:
                            o_cell[y, x] = [12, 12, 14, a]
                        else:
                            # Rich dark charcoal denim / cargo
                            pr = int(24 + lum * 0.18)
                            pg = int(24 + lum * 0.18)
                            pb = int(28 + lum * 0.20)
                            o_cell[y, x] = [pr, pg, pb, a]
            
            # --- 5. BOOTS: Chunky dark boots ---
            for y in range(285, min(310, ch)):
                for x in range(cw):
                    r, g, b, a = o_cell[y, x]
                    if a > 128:
                        lum = (int(r) + int(g) + int(b)) / 3.0
                        if lum < 18:
                            o_cell[y, x] = [10, 10, 12, a]
                        else:
                            o_cell[y, x] = [22, 22, 26, a]
            
            res[yb:yb+ch, xb:xb+cw] = o_cell
            
    out_img = Image.fromarray(res, 'RGBA')
    out_img.save('Assets/ArtSource/Allies/Fabio.png')
    print('Generated Assets/ArtSource/Allies/Fabio.png')
    
    # Save preview
    preview = Image.new('RGBA', (320, 160), (30, 35, 45, 255))
    c_front = out_img.crop((0, 0, cw, ch)).resize((140, 140), Image.NEAREST)
    c_back  = out_img.crop((0, 3*ch, cw, 4*ch)).resize((140, 140), Image.NEAREST)
    preview.paste(c_front, (12, 10), c_front)
    preview.paste(c_back, (168, 10), c_back)
    preview.save('scratch/preview_fabio.png')
    print('Saved scratch/preview_fabio.png')

if __name__ == '__main__':
    create_fabio()
