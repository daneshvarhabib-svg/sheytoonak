# The start picture for a video of a prop / poison: its own painted sprite on a plain screen.
#   python3 tools/frames/sprite_start.py <sprite.png> <out.png> [height_px] [centre_x] [green|magenta]
import sys
from PIL import Image
src, out = sys.argv[1], sys.argv[2]
h = int(sys.argv[3]) if len(sys.argv) > 3 else 420
cx = int(sys.argv[4]) if len(sys.argv) > 4 else 640
bg = (255, 0, 255) if (len(sys.argv) > 5 and sys.argv[5] == 'magenta') else (0, 255, 0)
im = Image.open(src).convert('RGBA'); im = im.crop(im.getbbox())
w = round(im.width * h / im.height)
if w > 1180: h = round(h * 1180 / w); w = 1180
im = im.resize((w, h), Image.LANCZOS)
a = im.getchannel('A').point(lambda v: 255 if v > 128 else 0)
o = Image.new('RGB', (1280, 720), bg); o.paste(im, (cx - w // 2, (720 - h) // 2 if h < 500 else 720 - h - 40), a); o.save(out)
