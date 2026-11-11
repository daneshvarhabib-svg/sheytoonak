import sys, os, re, uuid, numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage
import os; S=os.path.dirname(os.path.abspath(__file__))+"/"
sys.path.insert(0,S); from cfg import C
R="/home/user/sheytoonak/"
names={1:'head',2:'torso',3:'shorts',4:'arm_far',5:'arm_near',6:'leg_far',7:'leg_near'}
order={'leg_far':1,'leg_near':2,'head':3,'shorts':4,'arm_far':5,'torso':6,'arm_near':7}
meta=open(R+'Assets/Resources/Art/Heroes/abu_shahata_idle.png.meta').read()
def poly(pts,W,H):
    m=Image.new('L',(W,H),0); ImageDraw.Draw(m).polygon(pts,fill=1); return np.array(m,bool)
def run(h,write):
    c=C[h]; im=np.array(Image.open(R+f'Assets/ArtArchive/Heroes/toon/{h}_toon_back.png').convert('RGBA'))
    src=im[...,:3]; A=im[...,3]; H,W=A.shape; fg=A>40
    yy,xx=np.mgrid[0:H,0:W]
    L=np.zeros((H,W),np.int8)
    def put(k,m): L[(L==0)&fg&m]=k
    put(1,poly(c['head'],W,H)); put(4,poly(c['arm_far'],W,H)); put(5,poly(c['arm_near'],W,H))
    low=poly(c['lower'],W,H); lowbot=max(p[1] for p in c['lower'])
    put(3,low); put(2,yy<c['waist'][1]+5)
    put(6,(yy>=lowbot-5)&(xx<c['split'])); put(7,(yy>=lowbot-5))
    known=L>0
    _,(iy,ix)=ndimage.distance_transform_edt(~known,return_indices=True)
    L=np.where(fg&~known,L[iy,ix],L); L[~fg]=0
    J={'shorts':(None,c['waist']),'torso':('shorts',c['waist']),'head':('torso',c['neck']),
       'arm_far':('torso',c['sh_far']),'arm_near':('torso',c['sh_near']),
       'leg_far':('shorts',c['hip_far']),'leg_near':('shorts',c['hip_near'])}
    ys,xs=np.where(fg); top,bot=ys.min(),ys.max(); x0b,x1b=xs.min(),xs.max()
    FEET=((x0b+x1b)/2,bot)
    out=R+f'Assets/Resources/Art/Heroes/{h}_rig/'
    if write:
        os.makedirs(out,exist_ok=True)
        for f in os.listdir(out): os.remove(out+f)
    rng=np.random.default_rng(7); tab={}; prev=Image.new('RGBA',(W,H),(70,70,70,255)); layers=[]
    for k,n in names.items():
        M=L==k
        if not M.any(): raise SystemExit(f'{h}: empty {n}')
        above=np.zeros((H,W),bool)
        for k2,n2 in names.items():
            if order[n2]>order[n] and not (n=='shorts' and n2.startswith('arm')): above|=L==k2
        def disc(p,r): return (xx-p[0])**2+(yy-p[1])**2<r*r
        if n.startswith('leg'):
            band=np.zeros((H,W),bool)
            topy=np.array([np.where(M[:,x])[0].min() if M[:,x].any() else H for x in range(W)])
            hip=J[n][1][1]-20
            for x in range(W):
                t=topy[x]
                if t<H and t>hip and L[max(t-4,0),x]==3: band[hip:t,x]=True
            # ⚠️ only where the shorts/skirt piece will cover it: an A-line skirt is narrower at the waist than at the
            # hem, and a straight column of hidden shin stuck out beside it as a flat slab (preview, 2026-11-11)
            band&=~M; band&=ndimage.binary_dilation(L==3,iterations=6)
            ref=M&(yy<topy.min()+60) if topy.min()<H else M
        else:
            joints=[J[n][1]]+[J[m][1] for m in J if J[m][0]==n]
            dm=np.zeros((H,W),bool)
            for p in joints: dm|=disc(p,75)
            band=ndimage.binary_dilation(M,iterations=45)&above&dm
            ref=M&dm if (M&dm).sum()>200 else M
        light=ref&(src.sum(-1)>200)
        tone=np.median(src[light if light.sum()>100 else ref],axis=0)
        rgb=src.copy(); hole=band&~M
        rgb[hole]=np.clip(tone+rng.normal(0,4,(H,W,1))[hole],0,255).astype(np.uint8)
        if n.startswith('leg') and hole.any():
            hx=np.where(hole.any(0))[0]
            for x in list(hx[:6])+list(hx[-6:]): rgb[hole[:,x],x]=[60,32,18]
        alpha=np.where(M,A,0).astype(np.uint8); alpha[hole]=255
        lb,nb=ndimage.label(alpha>0)
        if nb>1:
            sizes=ndimage.sum(np.ones_like(lb),lb,range(1,nb+1)); keep=1+int(np.argmax(sizes)); alpha[(lb!=keep)&(lb>0)]=0
        ys2,xs2=np.where(alpha>0); x0,x1,y0,y1=xs2.min(),xs2.max()+1,ys2.min(),ys2.max()+1
        img=np.dstack([rgb,alpha])[y0:y1,x0:x1]
        if write:
            p=out+n+'.png'; Image.fromarray(img,'RGBA').save(p)
            open(p+'.meta','w').write(re.sub(r'guid: \w+','guid: '+uuid.uuid4().hex,meta).replace('maxTextureSize: 512','maxTextureSize: 1024'))
        par,(px,py)=J[n]; w,hh=x1-x0,y1-y0
        pvx=(px-x0)/w; pvy=1-(py-y0)/hh
        if par is None: lx,ly=(px-FEET[0])/100,-(py-FEET[1])/100
        else: ppx,ppy=J[par][1]; lx,ly=(px-ppx)/100,-(py-ppy)/100
        ps='"'+par+'"' if par else 'null'
        tab[n]=f'                new Part("{n}", {ps}, {pvx:.4f}f, {pvy:.4f}f, {lx:.3f}f, {ly:.3f}f, {order[n]}),'
        layers.append((order[n],img,x0,y0))
    for o,img,x0,y0 in sorted(layers,key=lambda t:t[0]): prev.alpha_composite(Image.fromarray(img,'RGBA'),(int(x0),int(y0)))
    cols=np.array([[0,0,0],[255,80,80],[240,240,240],[200,150,60],[80,80,255],[80,220,255],[160,80,200],[255,200,0]],np.uint8)
    seg=Image.fromarray(cols[L])
    both=Image.new('RGB',(W*2,H)); both.paste(seg,(0,0)); both.paste(prev.convert('RGB'),(W,0))
    both.resize((W,H//2)).save(S+h+'_seg.jpg')
    if write:
        # idle = the back picture cropped to the figure (same pixels the puppet is cut from)
        crop=Image.fromarray(im).crop((x0b,top,x1b+1,bot+1)); crop.save(R+f'Assets/Resources/Art/Heroes/{h}_idle.png')
    J_order=['shorts','torso','head','arm_far','arm_near','leg_far','leg_near']
    return "\n".join(tab[n] for n in J_order), (bot-top+1)/100
if __name__=='__main__':
    w='--write' in sys.argv
    for h in [a for a in sys.argv[1:] if not a.startswith('--')] or list(C):
        t,hgt=run(h,w); print(h,hgt); print(t) if w else None
