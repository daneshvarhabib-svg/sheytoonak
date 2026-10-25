using System;

namespace Sheytoonak.Core
{
    /// <summary>Plain 3D vector so the core stays Unity-free. x = right, y = up, z = toward the facade.</summary>
    public struct Vec3
    {
        public float X, Y, Z;
        public Vec3(float x, float y, float z) { X = x; Y = y; Z = z; }
        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public override string ToString() => "(" + X.ToString("0.00") + ", " + Y.ToString("0.00") + ", " + Z.ToString("0.00") + ")";
    }

    /// <summary>What the motion (or touch) gesture produced.</summary>
    public struct KickGesture
    {
        /// <summary>0..1, from the swing amplitude of the phone.</summary>
        public float Strength01;
        /// <summary>-1 (left) .. 1 (right), from the sideways part of the swing.</summary>
        public float Aim;
        /// <summary>-1..1, from how much the phone twisted during the swing; becomes side spin.</summary>
        public float Curl;

        public KickGesture(float strength01, float aim, float curl)
        {
            Strength01 = strength01; Aim = aim; Curl = curl;
        }
    }

    /// <summary>Tunables for <see cref="KickSolver"/>; mirrored by the Unity GameConfig asset.</summary>
    [Serializable]
    public class KickTuning
    {
        public float MinSpeed = 10f;          // m/s at strength 0
        public float MaxSpeed = 28f;          // m/s at strength 1
        public float MinLoftDeg = 20f;        // launch angle at strength 0
        public float MaxLoftDeg = 55f;        // launch angle at strength 1 (needed to reach upper floors)
        public float StrengthCurve = 1.0f;    // >1 makes soft swings softer
        public float MaxAimYawDeg = 25f;      // yaw at Aim = ±1
        public float ContactYawDeg = 12f;     // yaw added when the foot meets the side of the ball
        public float MaxErrorDeg = 4f;        // random yaw/loft error of a bad contact (7 until 2026-10-24: easier)
        public float SpeedErrorFraction = 0.05f;
        public float MaxSpinRadPerSec = 25f;  // side spin at Curl = ±1
        // Wall-aimed kick (since 2026-10-25, see SolveAtWall): the gesture picks a spot on the facade.
        public float WallFlightTime = 0.8f;       // s from foot to facade; shorter = flatter, faster ball
        public float MinTargetHeight = 1.4f;       // m on the facade at Strength01 = 0 (just below the lowest windows)
        public float IdealContactHeight = 0.35f;   // m above ground: sweet spot after the bounce
        public float ContactHeightTolerance = 0.5f; // m: how far from the sweet spot quality reaches 0
        public float Gravity = 9.81f;
    }

    public struct KickResult
    {
        public Vec3 Velocity;
        /// <summary>Side spin around the vertical axis, rad/s. Positive curls the ball to the right.</summary>
        public float SpinY;
    }

    /// <summary>Pure kick maths: gesture + contact -> launch velocity. Deterministic given the two noise inputs.</summary>
    public static class KickSolver
    {
        const float Deg = (float)(Math.PI / 180.0);

        /// <summary>
        /// 0..1: how clean the contact is. 1 = ball at the sweet-spot height, hit with the middle of the foot.
        /// </summary>
        /// <param name="ballHeight">Ball centre height above the ground at contact, metres.</param>
        /// <param name="contactOffset">-1..1: ball position relative to the foot, divided by reach.</param>
        public static float ContactQuality(float ballHeight, float contactOffset, KickTuning t)
        {
            float h = 1f - Math.Abs(ballHeight - t.IdealContactHeight) / Math.Max(0.01f, t.ContactHeightTolerance);
            float o = 1f - Math.Abs(contactOffset) * 0.6f;
            return Clamp01(Clamp01(h) * Clamp01(o));
        }

        /// <param name="noiseA">Uniform -1..1, drives direction error.</param>
        /// <param name="noiseB">Uniform -1..1, drives speed and loft error.</param>
        public static KickResult Solve(KickGesture g, float contactOffset, float quality, KickTuning t, float noiseA, float noiseB)
        {
            float s = (float)Math.Pow(Clamp01(g.Strength01), Math.Max(0.1f, t.StrengthCurve));
            float bad = 1f - Clamp01(quality);

            float speed = Lerp(t.MinSpeed, t.MaxSpeed, s) * (1f + bad * t.SpeedErrorFraction * Clamp(noiseB, -1f, 1f));
            float loft = (Lerp(t.MinLoftDeg, t.MaxLoftDeg, s) + bad * t.MaxErrorDeg * 0.5f * Clamp(noiseB, -1f, 1f)) * Deg;
            // Ball to the right of the foot = foot strikes its left side = ball goes right.
            float yaw = (Clamp(g.Aim, -1f, 1f) * t.MaxAimYawDeg
                         + Clamp(contactOffset, -1f, 1f) * t.ContactYawDeg
                         + bad * t.MaxErrorDeg * Clamp(noiseA, -1f, 1f)) * Deg;

            float horizontal = speed * (float)Math.Cos(loft);
            return new KickResult
            {
                Velocity = new Vec3(horizontal * (float)Math.Sin(yaw), speed * (float)Math.Sin(loft), horizontal * (float)Math.Cos(yaw)),
                SpinY = Clamp(g.Curl, -1f, 1f) * t.MaxSpinRadPerSec
            };
        }

        /// <summary>
        /// Kick that aims at the facade itself: Strength01 picks a height between
        /// <see cref="KickTuning.MinTargetHeight"/> and <paramref name="maxTargetHeight"/>, Aim picks the
        /// sideways spot, and the ball is launched to arrive there in <see cref="KickTuning.WallFlightTime"/>.
        /// User, 2026-10-25: with speed/loft from the gesture the lowest windows could never be hit
        /// ("پایین‌ترین جای صفحه هم که می‌زنی باز نمی‌خوره به شیشه") and most balls flew over the roof;
        /// mapping the tap straight to a spot on the wall makes the control honest: tap low = hit low.
        /// Contact quality still adds error (bad contact = the spot drifts), the foot still deflects.
        /// </summary>
        /// <param name="wallDistance">Horizontal distance from the ball to the facade plane, m.</param>
        /// <param name="startHeight">Ball centre height at contact, m.</param>
        /// <param name="maxTargetHeight">Facade height at Strength01 = 1 (a little above the top windows).</param>
        public static KickResult SolveAtWall(KickGesture g, float contactOffset, float quality, KickTuning t, float noiseA, float noiseB,
                                             float wallDistance, float startHeight, float maxTargetHeight)
        {
            float s = (float)Math.Pow(Clamp01(g.Strength01), Math.Max(0.1f, t.StrengthCurve));
            float bad = 1f - Clamp01(quality);
            float d = Math.Max(1f, wallDistance);

            float yaw = (Clamp(g.Aim, -1f, 1f) * t.MaxAimYawDeg
                         + Clamp(contactOffset, -1f, 1f) * t.ContactYawDeg
                         + bad * t.MaxErrorDeg * Clamp(noiseA, -1f, 1f)) * Deg;
            float x = d * (float)Math.Tan(yaw);
            float y = Lerp(t.MinTargetHeight, Math.Max(t.MinTargetHeight + 0.1f, maxTargetHeight), s)
                      + d * (float)Math.Tan(bad * t.MaxErrorDeg * 0.5f * Clamp(noiseB, -1f, 1f) * Deg);
            float flight = Math.Max(0.2f, t.WallFlightTime) * (1f - bad * t.SpeedErrorFraction * Clamp(noiseB, -1f, 1f));

            return new KickResult
            {
                Velocity = BallisticVelocity(new Vec3(0f, startHeight, 0f), new Vec3(x, y, d), flight, t.Gravity),
                SpinY = Clamp(g.Curl, -1f, 1f) * t.MaxSpinRadPerSec
            };
        }

        /// <summary>Height of a drag-free ball when it has travelled <paramref name="distance"/> metres forward. NaN if it never gets there.</summary>
        public static float HeightAtDistance(Vec3 v, float startHeight, float distance, float gravity)
        {
            if (v.Z <= 0.01f) return float.NaN;
            float time = distance / v.Z;
            return startHeight + v.Y * time - 0.5f * gravity * time * time;
        }

        /// <summary>
        /// Velocity that carries a ball from <paramref name="from"/> to <paramref name="to"/> in <paramref name="flightTime"/> seconds.
        /// Used for the facade return (so the first bounce lands where the player can run to it) and the owner's throw-back.
        /// </summary>
        public static Vec3 BallisticVelocity(Vec3 from, Vec3 to, float flightTime, float gravity)
        {
            float t = Math.Max(0.05f, flightTime);
            return new Vec3(
                (to.X - from.X) / t,
                (to.Y - from.Y + 0.5f * gravity * t * t) / t,
                (to.Z - from.Z) / t);
        }

        public static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
        public static float Clamp(float v, float lo, float hi) => v < lo ? lo : (v > hi ? hi : v);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
    }
}
