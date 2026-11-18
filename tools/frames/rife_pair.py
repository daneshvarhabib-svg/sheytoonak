# Two painted pictures of the same scene (A: legs apart, B: legs passing) -> A, two RIFE in-betweens, B on one canvas.
#   python3 rife_pair.py A.jpg B.jpg <out dir> <name> [poison|traffic]
# poison/props naming: name.png, name_step1.png, name_step2.png, name_step3.png (PoisonProps.AddWalk)
# traffic naming:      name_a.png .. name_d.png (StreetLife.Traffic)
import os, sys, tempfile, numpy as np
from PIL import Image
args=sys.argv[1:]; sys.argv=[sys.argv[0]]
sys.path.insert(0, os.path.dirname(__file__))
import rife_tween as rt
def keyfull(path):
    aa=np.asarray(Image.open(path).convert('RGB')).astype(float).copy()
    g=aa[...,1]-np.maximum(aa[...,0],aa[...,2]); al=np.clip(1-(g-40)/70,0,1); al[(aa[...,1]>150)&(g>90)]=0
    lim=np.maximum(aa[...,0],aa[...,2])+12; aa[...,1]=np.where(aa[...,1]>lim,lim,aa[...,1])
    return np.dstack([aa,al*255]).astype(np.uint8)
a,b,out,name=args[:4]; kind=args[4] if len(args)>4 else 'poison'
A,B=keyfull(a),keyfull(b)
if A.shape!=B.shape: B=np.asarray(Image.fromarray(B).resize((A.shape[1],A.shape[0]),Image.LANCZOS))
m=(A[...,3]>20)|(B[...,3]>20); ys,xs=np.where(m); y0,y1,x0,x1=ys.min(),ys.max()+1,max(0,xs.min()-4),min(A.shape[1],xs.max()+5)
H=(y1-y0+31)//32*32; W=(x1-x0+31)//32*32
def cv(k):
    c=np.zeros((H,W,4),np.uint8); s=k[y0:y1,x0:x1]; c[H-s.shape[0]:,:s.shape[1]]=s; return c
A,B=cv(A),cv(B); tmp=tempfile.mkdtemp()
fr=[A,rt.tween(A,B,1/3,tmp),rt.tween(A,B,2/3,tmp),B]
names=[name+'_'+c for c in 'abcd'] if kind=='traffic' else [name,name+'_step1',name+'_step2',name+'_step3']
for n,c in zip(names,fr): Image.fromarray(c[H-(y1-y0):, :x1-x0]).save(os.path.join(out,n+'.png'))
print(name, x1-x0, y1-y0)
