import sys, numpy as np
from PIL import Image
src, dst = sys.argv[1], sys.argv[2]
im = np.asarray(Image.open(src).convert("RGB")).astype(np.float32)
r, g, b = im[...,0], im[...,1], im[...,2]
# greenness: how much green exceeds the other two
gx = g - np.maximum(r, b)
alpha = np.clip(1.0 - (gx - 40.0) / (140.0 - 40.0), 0, 1)   # gx<=40 opaque, gx>=140 transparent
# despill: cap green at max(r,b) where it was keyed partially
spill = alpha < 1
cap = np.maximum(r, b)
g2 = np.where((gx > 0) & (alpha > 0), np.minimum(g, cap + 10), g)
out = np.dstack([r, g2, b, alpha * 255]).astype(np.uint8)
img = Image.fromarray(out, "RGBA")
bbox = img.getchannel("A").point(lambda v: 255 if v > 20 else 0).getbbox()
pad = 8
# keep the full width: every pose of a hero shares the framing, so the body stays put when the frame swaps
bbox = (0, max(0,bbox[1]-pad), img.width, min(img.height,bbox[3]+pad))
img = img.crop(bbox)
img.save(dst)
print(dst, img.size)
