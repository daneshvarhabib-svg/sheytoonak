"""Green-screen key for a single prop picture: python3 key_green.py in out [--flip] [--max W]
Alpha from how much greener a pixel is than its other channels; green spill pulled out of the edges; cropped to content."""
import sys
import numpy as np
from PIL import Image
from scipy import ndimage

def key(im, keep_small=False):
    a = np.asarray(im.convert('RGB')).astype(np.float32)
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    green = g - np.maximum(r, b)                      # > 0 only where green dominates
    alpha = np.clip(1.0 - (green - 40.0) / 70.0, 0, 1)  # 40: still object (olive cloth), 110: pure screen
    alpha[(g > 150) & (green > 90)] = 0
    # spill: no pixel may be greener than its other channels allow
    lim = np.maximum(r, b) + 12
    g2 = np.where((g > lim) & (alpha > 0), lim, g)
    solid = alpha > 0.5
    lab, n = ndimage.label(solid)
    if n > 1 and not keep_small:
        sizes = ndimage.sum(solid, lab, range(1, n + 1))
        small = np.isin(lab, [i + 1 for i, s in enumerate(sizes) if s < 150])
        alpha[small] = 0
    out = np.dstack([r, g2, b, alpha * 255]).clip(0, 255).astype(np.uint8)
    ys, xs = np.where(alpha > 0.03)
    return Image.fromarray(out, 'RGBA').crop((xs.min(), ys.min(), xs.max() + 1, ys.max() + 1))

if __name__ == '__main__':
    src, dst = sys.argv[1], sys.argv[2]
    im = key(Image.open(src))
    if '--flip' in sys.argv: im = im.transpose(Image.FLIP_LEFT_RIGHT)
    if '--max' in sys.argv:
        w = int(sys.argv[sys.argv.index('--max') + 1])
        if im.width > w: im = im.resize((w, round(im.height * w / im.width)), Image.LANCZOS)
    im.save(dst); print(dst, im.size)
