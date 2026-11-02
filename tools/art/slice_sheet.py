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
# split into n figures: columns with content, then cut at the n-1 widest empty gaps
cols = solid.sum(axis=0) > 2
runs, x = [], 0
W = len(cols)
while x < W:
    if not cols[x]:
        s = x
        while x < W and not cols[x]: x += 1
        runs.append((s, x))
    else: x += 1
inner = [g for g in runs if g[0] > 0 and g[1] < W]
inner.sort(key=lambda g: g[1] - g[0], reverse=True)
cuts = sorted((a + b) // 2 for a, b in inner[:n - 1])
if len(cuts) != n - 1: sys.exit("found %d gaps, need %d" % (len(cuts), n - 1))
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
for i, (f0, f1, hc) in enumerate(frames):
    canvas = Image.new("RGBA", (2 * half, H), (0, 0, 0, 0))
    crop = Image.fromarray(rgba[top:bottom, bounds[i]:bounds[i + 1]], "RGBA")
    canvas.paste(crop, (int(round(half - (hc - bounds[i]))), 5))
    out = "%s%d.png" % (prefix, i + 1)
    canvas.save(out)
    print(out, canvas.size)
