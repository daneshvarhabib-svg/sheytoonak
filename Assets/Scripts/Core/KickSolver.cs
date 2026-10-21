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
        public float MaxErrorDeg = 7f;        // random yaw/loft error of a bad contact
        public float SpeedErrorFraction = 0.08f;
        public float MaxSpinRadPerSec = 25f;  // side spin at Curl = ±1
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
