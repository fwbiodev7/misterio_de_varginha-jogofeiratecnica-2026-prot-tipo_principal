from PIL import Image

def blend_authentic_cabin():
    # Load original unscaled high-res image
    img = Image.open('Assets/Resources/Varginha/TravelPixel/CabinStudents.png').convert('RGBA')
    # Resize to 384x216 for precise pixel art editing
    base = img.resize((384, 216), Image.Resampling.BILINEAR)
    pix = base.load()

    ink = (18, 20, 28, 255)
    g_col = (25, 28, 35, 255)
    g_lens = (215, 238, 255, 255)

    def rect(x, y, w, h, c):
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                if 0 <= xx < 384 and 0 <= yy < 216:
                    pix[xx, yy] = c

    # 1. Ana Tavares (Top Left, x~140, y~90)
    # Give full curly hair, remove purple ribbon
    hair_tav = (34, 22, 18, 255)
    hair_tav_hi = (75, 48, 38, 255)
    for y in range(70, 95):
        for x in range(128, 150):
            # Overwrite purple band
            c = pix[x, y]
            # If color is purple/magenta (r~120..190, g<80, b>130)
            if c[0] > 90 and c[2] > 110 and c[1] < 90:
                pix[x, y] = hair_tav_hi if (x+y)%2==0 else hair_tav
    # Golden earring
    pix[132, 92] = (250, 215, 50, 255)
    pix[133, 92] = (255, 235, 120, 255)

    # 2. Fabio (Top 2nd, x~175, y~90): cabelo dividido ao meio e de oculos
    skin_fab = (235, 185, 145, 255)
    hair_fab = (56, 38, 25, 255)
    hair_fab_hi = (105, 70, 48, 255)
    # Parted hair on top
    for y in range(72, 85):
        for x in range(165, 188):
            if ((x - 176)**2 / 80 + (y - 78)**2 / 36) <= 1.0:
                pix[x, y] = hair_fab_hi if (x+y)%2==0 else hair_fab
    # Parting line at x=176
    for y in range(73, 80):
        pix[176, y] = ink
    # Add stylish glasses over his eyes (y~84..88, x~170..183)
    rect(170, 83, 5, 4, g_col)
    rect(171, 84, 3, 2, g_lens)
    rect(177, 83, 5, 4, g_col)
    rect(178, 84, 3, 2, g_lens)
    pix[175, 84] = g_col; pix[176, 84] = g_col # bridge

    # 3. Anna Sabia (Top 3rd, x~218, y~90): cabelo cacheado e roupa larga (remove pink headband)
    hair_sab = (45, 30, 22, 255)
    hair_sab_hi = (92, 62, 45, 255)
    for y in range(68, 95):
        for x in range(205, 232):
            c = pix[x, y]
            # Replace pink headband (r>180, g<120, b>120)
            if c[0] > 160 and c[2] > 110 and c[1] < 140:
                pix[x, y] = hair_sab_hi if (x+y)%2==0 else hair_sab

    # 4. Marcos (Top 4th, x~254, y~90): moreno de cabelo cacheado com burst fade
    hair_mar = (22, 18, 20, 255)
    hair_mar_hi = (50, 42, 46, 255)
    skin_mar = (118, 75, 48, 255)
    fade = (75, 50, 36, 255)
    # Burst fade around sides of head
    for y in range(80, 92):
        for dx in (0, 1):
            pix[244 + dx, y] = fade
            pix[264 - dx, y] = fade
    # Curls on top
    for y in range(71, 80):
        for x in range(247, 262):
            if ((x - 254)**2 / 50 + (y - 75)**2 / 25) <= 1.0:
                pix[x, y] = hair_mar_hi if (x+y)%2==0 else hair_mar

    # 5. Luis Martins (Top 5th, x~288, y~90): cabelo liso pro lado e alto e branco
    hair_mtn = (48, 35, 28, 255)
    hair_mtn_hi = (98, 72, 55, 255)
    # Sleek side-swept hair flowing left
    for y in range(70, 84):
        for x in range(278, 298):
            if ((x - 288)**2 / 65 + (y - 77)**2 / 40) <= 1.0:
                pix[x, y] = hair_mtn_hi if (x*2+y)%3!=0 else hair_mtn
    for y in range(76, 82):
        for x in range(278, 285):
            pix[x, y] = hair_mtn

    # 6. Yasmin (Bottom 1st, x~152, y~122): cabelo preto com franja e branca
    skin_yas = (248, 226, 214, 255)
    hair_yas = (15, 12, 18, 255)
    hair_yas_hi = (45, 38, 50, 255)
    # Make skin pale white
    for y in range(115, 133):
        for x in range(146, 160):
            c = pix[x, y]
            # If it was skin tone
            if c[0] > 180 and c[1] > 120 and c[2] > 80:
                pix[x, y] = skin_yas
    # Straight neat horizontal bangs across forehead
    rect(145, 110, 16, 5, hair_yas)
    for x in range(147, 159): pix[x, 109] = hair_yas_hi

    # 7. Pedro (Bottom 2nd, x~195, y~122): cabeludo cacheado e de oculos
    hair_ped = (52, 34, 22, 255)
    hair_ped_hi = (105, 68, 42, 255)
    # Add voluminous long curly hair cascading down both sides
    for y in range(106, 136):
        for x in range(178, 188):
            if ((x - 184)**2 + (y - 120)**2) < 95:
                pix[x, y] = hair_ped_hi if (x+y)%3==0 else hair_ped
        for x in range(203, 213):
            if ((x - 207)**2 + (y - 120)**2) < 95:
                pix[x, y] = hair_ped_hi if (x+y)%3==0 else hair_ped
    # Ensure sharp glasses
    rect(188, 116, 6, 5, g_col)
    rect(189, 117, 4, 3, g_lens)
    rect(197, 116, 6, 5, g_col)
    rect(198, 117, 4, 3, g_lens)
    pix[194, 117] = g_col; pix[195, 117] = g_col; pix[196, 117] = g_col

    # 8. Matias (Bottom 3rd, x~238, y~122): cabelo cacheado e faixa azul
    # Remove the two red hair ties and pigtails!
    seat_color = (155, 105, 70, 255)
    hair_mat = (32, 26, 26, 255)
    hair_mat_hi = (75, 60, 52, 255)
    b_blue = (25, 95, 235, 255)
    b_light = (90, 160, 255, 255)
    # Erase red pigtails on sides (x<226 and x>250)
    for y in range(110, 140):
        for x in range(220, 228):
            c = pix[x, y]
            if c[0] > 120 and c[1] < 70: # red ribbon or pigtails
                pix[x, y] = seat_color if y > 124 else (28, 65, 40, 255)
        for x in range(248, 258):
            c = pix[x, y]
            if c[0] > 120 and c[1] < 70:
                pix[x, y] = seat_color if y > 124 else (28, 65, 40, 255)
    # Add curly hair
    for y in range(104, 118):
        for x in range(228, 248):
            pix[x, y] = hair_mat_hi if (x+y)%2==0 else hair_mat
    # Add blue headband (faixa azul) across forehead
    rect(228, 114, 20, 4, b_blue)
    pix[233, 115] = b_light; pix[241, 115] = b_light
    rect(226, 115, 3, 5, b_blue) # knot hanging
    pix[227, 116] = b_light

    # 9. Luis Messias (Bottom 4th, x~282, y~122): buzzcut e barba (remove green backwards cap!)
    hair_mes = (30, 22, 18, 255)
    hair_mes_hi = (65, 48, 40, 255)
    beard_mes = (25, 18, 15, 255)
    # Replace green cap (g > 80 and r < 60) with close-cropped buzzcut
    for y in range(104, 126):
        for x in range(268, 296):
            c = pix[x, y]
            if c[1] > 65 and c[0] < 80 and c[2] < 90: # green cap
                # Close-cropped buzzcut
                pix[x, y] = hair_mes_hi if (x+y)%2==0 else hair_mes
    # Add neat beard and mustache
    rect(276, 128, 12, 3, beard_mes) # mustache
    rect(273, 130, 18, 8, beard_mes) # full beard / goatee / jawline
    pix[277, 131] = (210, 148, 108, 255); pix[283, 131] = (210, 148, 108, 255) # mouth opening

    base.save('scratch/cabin_authentic_preview.png')
    scaled = base.resize((1672, 941), Image.Resampling.NEAREST)
    scaled.save('Assets/Resources/Varginha/TravelPixel/CabinStudents.png')
    print('Authentic cabin blended and saved to Resources!')

if __name__ == '__main__':
    blend_authentic_cabin()
