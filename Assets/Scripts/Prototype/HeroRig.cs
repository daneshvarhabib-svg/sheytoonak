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
                new Part("shorts", null, 0.4858f, 0.9188f, -0.210f, 7.910f, 4),
                new Part("torso", "shorts", 0.5051f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4817f, 0.1178f, 0.100f, 3.250f, 3),
                new Part("arm_far", "torso", 0.6173f, 0.8639f, -1.600f, 2.000f, 5),
                new Part("arm_near", "torso", 0.2317f, 0.8732f, 1.100f, 2.300f, 7),
                new Part("leg_far", "shorts", 0.6816f, 0.9492f, -0.800f, -3.900f, 1),
                new Part("leg_near", "shorts", 0.1704f, 0.9526f, 1.000f, -3.900f, 2),
            },
            ["bubakr"] = new[]
            {
                new Part("shorts", null, 0.5181f, 0.7630f, -0.025f, 6.160f, 4),
                new Part("torso", "shorts", 0.5182f, -0.0098f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4817f, 0.1568f, 0.200f, 5.100f, 3),
                new Part("arm_far", "torso", 0.5455f, 0.8759f, -2.000f, 3.600f, 5),
                new Part("arm_near", "torso", 0.2542f, 0.8785f, 1.200f, 3.800f, 7),
                new Part("leg_far", "shorts", 0.6652f, 0.9599f, -0.800f, -1.100f, 1),
                new Part("leg_near", "shorts", 0.3723f, 0.9620f, 1.000f, -1.100f, 2),
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
        static readonly Dictionary<string, float> Heights = new Dictionary<string, float> { ["abu_shahata"] = 14.81f, ["rima"] = 14.69f, ["zaatar"] = 14.32f, ["fulla"] = 14.75f, ["toni"] = 14.54f, ["um_arabiyat"] = 14.67f, ["hamada"] = 14.54f, ["bubakr"] = 14.39f };

        // User (2026-11-17): «مثلاً چاق و لاغر کن یا کوتاه و بلند … تنوع رو زیاد کن و توی قهرمان هایی که بعداً هم اضافه
        // میکنی این تنوع رو ببین». Every picture fills the same height on screen, so a short hero drawn short would still
        // stand as tall as the rest: his height on the street is set here (1 = the usual).
        static readonly Dictionary<string, float> Statures = new Dictionary<string, float>
        { ["zaatar"] = 0.92f, ["rima"] = 0.95f, ["fulla"] = 0.96f, ["um_arabiyat"] = 1.03f, ["hamada"] = 0.84f, ["bubakr"] = 1.12f, ["shadi"] = 1.05f };
        public static float Stature(string id) => id != null && Statures.TryGetValue(id, out var s) ? s : 1f;

        public static bool Has(string id) => id != null && Tables.ContainsKey(id) && Resources.Load<Texture2D>("Art/Heroes/" + id + "_rig/torso") != null;

        /// <summary>Builds the puppet under <paramref name="painted"/> (hidden from then on). Null if the hero has no rig.</summary>
        public static HeroRig Build(SpriteRenderer painted, string id)
        {
            if (!Has(id)) return null;
            var rig = painted.gameObject.AddComponent<HeroRig>();
            rig.Assemble(painted, Tables[id], id);
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
        static readonly HashSet<string> BackView = new HashSet<string> { "zaatar", "abu_shahata", "rima", "fulla", "toni", "um_arabiyat" };
        const float DepthSwing = 0.4f, DepthShorten = 0.4f;
        bool foreshorten;
        readonly Dictionary<string, Vector3> legScale = new Dictionary<string, Vector3>();
        float Swing(float deg) => foreshorten ? deg * DepthSwing : deg;
        float Shorten(float deg) => foreshorten ? 1f - DepthShorten * Mathf.Sin(Mathf.Clamp(Mathf.Abs(deg), 0f, 90f) * Mathf.Deg2Rad) : 1f;
        void Leg(string bone, float deg)
        {
            Rot(bone, Swing(deg));
            if (bones.TryGetValue(bone, out var b) && legScale.TryGetValue(bone, out var s0))
                b.localScale = new Vector3(s0.x, s0.y * Shorten(deg), s0.z);
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
            if (!skins.TryGetValue("leg_near", out var sr) || sr.sprite == null) return Vector3.zero;
            var b = sr.sprite.bounds;
            var tip = new Vector3(b.max.x - b.size.x * 0.12f, b.min.y + b.size.y * 0.04f, 0f);
            tip.y *= Shorten(legDeg);
            var p = rootRest + bones["leg_near"].localPosition + Quaternion.Euler(0f, 0f, Swing(legDeg)) * tip;
            p.x *= host.flipX ? -rigScale : rigScale; p.y *= rigScale;
            return transform.TransformVector(p);
        }
        public bool Kicking => kickT >= 0f && kickT < KickSeconds;
        const float KickSeconds = 0.55f;

        void Rot(string bone, float deg)
        {
            if (!bones.TryGetValue(bone, out var b)) return;
            rest.TryGetValue(bone, out float r0);
            b.localRotation = Quaternion.Euler(0f, 0f, r0 + deg);
        }

        void LateUpdate()
        {
            if (host == null) return;
            host.enabled = false;
            flipRoot.localScale = new Vector3(host.flipX ? -rigScale : rigScale, rigScale, 1f);
            foreach (var kv in skins) kv.Value.color = host.color;

            float dt = Time.deltaTime;
            var pos = transform.position;
            float v = dt > 0f ? (pos - lastPos).magnitude / dt : 0f;
            lastPos = pos;
            speed = Mathf.Lerp(speed, v, 1f - Mathf.Exp(-dt * 12f));
            float walk = Mathf.Clamp01((speed - 0.15f) / 0.8f);
            phase += dt * Mathf.Lerp(1f, 9f, walk);

            // rest: a slow sway; walking: the swing of a stride (single-piece limbs from the hips and shoulders)
            float sn = Mathf.Sin(phase), c = Mathf.Cos(phase);
            float breathe = Mathf.Sin(Time.time * 1.9f + seed);
            float legFar = walk * 20f * sn, legNear = -walk * 20f * sn;
            // ⚠️ counter-clockwise swings the far (left) arm INTO the body and the near (right) arm away from it; each arm
            // swings mostly outward and only a few degrees in (user 2026-11-11 on the kick: «دست چپش … اومده تو کونش»)
            // User (2026-11-16): «شدت نفس کشیدن قوی تر بشه که معلوم شه». The sway roughly doubled and the chest now rises
            // (the torso stretches up a little from the waist, lifting shoulders, arms and head with it; see below).
            float armFar = 3.5f * breathe - walk * (6f + 6f * sn), armNear = -3.5f * breathe + walk * (6f - 6f * sn);
            float torso = walk * 2f * c + breathe * 1.2f, head = -breathe * 2.4f + walk * 1.5f * sn;
            float bob = walk * 0.16f * Mathf.Abs(c);

            ready = Mathf.MoveTowards(ready, Ready ? 1f : 0f, dt * 3f);
            if (ready > 0f && kickT < 0f) { legNear = Mathf.Lerp(legNear, -20f, ready); armFar -= 6f * ready; armNear += 5f * ready; torso += 2f * ready; }

            if (Push && kickT < 0f) { armFar = 72f; armNear = 78f; torso = -9f; head = 5f; }

            if (kickT >= 0f)
            {
                // ⚠️ the ball is launched on the first frame of the kick (DuelManager), so the kick STARTS at the contact
                // (the wind-up is the Ready stance before it); it used to wind up 0.14 s and meet the ball after it had gone
                kickT += dt;
                float k = kickT / KickSeconds, leg;
                float top = Mathf.Max(36f, kickFrom + 20f); // a high rebound: the follow-through goes on above the contact
                if (k < 0.2f) { float q = k / 0.2f; leg = Mathf.Lerp(kickFrom, top, 1f - (1f - q) * (1f - q)); }
                else if (k < 0.4f) leg = top;
                else leg = Mathf.Lerp(top, 0f, Mathf.Clamp01((k - 0.4f) / 0.6f));
                legNear = leg; legFar = -leg * 0.15f;
                armFar = -Mathf.Abs(leg) * 0.35f; armNear = Mathf.Abs(leg) * 0.28f; torso = -leg * 0.08f; head = leg * 0.04f;
                bob = Mathf.Max(0f, Mathf.Sin(Mathf.Clamp01(k * 2f - 0.6f) * Mathf.PI)) * 0.2f;
                if (kickT >= KickSeconds) kickT = -1f;
            }

            Leg("leg_far", legFar); Leg("leg_near", legNear);
            Rot("arm_far", armFar); Rot("arm_near", armNear);
            Rot("torso", torso); Rot("head", head);
            bones["shorts"].localPosition = rootRest + new Vector3(0f, bob, 0f);
            float rise = (kickT >= 0f || Push) ? 0f : 0.035f * (1f - walk) * (0.5f + 0.5f * breathe);
            bones["torso"].localScale = new Vector3(torsoScale.x, torsoScale.y * (1f + rise), torsoScale.z);
        }
    }
}
