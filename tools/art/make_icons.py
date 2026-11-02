"""White glyph icons (256 px, transparent) for the duel HUD. The HUD puts them on a red badge (poison),
green badge (antidote) or dark badge (controls). User 2026-11-02: «سم و زهر نباید نوشته بشه … باید تصویر و آیکون باشه»."""
import cairosvg, os, sys
OUT = sys.argv[1]
W = 'fill="#fff" stroke="#fff"'
S = 'fill="none" stroke="#fff" stroke-width="14" stroke-linecap="round" stroke-linejoin="round"'
icons = {
 # pigeon: body, head, wing, tail, beak
 "pigeon": f'''<ellipse cx="120" cy="150" rx="70" ry="45" {W}/><circle cx="185" cy="100" r="30" {W}/>
   <path d="M205 100 L235 108 L205 114 Z" {W}/><path d="M55 150 L15 120 L25 170 Z" {W}/>
   <path d="M80 140 Q120 110 160 140 Q120 160 80 140 Z" fill="#000" fill-opacity="0.3"/>
   <line x1="110" y1="192" x2="104" y2="222" {S}/><line x1="140" y1="192" x2="146" y2="222" {S}/>''',
 "bus": f'''<rect x="20" y="60" width="216" height="130" rx="18" {W}/>
   <rect x="40" y="78" width="40" height="40" rx="6" fill="#000" fill-opacity="0.45"/><rect x="95" y="78" width="40" height="40" rx="6" fill="#000" fill-opacity="0.45"/>
   <rect x="150" y="78" width="40" height="40" rx="6" fill="#000" fill-opacity="0.45"/><rect x="200" y="78" width="22" height="70" rx="5" fill="#000" fill-opacity="0.45"/>
   <circle cx="70" cy="196" r="24" {W}/><circle cx="186" cy="196" r="24" {W}/>
   <circle cx="70" cy="196" r="10" fill="#000" fill-opacity="0.45"/><circle cx="186" cy="196" r="10" fill="#000" fill-opacity="0.45"/>''',
 "wind": f'''<path d="M20 90 H160 A30 30 0 1 0 130 60" {S}/><path d="M20 135 H200 A30 30 0 1 1 170 165" {S}/><path d="M40 180 H120 A22 22 0 1 1 100 202" {S}/>''',
 "open_window": f'''<rect x="68" y="40" width="120" height="170" rx="6" {S}/><path d="M68 40 L22 62 V190 L68 210" {S}/><path d="M188 40 L234 62 V190 L188 210" {S}/>
   <circle cx="128" cy="105" r="28" {W}/><path d="M86 210 Q128 130 170 210 Z" {W}/>''',
 "puddle": f'''<ellipse cx="128" cy="180" rx="105" ry="40" {W}/><path d="M128 30 C 100 80 92 105 92 122 A36 36 0 0 0 164 122 C164 105 156 80 128 30 Z" {W}/>''',
 "nails": f'''<g {W}><rect x="44" y="40" width="40" height="14" rx="4"/><path d="M56 54 h16 v120 l-8 26 l-8 -26 z"/>
   <rect x="108" y="56" width="40" height="14" rx="4"/><path d="M120 70 h16 v110 l-8 26 l-8 -26 z"/>
   <rect x="172" y="40" width="40" height="14" rx="4"/><path d="M184 54 h16 v120 l-8 26 l-8 -26 z"/></g>''',
 "vane": f'''<line x1="128" y1="70" x2="128" y2="236" {S}/><line x1="40" y1="90" x2="216" y2="90" {S}/>
   <path d="M216 90 L182 66 L182 114 Z" {W}/><path d="M40 90 L20 62 L66 62 L66 118 L20 118 Z" {W}/><circle cx="128" cy="90" r="14" {W}/>''',
 "pebble": f'''<rect x="140" y="30" width="96" height="120" rx="6" {S}/><line x1="188" y1="30" x2="188" y2="150" {S}/>
   <ellipse cx="74" cy="182" rx="40" ry="32" {W}/><path d="M112 156 L140 134 M100 132 L122 112 M126 186 L154 176" {S}/>''',
 "plank": f'''<g transform="rotate(-18 128 128)"><rect x="18" y="104" width="220" height="50" rx="8" {W}/>
   <line x1="40" y1="122" x2="150" y2="122" stroke="#000" stroke-opacity="0.35" stroke-width="7"/>
   <circle cx="45" cy="129" r="7" fill="#000" fill-opacity="0.45"/><circle cx="210" cy="129" r="7" fill="#000" fill-opacity="0.45"/></g>
   <ellipse cx="128" cy="210" rx="96" ry="22" fill="#fff" fill-opacity="0.55"/>''',
 "fart": f'''<g {W}><circle cx="90" cy="140" r="44"/><circle cx="140" cy="110" r="52"/><circle cx="185" cy="148" r="40"/><rect x="70" y="140" width="140" height="44" rx="22"/></g>
   <path d="M36 70 q12 -14 0 -28 M62 58 q12 -14 0 -28 M224 78 q12 -14 0 -28" {S}/>''',
 "rage": f'''<path d="M128 18 C150 70 210 90 200 160 A72 72 0 0 1 56 160 C50 120 80 104 92 70 C104 100 118 104 120 120 C140 90 138 50 128 18 Z" {W}/>
   <path d="M128 230 A40 40 0 0 1 96 170 C110 150 124 150 128 130 C134 154 160 160 160 190 A40 40 0 0 1 128 230Z" fill="#000" fill-opacity="0.35"/>''',
 "pass": f'''<path d="M40 60 L130 128 L40 196 Z" {W}/><path d="M120 60 L210 128 L120 196 Z" {W}/><rect x="212" y="60" width="18" height="136" rx="4" {W}/>''',
 "surrender": f'''<line x1="60" y1="30" x2="60" y2="236" {S}/><path d="M66 40 Q120 20 150 44 T226 48 V140 Q190 116 150 138 T66 136 Z" {W}/>''',
 "window": f'''<rect x="40" y="30" width="176" height="196" rx="10" {S}/><line x1="128" y1="30" x2="128" y2="226" {S}/><line x1="40" y1="128" x2="216" y2="128" {S}/>
   <path d="M150 50 L196 50 L196 106 Z" fill="#fff" fill-opacity="0.5"/>''',
 "shards": f'''<g {W}><path d="M40 190 L90 60 L120 170 Z"/><path d="M120 200 L160 40 L210 150 Z"/><path d="M60 226 L110 196 L150 230 Z" fill-opacity="0.7"/></g>''',
 "curse": f'''<path d="M30 50 H226 V170 H120 L70 220 V170 H30 Z" {W}/>
   <text x="128" y="138" font-family="DejaVu Sans" font-weight="bold" font-size="78" text-anchor="middle" fill="#000" fill-opacity="0.55">#@!</text>''',
 "clock": f'''<circle cx="128" cy="138" r="96" {S}/><line x1="128" y1="138" x2="128" y2="76" {S}/><line x1="128" y1="138" x2="170" y2="160" {S}/><rect x="104" y="14" width="48" height="22" rx="6" {W}/>''',
 "replay": f'''<path d="M200 128 A72 72 0 1 1 170 70" {S}/><path d="M150 40 L206 60 L176 108 Z" {W}/>''',
}
os.makedirs(OUT, exist_ok=True)
for name, body in icons.items():
    svg = f'<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256">{body}</svg>'
    cairosvg.svg2png(bytestring=svg.encode(), write_to=os.path.join(OUT, name + ".png"), output_width=256, output_height=256)
print(len(icons), "icons")
