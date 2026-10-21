# What is missing before Sheytoonak is playable

Status on 2026-10-21: the repository contains gameplay C# scripts, EditMode tests, design docs, a
**placeholder prototype built entirely from code**, an editor build script, a pinned Unity version and a
GitHub Actions build workflow. The project has **never been compiled, run or built**: no C# compiler or
Unity Editor was available when the code was written (all C# files were syntax-checked only), and the
cloud build cannot run until the Unity licence secrets exist. No APK exists.

## A. Needed for a first playable build (placeholder graphics)

| # | Item | Status |
|---|---|---|
| 1 | Unity version | **Done:** `ProjectSettings/ProjectVersion.txt` = `6000.0.84f1` (a GameCI Android image exists for it). |
| 2 | Other `ProjectSettings/*` files | Unity generates them on first import; the editor build script (`Assets/Editor/SheytoonakBuild.cs`) sets everything the build relies on each time: product name, package id `ir.sheytoonak.prototype`, portrait, IL2CPP + ARM64, min API 26, Active Input Handling = Input System. Commit the generated files after the first import on any computer. |
| 3 | `.meta` files | Unity generates them on import. Commit them after the first import on a computer; CI regenerates them on every run, so they are not needed for cloud builds. |
| 4 | Scene | **Done (generated):** *Sheytoonak > Create Prototype Scene* or the CI build writes `Assets/_Generated/Prototype.unity` (git-ignored) with one `PrototypeBootstrap`. |
| 5 | Config, houses, ball, input, game manager | **Done (generated at runtime)** by `PrototypeBootstrap`: default `GameConfig`, 3 grey-box houses (6, 9 and 12 windows), a football sphere with physics material, motion/touch input, `GameManager`, debug overlay. |
| 6 | HUD | **Placeholder:** English IMGUI text (score, lives, house, windows, wind, messages, start/end screens). Unity's IMGUI cannot shape Persian, so the Persian UI is item 9. |
| 7 | **Unity licence secrets in GitHub** | **Missing - blocker.** `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`. Creating the licence file needs Unity Hub on a computer once; see `docs/ANDROID_PHONE_WORKFLOW.md`. |
| 8 | First successful build | Not done. Expect the first run to reveal compile errors that need fixing. |
| 9 | Persian text rendering | Missing: a Persian font with an open licence (e.g. Vazirmatn, OFL) as a TextMeshPro font asset, plus **RTLTMPro** (or equal), because plain TextMeshPro does not join Persian letters. |

## B. Needed for the intended look (`docs/reference/street-reference.png`)

| # | Item | Notes |
|---|---|---|
| 10 | 8 backdrop images, portrait, no people | Same anime-like sunny style as the reference: brick building across the street, shop awnings, sidewalk, trees. Code cannot produce this; it needs an artist or an image generator. |
| 11 | Window rectangles for each backdrop | Authored in uv (0..1). An EditorWindow for drawing them is listed in the brief. |
| 12 | Broken-glass overlay sprite(s) | Dark interior + jagged shards. |
| 13 | First-person legs model + animations | idle, side-step, kick (blend by strength), walk. Check the licence of any downloaded model. |
| 14 | Football model + texture | |
| 15 | Audio | kick, bounce on asphalt, hit on brick, 2-3 glass shatters, owner shout, street ambience, medal jingle. Only clips with a licence that allows use in a game. |
| 16 | Optional extras | neighbour sprite for the throw-back, cat, curtains, brick-dust decal. |

## C. Needed to build and install

| # | Item | Notes |
|---|---|---|
| 17 | Unity account + licence | Unity Personal is free below Unity's revenue threshold. Required for any build, local or cloud. |
| 18 | Build machine | The Unity Editor does not run on Android. Either a computer, or a cloud build (GitHub Actions with GameCI, or Unity Build Automation). |
| 19 | Android signing | Debug signing is enough for installing on your own phone. A release keystore is needed for Google Play. |
| 20 | iOS | Needs macOS + Xcode and an Apple Developer account to install on an iPhone. Not possible from an Android phone; a macOS cloud runner can build but signing still needs the Apple account. |

## E. Static review (2026-10-21), not verified by a compiler

Checked by reading the code only; nothing here has been compiled.

- Assemblies: `Sheytoonak.Core` has no Unity references and is the only thing the EditMode tests use, so
  the tests need no graphics device, scene or asset. `Sheytoonak.Editor` (Editor only) references `Sheytoonak.Game`,
  which contains `Sheytoonak.Prototype`. No namespace or reference gaps were found.
- Workflow: inputs match the GameCI docs for `unity-test-runner@v4` and `unity-builder@v4`; secrets are only
  passed as environment variables; the build method parses GameCI's `-customBuildPath` and `-androidVersionCode`.
  The Docker image `unityci/editor:ubuntu-6000.0.84f1-android-3` exists.
- Fixed in this review: the phone sensors are looked up again (once a second) if they were not present
  when the motion input started; on some Android phones they appear a frame later.
- Unverified until the first CI run:
  - `com.unity.test-framework` 1.4.5 and `com.unity.ugui` 2.0.0 (the package registry was not reachable from
    the review machine; Unity may replace these core packages with the editor's built-in versions).
  - Only `ProjectVersion.txt` is committed in `ProjectSettings/`. Unity generates the rest. Active input
    handling is switched to the Input System by script (`SheytoonakBuild.ApplyPlayerSettings`, through the
    serialized `activeInputHandler` property, which has no public API) just before the build.
  - Kick timing after the bounce (reach, maximum kick height, landing distance) is untuned.

## D. Deliberately not included

- No backend, accounts, online leaderboard or analytics: the first version is offline. Best score is stored on the device (`PlayerPrefs`).
