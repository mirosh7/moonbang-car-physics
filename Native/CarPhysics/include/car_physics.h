/*
 * car_physics.h
 *
 * Public C ABI for the universal car-simulation module.
 *
 * Design: data-in / data-out. The DLL contains ONLY the math (engine,
 * gearbox, clutch, differential, brakes, steering, suspension, slip, tire
 * and wheel acceleration). It never touches a game engine: it does no
 * raycasting, applies no forces and reads no transforms by itself.
 *
 * The HOST (Unity, Unreal, a Python lab harness, ...) is responsible for:
 *   - performing the wheel ground raycasts,
 *   - reading the rigid-body point velocity at each contact,
 *   - feeding the world-space basis of every wheel root,
 *   - applying the forces / torques the DLL returns.
 *
 * Per simulation tick the host calls, in order:
 *   1) carsim_update_drivetrain(...) -> steer angles + drivetrain telemetry
 *   2) host applies steer angles to the wheel roots, raycasts, gathers
 *      contact data and point velocities
 *   3) carsim_update_wheels(...) -> forces to apply + visuals + telemetry
 *   4) host applies the returned forces at the returned points
 *
 * All vectors are expressed in the host's own coordinate system. The module
 * is coordinate-system agnostic because the host supplies the wheel-root
 * basis vectors (right / up / forward) directly instead of a quaternion.
 *
 * The ABI is plain C so it can be consumed from C# (P/Invoke), Unreal C++,
 * Python (ctypes), etc.
 */
#ifndef CAR_PHYSICS_H
#define CAR_PHYSICS_H

#ifdef _WIN32
  #ifdef CARPHYSICS_BUILD_DLL
    #define CARPHYSICS_API __declspec(dllexport)
  #else
    #define CARPHYSICS_API __declspec(dllimport)
  #endif
  #define CARPHYSICS_CALL __cdecl
#else
  #define CARPHYSICS_API __attribute__((visibility("default")))
  #define CARPHYSICS_CALL
#endif

#ifdef __cplusplus
extern "C" {
#endif

/* The drivetrain math (differential / brakes / steering) is wired for a
 * 4-wheel car. Wheel index convention, matching the original C# project:
 *   0 = front-left, 1 = front-right, 2 = rear-left, 3 = rear-right. */
#define CARSIM_WHEEL_COUNT 4

typedef struct CP_Vec3 { float x, y, z; } CP_Vec3;

/* Piecewise-linear curve. Keyframes must be sorted by time ascending.
 * Evaluation clamps to the first/last value outside the key range. The host
 * owns the arrays only for the duration of carsim_create; the module copies
 * them internally. */
typedef struct CP_Curve {
    const float* times;
    const float* values;
    int          count;
} CP_Curve;

typedef struct CP_EngineInfo {
    CP_Curve torqueCurve;       /* torque vs rpm */
    CP_Vec3  engineOrientation; /* axis used for the neutral-gear body torque */
    float    idleRpm;
    float    maxRpm;
    float    mul;               /* engineMul */
    float    frictionCoeff;     /* engineFrictionCoefficient */
    float    startFriction;
    float    inertia;           /* engineInertia */
} CP_EngineInfo;

typedef struct CP_GearboxInfo {
    const float* ratios;        /* index 0 is reverse/neutral as authored */
    int          gearCount;
    float        shiftTime;     /* seconds spent in neutral while shifting */
} CP_GearboxInfo;

typedef struct CP_ClutchInfo {
    float stiffness;
    float capacity;
    float damping;
} CP_ClutchInfo;

typedef struct CP_DifferentialInfo {
    int   isLocked;             /* 0 = open, non-zero = locked */
    float ratio;                /* differentialRatio */
} CP_DifferentialInfo;

typedef struct CP_BrakesInfo {
    CP_Curve brakeTorqueCurve;  /* multiplier vs wheel angular speed */
    float    maxTorque;
    float    biasFront;         /* brakeBias[0] */
    float    biasRear;          /* brakeBias[1] */
} CP_BrakesInfo;

typedef struct CP_SteeringInfo {
    float turnRadius;
    float steerForce;
    float maxCorrectionAngle;
    float correctionSpeed;
} CP_SteeringInfo;

typedef struct CP_WheelInfo {
    float restLength;
    float suspensionStiffness;
    float damperStiffness;
    float slipAnglePeak;        /* slip angle (deg) at peak lateral force; sets Pacejka B_lat */
    float camber;               /* exposed for visuals; not used by the math */
    float caster;               /* idem */
    float longitudinalCoeff;    /* longitudinal peak-friction factor: Dx = coeff * Fz */
    float lateralCoeff;         /* lateral peak-friction factor:      Dy = coeff * Fz */
    float wheelRadius;
    float wheelMass;            /* wheelInertia = wheelRadius^2 * wheelMass */
    float longFrictionCoeff;
    float relaxationLength;

    /* ---- Pacejka Magic Formula shape parameters ----
     * Per axis force: F(x) = D*sin(C*atan(B*x - E*(B*x - atan(B*x)))).
     * D comes from the friction factor * normal load (above). The stiffness
     * factor B is derived so the curve peaks at the configured peak slip
     * (longSlipPeak for the slip ratio, slipAnglePeak for the slip angle):
     *   B = tan(pi / (2*C)) / peakSlip. */
    float longSlipPeak;         /* slip ratio at peak longitudinal force (e.g. 0.12) */
    float pacejkaShapeLong;     /* C_x (e.g. 1.65) */
    float pacejkaCurveLong;     /* E_x (e.g. 0.96) */
    float pacejkaShapeLat;      /* C_y (e.g. 1.35) */
    float pacejkaCurveLat;      /* E_y (e.g. 0.96) */
} CP_WheelInfo;

typedef struct CP_CarConfig {
    CP_EngineInfo       engine;
    CP_GearboxInfo      gearbox;
    CP_ClutchInfo       clutch;
    CP_DifferentialInfo differential;
    CP_BrakesInfo       brakes;
    CP_SteeringInfo     steering;
    CP_WheelInfo        wheels[CARSIM_WHEEL_COUNT];

    /* Ackermann geometry, measured once by the host from the wheel-root
     * world positions:
     *   wheelBase = distance(wheel[0], wheel[2])
     *   rearTrack = distance(wheel[2], wheel[3]) */
    float wheelBase;
    float rearTrack;
} CP_CarConfig;

/* ---- per-tick drivetrain phase ---- */

typedef struct CP_DrivetrainInput {
    float dt;        /* fixed timestep, seconds */
    float throttle;  /* 0..1 */
    float brake;     /* 0..1 */
    float steer;     /* -1..1 */
    int   gearUp;    /* 1 on the frame the shift-up was requested, else 0 */
    int   gearDown;  /* 1 on the frame the shift-down was requested, else 0 */
} CP_DrivetrainInput;

typedef struct CP_DrivetrainOutput {
    float   steerAngles[CARSIM_WHEEL_COUNT]; /* degrees, host applies to roots */
    CP_Vec3 neutralBodyTorque;               /* apply when applyNeutralTorque */
    int     applyNeutralTorque;
    float   engineRpm;
    float   engineAngularVelocity;
    int     currentGear;
    float   clutchTorque;
    float   clutchLock;
} CP_DrivetrainOutput;

/* ---- per-tick wheel phase ---- */

typedef struct CP_WheelState {
    CP_Vec3 position;       /* wheel-root world position */
    CP_Vec3 right;          /* wheel-root world basis (after steering) */
    CP_Vec3 up;
    CP_Vec3 forward;
    int     hit;            /* 1 if the ground raycast hit */
    CP_Vec3 hitPoint;       /* world contact point */
    CP_Vec3 hitNormal;      /* world contact normal */
    CP_Vec3 pointVelocity;  /* rigid-body velocity at hitPoint, world space */
} CP_WheelState;

typedef struct CP_WheelInput {
    float         dt;
    CP_WheelState wheels[CARSIM_WHEEL_COUNT];
} CP_WheelInput;

typedef struct CP_WheelOutput {
    /* Forces to apply with the engine equivalent of AddForceAtPosition.
     * applyForce already combines suspension + tire force for the wheel. */
    CP_Vec3 applyForce[CARSIM_WHEEL_COUNT];
    CP_Vec3 applyPoint[CARSIM_WHEEL_COUNT];

    /* Visuals, ready to apply on the host transforms. */
    CP_Vec3 visualPosition[CARSIM_WHEEL_COUNT];
    float   spinEulerX[CARSIM_WHEEL_COUNT];   /* rotating part local euler X */
    float   steerEulerY[CARSIM_WHEEL_COUNT];  /* visual local euler Y */

    /* Telemetry. */
    float angularVelocity[CARSIM_WHEEL_COUNT];
    float suspensionForce[CARSIM_WHEEL_COUNT];
    float currentLength[CARSIM_WHEEL_COUNT];
    CP_Vec3 linearVelocity[CARSIM_WHEEL_COUNT]; /* wheel-local, for telemetry */
    float slipAngle[CARSIM_WHEEL_COUNT];
    float lateralAcceleration[CARSIM_WHEEL_COUNT];
    float slipForceLong[CARSIM_WHEEL_COUNT];
    float slipForceLat[CARSIM_WHEEL_COUNT];
    float normalizedTireMagnitude[CARSIM_WHEEL_COUNT]; /* drives skid sound */
    float fx[CARSIM_WHEEL_COUNT];
} CP_WheelOutput;

/* Opaque simulation handle. */
typedef void* CP_Handle;

/* Create a simulation from a config. Returns NULL on invalid input.
 * The config (including curve and gear-ratio arrays) is deep-copied. */
CARPHYSICS_API CP_Handle CARPHYSICS_CALL carsim_create(const CP_CarConfig* config);

/* Destroy a simulation created by carsim_create. NULL is ignored. */
CARPHYSICS_API void CARPHYSICS_CALL carsim_destroy(CP_Handle handle);

/* Phase 1: steering + engine + gearbox + clutch + differential + brakes. */
CARPHYSICS_API void CARPHYSICS_CALL carsim_update_drivetrain(
    CP_Handle handle, const CP_DrivetrainInput* in, CP_DrivetrainOutput* out);

/* Phase 2: suspension + wheel acceleration + slip + tire + visuals. */
CARPHYSICS_API void CARPHYSICS_CALL carsim_update_wheels(
    CP_Handle handle, const CP_WheelInput* in, CP_WheelOutput* out);

/* Version string, e.g. "1.0.0". */
CARPHYSICS_API const char* CARPHYSICS_CALL carsim_version(void);

#ifdef __cplusplus
} /* extern "C" */
#endif

#endif /* CAR_PHYSICS_H */
