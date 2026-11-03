"""Slice a green-background sheet laid out in rows into tight-cropped transparent PNGs.
Usage: slice_grid.py sheet.png ROWS COLS out_dir name1 name2 ...   (names in reading order, row by row, left to right)
Rows and columns are cut at the widest empty gaps, so pieces of one item (shards, wind) stay together."""
import sys, os, numpy as np
from PIL import Image
src, R, C, out = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), sys.argv[4]
names = sys.argv[5:]
im = np.asarray(Image.open(src).convert("RGB")).astype(np.float32)
r, g, b = im[..., 0], im[..., 1], im[..., 2]
gx = g - np.maximum(r, b)
alpha = np.clip(1.0 - (gx - 40.0) / 100.0, 0, 1)
g2 = np.where((gx > 0) & (alpha > 0), np.minimum(g, np.maximum(r, b) + 10), g)
rgba = np.dstack([r, g2, b, alpha * 255]).astype(np.uint8)
solid = alpha > 0.5

def cuts(profile, n):
    on = profile > 2
    idx = np.where(on)[0]
    lo, hi = idx.min(), idx.max() + 1
    gaps, x = [], lo
    while x < hi:
        if not on[x]:
            s = x
            while x < hi and not on[x]: x += 1
            gaps.append((s, x))
        else: x += 1
    gaps.sort(key=lambda g: g[1] - g[0], reverse=True)
    if len(gaps) < n - 1: sys.exit("only %d gaps for %d parts" % (len(gaps), n))
    c = sorted((a + b) // 2 for a, b in gaps[:n - 1])
    return [lo] + c + [hi]

os.makedirs(out, exist_ok=True)
rb = cuts(solid.sum(axis=1), R)
k = 0
for i in range(R):
    band = solid[rb[i]:rb[i + 1]]
    cb = cuts(band.sum(axis=0), C)
    for j in range(C):
        cell = solid[rb[i]:rb[i + 1], cb[j]:cb[j + 1]]
        ys, xs = np.where(cell)
        y0, y1, x0, x1 = ys.min() + rb[i], ys.max() + 1 + rb[i], xs.min() + cb[j], xs.max() + 1 + cb[j]
        piece = rgba[max(0, y0 - 4):y1 + 4, max(0, x0 - 4):x1 + 4].copy()
        # keep only this cell's pixels (a neighbour's edge inside the padding is cleared)
        mask = np.zeros(piece.shape[:2], bool)
        oy, ox = max(0, y0 - 4), max(0, x0 - 4)
        mask[max(0, rb[i] - oy):rb[i + 1] - oy, max(0, cb[j] - ox):cb[j + 1] - ox] = True
        piece[..., 3] = np.where(mask, piece[..., 3], 0)
        name = names[k] if k < len(names) else "part%d" % k
        Image.fromarray(piece, "RGBA").save(os.path.join(out, name + ".png"))
        print(name, piece.shape[1], "x", piece.shape[0])
        k += 1
