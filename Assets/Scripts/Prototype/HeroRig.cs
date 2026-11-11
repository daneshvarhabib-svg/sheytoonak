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
                new Part("shorts", null, 0.4905f, 0.8994f, -0.200f, 6.450f, 4),
                new Part("torso", "shorts", 0.4719f, 0.0090f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4363f, 0.1634f, -0.100f, 4.150f, 3),
                new Part("arm_far", "torso", 0.7430f, 0.8856f, -1.400f, 3.650f, 5),
                new Part("arm_near", "torso", 0.2299f, 0.9043f, 0.900f, 3.650f, 7),
                new Part("leg_far", "shorts", 0.5745f, 0.9565f, -1.200f, -1.850f, 1),
                new Part("leg_near", "shorts", 0.3858f, 0.9592f, 1.400f, -1.850f, 2),
            },
            // The other seven (2026-11-11), cut by the same recipe from their cartoon back views (rig7/build.py in the
            // session scratchpad; polygons per hero). ⚠️ Long robes (um_arabiyat, hamada): the leg turns at the knee just
            // above the hem, not at the hip -- from the hip the whole hidden shin swung out beside the robe like a plank.
            ["rima"] = new[]
            {
                new Part("shorts", null, 0.4817f, 0.5635f, 0.190f, 5.760f, 4),
                new Part("torso", "shorts", 0.4815f, -0.0211f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.5852f, 0.1020f, -0.200f, 4.750f, 3),
                new Part("arm_far", "torso", 0.5856f, 0.8768f, -1.700f, 3.800f, 5),
                new Part("arm_near", "torso", 0.2183f, 0.8614f, 1.100f, 3.900f, 7),
                new Part("leg_far", "shorts", 0.7018f, 0.9630f, -0.800f, -0.350f, 1),
                new Part("leg_near", "shorts", 0.2484f, 0.9644f, 0.800f, -0.350f, 2),
            },
            ["zaatar"] = new[]
            {
                new Part("shorts", null, 0.4849f, 0.6809f, -0.015f, 6.770f, 4),
                new Part("torso", "shorts", 0.4437f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.4747f, 0.1070f, -0.100f, 4.400f, 3),
                new Part("arm_far", "torso", 0.6032f, 0.8851f, -1.800f, 3.200f, 5),
                new Part("arm_near", "torso", 0.2772f, 0.8761f, 0.900f, 3.400f, 7),
                new Part("leg_far", "shorts", 0.6901f, 0.9666f, -0.800f, -0.700f, 1),
                new Part("leg_near", "shorts", 0.2805f, 0.9682f, 0.800f, -0.700f, 2),
            },
            ["fulla"] = new[]
            {
                new Part("shorts", null, 0.5022f, 0.9128f, -0.125f, 8.320f, 4),
                new Part("torso", "shorts", 0.6486f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.5348f, 0.1008f, 0.200f, 2.600f, 3),
                new Part("arm_far", "torso", 0.6983f, 0.8462f, -1.200f, 0.500f, 5),
                new Part("arm_near", "torso", 0.2239f, 0.8929f, 1.000f, 1.950f, 7),
                new Part("leg_far", "shorts", 0.7570f, 0.9723f, -0.700f, -0.950f, 1),
                new Part("leg_near", "shorts", 0.2553f, 0.9736f, 0.900f, -0.950f, 2),
            },
            ["toni"] = new[]
            {
                new Part("shorts", null, 0.4770f, 0.7573f, 0.015f, 6.740f, 4),
                new Part("torso", "shorts", 0.4606f, -0.0117f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.5232f, 0.1277f, -0.050f, 4.250f, 3),
                new Part("arm_far", "torso", 0.6305f, 0.8767f, -1.750f, 3.250f, 5),
                new Part("arm_near", "torso", 0.2140f, 0.9020f, 0.950f, 3.550f, 7),
                new Part("leg_far", "shorts", 0.6653f, 0.9642f, -0.850f, -1.050f, 1),
                new Part("leg_near", "shorts", 0.3268f, 0.9661f, 0.950f, -1.050f, 2),
            },
            ["um_arabiyat"] = new[]
            {
                new Part("shorts", null, 0.4921f, 0.9332f, -0.040f, 8.170f, 4),
                new Part("torso", "shorts", 0.5751f, 0.0000f, 0.000f, 0.000f, 6),
                new Part("head", "torso", 0.6405f, 0.1321f, 0.600f, 3.100f, 3),
                new Part("arm_far", "torso", 0.6749f, 0.8849f, -1.700f, 1.800f, 5),
                new Part("arm_near", "torso", 0.1569f, 0.8743f, 1.000f, 1.900f, 7),
                new Part("leg_far", "shorts", 0.7455f, 0.9361f, -0.800f, -5.000f, 1),
                new Part("leg_near", "shorts", 0.1628f, 0.9408f, 1.000f, -5.000f, 2),
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
        static readonly Dictionary<string, float> Heights = new Dictionary<string, float> { ["abu_shahata"] = 14.39f, ["rima"] = 14.49f, ["zaatar"] = 14.6f, ["fulla"] = 14.5f, ["toni"] = 14.62f, ["um_arabiyat"] = 14.5f, ["hamada"] = 14.54f, ["bubakr"] = 14.39f };

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
        Vector3 rootRest; float rigScale = 1f;
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
            float armFar = 1.5f * breathe - walk * (6f + 6f * sn), armNear = -1.5f * breathe + walk * (6f - 6f * sn);
            float torso = walk * 2f * c + breathe * 0.6f, head = -breathe * 1.2f + walk * 1.5f * sn;
            float bob = walk * 0.16f * Mathf.Abs(c);

            if (kickT >= 0f)
            {
                // the near leg kicks: back, snap through the ball, follow-through, settle
                kickT += dt;
                float k = kickT / KickSeconds, leg;
                if (k < 0.25f) leg = Mathf.Lerp(0f, -22f, k / 0.25f);
                else if (k < 0.5f) { float q = (k - 0.25f) / 0.25f; leg = Mathf.Lerp(-22f, 36f, q * q); }
                else leg = Mathf.Lerp(36f, 0f, Mathf.Clamp01((k - 0.5f) / 0.5f));
                legNear = leg; legFar = -leg * 0.15f;
                armFar = -Mathf.Abs(leg) * 0.35f; armNear = Mathf.Abs(leg) * 0.28f; torso = -leg * 0.08f; head = leg * 0.04f;
                bob = Mathf.Max(0f, Mathf.Sin(Mathf.Clamp01(k * 2f - 0.6f) * Mathf.PI)) * 0.2f;
                if (kickT >= KickSeconds) kickT = -1f;
            }

            Rot("leg_far", legFar); Rot("leg_near", legNear);
            Rot("arm_far", armFar); Rot("arm_near", armNear);
            Rot("torso", torso); Rot("head", head);
            bones["shorts"].localPosition = rootRest + new Vector3(0f, bob, 0f);
        }
    }
}
