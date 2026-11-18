"""Key a side-view picture and take its painted wheels out, so loose wheels can turn in their place.
  near wheel (in front of everything): the whole disc goes; above `bodyline` (where the body shows between the spokes)
    it is filled with the body's own colour instead of a hole
  far wheel (behind the body): only the disc below `bodyline` goes
  dark=True: only dark (iron) pixels of the disc go (a thin hoop in front of wood)
Returns the cropped RGBA and the wheels' centres in cropped pixels."""
import numpy as np
from PIL import Image
def cut(path, wheels, bodyline, flip=False):
    aa=np.asarray(Image.open(path).convert('RGB')).astype(float).copy()
    g=aa[...,1]-np.maximum(aa[...,0],aa[...,2]); al=np.clip(1-(g-40)/70,0,1); al[(aa[...,1]>150)&(g>90)]=0
    lim=np.maximum(aa[...,0],aa[...,2])+12; aa[...,1]=np.where(aa[...,1]>lim,lim,aa[...,1])
    H,W=al.shape; yy,xx=np.mgrid[0:H,0:W]
    for (cx,cy,r,kind) in wheels:
        d=(xx-cx)**2+(yy-cy)**2<=(r+9)**2
        if kind=='near':
            up=d&(yy<bodyline); solid=up&(al>0.9)
            if solid.sum()>50:
                col=np.median(aa[solid],axis=0); aa[up&(al>0.2)]=col*0.8
            al[d&(yy>=bodyline)]=0
        elif kind=='far': al[d&(yy>=bodyline)]=0
        elif kind=='dark': al[d&(aa.max(-1)<95)]=0
        else: al[d]=0
    ys,xs=np.where(al>0.03); x0,x1,y0,y1=xs.min(),xs.max()+1,ys.min(),ys.max()+1
    # room below for the loose wheels
    low=max([cy+r for cx,cy,r,k in wheels]+[y1]); y1=int(min(H,max(y1,low+2)))
    im=Image.fromarray(np.dstack([aa,al*255]).astype(np.uint8)[y0:y1,x0:x1],'RGBA')
    ws=[(cx-x0,cy-y0,r,k) for cx,cy,r,k in wheels]
    if flip: im=im.transpose(Image.FLIP_LEFT_RIGHT); ws=[(im.width-cx,cy,r,k) for cx,cy,r,k in ws]
    return im,ws
