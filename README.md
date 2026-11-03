# Helicopter Control

A physics-based helicopter flight prototype made in Unity. The helicopter is moved only by forces and torques applied to a `Rigidbody`: there are no direct transform changes and no ready-made flight controllers.

**Unity 6000.2.7f2 · HDRP · Input System · Cinemachine · TextMeshPro**

---

## How to run

- **Build:** unzip the archive and run `Helicopter_Control.exe`.
- **Editor:** open the project in Unity 6000.2.7f2 and start the `Assets/Scenes/MainMenu` scene.
  `FlightTest` can also be started directly: it spawns the default helicopter in tuning mode.

## Controls

| Key | Action |
|---|---|
| **W / S** | Cyclic: pitch (nose down / nose up), which moves the helicopter forward / backward |
| **A / D** | Cyclic: roll left / right, which moves the helicopter sideways |
| **Space / Ctrl** | Collective lever up / down (the lever **holds its position** when released) |
| **Q / E** | Yaw left / right (tail rotor) |
| **Tab** | Show / hide the tuning panel (tuning mode only) |
| **Esc** | Back to the main menu |

## Features

- **Main menu** with three helicopters. Each card shows mass and blade length, read directly from the prefab and its rotor geometry. Hovering a card spins that helicopter's rotor.
- **Two modes:** normal flight, and **tuning mode** with live sliders for lift, torque and drag. Tuned values can be saved per helicopter and reset to defaults.
- **HUD:** compass tape, speed, height (above ground and above zero level), vertical speed, collective bar with a hover mark, and an attitude indicator (the helicopter icon rotates with roll; side scales show pitch).
- **Test area** (1.4 × 1.4 km) closed by mountains: landing pads on the ground, on platforms, towers and rooftops, gates in three sizes (S / M / L, one per helicopter class), a slalom, bridges, a downtown with high-rises, a residential area and an industrial zone.

### Helicopters

| | Scout | Utility | Transport |
|---|---|---|---|
| Mass | 650 kg | 2 400 kg | 8 000 kg |
| Rotor diameter | 6.8 m | 10.4 m | 17 m |
| Blades | 2 | 4 | 5 |
| Hover at collective | ≈ 50 % | ≈ 56 % | ≈ 63 % |

The physics parameters are calculated for each model separately, so the light helicopter is agile and the heavy one is slow and stable.

---

## Flight model

All forces are applied in `FixedUpdate` (`HelicopterController`).

**Gravity:** Unity `Rigidbody` gravity. Mass is set per helicopter.

**Lift / thrust**
```
lift = m · g · collective · maxLiftRatio  −  verticalDamping · v_along_rotor_axis
lift = max(0, lift)
```
- Lift acts along `transform.up` (the rotor axis), not world up. When the helicopter tilts, part of the lift becomes horizontal thrust. **That is the only thing that moves the helicopter forward and sideways**, just like a real helicopter. Tilting also reduces the vertical component, so the helicopter sinks unless the collective is raised.
- **Collective is a lever (0…1):** Space / Ctrl move it at `collectiveRate` per second, and it stays where it was left. Hover happens at `1 / maxLiftRatio`.
- **Vertical rotor damping:** in a climb the air flows onto the blades from above and lowers their angle of attack, so thrust drops; in a descent thrust grows. This term is proportional to the speed along the rotor axis. It limits climb and descent speed and makes hovering stable.
- Lift is never negative, because a rotor at minimum pitch does not pull the helicopter down.
- On the ground, collective 0 means zero thrust, so the helicopter rests on its full weight.

**Torque (inertia)**
```
torque = (cyclic.y · pitchTorque,  yaw · yawTorque,  −cyclic.x · rollTorque)   →  AddRelativeTorque
```
- Torque is applied in local axes with `ForceMode.Force`, so the response depends on the **inertia tensor** that Unity calculates from the colliders. The long tail boom makes pitch and yaw ~4× more inert than roll.
- Starting values were calculated as `torque = inertia × desired angular acceleration`.
- Roll uses a minus sign because in Unity a positive rotation around Z lifts the right side (roll left), while D (`cyclic.x = +1`) means roll right.

**Drag**
```
F_drag = −k · v · |v|        (per local axis, quadratic air drag)
τ_drag = −c · ω              (per local axis, linear angular drag)
```
- Linear drag is calculated separately for sideways, vertical and forward motion, because the helicopter has a different cross-section in each direction. The coefficients come from `k = ½ · ρ · Cd · A`, using the model's frontal area, side area and rotor disc area.
- Angular drag stops rotation when the cyclic is released: `c = torque / desired max angular speed`.
- Unity's built-in `linearDamping` / `angularDamping` are set to 0. All damping is done by the model above.

**Rigidbody settings:** Interpolate (smooth visuals with physics at 50 Hz), Continuous collision detection. Visual-only parts (rotors, fins, windows) have no colliders, so they do not distort the centre of mass and inertia.

---

## Architecture

```
Input layer         HelicopterInput ──implements──► IHelicopterInput (Cyclic, Collective, Yaw)
                          │
Physics             HelicopterController  (lift, torque, drag, ground check)
                          │  GetTuning() / ApplyTuning()
Data for UI         HelicopterTelemetry   (speed, height, attitude, collective)
                          │
Presentation        FlightHUD, CompassTape         RotorVisual (visual only)
Tuning              TuningPanel → TuningSliderRow   TuningStorage (PlayerPrefs)
Scene flow          MainMenu: HelicopterCardSpawner → HelicopterCard → FlightSession
                    FlightTest: FlightSceneController (spawn, camera, HUD, tuning, Esc)
```

Key decisions:
- **Input is separated from physics through `IHelicopterInput`.** The controller does not know about the keyboard, so an autopilot, AI or replay input could be plugged in without touching the physics.
- **The HUD does not read physics directly.** `HelicopterTelemetry` calculates all displayed values, and the HUD only formats them.
- **Fail fast:** components that miss a required dependency log an error once and disable themselves instead of failing every frame.
- **Prefabs are the single source of truth.** Menu models and flight helicopters are both instantiated from the same prefabs. Menu models have their scripts disabled and a kinematic `Rigidbody`.
- **`FlightSession`** (static) passes the selected helicopter and mode from the menu scene to the flight scene.
- **Tuning** is stored as one JSON object per helicopter (`TuningData` → `PlayerPrefs`). Saved values are applied on top of the prefab defaults in both modes.

### Project structure
```
Assets/
 ├─ HelicopterControls.inputactions   (Helicopter map: Cyclic, Collective, Yaw)
 ├─ Prefabs/      Helicopters/ (Scout, Utility, Transport), Card
 ├─ Scenes/       MainMenu, FlightTest
 └─ Scripts/
     ├─ Core/     FlightSession, HelicopterMetrics, TuningData, TuningStorage
     ├─ Flight/   HelicopterController, HelicopterTelemetry, FlightSceneController
     ├─ Input/    HelicopterInput, IHelicopterInput, generated HelicopterControls
     ├─ Menu/     MainMenuController, HelicopterCardSpawner, HelicopterCard
     ├─ UI/       FlightHUD, CompassTape, TuningPanel, TuningSliderRow
     └─ Visual/   RotorVisual
```

---

## Simplifications

- **Rotor RPM is visual.** In a real helicopter RPM is almost constant (held by the governor), and thrust changes with blade pitch. Here rotor speed follows the collective only so that the player can see the lever position.
- **No stability augmentation.** When the cyclic is released, angular drag stops the rotation, but the helicopter keeps its current attitude and does not level itself.
- **No main rotor torque reaction.** The fuselage is not pushed into yaw by the main rotor, so the tail rotor (Q/E) is used only for turning.
- Not modelled: blade flapping, gyroscopic precession, ground effect, translational lift, vortex ring state, wind.

## Possible improvements

- **Flight assists:** PD attitude hold (auto-level when the cyclic is released) and an alternative "assisted" collective mode (spring to neutral, auto-hover), switchable at runtime with bumpless transfer.
- Main rotor torque reaction compensated by the tail rotor, with the rotation direction as a parameter.
- Ground effect and translational lift.
- Move per-helicopter parameters into a `ScriptableObject` config.
- Gamepad and joystick support (the input layer already allows it).
- Tuning sliders in physical units (angular acceleration and max rate) instead of raw torque values.
