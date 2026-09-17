from PIL import Image, ImageDraw

def render_students():
    # 32x32 scaled to 64x64 like VarginhaPixelArtSprites
    width, height = 32, 32
    
    student_ids = [
        "Fabio", "Matias", "Marcos", "Pedro", "Yasmin",
        "Anna", "Tavares", "Messias", "Martins"
    ]
    
    shirt_colors = {
        "Fabio": (195, 45, 55, 255),
        "Matias": (45, 120, 220, 255),
        "Marcos": (50, 160, 95, 255),
        "Pedro": (205, 115, 35, 255),
        "Yasmin": (145, 65, 175, 255),
        "Anna": (215, 95, 135, 255),
        "Tavares": (40, 165, 175, 255),
        "Messias": (195, 170, 45, 255),
        "Martins": (105, 125, 145, 255)
    }

    def palette(sid):
        if sid == "Yasmin":
            return (245, 222, 210, 255), (18, 15, 20, 255), (46, 41, 55, 255) # white skin, black hair
        elif sid == "Pedro":
            return (225, 175, 135, 255), (51, 33, 20, 255), (107, 66, 41, 255) # light-med skin, curly long hair
        elif sid == "Matias":
            return (199, 143, 102, 255), (31, 26, 26, 255), (71, 59, 51, 255) # curly hair + blue headband
        elif sid == "Marcos":
            return (122, 77, 51, 255), (20, 18, 20, 255), (48, 41, 46, 255) # dark moreno, burst fade curly
        elif sid == "Anna": # Sabia
            return (222, 168, 128, 255), (41, 28, 20, 255), (97, 61, 41, 255) # curly hair, oversized clothes
        elif sid == "Tavares":
            return (168, 110, 74, 255), (36, 23, 20, 255), (82, 51, 38, 255) # tall, curly hair
        elif sid == "Messias":
            return (204, 140, 102, 255), (31, 23, 20, 255), (66, 48, 41, 255) # buzzcut + beard
        elif sid == "Martins":
            return (242, 214, 194, 255), (46, 33, 26, 255), (92, 64, 46, 255) # white skin, tall, side hair
        else: # Fabio
            return (227, 179, 138, 255), (56, 36, 23, 255), (115, 71, 46, 255) # center parting + glasses

    def draw_student_head(img, sid, shirt):
        pix = img.load()
        ink = (17, 12, 22, 255)
        skin, hair, hair_light = palette(sid)
        
        # Skin shading
        sr, sg, sb, _ = skin
        skin_shade = (int(sr * 0.72), int(sg * 0.65), int(sb * 0.62), 255)
        skin_light = (min(255, int(sr * 1.12)), min(255, int(sg * 1.08)), min(255, int(sb * 1.05)), 255)

        # Base head shape (y flipped or normal: in PIL y=0 is top, y=31 is bottom)
        # We'll work top-down: y from 6 (top of hair) to 25 (chin/collar)
        
        # Hair back for long hair
        if sid in ("Yasmin", "Tavares", "Anna", "Pedro"):
            for y in range(8, 23):
                for x in range(7, 25):
                    pix[x, y] = ink
            for y in range(9, 22):
                for x in range(8, 24):
                    pix[x, y] = hair

        # Face base
        for y in range(10, 21):
            for x in range(10, 22):
                pix[x, y] = ink
        for y in range(11, 20):
            for x in range(11, 21):
                pix[x, y] = skin

        # Cheeks and chin shading
        for x in range(12, 20):
            pix[x, 19] = skin_shade
        pix[11, 15] = skin_light
        pix[20, 15] = skin_shade

        # Eyes (y=14)
        pix[13, 14] = ink
        pix[14, 14] = (245, 245, 245, 255)
        pix[17, 14] = ink
        pix[18, 14] = (245, 245, 245, 255)

        # Mouth / nose (y=16, 17)
        pix[15, 16] = skin_shade
        pix[16, 16] = skin_shade
        pix[15, 17] = (int(sr * 0.65), int(sg * 0.35), int(sb * 0.35), 255)
        pix[16, 17] = (int(sr * 0.65), int(sg * 0.35), int(sb * 0.35), 255)

        # Hair & distinctive features:
        if sid == "Fabio":
            # Cabelo dividido ao meio (parting at x=15-16) e de oculos
            for x in range(10, 22):
                pix[x, 8] = ink
                pix[x, 9] = hair
            for y in range(9, 14):
                pix[10, y] = hair
                pix[11, y] = hair
                pix[20, y] = hair
                pix[21, y] = hair
            # bangs parting
            for x in range(11, 15):
                pix[x, 10] = hair_light
                pix[x, 11] = hair
            for x in range(17, 21):
                pix[x, 10] = hair_light
                pix[x, 11] = hair
            pix[15, 10] = skin
            pix[16, 10] = skin
            # Glasses (armação estilosa de oculos com brilho)
            glasses_frame = (35, 40, 50, 255)
            glass_shine = (210, 235, 255, 255)
            for x in range(12, 15):
                pix[x, 13] = glasses_frame
                pix[x, 15] = glasses_frame
            pix[12, 14] = glasses_frame
            pix[15, 14] = glasses_frame
            for x in range(17, 20):
                pix[x, 13] = glasses_frame
                pix[x, 15] = glasses_frame
            pix[16, 14] = glasses_frame
            pix[19, 14] = glasses_frame
            pix[16, 13] = glasses_frame # bridge
            pix[14, 14] = glass_shine
            pix[18, 14] = glass_shine

        elif sid == "Matias":
            # Cabelo cacheado e faixa azul
            # Curls on top
            curls = [(10, 8), (12, 7), (14, 8), (16, 7), (18, 8), (20, 7), (21, 9),
                     (9, 10), (11, 9), (13, 8), (15, 8), (17, 8), (19, 9), (21, 11)]
            for cx, cy in curls:
                for dx in (0, 1):
                    for dy in (0, 1):
                        if 0 <= cx+dx < 32 and 0 <= cy+dy < 32:
                            pix[cx+dx, cy+dy] = hair_light if (cx+cy)%2==0 else hair
            # Blue headband (faixa azul royal)
            blue = (25, 95, 235, 255)
            blue_light = (85, 155, 255, 255)
            for x in range(10, 22):
                pix[x, 11] = blue
            pix[12, 11] = blue_light
            pix[17, 11] = blue_light
            # Knot on side
            pix[9, 11] = blue
            pix[9, 12] = blue_light
            pix[8, 13] = blue

        elif sid == "Marcos":
            # Moreno de cabelo cacheado com burst fade
            # Top curly volume
            for y in range(7, 11):
                for x in range(11, 21):
                    pix[x, y] = hair_light if (x+y)%2==0 else hair
            # Curls bumps on top
            for x in (11, 13, 15, 17, 19):
                pix[x, 6] = hair
            # Burst fade on temples/sides: gradient transition around ears
            fade_mid = (int(skin[0]*0.6 + hair[0]*0.4), int(skin[1]*0.6 + hair[1]*0.4), int(skin[2]*0.6 + hair[2]*0.4), 255)
            fade_light = (int(skin[0]*0.8 + hair[0]*0.2), int(skin[1]*0.8 + hair[1]*0.2), int(skin[2]*0.8 + hair[2]*0.2), 255)
            pix[9, 11] = fade_light
            pix[10, 11] = fade_mid
            pix[9, 12] = fade_light
            pix[10, 12] = fade_mid
            pix[21, 11] = fade_mid
            pix[22, 11] = fade_light
            pix[21, 12] = fade_mid
            pix[22, 12] = fade_light

        elif sid == "Pedro":
            # Cabeludo cacheado e de oculos
            # Long voluminous curly hair dropping to shoulders
            for y in range(7, 23):
                for x in range(7, 12):
                    pix[x, y] = hair_light if (x+y)%3==0 else hair
                for x in range(20, 25):
                    pix[x, y] = hair_light if (x+y)%3==0 else hair
            for y in range(6, 11):
                for x in range(10, 22):
                    pix[x, y] = hair_light if (x*2+y)%3==0 else hair
            # Curls rim
            for x in (8, 10, 12, 14, 16, 18, 20, 23):
                pix[x, 5] = hair
            # Glasses
            glasses_frame = (30, 30, 35, 255)
            glass_shine = (215, 235, 255, 255)
            for x in range(12, 15):
                pix[x, 13] = glasses_frame
                pix[x, 15] = glasses_frame
            pix[12, 14] = glasses_frame
            pix[15, 14] = glasses_frame
            for x in range(17, 20):
                pix[x, 13] = glasses_frame
                pix[x, 15] = glasses_frame
            pix[16, 14] = glasses_frame
            pix[19, 14] = glasses_frame
            pix[15, 13] = glasses_frame
            pix[16, 13] = glasses_frame
            pix[13, 14] = glass_shine
            pix[18, 14] = glass_shine

        elif sid == "Yasmin":
            # Cabelo preto com franja e branca
            # Black straight hair + horizontal neat bangs
            for y in range(6, 23):
                pix[8, y] = hair
                pix[9, y] = hair
                pix[22, y] = hair
                pix[23, y] = hair
            for y in range(6, 10):
                for x in range(10, 22):
                    pix[x, y] = hair
            # Straight horizontal bangs at y=11
            for x in range(10, 22):
                pix[x, 10] = hair
                pix[x, 11] = hair
            # Hair highlights
            for x in range(11, 21):
                pix[x, 8] = hair_light
            pix[9, 13] = hair_light
            pix[22, 13] = hair_light

        elif sid == "Anna": # Sabia
            # Cabelo cacheado e roupa larga
            # Wide curly hair
            for y in range(7, 22):
                for x in range(8, 11):
                    pix[x, y] = hair_light if (x+y)%2==0 else hair
                for x in range(21, 24):
                    pix[x, y] = hair_light if (x+y)%2==0 else hair
            for y in range(6, 11):
                for x in range(10, 22):
                    pix[x, y] = hair_light if (x+y)%2==0 else hair

        elif sid == "Tavares":
            # Cabelo cacheado e alta
            for y in range(6, 23):
                for x in range(8, 12):
                    pix[x, y] = hair_light if (x+y)%2==0 else hair
                for x in range(20, 24):
                    pix[x, y] = hair_light if (x+y)%2==0 else hair
            for y in range(5, 10):
                for x in range(10, 22):
                    pix[x, y] = hair_light if (x+y)%2==0 else hair
            # Golden earring
            pix[8, 17] = (245, 205, 55, 255)

        elif sid == "Messias":
            # Buzzcut e barba
            # Buzzcut: clean close-cropped buzz
            for y in range(7, 11):
                for x in range(10, 22):
                    pix[x, y] = hair
            for x in range(11, 21):
                pix[x, 7] = hair_light
            # Beard & mustache & goatee
            beard = (28, 20, 18, 255)
            for x in range(13, 19):
                pix[x, 16] = beard # mustache
            for y in range(17, 21):
                pix[11, y] = beard
                pix[20, y] = beard
            for x in range(12, 20):
                pix[x, 19] = beard
                pix[x, 20] = beard # goatee

        elif sid == "Martins":
            # Cabelo liso pro lado e alto e branco
            # Side-swept sleek hair with parting on right flowing left
            for y in range(6, 11):
                for x in range(9, 23):
                    pix[x, y] = hair
            for x in range(10, 20):
                pix[x, 7] = hair_light
            for y in range(8, 14):
                pix[9, y] = hair
                pix[10, y] = hair_light
            pix[22, 9] = hair
            # Side-swept fringe
            for x in range(11, 16):
                pix[x, 11] = hair
                pix[x, 12] = hair_light

        # Collar / shirt top at bottom of head frame
        collar_color = shirt
        for y in range(21, 25):
            for x in range(9, 23):
                pix[x, y] = ink
        for y in range(22, 24):
            for x in range(10, 22):
                pix[x, y] = collar_color

    # Generate an overview image of all 9 heads
    grid = Image.new('RGBA', (32 * 9, 32), (0, 0, 0, 0))
    for i, sid in enumerate(student_ids):
        head = Image.new('RGBA', (32, 32), (0, 0, 0, 0))
        draw_student_head(head, sid, shirt_colors[sid])
        grid.paste(head, (i * 32, 0))
    
    grid_scaled = grid.resize((32 * 9 * 3, 32 * 3), Image.NEAREST)
    grid_scaled.save('scratch/students_heads_preview.png')
    print('Student heads generated successfully')

if __name__ == '__main__':
    render_students()
