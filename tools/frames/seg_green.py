# A video that did not keep its green screen (a park, a dark studio, a magenta screen): the person is cut out with a
# segmentation model (rembg, u2net_human_seg; runs here, nothing is sent anywhere) and put on pure green, so the
# video_walk / video_idle / video_kick tools can read the frames.
#   python3 tools/frames/seg_green.py <src_dir> <dst_dir> <first> <last> [x0 x1] [model] [noball]
# model: u2net_human_seg (default; a person in a real scene) or isnet-general-use (plain backdrop; keeps a costume whole).
# A white football the video added by itself is cut away (white, colourless pixels and the disc around them).
import sys, os, numpy as np
from PIL import Image
from rembg import remove, new_session
src, dst, a, b = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4])
x = (int(sys.argv[5]), int(sys.argv[6])) if len(sys.argv) > 6 else None
model = sys.argv[7] if len(sys.argv) > 7 else 'u2net_human_seg'
os.makedirs(dst, exist_ok=True); s = new_session(model)
for i in range(a, b + 1):
    im = Image.open(f'{src}/f_{i:03d}.png').convert('RGB')
    if x: im = im.crop((x[0], 0, x[1], im.height))
    m = np.array(remove(im, session=s, only_mask=True)); p = np.array(im)
    # RGBA: the tools take the cut-out as it is, so a hero's own greens (vine leaves) are not keyed away
    from scipy import ndimage
    keep = m >= 70
    # screen colour the model let through (between the legs, beside a hand): pure magenta or pure screen green
    q = p.astype(int)
    keep &= ~((q[..., 0] > 170) & (q[..., 2] > 170) & (q[..., 1] < 110))
    keep &= ~((q[..., 1] > 170) & (q[..., 0] < 110) & (q[..., 2] < 110))
    pf = p.astype(int); white = keep & (pf.min(-1) > 150) & (pf.max(-1) - pf.min(-1) < 40)
    white = ndimage.binary_opening(white, iterations=3)
    if white.sum() > 800 and 'noball' not in sys.argv:   # noball: a grey-white subject (the pigeon) is not a football
        ys, xs = np.where(white); cy, cx = (ys.min() + ys.max()) / 2, (xs.min() + xs.max()) / 2
        r = max(ys.max() - ys.min(), xs.max() - xs.min()) / 2 * 1.12
        yy, xx = np.ogrid[:keep.shape[0], :keep.shape[1]]; keep &= (yy - cy) ** 2 + (xx - cx) ** 2 > r * r
    lab, n = ndimage.label(keep)
    if n > 1:
        sz = ndimage.sum(keep, lab, range(1, n + 1)); keep = lab == 1 + int(np.argmax(sz))   # the video's own ball goes
    # a rim of green screen left on the outline: only there, pull the green down (inside, his own greens stay)
    rim = keep & ~ndimage.binary_erosion(keep, iterations=4)
    lim = np.maximum(p[..., 0], p[..., 2]).astype(int) + 8
    p[..., 1] = np.where(rim & (p[..., 1] > lim), lim, p[..., 1]).astype(np.uint8)
    p[~keep] = (0, 255, 0)
    Image.fromarray(np.dstack([p, (keep * 255).astype(np.uint8)])).save(f'{dst}/f_{i:03d}.png')
