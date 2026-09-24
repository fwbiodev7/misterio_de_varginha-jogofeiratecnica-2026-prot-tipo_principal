import os
from PIL import Image, ImageDraw
import numpy as np

def build_punch_atlas():
    src_path = 'Assets/Resources/Varginha/Allies/Edelzio.png'
    if not os.path.exists(src_path):
        raise FileNotFoundError(f"Missing {src_path}")
        
    src_img = Image.open(src_path).convert('RGBA')
    src_arr = np.array(src_img)
    
    # 18 columns (3 combos x 6 frames each) x 4 rows (4 directions)
    # Cell size is 64x64
    atlas_w = 18 * 64
    atlas_h = 4 * 64
    atlas = Image.new('RGBA', (atlas_w, atlas_h), (0, 0, 0, 0))
    
    # Palette constants matching canonical Edelzio
    C_OUTLINE = (38, 26, 16, 255)
    C_SHIRT_BASE = (212, 152, 48, 255)
    C_SHIRT_HI = (240, 182, 68, 255)
    C_SHIRT_SHADOW = (156, 102, 26, 255)
    C_SKIN_BASE = (218, 156, 108, 255)
    C_SKIN_HI = (244, 192, 142, 255)
    C_SKIN_SHADOW = (172, 108, 68, 255)
    C_FIST_OUTLINE = (42, 28, 18, 255)
    C_SWOOSH = (248, 245, 220, 210)
    C_JEANS_BASE = (45, 48, 56, 255)
    C_JEANS_SHADOW = (28, 30, 36, 255)
    
    # Let's define helper to draw pixel blocks
    def draw_pixel_box(arr, x0, y0, w, h, fill, border=None):
        for y in range(y0, y0 + h):
            for x in range(x0, x0 + w):
                if 0 <= x < 64 and 0 <= y < 64:
                    if border and (x == x0 or x == x0 + w - 1 or y == y0 or y == y0 + h - 1):
                        arr[y, x] = border
                    else:
                        arr[y, x] = fill

    # For each direction:
    # 0 = Down (Front view), PIL row 0 (Unity row 3)
    # 1 = Left, PIL row 1 (Unity row 2)
    # 2 = Right, PIL row 2 (Unity row 1)
    # 3 = Up (Back view), PIL row 3 (Unity row 0)
    
    for direction in range(4):
        base_idle = src_arr[direction * 64:(direction + 1) * 64, 0:64].copy()
        
        # 18 frames: combo 0 (0..5), combo 1 (6..11), combo 2 (12..17)
        for frame_idx in range(18):
            combo = frame_idx // 6       # 0 = jab, 1 = cross, 2 = heavy
            pose = frame_idx % 6         # 0=windup, 1=thrust, 2=reach, 3=impact, 4=hold, 5=recover
            
            frame = np.zeros((64, 64, 4), dtype=np.uint8)
            
            # --- DIRECTION 1: FACING LEFT ---
            if direction == 1:
                # Calculate body lean / translation
                # Windup pulls back (+1), punch drives forward (-2 to -4), recovery returns
                if combo == 0:
                    lean_x = [1, -2, -3, -3, -2, 0][pose]
                    reach = [0, 6, 11, 12, 9, 3][pose]
                    arm_y = 35 + [0, 0, -1, -1, 0, 1][pose]
                elif combo == 1:
                    lean_x = [2, -2, -4, -4, -3, 0][pose]
                    reach = [0, 7, 13, 14, 10, 4][pose]
                    arm_y = 34 + [1, 0, -1, -1, 0, 1][pose]
                else: # combo 2 (Heavy)
                    lean_x = [2, -3, -5, -6, -4, 0][pose]
                    reach = [0, 8, 15, 16, 12, 5][pose]
                    arm_y = 34 + [1, -1, -2, -2, -1, 1][pose]
                
                # Base body shifted by lean_x:
                # Legs stay grounded (y >= 43 stays at x=0 or shifts slightly)
                for y in range(64):
                    for x in range(64):
                        if base_idle[y, x, 3] < 128:
                            continue
                        
                        # Remove the hanging arm at x in [33..41], y in [34..43]
                        if 33 <= x <= 41 and 35 <= y <= 43:
                            continue
                            
                        # Shift upper body with lean
                        dx = lean_x if y < 44 else (lean_x // 2 if y < 52 else 0)
                        nx = x + dx
                        if 0 <= nx < 64:
                            frame[y, nx] = base_idle[y, x]
                            
                # Re-fill the gap on the torso where the old arm was removed:
                torso_x = 33 + lean_x
                for ty in range(35, 41):
                    for tx in range(torso_x, min(63, torso_x + 6)):
                        if frame[ty, tx, 3] == 0 and frame[ty, tx - 1, 3] > 0:
                            frame[ty, tx] = C_SHIRT_SHADOW if tx >= torso_x + 4 else C_SHIRT_BASE
                for py in range(41, 44):
                    for px in range(torso_x, min(63, torso_x + 5)):
                        if frame[py, px, 3] == 0 and frame[py, px - 1, 3] > 0:
                            frame[py, px] = C_JEANS_BASE
                            
                # Now draw the punching arm and guard arm!
                shoulder_x = 28 + lean_x
                shoulder_y = arm_y
                
                # Guard arm (rear arm tucked protecting face/chest)
                guard_x = shoulder_x + 7
                guard_y = shoulder_y + 1
                draw_pixel_box(frame, guard_x, guard_y, 4, 4, C_SHIRT_SHADOW, C_OUTLINE)
                draw_pixel_box(frame, guard_x - 1, guard_y + 2, 3, 3, C_SKIN_BASE, C_FIST_OUTLINE)
                
                # Punching lead arm:
                if pose == 0:
                    # Cocked back in chamber
                    draw_pixel_box(frame, shoulder_x + 1, shoulder_y, 5, 4, C_SHIRT_BASE, C_OUTLINE)
                    draw_pixel_box(frame, shoulder_x + 5, shoulder_y - 1, 4, 4, C_SKIN_BASE, C_FIST_OUTLINE)
                else:
                    # Extending forward
                    fist_x = shoulder_x - reach
                    fist_y = shoulder_y
                    
                    # Sleeve
                    sleeve_len = min(6, reach // 2 + 3)
                    draw_pixel_box(frame, shoulder_x - sleeve_len + 2, shoulder_y, sleeve_len, 4, C_SHIRT_BASE, C_OUTLINE)
                    frame[shoulder_y + 1, shoulder_x - sleeve_len + 3] = C_SHIRT_HI
                    
                    # Forearm
                    forearm_len = max(3, reach - sleeve_len + 4)
                    draw_pixel_box(frame, fist_x + 4, shoulder_y + 1, forearm_len, 3, C_SKIN_BASE, C_OUTLINE)
                    
                    # Clenched Fist
                    fist_w, fist_h = (5, 5) if combo < 2 else (6, 5)
                    draw_pixel_box(frame, fist_x, fist_y, fist_w, fist_h, C_SKIN_BASE, C_FIST_OUTLINE)
                    frame[fist_y + 1, fist_x + 1] = C_SKIN_HI
                    # Knuckle crease
                    frame[fist_y + 2, fist_x] = C_OUTLINE
                    frame[fist_y + 3, fist_x] = C_OUTLINE
                    
                    # Impact whoosh streak on poses 2, 3
                    if pose in (2, 3):
                        streak_x = max(2, fist_x - 3)
                        for sy in range(fist_y - 1, fist_y + fist_h + 2):
                            if 0 <= sy < 64:
                                frame[sy, streak_x] = C_SWOOSH
                        if combo == 2: # Heavy streak
                            for sy in range(fist_y, fist_y + fist_h):
                                if 0 <= sy < 64:
                                    frame[sy, max(1, streak_x - 2)] = C_SWOOSH
                                    
            # --- DIRECTION 2: FACING RIGHT (Exact clean mirror of Direction 1) ---
            elif direction == 2:
                # Get the frame generated for Direction 1 and horizontally mirror it
                left_col = (atlas_w // 64) # not built yet in atlas, let's build from left logic
                # We can mirror the frame logic of direction 1!
                if combo == 0:
                    lean_x = [1, -2, -3, -3, -2, 0][pose]
                    reach = [0, 6, 11, 12, 9, 3][pose]
                    arm_y = 35 + [0, 0, -1, -1, 0, 1][pose]
                elif combo == 1:
                    lean_x = [2, -2, -4, -4, -3, 0][pose]
                    reach = [0, 7, 13, 14, 10, 4][pose]
                    arm_y = 34 + [1, 0, -1, -1, 0, 1][pose]
                else:
                    lean_x = [2, -3, -5, -6, -4, 0][pose]
                    reach = [0, 8, 15, 16, 12, 5][pose]
                    arm_y = 34 + [1, -1, -2, -2, -1, 1][pose]
                
                # Flip x coordinates (63 - x)
                # Build direction 1 temp frame then flip:
                temp_d1 = np.zeros((64, 64, 4), dtype=np.uint8)
                src_idle_d2 = src_arr[2 * 64:3 * 64, 0:64].copy()
                
                # Direct mirroring of the facing-left frame preserves pixel perfection:
                # If we use dir 1 base:
                # Let's mirror base_idle of dir 1:
                mirrored_idle = np.fliplr(src_arr[1 * 64:2 * 64, 0:64])
                for y in range(64):
                    for x in range(64):
                        if mirrored_idle[y, x, 3] < 128:
                            continue
                        if (63 - 41) <= x <= (63 - 33) and 35 <= y <= 43:
                            continue
                        dx = -lean_x if y < 44 else (-lean_x // 2 if y < 52 else 0)
                        nx = x + dx
                        if 0 <= nx < 64:
                            frame[y, nx] = mirrored_idle[y, x]
                            
                torso_x = (63 - 33) - lean_x
                for ty in range(35, 41):
                    for tx in range(max(0, torso_x - 5), torso_x + 1):
                        if frame[ty, tx, 3] == 0 and tx + 1 < 64 and frame[ty, tx + 1, 3] > 0:
                            frame[ty, tx] = C_SHIRT_SHADOW if tx <= torso_x - 3 else C_SHIRT_BASE
                for py in range(41, 44):
                    for px in range(max(0, torso_x - 4), torso_x + 1):
                        if frame[py, px, 3] == 0 and px + 1 < 64 and frame[py, px + 1, 3] > 0:
                            frame[py, px] = C_JEANS_BASE
                            
                shoulder_x = 63 - (28 + lean_x)
                shoulder_y = arm_y
                
                # Guard arm
                guard_x = shoulder_x - 7 - 3
                guard_y = shoulder_y + 1
                draw_pixel_box(frame, guard_x, guard_y, 4, 4, C_SHIRT_SHADOW, C_OUTLINE)
                draw_pixel_box(frame, guard_x + 2, guard_y + 2, 3, 3, C_SKIN_BASE, C_FIST_OUTLINE)
                
                # Punching lead arm
                if pose == 0:
                    draw_pixel_box(frame, shoulder_x - 4, shoulder_y, 5, 4, C_SHIRT_BASE, C_OUTLINE)
                    draw_pixel_box(frame, shoulder_x - 7, shoulder_y - 1, 4, 4, C_SKIN_BASE, C_FIST_OUTLINE)
                else:
                    fist_x = shoulder_x + reach - (5 if combo < 2 else 6)
                    fist_y = shoulder_y
                    sleeve_len = min(6, reach // 2 + 3)
                    draw_pixel_box(frame, shoulder_x - 2, shoulder_y, sleeve_len, 4, C_SHIRT_BASE, C_OUTLINE)
                    frame[shoulder_y + 1, shoulder_x + sleeve_len - 3] = C_SHIRT_HI
                    forearm_len = max(3, reach - sleeve_len + 4)
                    draw_pixel_box(frame, shoulder_x + sleeve_len - 3, shoulder_y + 1, forearm_len, 3, C_SKIN_BASE, C_OUTLINE)
                    fist_w, fist_h = (5, 5) if combo < 2 else (6, 5)
                    draw_pixel_box(frame, fist_x, fist_y, fist_w, fist_h, C_SKIN_BASE, C_FIST_OUTLINE)
                    frame[fist_y + 1, fist_x + fist_w - 2] = C_SKIN_HI
                    frame[fist_y + 2, fist_x + fist_w - 1] = C_OUTLINE
                    frame[fist_y + 3, fist_x + fist_w - 1] = C_OUTLINE
                    if pose in (2, 3):
                        streak_x = min(61, fist_x + fist_w + 2)
                        for sy in range(fist_y - 1, fist_y + fist_h + 2):
                            if 0 <= sy < 64:
                                frame[sy, streak_x] = C_SWOOSH
                        if combo == 2:
                            for sy in range(fist_y, fist_y + fist_h):
                                if 0 <= sy < 64:
                                    frame[sy, min(62, streak_x + 2)] = C_SWOOSH
                                    
            # --- DIRECTION 0: FACING DOWN (FRONT VIEW) ---
            elif direction == 0:
                # In front view, punches strike down towards camera/enemies
                hand_side = -1 if combo == 0 else 1 if combo == 1 else 0 # left punch, right punch, or heavy center
                reach_y = [0, 3, 6, 7, 5, 2][pose]
                lean_y = [0, 1, 2, 2, 1, 0][pose]
                
                # Copy base body
                for y in range(64):
                    for x in range(64):
                        if base_idle[y, x, 3] < 128:
                            continue
                        # Mask out idle hands on sides if they are punching
                        if hand_side == -1 and x < 26 and y >= 36: # mask left hand
                            continue
                        if hand_side == 1 and x > 37 and y >= 36: # mask right hand
                            continue
                        if hand_side == 0 and (x < 26 or x > 37) and y >= 37: # mask both
                            continue
                        ny = y + (lean_y if y < 44 else 0)
                        if 0 <= ny < 64:
                            frame[ny, x] = base_idle[y, x]
                            
                # Punching fist(s):
                if hand_side in (-1, 0): # Left punch
                    px = 22
                    py = 37 + reach_y + lean_y
                    # Draw sleeve and forearm
                    draw_pixel_box(frame, px - 1, 34 + lean_y, 5, 4, C_SHIRT_BASE, C_OUTLINE)
                    draw_pixel_box(frame, px, 37 + lean_y, 4, max(2, reach_y), C_SKIN_BASE, C_OUTLINE)
                    # Big clenched fist reaching down
                    fist_sz = 6 if combo == 2 else 5
                    draw_pixel_box(frame, px - 1, py, fist_sz, fist_sz, C_SKIN_BASE, C_FIST_OUTLINE)
                    frame[py + 1, px] = C_SKIN_HI
                    frame[py + fist_sz - 1, px + 1] = C_OUTLINE
                    if pose in (2, 3):
                        for sx in range(px - 2, px + fist_sz + 2):
                            if 0 <= py + fist_sz + 1 < 64 and 0 <= sx < 64:
                                frame[py + fist_sz + 1, sx] = C_SWOOSH
                                
                if hand_side in (1, 0): # Right punch
                    px = 37
                    py = 37 + reach_y + lean_y
                    draw_pixel_box(frame, px - 1, 34 + lean_y, 5, 4, C_SHIRT_BASE, C_OUTLINE)
                    draw_pixel_box(frame, px, 37 + lean_y, 4, max(2, reach_y), C_SKIN_BASE, C_OUTLINE)
                    fist_sz = 6 if combo == 2 else 5
                    draw_pixel_box(frame, px, py, fist_sz, fist_sz, C_SKIN_BASE, C_FIST_OUTLINE)
                    frame[py + 1, px + 1] = C_SKIN_HI
                    frame[py + fist_sz - 1, px + 2] = C_OUTLINE
                    if pose in (2, 3):
                        for sx in range(px - 1, px + fist_sz + 3):
                            if 0 <= py + fist_sz + 1 < 64 and 0 <= sx < 64:
                                frame[py + fist_sz + 1, sx] = C_SWOOSH
                                
            # --- DIRECTION 3: FACING UP (BACK VIEW) ---
            elif direction == 3:
                hand_side = -1 if combo == 0 else 1 if combo == 1 else 0
                reach_y = [0, 3, 6, 7, 5, 2][pose]
                lean_y = [0, -1, -2, -2, -1, 0][pose]
                
                for y in range(64):
                    for x in range(64):
                        if base_idle[y, x, 3] < 128:
                            continue
                        if hand_side == -1 and x < 26 and y >= 36:
                            continue
                        if hand_side == 1 and x > 37 and y >= 36:
                            continue
                        if hand_side == 0 and (x < 26 or x > 37) and y >= 37:
                            continue
                        ny = y + (lean_y if y < 44 else 0)
                        if 0 <= ny < 64:
                            frame[ny, x] = base_idle[y, x]
                            
                # Fists punching upwards into the scene
                if hand_side in (-1, 0):
                    px = 22
                    py = 33 - reach_y + lean_y
                    draw_pixel_box(frame, px - 1, 35 + lean_y, 5, 4, C_SHIRT_SHADOW, C_OUTLINE)
                    draw_pixel_box(frame, px, py + 4, 4, max(2, reach_y), C_SKIN_BASE, C_OUTLINE)
                    fist_sz = 6 if combo == 2 else 5
                    draw_pixel_box(frame, px - 1, py, fist_sz, fist_sz, C_SKIN_BASE, C_FIST_OUTLINE)
                    frame[py + 1, px] = C_SKIN_HI
                    if pose in (2, 3):
                        for sx in range(px - 2, px + fist_sz + 2):
                            if 0 <= py - 2 < 64 and 0 <= sx < 64:
                                frame[py - 2, sx] = C_SWOOSH
                                
                if hand_side in (1, 0):
                    px = 37
                    py = 33 - reach_y + lean_y
                    draw_pixel_box(frame, px - 1, 35 + lean_y, 5, 4, C_SHIRT_SHADOW, C_OUTLINE)
                    draw_pixel_box(frame, px, py + 4, 4, max(2, reach_y), C_SKIN_BASE, C_OUTLINE)
                    fist_sz = 6 if combo == 2 else 5
                    draw_pixel_box(frame, px, py, fist_sz, fist_sz, C_SKIN_BASE, C_FIST_OUTLINE)
                    frame[py + 1, px + 1] = C_SKIN_HI
                    if pose in (2, 3):
                        for sx in range(px - 1, px + fist_sz + 3):
                            if 0 <= py - 2 < 64 and 0 <= sx < 64:
                                frame[py - 2, sx] = C_SWOOSH

            # Paste into atlas:
            cell_img = Image.fromarray(frame, 'RGBA')
            atlas.paste(cell_img, (frame_idx * 64, direction * 64), cell_img)
            
    # Save the master atlas
    out_path = 'Assets/Resources/Varginha/EdelzioAttackV1.png'
    atlas.save(out_path)
    print(f"Generated {out_path} ({atlas.size[0]}x{atlas.size[1]})")
    
    # Save preview montage of combo 0 (poses 0..5 in all 4 directions)
    preview = Image.new('RGBA', (6 * 64, 4 * 64), (20, 25, 30, 255))
    for d in range(4):
        row_crop = atlas.crop((0, d * 64, 6 * 64, (d + 1) * 64))
        preview.paste(row_crop, (0, d * 64), row_crop)
    preview.save('scratch/punch_preview_combo0.png')
    
    # Save preview montage of combo 2 (heavy punch)
    preview2 = Image.new('RGBA', (6 * 64, 4 * 64), (20, 25, 30, 255))
    for d in range(4):
        row_crop = atlas.crop((12 * 64, d * 64, 18 * 64, (d + 1) * 64))
        preview2.paste(row_crop, (0, d * 64), row_crop)
    preview2.save('scratch/punch_preview_combo2.png')
    print("Saved preview montages in scratch/")

if __name__ == '__main__':
    build_punch_atlas()
