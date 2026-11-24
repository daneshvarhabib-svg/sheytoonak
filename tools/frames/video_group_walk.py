# A walk of a group (two guards and a wolf, a camel train...) from a generated video in which it crosses the frame:
# all figures are keyed out together (every blob bigger than min_blob), held on the centre of their upper bodies,
# one whole cycle is found (two steps; the leg area of the first and last frame alike) and written as
# Art/Poison/<name>_walk1..N on one canvas, with <name>.png = the first of them (AddWalk needs the same canvas).
#   python3 -I tools/frames/video_group_walk.py <name> <frames_dir> <first> <last> [x_max] [min_blob] [pmin pmax]
# x_max: frames whose figures reach past it are not used (a watermark sits there).
import sys, os, glob, numpy as np
from PIL import Image
from scipy import ndimage
name, src, first, last = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4])
xmax = int(sys.argv[5]) if len(sys.argv) > 5 else 99999
minb = int(sys.argv[6]) if len(sys.argv) > 6 else 2000
pmin, pmax = (int(sys.argv[7]), int(sys.argv[8])) if len(sys.argv) > 8 else (16, 40)
D = '/home/user/sheytoonak/Assets/Resources/Art/Poison/'
def key(i):
    g = np.array(Image.open(f'{src}/f_{i:03d}.png').convert('RGB')).astype(np.float32)
    green = g[..., 1] - np.maximum(g[..., 0], g[..., 2]); al = np.clip(1 - (green - 25) / 55, 0, 1); al[(g[..., 1] > 120) & (green > 60)] = 0
    m = ndimage.binary_opening(al > 0.5, iterations=1); lab, n = ndimage.label(m)
    sz = ndimage.sum(m, lab, range(1, n + 1)); keep = np.isin(lab, [k + 1 for k, s in enumerate(sz) if s > minb])
    al[~ndimage.binary_dilation(keep, iterations=2)] = 0
    lim = np.maximum(g[..., 0], g[..., 2]) + 10; g[..., 1] = np.where(g[..., 1] > lim, lim, g[..., 1])
    return np.dstack([g, al * 255]).astype(np.uint8)
K, idx = [], []
for i in range(first, last + 1):
    k = key(i); a = k[..., 3] > 128; xs = np.where(a.any(0))[0]
    if len(xs) == 0 or xs.max() >= xmax: continue
    K.append(k); idx.append(i)
print('usable frames', len(K), idx[:1], idx[-1:])
B = []
for k in K:
    a = k[..., 3] > 128; ys = np.where(a.any(1))[0]; top, bot = ys.min(), ys.max()
    up = a[top: top + (bot - top) * 55 // 100]; cx = np.where(up.any(0))[0].mean()
    B.append((top, bot, cx))
bot = int(np.median([b[1] for b in B])); Hh = int(np.max([b[1] - b[0] for b in B])) + 20
W2 = int(max(max(np.where(k[..., 3] > 128)[1].max() - b[2], b[2] - np.where(k[..., 3] > 128)[1].min()) for k, b in zip(K, B))) + 12
def centred(j):
    k, (t, b, cx) = K[j], B[j]; cx = int(round(cx)); c = np.zeros((Hh, 2 * W2, 4), np.uint8)
    y0 = bot + 6 - Hh; part = k[max(0, y0):bot + 6, max(0, cx - W2):cx + W2]
    c[Hh - part.shape[0]:, max(0, cx - W2) - (cx - W2):max(0, cx - W2) - (cx - W2) + part.shape[1]] = part
    return c
C = [centred(j) for j in range(len(K))]
legs = [np.array(Image.fromarray(c[c.shape[0] * 55 // 100:, :, :3] * (c[c.shape[0] * 55 // 100:, :, 3:] > 128)).convert('L').resize((120, 60))).astype(np.float32) for c in C]
best = min(((np.abs(legs[a] - legs[a + p]).mean(), a, p) for p in range(pmin, pmax + 1) for a in range(len(C) - p)))
print('cycle: frames', idx[best[1]], '+', best[2], 'seam', round(float(best[0]), 2))
cyc = C[best[1]:best[1] + best[2]]
if len(cyc) > 39: cyc = cyc[::2]
al = np.max([c[..., 3] for c in cyc], 0) > 128; xs = np.where(al.any(0))[0]; ys = np.where(al.any(1))[0]
a0, a1, b0, b1 = max(0, xs.min() - 4), xs.max() + 5, max(0, ys.min() - 4), ys.max() + 3
for f in glob.glob(D + name + '_walk*.png*'): os.remove(f)
for i, c in enumerate(cyc):
    c = c[b0:b1, a0:a1].copy(); c[c[..., 3] < 8] = 0; Image.fromarray(c).save(D + f'{name}_walk{i + 1}.png')
Image.fromarray(cyc[0][b0:b1, a0:a1]).save(D + name + '.png')
print(len(cyc), 'frames', a1 - a0, 'x', b1 - b0)
