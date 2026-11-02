using System.Collections;
using System.Collections.Generic;
using Sheytoonak.Core.Duel;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// The shared stage seen from behind (SCENARIO v1.3): my three heroes bottom-left, the opponent's
    /// bottom-right, the hero of the move steps forward to the ball and back. Placeholder bodies until the
    /// painted heroes arrive; poisons appear as simple props on the facade / the street.
    /// </summary>
    public class DuelStage : MonoBehaviour
    {
        public DuelManager duel;
        public BackdropWindowMapper mapper;
        public PlayerFoot foot;
        public Material leftMat, rightMat, propMat, shutterMat, puddleMat;

        public readonly Transform[][] Heroes = new Transform[2][];
        readonly Vector3[][] homes = new Vector3[2][];
        readonly Dictionary<ActivePoison, List<GameObject>> props = new Dictionary<ActivePoison, List<GameObject>>();
        GameObject pigeon;

        void Start()
        {
            for (int side = 0; side < 2; side++)
            {
                Heroes[side] = new Transform[3];
                homes[side] = new Vector3[3];
                float sign = side == 0 ? -1f : 1f;
                for (int i = 0; i < 3; i++)
                {
                    // smaller and a little ahead of the ball so the facade stays the centre of the picture (run 26 had them filling the frame)
                    float x = sign * (1.1f + i * 0.7f);
                    var pos = new Vector3(x, 0f, foot.Z + 0.9f - i * 0.25f);
                    homes[side][i] = pos;
                    Heroes[side][i] = MakeHero(side == 0 ? leftMat : rightMat, pos, (side == 0 ? "L" : "R") + i).transform;
                }
            }
            duel.DuelStarted += OnDuelStarted;
            duel.KickLaunched += OnKick;
        }

        void OnDestroy()
        {
            if (duel == null) return;
            duel.DuelStarted -= OnDuelStarted;
            duel.KickLaunched -= OnKick;
        }

        GameObject MakeHero(Material mat, Vector3 pos, string name)
        {
            var root = new GameObject("Hero " + name);
            root.transform.position = pos;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            body.transform.localScale = new Vector3(0.34f, 0.55f, 0.34f);
            body.GetComponent<Renderer>().sharedMaterial = mat;
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(head.GetComponent<Collider>());
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.22f, 0f);
            head.transform.localScale = Vector3.one * 0.26f;
            head.GetComponent<Renderer>().sharedMaterial = mat;
            return root;
        }

        void OnDuelStarted()
        {
            foreach (var kv in props) foreach (var go in kv.Value) if (go != null) Destroy(go);
            props.Clear();
            duel.Rules.PoisonPlaced += OnPoison;
            duel.Rules.PoisonExpired += OnExpired;
            duel.Rules.AntidoteUsed += OnAntidote;
            duel.Rules.WindowTaken += (s, w, v) => Refresh();
        }

        void OnKick(Side side, int target)
        {
            StartCoroutine(StepForward(Heroes[(int)side][0], side));
            if (duel.Rules.ModifiersFor(side).Chance > 0) StartCoroutine(PigeonCrossing());
        }

        IEnumerator StepForward(Transform hero, Side side)
        {
            Vector3 home = homes[(int)side][0];
            Vector3 at = foot.StrikeSpot + new Vector3(side == Side.Left ? -0.4f : 0.4f, 0f, -0.3f);
            for (float k = 0f; k < 1f; k += Time.deltaTime * 4f) { hero.position = Vector3.Lerp(home, at, k); yield return null; }
            hero.position = at;
            yield return new WaitForSeconds(1.2f);
            for (float k = 0f; k < 1f; k += Time.deltaTime * 2.5f) { hero.position = Vector3.Lerp(at, home, k); yield return null; }
            hero.position = home;
        }

        IEnumerator PigeonCrossing()
        {
            if (pigeon == null) pigeon = PoisonProps.Pigeon();
            pigeon.SetActive(true);
            float z = mapper.FacadeZ - 4f, y = 2.8f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.1f)
            {
                pigeon.transform.position = new Vector3(Mathf.Lerp(-7f, 7f, t), y + Mathf.Sin(t * 20f) * 0.15f, z);
                yield return null;
            }
            pigeon.SetActive(false);
        }

        void OnPoison(Side by, ActivePoison p)
        {
            var list = new List<GameObject>();
            switch (p.Tool.Effect)
            {
                case Effect.Cover:
                    if (p.Windows.Length > 0)
                    {
                        var rects = new Rect[p.Windows.Length];
                        for (int i = 0; i < rects.Length; i++) rects[i] = mapper.WindowRects[p.Windows[i]];
                        list.Add(PoisonProps.Bus(this, rects, mapper.FacadeZ));
                    }
                    break;
                case Effect.Close:
                    foreach (int w in p.Windows) list.Add(PoisonProps.OpenWindow(mapper.WindowRects[w], mapper.FacadeZ));
                    break;
                case Effect.Reach:
                    list.Add(PoisonProps.Puddle(foot.StrikeSpot + new Vector3(0f, 0f, 0.1f)));
                    break;
                case Effect.Accuracy:
                    list.Add(PoisonProps.Wind(this, mapper.FacadeZ));
                    break;
                case Effect.Chance:
                    StartCoroutine(PigeonsWhile(p));
                    break;
            }
            props[p] = list;
        }

        /// <summary>The pigeon poison: birds keep crossing the stage while it lasts.</summary>
        IEnumerator PigeonsWhile(ActivePoison p)
        {
            while (duel.Rules != null && duel.Rules.Poisons.Contains(p))
            {
                yield return PigeonCrossing();
                yield return new WaitForSeconds(0.6f);
            }
        }

        void OnAntidote(Side s, Tool a)
        {
            var hero = Heroes[(int)s] != null ? Heroes[(int)s][0] : null;
            var go = PoisonProps.Antidote(a, hero != null ? hero.position : foot.StrikeSpot, mapper.FacadeZ);
            Destroy(go, 2.5f);
        }

        void OnExpired(ActivePoison p)
        {
            if (!props.TryGetValue(p, out var list)) return;
            foreach (var go in list) if (go != null) Destroy(go);
            props.Remove(p);
        }

        /// <summary>A covered window that broke anyway (never happens) or expired props: nothing to do yet.</summary>
        void Refresh() { }

        static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(go.GetComponent<Collider>());
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }
    }
}
