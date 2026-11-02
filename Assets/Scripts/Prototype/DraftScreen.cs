using Sheytoonak.Core.Duel;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// The ban/pick screen before every duel (user, 2026-11-01). Cards for every hero in the pool with the
    /// poison and the antidote; the current step says what to do; the opponent's picks stay hidden ("؟")
    /// until the duel starts, its bans are public because they remove the hero for both.
    /// </summary>
    public class DraftScreen : MonoBehaviour
    {
        public DuelManager duel;
        public TouchKickInput touch;

        void OnGUI()
        {
            var d = duel.Draft;
            if (d == null || duel.Rules != null) return;
            float u = Mathf.Max(1f, Screen.height / 900f);
            var font = Ar.Font;
            var label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(22 * u), normal = { textColor = Color.white }, font = font, alignment = TextAnchor.MiddleCenter };
            var small = new GUIStyle(label) { fontSize = Mathf.RoundToInt(17 * u) };
            var big = new GUIStyle(label) { fontSize = Mathf.RoundToInt(38 * u), fontStyle = FontStyle.Bold };
            var card = new GUIStyle(GUI.skin.button) { font = font, fontSize = Mathf.RoundToInt(20 * u), wordWrap = true };

            var old = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = old;

            float pad = 16 * u;
            GUI.Label(new Rect(0, pad, Screen.width, 50 * u), Ar.T(Ar.DraftTitle), big);
            string step = d.Done ? "" : d.Current.Who != duel.Me ? Ar.DraftWait : d.Current.What == DraftRules.Kind.Ban ? Ar.DraftBanYou : Ar.DraftPickYou;
            GUI.Label(new Rect(0, pad + 52 * u, Screen.width, 36 * u), Ar.T(step),
                new GUIStyle(label) { normal = { textColor = d.Current.Who == duel.Me ? new Color(0.6f, 1f, 0.6f) : new Color(1f, 0.8f, 0.6f) } });

            // the pool, right to left
            var all = StarterHeroes.All;
            int n = all.Length;
            float cw = Mathf.Min(260 * u, (Screen.width - pad * (n + 1)) / n), ch = 190 * u;
            float total = n * cw + (n - 1) * pad, x0 = (Screen.width + total) * 0.5f - cw, y0 = pad + 104 * u;
            for (int i = 0; i < n; i++)
            {
                var h = all[i];
                var r = new Rect(x0 - i * (cw + pad), y0, cw, ch);
                bool banned = d.Banned.Contains(h.Id);
                bool mine = d.Picked[(int)duel.Me].Contains(h.Id);
                bool myTurn = !d.Done && d.Current.Who == duel.Me;
                bool can = myTurn && (d.Current.What == DraftRules.Kind.Ban ? d.CanBan(duel.Me, h.Id) : d.CanPick(duel.Me, h.Id));
                GUI.enabled = can;
                var c = GUI.backgroundColor;
                GUI.backgroundColor = banned ? new Color(0.4f, 0.1f, 0.1f) : mine ? new Color(0.3f, 0.55f, 1f) : Color.white;
                if (GUI.Button(r, "", card)) { touch?.IgnoreFor(0.5f); duel.DraftAct(h.Id); }
                GUI.backgroundColor = c; GUI.enabled = true;
                var name = h.NameAr.Split(' ');
                GUI.Label(new Rect(r.x, r.y + 8 * u, r.width, 30 * u), Ar.T(name[0] + (name.Length > 1 ? " " + name[1] : "")), new GUIStyle(label) { fontStyle = FontStyle.Bold });
                GUI.Label(new Rect(r.x, r.y + 44 * u, r.width, 26 * u), Ar.T(Ar.PoisonWord + ": " + h.Poison.NameAr), new GUIStyle(small) { normal = { textColor = new Color(1f, 0.7f, 0.7f) } });
                GUI.Label(new Rect(r.x, r.y + 72 * u, r.width, 26 * u), Ar.T(Ar.AntidoteWord + ": " + h.Antidote.NameAr), new GUIStyle(small) { normal = { textColor = new Color(0.7f, 1f, 0.75f) } });
                if (banned) GUI.Label(new Rect(r.x, r.y + ch - 50 * u, r.width, 36 * u), Ar.T(Ar.Banned), new GUIStyle(label) { fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.4f, 0.4f) } });
                else if (mine) GUI.Label(new Rect(r.x, r.y + ch - 50 * u, r.width, 36 * u), Ar.T(Ar.Mine), new GUIStyle(label) { fontStyle = FontStyle.Bold });
            }

            // the two teams so far
            float ty = y0 + ch + 30 * u;
            string me = ""; foreach (var p in d.Picked[(int)duel.Me]) me += StarterHeroes.Get(p).NameAr.Split(' ')[0] + "   ";
            string them = ""; for (int i = 0; i < d.Picked[1 - (int)duel.Me].Count; i++) them += "؟   ";
            GUI.Label(new Rect(Screen.width * 0.5f, ty, Screen.width * 0.5f - pad, 34 * u), Ar.T(Ar.Mine + ": " + me), new GUIStyle(label) { alignment = TextAnchor.MiddleRight });
            GUI.Label(new Rect(pad, ty, Screen.width * 0.5f - pad, 34 * u), Ar.T(Ar.TheirPicks + ": " + them), new GUIStyle(label) { alignment = TextAnchor.MiddleLeft });
        }
    }
}
