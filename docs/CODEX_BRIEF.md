# Sheytoonak — brief for the frontend build (Codex)

You are building the **frontend** of a finished game design. The gameplay code is already written in
`Assets/Scripts` and has unit tests. Read `docs/GDD.fa.md` (Persian design doc, the source of truth for
rules) and look at `docs/reference/street-reference.png` (the required visual style).

**Do not change the rules.** If a rule in `Assets/Scripts/Core` must change, change its test in
`Assets/Tests/EditMode` in the same commit and say why.

## 1. The game in one paragraph

Third-person (camera behind and above the player) on a sunny sidewalk, phone held in landscape with
both hands. A football lies at the player's feet. The
player kicks it at the brick building across the street by **tapping the screen**: the tap's position is
the shot (left/right of centre = direction, higher on the screen = harder and higher). A ball that hits the wall bounces back; it must
touch the ground **exactly once** before the player kicks it again (tennis rule), and the player moves
left/right by **tilting or sliding the phone** to get to it. A second bounce costs one of 3 lives.
Hitting an intact window breaks it (+1). Hitting an already broken window sends the ball into the flat:
sometimes the angry owner throws it back, otherwise it is lost (-1 life). Breaking every window of a
building earns a medal and walks the player to the next building; 8 buildings in the street.

## 2. Tech

- **Unity 6000.0.84f1** (pinned in `ProjectSettings/ProjectVersion.txt`), built-in render pipeline for the
  placeholder prototype (moving to URP later means converting the generated materials). The code uses Unity 6 APIs
  (`Rigidbody.linearVelocity`, `FindAnyObjectByType`). Do not downgrade to 2022.
- **Input System** package (sensors: `Gyroscope`, `Accelerometer`, `GravitySensor`,
  `LinearAccelerationSensor`). Active Input Handling: **Both** (committed in `ProjectSettings.asset`), because the
  placeholder IMGUI HUD gets no input with "Input System Package" alone. A uGUI-only HUD could switch to Input System.
- **TextMeshPro** plus **RTLTMPro** (or another correct Persian shaping solution). All UI is Persian, right-to-left.
  Plain TMP does not join Persian letters.
- Landscape only (changed 2026-10-23 after the first phone test; it was portrait). Targets Android and iOS.
- This repository is the Unity project. The `.asmdef` files define `Sheytoonak.Core` (pure C#, no Unity),
  `Sheytoonak.Game`, `Sheytoonak.Editor` and `Sheytoonak.Tests.EditMode`. A grey-box prototype that builds
  itself from code already exists (`PrototypeBootstrap`); replace its visuals, keep its wiring as a reference.
  Run Window > General > Test Runner > EditMode: all tests must pass before you build anything else.

## 3. What the code already does

| Script | Role |
|---|---|
| `Core/RallyRules.cs` | All rules: phases, bounces, lives, score, windows, medals, houses, win/lose. Pure C#. |
| `Core/KickSolver.cs` | Gesture + contact -> launch velocity, contact quality, ballistic helpers. |
| `Core/MotionGestureDetector.cs` | Raw gyro/accelerometer samples -> lateral position (-1..1) and kick gestures (strength, aim, curl). |
| `Config/GameConfig.cs` | ScriptableObject with every tunable (lives, reach, kick speeds, motion thresholds, return assist, owner chance). |
| `Config/HouseDefinition.cs` | ScriptableObject per building: backdrop sprite, window rectangles in **image uv**, roofline, difficulty. |
| `Gameplay/GameManager.cs` | Wires everything; turns physics contacts into rule events; raises `GameEvents`. Creates ground/facade/kill colliders at runtime if missing. |
| `Gameplay/BallController.cs` | Football Rigidbody: wind, Magnus curl, reports ground/facade/kill-zone contacts. |
| `Gameplay/PlayerFoot.cs` | The kicking foot's position on the sidewalk, reach checks. Parent the legs to it. |
| `Gameplay/BackdropWindowMapper.cs` | Casts the window rectangles drawn on the backdrop image onto an invisible facade plane, so physics matches the art. Also the reachability report. |
| `Gameplay/GameEvents.cs` | **Your API.** Static events for UI, audio, VFX, animation. |
| `Input/MotionKickInput.cs` | Phone sensors -> detector. `Recenter()` and `CalibrateKick(seconds, callback)` for the calibration screen. |
| `Input/TouchKickInput.cs` | Touch/mouse/keyboard fallback for the editor. |
| `Input/KickInputRouter.cs` | Picks motion when available, else touch; the one `IKickInput` the game uses. |
| `Input/SensorDebugOverlay.cs` | Raw sensor readout (F1 or three-finger tap) to verify units and axis signs on a real phone. |

World convention: metres, x = right, y = up, z = toward the facade. The player's foot is at z = 0.

## 4. Your tasks

### 4.1 Scene `Game`
- `Main Camera`: perspective, placed at about (0, 1.6, -1.2) looking slightly down at the facade. Tune
  position/FOV until the ground plane and the facade line up with the backdrop image (a camera-match
  tool like fSpy can help). The bottom ~25% of the screen must show the player's legs and the ball.
- `Backdrop`: the current house's sprite drawn full-screen **behind** all 3D (Canvas in Screen Space -
  Camera at the far plane, or a camera background quad). Fit = Cover. Set `BackdropWindowMapper.fit` to match.
- `Player` at (0,0,0) with `PlayerFoot` and `BackdropWindowMapper` (mapper's `cam` = Main Camera).
- A **full player character seen from behind** (rigged model: jeans + sneakers + shirt) parented to `PlayerFoot`. Animations: idle, side-step
  (blend by `PlayerFoot.Velocity`), kick (play on `GameEvents.Kicked`, blend speed/height by strength), whiff
  (`GameEvents.KickRejected`), walk (during `GameEvents.WalkStarted`).
- `Ball`: a real football model under a GameObject with `Rigidbody`, `SphereCollider` and `BallController`.
  PhysicsMaterial on the ball: bounciness ≈ 0.6, bounce combine Maximum, dynamic friction ≈ 0.5.
- `Input`: `MotionKickInput` + `TouchKickInput` + `KickInputRouter` (drag both into the router).
- `Game`: `GameManager` with the config, the 8 houses, ball, foot, mapper, and the router as `inputSource`.
- `SensorDebugOverlay` (dev builds only).

### 4.2 Art — the important part
The look must match `docs/reference/street-reference.png`. Code cannot paint that style, so use
**pre-rendered backdrops** (2.5D):
1. Produce **8 backdrop images** in exactly that style (same lighting, palette, line quality): a brick
   building across the street with rows of windows and shops with awnings on the ground floor, sidewalk in
   the foreground, street in between, trees and a lamp post, **no people**. Landscape framing (about 19.5:9),
   wide enough for the camera behind the player. Vary brick colour, floors (3-6), shop types and awnings.
   The first can be derived from the reference image itself (remove the person).
2. For each, create a `HouseDefinition` asset: sprite, `facadeU`, `roofV`, and one `WindowSpot` per
   window with its **uv rectangle** (0..1, origin bottom-left). Build a small **EditorWindow** to drag
   rectangles over the sprite instead of typing numbers.
3. Broken glass: a shared overlay sprite (dark interior + jagged shards along the frame), placed over the
   window's uv rect (same mapping as the backdrop: use `BackdropWindowMapper.UvToScreen`).
4. Optional per-window extras: curtain that sways after breaking, a neighbour sprite who appears in a
   broken window on `GameEvents.OwnerThrow` and throws the ball.
5. Run the `GameManager` context menu **Reachability report** for every house; fix any window marked
   UNREACHABLE (lower it, change `facadeDistance`, or tune `GameConfig.kick`).

### 4.3 UI (Persian, RTL)
Screens: start, calibration, HUD, pause, walk interstitial, game over, victory, settings. See GDD §7.
Strings to use:

| key | text |
|---|---|
| play | بازی |
| best | رکورد: {0} شیشه |
| medals | مدال: {0} |
| hud_glass | شیشه |
| hud_house | ساختمونِ {0} از {1} |
| hud_windows | شیشه‌ها: {0} از {1} |
| calib_hold | گوشی رو صاف جلوی سینه‌ت نگه دار |
| calib_swing | حالا یه بار مثلِ شوت، لبه‌ی پایینِ گوشی رو به جلو تاب بده |
| calib_ok | عالی! آماده‌ای |
| calib_fail | حرکت رو حس نکردم، یه بار محکم‌تر تاب بده |
| reject_needs_bounce | اول باید بخوره زمین |
| reject_out_of_reach | دستت نرسید، برو سمتِ توپ |
| loss_second_bounce | دو بار خورد زمین |
| loss_short | نرسید |
| loss_roof | رفت پشتِ بوم! |
| loss_out | رفت تو خیابون |
| loss_house | توپ موند تو خونه |
| loss_passed | از کنارت رد شد |
| owner_back | صاحب‌خونه پرتش کرد بیرون! |
| medal | مدال! همه‌ی شیشه‌های {0} ریخت |
| game_over | باختی |
| victory | کلِ خیابون رو ترکوندی! |
| again | دوباره |
| settings_motion | کنترل با حرکتِ گوشی |
| settings_lateral | جابه‌جایی: ترکیبی / کج کردن / سُر دادن |
| recalibrate | کالیبره‌ی دوباره |

Digits shown to the player in Persian (۰۱۲۳۴۵۶۷۸۹).

### 4.4 Hooking up `GameEvents`
Subscribe in `OnEnable`, unsubscribe in `OnDisable`. Map:
`HouseLoaded` -> swap backdrop, clear overlays · `Served` -> HUD hint · `WindChanged` -> HUD arrow and tree sway ·
`Kicked` -> kick animation, sound, haptic · `KickRejected` -> whiff + hint text · `Bounced` -> thud
(show a subtle marker on bounce 1 = "now!") · `WallHit` -> brick dust decal + sound · `WindowBroken` ->
overlay, shatter particles, sound, haptic · `BallInHouse`/`OwnerThrow` -> neighbour animation + shout ·
`LifeLost` -> HUD life, loss text by `LossReason` · `ScoreChanged` -> HUD · `MedalEarned` -> medal popup ·
`WalkStarted` -> walk animation for exactly the given seconds, then the new backdrop arrives with `HouseLoaded` ·
`GameOver`/`Victory` -> end screens.
Call `GameManager.NewGame()` from the Play/Again buttons (keep `autoStart` off).

### 4.5 Calibration screen
1. "hold straight" -> `KickInputRouter.Recenter()`.
2. (only if `KickInputRouter.kickFromMotion` is on) "swing once" -> `MotionKickInput.CalibrateKick(3f, ok => ...)`.
3. A practice area: a ball, no lives, until the player taps "ready".
Show it on first launch and from settings.

## 5. Acceptance checklist
- [ ] All EditMode tests pass.
- [ ] On a real Android phone and an iPhone: the debug overlay shows gyro values in rad/s; tilting moves the
      player; a low tap kicks low, a high tap kicks high; a tap at the very top goes over the roof.
- [ ] If the overlay shows linear acceleration around 0.1-1 while shaking, set
      `MotionKickInput.linearAccelerationScale` to 9.81.
- [ ] A returning ball cannot be kicked before its bounce; the second bounce costs a life.
- [ ] Window hit boxes line up with the painted windows on every house, on a 19.5:9 phone and a 4:3 iPad.
- [ ] Every window is reachable (Reachability report).
- [ ] Owner throw-back happens roughly one time in three when hitting a broken window.
- [ ] Medal, walk, next house; victory after house 8; best score survives an app restart.
- [ ] 60 fps on a mid-range Android phone.
