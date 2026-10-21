using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// Development overlay: raw sensor values, lateral position and the last kick.
    /// Needed once on a real phone to confirm units and axis signs. Toggle with a 3-finger tap or F1.
    /// </summary>
    public class SensorDebugOverlay : MonoBehaviour
    {
        public MotionKickInput motion;
        public GameManager game;
        public bool visible;

        void Update()
        {
            var ts = UnityEngine.InputSystem.Touchscreen.current;
            int touching = 0;
            if (ts != null) foreach (var t in ts.touches) if (t.press.wasPressedThisFrame) touching++;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (touching >= 3 || (kb != null && kb.f1Key.wasPressedThisFrame)) visible = !visible;
        }

        void OnGUI()
        {
            if (!visible) return;
            var s = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(14, Screen.height / 50) };
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height * 0.6f), GUI.skin.box);
            if (motion != null)
            {
                GUILayout.Label("gyro rad/s  " + motion.RawGyro.ToString("F2"), s);
                GUILayout.Label("linear      " + motion.RawLinear.ToString("F2"), s);
                GUILayout.Label("gravity     " + motion.RawGravity.ToString("F2"), s);
                if (motion.Detector != null)
                {
                    GUILayout.Label("roll deg    " + motion.Detector.LastRollDeg.ToString("F1") + "   lateral " + motion.Detector.Lateral.ToString("F2") + (motion.Detector.InSwing ? "   SWING" : ""), s);
                }
                var k = motion.LastKick;
                GUILayout.Label("last kick   strength " + k.Strength01.ToString("F2") + "  aim " + k.Aim.ToString("F2") + "  curl " + k.Curl.ToString("F2"), s);
            }
            if (game != null && game.Rules != null)
            {
                GUILayout.Label("phase " + game.Rules.Phase + "  bounces " + game.Rules.BouncesSinceWall + "  lives " + game.Rules.Lives + "  score " + game.Rules.Score + "  wind " + game.Wind.ToString("F2"), s);
            }
            GUILayout.EndArea();
        }
    }
}
