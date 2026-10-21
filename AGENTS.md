# Instructions for coding agents (Codex, Claude, others)

Sheytoonak is a **Unity** mobile game (portrait, Android first, iOS later). Read `docs/GDD.fa.md`
(rules, Persian), `docs/CODEX_BRIEF.md` (frontend tasks) and `docs/MISSING.md` (current state) first.

## Non-negotiable

- Unity 6 LTS (6000.0.x), C#. **Never** replace the project with a web, React or browser game.
- No backend for the offline version (no servers, accounts, online leaderboards, analytics).
- Gameplay rules live in `Assets/Scripts/Core` (pure C#, unit-tested). Change a rule only together with
  its test in `Assets/Tests/EditMode`, and say why.
- Visuals listen to `GameEvents`; gameplay code never references visuals.
- All player-facing text is Persian, right-to-left, Persian digits.
- Be truthful about state: do not say the game compiles, runs, or that an APK was built or tested unless
  that actually happened in a tool run you can point to. Say what was not verified.
- The owner works from an Android phone only. Prefer changes that can be built and checked by the cloud
  build (`docs/ANDROID_PHONE_WORKFLOW.md`) over steps that need the Unity Editor UI; when Editor work is
  unavoidable, say so plainly.
- Small, reviewable changes, each on its own branch.
- Commit `.meta` files together with their assets.
