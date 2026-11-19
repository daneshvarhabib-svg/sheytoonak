"""A sheet of walk frames on green (a grid) -> frames keyed, aligned on the still upper body, on one common canvas.
frames(path, cols, rows, use=None, xr=None, flip=False): use = cell numbers (row-major) to keep; xr = {cell: (x0, x1)}
for sheets whose cells are not even thirds. Returns RGBA images of equal size."""
import numpy as np, cv2
from PIL import Image
def key(a):
    a=a.astype(float).copy(); r,g,b=a[...,0],a[...,1],a[...,2]
    k=g-np.maximum(r,b); al=np.clip(1-(k-40)/70,0,1); al[(g>150)&(k>90)]=0
    lim=np.maximum(r,b)+12; a[...,1]=np.where(g>lim,lim,g)
    # drop specks
    n,lab,st,_=cv2.connectedComponentsWithStats((al>0.5).astype(np.uint8),8)
    small=np.isin(lab,[i for i in range(1,n) if st[i,cv2.CC_STAT_AREA]<60]); al[small]=0
    return np.dstack([a,al*255]).astype(np.uint8)
def frames(path, cols, rows, use=None, xr=None, flip=False):
    im=np.asarray(Image.open(path).convert('RGB')); H,W=im.shape[:2]; ch=H//rows; cw=W//cols
    cells=[]
    for c in (use if use is not None else range(cols*rows)):
        r0=(c//cols)*ch; x0,x1=(xr or {}).get(c,((c%cols)*cw,(c%cols+1)*cw))
        cell=key(im[r0:r0+ch,x0:x1])
        ys,xs=np.where(cell[...,3]>128); cell=cell[ys.min():ys.max()+1,xs.min():xs.max()+1]
        if flip: cell=cell[:,::-1]
        cells.append(cell)
    # common canvas, bottoms together, then slide each onto the first by its upper 55 % (the part that does not walk)
    Hc=max(c.shape[0] for c in cells)+40; Wc=max(c.shape[1] for c in cells)+80
    def place(c,dx=0,dy=0):
        o=np.zeros((Hc,Wc,4),np.uint8); y=Hc-20-c.shape[0]+dy; x=(Wc-c.shape[1])//2+dx
        o[y:y+c.shape[0],x:x+c.shape[1]]=c; return o
    ref=place(cells[0]); out=[ref]
    top=int(Hc*0.55); ra=ref[:top,:,3].astype(np.float32)
    for c in cells[1:]:
        p=place(c); best=(1e18,0,0)
        pa=p[:,:,3].astype(np.float32)
        for dx in range(-30,31,2):
            for dy in range(-12,13,2):
                s=np.roll(np.roll(pa,dx,1),dy,0)[:top]
                e=float(np.abs(s-ra).sum())
                if e<best[0]: best=(e,dx,dy)
        out.append(place(c,best[1],best[2]))
    al=np.max([o[...,3] for o in out],0); ys,xs=np.where(al>8)
    y0,y1,x0,x1=max(0,ys.min()-4),ys.max()+1,max(0,xs.min()-4),min(Wc,xs.max()+5)
    return [Image.fromarray(o[y0:y1,x0:x1],'RGBA') for o in out]

def cells_as_drawn(path, cols, rows):
    """Frames of something that stands still (a drummer at his drum, a man sweeping): the cells as the sheet has them,
    no sliding, cut to their common box."""
    im=np.asarray(Image.open(path).convert('RGB')); H,W=im.shape[:2]; ch=H//rows; cw=W//cols
    cs=[key(im[(c//cols)*ch:(c//cols+1)*ch,(c%cols)*cw:(c%cols+1)*cw]) for c in range(cols*rows)]
    al=np.max([c[...,3] for c in cs],0); ys,xs=np.where(al>128)
    y0,y1,x0,x1=max(0,ys.min()-3),ys.max()+2,max(0,xs.min()-3),min(cw,xs.max()+4)
    return [Image.fromarray(c[y0:y1,x0:x1],'RGBA') for c in cs]
