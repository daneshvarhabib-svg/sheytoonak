using System;
using NUnit.Framework;
using Sheytoonak.Core.Bags;

namespace Sheytoonak.Tests
{
    public class BagNoticeTests
    {
        // Tehran = UTC + 3:30
        static DateTime Teh(int d, int h, int m = 0) => new DateTime(2026, 11, d, h, m, 0, DateTimeKind.Utc) - BagNotice.Tehran;

        [Test]
        public void Fires_when_the_bag_is_ready_in_the_daytime()
        {
            var now = Teh(11, 12); var ready = Teh(11, 15);
            Assert.AreEqual(ready, BagNotice.When(ready.Ticks, now, null));
        }

        [Test]
        public void Nothing_when_nothing_unlocks_or_it_is_already_ready()
        {
            var now = Teh(11, 12);
            Assert.IsNull(BagNotice.When(0, now, null));
            Assert.IsNull(BagNotice.When(long.MaxValue, now, null));
            Assert.IsNull(BagNotice.When(Teh(11, 11).Ticks, now, null));
        }

        [Test]
        public void Never_between_eleven_at_night_and_nine_in_the_morning()
        {
            var now = Teh(11, 20);
            Assert.AreEqual(Teh(12, 9), BagNotice.When(Teh(11, 23, 30).Ticks, now, null));
            Assert.AreEqual(Teh(12, 9), BagNotice.When(Teh(12, 3).Ticks, now, null));
            Assert.AreEqual(Teh(11, 22, 59), BagNotice.When(Teh(11, 22, 59).Ticks, now, null));
        }

        [Test]
        public void At_most_one_a_day()
        {
            var now = Teh(11, 12);
            Assert.AreEqual(Teh(12, 9), BagNotice.When(Teh(11, 18).Ticks, now, Teh(11, 10)), "one already fired today");
            Assert.AreEqual(Teh(11, 18), BagNotice.When(Teh(11, 18).Ticks, now, Teh(10, 22)), "yesterday's does not count");
        }
    }
}
