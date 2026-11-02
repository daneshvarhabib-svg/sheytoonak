"""Slice a green-background sprite sheet (one row of figures) into frames that share one canvas.
Usage: slice_sheet.py sheet.png out_prefix N
Each frame: the same canvas size, feet on the bottom edge, the HEAD centred horizontally (the head moves
least in a kick, so the body does not slide when frames swap)."""
import sys, numpy as np
from PIL import Image
sys.path.insert(0, __import__('os').path.dirname(__file__))
src, prefix, n = sys.argv[1], sys.argv[2], int(sys.argv[3])
im = np.asarray(Image.open(src).convert("RGB")).astype(np.float32)
r, g, b = im[...,0], im[...,1], im[...,2]
gx = g - np.maximum(r, b)
alpha = np.clip(1.0 - (gx - 40.0) / 100.0, 0, 1)
g2 = np.where((gx > 0) & (alpha > 0), np.minimum(g, np.maximum(r, b) + 10), g)
rgba = np.dstack([r, g2, b, alpha * 255]).astype(np.uint8)
solid = alpha > 0.5
rows = np.where(solid.any(axis=1))[0]
top, bottom = rows.min(), rows.max() + 1
W = solid.shape[1]
# heads: the top band of the figures; each head is one run of filled columns there
band = solid[top: top + int((bottom - top) * 0.14)].any(axis=0)
runs, x = [], 0
while x < W:
    if band[x]:
        s0 = x
        while x < W and band[x]: x += 1
        if x - s0 > 15: runs.append((s0, x))
    else: x += 1
if len(runs) != n: sys.exit("found %d heads, need %d" % (len(runs), n))
centres = [(a + b) // 2 for a, b in runs]
# cut between neighbouring heads at the emptiest column (limbs that touch get split there)
counts = solid.sum(axis=0)
cuts = []
for a, b in zip(centres, centres[1:]):
    lo, hi = a + (b - a) // 4, b - (b - a) // 4
    cuts.append(lo + int(np.argmin(counts[lo:hi])))
bounds = [0] + cuts + [W]
rows = np.where(solid.any(axis=1))[0]
top, bottom = rows.min(), rows.max() + 1
frames = []
for i in range(n):
    x0, x1 = bounds[i], bounds[i + 1]
    m = solid[:, x0:x1]
    ys, xs = np.where(m)
    fx0, fx1 = xs.min() + x0, xs.max() + 1 + x0
    head_rows = (ys < ys.min() + (bottom - top) * 0.12)
    head_cx = xs[head_rows].mean() + x0
    frames.append((fx0, fx1, head_cx))
half = int(max(max(hc - f0, f1 - hc) for f0, f1, hc in frames)) + 10
H = bottom - top + 10
def largest_component(mask):
    """Keep only the biggest 4-connected blob: a neighbour's hand reaching into this cell is dropped."""
    from collections import deque
    h, w = mask.shape
    lab = np.zeros((h, w), np.int32); best, best_n, cur = 0, 0, 0
    for y0, x0 in zip(*np.nonzero(mask)):
        if lab[y0, x0]: continue
        cur += 1; q = deque([(y0, x0)]); lab[y0, x0] = cur; cnt = 0
        while q:
            y, x = q.popleft(); cnt += 1
            for yy, xx in ((y+1,x),(y-1,x),(y,x+1),(y,x-1)):
                if 0 <= yy < h and 0 <= xx < w and mask[yy, xx] and not lab[yy, xx]:
                    lab[yy, xx] = cur; q.append((yy, xx))
        if cnt > best_n: best, best_n = cur, cnt
    return lab == best

for i, (f0, f1, hc) in enumerate(frames):
    canvas = Image.new("RGBA", (2 * half, H), (0, 0, 0, 0))
    cell = rgba[top:bottom, bounds[i]:bounds[i + 1]].copy()
    keep = largest_component(cell[..., 3] > 20)
    cell[..., 3] = np.where(keep, cell[..., 3], 0)
    crop = Image.fromarray(cell, "RGBA")
    canvas.paste(crop, (int(round(half - (hc - bounds[i]))), 5))
    out = "%s%d.png" % (prefix, i + 1)
    canvas.save(out)
    print(out, canvas.size)
