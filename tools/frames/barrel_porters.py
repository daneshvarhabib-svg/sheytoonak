"""Samir's poison: two porters and three barrels on a pole. The image model cannot draw six two-man groups in one sheet
(user 2026-11-21: «تصویر دو نفره درست در نمیاد»), so each porter has his own six-pose walk sheet and the group is put
together here: the pole and barrels come from the first two-man picture (the men cut away, the pole carried on behind
them), each porter gets two RIFE in-betweens per pose, the rear man walks half a cycle out of step, and the load bobs
with the steps. Output: Art/Poison/barrels.png (the first frame) + barrels_walk1..18, all on one canvas.
python3 barrel_porters.py group.png porterA_sheet.png porterB_sheet.png"""
import sys, os, tempfile, numpy as np
from PIL import Image
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import hero_sheets as hs, rife_tween as rt
from key_green import key
P='/home/user/sheytoonak/Assets/Resources/Art/Poison/'
def cycle(sheet, h):
    """18 frames of one porter, all on one canvas (hips at the middle, feet on the bottom line), h pixels tall."""
    F=hs.split(sheet); s=max(f.shape[0] for f in F); k=h/s; W=(int(max(f.shape[1] for f in F)*k*1.5)+63)//32*32; H=(h+12+31)//32*32
    K=[]
    for f in F:
        im=Image.fromarray(f).resize((max(1,round(f.shape[1]*k)),max(1,round(f.shape[0]*k))),Image.LANCZOS); a=np.array(im)
        c=np.zeros((H,W,4),np.uint8); x0=int(W/2-hs.hips(f)*k); y0=H-6-a.shape[0]; c[y0:y0+a.shape[0],x0:x0+a.shape[1]]=a; K.append(c)
    tmp=tempfile.mkdtemp(); out=[]
    for i in range(len(K)):
        a,b=K[i],K[(i+1)%len(K)]; out.append(a)
        for t in (1/3,2/3):
            c=rt.tween(a,b,t,tmp); sa,sb,sc=(a[...,3]>128).sum(),(b[...,3]>128).sum(),(c[...,3]>128).sum()
            out.append(c if 0.94*min(sa,sb)<=sc<=1.06*max(sa,sb) else (a if t<0.5 else b))
    return out
# Measured on the two-man picture (1776x896): the figures span x 144..1588, y 30..850; the pole lies on rows 150..180
# from x 186 to 1536; the barrels and their ropes hang between x 534 and 1210; the men's hips are at x ~400 and ~1350.
TOP,BOT,LEFT,RIGHT=30,850,144,1588; PY0,PY1,PX0,PX1=150,180,186,1536; LX0,LX1=548,1205; TILE=(570,610); HIPA,HIPB=400,1350; LOADBOT=512
def run(group, sa, sb):
    g=np.array(Image.open(group).convert('RGB')).astype(np.float32)
    green=g[...,1]-np.maximum(g[...,0],g[...,2]); al=np.clip(1-(green-40)/70,0,1); al[(g[...,1]>150)&(green>90)]=0
    lim=np.maximum(g[...,0],g[...,2])+12; g[...,1]=np.where(g[...,1]>lim,lim,g[...,1]); full=np.dstack([g,al*255]).astype(np.uint8)
    manH=BOT-TOP; H=manH+16; W=full.shape[1]
    A=cycle(sa,manH); B=cycle(sb,manH); n=len(A); tile=full[PY0:PY1,TILE[0]:TILE[1]]
    for i in range(n):
        bob=int(round(3*abs(np.sin(i/n*2*np.pi)))); off=10+bob-TOP      # picture row y -> canvas row y+off
        back=np.zeros((H,W,4),np.uint8)
        for x in range(PX0,PX1,tile.shape[1]):
            w=min(tile.shape[1],PX1-x); back[PY0+off:PY1+off,x:x+w]=tile[:,:w]
        ld=full[TOP:LOADBOT,LX0:LX1]; dst=back[TOP+off:LOADBOT+off,LX0:LX1]; back[TOP+off:LOADBOT+off,LX0:LX1]=np.where(ld[...,3:4]>0,ld,dst)   # only down to the barrels: lower, the men's legs reach into these columns
        c=Image.fromarray(back)
        for fr,x in ((A[i],HIPA),(B[(i+n//2)%n],HIPB)):
            im=Image.fromarray(fr); layer=Image.new('RGBA',(W,H),(0,0,0,0)); layer.paste(im,(x-im.width//2,H-im.height)); c=Image.alpha_composite(c,layer)
        c=c.crop((LEFT-60,0,RIGHT+60,H)); c.save(P+f'barrels_walk{i+1}.png')
        if i==0: c.save(P+'barrels.png')
    print('barrels',c.size,n,'frames')
if __name__=='__main__': run(*sys.argv[1:4])
