import re, math, sys
from PIL import Image
import os; S=os.path.dirname(os.path.abspath(__file__))+"/"
txt=open(S+"tables.txt").read().splitlines()
tabs={}; cur=None
for l in txt:
    if 'new Part' in l: tabs[cur].append(l)
    elif l.strip(): cur=l.split()[0]; tabs[cur]=[]
def render(h,pose):
    parts=[]
    for r in tabs[h]:
        m=re.search(r'Part\("(\w+)", (null|"(\w+)"), ([\-\d.]+)f, ([\-\d.]+)f, ([\-\d.]+)f, ([\-\d.]+)f, (\d+)',r)
        parts.append(dict(name=m[1],parent=m[3],pv=(float(m[4]),float(m[5])),loc=(float(m[6]),float(m[7])),order=int(m[8])))
    P={p['name']:p for p in parts}; world={}
    def wt(n):
        if n in world: return world[n]
        p=P[n]; a=pose.get(n,0)
        if p['parent'] is None: pos=p['loc']; ang=a
        else:
            ppos,pang=wt(p['parent']); c,s=math.cos(math.radians(pang)),math.sin(math.radians(pang))
            lx,ly=p['loc']; pos=(ppos[0]+lx*c-ly*s,ppos[1]+lx*s+ly*c); ang=pang+a
        world[n]=(pos,ang); return world[n]
    sc=25; cv=Image.new('RGBA',(400,420),(70,70,70,255)); ox,oy=200,400
    for p in sorted(parts,key=lambda p:p['order']):
        (x,y),ang=wt(p['name'])
        im=Image.open(f"/home/user/sheytoonak/Assets/Resources/Art/Heroes/{h}_rig/{p['name']}.png")
        w,hh=im.size; px,py=p['pv'][0]*w,(1-p['pv'][1])*hh
        big=Image.new('RGBA',(w*3,hh*3)); big.paste(im,(int(w*1.5-px),int(hh*1.5-py)))
        big=big.rotate(ang,resample=Image.BICUBIC,center=(w*1.5,hh*1.5)).resize((int(w*3*sc/100),int(hh*3*sc/100)))
        cv.alpha_composite(big,(int(ox+x*sc-big.width/2),int(oy-y*sc-big.height/2)))
    return cv
heroes=sys.argv[1:] or list(tabs)
kick={'leg_near':36,'leg_far':-5,'arm_far':-12,'arm_near':10,'torso':-3}
back={'leg_near':-22,'leg_far':3,'arm_far':-8,'arm_near':6}
walk={'leg_far':20,'leg_near':-20,'arm_far':-12,'arm_near':0}
out=Image.new('RGB',(400*len(heroes),420*3))
for i,h in enumerate(heroes):
    for j,pz in enumerate([back,kick,walk]): out.paste(render(h,pz).convert('RGB'),(400*i,420*j))
out.save(S+'poses.jpg')
