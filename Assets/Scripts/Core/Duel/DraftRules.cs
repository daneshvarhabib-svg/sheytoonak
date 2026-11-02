using System;
using System.Collections.Generic;

namespace Sheytoonak.Core.Duel
{
    /// <summary>
    /// Pre-duel draft (user, 2026-11-01): A bans one hero from the whole pool, blind (it may not even be
    /// in B's collection) and the ban removes it for both sides; B bans one and picks one; A picks two;
    /// B picks two; A picks the last. Each side picks from its own collection, so both may field the same
    /// hero. Every player owns at least five heroes, so two bans always leave three to pick.
    /// </summary>
    public sealed class DraftRules
    {
        public enum Kind { Ban, Pick }
        public struct Step { public Side Who; public Kind What; public Step(Side w, Kind k) { Who = w; What = k; } }

        public readonly Side First;
        public readonly Step[] Steps;
        public readonly string[][] Collection = new string[2][];
        public readonly List<string> Banned = new List<string>();
        public readonly List<string>[] Picked = { new List<string>(), new List<string>() };
        public int StepIndex { get; private set; }
        public bool Done => StepIndex >= Steps.Length;
        public Step Current => Steps[Math.Min(StepIndex, Steps.Length - 1)];

        public event Action<Side, Kind, string> Acted;
        public event Action Finished;

        public DraftRules(Side first, string[] leftCollection, string[] rightCollection)
        {
            First = first;
            Side a = first, b = first == Side.Left ? Side.Right : Side.Left;
            Steps = new[]
            {
                new Step(a, Kind.Ban),
                new Step(b, Kind.Ban), new Step(b, Kind.Pick),
                new Step(a, Kind.Pick), new Step(a, Kind.Pick),
                new Step(b, Kind.Pick), new Step(b, Kind.Pick),
                new Step(a, Kind.Pick)
            };
            Collection[0] = leftCollection; Collection[1] = rightCollection;
            if (leftCollection.Length < 5 || rightCollection.Length < 5) throw new ArgumentException("every player owns at least five heroes");
        }

        /// <summary>Heroes this side may still pick (own collection, not banned, not already picked by itself).</summary>
        public List<string> Pickable(Side s)
        {
            var list = new List<string>();
            foreach (var h in Collection[(int)s]) if (!Banned.Contains(h) && !Picked[(int)s].Contains(h)) list.Add(h);
            return list;
        }

        /// <summary>Anything in the pool that is not banned yet may be banned, owned by anyone or nobody.</summary>
        public bool CanBan(Side s, string hero) => !Done && Current.Who == s && Current.What == Kind.Ban && !Banned.Contains(hero);
        public bool CanPick(Side s, string hero) => !Done && Current.Who == s && Current.What == Kind.Pick && Pickable(s).Contains(hero);

        public bool Act(Side s, string hero)
        {
            if (Done || Current.Who != s) return false;
            if (Current.What == Kind.Ban) { if (!CanBan(s, hero)) return false; Banned.Add(hero); }
            else { if (!CanPick(s, hero)) return false; Picked[(int)s].Add(hero); }
            var k = Current.What;
            StepIndex++;
            Acted?.Invoke(s, k, hero);
            if (Done) Finished?.Invoke();
            return true;
        }

        /// <summary>Plain bot: bans the hero whose poison its own picks could not cure, picks for variety of effects.</summary>
        public string BotChoice(Side s, Func<string, HeroDef> lookup, string[] pool, Random rng)
        {
            if (Done || Current.Who != s) return null;
            if (Current.What == Kind.Ban)
            {
                var options = new List<string>();
                foreach (var h in pool) if (!Banned.Contains(h)) options.Add(h);
                return options.Count == 0 ? null : options[rng.Next(options.Count)];
            }
            var pickable = Pickable(s);
            if (pickable.Count == 0) return null;
            // prefer a hero whose poison effect the side does not have yet
            foreach (var h in pickable)
            {
                var e = lookup(h).Poison.Effect;
                bool have = false;
                foreach (var p in Picked[(int)s]) if (lookup(p).Poison.Effect == e) { have = true; break; }
                if (!have) return h;
            }
            return pickable[rng.Next(pickable.Count)];
        }
    }
}
