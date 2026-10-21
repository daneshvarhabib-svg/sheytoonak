using NUnit.Framework;
using Sheytoonak.Core;

namespace Sheytoonak.Tests
{
    public class RallyRulesTests
    {
        static RallyRules NewRules(int windows = 3, int houses = 2, int lives = 3) => new RallyRules(lives, houses, windows);

        static void KickToWallAndBounceOnce(RallyRules r)
        {
            r.OnKick();
            r.OnWallHit();
            r.OnGroundBounce();
        }

        [Test]
        public void Serve_kick_goes_out_and_the_wall_sends_it_back()
        {
            var r = NewRules();
            Assert.AreEqual(RallyPhase.Serve, r.Phase);
            Assert.AreEqual(KickRejectReason.None, r.CanKick(true));
            r.OnKick();
            Assert.AreEqual(RallyPhase.Outgoing, r.Phase);
            r.OnWallHit();
            Assert.AreEqual(RallyPhase.Returning, r.Phase);
            Assert.AreEqual(0, r.BouncesSinceWall);
        }

        [Test]
        public void Returning_ball_cannot_be_kicked_before_its_bounce()
        {
            var r = NewRules();
            r.OnKick();
            r.OnWallHit();
            Assert.AreEqual(KickRejectReason.NeedsBounce, r.CanKick(true));
            Assert.Throws<System.InvalidOperationException>(() => r.OnKick());
        }

        [Test]
        public void One_bounce_then_kick_is_legal()
        {
            var r = NewRules();
            KickToWallAndBounceOnce(r);
            Assert.AreEqual(KickRejectReason.None, r.CanKick(true));
            Assert.AreEqual(KickRejectReason.OutOfReach, r.CanKick(false));
            r.OnKick();
            Assert.AreEqual(RallyPhase.Outgoing, r.Phase);
            Assert.AreEqual(3, r.Lives);
        }

        [Test]
        public void Second_bounce_costs_a_life()
        {
            var r = NewRules();
            LossReason? why = null;
            r.LifeLost += (left, reason) => why = reason;
            KickToWallAndBounceOnce(r);
            r.OnGroundBounce();
            Assert.AreEqual(2, r.Lives);
            Assert.AreEqual(LossReason.SecondBounce, why);
            Assert.AreEqual(RallyPhase.Resolving, r.Phase);
            r.OnGroundBounce(); // the dead ball keeps bouncing: no further loss
            Assert.AreEqual(2, r.Lives);
            r.NextServe();
            Assert.AreEqual(RallyPhase.Serve, r.Phase);
        }

        [Test]
        public void Three_losses_end_the_game()
        {
            var r = NewRules();
            bool over = false;
            r.GameOver += () => over = true;
            for (int i = 0; i < 3; i++)
            {
                r.OnKick();
                r.OnBallLost(LossReason.OverRoof);
                if (i < 2) r.NextServe();
            }
            Assert.IsTrue(over);
            Assert.AreEqual(RallyPhase.GameOver, r.Phase);
            Assert.AreEqual(KickRejectReason.NotYourTurn, r.CanKick(true));
        }

        [Test]
        public void Kick_that_keeps_hitting_the_street_is_short()
        {
            var r = new RallyRules(3, 1, 2, maxBouncesBeforeWall: 2);
            r.OnKick();
            r.OnGroundBounce();
            Assert.AreEqual(3, r.Lives);
            r.OnGroundBounce();
            Assert.AreEqual(2, r.Lives);
        }

        [Test]
        public void Breaking_a_window_scores_and_waits_for_the_next_serve()
        {
            var r = NewRules();
            r.OnKick();
            var res = r.OnWindowHit(1, false);
            Assert.AreEqual(WindowHitResult.Broke, res);
            Assert.AreEqual(1, r.Score);
            Assert.IsTrue(r.IsBroken(1));
            Assert.AreEqual(2, r.WindowsLeft);
            Assert.AreEqual(RallyPhase.Resolving, r.Phase);
        }

        [Test]
        public void Already_broken_window_loses_the_ball_unless_the_owner_throws_it_back()
        {
            var r = NewRules();
            r.OnKick();
            r.OnWindowHit(0, false);
            r.NextServe();

            r.OnKick();
            Assert.AreEqual(WindowHitResult.ThrownBack, r.OnWindowHit(0, true));
            Assert.AreEqual(RallyPhase.Returning, r.Phase);
            Assert.AreEqual(1, r.Score);
            Assert.AreEqual(3, r.Lives);

            r.OnGroundBounce();
            r.OnKick();
            Assert.AreEqual(WindowHitResult.Lost, r.OnWindowHit(0, false));
            Assert.AreEqual(2, r.Lives);
            Assert.AreEqual(1, r.Score);
        }

        [Test]
        public void Last_window_gives_a_medal_and_moves_on()
        {
            var r = NewRules(windows: 2, houses: 2);
            int medals = 0;
            r.MedalEarned += m => medals = m;
            r.OnKick(); r.OnWindowHit(0, false); r.NextServe();
            r.OnKick();
            Assert.AreEqual(WindowHitResult.BrokeAndClearedHouse, r.OnWindowHit(1, false));
            Assert.AreEqual(1, medals);
            Assert.AreEqual(RallyPhase.HouseCleared, r.Phase);
            r.AdvanceHouse(4);
            Assert.AreEqual(1, r.HouseIndex);
            Assert.AreEqual(4, r.WindowsLeft);
            Assert.AreEqual(RallyPhase.Serve, r.Phase);
            Assert.AreEqual(2, r.Score); // score carries over
        }

        [Test]
        public void Clearing_the_last_house_wins()
        {
            var r = NewRules(windows: 1, houses: 1);
            bool won = false;
            r.Victory += () => won = true;
            r.OnKick();
            Assert.AreEqual(WindowHitResult.BrokeAndWonGame, r.OnWindowHit(0, false));
            Assert.IsTrue(won);
            Assert.AreEqual(RallyPhase.Victory, r.Phase);
        }

        [Test]
        public void Lives_carry_over_between_houses()
        {
            var r = NewRules(windows: 1, houses: 2);
            r.OnKick(); r.OnBallLost(LossReason.OverRoof); r.NextServe();
            r.OnKick(); r.OnWindowHit(0, false);
            r.AdvanceHouse(3);
            Assert.AreEqual(2, r.Lives);
        }
    }
}
