# Sheytoonak (شیطونک)

A portrait mobile game for Android and iOS, built with **Unity**. First-person on a sunny street, you
kick a football at the windows of the brick building across the road by swinging your phone. The ball
bounces back off the wall and must touch the ground exactly once before your next kick. Breaking every
window of a building earns a medal; 8 buildings, 3 lives. Persian, right-to-left UI. Offline.

## Status

**Not playable yet.** This repository holds the gameplay scripts, unit tests and design documents.
Scenes, assets, project settings and builds do not exist yet, and the code has not been compiled. See
[`docs/MISSING.md`](docs/MISSING.md) for the exact list.

| Path | Contents |
|---|---|
| `Assets/Scripts/Core` | Rules, kick maths, phone-motion gesture detection. Pure C#, no Unity dependency. |
| `Assets/Scripts/Config` | `GameConfig` and `HouseDefinition` ScriptableObjects. |
| `Assets/Scripts/Gameplay` | Game manager, ball physics, player foot, backdrop-to-physics window mapping, events. |
| `Assets/Scripts/Input` | Phone motion, touch/keyboard fallback, input router, sensor debug overlay. |
| `Assets/Tests/EditMode` | NUnit tests for rules, kick maths and gesture detection. |
| `Packages/manifest.json` | Unity packages (Input System, URP, uGUI/TextMeshPro, Test Framework). |
| `docs/GDD.fa.md` | Game design document (Persian). |
| `docs/CODEX_BRIEF.md` | Frontend build brief for a coding agent. |
| `docs/MISSING.md` | What is missing before the game is playable. |
| `docs/ANDROID_PHONE_WORKFLOW.md` | How to build and test using only an Android phone. |
| `docs/reference/street-reference.png` | Visual style reference. |
| `AGENTS.md` | Rules for coding agents working in this repo. |

## Opening the project (needs a computer)

1. Install Unity Hub and Unity **6000.0 LTS** with Android Build Support.
2. Open this folder as a project. Unity creates `ProjectSettings/`, `Library/` and the `.meta` files.
3. Window > General > Test Runner > EditMode > Run All.
4. Commit `ProjectSettings/` and all `.meta` files.

Phone-only route: [`docs/ANDROID_PHONE_WORKFLOW.md`](docs/ANDROID_PHONE_WORKFLOW.md).
