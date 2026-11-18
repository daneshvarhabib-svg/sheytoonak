# Key frames = step_0,3,6,... and shot_0,3,6,... (the painted pictures on a common canvas). Replace the in-betweens
# (step_1,2,4,5,...) with RIFE interpolations. Alpha is interpolated as a second pass (as a grey picture).
import sys,os,subprocess,numpy as np,tempfile,shutil
from PIL import Image
R='/home/user/sheytoonak/Assets/Resources/Art/Heroes/'
RIFE='/tmp/claude-0/-home-user/eabe8a76-3e76-5115-b83a-d74344bd39cf/scratchpad/rife'
BIN=RIFE+'/build/rife-ncnn-vulkan'; MODEL=RIFE+'/src/models/rife-v4.6'
def load(hero):
    d=R+hero+'_frames/'; F={}
    for l in open(d+'frames.txt'):
        f=l.split()
        if len(f)<4 or f[0].startswith('#') or not os.path.exists(d+f[0]+'.png'): continue
        F[f[0]]=(np.array(Image.open(d+f[0]+'.png').convert('RGBA')),float(f[1]),float(f[2]),float(f[3]))
    return F
H=768; W=832; PX,PY=W//2,H-12
def place(fr):
    im,fx,fy,st=fr; k=(H*0.92)/(st*im.shape[0])
    g=np.array(Image.fromarray(im).resize((max(1,round(im.shape[1]*k)),max(1,round(im.shape[0]*k))),Image.LANCZOS))
    c=np.zeros((H,W,4),np.uint8); x0=int(round(PX-fx*g.shape[1])); y0=int(round(PY-(1-fy)*g.shape[0]))
    xa,ya=max(0,x0),max(0,y0); xb,yb=min(W,x0+g.shape[1]),min(H,y0+g.shape[0])
    c[ya:yb,xa:xb]=g[ya-y0:yb-y0,xa-x0:xb-x0]; return c
BG=np.array([96,96,96],np.float32)
def rife(a,b,t,tmp):
    Image.fromarray(a).save(tmp+'/a.png'); Image.fromarray(b).save(tmp+'/b.png')
    r=subprocess.run([BIN,'-0',tmp+'/a.png','-1',tmp+'/b.png','-o',tmp+'/o.png','-m',MODEL,'-s',str(t),'-g','-1','-j','1:4:1'],capture_output=True,text=True)
    if not os.path.exists(tmp+'/o.png'): raise SystemExit(r.stderr[-800:])
    o=np.array(Image.open(tmp+'/o.png').convert('RGB')); os.remove(tmp+'/o.png'); return o
def tween(A,B,t,tmp):
    # the same pair twice, once over black and once over white: where the two answers agree the pixel is the figure,
    # where they differ it is backdrop. The alpha then moves WITH the picture (interpolating it apart left grey halos).
    def comp(x,bg):
        al=x[...,3:4].astype(np.float32)/255; return (x[...,:3]*al+bg*(1-al)).astype(np.uint8)
    k=rife(comp(A,0.0),comp(B,0.0),t,tmp).astype(np.float32); w=rife(comp(A,255.0),comp(B,255.0),t,tmp).astype(np.float32)
    a=1-np.clip((w-k).mean(-1),0,255)/255
    fg=k/np.maximum(a[...,None],0.12)           # the colour, taken back out of the black backdrop with the RAW alpha
    a=np.clip((a-0.5)/0.3,0,1)                  # then harden: a half-there ghost of a limb goes, it does not turn to mist
    from scipy import ndimage
    lab,n=ndimage.label(a>0.5)
    if n>1:
        sz=ndimage.sum(np.ones_like(lab),lab,range(1,n+1)); a[(lab!=1+int(np.argmax(sz)))&(lab>0)]=0   # stray specks
    return np.dstack([np.clip(fg,0,255),a*255]).astype(np.uint8)
def seq(F,prefix,loop):
    keys=[]; i=0
    while f'{prefix}_{i}' in F: keys.append(place(F[f'{prefix}_{i}'])); i+=3
    return keys
def run(hero,write=True,only=None):
    F=load(hero); d=R+hero+'_frames/'; tmp=tempfile.mkdtemp(); out={}
    for prefix,loop in (('step',True),('shot',False)):
        if only and prefix!=only: continue
        K=seq(F,prefix,loop); n=len(K)
        for i in range(n if loop else n-1):
            a,b=K[i],K[(i+1)%n]
            def stat(x):
                al=x[...,3]>128; lum=x[...,:3].astype(int).sum(-1)
                return al.sum(), ((lum<60)&al).sum()/max(1,al.sum())
            (sa,da),(sb,db)=stat(a),stat(b)
            for j,t in ((1,1/3),(2,2/3)):
                c=tween(a,b,t,tmp); sc,dc=stat(c)
                # a limb the model lost (the figure shrank) or smeared to black: keep the bent-neighbour in-between
                ok = sc>=0.955*min(sa,sb) and sc<=1.05*max(sa,sb) and dc<=max(da,db)+0.006
                print(f'  {prefix}_{i*3+j} area {sc/min(sa,sb):.3f} dark {dc-max(da,db):+.3f} {"ok" if ok else "KEEP OLD"}',flush=True)
                if ok: out[f'{prefix}_{i*3+j}']=c
    shutil.rmtree(tmp)
    if write:
        lines=open(d+'frames.txt').read().splitlines(); new=[]
        for l in lines:
            nm=l.split()[0] if l.split() else ''
            if nm in out:
                c=out[nm]; ys,xs=np.where(c[...,3]>8); x0,x1,y0,y1=max(0,xs.min()-4),min(W,xs.max()+5),max(0,ys.min()-4),min(H,ys.max()+5)
                im=Image.fromarray(c[y0:y1,x0:x1]); im.save(d+nm+'.png')
                l=f'{nm} {(PX-x0)/im.width:.4f} {1-(PY-y0)/im.height:.4f} {0.92*H/im.height:.4f}'
            new.append(l)
        open(d+'frames.txt','w').write("\n".join(new)+"\n")
    return F,out
if __name__=='__main__':
    for h in sys.argv[1:]: run(h); print(h,'done',flush=True)
