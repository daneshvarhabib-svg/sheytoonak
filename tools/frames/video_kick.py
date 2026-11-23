# The kick of a hero from a generated video: frames a..b (leg leaving the ground .. landed), green keyed out, written
# as shot_0..N-1 (N <= 40); kick_0/kick_1 (the Ready stance) are the standing frame, both feet on the ground (user
# 2026-11-22: «باید اول پاش رو زمین باشه»); kick_2..5 are spread over the clip. Pivot between the feet of the standing frame.
#   python3 tools/frames/video_kick.py <hero> <frames_dir> <stand_frame> <first> <last> [mirror|-] [skip_from skip_to]
# skip: frames left out (user 2026-11-23: «باید فقط اون بخشی رو استفاده کنی که کل پا تو کادره» -- the foot had left the frame).
import sys, os, glob, numpy as np
from PIL import Image
from scipy import ndimage
hero, src, st, a, b = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4]), int(sys.argv[5])
mirror = len(sys.argv) > 6 and sys.argv[6] == 'mirror'
skip = range(int(sys.argv[7]), int(sys.argv[8]) + 1) if len(sys.argv) > 8 else ()
D = f'/home/user/sheytoonak/Assets/Resources/Art/Heroes/{hero}_frames/'
def key(i):
    g = np.array(Image.open(f'{src}/f_{i:03d}.png').convert('RGB')).astype(np.float32)
    green = g[..., 1] - np.maximum(g[..., 0], g[..., 2]); al = np.clip(1 - (green - 25) / 55, 0, 1); al[(g[..., 1] > 120) & (green > 60)] = 0
    al[:, :3] = 0; al[:, -3:] = 0
    lab, n = ndimage.label(al > 0.5)
    if n > 1:
        sz = ndimage.sum(al > 0.5, lab, range(1, n + 1)); al[lab != 1 + int(np.argmax(sz))] = 0
    lim = np.maximum(g[..., 0], g[..., 2]) + 10; g[..., 1] = np.where(g[..., 1] > lim, lim, g[..., 1])
    al[:, :3] = 0; al[:, -3:] = 0   # the video's own edge columns are not green
    return np.dstack([g, al * 255]).astype(np.uint8)
seq = [i for i in range(a, b + 1) if i not in skip]
n = len(seq); assert n <= 40
use = {'kick_0': st, 'kick_1': st, 'kick_2': seq[n * 3 // 10], 'kick_3': seq[n * 5 // 10], 'kick_4': seq[n * 7 // 10], 'kick_5': b}
for i in range(n): use[f'shot_{i}'] = seq[i]
K = {k: key(v) for k, v in use.items()}
al = np.max([k[..., 3] for k in K.values()], 0) > 128
xs = np.where(al.any(0))[0]; ys = np.where(al.any(1))[0]
x0, x1, y0, y1 = max(0, xs.min() - 6), min(al.shape[1], xs.max() + 7), max(0, ys.min() - 6), min(al.shape[0], ys.max() + 7)
W, H = x1 - x0, y1 - y0
s = K['kick_0'][..., 3] > 128; yy = np.where(s.any(1))[0]; fx = np.where(s[yy.max() - 12:yy.max() + 1].any(0))[0]
px = ((fx.min() + fx.max()) / 2 - x0) / W
row = f'{(1 - px) if mirror else px:.4f} {(y1 - 1 - yy.max()) / H:.4f} {(yy.max() - yy.min()) / H:.4f}'
for f in glob.glob(D + 'kick_*.png*') + glob.glob(D + 'shot_*.png*'): os.remove(f)
L = [l for l in open(D + 'frames.txt').read().split('\n') if l and not l.startswith(('kick_', 'shot_'))]
for name, k in K.items():
    c = k[y0:y1, x0:x1].copy(); c[c[..., 3] < 8] = 0
    im = Image.fromarray(c).resize((W * 3 // 5, H * 3 // 5), Image.LANCZOS)
    if mirror: im = im.transpose(Image.FLIP_LEFT_RIGHT)
    im.save(D + name + '.png'); L.append(f'{name} {row}')
open(D + 'frames.txt', 'w').write('\n'.join(L) + '\n'); print(W, H, n, row)
