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

#define CARSIM_WHEEL_COUNT 4

typedef struct CP_Vec3 { float x, y, z; } CP_Vec3;

typedef struct CP_Curve {
    const float* times;
    const float* values;
    int          count;
} CP_Curve;

typedef struct CP_EngineInfo {
    CP_Curve torqueCurve;
    CP_Vec3  engineOrientation;
    float    idleRpm;
    float    maxRpm;
    float    mul;
    float    frictionCoeff;
    float    startFriction;
    float    inertia;
} CP_EngineInfo;

typedef struct CP_GearboxInfo {
    const float* ratios;
    int          gearCount;
    float        shiftTime;
} CP_GearboxInfo;

typedef struct CP_ClutchInfo {
    float stiffness;
    float capacity;
    float damping;
} CP_ClutchInfo;

enum { CP_DRIVE_FWD = 0, CP_DRIVE_RWD = 1, CP_DRIVE_AWD = 2 };
enum { CP_DIFF_OPEN = 0, CP_DIFF_LOCKED = 1, CP_DIFF_LSD = 2 };

typedef struct CP_DifferentialInfo {
    int   driveMode;
    int   diffType;
    float ratio;
    float torqueSplitFront;
    float lockingCoeff;
} CP_DifferentialInfo;

typedef struct CP_BrakesInfo {
    CP_Curve brakeTorqueCurve;
    float    maxTorque;
    float    biasFront;
    float    biasRear;
    float    handbrakeTorque;
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
    float slipAnglePeak;
    float camber;
    float caster;
    float longitudinalCoeff;
    float lateralCoeff;
    float wheelRadius;
    float wheelMass;
    float longFrictionCoeff;
    float relaxationLength;

    float longSlipPeak;
    float pacejkaShapeLong;
    float pacejkaCurveLong;
    float pacejkaShapeLat;
    float pacejkaCurveLat;

    float toe;
    float kingpinInclination;
    float camberCoeff;
} CP_WheelInfo;

typedef struct CP_AntirollBarInfo {
    int   isEnabled;
    float stiffnessFront;
    float stiffnessRear;
} CP_AntirollBarInfo;

typedef struct CP_CarConfig {
    CP_EngineInfo       engine;
    CP_GearboxInfo      gearbox;
    CP_ClutchInfo       clutch;
    CP_DifferentialInfo differential;
    CP_BrakesInfo       brakes;
    CP_SteeringInfo     steering;
    CP_WheelInfo        wheels[CARSIM_WHEEL_COUNT];
    CP_AntirollBarInfo  antiroll;

    float wheelBase;
    float rearTrack;
} CP_CarConfig;

typedef struct CP_DrivetrainInput {
    float dt;
    float throttle;
    float brake;
    float steer;
    float clutch;
    float handbrake;
    int   gearUp;
    int   gearDown;
} CP_DrivetrainInput;

typedef struct CP_DrivetrainOutput {
    float   steerAngles[CARSIM_WHEEL_COUNT];
    CP_Vec3 neutralBodyTorque;
    int     applyNeutralTorque;
    float   engineRpm;
    float   engineAngularVelocity;
    int     currentGear;
    float   clutchTorque;
    float   clutchLock;
} CP_DrivetrainOutput;

typedef struct CP_WheelState {
    CP_Vec3 position;
    CP_Vec3 right;
    CP_Vec3 up;
    CP_Vec3 forward;
    int     hit;
    CP_Vec3 hitPoint;
    CP_Vec3 hitNormal;
    CP_Vec3 pointVelocity;
} CP_WheelState;

typedef struct CP_WheelInput {
    float         dt;
    CP_WheelState wheels[CARSIM_WHEEL_COUNT];
} CP_WheelInput;

typedef struct CP_WheelOutput {
    CP_Vec3 applyForce[CARSIM_WHEEL_COUNT];
    CP_Vec3 applyPoint[CARSIM_WHEEL_COUNT];

    CP_Vec3 visualPosition[CARSIM_WHEEL_COUNT];
    float   spinEulerX[CARSIM_WHEEL_COUNT];
    float   steerEulerY[CARSIM_WHEEL_COUNT];

    float angularVelocity[CARSIM_WHEEL_COUNT];
    float suspensionForce[CARSIM_WHEEL_COUNT];
    float currentLength[CARSIM_WHEEL_COUNT];
    CP_Vec3 linearVelocity[CARSIM_WHEEL_COUNT];
    float slipAngle[CARSIM_WHEEL_COUNT];
    float lateralAcceleration[CARSIM_WHEEL_COUNT];
    float slipForceLong[CARSIM_WHEEL_COUNT];
    float slipForceLat[CARSIM_WHEEL_COUNT];
    float normalizedTireMagnitude[CARSIM_WHEEL_COUNT];
    float fx[CARSIM_WHEEL_COUNT];
    float fy[CARSIM_WHEEL_COUNT];
} CP_WheelOutput;

typedef void* CP_Handle;

CARPHYSICS_API CP_Handle CARPHYSICS_CALL carsim_create(const CP_CarConfig* config);

CARPHYSICS_API void CARPHYSICS_CALL carsim_destroy(CP_Handle handle);

CARPHYSICS_API void CARPHYSICS_CALL carsim_update_drivetrain(
    CP_Handle handle, const CP_DrivetrainInput* in, CP_DrivetrainOutput* out);

CARPHYSICS_API void CARPHYSICS_CALL carsim_update_wheels(
    CP_Handle handle, const CP_WheelInput* in, CP_WheelOutput* out);

CARPHYSICS_API const char* CARPHYSICS_CALL carsim_version(void);

#ifdef __cplusplus
}
#endif

#endif
