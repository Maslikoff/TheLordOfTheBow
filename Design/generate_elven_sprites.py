"""Generate Elven Forge Codex 9-slice UI sprites for Unity."""
from pathlib import Path
from PIL import Image, ImageDraw

OUT = Path(r"D:\GitHub\TheLordOfTheBow\Assets\Game\Art\UI\ElvenForge")
OUT.mkdir(parents=True, exist_ok=True)

def hex_rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i+2], 16) for i in (0, 2, 4))

STONE = hex_rgb("1A1612")
STONE_MID = hex_rgb("2C241C")
STONE_LIGHT = hex_rgb("3D3228")
PARCHMENT = hex_rgb("E8D9B8")
FOREST = hex_rgb("2D5A3D")
GOLD = hex_rgb("D4A84B")
MITHRIL = hex_rgb("C5D4E0")
DANGER = hex_rgb("A63D3D")
REWARD = hex_rgb("C9A227")
TEAL = hex_rgb("5C9FAE")

def rounded_rect(draw, box, radius, fill=None, outline=None, width=1):
    draw.rounded_rectangle(box, radius=radius, fill=fill, outline=outline, width=width)

def make_panel(path, size=256, fill=STONE_MID, border=GOLD, border_w=6, radius=36, inner=True):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    pad = 4
    rounded_rect(d, [pad, pad, size-1-pad, size-1-pad], radius, fill=fill + (255,), outline=border + (255,), width=border_w)
    if inner:
        inset = pad + border_w + 6
        rounded_rect(d, [inset, inset, size-1-inset, size-1-inset], max(8, radius-14),
                     outline=MITHRIL + (90,), width=2)
        # corner gems
        gem = 10
        for cx, cy in [(28, 28), (size-28, 28), (28, size-28), (size-28, size-28)]:
            d.ellipse([cx-gem, cy-gem, cx+gem, cy+gem], fill=GOLD + (220,))
            d.ellipse([cx-4, cy-4, cx+4, cy+4], fill=MITHRIL + (200,))
    img.save(path)
    return path

def make_button(path, fill, border, size=(256, 96), radius=22, border_w=5):
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    rounded_rect(d, [2, 2, w-3, h-3], radius, fill=fill + (255,), outline=border + (255,), width=border_w)
    # top highlight
    d.rounded_rectangle([8, 6, w-9, h//2], radius=radius//2, fill=(255, 255, 255, 28))
    img.save(path)
    return path

def make_chip(path, size=(256, 64)):
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    rounded_rect(d, [2, 2, w-3, h-3], h//2, fill=STONE + (235,), outline=GOLD + (200,), width=3)
    # coin
    d.ellipse([12, 14, 44, 46], fill=GOLD + (255,))
    d.ellipse([18, 20, 38, 40], fill=(240, 215, 138, 255))
    img.save(path)
    return path

def make_slider_track(path, size=(256, 32)):
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    rounded_rect(d, [1, 1, w-2, h-2], h//2, fill=STONE + (220,), outline=MITHRIL + (120,), width=2)
    img.save(path)

def make_slider_fill(path, size=(256, 32), fill=TEAL):
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    rounded_rect(d, [1, 1, w-2, h-2], h//2, fill=fill + (255,))
    img.save(path)

def make_icon_btn(path, size=96):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    rounded_rect(d, [2, 2, size-3, size-3], 18, fill=STONE + (240,), outline=MITHRIL + (200,), width=3)
    img.save(path)

files = []
files.append(make_panel(OUT / "panel_modal.png", fill=STONE_MID, border=GOLD))
files.append(make_panel(OUT / "panel_parchment.png", fill=PARCHMENT, border=GOLD, inner=True))
files.append(make_panel(OUT / "panel_hud.png", size=128, fill=STONE, border=MITHRIL, border_w=4, radius=20, inner=False))
files.append(make_button(OUT / "btn_primary.png", FOREST, GOLD))
files.append(make_button(OUT / "btn_secondary.png", STONE_LIGHT, MITHRIL))
files.append(make_button(OUT / "btn_danger.png", DANGER, GOLD))
files.append(make_button(OUT / "btn_reward.png", REWARD, GOLD))
files.append(make_chip(OUT / "chip_coin.png"))
files.append(make_icon_btn(OUT / "btn_icon.png"))
make_slider_track(OUT / "slider_track.png")
make_slider_fill(OUT / "slider_fill.png", fill=TEAL)
make_slider_fill(OUT / "slider_fill_gold.png", fill=GOLD)

# ornament bar (non-slice decorative)
orn = Image.new("RGBA", (512, 24), (0, 0, 0, 0))
od = ImageDraw.Draw(orn)
od.rectangle([0, 10, 230, 14], fill=GOLD + (180,))
od.ellipse([246, 4, 266, 24], fill=GOLD + (255,))
od.ellipse([252, 10, 260, 18], fill=MITHRIL + (255,))
od.rectangle([282, 10, 512, 14], fill=GOLD + (180,))
orn.save(OUT / "ornament_bar.png")

print("Wrote", len(list(OUT.glob('*.png'))), "pngs to", OUT)
