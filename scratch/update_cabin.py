from PIL import Image, ImageDraw

def update_cabin():
    base = Image.open('scratch/cabin_preview.png').convert('RGBA')
    draw = ImageDraw.Draw(base)
    pix = base.load()

    # Helpers
    def rect(x, y, w, h, col):
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                if 0 <= xx < 384 and 0 <= yy < 216:
                    pix[xx, yy] = col

    # 1. Ana Tavares (x=138, y=88): alta, morena, cabelo cacheado longo volumoso
    skin_tav = (175, 115, 78, 255)
    hair_tav = (32, 22, 18, 255)
    hair_tav_hi = (75, 48, 35, 255)
    # Hair volume around head
    for y in range(65, 100):
        for x in range(125, 155):
            dist = ((x - 140)**2 + (y - 80)**2)
            if dist < 170:
                pix[x, y] = hair_tav if (x+y)%3!=0 else hair_tav_hi
    # Face
    for y in range(74, 94):
        for x in range(133, 147):
            if ((x - 140)**2 / 45 + (y - 84)**2 / 70) < 1.0:
                pix[x, y] = skin_tav
    # Eyes & smile
    pix[136, 83] = (15, 12, 20, 255); pix[137, 83] = (240, 240, 240, 255)
    pix[142, 83] = (15, 12, 20, 255); pix[143, 83] = (240, 240, 240, 255)
    pix[139, 88] = (180, 80, 70, 255); pix[140, 88] = (180, 80, 70, 255)
    # Golden earring
    pix[132, 87] = (250, 210, 50, 255)

    # 2. Fabio (x=175, y=88): cabelo dividido ao meio e de oculos
    skin_fab = (235, 185, 145, 255)
    hair_fab = (58, 38, 24, 255)
    hair_fab_hi = (115, 75, 48, 255)
    # Hair with center parting at x=175
    for y in range(68, 88):
        for x in range(165, 186):
            if ((x - 175)**2 / 95 + (y - 78)**2 / 60) < 1.0:
                pix[x, y] = hair_fab if (x+y)%2==0 else hair_fab_hi
    # Center parting line
    for y in range(71, 76):
        pix[175, y] = (15, 12, 20, 255)
    # Face
    for y in range(76, 94):
        for x in range(168, 183):
            if ((x - 175)**2 / 45 + (y - 85)**2 / 60) < 1.0:
                pix[x, y] = skin_fab
    # Glasses
    g_col = (25, 25, 30, 255)
    g_lens = (210, 235, 255, 255)
    rect(169, 81, 5, 4, g_col)
    rect(170, 82, 3, 2, g_lens)
    rect(176, 81, 5, 4, g_col)
    rect(177, 82, 3, 2, g_lens)
    pix[174, 82] = g_col; pix[175, 82] = g_col # bridge
    pix[175, 89] = (180, 90, 80, 255)

    # 3. Anna Sabia (x=216, y=88): cabelo cacheado e roupa larga
    skin_sab = (230, 175, 135, 255)
    hair_sab = (45, 30, 22, 255)
    hair_sab_hi = (95, 62, 45, 255)
    for y in range(66, 98):
        for x in range(203, 229):
            if ((x - 216)**2 / 120 + (y - 82)**2 / 110) < 1.0:
                pix[x, y] = hair_sab if (x*2+y)%3!=0 else hair_sab_hi
    for y in range(76, 93):
        for x in range(210, 223):
            if ((x - 216)**2 / 40 + (y - 84)**2 / 60) < 1.0:
                pix[x, y] = skin_sab
    pix[213, 83] = (15, 12, 20, 255); pix[214, 83] = (240, 240, 240, 255)
    pix[219, 83] = (15, 12, 20, 255); pix[220, 83] = (240, 240, 240, 255)
    pix[216, 88] = (195, 95, 85, 255)
    # Oversized collar (roupa larga)
    rect(208, 93, 17, 8, (190, 85, 125, 255))
    rect(206, 97, 21, 6, (170, 75, 110, 255))

    # 4. Marcos (x=252, y=88): moreno de cabelo cacheado com burst fade
    skin_mar = (118, 75, 48, 255)
    hair_mar = (18, 16, 18, 255)
    hair_mar_hi = (45, 38, 42, 255)
    fade = (75, 50, 36, 255)
    # Top curly volume
    for y in range(68, 80):
        for x in range(244, 261):
            if ((x - 252)**2 / 65 + (y - 74)**2 / 40) < 1.0:
                pix[x, y] = hair_mar if (x+y)%2==0 else hair_mar_hi
    # Burst fade sides
    for y in range(77, 86):
        pix[243, y] = fade; pix[244, y] = fade
        pix[260, y] = fade; pix[261, y] = fade
    for y in range(78, 94):
        for x in range(245, 260):
            if ((x - 252)**2 / 45 + (y - 86)**2 / 60) < 1.0:
                pix[x, y] = skin_mar
    pix[248, 83] = (15, 12, 20, 255); pix[249, 83] = (240, 240, 240, 255)
    pix[255, 83] = (15, 12, 20, 255); pix[256, 83] = (240, 240, 240, 255)
    pix[252, 88] = (140, 65, 55, 255)

    # 5. Luis Martins (x=286, y=88): cabelo liso pro lado e alto e branco
    skin_mar_l = (245, 218, 198, 255)
    hair_mar_l = (48, 35, 28, 255)
    hair_mar_l_hi = (98, 70, 52, 255)
    # Side-swept sleek hair
    for y in range(67, 84):
        for x in range(277, 296):
            if ((x - 286)**2 / 70 + (y - 76)**2 / 50) < 1.0:
                pix[x, y] = hair_mar_l if (x*2+y)%3!=0 else hair_mar_l_hi
    # Fringe flowing to the left
    for y in range(74, 81):
        for x in range(276, 284):
            pix[x, y] = hair_mar_l_hi if y%2==0 else hair_mar_l
    for y in range(77, 94):
        for x in range(280, 293):
            if ((x - 286)**2 / 40 + (y - 86)**2 / 60) < 1.0:
                pix[x, y] = skin_mar_l
    pix[283, 83] = (15, 12, 20, 255); pix[284, 83] = (240, 240, 240, 255)
    pix[289, 83] = (15, 12, 20, 255); pix[290, 83] = (240, 240, 240, 255)
    pix[286, 88] = (200, 95, 85, 255)

    # ROW 2 (FRONT ROW OF BACK SEAT)
    # 6. Yasmin (x=152, y=120): cabelo preto com franja e branca
    skin_yas = (248, 226, 214, 255)
    hair_yas = (15, 12, 18, 255)
    hair_yas_hi = (45, 38, 50, 255)
    # Long straight black hair
    for y in range(100, 140):
        for x in range(141, 164):
            if ((x - 152)**2 / 90 + (y - 116)**2 / 120) < 1.0:
                pix[x, y] = hair_yas if (x+y)%3!=0 else hair_yas_hi
    for y in range(112, 134):
        for x in range(145, 159):
            if ((x - 152)**2 / 45 + (y - 123)**2 / 80) < 1.0:
                pix[x, y] = skin_yas
    # Straight bangs across forehead
    rect(145, 111, 15, 4, hair_yas)
    for x in range(147, 158): pix[x, 110] = hair_yas_hi
    pix[148, 118] = (15, 12, 20, 255); pix[149, 118] = (240, 240, 240, 255)
    pix[155, 118] = (15, 12, 20, 255); pix[156, 118] = (240, 240, 240, 255)
    pix[152, 125] = (210, 85, 95, 255)

    # 7. Pedro (x=195, y=120): cabeludo cacheado e de oculos
    skin_ped = (230, 180, 140, 255)
    hair_ped = (50, 32, 20, 255)
    hair_ped_hi = (105, 65, 40, 255)
    # Big curly mane
    for y in range(98, 140):
        for x in range(181, 210):
            if ((x - 195)**2 / 140 + (y - 115)**2 / 130) < 1.0:
                pix[x, y] = hair_ped if (x*3+y)%4!=0 else hair_ped_hi
    for y in range(112, 133):
        for x in range(188, 203):
            if ((x - 195)**2 / 48 + (y - 122)**2 / 75) < 1.0:
                pix[x, y] = skin_ped
    # Glasses
    rect(189, 117, 6, 5, g_col)
    rect(190, 118, 4, 3, g_lens)
    rect(197, 117, 6, 5, g_col)
    rect(198, 118, 4, 3, g_lens)
    pix[195, 118] = g_col; pix[196, 118] = g_col
    pix[195, 126] = (190, 90, 75, 255)

    # 8. Matias (x=238, y=120): cabelo cacheado e faixa azul
    skin_mat = (205, 148, 105, 255)
    hair_mat = (32, 26, 26, 255)
    hair_mat_hi = (75, 60, 52, 255)
    # Curly volume
    for y in range(100, 130):
        for x in range(226, 250):
            if ((x - 238)**2 / 95 + (y - 112)**2 / 70) < 1.0:
                pix[x, y] = hair_mat if (x+y)%2==0 else hair_mat_hi
    # Blue headband (faixa azul)
    b_blue = (25, 95, 235, 255)
    b_light = (90, 160, 255, 255)
    rect(228, 110, 20, 3, b_blue)
    pix[233, 111] = b_light; pix[241, 111] = b_light
    pix[226, 111] = b_blue; pix[225, 112] = b_light # knot
    for y in range(113, 134):
        for x in range(231, 245):
            if ((x - 238)**2 / 45 + (y - 123)**2 / 75) < 1.0:
                pix[x, y] = skin_mat
    pix[234, 118] = (15, 12, 20, 255); pix[235, 118] = (240, 240, 240, 255)
    pix[241, 118] = (15, 12, 20, 255); pix[242, 118] = (240, 240, 240, 255)
    pix[238, 125] = (175, 80, 65, 255)

    # 9. Luis Messias (x=278, y=120): buzzcut e barba
    skin_mes = (210, 148, 108, 255)
    hair_mes = (30, 22, 18, 255)
    hair_mes_hi = (65, 48, 40, 255)
    beard_mes = (25, 18, 15, 255)
    # Buzzcut (close-cropped)
    for y in range(104, 118):
        for x in range(268, 288):
            if ((x - 278)**2 / 75 + (y - 110)**2 / 45) < 1.0:
                pix[x, y] = hair_mes if (x+y)%2==0 else hair_mes_hi
    for y in range(112, 133):
        for x in range(271, 285):
            if ((x - 278)**2 / 45 + (y - 122)**2 / 70) < 1.0:
                pix[x, y] = skin_mes
    # Beard & goatee
    rect(273, 124, 11, 2, beard_mes) # mustache
    rect(272, 125, 13, 8, beard_mes) # full beard / goatee
    pix[275, 127] = skin_mes; pix[281, 127] = skin_mes
    pix[275, 118] = (15, 12, 20, 255); pix[276, 118] = (240, 240, 240, 255)
    pix[281, 118] = (15, 12, 20, 255); pix[282, 118] = (240, 240, 240, 255)

    base.save('scratch/cabin_updated_preview.png')
    # Also scale up to match original 1672x941 high-res file
    scaled = base.resize((1672, 941), Image.Resampling.NEAREST)
    scaled.save('Assets/Resources/Varginha/TravelPixel/CabinStudents.png')
    print('Cabin updated and saved!')

if __name__ == '__main__':
    update_cabin()
