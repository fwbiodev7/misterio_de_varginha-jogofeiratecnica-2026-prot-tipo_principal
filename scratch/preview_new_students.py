
from PIL import Image
import math

size = 64

def draw_student(name, shirt_col):
    im = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
    def pixel(x, y, col):
        if 0 <= x < 32 and 0 <= y < 32 and col[3] > 0:
            im.putpixel((x, 31 - y), col)
    def fill(x, y, w, h, col):
        for xx in range(x, x + w):
            for yy in range(y, y + h):
                pixel(xx, yy, col)
                
    ink = (16, 18, 30, 255)
    denim = (44, 56, 88, 255)
    
    # Palette definition
    if 'Yasmin' in name:
        skin = (248, 226, 214, 255)
        hair = (18, 16, 22, 255)
        hair_light = (52, 46, 62, 255)
    elif 'Pedro' in name:
        skin = (226, 175, 134, 255)
        hair = (52, 34, 22, 255)
        hair_light = (112, 70, 44, 255)
    elif 'Matias' in name:
        skin = (200, 144, 104, 255)
        hair = (30, 26, 26, 255)
        hair_light = (72, 60, 52, 255)
    elif 'Marcos' in name:
        skin = (124, 78, 52, 255)
        hair = (20, 18, 20, 255)
        hair_light = (52, 44, 46, 255)
    elif 'Anna' in name or 'Sabia' in name:
        skin = (222, 168, 128, 255)
        hair = (42, 28, 20, 255)
        hair_light = (98, 62, 42, 255)
    elif 'Tavares' in name:
        skin = (170, 110, 75, 255)
        hair = (36, 24, 20, 255)
        hair_light = (82, 52, 38, 255)
    elif 'Messias' in name:
        skin = (204, 140, 102, 255)
        hair = (32, 24, 20, 255)
        hair_light = (68, 50, 42, 255)
    elif 'Martins' in name:
        skin = (242, 216, 196, 255)
        hair = (46, 34, 26, 255)
        hair_light = (92, 64, 46, 255)
    elif 'Fabio' in name:
        skin = (228, 178, 138, 255)
        hair = (56, 36, 24, 255)
        hair_light = (115, 72, 46, 255)
    else:
        skin = (220, 155, 110, 255)
        hair = (44, 26, 24, 255)
        hair_light = (82, 54, 38, 255)
        
    skin_shade = tuple(int(a * 0.72 + b * 0.28) for a, b in zip(skin, (80, 40, 35, 255)))
    skin_light = tuple(min(255, int(a * 0.8 + 50)) for a in skin[:3]) + (255,)
    
    is_tall = 'Tavares' in name or 'Martins' in name
    is_wide = 'Anna' in name or 'Sabia' in name
    
    # Shadow under feet
    for dx in range(-5, 6):
        w = 5 if abs(dx) <= 3 else 3
        pixel(16 + dx, 2, (10, 14, 24, 110))
        
    leg_h = 9 if is_tall else 8
    leg_y = 5 if is_tall else 4
    # Legs / pants
    fill(10, leg_y, 6, leg_h, ink)
    fill(17, leg_y, 6, leg_h, ink)
    fill(11, leg_y + 2, 4, leg_h - 2, denim)
    fill(18, leg_y + 2, 4, leg_h - 2, denim)
    fill(11, leg_y + 3, 1, 4, (82, 102, 130, 255))
    fill(18, leg_y + 3, 1, 4, (82, 102, 130, 255))
    # Shoes
    fill(9, 3, 7, 2, ink)
    fill(17, 3, 7, 2, ink)
    fill(10, 3, 5, 1, (190, 200, 210, 255))
    fill(18, 3, 5, 1, (190, 200, 210, 255))
    
    # Torso
    torso_x = 8 if is_wide else 9
    torso_w = 17 if is_wide else 15
    torso_y = 12 if is_tall else 10
    
    fill(torso_x, torso_y, torso_w, 10, ink)
    fill(torso_x + 1, torso_y + 1, torso_w - 2, 8, shirt_col)
    
    # Shirt shading
    shirt_dark = tuple(int(c * 0.7) for c in shirt_col[:3]) + (255,)
    shirt_light = tuple(min(255, int(c * 1.25)) for c in shirt_col[:3]) + (255,)
    fill(torso_x + 1, torso_y + 1, 2, 7, shirt_dark)
    fill(12, torso_y + 7, 9, 2, shirt_light)
    fill(12, torso_y + 1, 9, 1, shirt_dark)
    
    # Arms / sleeves
    arm_l = 5 if is_wide else 6
    arm_r = 24 if is_wide else 23
    arm_w = 5 if is_wide else 4
    fill(arm_l, torso_y + 1, arm_w, 7, ink)
    fill(arm_r, torso_y + 1, arm_w, 7, ink)
    fill(arm_l + 1, torso_y + 3, arm_w - 1, 5, shirt_col)
    fill(arm_r + 1, torso_y + 3, arm_w - 1, 5, shirt_dark)
    # Hands
    fill(arm_l + 1, torso_y, 2, 3, skin)
    fill(arm_r + 1, torso_y, 2, 3, skin_shade)
    # Neck
    fill(14, torso_y + 8, 5, 2, skin_shade)
    
    # --- STUDENT UNIQUE ACCESSORIES ---
    if 'Marcos' in name:
        # Boxing wraps
        fill(arm_l, torso_y, arm_w, 3, (245, 245, 245, 255))
        fill(arm_r, torso_y, arm_w, 3, (245, 245, 245, 255))
        fill(10, torso_y + 4, 13, 1, (230, 230, 230, 255))
    elif 'Matias' in name:
        # Blue martial arts belt with hanging ends
        belt = (30, 110, 240, 255)
        belt_l = (90, 170, 255, 255)
        fill(9, torso_y + 1, 15, 2, belt)
        pixel(12, torso_y + 2, belt_l); pixel(18, torso_y + 2, belt_l)
        fill(14, torso_y - 3, 2, 4, belt)
        fill(16, torso_y - 4, 2, 5, belt)
    elif 'Yasmin' in name:
        # Drawing portfolio folder under arm + sling strap
        fill(12, torso_y + 2, 9, 2, ink)
        fill(22, torso_y - 2, 4, 7, (70, 45, 90, 255))
        fill(22, torso_y + 4, 4, 1, (240, 240, 240, 255))
        for sx in range(13, 21, 2):
            pixel(sx, torso_y + 2, (230, 235, 230, 255))
    elif 'Pedro' in name:
        # Sport wristbands
        fill(7, torso_y + 1, 2, 2, (250, 250, 250, 255))
        fill(24, torso_y + 1, 2, 2, (250, 250, 250, 255))
    elif 'Fabio' in name:
        # Neat shirt collar & buttons
        fill(15, torso_y + 2, 2, 6, shirt_light)
        pixel(15, torso_y + 5, ink); pixel(15, torso_y + 3, ink)
    elif 'Anna' in name or 'Sabia' in name:
        # Oversized collar / hoodie
        fill(11, torso_y + 8, 11, 2, shirt_dark)
        fill(13, torso_y + 7, 7, 2, (250, 250, 250, 255)) # Polo neck
    elif 'Tavares' in name:
        # Warm knitted scarf
        fill(11, torso_y + 6, 11, 3, (215, 195, 160, 255))
        fill(12, torso_y + 2, 8, 4, (190, 165, 130, 255))
        pixel(14, torso_y + 4, (210, 85, 65, 255))
    elif 'Messias' in name:
        # Leather guitar strap diagonal
        for st in range(7):
            pixel(11 + st, torso_y + 7 - st, (95, 65, 45, 255))
            pixel(12 + st, torso_y + 7 - st, (55, 35, 25, 255))
    elif 'Martins' in name:
        # DJ Headphones around neck
        fill(11, torso_y + 7, 11, 2, (35, 40, 48, 255))
        fill(9, torso_y + 6, 3, 3, (70, 130, 180, 255))
        fill(21, torso_y + 6, 3, 3, (70, 130, 180, 255))
        
    # --- HEAD & HAIR ---
    head_dy = 2 if is_tall else 0
    dy = head_dy
    
    # 1. Back hair silhouette (NOT a solid block!)
    if 'Yasmin' in name:
        # Elegant sleek hair falling to shoulders on either side
        fill(9, 17 + dy, 3, 8, hair)
        fill(21, 17 + dy, 3, 8, hair)
        fill(8, 19 + dy, 2, 6, hair)
        fill(23, 19 + dy, 2, 6, hair)
        pixel(9, 18 + dy, hair_light); pixel(23, 18 + dy, hair_light)
    elif 'Anna' in name or 'Sabia' in name:
        # Beautiful organic curly volume around head
        fill(8, 18 + dy, 4, 8, hair)
        fill(21, 18 + dy, 4, 8, hair)
        fill(7, 20 + dy, 2, 6, hair)
        fill(24, 20 + dy, 2, 6, hair)
        pixel(7, 21 + dy, hair_light); pixel(25, 21 + dy, hair_light)
        pixel(8, 25 + dy, hair_light); pixel(24, 25 + dy, hair_light)
    elif 'Tavares' in name:
        # Long cascading curls
        fill(8, 16 + dy, 4, 10, hair)
        fill(21, 16 + dy, 4, 10, hair)
        fill(7, 18 + dy, 2, 7, hair)
        fill(24, 18 + dy, 2, 7, hair)
        pixel(8, 17 + dy, hair_light); pixel(24, 17 + dy, hair_light)
    elif 'Pedro' in name:
        # Long curly hair
        fill(8, 18 + dy, 3, 8, hair)
        fill(22, 18 + dy, 3, 8, hair)
        fill(7, 20 + dy, 2, 6, hair)
        fill(24, 20 + dy, 2, 6, hair)
    else:
        # Short hair side / nape
        fill(10, 20 + dy, 13, 3, ink)
        
    # 2. Face Base
    fill(10, 18 + dy, 13, 10, ink)
    fill(11, 19 + dy, 11, 8, skin)
    fill(11, 21 + dy, 2, 5, skin_light)
    fill(20, 20 + dy, 2, 6, skin_shade)
    fill(13, 19 + dy, 7, 1, skin_shade)
    
    # 3. Eyes & Expression
    # Left eye: x=13-14, Right eye: x=18-19 (well spaced!)
    pixel(13, 23 + dy, ink); pixel(14, 23 + dy, (245, 245, 240, 255))
    pixel(18, 23 + dy, (245, 245, 240, 255)); pixel(19, 23 + dy, ink)
    # Pupil depth
    pixel(13, 24 + dy, ink); pixel(19, 24 + dy, ink)
    # Eyebrows
    pixel(13, 25 + dy, hair); pixel(14, 25 + dy, hair)
    pixel(18, 25 + dy, hair); pixel(19, 25 + dy, hair)
    # Nose & mouth
    pixel(16, 21 + dy, skin_shade)
    fill(15, 20 + dy, 3, 1, (170, 75, 70, 255))
    pixel(16, 19 + dy, skin_light)
    
    # 4. Front Hair & Unique Head Styling
    if 'Yasmin' in name:
        # Delicate straight fringe (bangs) at y=25..26, NOT covering eyes at y=23!
        fill(10, 26 + dy, 13, 4, hair)
        fill(11, 25 + dy, 11, 2, hair)
        # Clean straight cut edge
        fill(12, 28 + dy, 9, 1, hair_light)
        # Side strands framing cheeks
        fill(9, 21 + dy, 2, 6, hair)
        fill(22, 21 + dy, 2, 6, hair)
        # Red barrette / hairpin on left side!
        fill(10, 26 + dy, 2, 2, (220, 40, 50, 255))
        pixel(11, 27 + dy, (255, 140, 150, 255))
    elif 'Anna' in name or 'Sabia' in name:
        # Curly rounded crown with layered highlights
        fill(9, 26 + dy, 15, 4, hair)
        fill(10, 29 + dy, 13, 2, hair)
        fill(11, 30 + dy, 11, 1, hair)
        # Side curl clusters
        fill(8, 21 + dy, 3, 6, hair)
        fill(22, 21 + dy, 3, 6, hair)
        pixel(10, 28 + dy, hair_light); pixel(14, 29 + dy, hair_light)
        pixel(18, 29 + dy, hair_light); pixel(22, 28 + dy, hair_light)
        pixel(8, 24 + dy, hair_light); pixel(24, 24 + dy, hair_light)
    elif 'Tavares' in name:
        # High voluminous curls + golden earring
        fill(9, 26 + dy, 15, 5, hair)
        fill(10, 30 + dy, 13, 2, hair)
        fill(8, 21 + dy, 3, 7, hair)
        fill(22, 21 + dy, 3, 7, hair)
        pixel(11, 29 + dy, hair_light); pixel(16, 30 + dy, hair_light)
        pixel(20, 29 + dy, hair_light)
        pixel(8, 23 + dy, hair_light); pixel(24, 23 + dy, hair_light)
        # Golden hoop earring on left
        pixel(8, 21 + dy, (255, 215, 50, 255))
        pixel(8, 20 + dy, (240, 180, 30, 255))
        pixel(9, 20 + dy, (255, 235, 120, 255))
    elif 'Fabio' in name:
        # Center-parted 90s hair (risca central em x=16) + glasses
        fill(10, 26 + dy, 13, 4, hair)
        fill(11, 29 + dy, 11, 1, hair)
        fill(9, 23 + dy, 2, 5, hair)
        fill(22, 23 + dy, 2, 5, hair)
        # Center parting gap
        pixel(16, 26 + dy, skin)
        pixel(16, 27 + dy, skin)
        fill(12, 28 + dy, 3, 1, hair_light)
        fill(18, 28 + dy, 3, 1, hair_light)
        # Glasses with glint
        g_frame = (35, 40, 55, 255)
        g_shine = (200, 235, 255, 255)
        fill(12, 22 + dy, 4, 3, g_frame)
        fill(17, 22 + dy, 4, 3, g_frame)
        pixel(16, 23 + dy, g_frame) # bridge
        pixel(13, 23 + dy, g_shine); pixel(18, 23 + dy, g_shine)
    elif 'Matias' in name:
        # Curly hair + blue martial arts headband
        fill(10, 27 + dy, 13, 4, hair)
        fill(11, 30 + dy, 11, 1, hair)
        pixel(12, 29 + dy, hair_light); pixel(16, 30 + dy, hair_light); pixel(20, 29 + dy, hair_light)
        # Royal blue headband
        hb = (30, 110, 245, 255)
        hb_l = (110, 180, 255, 255)
        fill(10, 25 + dy, 13, 2, hb)
        pixel(13, 26 + dy, hb_l); pixel(18, 26 + dy, hb_l)
        fill(8, 23 + dy, 2, 4, hb)
        pixel(8, 24 + dy, hb_l)
    elif 'Marcos' in name:
        # Textured curls + burst fade on temples
        fill(11, 27 + dy, 11, 4, hair)
        fill(12, 30 + dy, 9, 1, hair)
        pixel(13, 29 + dy, hair_light); pixel(17, 29 + dy, hair_light)
        # Fade gradient on sides
        fade = tuple(int(a * 0.5 + b * 0.5) for a, b in zip(skin, hair))
        fill(9, 23 + dy, 2, 4, fade)
        fill(22, 23 + dy, 2, 4, fade)
    elif 'Pedro' in name:
        # Wild curly mane + glasses
        fill(9, 26 + dy, 15, 4, hair)
        fill(10, 29 + dy, 13, 2, hair)
        fill(8, 20 + dy, 3, 7, hair)
        fill(22, 20 + dy, 3, 7, hair)
        pixel(11, 28 + dy, hair_light); pixel(17, 28 + dy, hair_light)
        # Round glasses
        g_frame = (35, 40, 55, 255)
        g_shine = (200, 235, 255, 255)
        fill(12, 22 + dy, 4, 3, g_frame)
        fill(17, 22 + dy, 4, 3, g_frame)
        pixel(16, 23 + dy, g_frame)
        pixel(13, 23 + dy, g_shine); pixel(18, 23 + dy, g_shine)
    elif 'Messias' in name:
        # Buzzcut & trim beard
        fill(11, 26 + dy, 11, 3, hair)
        fill(12, 28 + dy, 9, 1, hair_light)
        # Beard along jawline and mustache
        beard = (28, 20, 18, 255)
        fill(13, 20 + dy, 7, 1, beard) # mustache
        fill(11, 19 + dy, 2, 4, beard) # sideburns
        fill(20, 19 + dy, 2, 4, beard)
        fill(13, 18 + dy, 7, 2, beard) # chin goatee
    elif 'Martins' in name:
        # Sleek side-swept hair with volume + headphone cups
        fill(10, 26 + dy, 13, 4, hair)
        fill(11, 29 + dy, 11, 2, hair)
        # Swept to the left
        fill(8, 25 + dy, 4, 4, hair)
        fill(9, 24 + dy, 2, 3, hair)
        fill(12, 28 + dy, 8, 2, hair_light)
        
    return im

students = [
    ('Fabio', (50, 120, 210, 255)),
    ('Matias', (230, 75, 60, 255)),
    ('Marcos', (240, 150, 40, 255)),
    ('Pedro', (60, 170, 90, 255)),
    ('Yasmin', (150, 70, 180, 255)),
    ('Anna Sabia', (235, 100, 150, 255)),
    ('Ana Tavares', (190, 140, 80, 255)),
    ('Luis Messias', (60, 140, 180, 255)),
    ('Luis Martins', (110, 120, 135, 255)),
]

sheet = Image.new('RGBA', (32 * 9 + 16, 64), (20, 24, 36, 255))
for i, (name, col) in enumerate(students):
    s_im = draw_student(name, col)
    # 2x scaled for clear view
    s_2x = s_im.resize((32, 32), Image.Resampling.NEAREST)
    sheet.paste(s_2x, (8 + i * 33, 16), s_2x)

sheet.resize((sheet.width * 2, sheet.height * 2), Image.Resampling.NEAREST).save('scratch/all_9_students_preview.png')
print('Generated all_9_students_preview.png')
