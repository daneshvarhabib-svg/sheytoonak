using System.Collections;
using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Research proposals 18 and 19, approved 2026-11-09. Only a breaking window gets the treatment (a wall or ground
    /// hit keeps its small shake): an 80 ms hit stop, a stronger shake, a phone buzz (switchable on the lobby), and the
    /// glass that wins the duel plays in slow motion with the camera easing in. Flash on the pane itself is drawn by
    /// PrototypeBootstrap (it owns the window quads).
    /// </summary>
    public class KickFeel : MonoBehaviour
    {
        public DuelManager duel;
        public CameraShake shake;
        public const string VibratePref = "sheytoonak.vibrate";
        public static bool VibrateOn { get => PlayerPrefs.GetInt(VibratePref, 1) == 1; set { PlayerPrefs.SetInt(VibratePref, value ? 1 : 0); PlayerPrefs.Save(); } }

        bool slowMo;
        Coroutine stopRoutine;

        void OnEnable() { GameEvents.WindowBroken += OnBroken; }
        void OnDisable() { GameEvents.WindowBroken -= OnBroken; Time.timeScale = 1f; }
        void Start() { if (duel != null) duel.FinalBlow += OnFinalBlow; }

        void OnBroken(int window, Vector3 point)
        {
            if (shake != null) shake.Kick(0.03f, 0.28f);
#if UNITY_ANDROID || UNITY_IOS
            if (VibrateOn) Handheld.Vibrate();
#endif
            if (!slowMo) { if (stopRoutine != null) StopCoroutine(stopRoutine); stopRoutine = StartCoroutine(HitStop()); }
        }

        IEnumerator HitStop()
        {
            // 50-100 ms reads as impact; shorter is unseen, longer feels like a stutter
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(0.08f);
            if (!slowMo) Time.timeScale = 1f;
            stopRoutine = null;
        }

        void OnFinalBlow(Vector3 point) => StartCoroutine(SlowFinish(point));

        IEnumerator SlowFinish(Vector3 point)
        {
            slowMo = true;
            if (stopRoutine != null) { StopCoroutine(stopRoutine); stopRoutine = null; }
            var cam = Camera.main;
            float fov0 = cam != null ? cam.fieldOfView : 60f;
            const float hold = 1.6f;
            for (float t = 0f; t < hold; t += Time.unscaledDeltaTime)
            {
                float e = Mathf.Sin(Mathf.Clamp01(t / hold) * Mathf.PI); // in and back out
                Time.timeScale = Mathf.Lerp(1f, 0.25f, Mathf.Clamp01(t / 0.12f)) ;
                if (cam != null) cam.fieldOfView = fov0 * (1f - 0.12f * e);
                yield return null;
            }
            if (cam != null) cam.fieldOfView = fov0;
            Time.timeScale = 1f;
            slowMo = false;
        }
    }
}
