using System;

namespace Sheytoonak.Core
{
    public enum LateralMode
    {
        /// <summary>Tilt the phone left/right (roll). Most reliable on every phone.</summary>
        Tilt,
        /// <summary>Slide the phone left/right in the air. Feels closest to "move to the ball", but accelerometers drift.</summary>
        Translation,
        /// <summary>Tilt sets the position, a sideways slide gives a short dash on top.</summary>
        Hybrid
    }

    public enum DeviceAxis { X, Y, Z }

    [Serializable]
    public class MotionTuning
    {
        public LateralMode Lateral = LateralMode.Hybrid;
        // Landscape (the game's orientation since 2026-10-23): the phone's long edge is horizontal, so
        // gravity's "down" sits along device X, a left/right tilt rotates about device X, and a
        // sideways slide moves along device Y. Portrait swaps those back. Tests pin both.
        public bool Landscape = true;
        // ⚠️ -1: on the user's phone (2026-10-24, landscape build) tilting right moved the player left:
        // "حرکت به راست و چپ هم جهتش برعکسه". The maths of RollDeg was derived, not measured; this sign is
        // the measurement. Flip it back if another device reports the opposite.
        public float LateralSign = -1f;
        public float TiltFullDeg = 18f;        // roll that maps to the edge of the lane (was 25: less tilt needed = easier)
        public float TiltDeadzoneDeg = 3f;
        public float SlideGain = 0.9f;         // translation: metres-ish of lane per (m/s) of phone speed
        public float SlideDamping = 4f;        // 1/s, kills accelerometer drift
        public float SlideRecenter = 0.6f;     // 1/s, slowly pulls a slide back to centre

        // The kick: the bottom edge of the phone swings forward = rotation about this device axis.
        // The kick: the phone is held in landscape with both hands and both thumbs push its lower long
        // edge forward (user, 2026-10-23). That tips the phone about its long axis, which Unity's
        // gyroscope reports as device Y. In portrait the same tip was device X. The calibration screen
        // still overrides axis and sign per phone.
        public DeviceAxis KickAxis = DeviceAxis.Y;
        public float KickAxisSign = 1f;        // flip on phones/platforms that report the other sign (calibration screen does it)
        public float KickStartRate = 3.0f;     // rad/s that starts a swing
        public float KickEndRate = 1.2f;       // rad/s that ends it
        public float MinSweepDeg = 12f;        // swing amplitude giving strength 0
        public float MaxSweepDeg = 75f;        // swing amplitude giving strength 1
        public float MaxSwingSeconds = 0.4f;
        public float CooldownSeconds = 0.45f;

        public DeviceAxis AimAxis = DeviceAxis.X;  // sideways component of the swing (device X in landscape)
        public float AimSign = 1f;
        public float AimFullDeg = 22f;
        public DeviceAxis CurlAxis = DeviceAxis.Z; // twist of the phone during the swing
        public float CurlSign = 1f;
        public float CurlFullDeg = 30f;
    }

    /// <summary>
    /// Turns raw phone sensor samples into the two controls of the game:
    /// a lateral position (-1..1, move to the ball) and discrete kicks.
    /// Pure C#: feed it samples from Unity's Input System, or synthetic samples in tests.
    /// </summary>
    public sealed class MotionGestureDetector
    {
        readonly MotionTuning t;
        float neutralRollDeg;
        float slideVel, slidePos;
        bool inSwing;
        float sweepDeg, aimDeg, curlDeg, swingTime, cooldown;
        float tiltPos;

        public MotionGestureDetector(MotionTuning tuning) { t = tuning ?? new MotionTuning(); }

        /// <summary>-1 (left edge of the lane) .. 1 (right edge).</summary>
        public float Lateral { get; private set; }
        public bool InSwing => inSwing;
        public float LastRollDeg { get; private set; }

        /// <summary>Remember the current tilt as "straight" (calibration screen, or the Recenter button).</summary>
        public void Calibrate(Vec3 gravity)
        {
            neutralRollDeg = RollDeg(gravity, t.Landscape);
            slideVel = 0f;
            slidePos = 0f;
        }

        /// <param name="dt">Seconds since the previous sample.</param>
        /// <param name="angularVelocity">Device-frame gyroscope, rad/s.</param>
        /// <param name="linearAcceleration">Device-frame acceleration with gravity removed, m/s².</param>
        /// <param name="gravity">Device-frame gravity vector (any length).</param>
        /// <param name="kick">Filled when this sample finished a swing.</param>
        /// <returns>True when a kick happened on this sample.</returns>
        public bool Feed(float dt, Vec3 angularVelocity, Vec3 linearAcceleration, Vec3 gravity, out KickGesture kick)
        {
            kick = default;
            if (dt <= 0f) return false;
            if (cooldown > 0f) cooldown -= dt;

            UpdateLateral(dt, linearAcceleration, gravity);

            float rate = Axis(angularVelocity, t.KickAxis) * t.KickAxisSign;
            if (!inSwing)
            {
                if (cooldown <= 0f && rate > t.KickStartRate)
                {
                    inSwing = true;
                    sweepDeg = aimDeg = curlDeg = swingTime = 0f;
                }
                else return false;
            }

            const float toDeg = (float)(180.0 / Math.PI);
            swingTime += dt;
            if (rate > 0f) sweepDeg += rate * dt * toDeg;
            aimDeg += Axis(angularVelocity, t.AimAxis) * t.AimSign * dt * toDeg;
            curlDeg += Axis(angularVelocity, t.CurlAxis) * t.CurlSign * dt * toDeg;

            if (rate < t.KickEndRate || swingTime >= t.MaxSwingSeconds)
            {
                inSwing = false;
                cooldown = t.CooldownSeconds;
                if (sweepDeg < t.MinSweepDeg * 0.5f) return false; // a twitch, not a kick
                float strength = (sweepDeg - t.MinSweepDeg) / Math.Max(1f, t.MaxSweepDeg - t.MinSweepDeg);
                kick = new KickGesture(
                    KickSolver.Clamp01(strength),
                    KickSolver.Clamp(aimDeg / Math.Max(1f, t.AimFullDeg), -1f, 1f),
                    KickSolver.Clamp(curlDeg / Math.Max(1f, t.CurlFullDeg), -1f, 1f));
                return true;
            }
            return false;
        }

        void UpdateLateral(float dt, Vec3 lin, Vec3 gravity)
        {
            LastRollDeg = RollDeg(gravity, t.Landscape) - neutralRollDeg;
            float r = LastRollDeg;
            if (Math.Abs(r) < t.TiltDeadzoneDeg) r = 0f;
            else r -= Math.Sign(r) * t.TiltDeadzoneDeg;
            tiltPos = KickSolver.Clamp(r / Math.Max(1f, t.TiltFullDeg - t.TiltDeadzoneDeg), -1f, 1f);

            // The swing itself shakes the accelerometer: freeze the slide while kicking.
            if (!inSwing)
            {
                slideVel += (t.Landscape ? lin.Y : lin.X) * dt;
                slideVel *= (float)Math.Exp(-t.SlideDamping * dt);
                slidePos += slideVel * t.SlideGain * dt;
                slidePos -= slidePos * t.SlideRecenter * dt;
                slidePos = KickSolver.Clamp(slidePos, -1f, 1f);
            }

            switch (t.Lateral)
            {
                case LateralMode.Tilt: Lateral = tiltPos; break;
                case LateralMode.Translation: Lateral = slidePos; break;
                default: Lateral = KickSolver.Clamp(tiltPos + slidePos, -1f, 1f); break;
            }
            Lateral *= Math.Sign(t.LateralSign) == 0 ? 1f : Math.Sign(t.LateralSign);
        }

        /// <summary>Roll of a portrait phone: 0 when held upright, positive when the top leans right.</summary>
        /// <summary>Left/right tilt in degrees. Portrait: down is -Y, tilt shows in X. Landscape-left: down is -X, tilt shows in Y.</summary>
        static float RollDeg(Vec3 g, bool landscape)
        {
            if (Math.Abs(g.X) + Math.Abs(g.Y) < 1e-4f) return 0f;
            // Landscape-left: device +Y points to the player's right, so a dip of the right end (gravity gains +Y) is a positive roll.
            double a = landscape ? Math.Atan2(g.Y, -g.X) : Math.Atan2(g.X, -g.Y);
            return (float)(a * 180.0 / Math.PI);
        }

        static float Axis(Vec3 v, DeviceAxis a) => a == DeviceAxis.X ? v.X : (a == DeviceAxis.Y ? v.Y : v.Z);
    }
}
