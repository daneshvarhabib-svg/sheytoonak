using NUnit.Framework;
using Sheytoonak.Core;

namespace Sheytoonak.Tests
{
    public class KickSolverTests
    {
        readonly KickTuning t = new KickTuning();

        float WallHeight(float strength, float distance = 12f)
        {
            var r = KickSolver.Solve(new KickGesture(strength, 0f, 0f), 0f, 1f, t, 0f, 0f);
            return KickSolver.HeightAtDistance(r.Velocity, 0.11f, distance, t.Gravity);
        }

        [Test]
        public void Harder_swing_hits_the_wall_higher()
        {
            float prev = float.MinValue;
            for (int i = 2; i <= 10; i++)
            {
                float h = WallHeight(i / 10f);
                Assert.Greater(h, prev, "strength " + (i / 10f));
                prev = h;
            }
        }

        [Test]
        public void Full_strength_reaches_the_upper_floors_and_can_overshoot()
        {
            Assert.Greater(WallHeight(1f), 10f);
        }

        [Test]
        public void Aim_and_contact_turn_the_ball()
        {
            var right = KickSolver.Solve(new KickGesture(0.5f, 1f, 0f), 0f, 1f, t, 0f, 0f);
            var left = KickSolver.Solve(new KickGesture(0.5f, -1f, 0f), 0f, 1f, t, 0f, 0f);
            Assert.Greater(right.Velocity.X, 0f);
            Assert.Less(left.Velocity.X, 0f);
            var ballRightOfFoot = KickSolver.Solve(new KickGesture(0.5f, 0f, 0f), 0.8f, 1f, t, 0f, 0f);
            Assert.Greater(ballRightOfFoot.Velocity.X, 0f);
        }

        [Test]
        public void Clean_contact_ignores_noise_and_bad_contact_does_not()
        {
            var g = new KickGesture(0.6f, 0f, 0f);
            var a = KickSolver.Solve(g, 0f, 1f, t, 1f, 1f);
            var b = KickSolver.Solve(g, 0f, 1f, t, -1f, -1f);
            Assert.AreEqual(a.Velocity.X, b.Velocity.X, 1e-4f);
            var c = KickSolver.Solve(g, 0f, 0f, t, 1f, 1f);
            Assert.AreNotEqual(0f, c.Velocity.X);
        }

        [Test]
        public void Contact_quality_peaks_at_the_sweet_spot()
        {
            float best = KickSolver.ContactQuality(t.IdealContactHeight, 0f, t);
            Assert.AreEqual(1f, best, 1e-4f);
            Assert.Less(KickSolver.ContactQuality(t.IdealContactHeight + 0.3f, 0f, t), best);
            Assert.Less(KickSolver.ContactQuality(t.IdealContactHeight, 0.9f, t), best);
        }

        [Test]
        public void Ballistic_velocity_lands_on_the_target()
        {
            var from = new Vec3(1f, 3f, 12f);
            var to = new Vec3(-0.5f, 0.11f, 1.8f);
            const float T = 1.4f;
            var v = KickSolver.BallisticVelocity(from, to, T, 9.81f);
            float x = from.X + v.X * T, y = from.Y + v.Y * T - 0.5f * 9.81f * T * T, z = from.Z + v.Z * T;
            Assert.AreEqual(to.X, x, 1e-3f);
            Assert.AreEqual(to.Y, y, 1e-3f);
            Assert.AreEqual(to.Z, z, 1e-3f);
        }
    }

    public class WallAimTests
    {
        static float HeightAtWall(float strength, float aim, float dist, out float x)
        {
            var t = new KickTuning();
            var r = KickSolver.SolveAtWall(new KickGesture(strength, aim, 0f), 0f, 1f, t, 0f, 0f, dist, 0.11f, 6.6f);
            float time = dist / r.Velocity.Z;
            x = r.Velocity.X * time;
            return KickSolver.HeightAtDistance(r.Velocity, 0.11f, dist, t.Gravity);
        }

        [Test]
        public void Bottom_tap_hits_low_and_top_tap_hits_high()
        {
            float low = HeightAtWall(0f, 0f, 13f, out _);
            float mid = HeightAtWall(0.5f, 0f, 13f, out _);
            float high = HeightAtWall(1f, 0f, 13f, out _);
            Assert.AreEqual(1.4f, low, 0.05f);   // MinTargetHeight: below the lowest window row (2.6 m centre)
            Assert.AreEqual(6.6f, high, 0.05f);  // the maxTargetHeight passed in
            Assert.Greater(mid, low); Assert.Less(mid, high);
        }

        [Test]
        public void Aim_moves_the_hit_sideways_and_a_clean_kick_is_exact()
        {
            float h = HeightAtWall(0.5f, 1f, 13f, out float xr);
            HeightAtWall(0.5f, -1f, 13f, out float xl);
            Assert.Greater(xr, 1f); Assert.Less(xl, -1f);
            Assert.AreEqual(xr, -xl, 0.001f);
            // The same tap on a nearer wall still lands at the same height: the control is in facade metres, not launch speed.
            Assert.AreEqual(HeightAtWall(0.5f, 0f, 10f, out _), h, 0.05f);
        }

        [Test]
        public void Bad_contact_scatters_the_hit()
        {
            var t = new KickTuning();
            var g = new KickGesture(0.5f, 0f, 0f);
            var clean = KickSolver.SolveAtWall(g, 0f, 1f, t, 1f, 1f, 13f, 0.11f, 6.6f);
            var bad = KickSolver.SolveAtWall(g, 0f, 0f, t, 1f, 1f, 13f, 0.11f, 6.6f);
            Assert.AreNotEqual(clean.Velocity.X, bad.Velocity.X);
            Assert.AreNotEqual(clean.Velocity.Y, bad.Velocity.Y);
        }
    }

    public class ReachRuleTests
    {
        // Defaults from GameConfig after the first phone test.
        const float ReachX = 0.6f, Behind = 0.5f, Ahead = 2.0f, Head = 1.9f, R = 0.11f;
        static readonly Vec3 Spot = new Vec3(0f, 0f, 0.45f);
        static bool Reach(float x, float y, float z) => ReachRule.InReach(new Vec3(x, y, z), R, Spot, ReachX, Behind, Ahead, Head);

        [Test]
        public void Ball_in_front_of_the_player_is_reachable_all_the_way_in()
        {
            // The bug from the phone: a returning ball at knee or chest height, 0.5 to 2 m ahead,
            // was rejected and sailed past. Every sample along that approach must now be reachable.
            for (float z = 2.4f; z >= 0.0f; z -= 0.2f)
                for (float y = 0.2f; y <= 1.5f; y += 0.4f)
                    Assert.IsTrue(Reach(0.2f, y, z), $"z={z} y={y}");
        }

        [Test]
        public void Ball_over_the_head_or_wide_or_behind_is_a_miss()
        {
            Assert.IsFalse(Reach(0f, 2.3f, 1.0f), "over the head");
            Assert.IsFalse(Reach(1.0f, 0.5f, 1.0f), "too wide");
            Assert.IsFalse(Reach(0f, 0.5f, -0.3f), "already behind the player");
            Assert.IsFalse(Reach(0f, 0.5f, 3.0f), "still too far ahead");
        }
    }

    public class MotionGestureDetectorTests
    {
        // Landscape-left: the phone's long edge is horizontal and gravity points along device -X.
        static readonly Vec3 Upright = new Vec3(-9.81f, 0f, 0f);
        static readonly Vec3 Zero = new Vec3(0f, 0f, 0f);

        /// <summary>Feeds a swing about the landscape kick axis (device Y) of the given amplitude over 0.15 s, then rest.</summary>
        static bool Swing(MotionGestureDetector d, float amplitudeDeg, float aimRate, out KickGesture g)
        {
            const float dt = 1f / 120f;
            float rate = amplitudeDeg * (float)System.Math.PI / 180f / 0.15f;
            g = default;
            for (int i = 0; i < 18; i++)
                if (d.Feed(dt, new Vec3(aimRate, rate, 0f), Zero, Upright, out g)) return true;
            for (int i = 0; i < 30; i++)
                if (d.Feed(dt, Zero, Zero, Upright, out g)) return true;
            return false;
        }

        [Test]
        public void Bigger_swing_means_stronger_kick()
        {
            var tuning = new MotionTuning();
            Assert.IsTrue(Swing(new MotionGestureDetector(tuning), 30f, 0f, out var soft));
            Assert.IsTrue(Swing(new MotionGestureDetector(tuning), 70f, 0f, out var hard));
            Assert.Greater(hard.Strength01, soft.Strength01);
        }

        [Test]
        public void Slow_wobble_is_not_a_kick()
        {
            var d = new MotionGestureDetector(new MotionTuning());
            for (int i = 0; i < 240; i++)
                Assert.IsFalse(d.Feed(1f / 120f, new Vec3(1.0f, 0.3f, 0.2f), Zero, Upright, out _));
        }

        [Test]
        public void Wrong_direction_swing_does_not_kick()
        {
            var d = new MotionGestureDetector(new MotionTuning());
            Assert.IsFalse(Swing(d, -60f, 0f, out _));
        }

        [Test]
        public void Sideways_component_aims()
        {
            Assert.IsTrue(Swing(new MotionGestureDetector(new MotionTuning()), 50f, 2.0f, out var g));
            Assert.Greater(g.Aim, 0f);
        }

        [Test]
        public void Cooldown_blocks_a_double_trigger()
        {
            var d = new MotionGestureDetector(new MotionTuning());
            Assert.IsTrue(Swing(d, 50f, 0f, out _));
            // Swing() returns on the sample that ends the kick, so the 0.45 s cooldown is still running.
            const float dt = 1f / 120f;
            bool kicked = false;
            for (int i = 0; i < 18; i++) kicked |= d.Feed(dt, new Vec3(0f, 6f, 0f), Zero, Upright, out _);
            Assert.IsFalse(kicked);
        }

        [Test]
        public void Tilt_moves_the_player()
        {
            var t = new MotionTuning { Lateral = LateralMode.Tilt };
            var d = new MotionGestureDetector(t);
            d.Calibrate(Upright);
            // Landscape: one end of the phone dips by ~20 degrees, so gravity gains a +Y part.
            // The default LateralSign (-1, measured on the user's phone) sends the player left for this.
            var tilted = new Vec3(-9.81f * 0.94f, 9.81f * 0.342f, 0f);
            d.Feed(1f / 60f, Zero, Zero, tilted, out _);
            Assert.Less(d.Lateral, -0.5f);
            d = new MotionGestureDetector(new MotionTuning { Lateral = LateralMode.Tilt, LateralSign = 1f });
            d.Calibrate(Upright);
            d.Feed(1f / 60f, Zero, Zero, tilted, out _);
            Assert.Greater(d.Lateral, 0.5f);
        }

        [Test]
        public void Portrait_mode_still_reads_tilt_and_kick_on_the_old_axes()
        {
            var t = new MotionTuning { Lateral = LateralMode.Tilt, Landscape = false, KickAxis = DeviceAxis.X, LateralSign = 1f };
            var d = new MotionGestureDetector(t);
            var portraitUp = new Vec3(0f, -9.81f, 0f);
            d.Calibrate(portraitUp);
            d.Feed(1f / 60f, Zero, Zero, new Vec3(9.81f * 0.342f, -9.81f * 0.94f, 0f), out _);
            Assert.Greater(d.Lateral, 0.5f);
            const float dt = 1f / 120f;
            float rate = 50f * (float)System.Math.PI / 180f / 0.15f;
            bool kicked = false;
            for (int i = 0; i < 18 && !kicked; i++) kicked = d.Feed(dt, new Vec3(rate, 0f, 0f), Zero, portraitUp, out _);
            for (int i = 0; i < 30 && !kicked; i++) kicked = d.Feed(dt, Zero, Zero, portraitUp, out _);
            Assert.IsTrue(kicked);
        }

        [Test]
        public void Landscape_ignores_a_portrait_style_swing()
        {
            var d = new MotionGestureDetector(new MotionTuning());
            const float dt = 1f / 120f;
            float rate = 50f * (float)System.Math.PI / 180f / 0.15f;
            bool kicked = false;
            for (int i = 0; i < 18; i++) kicked |= d.Feed(dt, new Vec3(rate, 0f, 0f), Zero, Upright, out _);
            for (int i = 0; i < 30; i++) kicked |= d.Feed(dt, Zero, Zero, Upright, out _);
            Assert.IsFalse(kicked);
        }
    }
}
