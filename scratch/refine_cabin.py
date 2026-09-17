from PIL import Image

def refine_cabin():
    img = Image.open('scratch/cabin_preview.png').convert('RGBA')
    base = img.resize((384, 216), Image.Resampling.BILINEAR)
    pix = base.load()

    ink = (18, 20, 28, 255)
    g_col = (28, 30, 38, 255)
    g_lens = (215, 238, 255, 255)
    seat_mid = (145, 95, 62, 255)
    seat_shadow = (105, 65, 42, 255)

    # 1. Ana Tavares (Top Left, x~140, y~90)
    # Remove purple headband completely
    hair_tav = (32, 22, 18, 255)
    hair_tav_hi = (75, 48, 38, 255)
    for y in range(68, 85):
        for x in range(130, 146):
            c = pix[x, y]
            if c[0] > 90 and c[2] > 90: # purple band
                pix[x, y] = hair_tav_hi if (x+y)%2==0 else hair_tav
    # Golden earring
    pix[132, 92] = (250, 215, 50, 255)

    # 2. Fabio (Top 2nd, x~175, y~90)
    # Cabelo dividido ao meio e de oculos
    hair_fab = (56, 38, 25, 255)
    hair_fab_hi = (105, 70, 48, 255)
    for y in range(70, 83):
        for x in range(166, 188):
            if ((x - 176)**2 / 75 + (y - 77)**2 / 32) <= 1.0:
                pix[x, y] = hair_fab_hi if (x+y)%2==0 else hair_fab
    for y in range(72, 80):
        pix[176, y] = ink # parting
    # Glasses centered over eyes (y=84..88, x=169..184)
    for x in range(169, 175):
        pix[x, 84] = g_col; pix[x, 88] = g_col
    for y in range(84, 89):
        pix[169, y] = g_col; pix[174, y] = g_col
    for x in range(177, 183):
        pix[x, 84] = g_col; pix[x, 88] = g_col
    for y in range(84, 89):
        pix[177, y] = g_col; pix[182, y] = g_col
    pix[175, 85] = g_col; pix[176, 85] = g_col # bridge
    for y in range(85, 88):
        for x in range(170, 174): pix[x, y] = g_lens
        for x in range(178, 182): pix[x, y] = g_lens
    # Pupils behind glasses
    pix[172, 86] = ink; pix[180, 86] = ink

    # 3. Anna Sabia (Top 3rd, x~218, y~90)
    # Remove pink headband completely
    hair_sab = (45, 30, 22, 255)
    hair_sab_hi = (92, 62, 45, 255)
    for y in range(65, 88):
        for x in range(206, 230):
            c = pix[x, y]
            if c[0] > 150 and c[2] > 100: # pink headband
                pix[x, y] = hair_sab_hi if (x+y)%2==0 else hair_sab

    # 4. Marcos (Top 4th, x~254, y~90)
    # Burst fade & curly top
    hair_mar = (20, 16, 18, 255)
    hair_mar_hi = (50, 42, 46, 255)
    fade = (75, 50, 36, 255)
    for y in range(71, 82):
        for x in range(246, 262):
            if ((x - 254)**2 / 50 + (y - 76)**2 / 24) <= 1.0:
                pix[x, y] = hair_mar_hi if (x+y)%2==0 else hair_mar
    for y in range(80, 88):
        pix[244, y] = fade; pix[245, y] = fade
        pix[262, y] = fade; pix[263, y] = fade

    # 5. Luis Martins (Top 5th, x~288, y~90)
    # Cabelo liso pro lado e alto e branco
    hair_mtn = (48, 35, 28, 255)
    hair_mtn_hi = (98, 72, 55, 255)
    for y in range(68, 83):
        for x in range(278, 298):
            if ((x - 288)**2 / 65 + (y - 76)**2 / 38) <= 1.0:
                pix[x, y] = hair_mtn_hi if (x*2+y)%3!=0 else hair_mtn
    for y in range(75, 82):
        for x in range(276, 283):
            pix[x, y] = hair_mtn

    # 6. Yasmin (Bottom 1st, x~152, y~122)
    # Cabelo preto com franja e branca
    skin_yas = (248, 226, 214, 255)
    hair_yas = (15, 12, 18, 255)
    hair_yas_hi = (45, 38, 50, 255)
    for y in range(116, 134):
        for x in range(146, 159):
            c = pix[x, y]
            if c[0] > 180 and c[1] > 120 and c[2] > 80:
                pix[x, y] = skin_yas
    # Straight blunt bangs across forehead
    for y in range(109, 115):
        for x in range(145, 160):
            pix[x, y] = hair_yas_hi if y == 110 else hair_yas

    # 7. Pedro (Bottom 2nd, x~195, y~122)
    # Cabeludo cacheado e de oculos
    hair_ped = (52, 34, 22, 255)
    hair_ped_hi = (105, 68, 42, 255)
    for y in range(104, 136):
        for x in range(178, 187):
            if ((x - 183)**2 + (y - 120)**2) < 110:
                pix[x, y] = hair_ped_hi if (x+y)%3==0 else hair_ped
        for x in range(203, 213):
            if ((x - 208)**2 + (y - 120)**2) < 110:
                pix[x, y] = hair_ped_hi if (x+y)%3==0 else hair_ped
    # Glasses
    for x in range(188, 194):
        pix[x, 116] = g_col; pix[x, 121] = g_col
    for y in range(116, 122):
        pix[188, y] = g_col; pix[193, y] = g_col
    for x in range(196, 202):
        pix[x, 116] = g_col; pix[x, 121] = g_col
    for y in range(116, 122):
        pix[196, y] = g_col; pix[201, y] = g_col
    pix[194, 118] = g_col; pix[195, 118] = g_col
    for y in range(117, 121):
        for x in range(189, 193): pix[x, y] = g_lens
        for x in range(197, 201): pix[x, y] = g_lens
    pix[191, 119] = ink; pix[199, 119] = ink

    # 8. Matias (Bottom 3rd, x~238, y~122)
    # Cabelo cacheado e faixa azul (erase red pigtails)
    hair_mat = (32, 26, 26, 255)
    hair_mat_hi = (75, 60, 52, 255)
    b_blue = (25, 95, 235, 255)
    b_light = (90, 160, 255, 255)
    # Cover red ties on sides
    for y in range(112, 138):
        for x in range(221, 228):
            pix[x, y] = seat_mid if y > 124 else seat_shadow
        for x in range(248, 256):
            pix[x, y] = seat_mid if y > 124 else seat_shadow
    # Curly hair top
    for y in range(104, 116):
        for x in range(228, 248):
            if ((x - 238)**2 / 80 + (y - 110)**2 / 30) <= 1.0:
                pix[x, y] = hair_mat_hi if (x+y)%2==0 else hair_mat
    # Blue headband
    for x in range(228, 248):
        pix[x, 114] = b_blue
        pix[x, 115] = b_light if x % 4 == 0 else b_blue
    # Knot on left
    for y in range(114, 120):
        pix[226, y] = b_blue
        pix[227, y] = b_light if y % 2 == 0 else b_blue

    # 9. Luis Messias (Bottom 4th, x~282, y~122)
    # Buzzcut e barba (erase green backwards cap completely)
    hair_mes = (30, 22, 18, 255)
    hair_mes_hi = (65, 48, 40, 255)
    beard_mes = (25, 18, 15, 255)
    # Erase the green cap visor and crown
    for y in range(102, 130):
        for x in range(267, 298):
            c = pix[x, y]
            # Green cap color: high green, low red
            if c[1] > 60 and c[0] < 85 and c[2] < 90:
                # If on the outer edges (visor sticking out to the right)
                if x > 288:
                    pix[x, y] = seat_mid if y > 118 else seat_shadow
                else:
                    # Buzzcut
                    pix[x, y] = hair_mes_hi if (x+y)%2==0 else hair_mes
    # Add beard & mustache
    for x in range(275, 287):
        pix[x, 128] = beard_mes
        pix[x, 129] = beard_mes
    for y in range(127, 134):
        pix[271, y] = beard_mes; pix[272, y] = beard_mes
        pix[287, y] = beard_mes; pix[288, y] = beard_mes
    for y in range(130, 136):
        for x in range(272, 288):
            pix[x, y] = beard_mes
    # Mouth gap
    pix[278, 131] = (205, 145, 105, 255); pix[279, 131] = (205, 145, 105, 255)

    base.save('scratch/cabin_refined_preview.png')
    scaled = base.resize((1672, 941), Image.Resampling.NEAREST)
    scaled.save('Assets/Resources/Varginha/TravelPixel/CabinStudents.png')
    print('Refined cabin saved!')

if __name__ == '__main__':
    refine_cabin()
