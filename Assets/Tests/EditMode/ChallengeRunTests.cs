using System;
using NUnit.Framework;
using Sheytoonak.Core.Challenge;

namespace Sheytoonak.Tests
{
    public class ChallengeRunTests
    {
        static readonly DateTime T0 = new DateTime(2026, 11, 3, 8, 0, 0, DateTimeKind.Utc);

        [Test]
        public void Seven_wins_pay_the_entry_back_and_prizes_only_grow()
        {
            Assert.AreEqual(100, ChallengeRun.PrizeFor(100, 7));
            for (int w = 1; w <= ChallengeRun.MaxWins; w++)
                Assert.Greater(ChallengeRun.PrizeFor(100, w), ChallengeRun.PrizeFor(100, w - 1), "wins " + w);
            Assert.AreEqual(ChallengeRun.MaxWins + 1, ChallengeRun.PrizeTable.Length);
        }

        [Test]
        public void Three_losses_end_the_run_and_the_prize_is_paid_once()
        {
            var r = new ChallengeRun(100, T0);
            r.Record(true, T0); r.Record(true, T0);
            Assert.AreEqual(0, r.Collect(T0), "not over yet");
            r.Record(false, T0); r.Record(false, T0); r.Record(false, T0);
            Assert.IsTrue(r.IsOver(T0));
            Assert.IsFalse(r.Record(true, T0), "nothing counts after the end");
            Assert.AreEqual(ChallengeRun.PrizeFor(100, 2), r.Collect(T0));
            Assert.AreEqual(0, r.Collect(T0), "paid once");
        }

        [Test]
        public void Twelve_wins_end_it_and_time_runs_out_after_48_hours()
        {
            var r = new ChallengeRun(50, T0);
            for (int i = 0; i < 12; i++) r.Record(true, T0);
            Assert.IsTrue(r.IsOver(T0));
            var late = new ChallengeRun(50, T0);
            late.Record(true, T0);
            Assert.IsFalse(late.IsOver(T0.AddHours(47)));
            Assert.IsTrue(late.IsOver(T0.AddHours(48)));
            Assert.IsFalse(late.Record(true, T0.AddHours(49)));
        }

        [Test]
        public void Survives_a_round_trip_and_rejects_garbage()
        {
            var r = new ChallengeRun(100, T0);
            r.Record(true, T0); r.Record(false, T0);
            var back = ChallengeRun.Parse(r.Serialize());
            Assert.AreEqual(1, back.Wins); Assert.AreEqual(1, back.Losses); Assert.AreEqual(T0, back.StartedUtc);
            Assert.IsNull(ChallengeRun.Parse("x,y")); Assert.IsNull(ChallengeRun.Parse("a,b,c,d,e")); Assert.IsNull(ChallengeRun.Parse(""));
        }
    }
}
