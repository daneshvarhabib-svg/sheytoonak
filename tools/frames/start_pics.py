# The three start pictures for a hero's videos, cut from the five-view studio turn sheet:
#   <hero>_walk_start.png  1280x720, side profile at the LEFT edge (he really walks across the frame)
#   <hero>_idle_start.png  900x1000, seen from behind, centred
#   <hero>_kick_start.png  1280x720, seen from behind, left of the middle (room for the kicking leg on the right)
#   python3 tools/frames/start_pics.py <hero> <out_dir> [profile_index back_index] [magenta]
# magenta: a hero who wears green (Hamada's vine leaves) cannot stand on a green screen.
import sys, numpy as np
from PIL import Image
from scipy import ndimage
hero, out = sys.argv[1], sys.argv[2]
pi, bi = (int(sys.argv[3]), int(sys.argv[4])) if len(sys.argv) > 4 else (2, 3)
im = Image.open(f'/home/user/sheytoonak/Assets/ArtArchive/Heroes/studio/{hero}_studio_turn.png').convert('RGB')
g = np.array(im).astype(int)
fig = ~((g[..., 1] > 120) & (g[..., 1] - np.maximum(g[..., 0], g[..., 2]) > 50))
col = ndimage.binary_closing(fig.sum(0) > 3, np.ones(9)); lab, n = ndimage.label(col)
runs = [(np.where(lab == i)[0].min(), np.where(lab == i)[0].max()) for i in range(1, n + 1)]
runs = [r for r in runs if r[1] - r[0] > im.width * 0.04]; print(len(runs), 'figures')
green = tuple(int(v) for v in np.median(g[:20, :20].reshape(-1, 3), 0))
if sys.argv[-1] == 'magenta': green = (255, 0, 255); fig = ndimage.binary_erosion(fig, iterations=2)
def pic(k, size, cx, h, name):
    x0, x1 = runs[k]; ys = np.where(fig[:, x0:x1 + 1].any(1))[0]
    c = im.crop((x0, ys.min(), x1 + 1, ys.max() + 1)); a = fig[ys.min():ys.max() + 1, x0:x1 + 1]
    wh = (round(c.width * h / c.height), h)
    c = c.resize(wh, Image.LANCZOS); m = Image.fromarray((a * 255).astype(np.uint8)).resize(wh, Image.LANCZOS)
    o = Image.new('RGB', size, green); o.paste(c, (cx - wh[0] // 2, size[1] - h - size[1] // 20), m); o.save(f'{out}/{hero}_{name}_start.png')
pic(pi, (1280, 720), 190, 610, 'walk'); pic(bi, (900, 1000), 450, 840, 'idle'); pic(bi, (1280, 720), 460, 610, 'kick')
