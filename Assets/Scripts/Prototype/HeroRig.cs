using System.Collections.Generic;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// Step 4 of the reference-game look (user 2026-11-11: «کیفیت انیمیشن و حرکت بازیکنا»): a hero cut into eleven pieces
    /// (Art/Heroes/&lt;id&gt;_rig/*.png, from one painted rig sheet) and moved by joints in code -- breathing and a swaying
    /// arm at rest, a walk cycle (thighs, knees, arms, a bob) whenever the hero moves, and a kick (wind-up, snap, follow
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

        // abu_shahata (second sheet, 2026-11-11). Origin = between the feet; units = sheet pixels / 100.
        // ⚠️ The first sheet was wrong (user: «جهت پا، پشت و جلو بودنش، شیوه اتصالش»): feet splayed like a front view,
        // no near/far limbs, flat cut joints. This one has both feet pointing right, round caps at the joints, and the
        // screen-left limbs are the far ones (drawn behind the torso and the shorts).
        static readonly Dictionary<string, Part[]> Tables = new Dictionary<string, Part[]>
        {
            ["abu_shahata"] = new[]
            {
                new Part("shorts", null, 0.4981f, 0.9590f, -0.050f, 5.420f, 7),
                new Part("thigh_l", "shorts", 0.4690f, 0.7612f, -0.580f, -1.800f, 2),
                new Part("shin_l", "thigh_l", 0.5781f, 0.9884f, -0.100f, -1.050f, 1),
                new Part("thigh_r", "shorts", 0.4912f, 0.7692f, 0.670f, -1.800f, 6),
                new Part("shin_r", "thigh_r", 0.2648f, 1.0000f, 0.150f, -1.100f, 5),
                new Part("torso", "shorts", 0.4933f, 0.0549f, 0.000f, -0.100f, 8),
                new Part("head", "torso", 0.4517f, 0.0484f, -0.050f, 2.250f, 9),
                new Part("upper_arm_l", "torso", 0.8039f, 0.7785f, -0.850f, 1.800f, 3),
                new Part("forearm_l", "upper_arm_l", 0.2452f, 0.8368f, -0.900f, -0.950f, 4),
                new Part("upper_arm_r", "torso", 0.2838f, 0.7750f, 0.650f, 1.950f, 10),
                new Part("forearm_r", "upper_arm_r", 0.7595f, 0.8413f, 0.800f, -1.000f, 11),
            },
        };

        /// <summary>
        /// Rest angle of a joint (degrees, counter-clockwise), where the sheet's piece is not drawn in the standing pose.
        /// User (2026-11-11) on the second sheet: «جهت دست ها درست نیست» -- its arms were still drawn akimbo with the fist
        /// in front of the belly; turned here so they hang beside the body like the reference picture.
        /// </summary>
        static readonly Dictionary<string, Dictionary<string, float>> Rest = new Dictionary<string, Dictionary<string, float>>
        {
            ["abu_shahata"] = new Dictionary<string, float> { ["upper_arm_l"] = 32f, ["forearm_l"] = -67f, ["upper_arm_r"] = -28f, ["forearm_r"] = 67f },
        };
        Dictionary<string, float> rest = new Dictionary<string, float>();

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
        Vector3 rootRest;
        float phase, kickT = -1f, seed;
        Vector3 lastPos; float speed;

        void Assemble(SpriteRenderer painted, Part[] table, string id)
        {
            host = painted;
            if (Rest.TryGetValue(id, out var r0)) rest = r0;
            seed = Random.value * 10f;
            flipRoot = new GameObject("Rig").transform;
            flipRoot.SetParent(transform, false);
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
                    r.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), p.Pivot, 100f);
                    // ⚠️ the pieces of one puppet sit in one plane: sortingOrder, not depth, says which covers which
                    r.sortingOrder = 100 + p.Order;
                    skins[p.Name] = r;
                }
                bones[p.Name] = go.transform;
            }
            rootRest = bones["shorts"].localPosition;
            host.enabled = false;
            lastPos = transform.position;
        }

        /// <summary>DuelStage calls this when the ball leaves the foot.</summary>
        public void Kick() { kickT = 0f; }
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
            flipRoot.localScale = new Vector3(host.flipX ? -1f : 1f, 1f, 1f);
            foreach (var kv in skins) kv.Value.color = host.color;

            float dt = Time.deltaTime;
            var pos = transform.position;
            float v = dt > 0f ? (pos - lastPos).magnitude / dt : 0f;
            lastPos = pos;
            speed = Mathf.Lerp(speed, v, 1f - Mathf.Exp(-dt * 12f));
            float walk = Mathf.Clamp01((speed - 0.15f) / 0.8f);
            phase += dt * Mathf.Lerp(1f, 9f, walk);

            // rest: a slow sway; walking: the swing of a stride
            float s = Mathf.Sin(phase), c = Mathf.Cos(phase);
            float breathe = Mathf.Sin(Time.time * 1.9f + seed);
            float thighL = walk * 28f * s, thighR = -walk * 28f * s;
            float shinL = -walk * 30f * Mathf.Max(0f, -s), shinR = -walk * 30f * Mathf.Max(0f, s);
            float armL = 3f * breathe - walk * 22f * s, armR = -3f * breathe + walk * 22f * s;
            float foreL = walk * -18f, foreR = walk * 18f;
            float torso = walk * 3f * c + breathe * 0.8f, head = -breathe * 1.5f + walk * 2f * s;
            float bob = walk * 0.18f * Mathf.Abs(c);

            if (kickT >= 0f)
            {
                // the kicking leg is the front (right) one: wind-up back, snap through the ball, follow-through, settle
                kickT += dt;
                float k = kickT / KickSeconds;
                float th, sh;
                if (k < 0.25f) { float a = k / 0.25f; th = Mathf.Lerp(0f, -38f, a); sh = Mathf.Lerp(0f, -55f, a); }
                else if (k < 0.5f) { float a = (k - 0.25f) / 0.25f; th = Mathf.Lerp(-38f, 62f, a * a); sh = Mathf.Lerp(-55f, 5f, a); }
                else { float a = Mathf.Clamp01((k - 0.5f) / 0.5f); th = Mathf.Lerp(62f, 0f, a); sh = Mathf.Lerp(5f, 0f, a); }
                thighR = th; shinR = sh; thighL = -th * 0.15f; shinL = 0f;
                armL = th * 0.45f; armR = -th * 0.35f; torso = -th * 0.08f; head = th * 0.05f;
                bob = Mathf.Max(0f, Mathf.Sin(Mathf.Clamp01(k * 2f - 0.6f) * Mathf.PI)) * 0.25f;
                if (kickT >= KickSeconds) kickT = -1f;
            }

            Rot("thigh_l", thighL); Rot("thigh_r", thighR);
            Rot("shin_l", shinL); Rot("shin_r", shinR);
            Rot("upper_arm_l", armL); Rot("upper_arm_r", armR);
            Rot("forearm_l", foreL); Rot("forearm_r", foreR);
            Rot("torso", torso); Rot("head", head);
            bones["shorts"].localPosition = rootRest + new Vector3(0f, bob, 0f);
        }
    }
}
