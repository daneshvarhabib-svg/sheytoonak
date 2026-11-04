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
            // Under xvfb the window never gets focus and a standalone player pauses without it:
            // coroutines never advanced in run 24. Keep running whatever the focus state.
            Application.runInBackground = true;
            Application.targetFrameRate = 30;
            Debug.Log("SHEYTOONAK SHOTS armed: " + dir + " gfx=" + SystemInfo.graphicsDeviceType + " " + Screen.width + "x" + Screen.height);
            var go = new GameObject("AutoScreenshot");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Runner>().dir = dir;
        }

        class Runner : MonoBehaviour
        {
            public string dir;
            int frames;
            void Update()
            {
                frames++;
                if (frames <= 3 || frames % 150 == 0)
                    Debug.Log($"SHEYTOONAK SHOTS frame {frames} t={Time.realtimeSinceStartup:0.0}s focus={Application.isFocused}");
            }

            IEnumerator Start()
            {
                StartCoroutine(Watchdog());
                var boot = FindFirstObjectByType<PrototypeBootstrap>();
                Debug.Log("SHEYTOONAK SHOTS bootstrap " + (boot != null ? "found" : "missing"));
                yield return Shot("01-start", 1.5f);
                if (boot != null)
                {
                    var duel = boot.Duel;
                    if (duel != null) duel.autoDraftForMe = true;
                    boot.StartGameFromScript();
                    if (duel != null)
                    {
                        yield return Shot("02a-draft", 1.6f);
                        float d0 = Time.realtimeSinceStartup + 20f;
                        while (Time.realtimeSinceStartup < d0 && duel.Rules == null) yield return null;
                        yield return Shot("02-stage", 1.0f);
                        // wait for my turn (the bot may open), then kick at the top-middle window, poison, and watch the bot
                        float until = Time.realtimeSinceStartup + 12f;
                        while (Time.realtimeSinceStartup < until && !duel.PlayerMayKick) yield return null;
                        yield return Shot("03-my-turn", 0.2f);
                        // a hero first, then its kick button: it walks up behind the ball, then the swipe
                        duel.Select(1);
                        yield return Shot("03a-selected", 0.3f);
                        duel.ArmKick();
                        float a0 = Time.realtimeSinceStartup + 4f;
                        while (Time.realtimeSinceStartup < a0 && !duel.KickArmed) yield return null;
                        yield return Shot("03b-at-ball", 0.2f);
                        boot.Router?.Inject(new KickGesture(0.85f, 0.05f, 0f));
                        yield return Shot("04-kick-air", 0.45f);
                        yield return Shot("05-kick-hit", 0.6f);
                        yield return Shot("06-after", 2.0f);
                        // show every poison my heroes carry, one per action, so CI pictures all the props
                        for (int i = 0; i < 3 && duel.Rules != null && !duel.Rules.Finished; i++)
                        {
                            float w0 = Time.realtimeSinceStartup + 25f;
                            while (Time.realtimeSinceStartup < w0 && !duel.PlayerMayKick) yield return null;
                            if (duel.PlayerMayKick) duel.Poison(i);
                            yield return Shot("07-poison-" + i, 1.4f);
                        }
                        yield return Shot("08-bot-turn", 4.0f);
                        yield return Shot("09-later", 5.0f);
                        // the challenge: a duel dealt at random (no draft), then home with the run under its button
                        boot.StartChallengeFromScript();
                        yield return Shot("10-challenge", 2.5f);
                        boot.GoHomeFromScript();
                        yield return Shot("11-home", 1.0f);
                        boot.Duel.ScreenshotBags();
                        yield return Shot("12-bags", 0.5f);
                        boot.Duel.TapBag(0);
                        yield return Shot("13-loot", 0.4f);
                    }
                    else
                    {
                        boot.Router?.Inject(new KickGesture(0.7f, 0.1f, 0f));
                        yield return Shot("03-kick-air", 0.45f);
                        yield return Shot("04-kick-hit", 0.6f);
                        yield return Shot("05-after", 2.0f);
                    }
                }
                Debug.Log("SHEYTOONAK SHOTS done in " + dir);
                yield return null;
                Application.Quit(0);
            }

            IEnumerator Watchdog()
            {
                yield return new WaitForSecondsRealtime(170f);
                Debug.LogWarning("SHEYTOONAK SHOTS watchdog: quitting after 60 s");
                Application.Quit(2);
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
