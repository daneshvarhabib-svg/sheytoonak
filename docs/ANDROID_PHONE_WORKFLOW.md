# Working on Sheytoonak with only an Android phone

## The hard limit

The Unity Editor runs on Windows, macOS and Linux only. From a phone you can **not** open the project,
edit scenes or press Build. Everything below works around that by keeping the project **code-driven**
and letting a cloud machine compile it.

## Recommended path: code-only project + cloud build to an APK

```
phone (chat with Claude / Codex)
   -> small commits to this GitHub repo
   -> GitHub Actions runs Unity in the cloud (GameCI)
   -> EditMode tests + Android build
   -> APK attached to the workflow run
   -> download on the phone, install, play, send screenshots/feedback
```

### What has to happen, in order (each one is a small reviewable change)

| Step | Change | Result you can check on the phone |
|---|---|---|
| 1 | **This change:** project structure, gameplay scripts, tests, docs, package manifest | Files visible on GitHub. Nothing to play yet. |
| 2 | Runtime bootstrap: the game scene, camera, ground, facade, ball and a placeholder house built from code; an editor build script that creates `Game.unity`, applies player settings and builds the APK | Still nothing to install until step 3. |
| 3 | GitHub Actions workflow (GameCI): run EditMode tests, build Android APK, upload it as an artifact | A green check (or the exact error) on GitHub; an APK download. |
| 4 | Persian UI: font, RTL shaping, HUD, start, game over, calibration screen | Persian screens on the phone. |
| 5 | Art: backdrops, window rectangles, broken glass, legs, football, audio | The intended look. |

### One-time setup that only you can do

1. **Unity account** at unity.com (free Personal plan).
2. **Unity licence for the cloud build.** GameCI needs your Unity email and password and, for a Personal
   licence, a licence file, saved as GitHub repository secrets (`UNITY_EMAIL`, `UNITY_PASSWORD`,
   `UNITY_LICENSE`). ⚠️ Getting that licence file has usually required signing in to Unity Hub on a
   computer once. Whether a fully phone-only activation is possible today was **not verified**; check the
   "Activation" page of the GameCI documentation before step 3. If it needs a computer, borrowing one or
   renting a cloud Windows/Linux desktop for an hour is enough; after that everything runs from the phone.
3. **GitHub Actions minutes.** Private repositories get a monthly free quota; a Unity Android build
   takes a sizeable chunk of it (tens of minutes per build is typical). Check your quota under GitHub
   Settings > Billing.
4. On the phone: allow installing apps from your browser/file manager ("unknown sources") to install the APK.

### Testing on the phone

- Install the APK from the workflow run's artifacts.
- Three-finger tap opens the sensor debug overlay. Screenshot it while tilting and swinging the phone;
  those numbers are what is needed to tune the motion controls.
- Report problems with a screenshot or screen recording plus what you did.

## Alternatives

- **Unity Build Automation** (Unity's own cloud build): connects to the GitHub repo and produces APKs.
  It is a paid/credit-based service; check current pricing before relying on it.
- **A computer for occasional sessions:** the fastest way to fix scene, art and layout problems,
  because those are visual and much easier in the Editor.

## What not to expect

- No claim in this repository that an APK exists or was tested is true until a workflow run or a phone
  install shows it.
- iPhone builds need macOS, Xcode and an Apple Developer account; that is a later, separate step.
