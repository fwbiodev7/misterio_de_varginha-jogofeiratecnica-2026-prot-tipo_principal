from PIL import Image

def build_backpack_preview():
    src = Image.open('Assets/Resources/Varginha/EdelzioTopDownV3.png')
    frames = [src.crop((0, r * 64, 64, (r + 1) * 64)) for r in range(4)]
    
    # Hiking Pack Palette
    outline = (28, 32, 38, 255)
    fabric = (115, 122, 130, 255)
    fabric_light = (135, 142, 150, 255)
    shadow = (72, 78, 86, 255)
    highlight = (165, 172, 180, 255)
    roll = (140, 148, 155, 255)
    roll_hi = (185, 192, 198, 255)
    roll_sh = (58, 64, 72, 255)
    buckle = (215, 222, 228, 255)
    strap = (50, 55, 62, 255)

    def is_shirt(c):
        r, g, b, a = c
        return a > 128 and r > 120 and g > 75 and g < r * 0.83 and b < g * 0.45

    def is_skin(c):
        r, g, b, a = c
        return a > 128 and r > 150 and g > 90 and b >= g * 0.45

    def is_hair(c):
        r, g, b, a = c
        return a > 128 and r < 80 and g < 60 and b < 50

    equipped = []
    for direction in range(4):
        f = frames[direction].copy()
        orig = frames[direction].load()
        pix = f.load()
        
        # 0: Down (Front view)
        if direction == 0:
            # Subtle top roll hint peeking behind shoulders
            for x, y in [(25, 30), (26, 30), (37, 30), (38, 30)]:
                if orig[x, y][3] == 0:
                    pix[x, y] = roll_sh
            # Straps over chest
            for y in range(32, 43):
                # Left strap
                for x in (26, 27):
                    if is_shirt(orig[x, y]):
                        pix[x, y] = strap if x == 26 else highlight if y in (33, 37) else fabric
                # Right strap
                for x in (36, 37):
                    if is_shirt(orig[x, y]):
                        pix[x, y] = strap if x == 37 else highlight if y in (33, 37) else fabric
                # Sternum chest clip at y=36
                if y == 36:
                    for x in range(28, 36):
                        if is_shirt(orig[x, y]):
                            pix[x, y] = buckle if x in (31, 32) else strap

        # 3: Up (Back view)
        elif direction == 3:
            # Back view: centered on torso
            # x=25..39, y=28..42 with rounded corners
            for py in range(28, 43):
                for px in range(25, 40):
                    # Cut 4 extreme corners for rounded silhouette
                    if (px in (25, 39) and py in (28, 42)) or (px in (25, 39) and py == 29):
                        continue
                    # Outer outline
                    is_edge = (px == 25 and py > 29 and py < 42) or (px == 39 and py > 29 and py < 42) or \
                              (py == 28 and px > 25 and px < 39) or (py == 42 and px > 25 and px < 39) or \
                              (px == 26 and (py == 29 or py == 42)) or (px == 38 and (py == 29 or py == 42))
                    if is_edge:
                        pix[px, py] = outline
                    elif py in (29, 30):
                        # Top bedroll / sleeping mat
                        if px in (28, 36):
                            pix[px, py] = roll_sh
                        elif py == 29:
                            pix[px, py] = roll_hi
                        else:
                            pix[px, py] = roll
                    elif py == 31:
                        pix[px, py] = outline
                    elif py in (32, 33, 34):
                        # Upper flap
                        if px in (28, 36):
                            pix[px, py] = strap
                        elif px > 34:
                            pix[px, py] = shadow
                        elif py == 32:
                            pix[px, py] = highlight
                        else:
                            pix[px, py] = fabric_light if px < 30 else fabric
                    elif py == 35:
                        pix[px, py] = buckle if px in (28, 36) else outline
                    else:
                        # Main compartment & pockets
                        if px in (28, 36):
                            pix[px, py] = strap
                        elif px in (31, 32, 33) and py in (38, 39):
                            pix[px, py] = buckle if py == 38 else shadow
                        elif px > 35:
                            pix[px, py] = shadow
                        elif py == 41:
                            pix[px, py] = shadow
                        else:
                            pix[px, py] = fabric

        # 1: Left (Side view - walking left)
        elif direction == 1:
            # Back is on right (x=38..47, y=29..42)
            for py in range(29, 43):
                for px in range(38, 48):
                    # Cut outer corners
                    if (px == 47 and py in (29, 42)) or (px == 47 and py == 30):
                        continue
                    is_edge = (px == 47 and py > 30 and py < 42) or (py == 29 and px >= 39 and px < 47) or \
                              (py == 42 and px >= 39 and px < 47) or (px == 46 and py == 30)
                    if is_edge:
                        pix[px, py] = outline
                    elif py in (30, 31):
                        # Roll end
                        if px in (44, 45):
                            pix[px, py] = roll_sh
                        elif py == 30:
                            pix[px, py] = roll_hi
                        else:
                            pix[px, py] = roll
                    elif py == 32:
                        pix[px, py] = outline
                    elif py in (33, 34, 35):
                        if px == 43:
                            pix[px, py] = strap
                        elif px >= 44:
                            pix[px, py] = shadow
                        elif py == 33:
                            pix[px, py] = highlight
                        else:
                            pix[px, py] = fabric
                    elif py == 36:
                        pix[px, py] = buckle if px == 43 else outline
                    else:
                        if px == 43:
                            pix[px, py] = strap
                        elif px >= 45 or py >= 41:
                            pix[px, py] = shadow
                        else:
                            pix[px, py] = fabric

            # Ensure Edelzio's front/arm stays in front
            for py in range(64):
                for px in range(64):
                    c = orig[px, py]
                    if c[3] > 0 and (is_skin(c) or px <= 38):
                        pix[px, py] = c

        # 2: Right (Side view - walking right)
        elif direction == 2:
            # Back is on left (x=16..26, y=29..42)
            for py in range(29, 43):
                for px in range(17, 27):
                    # Cut outer corners
                    if (px == 17 and py in (29, 42)) or (px == 17 and py == 30):
                        continue
                    is_edge = (px == 17 and py > 30 and py < 42) or (py == 29 and px > 17 and px <= 25) or \
                              (py == 42 and px > 17 and px <= 25) or (px == 18 and py == 30)
                    if is_edge:
                        pix[px, py] = outline
                    elif py in (30, 31):
                        # Roll end
                        if px in (19, 20):
                            pix[px, py] = roll_sh
                        elif py == 30:
                            pix[px, py] = roll_hi
                        else:
                            pix[px, py] = roll
                    elif py == 32:
                        pix[px, py] = outline
                    elif py in (33, 34, 35):
                        if px == 21:
                            pix[px, py] = strap
                        elif px <= 19:
                            pix[px, py] = shadow
                        elif py == 33:
                            pix[px, py] = highlight
                        else:
                            pix[px, py] = fabric
                    elif py == 36:
                        pix[px, py] = buckle if px == 21 else outline
                    else:
                        if px == 21:
                            pix[px, py] = strap
                        elif px <= 19 or py >= 41:
                            pix[px, py] = shadow
                        else:
                            pix[px, py] = fabric

            # Ensure Edelzio's front/arm stays in front
            for py in range(64):
                for px in range(64):
                    c = orig[px, py]
                    if c[3] > 0 and (is_skin(c) or px >= 26):
                        pix[px, py] = c

        equipped.append(f)

    # Assemble 2 rows: original (top), equipped (bottom)
    out = Image.new('RGBA', (256, 128), (0, 0, 0, 0))
    for i in range(4):
        out.paste(frames[i], (i * 64, 0))
        out.paste(equipped[i], (i * 64, 64))
    
    out_scaled = out.resize((256 * 3, 128 * 3), Image.NEAREST)
    out_scaled.save('Logs/BackpackQA/preview_test.png')
    out_scaled.save('C:/Users/Usuario/.gemini/antigravity-ide/brain/ae68f180-2763-4253-acd2-be2e5d71059b/edelzio-gray-backpack.png')
    print('Refined preview generated successfully')

if __name__ == '__main__':
    build_backpack_preview()
