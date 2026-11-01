using System;
using System.Collections;
using System.IO;
using Sheytoonak.Core;
using UnityEngine;

namespace Sheytoonak.Prototype
{
    /// <summary>
    /// Automatic screenshots for CI. The phone-only workflow has no editor: the Linux player is run
    /// under xvfb in GitHub Actions with SHEYTOONAK_SHOTS=&lt;dir&gt;, this takes a few PNGs of the
    /// start screen and of a kick, then quits. Without that variable it does nothing, so a normal
    /// build never contains the behaviour. Keep it tiny: it exists so the assistant can see the game.
    /// </summary>
    public static class AutoScreenshot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            string dir = Environment.GetEnvironmentVariable("SHEYTOONAK_SHOTS");
            if (string.IsNullOrEmpty(dir)) return;
            Directory.CreateDirectory(dir);
            var go = new GameObject("AutoScreenshot");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Runner>().dir = dir;
        }

        class Runner : MonoBehaviour
        {
            public string dir;
            IEnumerator Start()
            {
                var boot = FindFirstObjectByType<PrototypeBootstrap>();
                yield return Shot("01-start", 1.5f);
                if (boot != null)
                {
                    boot.StartGameFromScript();
                    yield return Shot("02-game", 1.5f);
                    // aim at the middle-low part of the facade: a realistic first kick
                    boot.Router?.Inject(new KickGesture(0.7f, 0.1f, 0f));
                    yield return Shot("03-kick-air", 0.45f);
                    yield return Shot("04-kick-hit", 0.6f);
                    yield return Shot("05-after", 2.0f);
                }
                Debug.Log("SHEYTOONAK SHOTS done in " + dir);
                yield return null;
                Application.Quit(0);
            }

            IEnumerator Shot(string name, float wait)
            {
                yield return new WaitForSeconds(wait);
                yield return new WaitForEndOfFrame();
                string path = Path.Combine(dir, name + ".png");
                ScreenCapture.CaptureScreenshot(path);
                yield return null; // the capture is written at the end of the next frame
                Debug.Log("SHEYTOONAK SHOT " + path);
            }
        }
    }
}
