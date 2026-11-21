"""Three six-pose sheets of a hero (idle, walk, kick; white or green backdrop) -> Art/Heroes/<id>_frames/ with frames.txt,
ready for rife_tween.run(id) (which replaces the in-between step_/shot_ pictures).
python3 hero_sheets.py <id> <idle.png> <walk.png> <kick.png> [--lefty]
Pivots: idle = between the feet, walk = the hips (torso rows), kick = between the feet on the ground (see run). Standing height is the tallest figure of each sheet."""
import sys, os, shutil, numpy as np
from PIL import Image
from scipy import ndimage
R='/home/user/sheytoonak/Assets/Resources/Art/Heroes/'
STAND=700   # pixels a standing hero is tall in the stored frames
def key(path):
    a=np.asarray(Image.open(path).convert('RGB')).astype(np.float32); r,g,b=a[...,0],a[...,1],a[...,2]
    corner=a[:20,:20].reshape(-1,3).mean(0)
    if corner.min()>225:   # white backdrop: what is white AND reaches the border (or is a big white hole) is backdrop
        white=(np.minimum(np.minimum(r,g),b)>236)&((a.max(-1)-a.min(-1))<14)
        lab,n=ndimage.label(white); border=set(lab[0])|set(lab[-1])|set(lab[:,0])|set(lab[:,-1]); border.discard(0)
        sz=ndimage.sum(white,lab,range(1,n+1)); bg=np.isin(lab,list(border)+[i+1 for i,s in enumerate(sz) if s>220])
        d=ndimage.distance_transform_edt(~bg); al=np.clip(d/1.6,0,1)
        # the rim is the figure mixed with white: take the white back out
        A=np.maximum(al,0.35)[...,None]; rim=(al<1)[...,None]
        a=np.where(rim,np.clip((a-(1-A)*255)/A,0,255),a)
    else:
        k=g-np.maximum(r,b); al=np.clip(1-(k-40)/70,0,1); al[(g>150)&(k>90)]=0
        lim=np.maximum(r,b)+12; a[...,1]=np.where(g>lim,lim,g)
    return a,al
def split(path,n=6):
    """The n figures of a row. Neighbours often touch (an arm over the next figure's back), so they are not separated
    as connected pieces: the row is cut at the n-1 emptiest columns between evenly spaced figures, then every piece
    keeps only what is connected to its own middle."""
    a,al=key(path); solid=al>0.5; H,W=solid.shape
    col=solid.sum(0).astype(np.float32); xs=np.where(col>0)[0]; x0,x1=xs.min(),xs.max()+1; step=(x1-x0)/n
    cuts=[x0]
    for i in range(1,n):
        c=int(x0+i*step); lo,hi=int(c-step*0.3),int(c+step*0.3); cuts.append(lo+int(np.argmin(col[lo:hi])))
    cuts.append(x1); out=[]
    for i in range(n):
        m=np.zeros_like(solid); m[:,cuts[i]:cuts[i+1]]=solid[:,cuts[i]:cuts[i+1]]
        lab,c=ndimage.label(ndimage.binary_dilation(m,iterations=3)&(np.arange(W)[None,:]>=cuts[i])&(np.arange(W)[None,:]<cuts[i+1]))
        sz=ndimage.sum(m,lab,range(1,c+1)); keep=lab==(1+int(np.argmax(sz)))
        ys,xx=np.where(keep&(al>0.03)); ya,yb,xa,xb=ys.min(),ys.max()+1,xx.min(),xx.max()+1
        out.append(np.dstack([a[ya:yb,xa:xb],(al*keep)[ya:yb,xa:xb]*255]).astype(np.uint8))
    return out
def feet(f):
    """x-centres of the blobs that touch the ground line (bottom 3 % of the figure), left to right."""
    h=f.shape[0]; band=(f[int(h*0.97):,:,3]>128).any(0); lab,n=ndimage.label(ndimage.binary_dilation(band,iterations=6))
    return [float(np.mean(np.where(lab==i)[0])) for i in range(1,n+1)]
def hips(f):
    h=f.shape[0]; m=f[int(h*0.40):int(h*0.58),:,3]>128; return float(np.where(m)[1].mean())
def run(hero,idle,walk,kick,lefty=False):
    d=R+hero+'_frames/'; os.makedirs(d,exist_ok=True); lines=['# frame pivotX pivotY (fractions of the picture, from its bottom-left) standing-height (fraction of the picture height)']
    PAD=6
    def put(name,f,px,stand,k):
        im=Image.fromarray(f); w,h=max(1,round(f.shape[1]*k)),max(1,round(f.shape[0]*k)); im=im.resize((w,h),Image.LANCZOS)
        c=Image.new('RGBA',(w+2*PAD,h+2*PAD),(0,0,0,0)); c.paste(im,(PAD,PAD)); c.save(d+name+'.png')
        lines.append(f'{name} {(px*k+PAD)/c.width:.4f} {PAD/c.height:.4f} {stand*k/c.height:.4f}')
    I=split(idle); W=split(walk); K=split(kick)
    si=max(f.shape[0] for f in I); sw=max(f.shape[0] for f in W); sk=K[-1].shape[0]   # the landed pose stands
    half=[]
    for i,f in enumerate(I):
        ft=feet(f); mid=(ft[0]+ft[-1])/2 if ft else f.shape[1]/2; put(f'idle_{i}',f,mid,si,STAND/si)
        if len(ft)>=2: half.append(((ft[-1]-ft[0])/2)/si)
    half=float(np.median(half)) if half else 0.1
    for i,f in enumerate(W): put(f'walk_{i}',f,hips(f),sw,STAND/sw)
    for i,f in enumerate(K):
        # ⚠️ the sheets do not keep to one kicking leg (the model swaps it between poses), so "the left foot stands"
        # cannot be trusted: with both feet down the pivot is between them, with one foot down it is that foot moved
        # half a stance toward the hips. Either way the body stays over the same spot as in the idle pictures.
        ft=feet(f); hx=hips(f)
        if len(ft)>=2: px=(ft[0]+ft[-1])/2
        elif ft: px=ft[0]+np.sign(hx-ft[0])*min(abs(hx-ft[0]),half*sk)
        else: px=hx
        put(f'kick_{i}',f,px,sk,STAND/sk)
    # key pictures of the long clips: every third is painted, rife_tween fills the two after it
    L={l.split()[0]:l for l in lines[1:]}
    def clip(prefix,src,loop):
        n=len(src)
        for i,s in enumerate(src):
            for j in range(3 if (loop or i<n-1) else 1):
                nm=f'{prefix}_{i*3+j}'; shutil.copy(d+s+'.png',d+nm+'.png'); lines.append(nm+' '+' '.join(L[s].split()[1:4]))
    clip('step',[f'walk_{i}' for i in range(6)],True); clip('shot',['kick_2','kick_3','kick_4','kick_5'],False)
    open(d+'frames.txt','w').write('\n'.join(lines)+'\n'); print(hero,'stance half',round(half,3),len(lines)-1,'frames')
if __name__=='__main__':
    a=[x for x in sys.argv[1:] if not x.startswith('--')]; run(a[0],a[1],a[2],a[3],'--lefty' in sys.argv)
