using NUnit.Framework;
using Sheytoonak.Core.Duel;

namespace Sheytoonak.Tests
{
    public class DuelRulesTests
    {
        static DuelRules Make(int seed = 7, int[] rows = null)
        {
            rows = rows ?? new[] { 3, 3 };
            int n = 0; foreach (int r in rows) n += r;
            var shards = new int[n]; for (int i = 0; i < n; i++) shards[i] = 40;
            var h = StarterHeroes.All;
            return new DuelRules(rows, shards, new[] { h[0], h[1], h[2] }, new[] { h[3], h[4], h[0] }, seed);
        }

        // User (2026-11-13): «تایم کلی بازی ۴ دقیقه تنظیم شه و ما تایمر رو هم ببینیم».
        [Test]
        public void The_match_clock_is_four_minutes_and_time_up_ends_the_duel()
        {
            Assert.AreEqual(240, DuelRules.MatchSeconds);
            Assert.AreEqual(20, DuelRules.TurnSeconds);
            Assert.Greater(DuelRules.MaxTurns * DuelRules.TurnSeconds, DuelRules.MatchSeconds, "the turn cap must not end a duel before the clock");
            var r = Make();
            r.Start(Side.Left);
            r.TimeUp();
            Assert.IsTrue(r.Finished);
            Assert.AreEqual("time up", r.FinishReason);
        }

        [Test]
        public void A_timeout_never_ends_a_turn_while_the_ball_is_in_the_air()
        {
            var r = Make();
            r.Start(Side.Left);
            Assert.AreEqual(ActionReject.None, r.BeginKick(Side.Left));
            r.Timeout(Side.Left);
            Assert.AreEqual(Side.Left, r.Current, "the kick is still flying (user 2026-11-07: the ball vanished on the third hit)");
            Assert.IsTrue(r.KickInFlight);
            r.EndKick(Side.Left, -1);
            r.Timeout(Side.Left);
            Assert.AreEqual(Side.Right, r.Current, "once the ball is down a timeout ends the turn as before");
        }

        [Test]
        public void Every_window_is_worth_one_and_an_even_house_seals_one_upper_window()
        {
            // user 2026-11-05: never a tie; user 2026-11-06: «تو هر دست یه پنجره رو نشون بذاریم. هرچی ضربه بزنی نشکنه»
            var h = StarterHeroes.All;
            for (int seed = 1; seed < 30; seed++)
            {
                var odd = new DuelRules(new[] { 3, 3, 3 }, new int[9], new[] { h[0], h[1], h[2] }, new[] { h[3], h[4], h[0] }, seed, new[] { 2, 2, 2, 1, 1, 1, 0, 0, 0 });
                for (int w = 0; w < 9; w++) Assert.AreEqual(1, odd.WindowValue[w]);
                Assert.AreEqual(-1, odd.SealedWindow);
                Assert.AreEqual(5, odd.Majority);

                var even = new DuelRules(new[] { 4, 4 }, new int[8], new[] { h[0], h[1], h[2] }, new[] { h[3], h[4], h[0] }, seed, new[] { 1, 1, 1, 1, 0, 0, 0, 0 });
                int sw = even.SealedWindow;
                Assert.IsTrue(sw >= 0 && sw < 4, "the sealed window sits in an upper row, seed " + seed);
                Assert.AreEqual(0, even.WindowValue[sw]);
                Assert.IsFalse(even.IsBreakable(sw));
                Assert.IsFalse(even.LooksBreakable(sw), "the seal is shown, nobody targets it (review 2026-11-07)");
                Assert.AreEqual(0, even.ShownValue(sw), "shown as sealed (review 2026-11-07)");
                Assert.AreEqual(7, even.TotalValue);
                Assert.AreEqual(1, even.TotalValue % 2);
            }
        }

        [Test]
        public void Opener_has_one_action_then_everyone_has_two()
        {
            var r = Make();
            r.Start(Side.Left);
            Assert.AreEqual(1, r.ActionsLeft);
            Assert.AreEqual(ActionReject.None, r.BeginKick(Side.Left));
            Assert.AreEqual(ActionReject.NotYourTurn, r.BeginKick(Side.Right));
            r.EndKick(Side.Left, -1);
            Assert.AreEqual(Side.Right, r.Current);
            Assert.AreEqual(2, r.ActionsLeft);
        }

        [Test]
        public void Breaking_windows_wins_at_the_majority()
        {
            var r = Make();
            r.Start(Side.Left);
            r.BeginKick(Side.Left); r.EndKick(Side.Left, -1);           // opener burns its single action
            int need = r.Majority, got = 0;
            // Right breaks windows until the majority, Left passes in between
            for (int w = 0; w < r.WindowCount && !r.Finished; w++)
            {
                Assert.AreEqual(Side.Right, r.Current);
                Assert.AreEqual(ActionReject.None, r.BeginKick(Side.Right));
                Assert.AreEqual(KickOutcome.Broke, r.EndKick(Side.Right, w));
                got += r.WindowValue[w];
                if (r.Finished) break;
                Assert.AreEqual(KickOutcome.Bounced, r.EndKickTwice(w), "second ball into the same window gives nothing");
                if (!r.Finished && r.Current == Side.Left) r.Pass(Side.Left);
            }
            Assert.IsTrue(r.Finished);
            Assert.AreEqual(Side.Right, r.Winner);
            Assert.IsTrue(got >= need);
            Assert.AreEqual(r.Shards[0] + r.Shards[1], r.RewardFor(Side.Right).shards, "winner takes every shard of the round");
            Assert.AreEqual(0, r.RewardFor(Side.Left).shards);
            Assert.AreEqual(5, r.RewardFor(Side.Right).curseDelta);
            Assert.AreEqual(-2, r.RewardFor(Side.Left).curseDelta);
            Assert.AreEqual(0, DuelRules.ApplyCurse(1, -2), "curses never go below zero");
        }

        [Test]
        public void A_cover_poison_with_nothing_to_hide_is_refused()
        {
            // user 2026-11-05: the bus grew huge to cover a higher row; now it only reaches the bottom row, and with
            // nothing left there it must be refused, not placed over no window at all
            var r = Make(seed: 3, rows: new[] { 2, 1 });
            r.Start(Side.Left); r.BeginKick(Side.Left); r.EndKick(Side.Left, -1);
            // the starter bus is one turn since 2026-11-07 (review §2.4); the two-turn mechanic is tested on its own tool
            var bus = new Tool("bus2", "باص", Effect.Cover, 1, Lifetime.TwoTurns);
            Assert.AreEqual(ActionReject.NoTarget, r.CanPoison(Side.Right, bus, new int[0]));
            Assert.AreEqual(ActionReject.None, r.CanPoison(Side.Right, bus, new[] { 0 }));
        }

        [Test]
        public void Poison_cannot_close_every_window_and_expires_by_turns()
        {
            var r = Make(seed: 3, rows: new[] { 2, 1 });
            r.Start(Side.Left); r.BeginKick(Side.Left); r.EndKick(Side.Left, -1);
            // the starter bus is one turn since 2026-11-07 (review §2.4); the two-turn mechanic is tested on its own tool
            var bus = new Tool("bus2", "باص", Effect.Cover, 1, Lifetime.TwoTurns);
            Assert.AreEqual(ActionReject.PoisonWouldCloseEverything, r.CanPoison(Side.Right, bus, new[] { 0, 1, 2 }));
            Assert.AreEqual(ActionReject.None, r.Poison(Side.Right, bus, new[] { 0 }));
            Assert.IsTrue(r.IsCovered(0));
            Assert.IsFalse(r.IsBreakable(0));
            r.Pass(Side.Right);                     // Right's turn ends: 2 → 1
            Assert.AreEqual(Side.Left, r.Current);
            Assert.IsTrue(r.IsCovered(0), "two-turn poison still covers during the opponent's turn");
            Assert.AreEqual(KickOutcome.Bounced, r.BeginAndEnd(Side.Left, 0), "a covered window does not break");
            r.Pass(Side.Left);                      // still Right's own next turn
            Assert.IsTrue(r.IsCovered(0), "two turns = the opponent's and the poisoner's own next turn");
            r.Pass(Side.Right);
            Assert.IsFalse(r.IsCovered(0), "gone afterwards");
        }

        [Test]
        public void Antidote_lifts_an_until_antidote_poison_and_shields_modifiers()
        {
            var r = Make();
            r.Start(Side.Left); r.BeginKick(Side.Left); r.EndKick(Side.Left, -1);
            var wind = StarterHeroes.Get("fulla").Poison;      // accuracy, one turn
            var open = new Tool("test_until", "x", Effect.Close, 1, Lifetime.UntilAntidote); // close, until antidote (no starter has one any more)
            r.Poison(Side.Right, wind);
            r.Poison(Side.Right, open, new[] { 4 });
            Assert.AreEqual(Side.Left, r.Current);
            Assert.AreEqual(1, r.ModifiersFor(Side.Left).Accuracy, "the wind blows on the stage");
            Assert.AreEqual(1, r.ModifiersFor(Side.Right).Accuracy, "for both sides");
            r.Antidote(Side.Left, StarterHeroes.Get("zaatar").Antidote); // vane: accuracy 1
            Assert.AreEqual(0, r.ModifiersFor(Side.Left).Accuracy, "the antidote shields its user");
            Assert.AreEqual(1, r.ModifiersFor(Side.Right).Accuracy, "not the other side");
            Assert.IsTrue(r.IsClosed(4));
            r.Antidote(Side.Left, StarterHeroes.Get("fulla").Antidote);  // pebble: close 1
            Assert.IsFalse(r.IsClosed(4), "an until-antidote poison is lifted by a matching antidote");
        }

        [Test]
        public void The_neighbour_shuts_her_window_after_two_turns()
        {
            var r = Make();
            r.Start(Side.Left);
            r.Poison(Side.Left, new Tool("open_window2", "الجار فتح الشباك", Effect.Close, 1, Lifetime.TwoTurns), new[] { 4 });
            Assert.IsTrue(r.IsClosed(4));
            r.Pass(Side.Left); r.Pass(Side.Right);
            Assert.IsTrue(r.IsClosed(4), "still there during the next two turns");
            r.Pass(Side.Left);
            Assert.IsFalse(r.IsClosed(4), "gone after two turns: the duel can always end");
        }

        [Test]
        public void Breaking_the_last_open_window_lifts_the_poisons_that_block_the_rest()
        {
            var r = Make();
            r.Start(Side.Left);
            int n = r.WindowCount;
            // cover every window but two, then let the opponent break one and Left break the last open one
            var covered = new System.Collections.Generic.List<int>();
            for (int i = 2; i < n; i++) covered.Add(i);
            Assert.AreEqual(ActionReject.None, r.Poison(Side.Left, StarterHeroes.Get("zaatar").Poison, covered.ToArray()));
            r.Pass(Side.Left);
            r.BeginKick(Side.Right); r.EndKick(Side.Right, 0);
            if (!r.Finished && r.Current == Side.Right) r.Pass(Side.Right);
            if (r.Finished) return; // a tiny house can end on majority first: nothing to check
            r.BeginKick(Side.Left); r.EndKick(Side.Left, 1);
            if (r.Finished) return;
            int open = 0;
            for (int i = 0; i < n; i++) if (r.IsBreakable(i)) open++;
            Assert.Greater(open, 0, "there is always something to break");
        }

        [Test]
        public void A_window_antidote_clears_the_opponents_window_poison()
        {
            // user (2026-11-09): «دکمه پادزهر سنگ ریزه کاری داره نمی‌کنه در بازی و اثر نداره»
            var r = Make();
            r.Start(Side.Left); r.BeginKick(Side.Left); r.EndKick(Side.Left, -1);
            r.Poison(Side.Right, StarterHeroes.Get("rima").Poison, new[] { 4 }); // the neighbour opens window 4 (close, one turn)
            if (r.Current == Side.Right) r.Pass(Side.Right);
            Assert.AreEqual(Side.Left, r.Current);
            Assert.IsTrue(r.IsClosed(4));
            Assert.AreEqual(ActionReject.None, r.Antidote(Side.Left, StarterHeroes.Get("fulla").Antidote)); // pebble: close 1
            Assert.IsFalse(r.IsClosed(4), "the pebble clears the neighbour off the window at once");
        }

        [Test]
        public void An_antidote_lifts_a_poison_one_level_up_but_not_two()
        {
            // levels (SCENARIO §8, 2026-11-09): upgrading the antidote has to matter
            foreach (int poisonLevel in new[] { 2, 3 })
            {
                var r = Make();
                r.Start(Side.Left); r.BeginKick(Side.Left); r.EndKick(Side.Left, -1);
                var p = StarterHeroes.Get("rima").Poison; p.Power = poisonLevel;
                r.Poison(Side.Right, p, new[] { 4 });
                if (r.Current == Side.Right) r.Pass(Side.Right);
                int before = r.ActionsLeft;
                var a = StarterHeroes.Get("fulla").Antidote; a.Power = 1;
                var got = r.Antidote(Side.Left, a);
                if (poisonLevel == 2) { Assert.AreEqual(ActionReject.None, got); Assert.IsFalse(r.IsClosed(4)); }
                else { Assert.AreEqual(ActionReject.TooWeak, got); Assert.IsTrue(r.IsClosed(4)); Assert.AreEqual(before, r.ActionsLeft, "a refused antidote costs nothing"); }
            }
        }

        [Test]
        public void Higher_levels_hit_harder_but_never_kill_the_kick()
        {
            Assert.AreEqual(0.15f, KickModifiers.Intercept(1), 1e-5);
            Assert.AreEqual(0.55f, KickModifiers.Intercept(5), 1e-5);
            Assert.Greater(KickModifiers.StrengthLeft(5), 0.4f, "a level-5 puddle still lets the ball reach the wall");
            for (int i = 1; i <= 5; i++) Assert.Less(KickModifiers.StrengthLeft(i), KickModifiers.StrengthLeft(i - 1));
            Assert.AreEqual(new[] { 1, 1, 2, 2, 3 }, new[] { KickModifiers.CloseCount(1), KickModifiers.CloseCount(2), KickModifiers.CloseCount(3), KickModifiers.CloseCount(4), KickModifiers.CloseCount(5) });
        }

        [Test]
        public void A_window_antidote_with_nothing_to_clear_does_not_spend_the_action()
        {
            var r = Make();
            r.Start(Side.Left);
            int before = r.ActionsLeft;
            Assert.AreEqual(ActionReject.NoTarget, r.Antidote(Side.Left, StarterHeroes.Get("fulla").Antidote));
            Assert.AreEqual(before, r.ActionsLeft);
        }

        [Test]
        public void No_rage_in_the_game_every_turn_is_two_actions()
        {
            // user (2026-11-09): «آتیش قدرت … برش داریم کلا و کاری نکنه همیشه دو تا دو تا ضربه بزنن»
            var r = Make();
            r.Start(Side.Left);
            for (int i = 0; i < 8; i++) { r.Pass(Side.Left); r.Pass(Side.Right); }
            Assert.IsFalse(r.RageReady[0]);
            Assert.AreEqual(2, r.ActionsLeft);
        }

        [Test]
        public void Rage_arrives_every_third_turn_and_restarts_when_spent()
        {
            var r = Make();
            r.RageEnabled = true; // off in the game since 2026-11-09; the mechanism is kept
            r.Start(Side.Left);
            Assert.IsFalse(r.RageReady[0]);
            r.Pass(Side.Left); r.Pass(Side.Right);   // L turn 2 → timer 2
            r.Pass(Side.Left); r.Pass(Side.Right);   // L turn 3 → timer 1
            r.Pass(Side.Left); r.Pass(Side.Right);   // L turn 4 → timer 0 → ready
            Assert.IsTrue(r.RageReady[0]);
            Assert.AreEqual(2, r.ActionsLeft);
            Assert.AreEqual(ActionReject.None, r.UseRage(Side.Left));
            Assert.AreEqual(3, r.ActionsLeft, "rage is one extra action");
            Assert.AreEqual(ActionReject.NoRage, r.UseRage(Side.Left));
            r.Pass(Side.Left); r.Pass(Side.Right); r.Pass(Side.Left); r.Pass(Side.Right);
            Assert.IsFalse(r.RageReady[0], "three own turns after spending it");
        }

        [Test]
        public void Two_timeouts_in_a_row_surrender()
        {
            var r = Make();
            r.Start(Side.Left);
            r.Timeout(Side.Left);
            Assert.AreEqual(Side.Right, r.Current);
            r.Pass(Side.Right);
            r.Timeout(Side.Left);
            Assert.IsTrue(r.Finished);
            Assert.AreEqual(Side.Right, r.Winner);
            Assert.AreEqual("surrender", r.FinishReason);
        }

        [Test]
        public void Bot_cures_then_kicks_then_poisons()
        {
            var r = Make(seed: 5);
            r.Start(Side.Left); r.BeginKick(Side.Left); r.EndKick(Side.Left, -1);
            r.Poison(Side.Right, StarterHeroes.Get("fulla").Poison); // Right poisons itself... it is Right's turn: wind on stage
            // now it is still Right's turn with one action; the bot (Right) should kick, not poison again
            var bot = new DuelBot(1);
            bot.NewTurn();
            var m = bot.Next(r, Side.Right);
            Assert.AreEqual(BotMove.Kind.Kick, m.What, "with one action left it kicks");
            Assert.IsTrue(r.IsBreakable(m.Window));
        }
    }

    static class DuelTestExt
    {
        public static KickOutcome BeginAndEnd(this DuelRules r, Side s, int window)
        {
            Assert.AreEqual(ActionReject.None, r.BeginKick(s));
            return r.EndKick(s, window);
        }
        /// <summary>Second kick of the same turn into an already broken window.</summary>
        public static KickOutcome EndKickTwice(this DuelRules r, int window)
        {
            var s = r.Current;
            if (r.ActionsLeft == 0) return KickOutcome.Bounced;
            Assert.AreEqual(ActionReject.None, r.BeginKick(s));
            return r.EndKick(s, window);
        }
    }
}

namespace Sheytoonak.Tests
{
    using NUnit.Framework;
    using Sheytoonak.Core.Duel;

    public class DraftRulesTests
    {
        static string[] Five() { var a = new string[5]; for (int i = 0; i < 5; i++) a[i] = StarterHeroes.All[i].Id; return a; }

        [Test]
        public void Order_is_ban_ban_pick_pick_pick_pick_pick_pick_and_each_side_gets_three()
        {
            var d = new DraftRules(Side.Left, Five(), Five());
            var k = DraftRules.Kind.Ban; var p = DraftRules.Kind.Pick;
            var expect = new[] { (Side.Left, k), (Side.Right, k), (Side.Right, p), (Side.Left, p), (Side.Left, p), (Side.Right, p), (Side.Right, p), (Side.Left, p) };
            for (int i = 0; i < expect.Length; i++) { Assert.AreEqual(expect[i].Item1, d.Steps[i].Who); Assert.AreEqual(expect[i].Item2, d.Steps[i].What); }
            Assert.IsTrue(d.Act(Side.Left, "toni"));          // ban
            Assert.IsFalse(d.Act(Side.Left, "rima"), "not Left's step");
            Assert.IsTrue(d.Act(Side.Right, "rima"));         // ban
            Assert.IsFalse(d.Act(Side.Right, "toni"), "banned for both");
            Assert.IsTrue(d.Act(Side.Right, "zaatar"));
            Assert.IsTrue(d.Act(Side.Left, "zaatar"), "both may field the same hero");
            Assert.IsTrue(d.Act(Side.Left, "fulla"));
            Assert.IsTrue(d.Act(Side.Right, "fulla"));
            Assert.IsTrue(d.Act(Side.Right, "abu_shahata"));
            Assert.IsTrue(d.Act(Side.Left, "abu_shahata"));
            Assert.IsTrue(d.Done);
            Assert.AreEqual(3, d.Picked[0].Count); Assert.AreEqual(3, d.Picked[1].Count);
        }

        [Test]
        public void A_blind_ban_may_hit_a_hero_nobody_owns_and_the_bot_always_finishes()
        {
            var pool = new System.Collections.Generic.List<string>(Five()) { "someone_else" };
            for (int seed = 1; seed < 30; seed++)
            {
                var d = new DraftRules(Side.Right, Five(), Five());
                var rng = new System.Random(seed);
                int guard = 0;
                while (!d.Done && guard++ < 20)
                {
                    var s = d.Current.Who;
                    var h = d.BotChoice(s, StarterHeroes.Get, pool.ToArray(), rng);
                    Assert.IsNotNull(h);
                    Assert.IsTrue(d.Act(s, h));
                }
                Assert.IsTrue(d.Done, "seed " + seed);
                Assert.AreEqual(3, d.Picked[0].Count); Assert.AreEqual(3, d.Picked[1].Count);
            }
        }
    }
}
