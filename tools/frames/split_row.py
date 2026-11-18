"""A row of objects on green (#00FF00) or magenta (#FF00FF) -> the N largest pieces, left to right, as RGBA crops."""
import numpy as np
from PIL import Image
from scipy import ndimage
def split(path, n, bg='green'):
    aa=np.asarray(Image.open(path).convert('RGB')).astype(float).copy(); r,g,b=aa[...,0],aa[...,1],aa[...,2]
    if bg=='green':
        k=g-np.maximum(r,b); al=np.clip(1-(k-40)/70,0,1); al[(g>150)&(k>90)]=0
        lim=np.maximum(r,b)+12; aa[...,1]=np.where(g>lim,lim,g)
    else:
        k=np.minimum(r,b)-g; al=np.clip(1-(k-120)/70,0,1)
        edge=(al<1)|ndimage.binary_dilation(al<0.5,iterations=2)      # pull the pink out of the rim only
        m=(r+b)/2; lim=g+0.35*(m-g)
        aa[...,0]=np.where(edge&(r>lim)&(k>40),lim,r); aa[...,2]=np.where(edge&(b>lim)&(k>40),lim,b)
    lab,c=ndimage.label(ndimage.binary_dilation(al>0.5,iterations=3))
    sz=ndimage.sum(al>0.5,lab,range(1,c+1)); top=sorted(np.argsort(sz)[-n:]+1,key=lambda i: ndimage.center_of_mass(lab==i)[1])
    out=[]
    for i in top:
        m=lab==i; ys,xs=np.where(m); y0,y1,x0,x1=ys.min(),ys.max()+1,xs.min(),xs.max()+1
        out.append(Image.fromarray(np.dstack([aa[y0:y1,x0:x1],(al*m)[y0:y1,x0:x1]*255]).astype(np.uint8),'RGBA'))
    return out
