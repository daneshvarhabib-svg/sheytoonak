using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The only "camera motion" left after the lateral movement was removed (user, 2026-10-27):
    /// a short shake when the ball hits the wall or the ground. Read <see cref="Offset"/> when placing the camera.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        // User, 2026-10-28: the first values (0.07 / 0.025) were "like an earthquake"; a small flinch is enough.
        public float wallAmplitude = 0.012f, wallSeconds = 0.18f;
        public float groundAmplitude = 0.004f, groundSeconds = 0.1f;

        float amp, until, total;
        public Vector3 Offset { get; private set; }

        void OnEnable() { GameEvents.WallHit += OnWall; GameEvents.Bounced += OnBounce; }
        void OnDisable() { GameEvents.WallHit -= OnWall; GameEvents.Bounced -= OnBounce; }

        void OnWall(Vector3 p) => Kick(wallAmplitude, wallSeconds);
        void OnBounce(Vector3 p, int n, bool returning) => Kick(groundAmplitude, groundSeconds);

        public void Kick(float amplitude, float seconds)
        {
            amp = Mathf.Max(amp, amplitude); total = seconds; until = Time.unscaledTime + seconds;
        }

        void Update()
        {
            float left = until - Time.unscaledTime;
            if (left <= 0f) { Offset = Vector3.zero; amp = 0f; return; }
            float k = left / Mathf.Max(0.01f, total);
            float t = Time.unscaledTime * 55f;
            Offset = new Vector3(Mathf.Sin(t) * amp * k, Mathf.Cos(t * 1.3f) * amp * k, 0f);
        }
    }
}
