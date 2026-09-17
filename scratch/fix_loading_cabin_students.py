from pathlib import Path
import sys

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "Assets" / "Resources" / "Varginha" / "TravelPixel" / "CabinStudents.png"


def shade(value, multiplier, floor=0, ceiling=255):
    return max(floor, min(ceiling, int(value * multiplier)))


def correct_luis(image):
    pixels = image.load()
    # Edelzio is driving. Luis Messias is the seated student in orange on the right.
    center = 1450
    for y in range(485, 800):
        for x in range(1245, 1625):
            red, green, blue, alpha = pixels[x, y]
            # Orange fabric only: facial skin, beard and the seat stay untouched.
            orange = red > 130 and green > 45 and green < red * .72 and blue < green * .72
            if not orange:
                continue
            # The shirt is below the head and between the two chair edges.
            torso = 1305 <= x <= 1545 and 615 <= y <= 790
            left_sleeve = 1255 <= x < 1365 and 600 <= y <= 780
            right_sleeve = 1500 < x <= 1600 and 600 <= y <= 780
            if not (torso or left_sleeve or right_sleeve):
                continue
            value = (red + green + blue) // 3
            if abs(x - center) > 140:
                pixels[x, y] = (shade(value, .16, 11, 38), shade(value, .17, 12, 40), shade(value, .22, 15, 48), alpha)
            else:
                stripe = int((x - (center - 135)) / 48) % 2
                if stripe == 0:
                    pixels[x, y] = (shade(value, .16, 13, 42), shade(value, .17, 14, 44), shade(value, .22, 16, 52), alpha)
                else:
                    pixels[x, y] = (shade(value, 1.34, 165, 240), shade(value, 1.32, 162, 236), shade(value, 1.25, 150, 226), alpha)

    draw = ImageDraw.Draw(image)
    # Crest with its gold star, placed on the black torso stripe without covering the seat belt.
    shield = [(1455, 665), (1473, 670), (1470, 693), (1464, 701), (1458, 693), (1455, 670)]
    draw.polygon(shield, fill=(18, 20, 28, 255), outline=(224, 225, 218, 255))
    draw.rectangle((1462, 674, 1467, 689), fill=(242, 242, 233, 255))
    draw.point((1464, 660), fill=(234, 193, 61, 255))
    draw.line((1405, 622, 1475, 622), fill=(18, 20, 28, 255), width=5)


def correct_ana(image):
    pixels = image.load()
    center = 1081
    for y in range(480, 786):
        for x in range(900, 1245):
            red, green, blue, alpha = pixels[x, y]
            teal = green > red * 1.65 and blue > red * 1.3 and green > 90
            if not teal:
                continue
            value = (red + green + blue) // 3
            diagonal = (x - 973) - (y - 488) * .42
            if -15 <= diagonal <= 16:
                pixels[x, y] = (shade(value, 1.48, 210, 242), shade(value, 1.46, 208, 238), shade(value, 1.42, 202, 232), alpha)
            elif abs(x - center) > 112:
                pixels[x, y] = (shade(value, .18, 14, 42), shade(value, .18, 14, 42), shade(value, .24, 18, 55), alpha)
            else:
                pixels[x, y] = (shade(value, 1.17, 115, 206), shade(value, .30, 25, 68), shade(value, .31, 27, 72), alpha)

    draw = ImageDraw.Draw(image)
    # Compact black collar, matching the red-and-black table-tennis jersey.
    draw.line((1039, 510, 1116, 510), fill=(18, 20, 28, 255), width=5)


def main():
    destination = Path(sys.argv[1]) if len(sys.argv) > 1 else SOURCE
    image = Image.open(SOURCE).convert("RGBA")
    correct_luis(image)
    correct_ana(image)
    destination.parent.mkdir(parents=True, exist_ok=True)
    image.save(destination)


if __name__ == "__main__":
    main()
