"""Every png under Assets/Resources needs a .meta (nPOTScale 0), else Unity stretches it to a power of two. Run after adding pictures."""
import os, hashlib, re
root=os.path.join(os.path.dirname(__file__),'..','..'); tpl=open(os.path.join(root,'Assets/Resources/Art/Poison/cart.png.meta')).read(); n=0
for d,_,fs in os.walk(os.path.join(root,'Assets/Resources')):
    for f in fs:
        p=os.path.join(d,f)
        if f.endswith('.png') and not os.path.exists(p+'.meta'):
            rel=os.path.relpath(p,root); open(p+'.meta','w').write(re.sub(r'guid: [0-9a-f]+','guid: '+hashlib.md5(('sheytoonak:'+rel).encode()).hexdigest(),tpl,count=1)); n+=1
        if f.endswith('.png.meta') and not os.path.exists(p[:-5]): os.remove(p)
print('metas added',n)
