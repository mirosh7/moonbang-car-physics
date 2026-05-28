/*
 * models.cpp - implementations ported 1:1 from the C# physics models.
 */
#include "models.h"

#include <algorithm>
#include <limits>

namespace carsim {

/* ------------------------------------------------------------------ engine */
void EngineModel::init(const CP_EngineInfo& info) {
    m_info = info;
    m_torqueCurve.assign(info.torqueCurve);
    m_rpm = 0.0f;
    m_angularVelocity = 100.0f;  // matches the original constructor
}

bool EngineModel::update(float throttle, float clutchTorque, int currentGear,
                         float dt, Vec3& outBodyTorque) {
    float maxEffectiveTorque = m_torqueCurve.evaluate(m_rpm) * m_info.mul;
    float engineFriction = (m_rpm * m_info.frictionCoeff) + m_info.startFriction;
    float engineTorque = maxEffectiveTorque * throttle - engineFriction;
    float engineAcceleration = (engineTorque - clutchTorque) / m_info.inertia;

    m_angularVelocity += engineAcceleration * dt;
    m_rpm = m_angularVelocity * RAD_TO_RPM;  // computed before clamp, as in C#
    m_angularVelocity = clampf(m_angularVelocity,
                               m_info.idleRpm * RPM_TO_RAD,
                               m_info.maxRpm * RPM_TO_RAD);

    if (currentGear == 0) {
        outBodyTorque = Vec3(m_info.engineOrientation) * (engineTorque * 2.0f);
        return true;
    }
    return false;
}

/* ----------------------------------------------------------------- gearbox */
void GearboxModel::init(const CP_GearboxInfo& info) {
    m_ratios.clear();
    if (info.ratios && info.gearCount > 0) {
        m_ratios.assign(info.ratios, info.ratios + info.gearCount);
    }
    m_shiftTime = info.shiftTime;
    m_currentGear = 1;
    m_targetGear = 1;
    m_isShifting = false;
    m_shiftTimer = 0.0f;
}

void GearboxModel::requestShiftUp() {
    if (!m_isShifting && m_currentGear < static_cast<int>(m_ratios.size()) - 1) {
        m_targetGear = m_currentGear + 1;
        m_isShifting = true;
        m_shiftTimer = m_shiftTime;
        m_currentGear = 1;  // neutral while shifting, as in the original
    }
}

void GearboxModel::requestShiftDown() {
    if (!m_isShifting && m_currentGear != 0) {
        m_targetGear = m_currentGear - 1;
        m_isShifting = true;
        m_shiftTimer = m_shiftTime;
        m_currentGear = 1;
    }
}

void GearboxModel::tick(float dt) {
    if (!m_isShifting) return;
    m_shiftTimer -= dt;
    if (m_shiftTimer <= 0.0f) {
        m_currentGear = m_targetGear;
        m_isShifting = false;
    }
}

/* ------------------------------------------------------------------ clutch */
void ClutchModel::init(const CP_ClutchInfo& info, float maxEngineTorque) {
    m_info = info;
    m_maxEngineTorque = maxEngineTorque;
    m_clutchTorque = 0.0f;
    m_clutchLock = 0.0f;
}

void ClutchModel::update(float engineAngularVelocity, float currentGearRatio,
                         float gearBoxInputShaftVelocity) {
    float clutchMaxTorque = m_maxEngineTorque * m_info.capacity;
    float clutchSlip = (engineAngularVelocity - gearBoxInputShaftVelocity)
                       * std::fabs(signf(currentGearRatio));
    m_clutchLock = currentGearRatio == 0.0f
                       ? 0.0f
                       : mapRangeClamped(engineAngularVelocity * RAD_TO_RPM,
                                         1000.0f, 1300.0f, 0.0f, 1.0f);
    float clt = clampf(clutchSlip * m_clutchLock * m_info.stiffness,
                       -clutchMaxTorque, clutchMaxTorque);
    m_clutchTorque = clt + ((m_clutchTorque - clt) * m_info.damping);
}

/* ------------------------------------------------------------ differential */
void DifferentialModel::init(const CP_DifferentialInfo& info,
                             const CP_WheelInfo& wheel0) {
    m_info = info;
    m_wheelInertia = wheel0.wheelRadius * wheel0.wheelRadius * wheel0.wheelMass;
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) m_outputTorque[i] = 0.0f;
}

void DifferentialModel::update(float inputTorque,
                               const float angularVelocities[CARSIM_WHEEL_COUNT],
                               float dt) {
    m_angularVelocityL = angularVelocities[2];
    m_angularVelocityR = angularVelocities[3];

    if (!m_info.isLocked) {
        float vel = (m_angularVelocityL - m_angularVelocityR) * 0.5f / dt * m_wheelInertia;
        float base = inputTorque * 0.5f * m_info.ratio;
        m_outputTorque[0] = base - vel;
        m_outputTorque[1] = base + vel;
        m_outputTorque[2] = base - vel;
        m_outputTorque[3] = base + vel;
    } else {
        float base = inputTorque * m_info.ratio * 0.5f;
        for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) m_outputTorque[i] = base;
    }
}

/* ------------------------------------------------------------------ brakes */
void BrakesModel::init(const CP_BrakesInfo& info) {
    m_info = info;
    m_curve.assign(info.brakeTorqueCurve);
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) m_brakeTorque[i] = 0.0f;
}

void BrakesModel::update(float brakeInput,
                         const float angularVelocities[CARSIM_WHEEL_COUNT]) {
    m_brakeTorque[0] = brakeInput * m_info.biasFront * m_info.maxTorque *
        m_curve.evaluate(std::fabs((angularVelocities[0] + angularVelocities[2]) * 0.5f));
    m_brakeTorque[1] = m_brakeTorque[0];
    m_brakeTorque[2] = brakeInput * m_info.biasRear * m_info.maxTorque *
        m_curve.evaluate(std::fabs((angularVelocities[1] + angularVelocities[3]) * 0.5f));
    m_brakeTorque[3] = m_brakeTorque[2];
}

/* ---------------------------------------------------------------- steering */
void SteeringModel::init(const CP_SteeringInfo& info, float wheelBase,
                         float rearTrack) {
    m_info = info;
    m_wheelBase = wheelBase;
    m_rearTrack = rearTrack;
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) m_steerAngles[i] = 0.0f;
}

void SteeringModel::update(float inputSteering,
                           const float lateralAccelerations[CARSIM_WHEEL_COUNT],
                           float dt) {
    float in = inputSteering;

    // Ackermann
    float denomL = in > 0 ? m_info.turnRadius + m_rearTrack / 2.0f
                          : m_info.turnRadius - m_rearTrack / 2.0f;
    float denomR = in > 0 ? m_info.turnRadius - m_rearTrack / 2.0f
                          : m_info.turnRadius + m_rearTrack / 2.0f;
    m_ackermannAngleL = RAD2DEG * std::atan(m_wheelBase / denomL) * in * m_info.steerForce;
    m_ackermannAngleR = RAD2DEG * std::atan(m_wheelBase / denomR) * in * m_info.steerForce;

    // Correction from lateral acceleration
    float correctionFactor = m_info.correctionSpeed * dt;
    m_ackermannAngleL -= lateralAccelerations[1] * correctionFactor;
    m_ackermannAngleR -= lateralAccelerations[0] * correctionFactor;
    m_ackermannAngleL = clampf(m_ackermannAngleL, -m_info.maxCorrectionAngle, m_info.maxCorrectionAngle);
    m_ackermannAngleR = clampf(m_ackermannAngleR, -m_info.maxCorrectionAngle, m_info.maxCorrectionAngle);

    float t = m_info.correctionSpeed * dt;
    m_steerAngles[0] = lerp(m_steerAngles[0], m_ackermannAngleR, t);
    m_steerAngles[1] = lerp(m_steerAngles[1], m_ackermannAngleL, t);
    // rear wheels (2,3) stay 0
}

/* -------------------------------------------------------------- suspension */
Vec3 SuspensionWheel::update(const CP_WheelState& w, float dt) {
    Vec3 up(w.up);
    Vec3 pos(w.position);
    Vec3 hitPoint(w.hitPoint);

    m_currentLength = magnitude(pos - (hitPoint + up * m_info.wheelRadius));

    float springForce = (m_info.restLength - m_currentLength) * m_info.suspensionStiffness;
    float damperForce = (m_lastLength - m_currentLength) / dt * m_info.damperStiffness;
    m_suspensionForce = std::max(0.0f, springForce + damperForce);
    m_lastLength = m_currentLength;

    m_linearVelocity = inverseTransformDirection(Vec3(w.pointVelocity),
                                                 Vec3(w.right), up, Vec3(w.forward));

    return up * m_suspensionForce;  // force to add at hitPoint
}

/* ------------------------------------------------------------ acceleration */
void AccelerationWheel::update(float fx, float driveTorque, float brakeTorque,
                               float dt) {
    float frictionTorque = fx * m_info.wheelRadius;
    float angularAcceleration = (driveTorque - frictionTorque) / m_wheelInertia;
    m_angularVelocity += angularAcceleration * dt;
    m_angularVelocity -= std::min(std::fabs(m_angularVelocity),
                                  brakeTorque * signf(m_angularVelocity) / m_wheelInertia * dt);
    m_angularVelocity = clampf(m_angularVelocity, -360.0f, 360.0f);
}

/* -------------------------------------------------------------------- slip */
void SlipWheel::update(const Vec3& linearVelocity, float suspensionForce,
                       float angularVelocity, float dt) {
    // longitudinal
    float targetAngularVelocity = linearVelocity.z / m_info.wheelRadius;
    float targetAngularAcceleration = (angularVelocity - targetAngularVelocity) / dt;
    float targetFrictionTorque = targetAngularAcceleration * m_wheelInertia;
    float maximumFrictionTorque = suspensionForce * m_info.wheelRadius * m_info.longFrictionCoeff;
    m_slipLong = suspensionForce == 0.0f ? 0.0f
                                         : targetFrictionTorque / maximumFrictionTorque;

    // lateral
    const float eps = std::numeric_limits<float>::epsilon();
    m_slipAngle = std::fabs(linearVelocity.z) < eps
                      ? 0.0f
                      : std::atan(-linearVelocity.x / std::fabs(linearVelocity.z)) * RAD2DEG;
    float coeff = (std::fabs(linearVelocity.x) / m_info.relaxationLength) * dt;
    coeff = clampf(coeff, 0.0f, 1.0f);
    m_dynamicSlipAngle += (m_slipAngle - m_dynamicSlipAngle) * coeff;
    m_dynamicSlipAngle = clampf(m_dynamicSlipAngle, -90.0f, 90.0f);
    m_slipLat = m_dynamicSlipAngle / m_info.slipAnglePeak;

    float mag = magnitude(linearVelocity);
    m_lateralAcceleration = (mag * mag / m_info.wheelRadius) * std::tan(m_slipAngle * DEG2RAD);
}

/* -------------------------------------------------------------------- tire */
Vec3 TireWheel::update(const CP_WheelState& w, float longitudinalForce,
                       float lateralForce, float suspensionForce) {
    Vec3 forwardProj = normalized(projectOnPlane(Vec3(w.forward), Vec3(w.hitNormal)));
    Vec3 sideProj = normalized(projectOnPlane(Vec3(w.right), Vec3(w.hitNormal)));

    float cx = longitudinalForce;
    float cy = lateralForce;
    float cmag = std::sqrt(cx * cx + cy * cy);
    if (cmag > 1.0f) { cx /= cmag; cy /= cmag; }

    m_fx = cx * suspensionForce * m_info.longitudinalCoeff;
    float fY = cy * suspensionForce * m_info.lateralCoeff;

    // normalizedForce magnitude (drives skid sound). normalize(combined) has
    // magnitude 1 when non-zero, 0 otherwise - matches Vector2.normalized.
    m_normalizedMagnitude = cmag > 1e-8f ? 1.0f : 0.0f;

    return forwardProj * m_fx + sideProj * fY;  // force to add at hitPoint
}

/* ------------------------------------------------------------------ visual */
void VisualWheel::update(float angularVelocity, float steerAngle,
                         bool isOppositeSide, float dt, float& outSpinEulerX,
                         float& outSteerEulerY) {
    m_currentAngle += angularVelocity * RAD2DEG * dt;
    m_currentAngle = std::fmod(m_currentAngle, 360.0f);

    outSpinEulerX = isOppositeSide ? -m_currentAngle : m_currentAngle;
    outSteerEulerY = isOppositeSide ? steerAngle + 180.0f : steerAngle;
}

} // namespace carsim
