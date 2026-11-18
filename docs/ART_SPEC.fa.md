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

## قهرمانِ آزمایشیِ سبکِ کارتونی: أبو شحّاطة (۲۱ آبان، مرحله‌ی ۳ و ۴)
کاربر: «پرامپت قهرمان آزمایشی رو بده». هدف: یک قهرمان در سبکِ کارتونیِ اغراق‌شده با خطِ دورِ ضخیم (الهام از حس‌وحالِ بازیِ مرجع، نه کپیِ شخصیت یا نقاشیِ آن). اگر پسندیده شد، بقیه با همین پیشوند. فایل‌ها رویِ سبزِ #00FF00؛ من جدا می‌کنم.
- `Art/Heroes/abu_shahata_toon_idle.png`: ایستاده، سه‌رخِ پشت، رو به راست (همان زاویه‌ی فعلی).
- `Art/Heroes/abu_shahata_toon_front.png`: همان شخصیت از روبه‌رو (برایِ کارت و انتخاب).
- `Art/Heroes/abu_shahata_parts.png`: همان شخصیت تکه‌تکه برایِ اسکلتِ متحرک (مرحله‌ی ۴).

## بقیه‌ی قهرمان‌ها در سبکِ کارتونی (۲۰ آبان)
کاربر: «پرامپت بقیه قهرمانان رو بنویس». هر قهرمان دو تصویر: پشت با ژستِ خنثی (همان را برایِ عروسک تکه می‌کنم) + روبه‌رو. تصویرِ ابوشحاطه پیوست شود تا سبک و قد یکی بماند. ⚠️ لباسِ بلند تا وسطِ ساق، تا پا برایِ حرکت دیده شود؛ سنِ عددی برایِ بچه ننوشتم (فیلتر).

#### ريما (`rima`)
`Art/Heroes/rima_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Rima, a scrappy cartoon street girl from Saida, wild curly black hair in a loose ponytail, freckles, a sly grin, an oversized faded red football jersey, rolled-up blue jeans, scuffed white sneakers, a small plaster on one elbow. Full body, three-quarter BACK view, the whole body turned to the RIGHT: we see the back and the right third of the face. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/rima_front.png`
```
The same original cartoon character as the previous picture (Rima, a scrappy cartoon street girl from Saida, wild curly black hair in a loose ponytail, freckles, a sly grin, an oversized faded red football jersey, rolled-up blue jeans, scuffed white sneakers, a small plaster on one elbow), same bold caricature style with thick dark brown outlines and cel shading, now FRONT view, full body, a confident pose with one foot on a worn leather football, showing personality. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

#### زعتر (`zaatar`)
`Art/Heroes/zaatar_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Zaatar, a young cartoon Damascus street kid, short curly brown hair, big bright eyes, a cheerful grin, an olive-green tracksuit with two white stripes on the sleeves and legs, grey sneakers. Full body, three-quarter BACK view, the whole body turned to the RIGHT: we see the back and the right third of the face. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/zaatar_front.png`
```
The same original cartoon character as the previous picture (Zaatar, a young cartoon Damascus street kid, short curly brown hair, big bright eyes, a cheerful grin, an olive-green tracksuit with two white stripes on the sleeves and legs, grey sneakers), same bold caricature style with thick dark brown outlines and cel shading, now FRONT view, full body, a confident pose with one foot on a worn leather football, showing personality. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

#### فُلّة (`fulla`)
`Art/Heroes/fulla_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Fulla, a young cartoon girl from Alexandria, two long black braids tied with small white jasmine flowers, rosy cheeks, a determined smile, a sea-blue knee-length dress over dark leggings, white sneakers. Full body, three-quarter BACK view, the whole body turned to the RIGHT: we see the back and the right third of the face. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/fulla_front.png`
```
The same original cartoon character as the previous picture (Fulla, a young cartoon girl from Alexandria, two long black braids tied with small white jasmine flowers, rosy cheeks, a determined smile, a sea-blue knee-length dress over dark leggings, white sneakers), same bold caricature style with thick dark brown outlines and cel shading, now FRONT view, full body, a confident pose with one foot on a worn leather football, showing personality. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

#### طوني (`toni`)
`Art/Heroes/toni_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Toni, a cocky cartoon Beirut street teenager, a baseball cap worn backwards, a smug half-smile, black sleeveless t-shirt, a thin gold chain, ripped denim shorts, red sneakers, and a sporty metal below-the-knee prosthetic leg on his NEAR leg (the leg closer to the viewer) with a red sneaker on it, clearly visible. Full body, three-quarter BACK view, the whole body turned to the RIGHT: we see the back and the right third of the face. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/toni_front.png`
```
The same original cartoon character as the previous picture (Toni, a cocky cartoon Beirut street teenager, a baseball cap worn backwards, a smug half-smile, black sleeveless t-shirt, a thin gold chain, ripped denim shorts, red sneakers, and a sporty metal below-the-knee prosthetic leg on his NEAR leg (the leg closer to the viewer) with a red sneaker on it, clearly visible), same bold caricature style with thick dark brown outlines and cel shading, now FRONT view, full body, a confident pose with one foot on a worn leather football, showing personality. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

#### أم العربيات (`um_arabiyat`)
`Art/Heroes/um_arabiyat_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Um al-Arabiyat, a tough cartoon Baghdad grandmother, a dark red patterned headscarf with a fringed edge, a stern face with strong eyebrows, a long dark brown dress with sleeves rolled up, a red underskirt showing below the hem at mid-shin, brown leather sandals. Full body, three-quarter BACK view, the whole body turned to the RIGHT: we see the back and the right third of the face. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/um_arabiyat_front.png`
```
The same original cartoon character as the previous picture (Um al-Arabiyat, a tough cartoon Baghdad grandmother, a dark red patterned headscarf with a fringed edge, a stern face with strong eyebrows, a long dark brown dress with sleeves rolled up, a red underskirt showing below the hem at mid-shin, brown leather sandals), same bold caricature style with thick dark brown outlines and cel shading, now FRONT view, full body, a confident pose with one foot on a worn leather football, showing personality. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

#### حمادة (`hamada`)
`Art/Heroes/hamada_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Hamada, a playful cartoon young Cairo street vendor, short curly black hair, a small white knitted cap, a big mischievous grin, a grey striped galabiya that ends at mid-shin, a stained cream apron tied at the waist, brown leather slippers. Full body, three-quarter BACK view, the whole body turned to the RIGHT: we see the back and the right third of the face. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/hamada_front.png`
```
The same original cartoon character as the previous picture (Hamada, a playful cartoon young Cairo street vendor, short curly black hair, a small white knitted cap, a big mischievous grin, a grey striped galabiya that ends at mid-shin, a stained cream apron tied at the waist, brown leather slippers), same bold caricature style with thick dark brown outlines and cel shading, now FRONT view, full body, a confident pose with one foot on a worn leather football, showing personality. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

#### بوبكر (`bubakr`)
`Art/Heroes/bubakr_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Bubakr, a calm cartoon Tunisian fisherman, grey hair, a short white beard, a red chechia cap, a relaxed smile, a faded blue work jacket with sleeves rolled up, beige trousers rolled up to mid-shin, blue flip-flops. Full body, three-quarter BACK view, the whole body turned to the RIGHT: we see the back and the right third of the face. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/bubakr_front.png`
```
The same original cartoon character as the previous picture (Bubakr, a calm cartoon Tunisian fisherman, grey hair, a short white beard, a red chechia cap, a relaxed smile, a faded blue work jacket with sleeves rolled up, beige trousers rolled up to mid-shin, blue flip-flops), same bold caricature style with thick dark brown outlines and cel shading, now FRONT view, full body, a confident pose with one foot on a worn leather football, showing personality. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

## صفحه‌ی پیروزی/شکست و محیطِ فانتزی (۲۰ آبان)
کاربر: «این گرافیک پیروزی … زشته» و «دیزاین خونه ها هم با بازیکنا نمیخوره، همینطور ماشین و رهگذرها و آسمون و آسفالت و پیاده رو». هر تصویر جدا؛ تصویرِ پشتِ ابوشحاطه را برایِ سبک پیوست کن. ⚠️ خانه‌ها: تصویرِ فعلیِ همان خانه را هم پیوست کن -- نقشه‌ی پنجره‌ها (محلِ برخوردِ توپ) از رویِ همان اندازه‌گیری شده و نباید جابه‌جا شود. متنِ عربی را بازی رویِ نوار می‌نویسد، نه تصویر.
پرامپت‌ها عیناً در پیامِ چتِ همان روز آمده‌اند.

## سم‌ها، پادزهرها و ساکنانِ فحش‌دهنده -- کارتونی (۲۲ آبان)
کاربر: «هنوز چندتا چیز درست نشده و کارتونی نشدن … یکی زهرها و پادزهرهاست، یکی ساکنان خونه ها که فحش میدن». هر تصویر جدا، تصویرِ پشتِ ابوشحاطه پیوست برایِ سبک، رویِ سبزِ #00FF00. نام‌ها همان نامِ فعلی تا کد دست نخورد؛ اندازه/نسبت همان قبلی (فهرستِ پرامپت‌ها در پیامِ چتِ همان روز). ⚠️ `granny.png`ِ قدیمی استفاده نمی‌شود؛ فقط `granny2`.

## قهرمانِ نهم: شادي و کسوف (۲۵ آبان)
کاربر: «یه قهرمان دیگه بسازیم که قدرتش خسوفه و جلوی خورشید رو میگیره و چراغ قوه به کار میاد». زهر = کسوف (ماه جلوی خورشید، خیابان تاریک، ضربه پخش می‌شود)؛ پادزهرِ آن چراغ‌قوه‌ی بوبکر؛ پادزهرِ خودِ شادي تورِ ماهیگیری (کبوتر را می‌گیرد). تصویرِ ابوشحاطه برایِ سبک پیوست شود.

`Art/Heroes/shadi_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Shadi, a cheerful cartoon old man from Muscat, a neat white beard, round glasses, a colourful embroidered round cap, a white robe that ends at mid-shin, brown leather sandals. Full body, three-quarter BACK view, the whole body turned to the RIGHT: we see the back and the right third of the face. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/shadi_front.png`
```
The same original cartoon character as the previous picture (Shadi, a cheerful cartoon old man from Muscat, a neat white beard, round glasses, a colourful embroidered round cap, a white robe that ends at mid-shin, brown leather sandals), same bold caricature style with thick dark brown outlines and cel shading, now FRONT view, full body, a confident pose with one foot on a worn leather football, holding a small brass telescope, showing personality. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Poison/eclipse.png` (1:1، شفاف)
```
A cartoon solar eclipse for a mobile street game: a dark moon disc almost covering a bright yellow sun, a glowing orange-white ring and soft rays around it, a few small stars nearby. Same hand-painted cartoon style as the attached picture, thick dark outlines, warm colours. Single object, centered, on a flat solid bright green background #00FF00, no text, no logo.
```
`Art/Icons/eclipse.png` (1:1، شفاف)
```
A simple game icon of a solar eclipse: a dark moon covering most of a yellow sun with a thin glowing ring, bold cartoon style, thick dark outline, readable at small size. Centered on a flat solid bright green background #00FF00, no text.
```
`Art/Poison/net.png` (1:1، شفاف)
```
A hand-held fishing net on a wooden pole, round wooden hoop with a few orange cork floats, loose cream-coloured mesh sagging below the hoop, slightly worn and patched. Same hand-painted cartoon style as the other street props, thick dark outlines, warm colours. Single object, centered, on a flat solid bright green background #00FF00, no text, no logo.
```

## قهرمان‌ها از پشت، حداکثر یک‌سومِ صورت (۲۶ آبان)
کاربر: «گرافیک کار باید تغییر کنه و از پشت سر بازیکنا باشه و نهایتاً یک سوم صورتشون معلوم باشه. الآن نصفشون معلومه که خوب نیست». پرامپت‌های ۱۱ آبان «right third of the face» گفته بودند و تصویرها نصفِ صورت دادند. عبارتِ تازه چشم را صریحاً ممنوع می‌کند. اول یک قهرمان (زعتر) برای آزمون؛ اگر درست درآمد، بقیه با همین عبارت. عروسک (`<id>_rig/`) را من از تصویرِ تازه دوباره تکه می‌کنم.

`Art/Heroes/zaatar_idle.png`
```
Original cartoon character for a mobile street game, in exactly the same bold caricature style as the attached Abu Shahata picture: thick dark brown outlines, big head (about 1/4 of body height), chunky hands and feet, warm saturated colours with soft cel shading. Character: Zaatar, a young cartoon Damascus street kid, short curly brown hair, an olive-green tracksuit with two white stripes on the sleeves and legs, grey sneakers. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head and the curly hair fill most of the head, we see only the right ear, the edge of the right cheek and the tip of the nose, NO eyes and NO mouth visible. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. Same height and scale as the attached picture. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

## سبکِ تازه‌ی قهرمان‌ها: کاریکاتورِ نقاشی‌شده با شخصیتِ تک (۲۶ آبان)
کاربر (با اسکرین‌شاتِ پسرخوانده): «مهمترین ویژگی ش سبک تصویر سازی و جزییاتیه که هر کارکتر داره و منحصر به فرده. از فرم بدن تا قد و پوشش و جزییات صورت هر بازیکن». ⚠️ فقط ویژگی‌ها برداشته می‌شود، نه تصویر یا شخصیتی از آن بازی؛ تصویرِ پسرخوانده به هیچ پرامپتی پیوست نمی‌شود.
ویژگی‌هایی که از آن تصویر خوانده شد: تناسبِ تقریباً واقعی ولی اغراق‌شده (سر حدودِ یک‌هفتمِ قد، نه یک‌چهارم)، هر شخصیت سیلوئتِ خودش (درازِ لاغر، کوتاهِ چاق، قوزکرده)، صورتِ پرجزئیات (چین، ته‌ریش، بینی و ابروی خاص)، لباس با چین و درز و کمربند و کهنگی، خطِ دورِ نازکِ قهوه‌ای تیره و سایه‌زنیِ نقاشی‌شده، رنگِ گرمِ کمی خاموش. جای‌گزینِ سبکِ قبلی (سرِ بزرگ، دست‌وپای چاق، خطِ کلفت).
ترتیب: اول زعتر (پشت + روبه‌رو) بی‌پیوست؛ اگر خوب شد، همان تصویر مرجعِ سبکِ بقیه می‌شود.

`Art/Heroes/zaatar_idle.png`
```
Original character for a mobile street game, hand-painted European comic caricature style: realistic but exaggerated proportions, head about one seventh of the body height, long expressive limbs, thin dark brown line art, soft painterly shading with visible brush texture, warm slightly muted colours, detailed clothing with folds, seams and wear. Character: Zaatar, a skinny, wiry young boy from Damascus with a distinctive silhouette: narrow shoulders, long thin legs, big feet, short messy curly brown hair, an olive-green tracksuit worn thin at the knees with two white stripes on the sleeves and legs, scuffed grey sneakers with one lace untied. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head and the curly hair fill most of the head, we see only the right ear, the edge of the right cheek and the tip of the nose, NO eyes and NO mouth visible. Neutral rig pose: standing straight, legs slightly apart, BOTH feet pointing to the right, the NEAR leg (closer to the viewer) slightly in front of the FAR leg, arms hanging down a little away from the body, hands open and relaxed, elbows and knees straight, nothing covering the legs below the knee. On a flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```
`Art/Heroes/zaatar_front.png`
```
The same original character as the previous picture (Zaatar, a skinny, wiry young boy from Damascus, narrow shoulders, long thin legs, big feet, short messy curly brown hair, an olive-green tracksuit worn thin at the knees with two white stripes, scuffed grey sneakers with one lace untied), same hand-painted European comic caricature style, thin dark brown line art, painterly shading, now FRONT view, full body: a cheeky gap-toothed grin, thick eyebrows, a scraped knee showing through the worn tracksuit, one foot on a worn leather football, hands in his pockets. On a flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height. No text, no logo, no flag, no watermark, no political symbols.
```

## سبکِ سوم: ماسک و نشانه (۲۷ آبان)
کاربر (با خروجیِ زعترِ سبکِ دوم و چند فریم از یک ویدئوی هنری): «هنوز دوستش ندارم … هر کارکتر یه ماسک خاص رو صورتش داشته باشه و بدنش هم تا جای ممکن طبیعی باشه و نزدیک به انسان ولی پر از نشانه‌ها». ⚠️ فقط حال‌وهوا برداشته می‌شود (ماسکِ دست‌ساز، بدنِ واقعی، نشانه‌های نقاشی‌شده، مه)، نه آن ویدئو؛ پیوست نمی‌شود.
قاعده‌ها: نقاشیِ دیجیتالِ نزدیک به واقعی با بافتِ قلم‌مو؛ تناسبِ واقعیِ انسان؛ هر قهرمان **ماسکِ دست‌سازِ خودش** (مقوا، چوب، حلبی، پارچه) که به زهرش ربط دارد؛ پوست و لباس پر از **نشانه‌های ساختگیِ دست‌کشیده** (آبی، قرمز، اُخرایی) — ⚠️ نه حروف یا کلمه‌ی واقعی، نه نمادِ دینی یا سیاسی (قاعده‌ی پروژه)؛ همه **لباس‌پوش** (بچه‌ها و زن‌ها حتماً؛ بازارِ عرب و ردهٔ سنی). نمای پشت: موی سر و بندِ ماسک دیده شود، لبه‌ی ماسک حداکثر یک‌سوم.
- (۲۷ آبان، بعد از آزمونِ زعتر) کاربر: «هنوز کارتونیه. می‌خوام واقعی تر باشه و نزدیک به رئالیستی. اصراری هم ندارم لباس کامل امروزی تنش باشه، میتونه یه لباس عجیب تنش باشه». پرامپت حالا «عکسِ سینمایی/واقعی» می‌گوید، نشانه‌ها «لکه و خطِ رنگِ دستی، نامرتب» (نه دایره/ضربدر/مثلثِ تمیز که شبیهِ آیکونِ دکمه‌ها شد)، و لباس «تکه‌دوزیِ عجیب با پارچه و طناب».

## سبکِ نهایی: عکس‌واقعی با ماسک، قرون وسطا (۲۷ آبان)
کاربر: «عالی شد همین رو بریم … کلا قصه رو ببریم زمان قرون وسطا». ترتیبِ کاربر: قهرمان‌ها، خانه‌ها، آسمان و زمین، قدرت‌ها، همسایه‌ها، وسایلِ نقلیه و عابران. هر تصویرِ روبه‌رو با پیوستِ تصویرِ پشتِ همان قهرمان. چهره‌ی کوچک (`_face`) را خودم از تصویرِ روبه‌رو می‌بُرم.

### ۱. قهرمان‌ها
`Art/Heroes/abu_shahata_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. Strange hand-made patchwork clothing of coarse linen, wool and rope, worn, patched and dusty. Skin and clothes covered in messy hand-painted strokes and smudges of blue, red and ochre paint (random strokes only, no letters, no numbers, no clean shapes). Character: Abu Shahata, a lean man about 50 from Beirut, grey stubble on the neck, slippers of woven palm leaf, knee-length patched tunic over loose trousers tied at the calf. His mask: a hand-made mask of real grey pigeon feathers glued on thin wood, a short beak-like nose piece, tied with twine. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/abu_shahata_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same paint marks, same photorealistic style, now FRONT view, full body, standing relaxed, one hand holding a grey pigeon on his forearm. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/rima_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. Strange hand-made patchwork clothing of coarse linen, wool and rope, worn, patched and dusty. Skin and clothes covered in messy hand-painted strokes and smudges of blue, red and ochre paint (random strokes only, no letters, no numbers, no clean shapes). Character: Rima, a sharp-tongued woman about 35 from Sidon, fully clothed: a long-sleeved knee-length tunic over wide trousers tied at the ankle, a cloth wrapped over her hair. Her mask: a tiny wooden window shutter with two small slats, worn as a mask, tied with cloth straps. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/rima_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same paint marks, same photorealistic style, now FRONT view, full body, standing relaxed, hands on her hips, leaning forward as if scolding. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/fulla_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. Strange hand-made patchwork clothing of coarse linen, wool and rope, worn, patched and dusty. Skin and clothes covered in messy hand-painted strokes and smudges of blue, red and ochre paint (random strokes only, no letters, no numbers, no clean shapes). Character: Fulla, a slim young woman about 25 from Alexandria, fully clothed: a long-sleeved knee-length tunic over loose trousers, a sea-blue scarf over her hair whose ends fly in the wind. Her mask: a mask of stiff sail-cloth stretched on thin reeds, with rope edges. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/fulla_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same paint marks, same photorealistic style, now FRONT view, full body, standing relaxed, one arm raised as if calling the wind, scarf ends blowing. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/toni_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. Strange hand-made patchwork clothing of coarse linen, wool and rope, worn, patched and dusty. Skin and clothes covered in messy hand-painted strokes and smudges of blue, red and ochre paint (random strokes only, no letters, no numbers, no clean shapes). Character: Toni, a stocky young street tough about 28 from Beirut, short black hair, thick neck, sleeveless leather jerkin over a patched shirt, rolled trousers, bare dusty feet. His mask: a cracked clay bowl turned into a mask, a little muddy water dripping from its rim. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/toni_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same paint marks, same photorealistic style, now FRONT view, full body, standing relaxed, arms crossed, chin up, standing in a small muddy puddle. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/um_arabiyat_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. Strange hand-made patchwork clothing of coarse linen, wool and rope, worn, patched and dusty. Skin and clothes covered in messy hand-painted strokes and smudges of blue, red and ochre paint (random strokes only, no letters, no numbers, no clean shapes). Character: Um al-Arabiyat, a big strong woman about 55 from Baghdad, fully clothed: a heavy long-sleeved knee-length coat over wide trousers, thick forearms, a wide rope belt. Her mask: a mask woven from straw and hay, like a piece of a hay cart. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/um_arabiyat_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same paint marks, same photorealistic style, now FRONT view, full body, standing relaxed, both palms forward as if about to shove a heavy cart. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/hamada_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. Strange hand-made patchwork clothing of coarse linen, wool and rope, worn, patched and dusty. Skin and clothes covered in messy hand-painted strokes and smudges of blue, red and ochre paint (random strokes only, no letters, no numbers, no clean shapes). Character: Hamada, a round cheerful man about 40 from Cairo, a pickle seller, an apron stained with brine over a knee-length tunic, sleeves rolled up. His mask: a mask cut from the wooden lid of a pickle barrel, with a brass ring. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/hamada_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same paint marks, same photorealistic style, now FRONT view, full body, standing relaxed, holding a small clay jar of pickles. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/bubakr_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. Strange hand-made patchwork clothing of coarse linen, wool and rope, worn, patched and dusty. Skin and clothes covered in messy hand-painted strokes and smudges of blue, red and ochre paint (random strokes only, no letters, no numbers, no clean shapes). Character: Bubakr, a weathered fisherman about 45 from Tunis, salt-white beard edge, a fishing net knotted over his shoulders like a shawl, knee-length tunic, rope sandals. His mask: a mask of dried fish skin stretched on a wooden frame. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/bubakr_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same paint marks, same photorealistic style, now FRONT view, full body, standing relaxed, holding a wet fishing net, raindrops on his shoulders. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/shadi_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. Strange hand-made patchwork clothing of coarse linen, wool and rope, worn, patched and dusty. Skin and clothes covered in messy hand-painted strokes and smudges of blue, red and ochre paint (random strokes only, no letters, no numbers, no clean shapes). Character: Shadi, a tall calm man about 35 from Oman, a long-sleeved knee-length robe with a sash, wide trousers, a wrapped cloth turban (plain, no symbols). His mask: a round mask of dark hammered iron with a thin bright ring around its edge, like the sun hidden by the moon. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/shadi_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same paint marks, same photorealistic style, now FRONT view, full body, standing relaxed, holding a small iron lantern, its flame lit. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
### ۲. خانه‌ها
`Art/house_36.png`
```
A straight-on flat elevation of ONE wide old town house facade, three storeys of honey-coloured limestone, a little wooden balcony on the second floor. Exactly 17 windows in three rows (6, 6, 5), each a small arched window of leaded panes of thick old greenish glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_36_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/house_37.png`
```
A straight-on flat elevation of ONE wide old town house facade, plastered mud-brick, peeling, with old wooden beams sticking out. Exactly 17 windows in three rows (6, 6, 5), each a small arched window of leaded panes of thick old greenish glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_37_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/house_38.png`
```
A straight-on flat elevation of ONE wide old town house facade, striped black and white stone courses, a heavy wooden door at street level. Exactly 17 windows in three rows (6, 6, 5), each a small arched window of leaded panes of thick old greenish glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_38_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/house_39.png`
```
A straight-on flat elevation of ONE wide old town house facade, rough grey stone with a projecting wooden latticed bay window on the top floor. Exactly 17 windows in three rows (6, 6, 5), each a small arched window of leaded panes of thick old greenish glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_39_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/house_40.png`
```
A straight-on flat elevation of ONE wide old town house facade, whitewashed walls, blue-painted wooden shutters, vines climbing from the ground. Exactly 17 windows in three rows (6, 6, 5), each a small arched window of leaded panes of thick old greenish glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_40_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/wall_left.png`
```
A straight-on flat view of a plain old town wall of honey-coloured stone blocks, a few cracks and weeds, no windows, no door, camera perpendicular, the wall fills the whole image. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. 3:2 image.
```
`Art/wall_right.png`
```
The same kind of old town wall as the attached picture, a different stretch of it with an old wooden gate shut on the right side, camera perpendicular, the wall fills the whole image. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. 3:2 image.
```
### ۳. آسمان و زمین
`Art/sky.png`
```
A wide panoramic sky over a medieval desert town, warm late-afternoon light, soft scattered clouds, a few birds far away, no buildings, no ground, no sun disc in the frame, smooth gradient from pale blue at the top to warm haze at the bottom. Photorealistic. Wide 2:1 image, no text.
```
`Art/ground_asphalt.png`
```
Seamless tileable top-down photo texture of an old street paved with worn rounded cobblestones and packed dust between them, straight overhead view, even soft light, no shadows of objects, no people, no lines. Square image.
```
`Art/ground_sidewalk.png`
```
Seamless tileable top-down photo texture of large worn flat limestone paving slabs with thin dusty joints, straight overhead view, even soft light, no objects. Square image.
```
### ۴. قدرت‌ها (زهر و پادزهر)
هر کدام: تصویرِ صحنه + آیکنِ مربع. وسیله‌ی چرخ‌دار: بدنه **بی‌چرخ** و چرخ جدا، تا چرخ بچرخد.
`Art/Poison/pigeon_0.png`
```
A grey city pigeon flying, wings fully up, seen from the side facing right. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/pigeon.png`
```
A grey pigeon flying, side view. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/pigeon_1.png`
```
The same grey pigeon as the attached picture, same size and angle, wings halfway down. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/pigeon_2.png`
```
The same grey pigeon as the attached picture, same size and angle, wings fully down. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/bus.png`
```
A long camel caravan seen exactly from the side facing left: three loaded camels in a row, tied together, bundles and rugs on their backs, one camel driver walking in front holding the rope. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/bus.png`
```
A loaded camel, side view. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/truck.png`
```
A tall overloaded hay wagon seen exactly from the side facing left, hay piled very high, pulled by one ox, WITHOUT its wheels (empty axles). Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/truck.png`
```
A tall overloaded hay wagon, side view. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/truck_wheel.png`
```
One big wooden cart wheel with spokes and an iron rim, seen straight from the side. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/cart.png`
```
A wooden street vendor's handcart seen from the side facing left, piled with pickled vegetables in clay jars, WITHOUT its wheel. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/cart.png`
```
A vendor's handcart full of clay jars. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/cart_wheel.png`
```
One small wooden handcart wheel with spokes, seen straight from the side. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/tow_truck.png`
```
Two medieval market guards walking to the left, side view, leather jerkins and wooden clubs, one carries a heavy iron hook on a rope, fully clothed, no face covering. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/nails.png`
```
An iron hook on a rope with a wooden club. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/tanker_0.png`
```
A water carrier walking to the left, side view, a big goatskin water bag on his back with its spout closed, a brass cup on his belt. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/plank.png`
```
A goatskin water bag with a brass cup. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/tanker_1.png`
```
The same water carrier as the attached picture, same size and angle, standing still and bending forward, pouring water from the goatskin spout to the ground. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/umbrella.png`
```
A square canopy of faded striped cloth on four thin wooden poles, seen from the front, slightly from above. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/umbrella.png`
```
A small striped cloth canopy on poles. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/flashlight.png`
```
A small hand lantern of black iron and thick glass, a candle flame lit inside, a ring handle on top. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/flashlight.png`
```
A small lit iron lantern. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/net.png`
```
A wet fishing net of brown rope knots spread wide, with cork floats on its edge, seen from the front. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/net.png`
```
A bundle of fishing net with cork floats. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/vane.png`
```
An old wooden weather vane on a rooftop pole, a rooster cut from wood on top, arrow pointing right. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/vane.png`
```
A wooden rooster weather vane. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/carpet.png`
```
A thick old hand-woven rug with faded red and blue patterns (geometric only), hanging over a rooftop edge, seen from the front. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/carpet.png`
```
A rolled hand-woven rug. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/rug_pusher_0.png`
```
A strong woman fully clothed in a long-sleeved knee-length tunic and trousers, side view facing left, leaning forward with both palms on something in front of her. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/rug_push.png`
```
Two hands pushing a rolled rug. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/rug_pusher_1.png`
```
The same woman as the attached picture, same size and angle, both arms fully stretched, shoving hard. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/puddle.png`
```
A shallow muddy puddle of brown water on cobblestones, seen at a low angle from the side, irregular edge, a few ripples. Isolated, only the puddle. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/puddle.png`
```
A muddy puddle, seen from above at an angle. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Poison/plank.png`
```
A rough old wooden plank, cracked, two iron nails sticking out of one end, seen from above at an angle. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/rain.png`
```
Heavy rain streaks falling, with splashes, nothing else, light streaks only. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Icons/rain.png`
```
A dark rain cloud with falling rain. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
آیکن‌های بدونِ صحنه (فرمان‌ها):
`Art/Icons/wind.png`
```
Swirling dust and a few dry leaves blown by a strong wind. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Icons/fart.png`
```
A small cloud of green-brown smelly gas, comic but realistic texture. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Icons/complaint.png`
```
Two big rough hands pushing forward, palms out. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Icons/pebble.png`
```
Three small round river pebbles. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
`Art/Icons/open_window.png`
```
A small arched wooden window with its shutters thrown open. Photorealistic, simple clear silhouette, isolated on a flat solid bright green background #00FF00, centered, fills 80% of a square image. No text.
```
### ۵. همسایه‌ها
`Art/Poison/window_open_0.png`
```
A small arched window of an old stone house with its two wooden shutters closed, seen straight-on, only the window and its stone frame. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/window_open_1.png`
```
The same window as the attached picture, same size and angle, both wooden shutters thrown wide open, the inside dark and empty. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/granny2.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: an old woman, a cloth wrapped over her grey hair, shaking her fist. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_01.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a bald old man with a thick grey moustache, shouting, pointing down. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_02.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a middle-aged woman, a cloth over her hair, holding a wooden ladle, furious. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_03.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a young man with a messy beard, half asleep, angry, yelling. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_04.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a heavy man holding a clay water jug as if about to throw it. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_05.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a thin old man in a felt cap, shaking a walking stick. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_06.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a young woman, a cloth over her hair, holding a crying swaddled baby, shouting. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
### ۶. وسایلِ نقلیه و عابران
آمبولانس ← دو نفر با برانکارِ چوبی؛ رفتگرِ ماشینی ← رفتگر با جارو و گاریِ دستی. هر عابر یک ردیف ۵ فریمِ راه‌رفتن در یک تصویر؛ من می‌بُرم.
`Art/Props/medics_empty.png`
```
Two medieval stretcher bearers walking to the left, side view, fully clothed in linen tunics and trousers, carrying an empty wooden stretcher between them. A horizontal sprite sheet of 5 walk-cycle frames in one row, same size and spacing, evenly spaced. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/medics_loaded.png`
```
The same two stretcher bearers as the attached picture, same size, now carrying a person lying on the stretcher completely covered by a plain white sheet. A horizontal sprite sheet of 5 walk-cycle frames in one row, same spacing. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/sweeper.png`
```
A street sweeper pushing a small wooden handcart to the left, side view, a long broom of palm fronds in the cart, fully clothed in a patched tunic and trousers, WITHOUT the cart's wheel. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/sweeper_wheel.png`
```
One small wooden handcart wheel with spokes, seen straight from the side. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/donkey_cart.png`
```
A donkey pulling a small two-wheeled wooden cart loaded with sacks, walking to the left, side view, a boy walking beside it, WITHOUT the cart's wheels. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/donkey_cart_wheel.png`
```
One wooden cart wheel with spokes, seen straight from the side. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/ped_01_merchant.png`
```
A pedestrian walking to the right, exact side view: a merchant in a striped robe and a felt cap carrying a cloth bundle. A horizontal sprite sheet of 5 walk-cycle frames in one row, same size and spacing, feet on the same baseline. Photorealistic, soft daylight, medieval Middle-Eastern era. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Props/ped_02_shopper.png`
```
A pedestrian walking to the right, exact side view: a woman fully clothed, a cloth over her hair, carrying a woven basket of bread. A horizontal sprite sheet of 5 walk-cycle frames in one row, same size and spacing, feet on the same baseline. Photorealistic, soft daylight, medieval Middle-Eastern era. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Props/ped_03_oldman.png`
```
A pedestrian walking to the right, exact side view: a bent old man with a walking stick and a long grey beard. A horizontal sprite sheet of 5 walk-cycle frames in one row, same size and spacing, feet on the same baseline. Photorealistic, soft daylight, medieval Middle-Eastern era. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Props/ped_04_hurried.png`
```
A pedestrian walking to the right, exact side view: a young man hurrying with a clay jar on his shoulder. A horizontal sprite sheet of 5 walk-cycle frames in one row, same size and spacing, feet on the same baseline. Photorealistic, soft daylight, medieval Middle-Eastern era. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Props/ped_05_crutches.png`
```
A pedestrian walking to the right, exact side view: a man with a bandaged leg walking on two wooden crutches. A horizontal sprite sheet of 5 walk-cycle frames in one row, same size and spacing, feet on the same baseline. Photorealistic, soft daylight, medieval Middle-Eastern era. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Props/ped_06_boy.png`
```
A pedestrian walking to the right, exact side view: a boy in a patched tunic running, holding a rag ball. A horizontal sprite sheet of 5 walk-cycle frames in one row, same size and spacing, feet on the same baseline. Photorealistic, soft daylight, medieval Middle-Eastern era. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```

### قهرمان‌های ۶ تا ۸ با رنگِ خودشان (۲۷ آبان)
کاربر (با ۵ قهرمانِ اول): «رنگ و لعاب بازیکنا دارن همه شبیه هم میشن. تا ام العربیات رو بگیر و مابقی رو رنگ و لعابشون رو تغییر بده». ۵ تای اول نصب شدند؛ این سه جای پرامپت‌های بالا را می‌گیرند: هر کدام رنگِ لباسِ خودش و نشانه‌ی غیرِ رنگیِ خودش.

`Art/Heroes/hamada_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. ⚠ His look must NOT be beige patchwork and NOT have blue/red/ochre paint: his clothes are clean, bright and solid-coloured. Character: Hamada, a round cheerful man about 40 from Cairo, a pickle seller: a saffron-yellow knee-length tunic, a short bright green vest over it, loose white trousers tied at the calf, a white apron stained with pale brine, red leather slippers. Marks: splashes of white lime-wash and drops of green pickle juice on his arms and clothes. His mask: a round mask cut from the polished wooden lid of a pickle barrel, with a shiny brass ring as a handle on the forehead. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/hamada_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same colours, same marks, same photorealistic style, now FRONT view, full body, standing relaxed, holding a small clay jar of pickles, smiling under the mask. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/bubakr_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. ⚠ His look must NOT be beige patchwork and NOT have blue/red/ochre paint: he is dressed in sea colours. Character: Bubakr, a weathered fisherman about 45 from Tunis, salt-white beard edge, a deep teal-green knee-length tunic darkened by sea water, a dark grey fishing net knotted over his shoulders like a shawl, short black trousers, rope sandals, his skin wet and shiny. Marks: crusts of dried white sea salt and a few silver fish scales stuck on his arms and legs. His mask: a mask of dried silver-grey fish skin stretched on a driftwood frame, with a small shell on the cheek. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/bubakr_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same colours, same marks, same photorealistic style, now FRONT view, full body, standing relaxed, holding a wet fishing net, raindrops on his shoulders. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/shadi_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, set in a medieval Middle-Eastern walled town. ⚠ His look must NOT be beige patchwork and NOT have blue/red/ochre paint: he is dark and elegant. Character: Shadi, a tall calm man about 35 from Oman, a long-sleeved knee-length robe of deep indigo almost black, a wide sash of dull gold cloth, wide dark grey trousers, a plain wrapped dark cloth turban (no symbols), black leather boots. Marks: thin silver-white painted lines and dots on his hands and neck, like stars. His mask: a round mask of dark hammered iron with a thin bright golden ring around its edge, like the sun hidden by the moon. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head, the hair and the mask's straps fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee). Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/shadi_front.png`
```
The same person as the attached back-view picture, same mask, same clothes, same colours, same marks, same photorealistic style, now FRONT view, full body, standing relaxed, holding a small iron lantern, its flame lit. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```

### حماده، بوبکر، شادی: تنوعِ بدن و جنس (۲۷ آبان، جای پرامپت‌های قبلی)
کاربر: «مثلاً چاق و لاغر کن یا کوتاه و بلند یا موی متفاوت یا رنگ لباس متفاوت یا لباس با بافت برگ سبز یا یه ساک یه دوشی که توش پر از انگوره … تنوع رو زیاد کن و توی قهرمان هایی که بعداً هم اضافه میکنی این تنوع رو ببین». قاعده در `HEROES.fa.md`. قدِ روی خیابان در `HeroRig.Stature`.

`Art/Heroes/hamada_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, medieval Middle-Eastern town. ⚠ Must look clearly DIFFERENT from a thin man in a beige patched tunic with blue/red/ochre paint: different body, different material, different colours. Character: Hamada, a pickle and fruit seller from Cairo, about 40, SHORT and very FAT, a big round belly, short thick legs and arms, a shiny bald head with a ring of black curly hair around the back. His clothes are made of real green grape-vine leaves stitched together into a short sleeveless tunic, over baggy saffron-yellow trousers tied at the calf, red leather slippers. A big wicker basket bag on a strap over his shoulder, overflowing with purple grape bunches and green pickled cucumbers. His mask: a round mask cut from the polished wooden lid of a pickle barrel, a shiny brass ring as a handle on the forehead. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head and the hair fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee), any bag or strap on the back or shoulder, not over the arms. Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/hamada_front.png`
```
The same person as the attached back-view picture, same body shape, same mask, same hair, same clothes and colours, same bag and details, same photorealistic style, now FRONT view, full body, standing relaxed, holding up a bunch of purple grapes, belly forward. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/bubakr_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, medieval Middle-Eastern town. ⚠ Must look clearly DIFFERENT from a thin man in a beige patched tunic with blue/red/ochre paint: different body, different material, different colours. Character: Bubakr, an old fisherman from Tunis, about 60, very TALL and very THIN, long bony limbs, long wild white hair down to his shoulder blades tied loosely with a cord, a long white beard edge. His clothes: a sleeveless knee-length tunic of black tarred sailcloth, shiny and stiff, over short dark grey trousers, bare feet with rope anklets, his skin deeply sun-tanned and wet. A long wooden oar strapped diagonally across his back, a string of dried silver fish hanging from his belt. His mask: a mask of dried silver-grey fish skin stretched on a driftwood frame, a small white shell on the cheek. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head and the hair fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee), any bag or strap on the back or shoulder, not over the arms. Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/bubakr_front.png`
```
The same person as the attached back-view picture, same body shape, same mask, same hair, same clothes and colours, same bag and details, same photorealistic style, now FRONT view, full body, standing relaxed, leaning on his wooden oar, a wet fishing net over one arm. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```
`Art/Heroes/shadi_idle.png`
```
Original character for a mobile street game. Photorealistic cinematic photograph, soft natural daylight, real human body proportions, medieval Middle-Eastern town. ⚠ Must look clearly DIFFERENT from a thin man in a beige patched tunic with blue/red/ochre paint: different body, different material, different colours. Character: Shadi, a warrior-like lamplighter from Oman, about 30, MEDIUM height, BROAD and MUSCULAR, thick neck, long straight black hair in a high ponytail. His clothes: a knee-length coat of overlapping small plates of dark brown leather like scales, a wide sash of deep indigo cloth, wide dark indigo trousers tied at the ankle, black leather boots. Three small round copper lanterns hanging from his sash, a coil of rope on his hip. His mask: a round mask of dark hammered iron with a thin bright golden ring around its edge, like the sun hidden by the moon. Full body seen MOSTLY FROM BEHIND, turned only slightly to the RIGHT: the back of the head and the hair fill most of the head, only the right edge of the mask shows (at most one third of it), NO eyes visible. Neutral rig pose: standing straight, legs slightly apart, both feet pointing to the right, the near leg slightly in front of the far leg, arms hanging a little away from the body, hands open and relaxed, elbows and knees straight, both lower legs visible and separate (nothing long below the knee), any bag or strap on the back or shoulder, not over the arms. Flat solid bright green background #00FF00, no ground, no shadow, no scene. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no flag, no religious or political symbols.
```
`Art/Heroes/shadi_front.png`
```
The same person as the attached back-view picture, same body shape, same mask, same hair, same clothes and colours, same bag and details, same photorealistic style, now FRONT view, full body, standing relaxed, holding one copper lantern up, its flame lit. Flat solid bright green background #00FF00, no ground, no shadow. Whole body visible, centered, about 85% of the image height, portrait 2:3. No text, no letters, no logo, no religious or political symbols.
```

### خانه‌های بیشتر (۲۷ آبان)
کاربر: «خونه ها رو چندتا دیگه هم بده که تنوعش بیشتر باشه». ⚠️ دیوارِ چپ و راست از بازی رفته‌اند (کاربر: «دیوار راست و چپ رو که خیلی وقته بیخیال شدیم») — پرامپتشان بی‌اعتبار. هر خانه تعدادِ پنجره‌ی فرد.

`Art/house_40.png`
```
A straight-on flat elevation of ONE wide old town house facade, rose-pink sandstone with carved stone frames, a row of hanging clay pots of red geraniums on the second floor ledge. Exactly 17 windows in three rows (6, 6, 5), each a small window of leaded panes of thick old glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_40_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/house_41.png`
```
A straight-on flat elevation of ONE wide old town house facade, dark brown timber-framed upper storeys over a ground floor of rough grey stone, wooden beams criss-crossing the white plaster. Exactly 19 windows in three rows (7, 7, 5), each a small window of leaded panes of thick old glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_41_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/house_42.png`
```
A straight-on flat elevation of ONE wide old town house facade, turquoise and white glazed tiles around each window on a cream stone wall, a small fountain niche at street level. Exactly 15 windows in three rows (5, 5, 5), each a small window of leaded panes of thick old glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_42_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/house_43.png`
```
A straight-on flat elevation of ONE wide old town house facade, tall narrow house of red fired bricks in decorative patterns, small wooden latticed bay windows, laundry ropes with coloured cloths between windows. Exactly 21 windows in three rows (7, 7, 7), each a small window of leaded panes of thick old glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_43_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```
`Art/house_44.png`
```
A straight-on flat elevation of ONE wide old town house facade, crumbling yellow ochre plaster with big cracks, a half-collapsed wooden roof edge on top, wooden props holding a sagging balcony. Exactly 17 windows in three rows (6, 5, 6), each a small window of leaded panes of thick old glass in a wooden frame, all clearly separate and fully visible. Camera exactly perpendicular, no perspective, no street, no sky: the facade fills the whole image edge to edge. Photorealistic cinematic photograph, soft warm late-afternoon light, a medieval Middle-Eastern walled town. No modern objects at all: no cars, no wires, no electric lights, no air conditioners, no satellite dishes, no signs. No text, no letters, no logo, no religious or political symbols. Wide 3:1 image.
```
`Art/house_44_broken.png`
```
The attached picture, EXACTLY the same in every pixel (same framing, same walls, same window positions and sizes), only every window's glass is smashed: jagged holes, cracked panes, shards still in the frames. Wide 3:1 image.
```

### همه‌ی موجوداتِ زنده ماسک دارند (۲۷ آبان)
کاربر (با تصویرِ کبوترِ بی‌ماسک): «موجودات زنده مثل اسب و پرنده و شتر هم ماسک داشته باشن. عابرها و همسایه‌ها هم». این پرامپت‌ها جای همان‌های بالا را می‌گیرند؛ همسایه‌ها، عابران، برانکاردارها، رفتگر و الاغ هم همین جمله‌ی ماسک را می‌گیرند (هر کدام جنسِ ماسکِ خودش).

`Art/Poison/pigeon_0.png`
```
A grey city pigeon flying, wings fully up, seen from the side facing right, wearing a tiny mask of thin carved wood with a pointed tip over its beak, tied behind the head with red thread. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/pigeon_1.png`
```
The same masked grey pigeon as the attached picture, same mask, same size and angle, wings halfway down. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/pigeon_2.png`
```
The same masked grey pigeon as the attached picture, same mask, same size and angle, wings fully down. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/bus.png`
```
A long camel caravan seen exactly from the side facing left: three loaded camels in a row, tied together, bundles and rugs on their backs, one camel driver walking in front holding the rope; each camel wears a long face mask of tooled leather with brass studs, the driver a half mask of dark wood. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/truck.png`
```
A tall overloaded hay wagon seen exactly from the side facing left, hay piled very high, pulled by one ox wearing a broad mask of woven straw between its horns, WITHOUT the wagon's wheels (empty axles). Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/tow_truck.png`
```
Two medieval market guards walking to the left, side view, leather jerkins and wooden clubs, one carries a heavy iron hook on a rope, fully clothed; each wears a stern mask of hammered tin, one plain, one with a crest of bristles. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/tanker_0.png`
```
A water carrier walking to the left, side view, a big goatskin water bag on his back with its spout closed, a brass cup on his belt, wearing a round mask of blue-glazed clay with a small spout-shaped mouth. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/tanker_1.png`
```
The same masked water carrier as the attached picture, same mask, same size and angle, standing still and bending forward, pouring water from the goatskin spout to the ground. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/rug_pusher_0.png`
```
A strong woman fully clothed in a long-sleeved knee-length tunic and trousers, a cloth over her hair, wearing a mask woven from red and blue rug threads, side view facing left, leaning forward with both palms on something in front of her. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/rug_pusher_1.png`
```
The same masked woman as the attached picture, same mask, same size and angle, both arms fully stretched, shoving hard. Every living creature in this picture wears its own small hand-made mask over the face (animals too), like the heroes of this game: carved wood, stiff leather, woven straw or hammered tin, tied with cord, eyes still visible through holes; the mask's material and shape are different for each creature. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow, whole object visible, centered. No text, no letters, no logo, no religious or political symbols.
```
`Art/Poison/granny2.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: an old woman, a cloth over her grey hair, shaking her fist, a mask of cracked white porcelain, eyes visible through the mask holes, the mouth area open so the shouting shows. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_01.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a bald old man with a thick grey moustache, shouting, pointing down, a mask of dark walnut wood, eyes visible through the mask holes, the mouth area open so the shouting shows. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_02.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a middle-aged woman, a cloth over her hair, holding a wooden ladle, furious, a mask of copper sheet, eyes visible through the mask holes, the mouth area open so the shouting shows. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_03.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a young man with a messy beard, half asleep, angry, yelling, a mask of stitched felt, eyes visible through the mask holes, the mouth area open so the shouting shows. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_04.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a heavy man holding a clay water jug as if about to throw it, a mask of terracotta, eyes visible through the mask holes, the mouth area open so the shouting shows. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_05.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a thin old man in a felt cap, shaking a walking stick, a mask of woven reed, eyes visible through the mask holes, the mouth area open so the shouting shows. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```
`Art/Poison/resident_06.png`
```
A neighbour leaning out of a window, seen from the front from the waist up, fully clothed in medieval Middle-Eastern linen and wool: a young woman, a cloth over her hair, holding a crying swaddled baby (the woman in a mask of painted leather, the baby in a tiny soft cloth mask), shouting, eyes visible through the mask holes, the mouth area open so the shouting shows. Only the person, no window frame. Photorealistic, soft daylight. Isolated on a flat solid bright green background #00FF00, no shadow. No text, no religious or political symbols.
```

### قاب‌های حرکت به‌جای عروسک (۲۷ آبان)
کاربر: «برای هر کارکتر چندتا پک شیش تایی بگیریم تصویر و اونا رو انیمیشنی به هم وصل کنیم بهتره: یکی برای واستادن و نفس کشیدن، یکی برای راه رفتن به سمت توپ که باید نیم رخ شن، یکی برای ضربه زدن. یکی هم آتلیه ای … رو پوستر بازی و عکس پروفایل». زعتر آمد و نصب شد (`zaatar_frames/`؛ `HeroRig` حالتِ قاب). قهرمان‌ها **یکی‌یکی**. به هر پنج پرامپت تصویرِ پشتِ فعلیِ همان قهرمان پیوست شود. در برگه‌ی شوت، قابِ سومِ زعتر پا را به بغل برده بود (کاربر: «سومی از چپ زاویه پاش مناسب نیست»)؛ جمله‌ی قابِ ۳ سفت‌تر شد.
الگو (جای NAME اسمِ فایل):

`Art/Heroes/NAME_sheet_idle.png`
```
A sprite sheet of the SAME character as the attached picture (same mask, same hair, same clothes, same marks, same body, same size, same bag or tools), photorealistic, soft natural daylight. 6 frames in ONE horizontal row, equal spacing, the character the same size and at the same place in every frame, feet on the same baseline. Seen MOSTLY FROM BEHIND, turned slightly to the right, as in the attached picture. A standing idle loop: frame 1 standing relaxed, weight on both feet; frame 2 breathing in, chest and shoulders slightly raised; frame 3 full breath, weight shifting onto the left foot; frame 4 breathing out, weight on the left foot, right knee slightly bent; frame 5 weight coming back to the middle, head turned a little more to the right; frame 6 almost back to frame 1. Small natural differences only, feet never move. Flat solid bright green background #00FF00, no ground, no shadow, no frame lines, no numbers, no text. Wide 2:1 image.
```
`Art/Heroes/NAME_sheet_walk.png`
```
A sprite sheet of the SAME character as the attached picture (same mask, same hair, same clothes, same marks, same body, same size, same bag or tools), photorealistic, soft natural daylight. 6 frames in ONE horizontal row, equal spacing, the character the same size in every frame, feet on the same baseline. Exact SIDE VIEW (profile), walking to the RIGHT, one full walk cycle that loops: frame 1 right foot forward touching the ground with the heel, left arm forward; frame 2 weight on the right foot, left foot lifting behind; frame 3 left leg passing under the body, knee bent; frame 4 left foot forward touching the ground with the heel, right arm forward; frame 5 weight on the left foot, right foot lifting behind; frame 6 right leg passing under the body, knee bent. Natural bent knees and elbows, relaxed hands. Flat solid bright green background #00FF00, no ground, no shadow, no frame lines, no numbers, no text. Wide 2:1 image.
```
`Art/Heroes/NAME_sheet_kick.png`
```
A sprite sheet of the SAME character as the attached picture (same mask, same hair, same clothes, same marks, same body, same size, same bag or tools), photorealistic, soft natural daylight. 6 frames in ONE horizontal row, equal spacing, the character the same size in every frame, the standing LEFT foot at the same place on the same baseline in every frame. Seen MOSTLY FROM BEHIND, turned slightly to the right, as in the attached picture. Kicking a ball with the RIGHT foot; the kick goes FORWARD, away from the camera, into the picture, never out to the side; NO ball in the picture: frame 1 wind-up, right leg drawn back behind the body, knee bent, arms out for balance, body leaning forward; frame 2 right leg swinging down past the standing leg, standing knee slightly bent; frame 3 the moment of contact, right foot low, just in front of the standing foot and close to it, toes pointing away from the camera, we see the heel and the back of the calf; frame 4 follow-through, right leg raised forward away from the camera (we see the sole of the sandal and the back of the thigh), body leaning back, left arm forward; frame 5 right leg coming down; frame 6 landed, both feet on the ground, knees slightly bent. Flat solid bright green background #00FF00, no ground, no shadow, no frame lines, no numbers, no text. Wide 2:1 image.
```
`Art/Heroes/NAME_studio_turn.png`
```
A studio character sheet of the SAME character as the attached picture (same mask, same hair, same clothes, same marks, same body, same bag or tools), photorealistic studio photograph, soft even studio light. 5 full-body views in ONE horizontal row, same size, feet on the same baseline: front view, three-quarter front view, side profile, three-quarter back view, back view. Standing relaxed, arms slightly away from the body. Flat solid bright green background #00FF00, no ground, no shadow, no text, no labels. Wide 2:1 image.
```
`Art/Heroes/NAME_studio_portrait.png`
```
A studio portrait of the SAME character as the attached picture (same mask, same hair, same clothes, same marks), photorealistic, dramatic soft studio light from the left, head and shoulders, three-quarter front view, the mask sharp and detailed, looking slightly past the camera. Flat solid bright green background #00FF00, no shadow on the background, no text. Square image.
```

- (۲۷ آبان) فلّه: کاربر: «ته روسریش همیشه بالاست. انگار همیشه توفانه. اینم درستش کنیم تو پرامپت جدید». در برگه‌های فلّه جمله‌ی «روسری آرام از پشت آویزان است، بادی نیست؛ فقط در قاب‌های شوت کمی تکان می‌خورد» اضافه می‌شود و «same clothes» با «the scarf is the same scarf but hanging down» همراه می‌شود (تصویرِ پیوست روسریِ در باد دارد).

### عابرها با ماسک، برگه‌ی ۶ قابی (۲۸ آبان)
`Art/Props/ped_01_merchant.png`
```
A pedestrian walking to the right, exact side view, full body, fully clothed: a fat merchant in a striped robe and a felt cap carrying a cloth bundle under his arm, wearing a full-face mask of polished brass. A horizontal sprite sheet of 6 walk-cycle frames in one row, the same person, same mask and clothes in every frame, same size and spacing, feet on the same baseline, legs clearly apart in different step positions. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/ped_02_shopper.png`
```
A pedestrian walking to the right, exact side view, full body, fully clothed: a woman in a long dress, a cloth over her hair, carrying a woven basket of bread, wearing a full-face mask of embroidered linen. A horizontal sprite sheet of 6 walk-cycle frames in one row, the same person, same mask and clothes in every frame, same size and spacing, feet on the same baseline, legs clearly apart in different step positions. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/ped_03_oldman.png`
```
A pedestrian walking to the right, exact side view, full body, fully clothed: a bent old man with a walking stick and a long grey beard showing under a full-face mask of pale birch bark. A horizontal sprite sheet of 6 walk-cycle frames in one row, the same person, same mask and clothes in every frame, same size and spacing, feet on the same baseline, legs clearly apart in different step positions. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/ped_04_hurried.png`
```
A pedestrian walking to the right, exact side view, full body, fully clothed: a thin young man hurrying with a clay jar on his shoulder, wearing a full-face mask of red clay. A horizontal sprite sheet of 6 walk-cycle frames in one row, the same person, same mask and clothes in every frame, same size and spacing, feet on the same baseline, legs clearly apart in different step positions. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/ped_05_crutches.png`
```
A pedestrian walking to the right, exact side view, full body, fully clothed: a man with a bandaged leg walking on two wooden crutches, wearing a full-face mask of patched sackcloth. A horizontal sprite sheet of 6 walk-cycle frames in one row, the same person, same mask and clothes in every frame, same size and spacing, feet on the same baseline, legs clearly apart in different step positions. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow. No text, no letters, no logo, no religious or political symbols.
```
`Art/Props/ped_06_boy.png`
```
A pedestrian walking to the right, exact side view, full body, fully clothed: a small boy in a patched tunic and trousers running with a rag ball under his arm, wearing a full-face mask of painted gourd. A horizontal sprite sheet of 6 walk-cycle frames in one row, the same person, same mask and clothes in every frame, same size and spacing, feet on the same baseline, legs clearly apart in different step positions. Photorealistic photograph, soft natural daylight, medieval Middle-Eastern era, nothing modern. Isolated on a flat solid bright green background #00FF00, no ground, no shadow. No text, no letters, no logo, no religious or political symbols.
```
