# What is missing before Sheytoonak is playable

Status on 2026-10-21: the repository contains **gameplay C# scripts, EditMode tests and design docs only**.
There is no Unity scene, prefab, asset, project setting or build. The game has **never been compiled,
run or built**: no C# compiler or Unity Editor was available when the scripts were written (they were
syntax-checked only). No APK exists.

## A. Needed for a first playable build (placeholder graphics are fine)

| # | Item | Path / form | How it gets made |
|---|---|---|---|
| 1 | Unity project settings | `ProjectSettings/*` incl. `ProjectVersion.txt` (Unity 6000.0 LTS) | Generated the first time the project is opened in Unity 6, or by the cloud build (see `docs/ANDROID_PHONE_WORKFLOW.md`). Commit them afterwards. |
| 2 | `.meta` files for every file under `Assets/` | next to each asset | Generated on first import. Must be committed, or asset references break between machines. |
| 3 | Player settings | inside `ProjectSettings/ProjectSettings.asset` | Product name `Sheytoonak`, package id (e.g. `ir.sheytoonak.game`), **portrait only**, Active Input Handling = **Input System Package**, Android: IL2CPP + ARM64, minimum API 24+. |
| 4 | Game scene | `Assets/Scenes/Game.unity` | Either built by hand in the Editor (layout in `docs/CODEX_BRIEF.md` §4.1) **or** created from code by a runtime bootstrap (planned next change, so a phone-only cloud build is possible). |
| 5 | `GameConfig` asset | `Assets/Config/GameConfig.asset` | Create > Sheytoonak > Game Config (or the bootstrap creates one at runtime with defaults). |
| 6 | At least one `HouseDefinition` | `Assets/Config/Houses/House_01.asset` | Backdrop sprite + window rectangles (uv) + roofline. A placeholder backdrop (flat image with drawn windows) is enough to start. |
| 7 | Ball prefab | `Assets/Prefabs/Ball.prefab` | Rigidbody + SphereCollider (r 0.11) + `BallController` + PhysicsMaterial (bounciness ≈ 0.6, bounce combine Maximum). A white sphere is fine at first. |
| 8 | Minimal UI | HUD (score, 3 lives, house n/8), start, game over | uGUI + TextMeshPro. |
| 9 | Persian text rendering | font asset + shaping | A Persian font with an open licence (e.g. Vazirmatn, OFL) as a TMP font asset, and **RTLTMPro** (or equal) because plain TextMeshPro does not join Persian letters. |

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

## D. Deliberately not included

- No backend, accounts, online leaderboard or analytics: the first version is offline. Best score is stored on the device (`PlayerPrefs`).
