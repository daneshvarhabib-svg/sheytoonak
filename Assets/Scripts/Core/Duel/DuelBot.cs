using System;
using System.Collections.Generic;

namespace Sheytoonak.Core.Duel
{
    /// <summary>What the bot wants to do next on its turn. The scene performs it (kicks need physics).</summary>
    public struct BotMove
    {
        public enum Kind { Antidote, Kick, Poison, Pass }
        public Kind What;
        public int Hero;        // for Antidote / Poison
        public int Window;      // for Kick (target)
        public int[] Windows;   // for Cover / Close poisons
    }

    /// <summary>
    /// A plain opponent that plays by the same rules, with the ordinary player's dilemmas: cure first,
    /// kick, then poison after the kick (never before, SCENARIO §3). Its aim error is set by the scene.
    /// </summary>
    public sealed class DuelBot
    {
        readonly Random rng;
        public DuelBot(int seed = 0) { rng = seed != 0 ? new Random(seed) : new Random(); }

        public BotMove Next(DuelRules rules, Side me)
        {
            var move = new BotMove { What = BotMove.Kind.Pass, Hero = -1, Window = -1, Windows = new int[0] };
            if (rules.CanAct(me) != ActionReject.None) return move;
            var heroes = rules.Heroes[(int)me];
            var mods = rules.ModifiersFor(me);

            // 1. a matching antidote when something on the stage hurts me and I still have a kick to protect
            if (mods.Any && rules.ActionsLeft >= 2)
                for (int i = 0; i < heroes.Length; i++)
                {
                    var e = heroes[i].Antidote.Effect;
                    if ((e == Effect.Accuracy && mods.Accuracy > 0) || (e == Effect.Reach && mods.Reach > 0) || (e == Effect.Chance && mods.Chance > 0))
                        return new BotMove { What = BotMove.Kind.Antidote, Hero = i, Window = -1, Windows = new int[0] };
                }
            if (rules.ActionsLeft >= 2)
                for (int i = 0; i < heroes.Length; i++)
                    if (heroes[i].Antidote.Effect == Effect.Close && HasPoison(rules, Effect.Close))
                        return new BotMove { What = BotMove.Kind.Antidote, Hero = i, Window = -1, Windows = new int[0] };

            // 2. kick: the most valuable breakable window, with a little whim
            var targets = new List<int>();
            for (int w = 0; w < rules.WindowCount; w++) if (rules.IsBreakable(w)) targets.Add(w);
            bool kicked = rules.ActionsLeft == 1 && !LastActionWasKick; // heuristic: second action after a kick → poison
            if (targets.Count > 0 && !(rules.ActionsLeft == 1 && LastActionWasKick))
            {
                int best = targets[0];
                foreach (int w in targets)
                    if (rules.ShownValue(w) > rules.ShownValue(best) || (rules.ShownValue(w) == rules.ShownValue(best) && rng.NextDouble() < 0.5)) best = w;
                LastActionWasKick = true;
                return new BotMove { What = BotMove.Kind.Kick, Hero = -1, Window = best, Windows = new int[0] };
            }

            // 3. poison after the kick: pick a hero whose poison is legal; cover the opponent's best windows
            LastActionWasKick = false;
            for (int i = 0; i < heroes.Length; i++)
            {
                var p = heroes[i].Poison;
                int[] wins = new int[0];
                if (p.Effect == Effect.Cover || p.Effect == Effect.Close)
                {
                    var sorted = new List<int>(targets);
                    sorted.Sort((a, b) => rules.ShownValue(b).CompareTo(rules.ShownValue(a)));
                    int take = p.Effect == Effect.Close ? 1 : Math.Min(p.Power, sorted.Count);
                    wins = sorted.GetRange(0, Math.Min(take, sorted.Count)).ToArray();
                }
                if (rules.CanPoison(me, p, wins) == ActionReject.None)
                    return new BotMove { What = BotMove.Kind.Poison, Hero = i, Window = -1, Windows = wins };
            }
            return move;
        }

        /// <summary>Remembers whether the previous move this turn was a kick, so the second action becomes a poison.</summary>
        public bool LastActionWasKick { get; set; }
        public void NewTurn() => LastActionWasKick = false;

        static bool HasPoison(DuelRules r, Effect e) { foreach (var p in r.Poisons) if (p.Tool.Effect == e) return true; return false; }
    }
}
