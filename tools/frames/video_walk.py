# The walk of a hero from a generated video: one whole cycle (two steps, at most 40 frames) of the figure in columns
# x0..x1, green keyed out, written as step_0..N-1. The figure is held on its own middle (hips), so a video in which
# he really walks across the frame works too.
#   python3 tools/frames/video_walk.py <hero> <frames_dir> <x0> <x1> [first_frame]
import sys, os, glob, numpy as np
from PIL import Image
from scipy import ndimage
hero, src, x0, x1 = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4])
first = int(sys.argv[5]) if len(sys.argv) > 5 else 8
D = f'/home/user/sheytoonak/Assets/Resources/Art/Heroes/{hero}_frames/'
def key(path):
    g = np.array(Image.open(path).convert('RGB')).astype(np.float32)[:, x0:x1]
    green = g[..., 1] - np.maximum(g[..., 0], g[..., 2]); al = np.clip(1 - (green - 25) / 55, 0, 1); al[(g[..., 1] > 120) & (green > 60)] = 0
    lab, n = ndimage.label(al > 0.5)
    if n > 1:
        sz = ndimage.sum(al > 0.5, lab, range(1, n + 1)); al[lab != 1 + int(np.argmax(sz))] = 0
    lim = np.maximum(g[..., 0], g[..., 2]) + 10; g[..., 1] = np.where(g[..., 1] > lim, lim, g[..., 1])
    return np.dstack([g, al * 255]).astype(np.uint8)
files = sorted(glob.glob(src + '/f_*.png'))
K = [key(f) for f in files[first:]]
def box(k):
    a = k[..., 3] > 128; ys = np.where(a.any(1))[0]; top, bot = ys.min(), ys.max()
    hip = np.where(a[top + (bot - top) * 35 // 100: top + (bot - top) * 55 // 100].any(0))[0]
    return top, bot, (hip.min() + hip.max()) / 2
B = [box(k) for k in K]
bot = int(np.median([b[1] for b in B])); Hh = int(np.median([b[1] - b[0] for b in B])); half = int(Hh * 0.55)
def centred(i):   # the figure on a common canvas, hips in the middle, ground at the bottom
    k, (t, b, cx) = K[i], B[i]; cx = int(round(cx)); c = np.zeros((Hh + 40, 2 * half, 4), np.uint8)
    sx0, sx1 = max(0, cx - half), min(k.shape[1], cx + half); sy0 = max(0, bot - Hh - 30)
    part = k[sy0:bot + 10, sx0:sx1]; c[c.shape[0] - part.shape[0]:, sx0 - (cx - half):sx0 - (cx - half) + part.shape[1]] = part
    return c
C = [centred(i) for i in range(len(K))]
legs = [np.array(Image.fromarray(c[c.shape[0] // 2:, :, :3] * (c[c.shape[0] // 2:, :, 3:] > 128)).convert('L').resize((80, 80))).astype(np.float32) for c in C]
best = min(((np.abs(legs[a] - legs[a + p]).mean(), a, p) for p in range(24, 41) for a in range(len(C) - p)))
print('cycle: frames', first + best[1] + 1, '+', best[2], 'seam', round(float(best[0]), 2))
cyc = C[best[1]:best[1] + best[2]]
al = np.max([c[..., 3] for c in cyc], 0) > 128; xs = np.where(al.any(0))[0]; ys = np.where(al.any(1))[0]
a0, a1, b0, b1 = max(0, xs.min() - 6), min(al.shape[1], xs.max() + 7), max(0, ys.min() - 6), min(al.shape[0], ys.max() + 7)
W, H = a1 - a0, b1 - b0
for f in glob.glob(D + 'step_*.png*'): os.remove(f)
L = [l for l in open(D + 'frames.txt').read().split('\n') if l and not l.startswith('step_')]
stand = Hh / H; out = []
for i, c in enumerate(cyc):
    c = c[b0:b1, a0:a1].copy(); c[c[..., 3] < 8] = 0; Image.fromarray(c).save(D + f'step_{i}.png')
    yy = np.where((c[..., 3] > 128).any(1))[0]
    out.append(f'step_{i} {(half - a0) / W:.4f} {(H - 1 - (al.shape[0] - 11 - b0)) / H:.4f} {stand:.4f}')
open(D + 'frames.txt', 'w').write('\n'.join(L + out) + '\n'); print(W, H, len(cyc))
