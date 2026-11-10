# Иконка Unbound Soundtrack: каждый размер рисуется отдельно (с 8x сглаживанием), мелкие — упрощенные, чтобы не мылилось.
# Выход: res/app.ico (16..256), web/img/icon-{16,32,48,192,256}.png (значок окна), web/img/modicon.png (картинка мода в Mod Manager).
# Запуск: python -P res/make_icon.py  (нужен Pillow)
from PIL import Image, ImageDraw

ACID, SUN, INK = (182, 255, 26, 255), (255, 230, 0, 255), (10, 10, 10, 255)


def note(d, k, cx, cy, s, double):
    """нота в квадрате s*s с центром (cx, cy); k — масштаб сглаживания"""
    def head(x, y, w, h):
        lay = Image.new("RGBA", (int(w * 2), int(h * 2)), (0, 0, 0, 0))
        ImageDraw.Draw(lay).ellipse([w * 0.5, h * 0.5, w * 1.5, h * 1.5], fill=INK)
        lay = lay.rotate(22, resample=Image.BICUBIC)
        d._image.alpha_composite(lay, (int(x - w), int(y - h)))
    hw, hh = s * 0.30, s * 0.21            # головка
    stem = s * 0.085                        # толщина штиля
    if double:
        x1, x2, yb1, yb2, top1, top2 = cx - s * 0.20, cx + s * 0.20, cy + s * 0.27, cy + s * 0.19, cy - s * 0.22, cy - s * 0.30
        head(x1, yb1, hw, hh); head(x2, yb2, hw, hh)
        r1, r2 = x1 + hw * 0.5 - stem * 0.3, x2 + hw * 0.5 - stem * 0.3
        d.rectangle([r1 - stem, top1, r1, yb1 - hh * 0.1], fill=INK)
        d.rectangle([r2 - stem, top2, r2, yb2 - hh * 0.1], fill=INK)
        bw = s * 0.15
        d.polygon([(r1 - stem, top1), (r2, top2), (r2, top2 + bw), (r1 - stem, top1 + bw)], fill=INK)
    else:
        x, yb, top = cx - s * 0.08, cy + s * 0.24, cy - s * 0.34
        head(x, yb, hw * 1.15, hh * 1.15)
        r = x + hw * 0.62
        d.rectangle([r - stem * 1.25, top, r, yb - hh * 0.2], fill=INK)
        d.polygon([(r, top), (r + s * 0.26, top + s * 0.18), (r + s * 0.20, top + s * 0.30), (r, top + s * 0.17)], fill=INK)


def render(size):
    k = 8; S = size * k
    im = Image.new("RGBA", (S, S), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    small = size <= 24
    pad = 0 if small else S * 0.02
    sh = 0 if small else S * 0.06                                     # желтая тень — только на больших
    rad = S * (0.16 if small else 0.2)
    if sh:
        d.rounded_rectangle([pad + sh, pad + sh, S - pad, S - pad], rad, fill=SUN)
    d.rounded_rectangle([pad, pad, S - pad - sh, S - pad - sh], rad, fill=ACID)
    box = S - 2 * pad - sh
    note(d, k, pad + box / 2, pad + box / 2, box * (0.78 if small else 0.66), double=size >= 32)
    return im.resize((size, size), Image.LANCZOS)


sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256]
imgs = {s: render(s) for s in sizes}
imgs[256].save("res/app.ico", format="ICO", sizes=[(s, s) for s in sizes], append_images=[imgs[s] for s in sizes if s != 256])
for s in (16, 32, 48, 192, 256):
    (imgs[s] if s in imgs else render(s)).save(f"web/img/icon-{s}.png")
bg = Image.new("RGBA", (128, 128), (11, 11, 12, 255)); bg.alpha_composite(render(112), (8, 8))
bg.convert("RGB").save("web/img/modicon.png")
# лист для проверки глазами
sheet = Image.new("RGBA", (sum(sizes) + 10 * len(sizes) + 10, 276), (24, 24, 26, 255)); x = 10
for s in sizes:
    sheet.alpha_composite(imgs[s], (x, 10)); x += s + 10
sheet.save("res/_check.png")
print("ok")
