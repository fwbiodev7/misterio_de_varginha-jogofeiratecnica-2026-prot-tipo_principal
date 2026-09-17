from PIL import Image

def generate_full_cabin():
    base = Image.open('scratch/cabin_cleaned.png').convert('RGBA')
    pix = base.load()

    # Ink outline color
    ink = (18, 22, 34, 255)
    shadow_soft = (35, 42, 58, 255)

    def fill_rect(x, y, w, h, c):
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                if 0 <= xx < 384 and 0 <= yy < 216:
                    pix[xx, yy] = c

    def ellipse(cx, cy, rx, ry, c):
        for y in range(cy - ry, cy + ry + 1):
            for x in range(cx - rx, cx + rx + 1):
                if 0 <= x < 384 and 0 <= y < 216:
                    if ((x - cx)**2 / max(1, rx**2) + (y - cy)**2 / max(1, ry**2)) <= 1.0:
                        pix[x, y] = c

    # Colors
    shirt_colors = {
        "Tavares": (40, 165, 175, 255),
        "Fabio": (195, 45, 55, 255),
        "Sabia": (215, 95, 135, 255),
        "Marcos": (50, 160, 95, 255),
        "Martins": (105, 125, 145, 255),
        "Yasmin": (145, 65, 175, 255),
        "Pedro": (205, 115, 35, 255),
        "Matias": (45, 120, 220, 255),
        "Messias": (195, 170, 45, 255),
    }

    # Draw Back Row (Tavares, Fabio, Sabia, Marcos, Martins)
    # 1. Ana Tavares (x=136, y=86): morena, alta, cacheada
    skin_tav = (175, 115, 78, 255)
    hair_tav = (32, 22, 18, 255)
    hair_tav_hi = (72, 48, 35, 255)
    # Body
    fill_rect(125, 96, 22, 22, shirt_colors["Tavares"])
    # Hair back
    ellipse(136, 80, 15, 17, hair_tav)
    for y in range(66, 96):
        for x in range(122, 151):
            if (x*2 + y*3) % 4 == 0 and pix[x, y] == hair_tav:
                pix[x, y] = hair_tav_hi
    # Head & face
    ellipse(136, 82, 9, 10, ink)
    ellipse(136, 82, 8, 9, skin_tav)
    pix[133, 81] = ink; pix[134, 81] = (245, 245, 245, 255)
    pix[139, 81] = ink; pix[140, 81] = (245, 245, 245, 255)
    pix[136, 86] = (190, 85, 75, 255); pix[137, 86] = (190, 85, 75, 255)
    # Golden earring
    pix[128, 85] = (250, 210, 50, 255)

    # 2. Fabio (x=174, y=86): cabelo dividido ao meio e de oculos
    skin_fab = (235, 185, 145, 255)
    hair_fab = (58, 38, 24, 255)
    hair_fab_hi = (115, 75, 48, 255)
    fill_rect(163, 96, 22, 22, shirt_colors["Fabio"])
    ellipse(174, 80, 12, 13, hair_fab)
    # Face
    ellipse(174, 82, 8, 9, ink)
    ellipse(174, 82, 7, 8, skin_fab)
    # Center parted hair
    fill_rect(167, 72, 15, 5, hair_fab)
    fill_rect(168, 73, 5, 2, hair_fab_hi)
    fill_rect(176, 73, 5, 2, hair_fab_hi)
    fill_rect(174, 72, 1, 6, ink) # parting line
    fill_rect(166, 76, 3, 7, hair_fab)
    fill_rect(180, 76, 3, 7, hair_fab)
    # Glasses
    g_col = (25, 28, 38, 255)
    g_shine = (210, 240, 255, 255)
    fill_rect(169, 79, 4, 3, g_col)
    fill_rect(175, 79, 4, 3, g_col)
    pix[170, 80] = g_shine; pix[176, 80] = g_shine
    pix[173, 80] = g_col; pix[174, 80] = g_col # bridge
    pix[174, 85] = (195, 90, 80, 255)

    # 3. Anna Sabia (x=214, y=86): cabelo cacheado e roupa larga
    skin_sab = (230, 175, 135, 255)
    hair_sab = (45, 30, 22, 255)
    hair_sab_hi = (95, 62, 45, 255)
    # Oversized wide sweater
    fill_rect(198, 94, 32, 24, shirt_colors["Sabia"])
    fill_rect(203, 95, 22, 3, (240, 135, 175, 255)) # wide collar
    ellipse(214, 80, 14, 15, hair_sab)
    for y in range(67, 95):
        for x in range(200, 229):
            if (x*3 + y*2) % 4 == 0 and pix[x, y] == hair_sab:
                pix[x, y] = hair_sab_hi
    ellipse(214, 82, 8, 9, ink)
    ellipse(214, 82, 7, 8, skin_sab)
    pix[211, 81] = ink; pix[212, 81] = (245, 245, 245, 255)
    pix[217, 81] = ink; pix[218, 81] = (245, 245, 245, 255)
    pix[214, 86] = (195, 90, 85, 255)

    # 4. Marcos (x=254, y=86): moreno de cabelo cacheado com burst fade
    skin_mar = (118, 75, 48, 255)
    hair_mar = (18, 16, 18, 255)
    hair_mar_hi = (45, 38, 42, 255)
    fade = (75, 50, 36, 255)
    fill_rect(243, 96, 22, 22, shirt_colors["Marcos"])
    # Head
    ellipse(254, 82, 8, 9, ink)
    ellipse(254, 82, 7, 8, skin_mar)
    # Burst fade sides (gradient on ears)
    fill_rect(246, 79, 2, 4, fade)
    fill_rect(261, 79, 2, 4, fade)
    # Curly high top
    ellipse(254, 74, 8, 6, hair_mar)
    for x in range(248, 261):
        if x % 2 == 0: pix[x, 72] = hair_mar_hi
    pix[251, 81] = ink; pix[252, 81] = (245, 245, 245, 255)
    pix[257, 81] = ink; pix[258, 81] = (245, 245, 245, 255)
    pix[254, 86] = (150, 68, 55, 255)

    # 5. Luis Martins (x=290, y=86): cabelo liso pro lado e alto e branco
    skin_mtn = (245, 218, 198, 255)
    hair_mtn = (48, 35, 28, 255)
    hair_mtn_hi = (98, 70, 52, 255)
    fill_rect(279, 95, 22, 23, shirt_colors["Martins"])
    # Head & face
    ellipse(290, 81, 8, 9, ink)
    ellipse(290, 81, 7, 8, skin_mtn)
    # Sleek side-parted hair swept to the left
    ellipse(290, 73, 9, 6, hair_mtn)
    fill_rect(284, 71, 7, 3, hair_mtn_hi)
    fill_rect(282, 74, 4, 6, hair_mtn) # side fringe
    pix[287, 80] = ink; pix[288, 80] = (245, 245, 245, 255)
    pix[293, 80] = ink; pix[294, 80] = (245, 245, 245, 255)
    pix[290, 85] = (205, 95, 85, 255)

    # Front Row (Yasmin, Pedro, Matias, Messias)
    # 6. Yasmin (x=148, y=114): cabelo preto com franja e branca
    skin_yas = (248, 226, 214, 255)
    hair_yas = (15, 12, 18, 255)
    hair_yas_hi = (45, 38, 50, 255)
    fill_rect(134, 124, 28, 26, shirt_colors["Yasmin"])
    # Long straight black hair behind
    fill_rect(135, 102, 26, 26, hair_yas)
    for y in range(104, 128):
        pix[137, y] = hair_yas_hi
        pix[159, y] = hair_yas_hi
    # Face
    ellipse(148, 114, 9, 10, ink)
    ellipse(148, 114, 8, 9, skin_yas)
    # Straight blunt bangs
    fill_rect(140, 104, 17, 5, hair_yas)
    fill_rect(142, 103, 13, 2, hair_yas_hi)
    pix[144, 113] = ink; pix[145, 113] = (245, 245, 245, 255)
    pix[151, 113] = ink; pix[152, 113] = (245, 245, 245, 255)
    pix[148, 119] = (215, 88, 98, 255)

    # 7. Pedro (x=190, y=114): cabeludo cacheado e de oculos
    skin_ped = (230, 180, 140, 255)
    hair_ped = (50, 32, 20, 255)
    hair_ped_hi = (105, 65, 40, 255)
    fill_rect(177, 124, 26, 26, shirt_colors["Pedro"])
    # Big curly hair
    ellipse(190, 110, 16, 17, hair_ped)
    for y in range(96, 128):
        for x in range(175, 206):
            if (x*3 + y*2) % 4 == 0 and pix[x, y] == hair_ped:
                pix[x, y] = hair_ped_hi
    ellipse(190, 114, 9, 10, ink)
    ellipse(190, 114, 8, 9, skin_ped)
    # Glasses
    fill_rect(184, 111, 5, 4, g_col)
    fill_rect(191, 111, 5, 4, g_col)
    pix[185, 112] = g_shine; pix[192, 112] = g_shine
    pix[189, 112] = g_col; pix[190, 112] = g_col
    pix[190, 119] = (195, 95, 80, 255)

    # 8. Matias (x=234, y=114): cabelo cacheado e faixa azul
    skin_mat = (205, 148, 105, 255)
    hair_mat = (32, 26, 26, 255)
    hair_mat_hi = (75, 60, 52, 255)
    fill_rect(221, 124, 26, 26, shirt_colors["Matias"])
    # Curly hair
    ellipse(234, 108, 13, 14, hair_mat)
    for y in range(96, 122):
        for x in range(222, 247):
            if (x + y) % 3 == 0 and pix[x, y] == hair_mat:
                pix[x, y] = hair_mat_hi
    ellipse(234, 114, 9, 10, ink)
    ellipse(234, 114, 8, 9, skin_mat)
    # Blue headband (faixa azul)
    b_blue = (25, 95, 235, 255)
    b_light = (90, 160, 255, 255)
    fill_rect(226, 106, 17, 4, b_blue)
    pix[230, 107] = b_light; pix[237, 107] = b_light
    fill_rect(223, 107, 3, 4, b_blue) # knot hanging
    pix[224, 108] = b_light
    pix[230, 113] = ink; pix[231, 113] = (245, 245, 245, 255)
    pix[237, 113] = ink; pix[238, 113] = (245, 245, 245, 255)
    pix[234, 119] = (185, 85, 70, 255)

    # 9. Luis Messias (x=276, y=114): buzzcut e barba
    skin_mes = (210, 148, 108, 255)
    hair_mes = (30, 22, 18, 255)
    hair_mes_hi = (65, 48, 40, 255)
    beard_mes = (25, 18, 15, 255)
    fill_rect(263, 124, 26, 26, shirt_colors["Messias"])
    # Head & buzzcut
    ellipse(276, 114, 9, 10, ink)
    ellipse(276, 114, 8, 9, skin_mes)
    ellipse(276, 106, 9, 5, hair_mes)
    fill_rect(271, 104, 11, 2, hair_mes_hi)
    # Beard
    fill_rect(271, 118, 11, 2, beard_mes) # mustache
    fill_rect(269, 116, 2, 5, beard_mes) # sideburns
    fill_rect(282, 116, 2, 5, beard_mes)
    fill_rect(270, 120, 13, 5, beard_mes) # full beard
    pix[272, 113] = ink; pix[273, 113] = (245, 245, 245, 255)
    pix[279, 113] = ink; pix[280, 113] = (245, 245, 245, 255)

    base.save('scratch/cabin_perfect_preview.png')
    scaled = base.resize((1672, 941), Image.Resampling.NEAREST)
    scaled.save('Assets/Resources/Varginha/TravelPixel/CabinStudents.png')
    print('Full custom cabin generated successfully!')

if __name__ == '__main__':
    generate_full_cabin()
