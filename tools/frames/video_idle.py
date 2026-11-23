# The breathing clip of a hero from a generated video: the best 40-frame loop of one figure (columns x0..x1 of the
# frame), green keyed out, written as idle_0..39 with the pivot between his feet.
#   python3 tools/frames/video_idle.py <hero> <frames_dir> <x0> <x1> [first_frame] [mirror]
import sys, os, glob, numpy as np
from PIL import Image
from scipy import ndimage
hero, src, x0, x1 = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4])
first = int(sys.argv[5]) if len(sys.argv) > 5 else 40
mirror = len(sys.argv) > 6
D = f'/home/user/sheytoonak/Assets/Resources/Art/Heroes/{hero}_frames/'
def key(path):
    src_im = Image.open(path)
    if src_im.mode == 'RGBA': return np.array(src_im)[:, x0:x1]   # already cut out (seg_green.py): his own greens stay
    g = np.array(src_im.convert('RGB')).astype(np.float32)[:, x0:x1]
    green = g[..., 1] - np.maximum(g[..., 0], g[..., 2]); al = np.clip(1 - (green - 25) / 55, 0, 1); al[(g[..., 1] > 120) & (green > 60)] = 0
    lab, n = ndimage.label(al > 0.5)
    if n > 1:
        sz = ndimage.sum(al > 0.5, lab, range(1, n + 1)); al[lab != 1 + int(np.argmax(sz))] = 0
    lim = np.maximum(g[..., 0], g[..., 2]) + 10; g[..., 1] = np.where(g[..., 1] > lim, lim, g[..., 1])
    return np.dstack([g, al * 255]).astype(np.uint8)
files = sorted(glob.glob(src + '/f_*.png')); N = len(files)
small = {i: np.array(Image.open(files[i]).convert('L').crop((x0, 0, x1, 9999)).resize((120, 240))).astype(np.float32) for i in range(first, N)}
best = min(((np.abs(small[a] - small[a + 40]).mean(), a) for a in range(first, N - 40)))
print('loop from frame', best[1] + 1, 'seam', round(float(best[0]), 2))
K = [key(files[i]) for i in range(best[1], best[1] + 40)]
al = np.max([k[..., 3] for k in K], 0) > 128
xs = np.where(al.any(0))[0]; ys = np.where(al.any(1))[0]
a0, a1, b0, b1 = max(0, xs.min() - 6), xs.max() + 7, max(0, ys.min() - 6), ys.max() + 7
W, H = a1 - a0, b1 - b0
f0 = K[0][b0:b1, a0:a1, 3] > 128; yy = np.where(f0.any(1))[0]; fx = np.where(f0[yy.max() - 12:yy.max() + 1].any(0))[0]; cx = (fx.min() + fx.max()) / 2 / W
for f in glob.glob(D + 'idle_*.png*'): os.remove(f)
L = [l for l in open(D + 'frames.txt').read().split('\n') if l and not l.startswith('idle_')]
out = []
for i, k in enumerate(K):
    c = k[b0:b1, a0:a1].copy(); c[c[..., 3] < 8] = 0; im = Image.fromarray(c)
    if mirror: im = im.transpose(Image.FLIP_LEFT_RIGHT)
    im.save(D + f'idle_{i}.png')
    a = c[..., 3] > 128; y = np.where(a.any(1))[0]
    out.append(f'idle_{i} {(1 - cx) if mirror else cx:.4f} {(H - 1 - y.max()) / H:.4f} {(y.max() - y.min()) / H:.4f}')
open(D + 'frames.txt', 'w').write('\n'.join(L[:1] + out + L[1:]) + '\n')
print(W, H)
