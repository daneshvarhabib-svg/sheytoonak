# Working on Sheytoonak with only an Android phone

## The hard limit

The Unity Editor runs on Windows, macOS and Linux only. From a phone you can **not** open the project,
edit scenes or press Build. This project works around that: the playable prototype is built entirely
from code (`Assets/Scripts/Prototype/PrototypeBootstrap.cs` + `Assets/Editor/SheytoonakBuild.cs`), and a
GitHub Actions workflow (`.github/workflows/android-build.yml`) compiles it in the cloud with GameCI.

```
phone -> commit on GitHub -> GitHub Actions (GameCI, Unity 6000.0.84f1 in Docker)
      -> EditMode tests -> Android APK -> artifact "Sheytoonak-APK" -> download and install on the phone
```

**Status (2026-10-21): the workflow has never run successfully.** It cannot run until the Unity licence
secrets exist (below). No APK has been built or tested.

## Licensing: the blocker for a phone-only workflow

Checked on 2026-10-21 against GameCI's own documentation source
(`game-ci/documentation`, file `docs/03-github/02-activation.mdx`, the page published at
game.ci/docs/github/activation):

- Every GameCI action needs an activated Unity licence.
- For the free **Personal** licence the documented method is: install **Unity Hub on a local machine**,
  sign in, go to *Preferences > Licenses > Add > Get a free personal license*, then copy the generated
  `Unity_lic.ulf` file and store its contents as the `UNITY_LICENSE` secret, plus `UNITY_EMAIL` and
  `UNITY_PASSWORD`.
- The `.ulf` file is found at:
  Windows `C:\ProgramData\Unity\Unity_lic.ulf` · macOS `/Library/Application Support/Unity/Unity_lic.ulf` ·
  Linux `~/.local/share/unity3d/Unity/Unity_lic.ulf`.
- The licence is not tied to the OS: a file made on Windows works for the Ubuntu build machine.
- The older "manual activation" website route (upload an `.alf`, download an `.ulf`, no Unity Hub) no
  longer works for Personal licences, according to Unity's forum thread "Unity no longer supports manual
  activation of Personal licenses" and community reports. GameCI's page points to their Discord for
  "Personal License UI Variations".

**Conclusion:** creating the licence file needs Unity Hub, which runs on a computer. There is no
documented way to do this step from an Android phone. It is a **one-time** step: after the three secrets
are saved, every build runs from the phone.

Ways to get past it, in order of practicality:

1. **Borrow any computer for ~15 minutes** (a friend's, a work PC, an internet café): install Unity Hub,
   sign in, add the free Personal licence, open the `.ulf` file in a text editor, and paste its contents
   into the GitHub secret from that same computer's browser. Then sign out of Unity Hub if it is not your computer.
2. **A rented cloud desktop** (any Windows or Linux remote-desktop service you can open in the phone's
   browser): same steps. Not tested here.
3. **GitHub Codespaces with a desktop** (a browser-accessible Linux desktop): in theory Unity Hub for Linux
   can run there and produce the file. Not tested here, and awkward on a phone screen.
4. **Unity Build Automation** (Unity's own cloud build): see the next section.

### Routes that do not need a `.ulf` (investigated 2026-10-21, none tried yet)

**A. GameCI's new CLI-based actions, email and password only (free, experimental).**
- `game-ci/cli` merged a "personal" licensing strategy on 2026-09-05 (PR #246, later fixes #254-#290). It asks
  Unity's licensing service for a Personal seat with `UNITY_EMAIL` + `UNITY_PASSWORD`; with no `UNITY_LICENSE`
  secret its automatic order (file, serial, floating, personal) falls through to it.
- The actions that run this CLI are `game-ci/unity-test-runner` **v4.4.0** (the current `v4` tag) and
  `game-ci/unity-builder` **v6.0.0**. This repository's workflow is pinned to the older classic releases
  (v4.3.2 / v4.8.2), which need the `.ulf`.
- Caveats stated in the PR: the licensing-client flags it uses are undocumented by Unity and can change without
  notice; it cannot answer two-factor or device-verification challenges; it recommends a dedicated CI-only Unity
  account and calls sharing a Personal seat with CI a grey area under Unity's terms; the seat must be returned
  after each run. GameCI's public activation page still documents only the `.ulf` route.
- Cost: none beyond GitHub Actions minutes. Switching would mean a separate workflow on the new actions; not done.

**B. Unity Build Automation (Unity's cloud build).**
- Unity handles the licence itself, it connects to a GitHub repository, and it is configured in a web dashboard
  (usable from a phone browser).
- Free tier, from Unity's 2025 pricing-update page and forum posts (Unity's own sites were not reachable from
  the review machine, so not confirmed first-hand): 200 Windows build minutes and 100 Mac build minutes per month
  from Q1 2026; beyond that, pay-as-you-go, which requires adding a credit card.
- Unknown: whether a card is needed to start, how many minutes one Android IL2CPP build of this project takes,
  and whether the build script here needs changes for it. **Not set up: it can cost money past the free tier.**

**C. Unity Hub command line.** Unity's documentation says Personal licences are activated by signing in to Unity
Hub; command-line activation is documented only for paid serials. A community report describes an experimental
Hub CLI (`unity license activate --personal`), unverified.

Not verified and worth knowing:
- If your Unity account uses two-factor authentication, signing in from the CI machine with email and
  password may fail. If the build log shows a sign-in error, check GameCI's documentation and Discord.
- A Unity Personal licence must be refreshed periodically (Unity Support says Hub asks you to reconnect at
  least every 30 days). If CI builds start failing with licence errors after a while, regenerate the `.ulf`.

Sources:
- GameCI CLI, personal licence strategy: https://github.com/game-ci/cli/pull/246
- Unity pricing updates (Build Automation free tier): https://unity.com/products/pricing-updates
- Unity forum, DevOps updates in 2026: https://discussions.unity.com/t/unity-devops-updates-in-2026/1711090
- GameCI activation docs: https://github.com/game-ci/documentation/blob/main/docs/03-github/02-activation.mdx
- Unity forum, manual activation removed for Personal: https://discussions.unity.com/t/unity-no-longer-supports-manual-activation-of-personal-licenses/926760
- GameCI builder docs (buildMethod, customBuildPath, androidExportType): https://github.com/game-ci/documentation/blob/main/docs/03-github/04-builder.mdx

## After the secrets exist: building from the phone

1. Open `https://github.com/daneshvarhabib-svg/sheytoonak/actions` in the phone's browser.
2. Choose **Android build** > **Run workflow** > branch `claude/unity-project-structure` > **Run workflow**.
   (Every push to that branch also starts it.)
3. Wait. The first run downloads the Unity Docker image and imports the project; expect it to be slow
   (tens of minutes). Later runs use the cache.
4. Open the finished run. If it is green, download the artifact **Sheytoonak-APK** (a zip containing the APK).
   If it is red, open the failed step and send a screenshot of the error lines.
5. Unzip it on the phone (the Files app can), tap the `.apk`, allow "install unknown apps" for the Files app
   or browser when asked, install.

GitHub Actions minutes: a private repository has a monthly free quota and Unity builds use a lot of it.
Check usage under GitHub > Settings > Billing and plans.

## Testing the prototype on the phone

- Hold the phone in landscape with both hands. Tilt or slide it left/right to move; push its bottom edge forward with both thumbs
  to kick. Use **Calibrate kick** on the start screen first.
- Three-finger tap opens the sensor debug overlay. Screenshots of it while tilting and swinging are what is
  needed to tune the controls.
- Report problems with a screenshot or screen recording plus what you did.

## Roadmap (small reviewable changes)

| Step | Change | Status |
|---|---|---|
| 1a | Project structure, gameplay scripts, tests, docs | done |
| 1b | Placeholder prototype built from code, Unity version pinned, editor build script, cloud build workflow | done, **never built** |
| 2 | First successful cloud build; fix whatever the first real compile shows | needs the licence secrets |
| 3 | Persian UI: font, RTL shaping, HUD, start, game over, calibration | not started |
| 4 | Art: backdrops, window rectangles, broken glass, legs, football, audio | not started |
| 5 | iOS | needs macOS, Xcode and an Apple Developer account |
