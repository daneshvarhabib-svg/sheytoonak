# مشخصاتِ تصویرها (آرت) — مدينة الفاجعة

⚠️ **قاعده‌ی ثابت (۲۱ آبان، کاربر: «چیزهای سیاسی رو هرجا دیدی پاک کن»):** در هیچ تصویری پوسترِ چهره، پرتره‌ی شخصِ واقعی، شعار (مثلاً «الحرية»)، مشتِ گره‌کرده‌ی شعاری، پرچم، نشانِ کشور/حزب/سازمان یا نقاشیِ دیواریِ سیاسی نباشد. به هر پرامپتِ صحنه این را اضافه کن: `No political posters, no portraits on walls, no slogans, no flags, no emblems.`

⚠️ **به‌روزرسانیِ ۵ آبان ۱۴۰۵:** شهرِ این مرحله **بیروت** است. ساختمان‌های ۱ تا ۴ قبل از این تصمیم ساخته شده‌اند
(اروپایی‌اند) و فعلاً می‌مانند؛ خانه‌های ۵ تا ۸ و همه‌ی چیزهای تازه با حال‌وهوایِ بیروت ساخته می‌شوند و بعداً می‌شود
۱ تا ۴ را هم با همین پرامپت‌ها از نو ساخت. بازیکن دیده نمی‌شود (دیدِ اول‌شخص)؛ تصویرِ بازیکن لازم نیست.

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
3. نما **کاملاً از جلو** (الویشن، بدونِ پرسپکتیو)، لبه‌ی چپ و راستِ ساختمان داخلِ کادر، پایینِ کادر = خطِ پیاده‌رو، و **آجر تا لبه‌ی بالای کادر ادامه دارد: هیچ بام، کرنیش یا آسمانی در تصویر نیست** (ساختمان بوم ندارد؛ بالای بالاترین ردیفِ پنجره فقط دیوار).
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

## تصویرهای تازه‌ی ۵ آبان (بیروت، زنده‌بودنِ صحنه)

| نام فایل | نسبت | چیست |
|---|---|---|
| `wall_left.png`، `wall_right.png` | 3:2 | دیوارِ خانه‌های کناری **بدونِ پنجره** (کاربر: «دیوارهای بغل خالی نباشه، خونه باشه ولی فقط دیوارش») — آجر/سنگِ بیروتی، کولرِ گازی، لوله، کمی بوگنویلا، بدونِ پنجره‌ی واضح |
| `Props/tree_01.png`، `tree_02.png` | 2:3 شفاف | درختِ پیاده‌رو (فیکوس یا چنارِ بیروت)، کلِ درخت با تنه، پس‌زمینه‌ی شفاف |
| `Props/ped_01..04.png` | 2:3 شفاف | رهگذرِ بیروتی **از پهلو، رو به راست، در حالِ راه‌رفتن**: پیرمرد با طربوش و عبا، زنِ جوان با روسری و کیفِ خرید، مردِ جوان با پیراهنِ باز و سیگار، بچه‌مدرسه‌ای با کیف |
| `Props/car_01..03.png` | 3:1 (افقی) شفاف | ماشینِ قدیمیِ بیروتی **از پهلو، رو به راست**: مرسدسِ تاکسیِ کرم با نوارِ قرمز، فولکس‌واگنِ قدیمی، وانتِ کوچک |

### پرامپتِ دیوارهای کناری (دو بار، left و right)
```
Same style, flat straight-on elevation, landscape 3:2, wall continuing to the very top edge of the frame, sidewalk line at the very bottom. Subject: the blank side wall of an old Beirut apartment building: weathered sandstone and plaster, a few air-conditioning units, a drainpipe, faded paint, a little bougainvillea climbing one corner. No visible windows, no doors, no people, no text, no letters, no signs.
```
### پرامپتِ درخت
```
Same style. A single Beirut sidewalk tree (ficus), full tree with trunk and dense green leaves, seen from the side, isolated on a transparent background, no ground, no shadow. Portrait 2:3.
```
### پرامپتِ رهگذر (چهار بار، هر بار یک نفر)
```
Same style. Full-body Lebanese pedestrian seen exactly from the side, walking to the right, mid-stride: [an old man in a tarboush fez, grey moustache, loose white shirt and dark trousers, prayer beads in hand] / [a young woman in a headscarf and long coat carrying shopping bags] / [a young man in an open shirt and jeans, sunglasses] / [a schoolboy with a backpack]. Transparent background, no ground, no shadow, no text. Portrait 2:3.
```
### پرامپتِ ماشین (سه بار)
```
Same style. An old Beirut car seen exactly from the side, facing right: [a cream 1980s Mercedes taxi with a red stripe] / [a dusty old Volkswagen Beetle] / [a small white pickup truck with a few crates in the back]. Transparent background, no ground, no shadow, no text, no number plate characters. Landscape 3:1.
```

## قهرمان‌ها (۱۱ آبان ۱۴۰۵) — پنج قهرمانِ شروع

کاربر: «زاویه دوربین یه جوری باشه که ما یک سوم از صورت قهرمان رو ببینیم، کامل از پشت نباشه». قهرمان‌ها
اسپرایتِ دوبعدی‌اند، پس این زاویه در خودِ نقاشی است: **سه‌رخِ از پشت** — بدن رو به دیوار (دور از ما) و کمی به
راست چرخیده، سر به راست برگشته تا حدودِ یک‌سوم صورت (گونه، گوش، گوشه‌ی چشم، نوکِ بینی) دیده شود.
همه **رو به راست** کشیده می‌شوند؛ بازی برای تیمِ حریف (سمتِ راست) آینه‌شان می‌کند تا هر دو تیم رو به وسط باشند.

* هر قهرمان **دو تصویر**: `<id>_idle.png` (ایستاده) و `<id>_kick.png` (لحظه‌ی ضربه به توپ با پای راست).
* عمودی 2:3، **پس‌زمینه‌ی شفاف**، بدونِ سایه‌ی زمین، کلِ بدن داخلِ کادر با کمی حاشیه، **بدونِ توپ**.
* همه در **یک گفت‌وگوی ChatGPT**: اول `docs/reference/street-reference.png` و یکی از رهگذرها
  (`Assets/Resources/Art/Props/ped_01_tarboush.png`) را پیوست کن تا سبک همان شود؛ بعد از اولین قهرمان،
  تصویرِ همان را هم پیوست کن و بنویس «same scale and style as this one» تا قدها یکی بمانند.
* بفرست همین‌جا در چت؛ برش و جاگذاری در `Assets/Resources/Art/Heroes/` کارِ من است.

| شناسه (نامِ فایل) | قهرمان | ظاهر |
|---|---|---|
| `abu_shahata` | أبو شحّاطة البيروتي | پسرِ ۱۱ ساله‌ی تپل، سرِ تراشیده، رکابیِ سفید، شلوارکِ جیب‌دارِ گشاد، **دمپاییِ پلاستیکی (شحّاطه)** |
| `rima` | ريما الشرشوحة الصيداوية | دخترِ ۱۲ ساله‌ی شر، موهای فرِ مشکیِ ژولیده با کشِ شل، پیراهنِ فوتبالِ رنگ‌رفته‌ی گشاد، جینِ تا خورده، کتانیِ کهنه، چسب‌زخم روی آرنج |
| `zaatar` | زعتر الشامي | پسرِ ۱۱ ساله‌ی لاغر، موهای فرِ کوتاه، **گرمکنِ سبزِ زیتونی** با نوارِ سفید، کتانی |
| `fulla` | فُلّة الإسكندرانية | دخترِ ۱۰ ساله، دو بافتِ بلند با **گل‌های یاسِ سفید (فُل)**، پیراهنِ آبیِ دریایی رویِ ساپورت، کتانیِ سفید |
| `toni` | طوني الزعران البيروتي | پسرِ ۱۳ ساله‌ی لات، **کلاهِ برعکس**، رکابیِ مشکی، زنجیرِ نازکِ طلایی، جینِ پاره، کتانیِ قرمز |

### پرامپتِ ایستاده (پنج بار، هر بار یک قهرمان)
```
In exactly the style of the attached reference images (semi-realistic anime, sunny late afternoon, warm light from the upper left, clean line art, soft painted shading). Full-body character in a three-quarter BACK view: the body faces away from the viewer and is turned slightly to the right, the head turned to the right so that about one third of the face is visible (cheek, ear, the side of one eye, the tip of the nose); we never see the full face. Standing relaxed, ready, feet apart, looking at a building in front of them. Character: [DESCRIPTION]. Background: one flat solid pure green colour (#00FF00) filling the whole frame, no glow, no halo, no vignette, no gradient, no ground, no shadow. No ball, no text. Portrait 2:3, the whole figure inside the frame with a small margin.
```
### پرامپتِ ضربه (پنج بار) — لحظه‌ی «عقب‌کشیدنِ پا»
⚠️ دو بار شکست خورد. نسخه‌ی ۱ (توضیحِ دقیقِ ساق و مچ) مچ را چرخاند؛ کاربر: «زاویه پاش موقع زدن ضربه درست نیست،
انگار مچ پاش چرخیده، از زانو به پایین». نسخه‌ی ۲ را سیستمِ ایمنیِ ChatGPT رد کرد (بچه + جزئیاتِ بدن). نسخه‌ی ۳
(«شوتِ فوتبالی») پای اشتباه را بالا آورد و کاربر: «پای چپ داره شوت میزنه ولی جهت مچ پاش مثل پای راسته». پای
جلو‌رفته از پشت برای مدل سخت است؛ پس فریمِ ضربه لحظه‌ی **قبل از** ضربه است: پا عقب کشیده و زانو خم، که از پشت
طبیعی دیده می‌شود (کفِ کفش رو به ما درست است). خودِ ضربه را حرکتِ توپ نشان می‌دهد.
```
Same cartoon character, same style, same scale, same three-quarter back view as the previous image. Now the character is winding up to kick a football: standing on one foot, the other leg pulled back behind with the knee bent, so the bottom of that shoe faces the viewer; arms out for balance, leaning slightly forward toward the building. No ball. Flat solid pure green background (#00FF00), no glow, no shadow, no text. Portrait 2:3.
```
⚠️ پس‌زمینه: ChatGPT به‌جایِ «شفاف» یک هاله‌ی نارنجی رویِ مشکی کشید که دورِ بدن را نمی‌شود تمیز برید؛ سبزِ یک‌دست را
خودم شفاف می‌کنم (سفید نه، چون رکابیِ أبو شحّاطة سفید است).

جاگذاریِ [DESCRIPTION] (انگلیسی):
* abu_shahata: `a chubby 11-year-old Lebanese boy, shaved head, white tank top, baggy cargo shorts, worn plastic flip-flops`
* rima: `a scrappy 12-year-old Lebanese girl, wild curly black hair in a loose ponytail, an oversized faded football jersey, rolled-up jeans, scuffed sneakers, a plaster on one elbow`
* zaatar: `a young cartoon Syrian kid, short curly hair, an olive-green tracksuit with white stripes, sneakers` (⚠️ «skinny 11-year-old» را فیلترِ ChatGPT رد کرد: سنِ دقیق و صفتِ بدنی برای بچه ننویس)
* fulla (⚠️ چپ‌پا: در پرامپتِ ضربه «with the LEFT foot»): `a young cartoon girl from Alexandria, two long braids tied with small white jasmine flowers, a sea-blue dress over leggings, white sneakers`
* toni: `a cocky cartoon Beirut street teenager with a below-the-knee prosthetic leg (the leg on the RIGHT side of the picture when standing) (a sporty metal prosthesis with a red sneaker on it, clearly visible), baseball cap worn backwards, black sleeveless t-shirt, thin gold chain, ripped denim shorts, red sneakers` (⚠️ معلولیت عمدی است، کاربر: «همه سالم نباشن»)

### شیتِ ضربه‌ی پنج‌فریمی (کاربر، ۱۱ آبان: «تو یه پرامپت از خروجی چندتا تصویر بگیریم … پنج تا تصویر به شکل کمیک»)
گفت‌وگوی تازه، فقط تصویرِ ایستاده‌ی همان قهرمان پیوست. خروجی را `tools/art/slice_sheet.py <sheet> Assets/Resources/Art/Heroes/<id>_k 5` می‌بُرد
(فریم‌ها رویِ سر هم‌تراز). اولین نمونه: أبو شحّاطة، درست درآمد.
```
Use the attached character exactly (same face, clothes, colours, art style, three-quarter back view with about one third of the face visible, turned to the right). Make a sprite sheet: ONE horizontal row of 5 frames of this same character kicking a football toward a building in front of him, like an animation: 1) running toward the ball, 2) last step, planting the standing foot, 3) kicking leg pulled back with the knee bent, arms out, 4) kicking leg swung forward through the ball, foot pointing straight ahead toward the building, 5) landing and recovering balance. All 5 figures the same size and scale, feet on the same ground line, evenly spaced with clear empty space between them, nothing overlapping. No ball, no panel borders, no comic frames, no speech bubbles, no text, no numbers. Background: one flat solid pure green colour (#00FF00), no shadow. Landscape 3:2.
```
⚠️ **قاعده برای قهرمان‌های بعدی (کاربر، ۱۱ آبان):** «واسه قهرمانان بعدی از اول تو پرامپتشون همین شکلی یه تیکه چندتا فریم خروجی بگیریم» — هر قهرمانِ تازه: یک تصویرِ ایستاده + همین شیتِ پنج‌فریمی، از همان اول.
انتخابِ کاربر از شیت‌های پنج قهرمانِ شروع: طوني بی‌فریمِ ۱ (فریمِ ۵ جای دویدن)؛ فُلّة و ريما بی‌فریمِ ۴ (تصویرِ ضربه‌ی تکیِ قبلی جایش)؛ زعتر و أبو هر پنج.
(فُلّة چپ‌پاست: «kicking with the LEFT leg» به ۳ و ۴ اضافه شود؛ دخترها «her».)

## جمله‌های فحشِ صاحب‌خانه (برای ضبط)

کاربر: «هر بار شیشه می‌شکنه یه صدای فحشی از توی خونه به زبون عربی بیاد، ترجیحاً غیررکیک». هشت جمله‌ی شامی، تند ولی تمیز؛ هر کدام را یک نفر (ترجیحاً دو صدا: مرد و زن) با عصبانیت و کمی دور (از پشتِ پنجره) ضبط کند، ۱ تا ۲ ثانیه، فایلِ `.wav` یا `.mp3` یا `.ogg` (نه `.m4a`) با هر نامی در `Assets/Resources/Audio/Curses/`:

1. ولك شو عم تعمل؟!
2. يا حيوان! كسرتلي الشباك!
3. الله يقطعك يا ولد!
4. ولاك روح من هون!
5. يا مجنون! شو هالمصيبة؟!
6. الله ياخدك يا زلمة!
7. يلعن أبو هالشغلة!
8. بدّي خبّر أبوك، فهمت؟!

تا این‌ها نرسد، بازی یک غرغرِ ساخته‌شده پخش می‌کند تا زمان و بلندی‌اش روی گوشی سنجیده شود.

## پرامپت‌ها (انگلیسی؛ عیناً کپی کن)

پیشوندِ مشترک که اولِ همه‌ی پرامپت‌ها می‌آید:

> In exactly the style of the attached reference image (semi-realistic anime, sunny late afternoon, warm light from the upper left, clean line art, soft painted shading, saturated but natural colours). No people, no text, no signs with letters, no ball.

### 1 — `house_01.png` (3:2)
> [پیشوند] A straight-on front elevation of a three-storey red-orange brick townhouse on a city street, drawn flat with no perspective, the whole building edge to edge inside the frame, the sidewalk line at the very bottom, the brick wall continues to the very top edge of the frame with no roof, cornice or sky. Ground floor is a small cafe with a red fabric awning and bright shop windows. Above it exactly two rows of three large rectangular windows with pale stone frames, light blue-grey reflective glass, some with half-drawn curtains and a flower pot; generous brick wall between the windows. Drainpipe on one side.

### 2 — `house_02.png`
> [پیشوند] Straight-on flat front elevation of a four-storey cream-coloured brick building, whole facade inside the frame, sidewalk line at the bottom, brick continuing to the top edge of the frame, no roof or sky. Ground floor bakery with a green awning and a display window full of bread. Above it exactly three rows of three large rectangular windows with white frames and reflective glass, wide brick spacing between them, one window with laundry hanging, one with a cat on the sill.

### 3 — `house_03.png`
> [پیشوند] Straight-on flat front elevation of a four-storey dark red brick building, whole facade inside the frame, sidewalk at the bottom, brick continuing to the top edge of the frame, no roof or sky. Ground floor flower shop with a blue awning and buckets of flowers in front. Above it exactly three rows of four large windows with light stone frames and reflective glass, generous brick between them, ivy climbing one corner.

### 4 — `house_04.png`
> [پیشوند] Straight-on flat front elevation of a five-storey yellow brick building, whole facade inside the frame, sidewalk at the bottom, brick continuing to the top edge of the frame, no roof or sky. Ground floor bookshop with a dark green wooden front and a bay display window. Above it exactly four rows of three tall windows with white frames and reflective glass, wide spacing, window boxes with red geraniums on the second row.

### 5 — `house_05.png` (بیروت)
> [پیشوند] Straight-on flat front elevation of a five-storey old Beirut apartment building in warm sandstone with faded ochre plaster and wooden shutters, whole facade inside the frame, sidewalk at the bottom, brick continuing to the top edge of the frame, no roof or sky. Ground floor pharmacy with a glowing green cross symbol (no letters) and a white awning. Above it exactly four rows of four large windows with stone frames and reflective glass, generous spacing, iron balcony rail on the top row.

### 6 — `house_06.png` (بیروت)
> [پیشوند] Straight-on flat front elevation of a six-storey Beirut building in pale limestone with green wooden shutters, bougainvillea on one balcony, whole facade inside the frame, sidewalk at the bottom, brick continuing to the top edge of the frame, no roof or sky. Ground floor ice-cream parlour with a pink-and-white striped awning. Above it exactly four rows of four large windows with cream frames and reflective glass; every window on the third row has a small wrought-iron balcony with potted plants; generous brick spacing.

### 7 — `house_07.png` (بیروت)
> [پیشوند] Straight-on flat front elevation of a six-storey Ottoman-era Beirut mansion front in honey sandstone with white stone arches, whole facade inside the frame, sidewalk at the bottom, brick continuing to the top edge of the frame, no roof or sky. Ground floor restaurant with a dark red awning and warm lit windows. Above it exactly five rows of four large arched-top windows with white frames and reflective glass, generous spacing, string lights along the first-floor ledge.

### 8 — `house_08.png` (بیروت)
> [پیشوند] Straight-on flat front elevation of a grand seven-storey French-mandate Beirut building in cream stone with wrought-iron balconies and tall shuttered windows, whole facade inside the frame, sidewalk at the bottom, brick continuing to the top edge of the frame, no roof or sky. Ground floor is a small boutique hotel entrance with a golden awning, brass lamps and a doorman's red carpet (no person). Above it exactly five rows of four tall windows with white frames and reflective glass, wide spacing, plain flower boxes on the second floor (no flags).

### 9 — همسایه‌ها (3:2، دو بار: left / right)
> [پیشوند] Straight-on flat front elevation of a plain, slightly lower neighbouring brick building seen on a city street, muted colours, small dark windows, a closed roller shutter on the ground floor, no awning, whole facade inside the frame, sidewalk at the bottom, brick continuing to the top edge of the frame, no roof or sky.

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

### ⚠️ قهرمانِ چپ‌پا (کشفِ کاربر، ۱۴ آبان)
فُلّة و أم العربيات هر دو در تصویر با پای **راست** شوت زدند، با اینکه «چپ‌پا» توی پرامپت بود؛ پس چپ‌پا بودنشان دیده نمی‌شود.
برای قهرمانِ چپ‌پا در پرامپتِ ۵ فریم این جمله‌ها حتماً بیاید:
```
IMPORTANT: this character is LEFT-footed. The kicking leg in frames 3, 4 and 5 is the LEFT leg (the leg on the left side of the body as seen from behind); the RIGHT leg is the standing leg planted on the ground. Do not kick with the right leg.
```
و در تصویرِ ایستاده: `standing with weight on the right leg, left foot slightly forward, ready to kick with the LEFT foot`.


## دوچرخه (گرفته شد ۱۴ آبان: `Props/bike_frame`, `bike_wheel`, `rider_01_a/b`)

پرامپتی که استفاده شد:

```
Side-view sprite sheet on a flat pure green background (#00FF00), no shadow, no text, same painted style as the street.
1) an old black city bicycle facing RIGHT, drawn WITHOUT wheels (empty forks), 2) one bicycle wheel seen exactly from the side with visible spokes,
3) a young man in a white shirt sitting on that bicycle, drawn alone without the bicycle, in two pedalling poses (one knee up, the other knee up).
Clear green gaps between the four pictures, crisp edges.
```

## فضای واقعیِ بیروت و دمشقِ امروز (کاربر، ۱۶ آبان): «اینقدر همه چی گل و بلبل نباشه، یکمی چرک باشه، گرافیتی داشته باشه»

مرجعِ کاربر: دیوارِ بتنیِ بیروت با گرافیتیِ بزرگِ پرتره و تگ‌های رنگی (Rebel / Refund / ثورة)، و گرافیتیِ سیاه‌سفیدِ دمشق (موتور و آدم‌ها، تابلوی «حرية / FREEDOM»). **هیچ‌کدام از این عکس‌ها کپی نمی‌شوند؛ فقط حال‌وهوا.**

### قاعده‌ی تازه برای همه‌ی خانه‌ها (۳:۱، دو تصویر: سالم + همه‌شیشه‌شکسته)
به هر پرامپتِ خانه این بند اضافه می‌شود:
```
Present-day Beirut / Damascus street, NOT picturesque: the plaster is stained, cracked and patched, paint peeling in sheets, rust streaks under the balconies, a bundle of sagging electricity cables and a dangling wire across the facade, an old air-conditioner box dripping, a cracked satellite dish, bullet-pocked corner stone. The ground-floor wall carries street graffiti: one big colourful spray-paint mural (abstract shapes and a stylised face, no readable words) and several smaller tags and stencils in black, red and yellow; two faded torn paper flyers pasted over each other (no faces, no portraits, no slogans, no flags). Keep the windows and shop front clear of graffiti. Hand-painted illustration, muted dusty colours, late afternoon light.
```
و از هر پرامپت این‌ها حذف می‌شود: «geraniums», «bougainvillea», «jasmine», «potted plants», «bright noon light»، «cheerful».

### نسبتِ چرکی
- یک‌سوم خانه‌ها: گرافیتیِ بزرگ + تگ‌ها (مثلِ دیوارِ بتنیِ بیروت).
- یک‌سوم: فقط تگ‌های کوچک و پوسترِ پاره و کابل (معمولی‌تر).
- یک‌سوم: تمیزتر ولی با گچِ ریخته و کابل (تا تنوع بماند).

### پرامپتِ کاملِ نمونه (دمشق، ۳:۱)
```
Ultra-wide street-level view of a single present-day Damascus house facade, aspect ratio 3:1, 2172x724, hand-painted illustration, muted dusty colours, late afternoon light. The wall reaches both edges of the frame; no neighbouring buildings, no sky, no street in front; flat front view. The top 8 % of the image is plain wall; one row of large windows with wooden shutters evenly spread across the width, a barred ground-floor window, an iron door, a small grocer with a glass front on the right. The plaster is stained, cracked and patched, paint peeling, rust streaks under the sills, a bundle of sagging electricity cables across the facade, an old dripping air-conditioner box, a cracked satellite dish. The ground-floor wall carries street graffiti: a big black-and-white spray-paint mural of stylised figures (no readable words) and several small tags and stencils in red and black, two faded torn paper flyers (no faces, no portraits, no slogans). Windows and the shop glass stay clear of graffiti.
```
نسخه‌ی شکسته: همان + `identical composition, same framing and light, every window pane shattered`.

### زمینِ خیابان و پیاده‌رو (بازگرفتن)
```
Seamless tileable texture of a worn Beirut asphalt road, hand-painted: patched tar, cracks, a faded white lane line fragment, oil stain, dust. 1024x1024, flat even lighting.
```
```
Seamless tileable texture of a broken Beirut sidewalk, hand-painted: cracked concrete slabs, one slab missing showing earth, cigarette butts, a bottle cap, dust. 1024x1024, flat even lighting.
```

### آسمان (اختیاری، `Art/sky.png`، 2:1)
```
Hazy late-afternoon city sky over Beirut, hand-painted, pale dusty horizon to muted blue, thin smog band, a few soft clouds, two distant power lines crossing the top; no buildings, no sun disc. 1024x512.
```

## موسیقیِ متن (۱۶ آبان)
فایل‌ها در `Assets/Resources/Audio/Music/` (ogg؛ لوپ‌ها با crossfadeِ ffmpeg لوپ‌پذیر شده‌اند): `duel_loop` (شارع الأطفال، ۱۲۰ ثانیه)، `tension_loop` (شارع الجاهزة، ۲۶ ثانیه)، `sting_win`، `sting_lose`. `lobby_loop` (Morning in Beirut، ۹۰ ثانیه؛ نسخه‌ی اولِ لابی «زیادی غمگین» بود و رد شد). پرامپت‌ها انگلیسی، بی‌کلام؛ ⚠️ **مجوز (نقدِ ۱۷ آبان §۲.۷):** هر چهار قطعه با نسخه‌ی رایگانِ Suno ساخته شده = **فقط غیرتجاری، منتشر نشود**. پیش از اولین انتشار یا اشتراکِ پولی (با تأییدِ کاربر) یا جایگزین. وضعیتِ مجوز: `duel_loop` ❌، `tension_loop` ❌، `sting_win` ❌، `sting_lose` ❌، `lobby_loop` ❌. فحش‌ها: TTSِ رایگان، شرایطش هنوز بررسی نشده ❓.

- **۱۹ آبان: کلِ موسیقی عوض می‌شود.** حرفِ کاربر: «تمپوی شادی داره و نباید اینطوری باشه، باید بیشتر جنایی کارآگاهی باشه نه شاد و رقصی». حالِ تازه: نوآرِ شرقی، کند و مرموز، بدونِ دست‌زدن و ریتمِ رقص. پرامپت‌های تازه (Suno، بی‌کلام؛ همان نام‌فایل‌ها، کد عوض نمی‌شود):
  - `lobby_loop` (۹۰ ثانیه): *Instrumental, no vocals. Slow Middle Eastern film-noir theme, 72 BPM, seamless loop. Muted trumpet and smoky clarinet over a walking upright bass, brushed snare, sparse low oud plucks, distant rain and night street hum. Mysterious detective mood in a dark Beirut alley, minor key, tense but calm, no dancing rhythm, no handclaps.*
  - `duel_loop` (۱۲۰ ثانیه): *Instrumental, no vocals. Suspenseful crime-investigation underscore, 84 BPM, seamless loop. Pizzicato strings and a low plucked oud ostinato in a Hijaz minor mode, walking double bass, soft brushed drums, a muted trumpet answering in short phrases, ticking clock percussion. Sneaky, sly and tense like detectives following a suspect at night, not happy, not danceable, no darbuka grooves, no handclaps.*
  - `tension_loop` (۳۰ ثانیه): *Same crime theme, now urgent: 100 BPM, seamless 30-second loop. Tremolo low strings, pounding timpani heartbeat, nervous staccato oud, dissonant muted brass stabs, rising tension like the final minutes before an arrest. Dark, no melody resolution, no dance beat.*
  - `sting_win` (۴ ثانیه): *4-second sting: a sly noir resolution, muted trumpet phrase landing on a dark major chord, one soft cymbal swell. Case closed. No vocals.*
  - `sting_lose` (۴ ثانیه): *4-second sting: a descending minor clarinet line, a low piano cluster and one deep bass note fading out. The suspect got away. No vocals.*
- (رد شده ۱۹ آبان، شاد بود) دوئل: *Instrumental, no vocals. Playful Levantine street-game music, 110 BPM, seamless loop. Oud and buzuq riff over darbuka and riq, cheeky clarinet, off-beat handclaps. Mischievous kids-in-the-alley mood, Beirut/Damascus back street, slightly lo-fi.*
- تنش: *same theme but urgent: 135 BPM, driving darbuka and tabla rolls, fast buzuq tremolo, rising tension, 30-second loop.*
- برد/باخت: *4-second sting: triumphant oud and clarinet flourish with a darbuka hit* / *comic descending clarinet slide, a sad oud note, one darbuka thud.*

## صدای قهرمان‌ها و ابزارها (۱۷ آبان)
`HeroVoice` این فایل‌ها را از Resources برمی‌دارد (ogg؛ هر چند تا، نام با همان پیشوند، تصادفی و هرگز دو بار پشتِ هم یکی):

| مسیر | کِی |
|---|---|
| `Audio/Heroes/<id>/kick_01.ogg …` | لحظه‌ی شوتِ همان قهرمان |
| `Audio/Fx/poison_01.ogg …` | گذاشتنِ سم (هر دو طرف) |
| `Audio/Fx/antidote_01.ogg …` | پادزهر (هر دو طرف) |

شناسه‌ها: `abu_shahata` أبو شحّاطة (بیروتی)، `zaatar` زعتر (شامی)، `fulla` فُلّة (اسکندرانی)، `rima` ريما الشرشوحة (صیداوی)، `toni` طوني الزعران (بیروتی)، `um_arabiyat` أم العربيات (بغدادی)، `hamada` حمادة الطرشجي (مصری)، `bubakr` بوبكر الصيّاد (تونسی).

خطِ شوتِ هر قهرمان (۱۷ آبان؛ ۳ خط، هر خط یک فایل `kick_01..03`؛ گوینده‌ی TTS به لهجه‌ی خودش، بعد همان فیلترِ عصبانیِ فحش‌ها ولی ملایم‌تر: سرعت ۱٫۰۸، زیری ۰):

| id | گوینده‌ی پیشنهادی | خطِ ۱ | خطِ ۲ | خطِ ۳ |
|---|---|---|---|---|
| abu_shahata | مرد، میان‌سال، لبنانی، خشن | خود هالشحّاطة! | يا الله شوف! | ولك طير! |
| zaatar | مرد، جوان، سوری/شامی، شاد | بسم الله! | شوفوا هالضربة! | هيك بتنكسر! |
| fulla | زن، جوان، مصری، پرانرژی | خدها يا بابا! | أيوه كده! | يلّا يا شباك! |
| rima | زن، میان‌سال، لبنانیِ جنوب، بلند | يا ويلك! | هيّا هيّا! | هيدي من ريما! |
| toni | مرد، جوان، بیروتی، زورگو | ولك رووح! | هيدي إلك! | شو هالكفّ! |
| um_arabiyat | زن، مسن، عراقی، محکم | ها! | خذها يا ابني! | هسّه تشوف! |
| hamada | مرد، جوان، مصری، شوخ | يلّا بينا! | آه يا سلام! | خد عندك! |
| bubakr | مرد، میان‌سال، تونسی، خونسرد | برا! | هيّا هيّا! | باهي، خوذها! |

در سایتِ TTS: زبان را عربی با همان کشور بگذار (لبنان/سوریه/مصر/عراق/تونس)؛ علامتِ تعجب نگه دار؛ اگر گوینده‌ی آن کشور نبود، نزدیک‌ترین (عراقی → خلیجی، تونسی → مراکشی/الجزایری).

افکتِ سم و پادزهر (ElevenLabs Sound Effects رایگان، یا هر مولدِ افکت؛ ۱–۱٫۵ ثانیه، بی‌موسیقی):
- `poison_01`: *Short cartoon sound effect, 1.2 seconds: a glass bottle uncorks with a pop, thick liquid glugs out, then a sly muffled chuckle. Comic, playful, mono, no music.*
- `poison_02`: *Short cartoon sfx, 1 second: a mischievous "psst" whisper followed by a quick wet splat and a tiny bubbling fizz. Comic, no music.*
- `antidote_01`: *Short cartoon sound effect, 1.2 seconds: a bright magic puff with sparkle, then a relieved human sigh "aah". Warm, comic, mono, no music.*
- `antidote_02`: *Short cartoon sfx, 1 second: a quick broom sweep whoosh and a cheerful ding, like a problem being brushed away. No music.*

### تعدادِ پنجره: فرد، بین ۱۱ و ۲۷ (کاربر، ۱۷ آبان)
«تعداد پنجره‌های قابلِ شکستن در همه‌ی تصاویر فرد باشه؛ تعدادشون بیشتر باشه، حداقل ۱۱ و حداکثر ۲۷.» با جمعِ فرد، پنجره‌ی مهرشده دیگر لازم نیست (کد فقط برای خانه‌ی زوج کرکره می‌گذارد). ویترینِ مغازه یک پنجره حساب می‌شود (یک قابِ شیشه‌ایِ بزرگ، نه چند تکه).

چیدمان‌های پیشنهادی (ردیف از بالا؛ هر عدد = پنجره‌ی شیشه‌دارِ شمردنی):
| جمع | چیدمان |
|---|---|
| ۱۱ | ۴ + ۴ + (۲ پنجره + ۱ ویترین) |
| ۱۳ | ۵ + ۵ + (۲ + ۱ ویترین) |
| ۱۵ | ۵ + ۵ + ۵ |
| ۱۷ | ۶ + ۶ + (۴ + ۱ ویترین) |
| ۲۱ | ۷ + ۷ + ۷ |
| ۲۷ | ۹ + ۹ + ۹ |

پرامپتِ پایه (عددها را از جدول بگذار؛ ۳ ردیف تا در قابِ ۳:۱ پنجره‌ها ریز نشوند، هر ردیف هم‌تراز):
```
Ultra-wide street-level view of a single present-day Beirut house facade, aspect ratio 3:1, 2172x724, hand-painted illustration, muted dusty colours, late afternoon light. The wall reaches both edges of the frame; no neighbouring buildings, no sky, no street in front; flat front view. The top 6 % of the image is plain wall. EXACTLY three rows of windows, aligned in straight columns: top row EXACTLY 5 identical rectangular windows, each with its wooden shutters OPEN and folded back flat against the wall on both sides, the glass pane fully visible in every window; middle row EXACTLY 5 identical windows with small balconies, glass visible; ground floor EXACTLY 5 openings: 4 barred windows with glass behind the bars and 1 wide shop front with a single large glass pane, plus an iron door. Every window is fully visible, none hidden by balconies, awnings, cables or plants. No closed shutters, no blinds, no curtains covering the glass: every window shows its glass pane clearly. Count the windows: 15 in total. The plaster is stained, cracked and patched, paint peeling in sheets, rust streaks under the sills, a bundle of sagging electricity cables, an old air-conditioner box, a cracked satellite dish; street graffiti on the ground floor (abstract shapes, no readable words), two torn paper flyers (no faces, no portraits, no slogans). No people, no cars.
```
نسخه‌ی شکسته: همان + `identical composition, same framing and light, every window pane and the shop glass shattered with jagged holes, dark rooms behind`.

⚠️ «wooden shutters» تنها = کرکره‌ی بسته (اولین خروجی، ۱۷ آبان: پنج پنجره‌ی بالا بی‌شیشه)؛ همیشه «OPEN and folded back … glass pane fully visible».

⚠️ مدل‌های تصویر در شمردن ضعیف‌اند: اگر خروجی ۱۴ یا ۱۶ شد، پنجره‌ها را از رویِ خودِ تصویر می‌شمارم و اگر زوج بود یکی را در کد مهر می‌کنم (کرکره)، پس عددِ دقیق حیاتی نیست؛ ولی «EXACTLY n» و «Count the windows» کمک می‌کند.

## لوگو، آیکون و اسپلش (۱۷ آبان)
⚠️ مدل‌های تصویر حروفِ عربی را خراب می‌کنند. دو راه: (الف) با متن بگیر و اگر حروف درست بود همان؛ (ب) **تابلو را بی‌متن بگیر** (`logo_plate.png`) و خودِ بازی «مدينة الفاجعة» را با فونت رویش می‌نویسد. (ب) امن‌تر است؛ هر دو را بگیر.
فایل‌ها: `UI/logo.png` (لوگو با متن، رویِ سبزِ #00FF00 برای برش)، `UI/logo_plate.png` (همان بی‌متن)، `UI/app_icon.png` (۱:۱، ۱۰۲۴×۱۰۲۴، بی‌متن)، `UI/splash.png` (۱۶:۹، ۲۰۴۸×۱۱۵۲، افقی).

### لوگو (با متن)
```
Game logo for a mobile game, hand-painted illustration, on a flat solid bright green background #00FF00 for cut-out. A battered rectangular tin shop sign hanging from two rusty chains, cobalt blue enamel paint chipped and rusted at the edges, a few bullet pockmarks, one corner bent. On the sign, big bold Arabic hand-lettered title «مدينة الفاجعة» in cream white with a red drop shadow, letters slightly uneven like a street sign painter; one letter's paint dripping. A small worn leather street ball breaking through the sign's corner with radiating cracks. Warm dusty afternoon light, Beirut street feel. No other text, no watermark.
```
### تابلویِ بی‌متن
همان پرامپت، به‌جایِ جمله‌ی «On the sign, big bold Arabic …» بنویس: `The sign is blank, empty cream-white enamel panel in the middle, nothing written on it.`

### آیکونِ اپ (۱:۱، بی‌متن)
```
Mobile game app icon, 1:1, hand-painted, no text. Close-up of a single old wooden-shuttered window on a stained, peeling pink-plaster Beirut wall, the glass pane shattered in a jagged star with a worn leather street ball stuck in the middle, a few shards flying, a strip of blue sky reflected in the remaining glass. Bold, readable at small size, warm dusty colours, slight vignette. Fills the whole square, no border, no text, no watermark.
```
### اسپلش (۱۶:۹، افقی)
```
Splash screen for a mobile game, landscape 16:9, 2048x1152, hand-painted illustration, same gritty present-day Beirut/Damascus alley style as the game: cracked stained plaster facades, sagging electricity cables, graffiti, torn paper flyers (no faces, no portraits, no slogans, no flags). Low camera on the street at late afternoon. Three mischievous kids (a boy in a white vest and green shorts, a girl in a blue dress with braids, a teenager in a green tracksuit) seen from behind, mid-celebration, as a worn leather ball smashes through an upper window and glass explodes outward; an angry old neighbour leaning out of the next window shaking a fist. Empty calm space at the top-centre of the sky for the logo, and the lower 15 % slightly darker for a loading bar. Warm dusty light, muted colours, no text, no watermark.
```

## پس‌زمینه‌ی لابی و کارتِ قهرمان‌ها (۱۷ آبان)
کاربر: «صفحه بک گراند اولیه خیلی زشته … عکسی هم که هست عکس خونه نباشه. عکس قهرمان‌ها باشه در حال نبرد.» تا این دو نرسند بازی اسپلش را می‌گذارد.
فایل‌ها: `UI/home.png` (۱۶:۹، ۲۰۴۸×۱۱۵۲، افقی؛ کلِ صفحه‌ی لابی زیرِ دکمه‌ها -- کم‌کنتراست و تیره‌تر از اسپلش تا نوشته‌ها خوانا بمانند)، `UI/heroes_battle.png` (۱۶:۹، ۱۵۳۶×۸۶۴؛ کارتِ وسطِ لابی، پرجنب‌وجوش).

### `home.png`
```
Lobby background for a mobile game, landscape 16:9, 2048x1152, hand-painted illustration, gritty present-day Beirut/Damascus alley style: cracked stained plaster facades, sagging cables, graffiti, torn paper flyers (no faces, no portraits, no slogans, no flags), dusk light with a few lit windows. Wide shot of a street football "battle" between two gangs of kids: on the left three kids (a boy in a white vest and green shorts, a girl in a blue dress with braids, a teenager in a green tracksuit) and on the right three rival kids (a chubby boy in a red striped shirt, a tall girl in a yellow hijab and jeans, a small boy with a cap), facing each other across the street, a worn leather ball frozen mid-air between them, shattered glass glinting around a broken upper window, an old neighbour shouting from a balcony. Composition keeps the centre of the frame open and quiet (the UI sits there) with the figures toward the lower third and edges. Overall darker, low-contrast, slightly desaturated so white text reads on top. No text, no watermark.
```
### `heroes_battle.png`
```
Key art for a mobile game card, landscape 16:9, 1536x864, hand-painted, same gritty Beirut alley style. Dynamic close shot: a boy in a white vest and green shorts mid-kick, a worn leather ball exploding a ground-floor window into shards, a girl in a blue dress with braids laughing and pointing, a teenager in a green tracksuit raising a fist; across the street two rival kids ducking behind a parked old Mercedes, one holding a slingshot. Dust, flying glass, warm late-afternoon light, strong diagonal motion, faces visible and expressive (three-quarter view, not from behind). No text, no watermark.
```
⚠️ قهرمان‌ها هشت نفرند و نقاشی‌شان جداست (`Art/Heroes/`)؛ این دو تصویر فقط حال‌وهوا می‌دهند، لازم نیست عیناً همان هشت نفر باشند.

## پیرزنِ فحّاش، نسخه‌ی هم‌سبک با خانه‌ها (۱۸ آبان)
کاربر: «این پیرزنه اصلاً به آرت فعلی نمی‌خوره». کات‌اوتِ کارتونیِ قدیمی (`Poison/granny.png`) کنار گذاشته شد؛ تا تصویرِ تازه نرسد پیرزن کشیده نمی‌شود (صدای فحش می‌ماند). فایل: `Poison/granny2.png`، ۳:۴ عمودی، ~۹۰۰×۱۲۰۰، رویِ سبزِ #00FF00.
```
Cut-out character for a game, portrait 3:4, on a flat solid bright green background #00FF00, NO window frame, NO wall, NO background scene. Painted in the same realistic hand-painted style as a weathered Beirut facade painting (soft brushwork, muted dusty colours, natural light from above-left). A stout old Levantine grandmother seen from the waist up, leaning forward as if out of a window sill, one fist raised and shaking, mouth open mid-shout, furious eyes; floral headscarf, cardigan over a house dress, apron. Her forearms rest on an invisible sill at the bottom edge of the image (crop at the sill line). Slightly three-quarter view facing the viewer's left. No text, no speech bubble, no watermark.
```

## آمبولانس و عابرِ زخمی (۱۸ آبان)
کاربر: «وقتی شیشه می‌شکنه عابر پیاده زیر پنجره است یه لحظه شوکه شه و سرش خونی بشه و بعد آمبولانس بیاد ببردش». شوک، لکه‌ی خون و افتادن در کد است؛ فقط یک تصویر لازم است: `Props/ambulance.png` (نمای جانبی، رو به راست، ~۱۶۰۰×۹۰۰، رویِ سبزِ #00FF00). تا نرسد عابر فقط می‌افتد و محو می‌شود.
```
Cut-out for a game, side view, on a flat solid bright green background #00FF00, no ground, no shadow on the ground, no scene. Painted in the same realistic hand-painted style as a weathered Beirut street painting (soft brushwork, muted dusty colours). An old boxy Lebanese Red Cross style ambulance, white with a red stripe and a red crescent/cross, rear doors open toward the viewer's right end of the vehicle, facing right; two paramedics in white shirts lifting a wheeled stretcher into the back, a person lying on it fully covered by a white sheet, one shoe sticking out from under the sheet. Whole vehicle and stretcher visible, wheels fully included. No text, no watermark.
```

## ساکنانِ فحّاش، چند چهره (۱۸ آبان)
کاربر: «چندتا تصویر داشته باشیم از کسی که سرش رو از شیشه شکسته میاره بیرون و فحش میده. با شکستن هر چندتا پنجره بیاد بیرون». هر ۲ تا ۴ پنجره یکی بیرون می‌آید (نه هر پنجره) و صدای فحش فقط همان لحظه پخش می‌شود. فایل‌ها: `Poison/resident_01.png` … `resident_04.png` (۳:۴، ~۹۰۰×۱۲۰۰، رویِ سبزِ #00FF00؛ `granny2.png` از قبل هست). همان قالبِ پیرزن، فقط شخصیت عوض می‌شود -- جمله‌ی اول و آخرِ پرامپت ثابت بماند:
```
Cut-out character for a game, portrait 3:4, on a flat solid bright green background #00FF00, NO window frame, NO wall, NO background scene. Painted in the same realistic hand-painted style as a weathered Beirut facade painting (soft brushwork, muted dusty colours, natural light from above-left). <CHARACTER>, seen from the waist up, leaning forward as if out of a window sill, shouting furiously, forearms resting on an invisible sill at the bottom edge of the image (crop at the sill line). Slightly three-quarter view facing the viewer's left. No text, no speech bubble, no watermark.
```
جایِ `<CHARACTER>`:
- `A bald middle-aged Levantine man in a white sleeveless undershirt, thick moustache, hairy arms, a slipper raised in one hand`
- `A young Levantine woman in a pink bathrobe with a towel turban, face cream on her cheeks, pointing an accusing finger`
- `A skinny old man in pyjamas and a wool skullcap, round glasses crooked, waving a cane`
- `A big Levantine grandfather in a grey tracksuit top, white stubble, worry beads swinging from the raised fist`

## صدا: لحن، نه فقط لهجه (۱۸ آبان)
کاربر: «لهجه رو خوب در آورده ولی هیچ لحنی نداره … باید با یه AI دیگه که لحن و خشونت و فحش و هیجان رو می‌فهمه کار کنیم». TTSهایی که بازیگری می‌کنند و عربی را هم دارند (پیشنهاد، تست با یک خط):
1. **ElevenLabs، مدلِ v3** -- تگِ احساس داخلِ متن: `[shouting] [angry]`، `[laughs]`، `[whispers]`؛ عربی را پشتیبانی می‌کند، صدایِ آماده‌ی مرد/زنِ عربی دارد. نمونه‌ی ورودی: `[furious, shouting from a window] يا كلب! كسّرت الشبّاك!`. کنترلِ Stability را پایین (۳۰–۴۰) بگذار تا اغراق کند.
2. **Hume Octave** -- به‌جایِ تگ، توضیحِ صحنه می‌گیرد («an old Beirut woman screaming out of a window at kids, hoarse, outraged»)؛ برایِ فحش و هیجان خوب است.
3. **OpenAI gpt-4o-mini-tts** -- فیلدِ `instructions` («shout, Levantine street accent, enraged, fast»)؛ ارزان ولی عربی‌اش ضعیف‌تر.
قاعده‌ها همان قبلی: هر خط جدا، سکوتِ کوتاه اول و آخر، mp3/wav؛ من همان فیلترِ A را می‌زنم. برایِ قهرمان‌ها هم همین: ضربه = نفسِ کوتاه + فریادِ پیروزی، سم = خنده‌ی شیطانی، پادزهر = «هه!» راحت‌شده.

### آمبولانس، نسخه‌ی چندتکه (۱۸ آبان)
کاربر: «نباید تعداد بیشتری تصویر از آمبولانس و پرسنلش داشته باشی؟». تصویرِ یک‌تکه (`Props/ambulance.png`) فعلاً کار می‌کند؛ با این شش فایل صحنه مرحله‌ای می‌شود (می‌آید، می‌ایستد، در باز، برانکاردِ خالی تا عابر، برگشت با برانکاردِ پر، در بسته، می‌رود). همه رویِ سبزِ #00FF00، **همان آمبولانسِ تصویرِ اول و همان دو امدادگر، همه رو به چپ** (کابین سمتِ چپ):
- `ambulance_closed.png`: `The same old white Red Crescent van from the previous picture, exact same side view and angle facing left, all doors shut, nobody around it, no stretcher.`
- `ambulance_open.png`: `The same van, same view, rear doors wide open showing the empty interior, nobody around it, no stretcher.`
- `medics_empty_a.png` / `medics_empty_b.png`: `The same two paramedics (white shirts with red crescent, navy trousers) pushing an EMPTY wheeled stretcher to the left, side view, no vehicle. Frame A: front medic's left leg forward. Frame B: right leg forward.`
- `medics_loaded_a.png` / `medics_loaded_b.png`: همان دو فریم، `… pushing the wheeled stretcher with a person lying on it fully covered by a white sheet, one shoe showing …`

### فحش‌ها برای ElevenLabs v3، دانه‌دانه (۱۸ آبان)
نمونه‌ی «Ahmed» لحن را درآورد (کاربر). هر خط یک فایل؛ Stability ~۳۵، Similarity ~۷۵، Speed ۱٫۰. مرد = Ahmed؛ زن = یک صدای عربیِ زنِ میان‌سال/مسن (مثلاً «Sana»/«Laila» یا هر زنِ عربیِ کتابخانه). خروجی mp3 → `Audio/Curses/curse_NN` (من فیلتر می‌زنم).

| # | صدا | متن برایِ ElevenLabs (عیناً) |
|---|---|---|
| 1 | مرد | `[furious, shouting from an upstairs window] ولك شو عم تعمل؟! ها؟!` |
| 2 | مرد | `[enraged, yelling] يا حيوان! كسرتلي الشبّاك!` |
| 3 | زن | `[old woman, screaming hoarsely] يا قليل الأدب! يا قليل الأدب!` (۲۱ آبان: بی‌نامِ خدا) |
| 4 | مرد | `[angry, barking an order] ولاك روح من هون! يلّا!` |
| 5 | زن | `[shrieking, outraged] يا مجنون! شو هالمصيبة؟!` |
| 6 | مرد | `[furious, voice cracking] يا زفت! شو عملت؟!` (۲۱ آبان) |
| 7 | مرد | `[growling, then shouting] يا خراب بيتي! يا خراب بيتي!` (۲۱ آبان) |
| 8 | زن | `[threatening, sharp] بدّي خبّر أبوك، فهمت؟! بدّي خبّرو!` |
| 9 | زن | `[screaming at the top of her lungs] ولااااك! تعا لهون!` |
| 10 | مرد | `[exasperated, shouting] كل يوم نفس القصّة! كل يوم!` (۲۱ آبان) |

### خطِ قهرمان‌ها برای ElevenLabs v3 (هیجان‌زده)
Stability ~۴۰. هر خط یک فایل → `Audio/Heroes/<id>/kick_01..03`. مرد = Ahmed (یا یک مردِ جوانِ عربی برایِ جوان‌ها)، زن = صدای زنِ جوانِ عربی.

| id | صدا | ۱ | ۲ | ۳ |
|---|---|---|---|---|
| abu_shahata | مرد، خشن | `[gruff, excited shout] خود هالشحّاطة!` | `[excited] يا عيني شوف!` | `[shouting, triumphant] ولك طير!` |
| zaatar | مرد، جوان، شاد | `[cheerful, excited] يلّا يا شباب!` | `[excited, laughing] شوفوا هالضربة!` | `[triumphant shout] هيك بتنكسر!` |
| fulla | زن، جوان، پرانرژی | `[excited, playful] خدها يا بابا!` | `[cheering] أيوه كده!` | `[shouting, excited] يلّا يا شبّاك!` |
| rima | زن، میان‌سال، بلند | `[shouting, fierce] يا ويلك!` | `[excited, urging] هيّا هيّا!` | `[triumphant] هيدي من ريما!` |
| toni | مرد، جوان، زورگو | `[aggressive shout] ولك رووح!` | `[cocky, excited] هيدي إلك!` | `[laughing, shouting] شو هالكفّ!` |
| um_arabiyat | زن، مسن، محکم | `[sharp, excited] ها!` | `[fierce, shouting] خذها يا ابني!` | `[triumphant] هسّه تشوف!` |
| hamada | مرد، جوان، شوخ | `[excited, playful] يلّا بينا!` | `[laughing, delighted] آه يا سلام!` | `[shouting, excited] خد عندك!` |
| bubakr | مرد، میان‌سال، خونسرد | `[calm then sudden shout] برا!` | `[excited, urging] هيّا هيّا!` | `[satisfied, grinning] باهي، خوذها!` |

سم: `[sly, whispering then giggling] هههه… شوف هلّق!` (دو نسخه: مرد و زن) → `Audio/Fx/poison_0N`. پادزهر: `[relieved, laughing] هااا! ولا شي!` → `Audio/Fx/antidote_0N`.

### صدای عابرِ قربانی و آژیر (۱۸ آبان)
`Audio/Fx/victim_01..04` (ElevenLabs v3، هر خط یک فایل، Stability ~۳۵) و `Audio/Fx/siren_01` (ElevenLabs Sound Effects). `StreetLife.PlayFx` تصادفی و بی‌تکرارِ پشتِ هم پخش می‌کند.
| # | صدا | متن |
|---|---|---|
| victim_01 | مرد | `[sudden pained yelp, then dazed] آآخ! … راسي… راسي!` |
| victim_02 | زن | `[shrieks in pain and shock] آآآي! يا ويلي! شو صار؟!` |
| victim_03 | مرد مسن | `[groaning, winded, falling] أوووف… يا لطيف… وقعت…` |
| victim_04 | زن | `[screaming for help] ساعدوني! حدا يتّصل بالإسعاف!` |
| siren_01 | افکت | `Old Middle Eastern ambulance siren approaching on a narrow street, two-tone wail, slightly distorted horn, 4 seconds, no music.` |

## کبوترِ زده‌شده و ماشینِ جاروی شهرداری (۱۹ آبان)
کاربر: «وقتی توپ می‌خوره به پرنده باید یه صدایی داشته باشه … خونش ریخته بشه و بعدش ماشین تمیزکن شهرداری بیاد و جارو کنه و جنازه پرنده رو برداره». خون و صحنه در کد است (`DuelStage.PigeonCorpse`)؛ تا تصویر نرسد کبوتر فقط محو می‌شود.
- `Props/sweeper.png` (نمای جانبی، رو به چپ، رویِ سبزِ #00FF00): `Cut-out for a game, side view facing left, on a flat solid bright green background #00FF00, no ground, no shadow, no scene. Painted in the same realistic hand-painted style as a weathered Beirut street painting. A small old municipal street-sweeper truck, dusty orange and white with no emblem, flag or logo of any kind, an operator in an orange vest visible in the cab, a round rotating side brush under the front bumper, a water tank and a hopper behind. Whole vehicle visible, wheels included. No text, no watermark.`
- `Props/sweeper_brush.png` (اختیاری، برایِ چرخیدن): `Same style, on green #00FF00: only the round street-sweeper side brush seen from the side, a disc of stiff dark bristles with a metal hub, flat and centred, nothing else.`
- `Audio/Fx/pigeon_01..02` (ElevenLabs Sound Effects، ~۱ ثانیه): `A pigeon hit by a football mid-air: a sharp startled coo-squawk, a burst of wing flaps and a soft thud on asphalt, 1 second, no music.`

### بار روی سقفِ ماشین‌ها (لول‌های ۲ تا ۵ِ اتوبوس/کامیون/گاری، ۱۹ آبان)
هر لول یک تکه بار روی سقف می‌نشیند و پنجره‌ی طبقه‌ی بالا را می‌پوشاند. تا تصویر نرسیده جعبه‌ی ساده‌ی رنگی کشیده می‌شود. هر کدام جدا، روی سبزِ #00FF00، رو به دوربین، کامل در کادر:
- `Poison/cargo_0.png`: `Cut-out for a game, on a flat solid bright green background #00FF00, no ground, no shadow. Same realistic hand-painted style as a weathered Beirut street painting. A stack of three battered cardboard boxes tied with rope, seen from the front, slightly uneven. No text, no logos.`
- `Poison/cargo_1.png`: `Same style and background: two old leather suitcases strapped on top of each other, one red-brown and one faded blue, with stickers worn off, seen from the front. No text.`
- `Poison/cargo_2.png`: `Same style and background: a pile of stuffed jute sacks and a bundle wrapped in a patterned blanket, tied with rope, seen from the front. No text.`
- `Poison/cargo_3.png`: `Same style and background: a rolled striped mattress and a plastic chair tied on top of a wooden crate, seen from the front. No text.`

### فرشِ ریما (به‌جای همسایه‌ی پنجره‌باز، ۱۹ آبان)
- `Poison/carpet.png` (عمودی و بلند، حدود ۱ به ۴): `Cut-out for a game, on a flat solid bright green background #00FF00, no ground, no shadow, no wall, no rope. Same realistic hand-painted style as a weathered Beirut street painting. A long, narrow, old Persian-style rug hanging straight down, seen flat from the front, deep red with dark blue and cream patterns, worn and slightly faded, long fringe at the bottom end only, the top edge cut off by the top of the frame. Very tall and narrow, about 1:4. No text.`
- `Icons/carpet.png` (مربع): `Game icon, on a flat solid bright green background #00FF00: a small red patterned rug hanging over the edge of a flat roof, seen from the front, bold simple shapes, same painted style as the other icons, no text.`

### پادزهرِ فرش: «زقّ السجّادة» (به‌جایِ سنگ‌ریزه‌ی فُلّة، ۲۰ آبان)
- `Poison/rug_pusher_0.png` و `rug_pusher_1.png`: کسی روی لبه‌ی پشت‌بام، از پایین (کوچه) دیده می‌شود؛ ۰ = خم شده و دو دست روی فرش، ۱ = هل داده، دست‌ها جلو. روی سبزِ #00FF00.
- `Icons/rug_push.png`: آیکونِ مربع.

## جشنِ ارتقا: بادکنک، ربان، موزیک (۲۱ آبان)
کاربر: «وقتی ارتقاء میدی یه چهارتا بادکنک و ربان از بالا بیاد و یه موزیکی که نشون بده خوشحالیم». در کد هست (`UpgradeCelebration.cs`) با بادکنکِ ساده و جینگلِ ساختگی؛ با این فایل‌ها واقعی می‌شود. هر تصویر جدا، روی سبزِ #00FF00.
- `Art/UI/balloon_0..3.png` (چهار رنگ: قرمز، زرد، آبی، سبز): `Cut-out for a mobile game UI, on a flat solid bright green background #00FF00, no shadow, no scene. Hand-painted cartoon style matching a warm, weathered Middle-Eastern street game. One shiny party balloon, {red|yellow|blue|green}, slightly oval, a soft white highlight on the upper left, a small tied knot at the bottom, a short curly white string. Centered, whole balloon visible. No text, no logo, no watermark.`
- `Art/UI/ribbon_0..1.png`: `Same style, on green #00FF00: one long curly party streamer ribbon hanging vertically, glossy {gold|pink} paper, three loose spirals, centered. No text.`
- `Audio/Fx/levelup_01` (ElevenLabs Sound Effects یا Suno، ۲ ثانیه): `Short cheerful level-up fanfare for a mobile game, bright brass and a hand drum, a little Middle-Eastern flavour (oud pluck at the end), happy and triumphant, 2 seconds, no voice.`
