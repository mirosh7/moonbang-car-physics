/*
 * models.h - C++ port of the C# physics models / wheel components.
 *
 * Each class reproduces the math of its Unity counterpart 1:1. All engine
 * coupling (raycasts, AddForce, GetPointVelocity, Transform reads) is
 * removed: inputs arrive as plain values, outputs are returned as plain
 * values, and the host applies them.
 */
#ifndef CARSIM_MODELS_H
#define CARSIM_MODELS_H

#include "math_util.h"
#include "car_physics.h"

namespace carsim {

/* ------------------------------------------------------------------ engine */
class EngineModel {
public:
    void init(const CP_EngineInfo& info);

    /* Returns true and fills outBodyTorque when the gear is neutral (0). */
    bool update(float throttle, float clutchTorque, int currentGear, float dt,
                Vec3& outBodyTorque);

    float angularVelocity() const { return m_angularVelocity; }
    float rpm() const             { return m_rpm; }
    float maxEngineTorque() const { return m_torqueCurve.maxValue(); }

private:
    CP_EngineInfo m_info{};
    Curve m_torqueCurve;
    float m_rpm = 0.0f;
    float m_angularVelocity = 100.0f;
};

/* ----------------------------------------------------------------- gearbox */
class GearboxModel {
public:
    void init(const CP_GearboxInfo& info);

    void requestShiftUp();
    void requestShiftDown();
    void tick(float dt);                         /* advances shift timer */

    float currentGearRatio() const { return m_ratios[m_currentGear]; }
    int   currentGear() const      { return m_currentGear; }

    float inputShaftVelocity(float diffShaftVelocity) const {
        return diffShaftVelocity * currentGearRatio();
    }
    float outputTorque(float inputTorque) const {
        return inputTorque * m_ratios[m_currentGear];
    }

private:
    std::vector<float> m_ratios;
    float m_shiftTime = 0.0f;
    int   m_currentGear = 1;
    int   m_targetGear = 1;
    bool  m_isShifting = false;
    float m_shiftTimer = 0.0f;
};

/* ------------------------------------------------------------------ clutch */
class ClutchModel {
public:
    void init(const CP_ClutchInfo& info, float maxEngineTorque);

    void update(float engineAngularVelocity, float currentGearRatio,
                float gearBoxInputShaftVelocity);

    float torque() const { return m_clutchTorque; }
    float lock() const   { return m_clutchLock; }

private:
    CP_ClutchInfo m_info{};
    float m_maxEngineTorque = 0.0f;
    float m_clutchTorque = 0.0f;
    float m_clutchLock = 0.0f;
};

/* ------------------------------------------------------------ differential */
class DifferentialModel {
public:
    void init(const CP_DifferentialInfo& info, const CP_WheelInfo& wheel0);

    void update(float inputTorque, const float angularVelocities[CARSIM_WHEEL_COUNT],
                float dt);

    float inputShaftVelocity() const {
        return (m_angularVelocityL + m_angularVelocityR) * 0.5f * m_info.ratio;
    }
    const float* outputTorque() const { return m_outputTorque; }

private:
    CP_DifferentialInfo m_info{};
    float m_wheelInertia = 0.0f;
    float m_outputTorque[CARSIM_WHEEL_COUNT] = { 0 };
    float m_angularVelocityL = 0.0f;
    float m_angularVelocityR = 0.0f;
};

/* ------------------------------------------------------------------ brakes */
class BrakesModel {
public:
    void init(const CP_BrakesInfo& info);

    void update(float brakeInput, const float angularVelocities[CARSIM_WHEEL_COUNT]);

    const float* brakeTorque() const { return m_brakeTorque; }

private:
    CP_BrakesInfo m_info{};
    Curve m_curve;
    float m_brakeTorque[CARSIM_WHEEL_COUNT] = { 0 };
};

/* ---------------------------------------------------------------- steering */
class SteeringModel {
public:
    void init(const CP_SteeringInfo& info, float wheelBase, float rearTrack);

    void update(float inputSteering,
                const float lateralAccelerations[CARSIM_WHEEL_COUNT],
                float dt);

    const float* steerAngles() const { return m_steerAngles; }

private:
    CP_SteeringInfo m_info{};
    float m_wheelBase = 0.0f;
    float m_rearTrack = 0.0f;
    float m_ackermannAngleL = 0.0f;
    float m_ackermannAngleR = 0.0f;
    float m_steerAngles[CARSIM_WHEEL_COUNT] = { 0 };
};

/* ------------------------------------------------- per-wheel components --- */

class SuspensionWheel {
public:
    void init(const CP_WheelInfo& info) { m_info = info; }

    /* Computes suspension force + local point velocity, returns the world
     * force to add at the contact point (= suspensionForce * up). */
    Vec3 update(const CP_WheelState& w, float dt);

    float suspensionForce() const { return m_suspensionForce; }
    float currentLength() const   { return m_currentLength; }
    const Vec3& linearVelocity() const { return m_linearVelocity; }

private:
    CP_WheelInfo m_info{};
    float m_suspensionForce = 0.0f;
    float m_currentLength = 0.0f;
    float m_lastLength = 0.0f;
    Vec3  m_linearVelocity;
};

class AccelerationWheel {
public:
    void init(const CP_WheelInfo& info) {
        m_info = info;
        m_wheelInertia = info.wheelRadius * info.wheelRadius * info.wheelMass;
        m_bLong = magicStiffnessFromPeak(info.pacejkaShapeLong, info.longSlipPeak);
    }

    /* Integrates wheel spin. The longitudinal tire reaction is linearised about
     * the current wheel speed (implicit) for stability, which needs the normal
     * load via suspensionForce and the derived Pacejka stiffness m_bLong. */
    void update(float fx, float driveTorque, float brakeTorque,
                float groundForwardSpeed, float suspensionForce, float dt);

    float angularVelocity() const { return m_angularVelocity; }

private:
    CP_WheelInfo m_info{};
    float m_wheelInertia = 0.0f;
    float m_bLong = 0.0f;   /* derived Pacejka longitudinal stiffness factor */
    float m_angularVelocity = 0.0f;
};

class SlipWheel {
public:
    void init(const CP_WheelInfo& info) {
        m_info = info;
        m_wheelInertia = info.wheelRadius * info.wheelRadius * info.wheelMass;
    }

    void update(const Vec3& linearVelocity, float suspensionForce,
                float angularVelocity, float dt);

    float slipRatio() const     { return m_slipRatio; }          /* longitudinal, dimensionless */
    float slipAngleRad() const  { return m_dynamicSlipAngle; }   /* lateral, radians (relaxed) */
    float slipAngle() const     { return m_dynamicSlipAngle * RAD2DEG; } /* telemetry, degrees */
    float lateralAcceleration() const { return m_lateralAcceleration; }

private:
    CP_WheelInfo m_info{};
    float m_wheelInertia = 0.0f;
    float m_slipRatio = 0.0f;
    float m_dynamicSlipAngle = 0.0f;   /* relaxed slip angle, radians */
    float m_lateralAcceleration = 0.0f;
};

class TireWheel {
public:
    void init(const CP_WheelInfo& info);

    /* Pacejka Magic Formula tire. Takes the longitudinal slip ratio and the
     * (relaxed) slip angle in radians; returns the world tire force to add at
     * the contact point. */
    Vec3 update(const CP_WheelState& w, float slipRatio, float slipAngleRad,
                float suspensionForce);

    float fx() const { return m_fx; }
    float fy() const { return m_fy; }   /* lateral tire force (after combined-slip scaling) */
    float normalizedMagnitude() const { return m_normalizedMagnitude; }

private:
    CP_WheelInfo m_info{};
    float m_bLong = 0.0f;   /* derived Pacejka stiffness factors */
    float m_bLat = 0.0f;
    float m_fx = 0.0f;
    float m_fy = 0.0f;
    float m_normalizedMagnitude = 0.0f;
};

class VisualWheel {
public:
    void init() {}

    /* Accumulates spin and produces euler angles ready for the host. */
    void update(float angularVelocity, float steerAngle, bool isOppositeSide,
                float dt, float& outSpinEulerX, float& outSteerEulerY);

private:
    float m_currentAngle = 0.0f;
};

} // namespace carsim

#endif // CARSIM_MODELS_H
