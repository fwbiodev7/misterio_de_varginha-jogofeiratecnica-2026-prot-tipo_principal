from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
ALLIES = ROOT / "Assets" / "ArtSource" / "Allies"
OUTPUT = ROOT / "Assets" / "Resources" / "Varginha" / "Allies"


def cells(image):
    width, height = image.size
    for row in range(4):
        for column in range(4):
            left = column * width // 4
            top = row * height // 4
            right = (column + 1) * width // 4
            bottom = (row + 1) * height // 4
            alpha = image.crop((left, top, right, bottom)).getchannel("A")
            bounds = alpha.point(lambda value: 255 if value >= 128 else 0).getbbox()
            if bounds is None:
                raise ValueError(f"Empty cell at row {row}, column {column}")
            yield row, column, left, top, right, bottom, bounds


def tinted_red(red, green, blue):
    value = max(45, min(235, (red + green + blue) // 3))
    return max(60, value), max(12, value // 4), max(14, value // 4)


def convert_ana():
    path = ALLIES / "AnaTavares.png"
    image = Image.open(path).convert("RGBA")
    pixels = image.load()
    for row, _, left, top, right, bottom, (min_x, min_y, max_x, max_y) in cells(image):
        height = max_y - min_y
        center = (min_x + max_x) / 2
        for y in range(top + min_y, top + max_y):
            relative_y = (y - top - min_y) / max(1, height)
            for x in range(left + min_x, left + max_x):
                red, green, blue, alpha = pixels[x, y]
                if alpha == 0:
                    continue
                local_x = x - left - center
                width = max_x - min_x
                skin = red > 170 and red > green * 1.35 and green > 70 and blue > 60 and blue < green * .92
                hair = red < 155 and red > green * 1.18 and green > blue * 1.10

                # Jersey fitted red-and-black: reuses the original silhouette and leaves
                # the face and hands visible.  The white band is deliberately continuous
                # across each walking pose instead of a white trouser artifact.
                if .30 <= relative_y < .57 and not skin and not hair:
                    diagonal = local_x + relative_y * width * .74
                    if -width * .05 <= diagonal <= width * .05:
                        pixels[x, y] = (234, 230, 220, alpha)
                    elif abs(local_x) > width * .28:
                        pixels[x, y] = (24, 25, 31, alpha)
                    else:
                        pixels[x, y] = (*tinted_red(red, green, blue), alpha)
                # Black athletic shorts, then bare legs, white socks and blue court shoes.
                elif .57 <= relative_y < .69 and (abs(local_x) < width * .31 or not skin):
                    pixels[x, y] = (23, 24, 30, alpha)
                elif .69 <= relative_y < .83 and not hair:
                    pixels[x, y] = (220, 151, 102, alpha)
                elif .83 <= relative_y < .93 and not hair:
                    pixels[x, y] = (236, 235, 226, alpha)
                elif relative_y >= .93 and not hair:
                    pixels[x, y] = (48, 104, 178, alpha) if int((x - left) / 5) % 3 else (235, 238, 235, alpha)

                # Remove the loose lengths that made the player read as wearing the old
                # sweater portrait. A compact ponytail is drawn below for every view.
                if hair and relative_y > .32 and abs(local_x) > width * .25:
                    pixels[x, y] = (0, 0, 0, 0)

        draw = ImageDraw.Draw(image)
        ponytail_x = left + int(center + width * (.36 if row != 1 else -.36))
        ponytail_y = top + min_y + int(height * .27)
        ponytail_w = max(5, int(width * .075))
        ponytail_h = max(10, int(height * .11))
        draw.ellipse((ponytail_x - ponytail_w, ponytail_y - ponytail_h // 2,
                      ponytail_x + ponytail_w, ponytail_y + ponytail_h), fill=(78, 43, 31, 255), outline=(37, 26, 24, 255))
        draw.line((ponytail_x - ponytail_w, ponytail_y - ponytail_h // 2,
                   ponytail_x + ponytail_w, ponytail_y - ponytail_h // 2), fill=(131, 76, 48, 255), width=2)

    image.save(path)


def convert_luis_miguel():
    path = ALLIES / "LuisMiguelMessias.png"
    image = Image.open(path).convert("RGBA")
    pixels = image.load()
    for row, _, left, top, right, bottom, (min_x, min_y, max_x, max_y) in cells(image):
        width = max_x - min_x
        height = max_y - min_y
        center = (min_x + max_x) / 2
        for y in range(top + min_y, top + max_y):
            relative_y = (y - top - min_y) / max(1, height)
            for x in range(left + min_x, left + max_x):
                red, green, blue, alpha = pixels[x, y]
                if alpha == 0:
                    continue
                local_x = x - left - center
                skin = red > 170 and red > green * 1.35 and green > 70 and blue > 60 and blue < green * .92
                hair = red < 125 and red > green * 1.08 and green > blue * .95
                if .40 <= relative_y < .66 and not hair:
                    # Atlético Mineiro: black sleeves and alternating vertical stripes.
                    if abs(local_x) > width * .28:
                        pixels[x, y] = (18, 19, 24, alpha)
                    else:
                        stripe = int((local_x + width * .28) / max(1, width * .14)) % 2
                        pixels[x, y] = (25, 26, 31, alpha) if stripe == 0 else (236, 233, 222, alpha)
                elif .66 <= relative_y < .91 and not hair:
                    pixels[x, y] = (23, 41, 68, alpha)
                elif relative_y >= .91:
                    pixels[x, y] = (21, 22, 28, alpha) if int((x - left) / 4) % 3 else (224, 224, 216, alpha)

        if row == 0:
            draw = ImageDraw.Draw(image)
            shield_x = left + int(center + width * .16)
            shield_y = top + min_y + int(height * .49)
            draw.polygon(((shield_x, shield_y - 7), (shield_x + 7, shield_y - 3),
                          (shield_x + 5, shield_y + 7), (shield_x, shield_y + 10),
                          (shield_x - 5, shield_y + 7), (shield_x - 7, shield_y - 3)),
                         fill=(18, 19, 24, 255), outline=(232, 230, 220, 255))
            draw.rectangle((shield_x - 1, shield_y - 2, shield_x + 1, shield_y + 2), fill=(34, 35, 40, 255))
            draw.point((shield_x, shield_y - 9), fill=(230, 190, 55, 255))
    image.save(path)


def export_atlas(source):
    """Mirror the Unity importer so the revised source art reaches Resources immediately."""
    image = Image.open(source).convert("RGBA")
    width, height = image.size
    frames = []
    tallest = widest = 1
    for row in range(4):
        for column in range(4):
            left, top = column * width // 4, row * height // 4
            right, bottom = (column + 1) * width // 4, (row + 1) * height // 4
            frame = image.crop((left, top, right, bottom))
            bounds = frame.getchannel("A").point(lambda value: 255 if value >= 128 else 0).getbbox()
            if bounds is None:
                raise ValueError(f"Empty frame: {source.name}, {row}, {column}")
            frame = frame.crop(bounds)
            frames.append(frame)
            widest, tallest = max(widest, frame.width), max(tallest, frame.height)

    target_height = 44 if source.stem == "AnnaSabia" else 56 if source.stem == "AnaTavares" else 50
    ratio = min(target_height / tallest, 56 / widest)
    atlas = Image.new("RGBA", (256, 256))
    for index, frame in enumerate(frames):
        resized = frame.resize((max(1, round(frame.width * ratio)), max(1, round(frame.height * ratio))), Image.Resampling.NEAREST)
        column, row = index % 4, index // 4
        x = column * 64 + (64 - resized.width) // 2
        y = row * 64 + 64 - 6 - resized.height
        atlas.alpha_composite(resized, (x, y))
    OUTPUT.mkdir(parents=True, exist_ok=True)
    atlas.save(OUTPUT / source.name)


convert_ana()
convert_luis_miguel()
for source_file in ALLIES.glob("*.png"):
    export_atlas(source_file)
