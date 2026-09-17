from PIL import Image

def clean_and_draw():
    base = Image.open('scratch/cabin_preview.png').convert('RGBA')
    pix = base.load()

    # Leather seat colors
    leather_mid = (145, 95, 62, 255)
    leather_dark = (105, 65, 42, 255)
    leather_light = (175, 120, 80, 255)
    leather_shadow = (78, 48, 30, 255)

    # Night forest window colors
    tree_dark = (18, 45, 28, 255)
    tree_mid = (28, 70, 42, 255)
    tree_light = (45, 95, 58, 255)
    night_sky = (15, 24, 42, 255)

    # Clean the back area: x from 120 to 305, y from 62 to 140
    # From y=62 to y=82: rear window
    for y in range(60, 84):
        for x in range(120, 308):
            # Window frame border check
            if y < 64:
                pix[x, y] = (35, 75, 115, 255) # blue car frame
            elif y == 64:
                pix[x, y] = (15, 20, 30, 255) # rubber seal
            else:
                # Trees / night sky pattern
                if (x % 35 < 18) and y > 68:
                    pix[x, y] = tree_light if (x+y)%3==0 else tree_mid
                elif y > 72:
                    pix[x, y] = tree_dark
                else:
                    pix[x, y] = night_sky
                    if (x * 7 + y * 13) % 47 == 0:
                        pix[x, y] = (220, 235, 255, 255) # star

    # From y=84 to y=142: rear leather seats
    for y in range(84, 142):
        for x in range(120, 308):
            # Seat horizontal ridges
            if y in (84, 106, 126):
                pix[x, y] = leather_shadow
            elif y in (85, 107, 127):
                pix[x, y] = leather_light
            elif x in (160, 200, 240, 280):
                pix[x, y] = leather_dark # vertical seams
            else:
                pix[x, y] = leather_mid

    base.save('scratch/cabin_cleaned.png')
    print('Cleaned back seat area!')

if __name__ == '__main__':
    clean_and_draw()
