using System.Collections.Generic;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// Step 4 of the reference-game look (user 2026-11-11: «کیفیت انیمیشن و حرکت بازیکنا»): a hero cut into pieces
    /// (Art/Heroes/&lt;id&gt;_rig/*.png, from one painted rig sheet) and moved by joints in code -- breathing and a swaying
    /// arm at rest, a walk cycle (legs, arms, a bob) whenever the hero moves, and a kick (wind-up, snap, follow
    /// through). Sits under the hero's painted SpriteRenderer, which stays as the size reference and is hidden; its
    /// flipX turns the whole puppet, so DuelStage's facing logic needs no change.
    /// ⚠️ The joint table is measured on the sheet (bundled in the comment of each row: pivot = the joint inside the
    /// piece, then where it sits on its parent, in the sheet's pixels / 100). A new hero needs its own table.
    /// </summary>
    public class HeroRig : MonoBehaviour
    {
        struct Part
        {
            public string Name, Parent; public Vector2 Pivot, Local; public int Order;
            public Part(string n, string p, float px, float py, float lx, float ly, int o) { Name = n; Parent = p; Pivot = new Vector2(px, py); Local = new Vector2(lx, ly); Order = o; }
        }

        // abu_shahata, third try (2026-11-11). User: «باید مثل تصویر اصلیش بشه نه این‌جوری». Puppets assembled from a
        // separate pieces sheet never looked like the hero (two sheets: wrong feet, extra joints on the shorts, arms
        // akimbo). Now the REFERENCE picture itself (Art/Heroes/abu_shahata_rig_ref, standing straight) is cut along the
        // joints into seven pieces -- head, tank top, shorts, two arms, two legs -- so at rest it IS the picture; each
        // piece carries a little hidden flesh painted in around its own joint so a swing shows no hole.
        // Origin = between the feet; units = reference pixels / 100. Screen-left arm and leg are the far ones.
        static readonly Dictionary<string, Part[]> Tables = new Dictionary<string, Part[]>
        {
            ["abu_shahata"] = new[]
            {
                new Part("shorts", null, 0.5171f, 0.9111f, -0.045f, 8.960f, 4),
                new Part("torso", "shorts", 0.4951f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4083f, 0.1890f, 0.050f, 3.480f, 3),
                new Part("arm_far", "torso", 0.5799f, 0.9169f, -1.550f, 2.700f, 5),
                new Part("arm_near", "torso", 0.2959f, 0.9324f, 1.150f, 2.700f, 7),
                new Part("leg_far", "shorts", 0.5355f, 0.9550f, -0.950f, -4.400f, 1),
                new Part("leg_near", "shorts", 0.3610f, 0.9581f, 0.900f, -4.400f, 2),
            },
            // The other seven (2026-11-11), cut by the same recipe from their cartoon back views (rig7/build.py in the
            // session scratchpad; polygons per hero). ⚠️ Long robes (um_arabiyat, hamada): the leg turns at the knee just
            // above the hem, not at the hip -- from the hip the whole hidden shin swung out beside the robe like a plank.
            ["rima"] = new[]
            {
                new Part("shorts", null, 0.4710f, 0.9191f, -0.150f, 9.190f, 4),
                new Part("torso", "shorts", 0.5272f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4365f, 0.1730f, -0.100f, 3.100f, 3),
                new Part("arm_far", "torso", 0.5312f, 0.9156f, -1.550f, 2.400f, 5),
                new Part("arm_near", "torso", 0.1832f, 0.9462f, 1.150f, 2.500f, 7),
                new Part("leg_far", "shorts", 0.5455f, 0.9518f, -0.900f, -4.900f, 1),
                new Part("leg_near", "shorts", 0.3629f, 0.9556f, 0.900f, -4.900f, 2),
            },
            // User (2026-11-17): the realistic masked Zaatar («عالی شد، همین رو بریم»), cut from Art/ArtArchive/Heroes/real/zaatar_real_back.png
            ["zaatar"] = new[]
            {
                new Part("shorts", null, 0.4921f, 0.9049f, -0.310f, 8.710f, 4),
                new Part("torso", "shorts", 0.4828f, -0.0060f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4755f, 0.2143f, -0.070f, 3.400f, 3),
                new Part("arm_far", "torso", 0.4892f, 0.8655f, -1.400f, 1.970f, 5),
                new Part("arm_near", "torso", 0.2483f, 0.8948f, 1.200f, 2.070f, 7),
                new Part("leg_far", "shorts", 0.5879f, 0.9603f, -0.820f, -3.680f, 1),
                new Part("leg_near", "shorts", 0.3129f, 0.9618f, 0.800f, -3.680f, 2),
            },
            ["fulla"] = new[]
            {
                new Part("shorts", null, 0.5184f, 0.9168f, 0.880f, 9.320f, 4),
                new Part("torso", "shorts", 0.7650f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4324f, 0.2051f, 0.000f, 3.250f, 3),
                new Part("arm_far", "torso", 0.8116f, 1.3191f, -1.250f, 2.050f, 5),
                new Part("arm_near", "torso", 0.1488f, 0.9482f, 0.950f, 2.450f, 7),
                new Part("leg_far", "shorts", 0.6040f, 0.9553f, -1.000f, -4.850f, 1),
                new Part("leg_near", "shorts", 0.3597f, 0.9573f, 0.800f, -4.850f, 2),
            },
            ["toni"] = new[]
            {
                new Part("shorts", null, 0.4834f, 0.8553f, -0.155f, 8.730f, 4),
                new Part("torso", "shorts", 0.5174f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4663f, 0.2115f, 0.100f, 3.750f, 3),
                new Part("arm_far", "torso", 0.4071f, 0.9126f, -1.700f, 2.750f, 5),
                new Part("arm_near", "torso", 0.2646f, 0.9471f, 1.100f, 2.850f, 7),
                new Part("leg_far", "shorts", 0.5337f, 0.9671f, -0.900f, -2.550f, 1),
                new Part("leg_near", "shorts", 0.3571f, 0.9687f, 0.900f, -2.550f, 2),
            },
            ["um_arabiyat"] = new[]
            {
                new Part("shorts", null, 0.4719f, 0.9183f, -0.225f, 9.050f, 4),
                new Part("torso", "shorts", 0.4966f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.5018f, 0.2241f, 0.000f, 3.350f, 3),
                new Part("arm_far", "torso", 0.6125f, 0.9442f, -1.600f, 2.400f, 5),
                new Part("arm_near", "torso", 0.1493f, 0.9322f, 1.300f, 2.500f, 7),
                new Part("leg_far", "shorts", 0.5261f, 0.9507f, -0.900f, -4.900f, 1),
                new Part("leg_near", "shorts", 0.3401f, 0.9541f, 1.100f, -4.900f, 2),
            },
            ["hamada"] = new[]
            {
                new Part("shorts", null, 0.5109f, 0.7115f, 0.245f, 5.330f, 4),
                new Part("torso", "shorts", 0.5336f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4234f, 0.2213f, 0.100f, 5.850f, 3),
                new Part("arm_far", "torso", 0.4565f, 0.7523f, -2.550f, 4.800f, 5),
                new Part("arm_near", "torso", 0.2000f, 0.9307f, 1.300f, 5.000f, 7),
                new Part("leg_far", "shorts", 0.5433f, 0.9554f, -1.400f, -0.900f, 1),
                new Part("leg_near", "shorts", 0.3931f, 0.9569f, 0.800f, -0.900f, 2),
            },
            ["bubakr"] = new[]
            {
                new Part("shorts", null, 0.5029f, 0.9055f, 0.280f, 8.910f, 4),
                new Part("torso", "shorts", 0.6745f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.5654f, 0.1581f, 0.200f, 3.200f, 3),
                new Part("arm_far", "torso", 0.4365f, 0.8872f, -1.800f, 2.000f, 5),
                new Part("arm_near", "torso", 0.1515f, 0.9527f, 1.200f, 2.700f, 7),
                new Part("leg_far", "shorts", 0.5263f, 0.9584f, -0.800f, -4.100f, 1),
                new Part("leg_near", "shorts", 0.2629f, 0.9602f, 1.000f, -4.100f, 2),
            },
            ["shadi"] = new[]
            {
                new Part("shorts", null, 0.4832f, 0.9191f, -0.240f, 9.420f, 4),
                new Part("torso", "shorts", 0.4912f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.5409f, 0.1993f, 0.000f, 3.100f, 3),
                new Part("arm_far", "torso", 0.4526f, 0.8852f, -1.700f, 2.200f, 5),
                new Part("arm_near", "torso", 0.2513f, 0.9303f, 1.200f, 2.400f, 7),
                new Part("leg_far", "shorts", 0.5475f, 0.9531f, -1.000f, -4.900f, 1),
                new Part("leg_near", "shorts", 0.3546f, 0.9577f, 1.000f, -4.900f, 2),
            },
        };

        /// <summary>
        /// Rest angle of a joint (degrees, counter-clockwise), where the sheet's piece is not drawn in the standing pose.
        /// User (2026-11-11) on the second sheet: «جهت دست ها درست نیست» -- its arms were still drawn akimbo with the fist
        /// in front of the belly; turned here so they hang beside the body like the reference picture.
        /// </summary>
        static readonly Dictionary<string, Dictionary<string, float>> Rest = new Dictionary<string, Dictionary<string, float>>
        {
            
        };
        Dictionary<string, float> rest = new Dictionary<string, float>();

        /// <summary>Height of each puppet from the feet to the top of the head, in its own units (reference px / 100).</summary>
        static readonly Dictionary<string, float> Heights = new Dictionary<string, float> { ["abu_shahata"] = 14.81f, ["rima"] = 14.69f, ["zaatar"] = 14.32f, ["fulla"] = 14.75f, ["toni"] = 14.54f, ["um_arabiyat"] = 14.67f, ["hamada"] = 13.16f, ["bubakr"] = 14.57f, ["shadi"] = 14.78f };

        // User (2026-11-17): «مثلاً چاق و لاغر کن یا کوتاه و بلند … تنوع رو زیاد کن و توی قهرمان هایی که بعداً هم اضافه
        // میکنی این تنوع رو ببین». Every picture fills the same height on screen, so a short hero drawn short would still
        // stand as tall as the rest: his height on the street is set here (1 = the usual).
        static readonly Dictionary<string, float> Statures = new Dictionary<string, float>
        { ["zaatar"] = 0.92f, ["rima"] = 0.95f, ["fulla"] = 0.96f, ["um_arabiyat"] = 1.03f, ["hamada"] = 0.84f, ["bubakr"] = 1.12f, ["shadi"] = 1.05f };
        public static float Stature(string id) => id != null && Statures.TryGetValue(id, out var s) ? s : 1f;

        public static bool Has(string id) => id != null && (HasFrames(id) || (Tables.ContainsKey(id) && Resources.Load<Texture2D>("Art/Heroes/" + id + "_rig/torso") != null));
        static bool HasFrames(string id) => Resources.Load<TextAsset>("Art/Heroes/" + id + "_frames/frames") != null;

        /// <summary>Builds the puppet under <paramref name="painted"/> (hidden from then on). Null if the hero has no rig.</summary>
        public static HeroRig Build(SpriteRenderer painted, string id)
        {
            if (!Has(id)) return null;
            var rig = painted.gameObject.AddComponent<HeroRig>();
            if (HasFrames(id)) rig.AssembleFrames(painted, id); else rig.Assemble(painted, Tables[id], id);
            return rig;
        }

        SpriteRenderer host;
        Transform flipRoot;
        readonly Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
        readonly Dictionary<string, SpriteRenderer> skins = new Dictionary<string, SpriteRenderer>();
        Vector3 rootRest, torsoScale = Vector3.one; float rigScale = 1f;
        // User (2026-11-17) on the new back-view Zaatar: «با توجه به این که بدن زاویه یک سوم به دوربین داره پا هم باید یک
        // سوم باشه ولی الان پا رفته راست کامل موقع شوت». Seen from behind, a kick goes INTO the picture, away from the
        // camera: the leg only swings a little sideways and mostly gets shorter. Only heroes drawn from behind.
        static readonly HashSet<string> BackView = new HashSet<string> { "zaatar", "abu_shahata", "rima", "fulla", "toni", "um_arabiyat", "hamada", "bubakr", "shadi" };
        // User (2026-11-17, after 0.4/0.4): «هنوز موقع زدن ضربه پاییه که رفته جلو باید به اون یکی پا نزدیک‌تر باشه برای همه
        // قهرمانا» -- the kicking foot still flew out sideways: a fifth of the swing, and more of it goes into the depth.
        const float DepthSwing = 0.2f, DepthShorten = 0.5f;
        bool foreshorten;
        readonly Dictionary<string, Vector3> legScale = new Dictionary<string, Vector3>();
        float Swing(float deg) => foreshorten ? deg * DepthSwing : deg;
        float Shorten(float deg) => foreshorten ? 1f - DepthShorten * Mathf.Sin(Mathf.Clamp(Mathf.Abs(deg), 0f, 90f) * Mathf.Deg2Rad) : 1f;
        /// <summary>swing: the stride or kick (goes into the depth for a back view); plant: a plain tilt that keeps the
        /// foot where it is while the hips move; lift: the foot off the ground (a shorter leg).</summary>
        void Leg(string bone, float swing, float plant = 0f, float lift = 0f)
        {
            Rot(bone, Swing(swing) + plant);
            if (bones.TryGetValue(bone, out var b) && legScale.TryGetValue(bone, out var s0))
                b.localScale = new Vector3(s0.x, s0.y * Shorten(swing) * (1f - lift), s0.z);
        }
        float phase, kickT = -1f, seed;
        Vector3 lastPos; float speed;

        void Assemble(SpriteRenderer painted, Part[] table, string id)
        {
            host = painted;
            if (Rest.TryGetValue(id, out var r0)) rest = r0;
            seed = Random.value * 10f;
            flipRoot = new GameObject("Rig").transform;
            flipRoot.SetParent(transform, false);
            // ⚠️ user (2026-11-12, phone): «بازیکنا حرکت میکنن بعد ضربه و میرن تو همدیگه». Every puppet drew its pieces at
            // the same orders (100 + piece), so two overlapping kids interleaved (one's arm on the other's chest). A
            // sorting group draws each puppet as one picture, nearer kid in front; the pieces keep their order inside it.
            flipRoot.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = 100;
            // ⚠️ user (2026-11-12, phone): «ام العربیات هنوز پاش مشکل داره». On the phone the skirt still came out shorter than
            // on CI and the flat top of each leg (made to sit under it) showed. Each piece now keeps the size of its
            // original picture (sizes.txt) whatever size the phone imported: its pixels per unit follow the import.
            var origW = new Dictionary<string, float>();
            var sizes = Resources.Load<TextAsset>("Art/Heroes/" + id + "_rig/sizes");
            if (sizes != null)
                foreach (var line in sizes.text.Split('\n'))
                {
                    var f = line.Trim().Split(' ');
                    if (f.Length >= 3 && !f[0].StartsWith("#") && float.TryParse(f[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ow) && ow > 0f) origW[f[0]] = ow;
                }
            foreach (var p in table)
            {
                var tex = Resources.Load<Texture2D>("Art/Heroes/" + id + "_rig/" + p.Name);
                var go = new GameObject(p.Name);
                var parent = p.Parent == null ? flipRoot : bones[p.Parent];
                go.transform.SetParent(parent, false);
                go.transform.localPosition = new Vector3(p.Local.x, p.Local.y, 0f);
                if (tex != null)
                {
                    var r = go.AddComponent<SpriteRenderer>();
                    float ppu = origW.TryGetValue(p.Name, out float ow) ? 100f * tex.width / ow : 100f;
                    r.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), p.Pivot, ppu);
                    // ⚠️ the pieces of one puppet sit in one plane: sortingOrder, not depth, says which covers which
                    r.sortingOrder = 100 + p.Order;
                    skins[p.Name] = r;
                }
                bones[p.Name] = go.transform;
            }
            rootRest = bones["shorts"].localPosition;
            torsoScale = bones["torso"].localScale;
            foreshorten = BackView.Contains(id);
            foreach (var n in new[] { "leg_far", "leg_near" }) if (bones.TryGetValue(n, out var lb)) legScale[n] = lb.localScale;
            // ⚠️ The host's scale comes from its standing picture, and on Android every hero picture is imported at
            // 512 px (on the CI Linux player at full size), so that picture is ~3x smaller there and the host ~3x larger.
            // User (2026-11-11, on the phone): «سایزش درست نیست، باید کوچیک شه اندازه بقیه» -- the puppet was a giant.
            // The puppet now takes the host's picture height as its own height, whatever size it was imported at.
            float h = Heights.TryGetValue(id, out var hh) ? hh : 14f;
            rigScale = host.sprite != null ? host.sprite.bounds.size.y / h : 1f;
            host.enabled = false;
            lastPos = transform.position;
        }

        /// <summary>DuelStage calls this when the ball leaves the foot.</summary>
        public void Kick(float fromDeg = ContactDeg) { kickT = 0f; Ready = false; kickFrom = fromDeg; }
        float kickFrom = ContactDeg;

        /// <summary>
        /// The leg angle at which the toe is at the height of a ball at <paramref name="ballWorld"/>: a rebound is hit
        /// where it is, often off the ground (user 2026-11-12: «ضربه دوم و سوم و چهارم رو هم درست کن بخوره به پاش حتماً»).
        /// </summary>
        public float ContactAngleFor(Vector3 ballWorld)
        {
            if (frameMode) return ContactDeg; // one painted contact frame: the foot is where the picture has it
            float best = ContactDeg, err = float.MaxValue;
            for (float d = 0f; d <= 75f; d += 1.5f)
            {
                float e = Mathf.Abs(transform.position.y + FootOffset(d).y - ballWorld.y);
                if (e < err) { err = e; best = d; }
            }
            return best;
        }

        /// <summary>Standing at the ball, waiting for the kick: the kicking leg is drawn back (the wind-up), so the kick
        /// itself starts at the contact and the ball leaves the foot on the very frame it is launched.</summary>
        public bool Ready;
        /// <summary>Both arms straight forward, leaning in: Um al-Arabiyat shoving a parked vehicle (user 2026-11-13: «زور ام العربیات خوبه»).</summary>
        public bool Push;
        float ready;

        /// <summary>Leg angle at which the foot meets the ball (the first frame of the kick).</summary>
        public const float ContactDeg = 8f;

        /// <summary>
        /// Where the toe of the kicking foot is, relative to this transform, with the leg at <paramref name="legDeg"/>.
        /// User (2026-11-11): «توپ رو موقع شوت کردن باید بخوره حتماً به پای بازیکن. الآن نمیخوره و چند سانت اون طرف‌تره»
        /// -- the kicker stood at a fixed 0.75 from the ball whatever the length of his legs; DuelStage now stands him
        /// so this point is on the ball.
        /// </summary>
        public Vector3 FootOffset(float legDeg)
        {
            if (frameMode) { var q = toe; q.x *= host.flipX ? -1f : 1f; return transform.TransformVector(q); }
            if (!skins.TryGetValue("leg_near", out var sr) || sr.sprite == null) return Vector3.zero;
            var b = sr.sprite.bounds;
            var tip = new Vector3(b.max.x - b.size.x * 0.12f, b.min.y + b.size.y * 0.04f, 0f);
            tip.y *= Shorten(legDeg);
            var p = rootRest + bones["leg_near"].localPosition + Quaternion.Euler(0f, 0f, Swing(legDeg)) * tip;
            p.x *= host.flipX ? -rigScale : rigScale; p.y *= rigScale;
            return transform.TransformVector(p);
        }
        public bool Kicking => kickT >= 0f && kickT < KickSeconds;
        const float KickSeconds = 0.7f; // 0.55 until 2026-11-17: room for the leg to come down and settle

        void Rot(string bone, float deg)
        {
            if (!bones.TryGetValue(bone, out var b)) return;
            rest.TryGetValue(bone, out float r0);
            b.localRotation = Quaternion.Euler(0f, 0f, r0 + deg);
        }

        // ---- painted frames instead of the puppet --------------------------------------------------------------------
        // User (2026-11-17, with three sheets of six frames of Zaatar): «همین الان زعتر یه جاهایی دستش شکافته. به نظرم برای
        // هر کارکتر چندتا پک شیش تایی بگیریم تصویر و اونا رو انیمیشنی به هم وصل کنیم بهتره … تو الان باید فواصل بین هر
        // تصویر رو خودت پر کنی. دقت کن که پوست دست و پا شکافت نشه و زاویه‌های پا … طبیعی بمونه». A hero with
        // Art/Heroes/<id>_frames/ (frames.txt + idle_N, walk_N, kick_N) is drawn from whole pictures: nothing is cut,
        // so nothing can split, and knees and elbows bend as painted. The gap between two pictures is a cross-fade:
        // the new picture comes up over the old one, then the old one goes (never both half-transparent at once, so
        // the street does not show through him). Heroes without frames keep the puppet below.
        // ⚠️ frames.txt gives every frame its pivot (the hips over the ground line) and how tall a standing hero is in
        // that picture: the three sheets were painted at three sizes, and the phone imports them smaller still.
        bool frameMode;
        readonly Dictionary<string, Sprite> frame = new Dictionary<string, Sprite>();
        readonly List<Sprite> idleClip = new List<Sprite>(), walkClip = new List<Sprite>(), shotClip = new List<Sprite>();
        SpriteRenderer under, over;
        float fade = 1f, fadeDur = 0.2f, idleClock, walkClock, facing = 1f;
        Vector3 toe;

        void AssembleFrames(SpriteRenderer painted, string id)
        {
            host = painted; frameMode = true;
            seed = Random.value * 10f;
            flipRoot = new GameObject("Frames").transform;
            flipRoot.SetParent(transform, false);
            flipRoot.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = 100;
            float hostH = host.sprite != null ? host.sprite.bounds.size.y : 1f;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            foreach (var line in Resources.Load<TextAsset>("Art/Heroes/" + id + "_frames/frames").text.Split('\n'))
            {
                var f = line.Trim().Split(' ');
                if (f.Length < 4 || f[0].StartsWith("#")) continue;
                var tex = Resources.Load<Texture2D>("Art/Heroes/" + id + "_frames/" + f[0]);
                if (tex == null) continue;
                float px = float.Parse(f[1], inv), py = float.Parse(f[2], inv), stand = float.Parse(f[3], inv);
                float ppu = stand * tex.height / Mathf.Max(0.01f, hostH);
                frame[f[0]] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(px, py), ppu);
                if (f.Length >= 7 && f[4] == "toe")
                    toe = new Vector3((float.Parse(f[5], inv) - px) * tex.width / ppu, (float.Parse(f[6], inv) - py) * tex.height / ppu, 0f);
            }
            // User (2026-11-18, twice, with screenshots): «توپ باید جلوی پایی باشه که داره ضربه میزنه نه اون یکی پا». The toe
            // measured on the contact picture (frames.txt) was the standing foot for half the heroes, and the contact
            // picture stands a little aside of the idle one, so the ball lay beside or under the wrong foot. The heroes
            // are seen from behind and all swing the right leg: the ball lies a tenth of the hero's height to the right of
            // his middle, just ahead of the right foot he waits on (DuelStage.KickerNear puts him nearer the camera).
            toe = new Vector3(0.10f * hostH, 0.02f * hostH, 0f);
            // User (2026-11-18): «بیا حالا فواصل بین هر دو تصویر رو خودت بساز … دیگه آدم ها دست و پاشون نباید شکافته بشه».
            // step_N / shot_N are the six painted pictures of the walk and of the kick (from the contact on) with two
            // in-betweens made between every pair: each in-between is ONE of its two neighbours bent a third of the way
            // toward the other (optical flow, offline), so it is sharp, whole, and never two pictures at once.
            for (int i = 0; i < 40; i++)
            {
                if (frame.TryGetValue("idle_" + i, out var a)) idleClip.Add(a);
                if (frame.TryGetValue("step_" + i, out var b)) walkClip.Add(b);
                if (frame.TryGetValue("shot_" + i, out var c)) shotClip.Add(c);
            }
            if (walkClip.Count == 0)
                for (int i = 0; i < 12; i++) if (frame.TryGetValue("walk_" + i, out var b)) walkClip.Add(b);
            under = Layer("under", 100); over = Layer("over", 101);
            under.sprite = idleClip.Count > 0 ? idleClip[0] : host.sprite;
            over.sprite = under.sprite; fade = 1f;
            idleClock = seed; // not all breathing in step
            host.enabled = false;
            lastPos = transform.position;
        }

        SpriteRenderer Layer(string n, int order)
        {
            var go = new GameObject(n);
            go.transform.SetParent(flipRoot, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sortingOrder = order;
            return r;
        }

        /// <summary>Cross-fade to this picture over <paramref name="seconds"/> (nothing happens if it is already the target).</summary>
        void Show(Sprite sp, float seconds)
        {
            if (sp == null || over.sprite == sp) return;
            if (fade >= 0.5f) under.sprite = over.sprite; // what is mostly on screen becomes the old picture
            over.sprite = sp; fade = 0f; fadeDur = Mathf.Max(0.01f, seconds);
        }

        /// <summary>This picture at once, nothing of the old one left (movement: two poses blended are a ghost).</summary>
        void Cut(Sprite sp)
        {
            if (sp == null) return;
            over.sprite = sp; under.sprite = sp; fade = 1f;
        }
        float readyT; bool wasMoving;

        Sprite KickFrame(int i) => frame.TryGetValue("kick_" + i, out var sp) ? sp : null;

        void FrameUpdate()
        {
            float dt = Time.deltaTime;
            var pos = transform.position;
            var step = pos - lastPos;
            float v = dt > 0f ? step.magnitude / dt : 0f;
            lastPos = pos;
            speed = Mathf.Lerp(speed, v, 1f - Mathf.Exp(-dt * 12f));
            float walk = Mathf.Clamp01((speed - 0.15f) / 0.8f);
            float home = host.flipX ? -1f : 1f;

            // User (2026-11-18, with a screen recording): «حرکت بازیکنا موقع شوت و راه رفتن خیلی بدتر شد که. یه حال ماتی داره».
            // The cross-fade between two different poses drew both at once: a ghost with four legs. Walking and kicking
            // now CUT from picture to picture (Cut), like any frame animation; only the standing pictures, which are
            // nearly the same, still melt into each other.
            if (kickT >= 0f)
            {
                // ⚠️ the ball leaves on the first frame of the kick: that frame is the contact picture (kick_2; the wind-up
                // kick_0 is the Ready stance). kick_3 (the high follow-through) is only in a sheet where it came out
                // right (user on Zaatar's and Rima's: «سومی از راست … زاویه پاش مناسب نیست»).
                kickT += dt;
                bool high = KickFrame(3) != null;
                if (shotClip.Count > 0)
                {
                    // contact on the first frame, then the whole follow-through and landing in 0.55 s, eased out
                    float q = Mathf.Clamp01(kickT / 0.55f);
                    Cut(shotClip[Mathf.Min(shotClip.Count - 1, (int)((1f - (1f - q) * (1f - q) * 0.5f - 0.5f * (1f - q)) * shotClip.Count))]);
                }
                else if (kickT < 0.10f) Cut(KickFrame(2));
                else if (high && kickT < 0.30f) Cut(KickFrame(3));
                else if (kickT < (high ? 0.44f : 0.34f)) Cut(KickFrame(4));
                else if (kickT < 0.60f) Cut(KickFrame(5));
                else if (idleClip.Count > 0) Cut(idleClip[0]);
                facing = home;
                if (kickT >= KickSeconds) { kickT = -1f; idleClock = 0f; }
            }
            else if (Ready)
            {
                // drawing the leg back: the swing-down picture for a moment, then the wind-up
                readyT += dt;
                Cut(readyT < 0.09f && KickFrame(1) != null ? KickFrame(1) : KickFrame(0));
                facing = home;
            }
            else if (walk > 0.2f && walkClip.Count > 0)
            {
                // the walk sheet is a profile walking right: he turns to where he is going. The pictures advance with
                // the ground covered, not with the clock, so the planted foot does not slide: one cycle of the sheet
                // (two steps) is about 0.9 of his height.
                if (Mathf.Abs(step.x) > 0.0005f) facing = step.x > 0f ? 1f : -1f;
                float height = host.sprite != null ? host.sprite.bounds.size.y * Mathf.Abs(host.transform.lossyScale.y) : 1f;
                walkClock += step.magnitude / Mathf.Max(0.01f, 0.9f * height) * walkClip.Count;
                Cut(walkClip[(int)walkClock % walkClip.Count]);
            }
            else if (idleClip.Count > 0)
            {
                // six nearly identical pictures of him breathing and shifting his weight, each melting into the next
                const float hold = 0.8f;
                if (wasMoving) { Cut(idleClip[0]); idleClock = 0f; }
                idleClock += dt;
                Show(idleClip[(int)(idleClock / hold) % idleClip.Count], hold);
                facing = home;
            }
            if (!Ready) readyT = 0f;
            wasMoving = kickT >= 0f || Ready || (walk > 0.2f && walkClip.Count > 0);

            fade = Mathf.Min(1f, fade + dt / fadeDur);
            var col = host.color;
            over.color = new Color(col.r, col.g, col.b, col.a * Ease(fade * 2f));
            under.color = new Color(col.r, col.g, col.b, col.a * (1f - Ease(fade * 2f - 1f)));
            flipRoot.localScale = new Vector3(facing, 1f, 1f);
        }

        // User (2026-11-17): «حرکت بازیکنا رو طبیعی تر کنیم. هم راه رفتنشون هم ضربه زدنشون هم نفس کشیدنشون هم واستادنشون به
        // دلم ننشسته». What was wrong: everything was one sine -- the arms flapped with the breath, a standing hero was a
        // statue between breaths, both legs of a walker shortened together (so he floated), and the kick froze at the
        // top and slid back. Now: the breath is in the chest (quick in, slow out), a standing hero shifts his weight from
        // foot to foot with his feet planted and looks around, a walker lifts one foot at a time and bobs on the other,
        // and the kick follows through, hangs, and comes down past rest before settling. Poses are eased into each other
        // (pose[]), except during the kick: the foot must be on the ball on the very first frame.
        readonly float[] pose = new float[9]; // legFar, legNear, armFar, armNear, torso, head, bob, sway, lift
        float idleW = 1f, leanV;
        static float Ease(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        void LateUpdate()
        {
            if (host == null) return;
            host.enabled = false;
            if (frameMode) { FrameUpdate(); return; }
            flipRoot.localScale = new Vector3(host.flipX ? -rigScale : rigScale, rigScale, 1f);
            foreach (var kv in skins) kv.Value.color = host.color;

            float dt = Time.deltaTime;
            var pos = transform.position;
            var step = pos - lastPos;
            float v = dt > 0f ? step.magnitude / dt : 0f;
            float vx = dt > 0f ? step.x / dt : 0f;
            lastPos = pos;
            speed = Mathf.Lerp(speed, v, 1f - Mathf.Exp(-dt * 12f));
            leanV = Mathf.Lerp(leanV, Mathf.Clamp(vx / 2.5f, -1f, 1f), 1f - Mathf.Exp(-dt * 6f));
            float walk = Mathf.Clamp01((speed - 0.15f) / 0.8f);
            phase += dt * Mathf.Lerp(0f, 8.5f, walk);
            float t = Time.time + seed;
            bool kicking = kickT >= 0f;

            // breath: 40% of the cycle in, 60% out, a little slower or faster per hero
            float bu = Mathf.Repeat(t / (3.2f + (seed % 1f) * 0.8f), 1f);
            float breath = bu < 0.4f ? Ease(bu / 0.4f) : 1f - Ease((bu - 0.4f) / 0.6f);

            // standing: the weight wanders from one foot to the other, the feet stay where they are
            idleW = Mathf.MoveTowards(idleW, (walk > 0.05f || Ready || Push || kicking) ? 0f : 1f, dt * 2.5f);
            float shift = (Mathf.Sin(t * 0.55f) + 0.45f * Mathf.Sin(t * 0.23f + 1.3f)) / 1.45f;
            float glance = Mathf.Sin(t * 0.37f + seed * 2f) * Mathf.Sin(t * 0.11f + seed);

            float sn = Mathf.Sin(phase), c = Mathf.Cos(phase);
            // ⚠️ counter-clockwise swings the far (left) arm INTO the body and the near (right) arm away from it; each arm
            // swings mostly outward and only a few degrees in (user 2026-11-11 on the kick: «دست چپش … اومده تو کونش»)
            float legFar = 0f, legNear = 0f, liftFar = 0f, liftNear = 0f, tiltFar = 0f, tiltNear = 0f;
            float armFar = -1.4f * breath, armNear = 1.4f * breath;      // the shoulders open a little on the in-breath
            float torso = 0f, head = -1.6f * (breath - 0.5f);
            float bob = 0f, sway = 0f;

            // idle
            sway += idleW * 0.4f * shift;
            torso += idleW * -1.6f * shift;
            head += idleW * (1.0f * shift + 3.5f * glance);
            armFar += idleW * 1.2f * shift; armNear += idleW * 1.2f * shift;

            // walk: one foot up at a time (the lifted leg is the shorter one), the body rides up over the standing leg
            if (walk > 0f)
            {
                // ⚠️ a plain tilt, not Leg()'s swing: the swing of a back view shortens the leg at BOTH ends of the stride
                // and the walker floated
                float stride = foreshorten ? 5f : 20f;
                tiltFar = walk * stride * sn; tiltNear = -walk * stride * sn;
                liftFar = walk * 0.11f * Mathf.Max(0f, c); liftNear = walk * 0.11f * Mathf.Max(0f, -c);
                bob += walk * 0.22f * (1f - Mathf.Abs(sn));
                sway += walk * 0.2f * -c;
                torso += walk * (1.8f * c - 3.5f * leanV * (host.flipX ? -1f : 1f));
                head += walk * -1.2f * c;
                armFar += walk * (-4f - 7f * sn); armNear += walk * (4f - 7f * sn);
            }

            ready = Mathf.MoveTowards(ready, Ready ? 1f : 0f, dt * 3f);
            if (ready > 0f && !kicking)
            {
                // wound up: the kicking leg back, the body over the standing leg and leaning in, alive on its toes
                float tense = 0.5f + 0.5f * Mathf.Sin(t * 5.2f);
                legNear = Mathf.Lerp(legNear, -20f, ready);
                armFar -= (8f + 2f * tense) * ready; armNear += (7f + 2f * tense) * ready;
                torso += 3.5f * ready; head -= 2f * ready;
                bob += 0.06f * tense * ready;
            }

            if (Push && !kicking) { armFar = 72f; armNear = 78f; torso = -9f; head = 5f; }

            if (kicking)
            {
                // ⚠️ the ball is launched on the first frame of the kick (DuelManager), so the kick STARTS at the contact
                // (the wind-up is the Ready stance before it); it used to wind up 0.14 s and meet the ball after it had gone
                kickT += dt;
                float k = kickT / KickSeconds, leg;
                float top = Mathf.Max(36f, kickFrom + 20f); // a high rebound: the follow-through goes on above the contact
                if (k < 0.16f) { float q = k / 0.16f; leg = Mathf.Lerp(kickFrom, top, 1f - (1f - q) * (1f - q) * (1f - q)); }   // through the ball
                else if (k < 0.34f) leg = top + 3f * Mathf.Sin((k - 0.16f) / 0.18f * Mathf.PI);                             // hangs, still rising a touch
                else { float q = (k - 0.34f) / 0.66f; leg = top * (1f - Ease(q * 1.25f)) - 5f * Mathf.Sin(Mathf.Clamp01(q * 1.25f - 0.25f) * Mathf.PI); } // down, past rest, settle
                float body = Mathf.Clamp01(k / 0.22f) * (1f - Ease((k - 0.45f) / 0.55f)); // the body follows the leg a beat later
                legNear = leg; legFar = -6f * body;
                armFar = -19f * body; armNear = 12f * body;
                torso = -6.5f * body; head = 4.5f * body;       // leaning back, eyes still on the ball
                bob = 0.22f * Mathf.Sin(Mathf.Clamp01((k - 0.05f) / 0.5f) * Mathf.PI);   // up on the standing toes, and down
                sway = 0f; liftFar = liftNear = 0f; tiltFar = tiltNear = 0f;
                if (kickT >= KickSeconds) kickT = -1f;
            }

            float[] want = { legFar, legNear, armFar, armNear, torso, head, bob, sway, 0f };
            float a = kicking ? 1f : 1f - Mathf.Exp(-dt * 16f);
            for (int i = 0; i < want.Length; i++) pose[i] = Mathf.Lerp(pose[i], want[i], a);

            // the hips move sideways by sway; each leg leans the other way so its foot stays put
            float legLen = Mathf.Max(1f, rootRest.y + bones["leg_near"].localPosition.y);
            float plant = -Mathf.Atan2(pose[7], legLen) * Mathf.Rad2Deg;
            Leg("leg_far", pose[0], plant + tiltFar, liftFar); Leg("leg_near", pose[1], plant + tiltNear, liftNear);
            Rot("arm_far", pose[2]); Rot("arm_near", pose[3]);
            Rot("torso", pose[4]); Rot("head", pose[5]);
            bones["shorts"].localPosition = rootRest + new Vector3(pose[7], pose[6], 0f);
            float chest = (kicking || Push) ? 0f : breath;
            bones["torso"].localScale = new Vector3(torsoScale.x * (1f + 0.014f * chest), torsoScale.y * (1f + 0.04f * chest), torsoScale.z);
        }
    }
}
