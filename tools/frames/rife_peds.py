# In-betweens for the pedestrians' walk sheets (user 2026-11-18: «حرکت عابر ها رو مثل حرکت قهرمان ها ... طبیعی کن»).
# Key frames ped_NN_name_a.._f (the painted sheet) become a 3x longer loop a..r: every third frame is painted, the two
# after it come from RIFE (tools/frames/rife_tween.py). Run once on freshly cut sheets: python3 rife_peds.py
import os, sys, glob, tempfile, numpy as np
from PIL import Image
sys.argv=[sys.argv[0]]
sys.path.insert(0, os.path.dirname(__file__))
import rife_tween as rt
R='/home/user/sheytoonak/Assets/Resources/Art/Props/'
L='abcdefghijklmnopqrstuvwxyz'
def stat(x):
    al=x[...,3]>128; lum=x[...,:3].astype(int).sum(-1)
    return al.sum(), ((lum<60)&al).sum()/max(1,al.sum())
for first in sorted(glob.glob(R+'ped_*_a.png')):
    base=first[:-6]; files=sorted(glob.glob(base+'_?.png')); K=[np.array(Image.open(f).convert('RGBA')) for f in files]
    H=max(k.shape[0] for k in K); W=max(k.shape[1] for k in K); H32=(H+31)//32*32; W32=(W+63)//32*32
    def canvas(k):
        c=np.zeros((H32,W32,4),np.uint8); x=(W32-k.shape[1])//2; c[H32-k.shape[0]:,x:x+k.shape[1]]=k; return c
    K=[canvas(k) for k in K]; n=len(K); tmp=tempfile.mkdtemp(); out=[]; good=bad=0
    for i in range(n):
        a,b=K[i],K[(i+1)%n]; out.append(a); (sa,da),(sb,db)=stat(a),stat(b)
        for t in (1/3,2/3):
            c=rt.tween(a,b,t,tmp); sc,dc=stat(c)
            if sc>=0.94*min(sa,sb) and sc<=1.06*max(sa,sb) and dc<=max(da,db)+0.01: out.append(c); good+=1
            else: out.append(a if t<0.5 else b); bad+=1
    for f in files: os.remove(f)
    for i,c in enumerate(out): Image.fromarray(c).save(base+'_'+L[i]+'.png')
    print(os.path.basename(base), n,'->',len(out),'model',good,'kept',bad, flush=True)
