# In-betweens for every six-pose loop that still stepped (user 2026-11-21: «حرکت ها رو انیمیشنی تر کنیم ... هنوز پله
# داره بیشتر حرکت ها»). Same recipe as rife_peds.py: each painted frame stays, two RIFE frames follow it (3x longer).
# Sets are named either prefix_a.._f (letters) or prefix1..6 (numbers); loops wrap, one-shots (the pour) do not.
# Run once on freshly cut sheets: python3 rife_sets.py   (a set that already has more than 8 frames is skipped)
import os, sys, tempfile, numpy as np
from PIL import Image
sys.argv=[sys.argv[0]]
sys.path.insert(0, os.path.dirname(__file__))
import rife_tween as rt
A='/home/user/sheytoonak/Assets/Resources/Art/'
L='abcdefghijklmnopqrstuvwxyz'
SETS=[('Traffic/carriage_','L',True),('Traffic/donkey_cart_','L',True),('Traffic/horseman_','L',True),('Traffic/litter_','L',True),
      ('Traffic/porter_cart_','L',True),('Traffic/shepherd_','L',True),
      ('Props/medics_empty_','L',True),('Props/medics_loaded_','L',True),('Props/sweeper_walk','N',True),('Props/sweeper_sweep','N',True),
      ('Poison/bus_walk','N',True),('Poison/truck_walk','N',True),('Poison/tow_truck_walk','N',True),('Poison/tanker_0_walk','N',True),
      ('Poison/tanker_1_pour','N',False),('UI/searching_','N',True)]
def stat(x):
    al=x[...,3]>128; lum=x[...,:3].astype(int).sum(-1)
    return al.sum(), ((lum<60)&al).sum()/max(1,al.sum())
def names(base,kind):
    out=[]
    for i in range(26):
        f=base+(L[i] if kind=='L' else str(i+1))+'.png'
        if not os.path.exists(f): break
        out.append(f)
    return out
only=os.environ.get('ONLY')
for base,kind,loop in SETS:
    if only and only not in base: continue
    files=names(A+base,kind)
    if len(files)<2 or len(files)>8: print(base,'skip',len(files)); continue
    K=[np.array(Image.open(f).convert('RGBA')) for f in files]
    H,W=K[0].shape[:2]
    if any(k.shape[:2]!=(H,W) for k in K): print(base,'SIZES DIFFER, skipped'); continue
    H32=(H+31)//32*32; W32=(W+31)//32*32
    def canvas(k):
        c=np.zeros((H32,W32,4),np.uint8); c[:H,:W]=k; return c
    n=len(K); tmp=tempfile.mkdtemp(); out=[]; good=bad=0
    for i in range(n if loop else n-1):
        a,b=canvas(K[i]),canvas(K[(i+1)%n]); out.append(K[i]); (sa,da),(sb,db)=stat(a),stat(b)
        for t in (1/3,2/3):
            c=rt.tween(a,b,t,tmp,one=False)[:H,:W]; sc,dc=stat(c)
            if sc>=0.94*min(sa,sb) and sc<=1.06*max(sa,sb) and dc<=max(da,db)+0.01: out.append(c); good+=1
            else: out.append(K[i] if t<0.5 else K[(i+1)%n]); bad+=1
    if not loop: out.append(K[-1])
    for i,c in enumerate(out): Image.fromarray(c).save(A+base+(L[i] if kind=='L' else str(i+1))+'.png')
    print(base, n,'->',len(out),'model',good,'kept',bad, flush=True)
