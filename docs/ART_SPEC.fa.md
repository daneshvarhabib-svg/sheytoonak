# مشخصاتِ تصویرها (آرت) — شیطونک

این سند می‌گوید چه تصویرهایی لازم است، با چه اندازه و چه نامی، و پرامپتِ آماده‌ی هر یک برای
ChatGPT. سبکِ مرجع: `docs/reference/street-reference.png` (انیمه‌ی نیمه‌واقعی، عصرِ آفتابی، خطِ تمیز).

## چرا این شکل

دوربین از پشتِ بازیکن و سه‌بعدی است و با بازیکن چپ و راست می‌رود، پس «یک تصویرِ تمام‌صفحه‌ی ثابت» کار
نمی‌کند. به‌جایش هر تصویر رویِ یک سطحِ سه‌بعدی می‌نشیند:

| چی | چطور در بازی می‌نشیند |
|---|---|
| نمایِ ساختمان | یک صفحه‌ی عمودی در ۱۳ متریِ بازیکن، به عرضِ ۹ تا ۱۲ متر. پنجره‌ها با مختصاتِ نرمال رویِ همین تصویر مارک می‌شوند (`HouseDefinition.windows`) |
| همسایه‌ها | دو صفحه‌ی کم‌ارتفاع‌تر چپ و راستِ نما |
| خیابان و پیاده‌رو | بافتِ تکرارشونده روی زمین |
| آسمان | یک پانورامای عریض که دورتادور می‌چرخد (skybox) |
| بازیکن | اسپرایتِ از پشت، چهار فریم (ایستاده، دو فریمِ ضربه، راه‌رفتن)؛ بعداً شاید سه‌بعدی |
| توپ | بافتِ توپِ فوتبال روی کره‌ی موجود |
| جزئیات | چراغ‌برق، درخت، سطلِ زباله، ماشینِ پارک‌شده، اسپرایت با پس‌زمینه‌ی شفاف |

## قاعده‌های کلی برایِ همه‌ی تصویرها

1. همه در **یک گفت‌وگوی ChatGPT** ساخته شوند و تصویرِ مرجع اولِ همان گفت‌وگو پیوست شود؛ در هر پرامپت
   بنویس «in exactly the style of the reference image». این تنها راهِ یک‌دست شدنِ ۸ ساختمان است.
2. **هیچ آدم، هیچ توپ، هیچ متن یا تابلویِ نوشته‌دار** داخلِ نماها (متن توسطِ مدل خراب می‌شود؛ تابلوی پلاک را بازی خودش می‌نویسد).
3. نما **کاملاً از جلو** (الویشن، بدونِ پرسپکتیو)، لبه‌ی چپ و راستِ ساختمان داخلِ کادر، پایینِ کادر = خطِ پیاده‌رو، بالای کادر کمی آسمان بالای بام.
4. نور همیشه از **بالا-چپ** (همان مرجع)، عصرِ آفتابی، سایه‌های گرم.
5. پنجره‌ها **بزرگ و واضح**، شیشه‌ی روشن (آبی-خاکستریِ بازتابی)، قابِ سنگی روشن. بینِ پنجره‌ها فاصله‌ی کافی (دستِ کم نیم‌عرضِ پنجره) تا ضربه‌ها قابلِ تشخیص باشد.
6. فایل‌ها PNG. نام‌گذاری دقیقاً طبقِ جدول. بعد از ساخت، یا در این چت بفرست یا در GitHub رویِ شاخه‌ی
   `claude/unity-project-structure` در پوشه‌ی `Assets/Art/Raw/` آپلود کن (Add file → Upload files).
7. مارک‌کردنِ پنجره‌ها کارِ من است: روی تصویر شبکه می‌اندازم و مختصات را درمی‌آورم؛ تو فقط تصویر را بده.

## فهرستِ تصویرها

| # | نام فایل | نسبت / اندازه در ChatGPT | چیست |
|---|---|---|---|
| 1 | `house_01.png` | افقی 3:2 (۱۵۳۶×۱۰۲۴) | نمایِ خانه‌ی ۱: آجرِ قرمز-نارنجی، ۲ طبقه + همکفِ مغازه (کافه، سایه‌بونِ قرمز)، **۶ پنجره** (۲ ردیف × ۳) |
| 2 | `house_02.png` | 3:2 | آجرِ کرم، ۳ طبقه + همکف (نانوایی، سایه‌بونِ سبز)، **۹ پنجره** (۳×۳) |
| 3 | `house_03.png` | 3:2 | آجرِ قرمزِ تیره، ۳ طبقه + همکف (گل‌فروشی، سایه‌بونِ آبی)، **۱۲ پنجره** (۳×۴) |
| 4 | `house_04.png` | 3:2 | آجرِ زرد، ۴ طبقه + همکف (کتاب‌فروشی)، **۱۲ پنجره** (۴×۳) |
| 5 | `house_05.png` | 3:2 | سنگِ خاکستری با آجر، ۴ طبقه + همکف (داروخانه)، **۱۶ پنجره** (۴×۴) |
| 6 | `house_06.png` | 3:2 | آجرِ قهوه‌ای، ۴ طبقه + همکف (بستنی‌فروشی، سایه‌بونِ راه‌راه)، **۱۶ پنجره** (۴×۴) + بالکن‌های کوچک |
| 7 | `house_07.png` | 3:2 | آجرِ قرمز با نمایِ گچیِ سفید، ۵ طبقه + همکف (رستوران)، **۲۰ پنجره** (۵×۴) |
| 8 | `house_08.png` | 3:2 | ساختمانِ گوشه‌ای مجلل، آجرِ تیره و قابِ سفید، ۵ طبقه + همکف (هتلِ کوچک، سایه‌بونِ طلایی)، **۲۰ پنجره** (۵×۴) |
| 9 | `neighbour_left.png`، `neighbour_right.png` | 3:2 | دو ساختمانِ همسایه‌ی ساده (بدونِ پنجره‌ی قابلِ شکستن لازم نیست؛ پنجره‌های کوچک و تیره) |
| 10 | `sky_panorama.png` | افقی 3:2، بعداً خودم کِش می‌دهم | آسمانِ عصرِ آفتابی با ابرهای انیمه‌ای، بدونِ زمین |
| 11 | `ground_asphalt.png` | مربع 1:1 (۱۰۲۴×۱۰۲۴) | آسفالتِ خیابان از بالا، **تکرارشونده (seamless)** |
| 12 | `ground_sidewalk.png` | مربع 1:1 | موزاییکِ پیاده‌رو از بالا، تکرارشونده |
| 13 | `player_back_idle.png` | عمودی 2:3، **پس‌زمینه شفاف** | پسرِ نوجوانِ شیطون از پشت، تی‌شرتِ راه‌راه، شلوارکِ کوتاه، کتانی، ایستاده |
| 14 | `player_back_kick_a.png`، `player_back_kick_b.png` | 2:3 شفاف | همان شخصیت، پای راست عقب (آماده‌ی ضربه) / پای راست جلو (بعد از ضربه) |
| 15 | `player_back_walk_a.png`، `player_back_walk_b.png` | 2:3 شفاف | دو فریمِ راه‌رفتن |
| 16 | `ball.png` | 1:1 | بافتِ توپِ فوتبالِ سفید-مشکیِ کلاسیک، کمی کثیف، برای پوشاندنِ کره (equirectangular) |
| 17 | `props_*.png` | 2:3 یا 1:1 شفاف | چراغ‌برق، درختِ پیاده‌رو، سطلِ زباله، ماشینِ پارک‌شده‌ی از پهلو |

ترتیبِ کار: **۱، ۱۱، ۱۲، ۱۰، ۱۳** اول (یک خانه‌ی کامل تا سبک تثبیت شود)، بعد ۱۴ و ۱۵، بعد ۲ تا ۸، بعد ۹ و ۱۷.

## پرامپت‌ها (انگلیسی؛ عیناً کپی کن)

پیشوندِ مشترک که اولِ همه‌ی پرامپت‌ها می‌آید:

> In exactly the style of the attached reference image (semi-realistic anime, sunny late afternoon, warm light from the upper left, clean line art, soft painted shading, saturated but natural colours). No people, no text, no signs with letters, no ball.

### 1 — `house_01.png` (3:2)
> [پیشوند] A straight-on front elevation of a three-storey red-orange brick townhouse on a city street, drawn flat with no perspective, the whole building edge to edge inside the frame, the sidewalk line at the very bottom, a strip of sky above the roof cornice. Ground floor is a small cafe with a red fabric awning and bright shop windows. Above it exactly two rows of three large rectangular windows with pale stone frames, light blue-grey reflective glass, some with half-drawn curtains and a flower pot; generous brick wall between the windows. Drainpipe on one side, small satellite dish on the roof.

### 2 — `house_02.png`
> [پیشوند] Straight-on flat front elevation of a four-storey cream-coloured brick building, whole facade inside the frame, sidewalk line at the bottom, sky strip above the roof. Ground floor bakery with a green awning and a display window full of bread. Above it exactly three rows of three large rectangular windows with white frames and reflective glass, wide brick spacing between them, one window with laundry hanging, one with a cat on the sill.

### 3 — `house_03.png`
> [پیشوند] Straight-on flat front elevation of a four-storey dark red brick building, whole facade inside the frame, sidewalk at the bottom, sky above the roof. Ground floor flower shop with a blue awning and buckets of flowers in front. Above it exactly three rows of four large windows with light stone frames and reflective glass, generous brick between them, ivy climbing one corner.

### 4 — `house_04.png`
> [پیشوند] Straight-on flat front elevation of a five-storey yellow brick building, whole facade inside the frame, sidewalk at the bottom, sky above the roof. Ground floor bookshop with a dark green wooden front and a bay display window. Above it exactly four rows of three tall windows with white frames and reflective glass, wide spacing, window boxes with red geraniums on the second row.

### 5 — `house_05.png`
> [پیشوند] Straight-on flat front elevation of a five-storey grey stone and brick building, whole facade inside the frame, sidewalk at the bottom, sky above the roof. Ground floor pharmacy with a glowing green cross symbol (no letters) and a white awning. Above it exactly four rows of four large windows with stone frames and reflective glass, generous spacing, iron balcony rail on the top row.

### 6 — `house_06.png`
> [پیشوند] Straight-on flat front elevation of a five-storey chocolate-brown brick building, whole facade inside the frame, sidewalk at the bottom, sky above the roof. Ground floor ice-cream parlour with a pink-and-white striped awning. Above it exactly four rows of four large windows with cream frames and reflective glass; every window on the third row has a small wrought-iron balcony with potted plants; generous brick spacing.

### 7 — `house_07.png`
> [پیشوند] Straight-on flat front elevation of a six-storey red brick building with white plaster trim and a decorative cornice, whole facade inside the frame, sidewalk at the bottom, sky above the roof. Ground floor restaurant with a dark red awning and warm lit windows. Above it exactly five rows of four large arched-top windows with white frames and reflective glass, generous spacing, string lights along the first-floor ledge.

### 8 — `house_08.png`
> [پیشوند] Straight-on flat front elevation of a grand six-storey corner building in dark brick with white window frames and a copper-green mansard roof, whole facade inside the frame, sidewalk at the bottom, sky above the roof. Ground floor is a small boutique hotel entrance with a golden awning, brass lamps and a doorman's red carpet (no person). Above it exactly five rows of four tall windows with white frames and reflective glass, wide spacing, flags without emblems on the second floor.

### 9 — همسایه‌ها (3:2، دو بار: left / right)
> [پیشوند] Straight-on flat front elevation of a plain, slightly lower neighbouring brick building seen on a city street, muted colours, small dark windows, a closed roller shutter on the ground floor, no awning, whole facade inside the frame, sidewalk at the bottom, sky above the roof.

### 10 — `sky_panorama.png` (3:2)
> [پیشوند] A wide painted sky only, no ground and no buildings: warm late-afternoon blue gradient fading to pale gold near the bottom edge, a few soft cumulus clouds with sunlit edges, the sun just outside the frame on the upper left. Seamless from left edge to right edge.

### 11 — `ground_asphalt.png` (1:1)
> [پیشوند] A seamless tileable texture of grey city asphalt seen straight from above, subtle cracks and a few small pebbles, painted anime shading, no markings, repeats perfectly on all four edges.

### 12 — `ground_sidewalk.png` (1:1)
> [پیشوند] A seamless tileable texture of a city sidewalk seen straight from above, large warm-grey concrete paving slabs with thin joints, slight wear and a little moss in the gaps, painted anime shading, repeats perfectly on all four edges.

### 13 — `player_back_idle.png` (2:3، شفاف)
> [پیشوند] Full-body character seen directly from behind, standing relaxed with feet slightly apart: a mischievous 12-year-old boy, messy dark hair, red-and-white striped t-shirt, dark blue shorts, white sneakers, hands loosely at his sides. Transparent background, no ground shadow, the whole figure inside the frame with a little margin.

### 14 — ضربه (2:3، شفاف)
> [پیشوند + همان توصیفِ شخصیتِ ۱۳] Seen directly from behind, right leg swung far back and arms out for balance, about to kick a ball. Transparent background, no shadow, same proportions and colours as the previous image.

> [پیشوند + همان توصیفِ شخصیت] Seen directly from behind, right leg swung forward and up after kicking, body leaning back slightly, arms out. Transparent background, no shadow, same proportions and colours as the previous image.

### 15 — راه‌رفتن (2:3، شفاف)
> [پیشوند + همان توصیفِ شخصیت] Seen directly from behind, mid-stride walking away, left foot forward. Transparent background, no shadow.
> … second frame: right foot forward.

### 16 — `ball.png` (1:1)
> [پیشوند] An equirectangular (sphere-mapping) texture of a classic black-and-white hexagon football, slightly scuffed and dusty, painted anime shading, no background, fills the whole square.

### 17 — جزئیات (شفاف)
> [پیشوند] A cast-iron street lamp post seen from the side, transparent background.
> [پیشوند] A small sidewalk tree in a square iron grate, full green leaves, transparent background.
> [پیشوند] A green public rubbish bin, transparent background.
> [پیشوند] A small hatchback car parked, seen exactly from the side, pastel colour, transparent background.

## بعد از رسیدنِ تصویرها (کارِ من)

1. `Assets/Art/Raw/` → تنظیمِ import (sprite / texture، فشرده‌سازی)، بافت‌ها seamless چک می‌شوند.
2. برای هر نما: شبکه رویِ تصویر، مختصاتِ نرمالِ هر پنجره و خطِ بام → `HouseDefinition`؛ گزارشِ دسترسی (هیچ پنجره‌ای دست‌نیافتنی نباشد).
3. صحنه‌ی prototype با تصویرها ساخته می‌شود: صفحه‌ی نما، همسایه‌ها، زمین، skybox، اسپرایتِ بازیکن، توپ.
4. بیلد از UBA؛ تو تست می‌کنی و سبک/اندازه‌ها را با تصویر تطبیق می‌دهیم.
