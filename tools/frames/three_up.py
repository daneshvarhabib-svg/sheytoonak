# The start picture for one video with all three clips of a hero: left = side profile (walk), middle and right = seen
# from behind (breathing, kick). Cut from the five-view studio turn sheet (views 3 and 4).
#   python3 tools/frames/three_up.py <hero> <out.png> [profile_index back_index]
import sys, numpy as np
from PIL import Image
from scipy import ndimage
hero, out = sys.argv[1], sys.argv[2]
pi, bi = (int(sys.argv[3]), int(sys.argv[4])) if len(sys.argv) > 4 else (2, 3)
im = Image.open(f'/home/user/sheytoonak/Assets/ArtArchive/Heroes/studio/{hero}_studio_turn.png').convert('RGB')
g = np.array(im).astype(int)
fig = ~((g[..., 1] > 120) & (g[..., 1] - np.maximum(g[..., 0], g[..., 2]) > 50))
col = ndimage.binary_closing(fig.sum(0) > 3, np.ones(9))
lab, n = ndimage.label(col)
runs = [(np.where(lab == i)[0].min(), np.where(lab == i)[0].max()) for i in range(1, n + 1)]
runs = [r for r in runs if r[1] - r[0] > im.width * 0.04]
print(len(runs), 'figures', runs)
green = tuple(int(v) for v in np.median(g[:20, :20].reshape(-1, 3), 0))
o = Image.new('RGB', (1280, 720), green)
def put(k, cx):
    x0, x1 = runs[k]; ys = np.where(fig[:, x0:x1 + 1].any(1))[0]
    c = im.crop((x0, ys.min(), x1 + 1, ys.max() + 1)); a = fig[ys.min():ys.max() + 1, x0:x1 + 1]
    s = 600 / c.height; size = (round(c.width * s), 600)
    c = c.resize(size, Image.LANCZOS); m = Image.fromarray((a * 255).astype(np.uint8)).resize(size, Image.LANCZOS)
    o.paste(c, (cx - size[0] // 2, 720 - 600 - 30), m)
put(pi, 170); put(bi, 520); put(bi, 930)
o.save(out)
