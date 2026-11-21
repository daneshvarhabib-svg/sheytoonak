using System;
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

        // User (2026-11-14): «مساوی ای که بر اساس زمان بوده مساوی میمونه و هرکسی خرده شیشه های خودش رو بدست میاره ولی فحش نه
        // کم نه زیاد میشه براش».
        [Test]
        public void A_level_duel_on_time_is_a_draw_each_keeps_its_shards_and_curses_stay()
        {
            var r = Make();
            r.Start(Side.Left);
            r.TimeUp();
            Assert.IsTrue(r.Finished);
            Assert.IsTrue(r.Draw);
            Assert.IsNull(r.Winner);
            Assert.AreEqual((r.Shards[0], 0), r.RewardFor(Side.Left));
            Assert.AreEqual((r.Shards[1], 0), r.RewardFor(Side.Right));
        }

        // User (2026-11-21): «۵۳ رو بساز / اگر خواست ماموریت های بیشتر تغییر بده باید ۵ تا تیله بسوزونه».
        [Test]
        public void A_mission_trade_is_free_once_a_day_then_costs_five_marbles()
        {
            var day = new System.DateTime(2026, 11, 21);
            var m = new Sheytoonak.Core.Missions.DailyMissions();
            var before = m.Jobs(day);
            m.Note(before[0].Kind, 1, day);
            Assert.AreEqual(0, m.SwapCost(day));
            Assert.IsTrue(m.Swap(0, day));
            var after = m.Jobs(day);
            Assert.AreNotEqual(before[0].Kind, after[0].Kind);
            Assert.AreNotEqual(after[0].Kind, after[1].Kind); Assert.AreNotEqual(after[0].Kind, after[2].Kind);
            Assert.AreEqual(before[1].Kind, after[1].Kind);
            Assert.AreEqual(0, m.Progress(0, day), "the traded job starts from zero");
            Assert.AreEqual(Sheytoonak.Core.Missions.DailyMissions.SwapMarbles, m.SwapCost(day));
            Assert.AreEqual(5, m.SwapCost(day));
            // it survives a save and stays the same job for the rest of the day
            var back = Sheytoonak.Core.Missions.DailyMissions.Parse(m.Serialize());
            Assert.AreEqual(after[0].Kind, back.Jobs(day)[0].Kind);
            Assert.AreEqual(5, back.SwapCost(day));
            // a finished job cannot be traded
            back.Note(after[1].Kind, 99, day);
            Assert.IsFalse(back.Swap(1, day));
            // an old save (before trades) still loads; tomorrow the trade is gone and free again
            Assert.AreEqual(0, Sheytoonak.Core.Missions.DailyMissions.Parse("20261121,0,1,0,0,0,0,0").SwapCost(day));
            Assert.AreEqual(0, back.SwapCost(day.AddDays(1)));
            Assert.AreEqual(Sheytoonak.Core.Missions.DailyMissions.For(day.AddDays(1))[0].Kind, back.Jobs(day.AddDays(1))[0].Kind);
        }

        // User (2026-11-19): «هر تیله بیست تا خرده شیشه است».
        [Test]
        public void Daily_missions_count_pay_once_and_reset_with_the_day()
        {
            var day = new System.DateTime(2026, 11, 19);
            var jobs = Sheytoonak.Core.Missions.DailyMissions.For(day);
            Assert.AreEqual(3, jobs.Length);
            Assert.AreNotEqual(jobs[0].Kind, jobs[1].Kind); Assert.AreNotEqual(jobs[1].Kind, jobs[2].Kind); Assert.AreNotEqual(jobs[0].Kind, jobs[2].Kind);
            Assert.AreEqual(jobs[0].Kind, Sheytoonak.Core.Missions.DailyMissions.For(day)[0].Kind);
            var m = new Sheytoonak.Core.Missions.DailyMissions();
            Assert.AreEqual(0, m.Claim(0, day));
            Assert.IsFalse(m.AnyReady(day));
            for (int i = 0; i < 3; i++) m.Note(jobs[i].Kind, jobs[i].Goal + 5, day);
            Assert.AreEqual(jobs[1].Goal, m.Progress(1, day));
            Assert.IsTrue(m.AnyReady(day));
            Assert.AreEqual(0, m.ClaimBonus(day), "the three prizes first");
            m = Sheytoonak.Core.Missions.DailyMissions.Parse(m.Serialize());
            for (int i = 0; i < 3; i++) { Assert.AreEqual(jobs[i].Shards, m.Claim(i, day)); Assert.AreEqual(0, m.Claim(i, day)); }
            Assert.AreEqual(Sheytoonak.Core.Missions.DailyMissions.BonusMarbles, m.ClaimBonus(day));
            Assert.AreEqual(0, m.ClaimBonus(day));
            Assert.IsFalse(m.AnyReady(day));
            var next = day.AddDays(1);
            Assert.AreEqual(0, m.Progress(0, next));
            Assert.IsFalse(m.Claimed(0, next));
        }

        [Test]
        public void The_city_map_follows_the_curses_and_pays_each_quarter_once()
        {
            var M = typeof(Sheytoonak.Core.Missions.CityMap);
            Assert.AreEqual(50, Sheytoonak.Core.Missions.CityMap.Quarters);
            Assert.AreEqual(5, Sheytoonak.Core.Missions.CityMap.PerHouse, "one house per win");
            Assert.AreEqual(Sheytoonak.Core.Missions.CityMap.PerHouse, DuelRules.CurseWin);
            // the gates (research p48): past a gate (every 5 quarters) the line never falls behind it
            Assert.AreEqual(620, Sheytoonak.Core.Missions.CityMap.Line(620, 640));
            Assert.AreEqual(500, Sheytoonak.Core.Missions.CityMap.Line(430, 640));
            Assert.AreEqual(430, Sheytoonak.Core.Missions.CityMap.Line(430, 480));
            Assert.AreEqual(0, Sheytoonak.Core.Missions.CityMap.District(499));
            Assert.AreEqual(1, Sheytoonak.Core.Missions.CityMap.District(500));
            Assert.AreEqual(9, Sheytoonak.Core.Missions.CityMap.District(99999));
            Assert.AreEqual(2, Sheytoonak.Core.Missions.CityMap.QuartersHeld(250));
            Assert.AreEqual(10, Sheytoonak.Core.Missions.CityMap.HousesInFront(250));
            Assert.AreEqual(1, Sheytoonak.Core.Missions.CityMap.QuartersHeld(198), "a loss gives ground back");
            Assert.AreEqual(50, Sheytoonak.Core.Missions.CityMap.QuartersHeld(9999));
            Assert.AreEqual(0, Sheytoonak.Core.Missions.CityMap.HousesInFront(5000));
            Assert.AreEqual(2, Sheytoonak.Core.Missions.CityMap.Waiting(250, 0));
            Assert.AreEqual(0, Sheytoonak.Core.Missions.CityMap.Waiting(250, 2), "retaking a quarter pays nothing again");
            Assert.AreEqual(Sheytoonak.Core.Missions.MapPrizeKind.Bag, Sheytoonak.Core.Missions.CityMap.PrizeOf(50).Kind);
            Assert.AreEqual(Sheytoonak.Core.Missions.MapPrizeKind.Shards, Sheytoonak.Core.Missions.CityMap.PrizeOf(1).Kind);
            Assert.AreEqual(Sheytoonak.Core.Missions.MapPrizeKind.Marbles, Sheytoonak.Core.Missions.CityMap.PrizeOf(2).Kind);
            Assert.IsNotNull(M);
        }

        [Test]
        public void Every_money_pack_has_a_star_price()
        {
            Assert.AreEqual(50, Sheytoonak.Core.Bags.ShopCatalog.StarsOf("lira_200"));
            Assert.AreEqual(1000, Sheytoonak.Core.Bags.ShopCatalog.StarsOf("lira_6500"));
            Assert.AreEqual(500, Sheytoonak.Core.Bags.ShopCatalog.StarsOf("special_a"));
            Assert.AreEqual(1000, Sheytoonak.Core.Bags.ShopCatalog.StarsOf("special_a_full"));
            Assert.AreEqual(0, Sheytoonak.Core.Bags.ShopCatalog.StarsOf("nothing"));
            // the jar: half the price of the same marbles in packs, and it never overflows
            Assert.AreEqual(50, Sheytoonak.Core.Bags.ShopCatalog.StarsOf(Sheytoonak.Core.Bags.ShopCatalog.JarSku));
            Assert.AreEqual(Sheytoonak.Core.Bags.ShopCatalog.JarCapacity, 2 * 200 * Sheytoonak.Core.Bags.ShopCatalog.JarCents / 99);
            Assert.AreEqual(Sheytoonak.Core.Bags.ShopCatalog.JarCapacity, Sheytoonak.Core.Bags.ShopCatalog.JarAfterWin(Sheytoonak.Core.Bags.ShopCatalog.JarCapacity - 1));
        }

        [Test]
        public void A_marble_always_breaks_into_twenty_shards()
        {
            Assert.AreEqual(20, Sheytoonak.Core.Bags.ShopCatalog.ShardsPerMarble);
            foreach (var p in Sheytoonak.Core.Bags.ShopCatalog.ShardPacks) Assert.AreEqual(p.Lira * 20, p.Shards);
        }

        // User (2026-11-18): «گوز الان همه موجودات زنده رو بی هوش می‌کنه … مگر این که سم دو پله بالاتر از قدرت گوز باشه».
        [Test]
        public void A_fart_knocks_out_only_the_birds()
        {
            var fart1 = new Tool("fart", "ضرطة", Effect.Chance, 1);
            Tool bus(int lv) => new Tool("bus", "قافلة", Effect.Cover, lv, Lifetime.OneTurn);
            Assert.IsTrue(DuelRules.FartKnocksOut(fart1, new Tool("pigeon", "حمامة", Effect.Chance, 1)));
            Assert.IsFalse(DuelRules.FartKnocksOut(fart1, bus(1)), "2026-11-19: birds only, the caravan walks on");
            Assert.IsFalse(DuelRules.FartKnocksOut(fart1, new Tool("cart", "عربة", Effect.Cover, 1, Lifetime.OneTurn)));

            // the caravan already parked stays when the other side farts
            var r = Make(); r.Start(Side.Right);
            Assert.AreEqual(ActionReject.None, r.Poison(Side.Right, bus(1), new[] { 0 }));
            if (r.Current != Side.Left) r.Pass(Side.Right);
            Assert.IsTrue(r.IsCovered(0));
            r.Antidote(Side.Left, fart1);
            Assert.IsTrue(r.IsCovered(0));
        }

        // User (2026-11-14): «قدرت گوز مگه قرار نبود جلوی پرنده‌ها رو بگیره؟». The stage asks the rules whether a fart is up.
        [Test]
        public void A_fart_shows_as_the_active_protection_against_the_pigeon()
        {
            var r = Make();
            r.Start(Side.Right);
            Assert.IsNull(r.ActiveProtection(Effect.Chance));
            Assert.AreEqual(ActionReject.None, r.Antidote(Side.Right, StarterHeroes.All[4].Antidote));
            Assert.AreEqual("fart", r.ActiveProtection(Effect.Chance));
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
        public void Curse_windows_are_worth_two_and_make_every_total_odd()
        {
            // user 2026-11-16: «پنجره فحش دار دو امتیاز حساب می‌شه»; 2026-11-17: «تعداد فحش هایی که میاد و دو امتیازی میکنه رو
            // درست کن که کل امتیازات هر دست فرد باشه» -- no sealed window when curses are asked for.
            var h = StarterHeroes.All;
            for (int seed = 1; seed < 60; seed++)
                foreach (int ask in new[] { 2, 3, 4 })
                    foreach (var rows in new[] { new[] { 5, 5, 5 }, new[] { 4, 4 }, new[] { 8, 8, 8 } })
                    {
                        int n = 0; foreach (int c in rows) n += c;
                        var r = new DuelRules(rows, new int[n], new[] { h[0], h[1], h[2] }, new[] { h[3], h[4], h[0] }, seed, null, ask);
                        Assert.AreEqual(-1, r.SealedWindow, "curses fix the parity, nothing is sealed");
                        Assert.IsTrue(r.DoubleWindows.Length >= 1 && Math.Abs(r.DoubleWindows.Length - ask) <= 1);
                        int twos = 0; for (int w = 0; w < n; w++) if (r.WindowValue[w] == 2) { twos++; Assert.IsTrue(r.IsDouble(w)); }
                        Assert.AreEqual(r.DoubleWindows.Length, twos);
                        Assert.AreEqual(n + twos, r.TotalValue);
                        Assert.AreEqual(1, r.TotalValue % 2, "the round total is odd: no tie, " + n + " windows asked " + ask);
                    }
            var none = new DuelRules(new[] { 5, 5, 5 }, new int[15], new[] { h[0], h[1], h[2] }, new[] { h[3], h[4], h[0] }, 3);
            Assert.AreEqual(0, none.DoubleWindows.Length, "no doubles unless asked (old callers unchanged)");
        }

        [Test]
        public void Taking_a_double_window_scores_two()
        {
            var h = StarterHeroes.All;
            var r = new DuelRules(new[] { 5, 5, 5 }, new int[15], new[] { h[0], h[1], h[2] }, new[] { h[3], h[4], h[0] }, 5, null, 2);
            int w = r.DoubleWindows[0];
            r.Start(Side.Left);
            Assert.AreEqual(ActionReject.None, r.BeginKick(Side.Left));
            r.EndKick(Side.Left, w);
            Assert.AreEqual(2, r.Windows[(int)Side.Left]);
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
