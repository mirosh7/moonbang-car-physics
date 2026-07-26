# Native library API

`CarPhysics.dll` exposes a plain **C ABI** declared in [`Native/CarPhysics/include/car_physics.h`](../Native/CarPhysics/include/car_physics.h). Plain C makes the module consumable from C# (P/Invoke), Unreal C++, Python (`ctypes`) — anything that can call a C function.

- [Conventions](#conventions)
- [Functions](#functions)
- [Configuration structs](#configuration-structs)
- [Per-tick structs](#per-tick-structs)

## Conventions

| Convention | Value |
| --- | --- |
| Wheel indexing | `0` front-left · `1` front-right · `2` rear-left · `3` rear-right (`CARSIM_WHEEL_COUNT = 4`) |
| Coordinate system | **The host's own.** The module never assumes axes — it works with the wheel-root basis vectors (`right`, `up`, `forward`) the host supplies |
| Units | SI: metres, seconds, newtons, N·m, rad/s; angles in degrees where noted; engine speed in RPM |
| Calling convention | `__cdecl` (`CARPHYSICS_CALL`) |
| Threading | One handle = one car; handles are independent and may be updated from different threads (no shared state) |

## Functions

### `carsim_create`

```c
CP_Handle carsim_create(const CP_CarConfig* config);
```

Creates a simulation instance from a full car description. The config — including curve and gear-ratio arrays — is **deep-copied**, so the host may free its buffers immediately after the call. Returns `NULL` on invalid input (e.g. empty gear table).

### `carsim_destroy`

```c
void carsim_destroy(CP_Handle handle);
```

Destroys an instance created by `carsim_create`. Passing `NULL` is a no-op.

### `carsim_update_drivetrain`

```c
void carsim_update_drivetrain(CP_Handle handle,
                              const CP_DrivetrainInput* in,
                              CP_DrivetrainOutput* out);
```

**Phase 1 of a tick.** Consumes driver input and advances steering, gearbox, clutch, engine, differential and brakes. Returns the steering angles the host must apply to the wheel roots, plus drivetrain telemetry (RPM, gear, clutch state) and an optional body torque to apply while in neutral gear.

### `carsim_update_wheels`

```c
void carsim_update_wheels(CP_Handle handle,
                          const CP_WheelInput* in,
                          CP_WheelOutput* out);
```

**Phase 2 of a tick.** Consumes the ground-contact data gathered by the host (raycast hits, contact normals, rigid-body point velocities) and advances suspension, wheel spin, slip and Pacejka tire models. Returns, per wheel: the total force and the world point to apply it at, wheel-mesh visual transforms, and full telemetry.

### `carsim_version`

```c
const char* carsim_version(void);
```

Returns the module version string, e.g. `"1.0.0"`.

## Configuration structs

### `CP_Curve`

Piecewise-linear curve. Keyframes must be sorted by time ascending; evaluation clamps outside the key range. Arrays are copied inside `carsim_create`.

| Field | Type | Meaning |
| --- | --- | --- |
| `times` | `const float*` | Key positions (e.g. RPM) |
| `values` | `const float*` | Key values (e.g. N·m) |
| `count` | `int` | Number of keys |

> The Unity host samples its Hermite `AnimationCurve` into 64 linear segments before passing it in (see `CarPhysicsNative.MakeCurve`).

### `CP_EngineInfo`

| Field | Meaning |
| --- | --- |
| `torqueCurve` | Torque vs RPM |
| `engineOrientation` | Axis for the neutral-gear body torque (engine-rock effect) |
| `idleRpm`, `maxRpm` | Angular-velocity clamp range |
| `mul` | Global torque multiplier |
| `frictionCoeff` | Friction torque per RPM |
| `startFriction` | Constant friction torque |
| `inertia` | Engine rotational inertia, kg·m² |

### `CP_GearboxInfo`

| Field | Meaning |
| --- | --- |
| `ratios`, `gearCount` | Ratio table: index `0` = reverse, `1` = neutral, `2+` = forward gears |
| `shiftTime` | Seconds spent in neutral while shifting |

### `CP_ClutchInfo`

| Field | Meaning |
| --- | --- |
| `stiffness` | Torque per rad/s of clutch slip |
| `capacity` | Max torque as a fraction of peak engine torque |
| `damping` | Output smoothing factor (0..1) |

### `CP_DifferentialInfo`

| Field | Meaning |
| --- | --- |
| `driveMode` | `CP_DRIVE_FWD` / `CP_DRIVE_RWD` / `CP_DRIVE_AWD` |
| `diffType` | `CP_DIFF_OPEN` / `CP_DIFF_LOCKED` / `CP_DIFF_LSD` |
| `ratio` | Final drive ratio |
| `torqueSplitFront` | AWD only: fraction of torque to the front axle (0..1) |
| `lockingCoeff` | LSD only: bias torque, N·m per rad/s of L/R wheel-speed difference |

### `CP_BrakesInfo`

| Field | Meaning |
| --- | --- |
| `brakeTorqueCurve` | Torque multiplier vs wheel angular speed |
| `maxTorque` | Peak brake torque, N·m |
| `biasFront`, `biasRear` | Per-axle bias factors |
| `handbrakeTorque` | Extra rear-wheel torque at full handbrake, N·m |

### `CP_SteeringInfo`

| Field | Meaning |
| --- | --- |
| `turnRadius` | Reference turn radius for the Ackermann angles, m |
| `steerForce` | Steering gain |
| `maxCorrectionAngle` | Clamp for the lateral-acceleration correction, deg |
| `correctionSpeed` | Correction / smoothing rate |

### `CP_WheelInfo` (×4)

| Field | Meaning |
| --- | --- |
| `restLength` | Suspension rest length, m |
| `suspensionStiffness` | Spring rate, N/m |
| `damperStiffness` | Damper rate, N·s/m |
| `wheelRadius`, `wheelMass` | Wheel geometry & mass (inertia = `r² · m`) |
| `relaxationLength` | Slip-angle relaxation length, m — tire force builds over distance, not instantly |
| `longitudinalCoeff` | Longitudinal friction factor: peak `Fx = μx · Fz` |
| `lateralCoeff` | Lateral friction factor: peak `Fy = μy · Fz` |
| `longSlipPeak` | Slip ratio at peak `Fx` (e.g. `0.12`) |
| `slipAnglePeak` | Slip angle at peak `Fy`, deg |
| `pacejkaShapeLong` / `pacejkaCurveLong` | Magic Formula `C` / `E` for the longitudinal axis |
| `pacejkaShapeLat` / `pacejkaCurveLat` | Magic Formula `C` / `E` for the lateral axis |
| `camber`, `camberCoeff` | Static camber (deg) and camber-thrust factor: `Fy += k · sin(camber) · Fz` |
| `toe` | Static toe per wheel, deg (>0 = toe-in). Applied by the **host** to the wheel-root basis |
| `caster`, `kingpinInclination` | Steering-axis tilts, deg — exposed for host visuals/geometry |
| `longFrictionCoeff` | Reserved |

For each axis the tire force is the Pacejka Magic Formula
`F(x) = D·sin(C·atan(B·x − E·(B·x − atan(B·x))))`, where `D = μ·Fz` and the stiffness factor `B` is derived so the curve peaks exactly at the configured peak slip: `B = tan(π / 2C) / peakSlip`.

### `CP_AntirollBarInfo`

| Field | Meaning |
| --- | --- |
| `isEnabled` | 0 = off |
| `stiffnessFront`, `stiffnessRear` | N per metre of left/right suspension-travel difference |

### `CP_CarConfig`

Aggregates everything above, plus geometry the host measures once from the wheel-root world positions:

| Field | Meaning |
| --- | --- |
| `wheelBase` | Distance between wheel 0 and wheel 2, m |
| `rearTrack` | Distance between wheel 2 and wheel 3, m |

## Per-tick structs

### `CP_DrivetrainInput`

| Field | Range | Meaning |
| --- | --- | --- |
| `dt` | s | Fixed timestep |
| `throttle`, `brake` | 0..1 | Pedals |
| `steer` | −1..1 | Steering |
| `clutch` | 0..1 | 0 = engaged, 1 = fully depressed |
| `handbrake` | 0..1 | Locks the rear wheels |
| `gearUp`, `gearDown` | 0/1 | Edge-triggered: `1` only on the frame the shift was requested |

### `CP_DrivetrainOutput`

| Field | Meaning |
| --- | --- |
| `steerAngles[4]` | Degrees; host applies to the wheel roots (plus static toe) |
| `neutralBodyTorque`, `applyNeutralTorque` | Engine-rock torque to apply to the body while in neutral |
| `engineRpm`, `engineAngularVelocity` | Engine state |
| `currentGear` | `0` = reverse, `1` = neutral, `2+` = forward gears |
| `clutchTorque`, `clutchLock` | Clutch telemetry |

### `CP_WheelState` (host → module, ×4 in `CP_WheelInput`)

| Field | Meaning |
| --- | --- |
| `position` | Wheel-root world position |
| `right`, `up`, `forward` | Wheel-root world basis **after** steering was applied |
| `hit` | 1 if the suspension raycast hit the ground |
| `hitPoint`, `hitNormal` | World contact point and normal |
| `pointVelocity` | Rigid-body velocity at `hitPoint`, world space |

### `CP_WheelOutput` (module → host)

| Field | Meaning |
| --- | --- |
| `applyForce[4]`, `applyPoint[4]` | Combined suspension + tire force per wheel and the world point to apply it at (`AddForceAtPosition` equivalent) |
| `visualPosition[4]` | World position for the wheel mesh |
| `spinEulerX[4]`, `steerEulerY[4]` | Local Euler angles for the spinning part / steering |
| `angularVelocity[4]` | Wheel spin, rad/s |
| `suspensionForce[4]`, `currentLength[4]` | Suspension state (`Fz`, length) |
| `linearVelocity[4]` | Contact velocity in wheel-local axes (x = lateral, z = longitudinal) |
| `slipAngle[4]`, `slipForceLong[4]`, `slipForceLat[4]` | Slip telemetry (deg / slip ratio / deg) |
| `lateralAcceleration[4]` | Feeds the steering correction |
| `normalizedTireMagnitude[4]` | 0..1 friction-ellipse usage — drives skid audio/effects |
| `fx[4]`, `fy[4]` | Tire forces along the wheel axes, N |
