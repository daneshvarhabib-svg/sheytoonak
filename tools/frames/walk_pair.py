"""Two pictures of one scene (A installed, B = same scene with the legs mid-step, any size) -> 4 frames on A's canvas.
A, RIFE 1/3, RIFE 2/3, B-aligned; painted wheels cut out of every frame the same way (cut_wheels kinds)."""
import os, sys, tempfile, numpy as np
from PIL import Image
sys.argv=[sys.argv[0]]; sys.path.insert(0, os.path.dirname(__file__))
import rife_tween as rt
from align import align
def keyfull(rgb):
    aa=rgb.astype(float).copy()
    g=aa[...,1]-np.maximum(aa[...,0],aa[...,2]); al=np.clip(1-(g-40)/70,0,1); al[(aa[...,1]>150)&(g>90)]=0
    lim=np.maximum(aa[...,0],aa[...,2])+12; aa[...,1]=np.where(aa[...,1]>lim,lim,aa[...,1])
    return np.dstack([aa,al*255]).astype(np.uint8)
def wheels_out(f, wheels, bodyline):
    aa=f[...,:3].astype(float); al=f[...,3].astype(float)/255; H,W=al.shape; yy,xx=np.mgrid[0:H,0:W]
    for (cx,cy,r,kind) in wheels:
        d=(xx-cx)**2+(yy-cy)**2<=(r+9)**2
        if kind=='near':
            up=d&(yy<bodyline); solid=up&(al>0.9)
            if solid.sum()>50: aa[up&(al>0.2)]=np.median(aa[solid],axis=0)*0.8
            al[d&(yy>=bodyline)]=0
        elif kind=='far': al[d&(yy>=bodyline)]=0
        elif kind=='dark': al[d&(aa.max(-1)<95)]=0
        else: al[d]=0
    return np.dstack([aa,al*255]).astype(np.uint8)
def frames(a_path, b_path, wheels=(), bodyline=0):
    A=np.asarray(Image.open(a_path).convert('RGB')); B=np.asarray(Image.open(b_path).convert('RGB'))
    Bw,inl,s=align(A,B); KA,KB=keyfull(A),keyfull(Bw)
    H,W=A.shape[:2]; H32,W32=(H+31)//32*32,(W+31)//32*32
    # ⚠️ the black/white double run of rife_tween lost the swinging leg here (the two runs disagreed: a hole, or a black
    # leg). These pictures are photographs on green already: interpolate them as they are, then key the green.
    def pad(k): c=np.full((H32,W32,3),(40,240,30),np.uint8); c[:H,:W]=k; return c
    tmp=tempfile.mkdtemp(); PA,PB=pad(A),pad(Bw)
    fr=[KA,keyfull(rt.rife(PA,PB,1/3,tmp)[:H,:W]),keyfull(rt.rife(PA,PB,2/3,tmp)[:H,:W]),KB]
    fr=[wheels_out(f,wheels,bodyline) for f in fr]
    al=fr[0][...,3]/255.; ys,xs=np.where(al>0.03); x0,x1,y0,y1=xs.min(),xs.max()+1,ys.min(),ys.max()+1
    low=max([cy+r for cx,cy,r,k in wheels]+[y1]); y1=int(min(H,max(y1,low+2)))
    return [Image.fromarray(f[y0:y1,x0:x1],'RGBA') for f in fr],(x0,y0,x1,y1),inl
