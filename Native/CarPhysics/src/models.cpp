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
    // Index convention: 0=FL, 1=FR, 2=RL, 3=RR. The brake-torque curve is
    // looked up per axle from that axle's mean wheel speed.
    float frontSpeed = std::fabs((angularVelocities[0] + angularVelocities[1]) * 0.5f);
    float rearSpeed  = std::fabs((angularVelocities[2] + angularVelocities[3]) * 0.5f);

    m_brakeTorque[0] = brakeInput * m_info.biasFront * m_info.maxTorque * m_curve.evaluate(frontSpeed);
    m_brakeTorque[1] = m_brakeTorque[0];
    m_brakeTorque[2] = brakeInput * m_info.biasRear * m_info.maxTorque * m_curve.evaluate(rearSpeed);
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
                               float groundForwardSpeed, float suspensionForce,
                               float dt) {
    // The longitudinal tire force is very stiff w.r.t. wheel speed (its slope
    // is B*C*D, scaled up at low ground speed). Integrating it explicitly makes
    // the wheel speed oscillate and blow up at a fixed timestep. We instead
    // treat that dependence implicitly: linearise the tire reaction around the
    // current wheel speed and solve for the new speed. This is unconditionally
    // stable and lets the wheel settle on its rolling speed instead of spinning
    // up to the clamp.
    const float kVelFloor = 1.0f;  // m/s, keeps the slope finite near standstill
    float denom = std::max(std::fabs(groundForwardSpeed), kVelFloor);

    float reactionTorque = fx * m_info.wheelRadius;          // last tick's tire reaction

    float slipRatio = (m_angularVelocity * m_info.wheelRadius - groundForwardSpeed) / denom;
    float dLong = m_info.longitudinalCoeff * std::max(0.0f, suspensionForce);
    float forceSlope = magicFormulaSlope(slipRatio, m_bLong, m_info.pacejkaShapeLong,
                                         dLong, m_info.pacejkaCurveLong);
    // dTorque/dOmega = R * dF/dslip * dslip/dOmega, dslip/dOmega = R/denom.
    float torqueSlope = std::max(0.0f, forceSlope) * m_info.wheelRadius * m_info.wheelRadius / denom;

    float deltaOmega = (driveTorque - reactionTorque) / (m_wheelInertia / dt + torqueSlope);
    m_angularVelocity += deltaOmega;

    // Braking always opposes the current spin and can at most bring the wheel
    // to a stop this tick (the min() prevents braking from reversing it).
    float brakeDelta = brakeTorque / m_wheelInertia * dt;  // >= 0
    m_angularVelocity -= signf(m_angularVelocity) * std::min(std::fabs(m_angularVelocity), brakeDelta);

    m_angularVelocity = clampf(m_angularVelocity, -360.0f, 360.0f);
}

/* -------------------------------------------------------------------- slip */
void SlipWheel::update(const Vec3& linearVelocity, float suspensionForce,
                       float angularVelocity, float dt) {
    // A velocity floor keeps the slip ratio / slip angle finite near standstill
    // (both are otherwise singular as the forward speed approaches zero).
    const float kVelFloor = 1.0f;  // m/s
    float vLong = linearVelocity.z;
    float vLat  = linearVelocity.x;
    float denom = std::max(std::fabs(vLong), kVelFloor);

    // longitudinal slip ratio: (wheel surface speed - ground speed) / ground speed
    float wheelSurfaceSpeed = angularVelocity * m_info.wheelRadius;
    m_slipRatio = (wheelSurfaceSpeed - vLong) / denom;
    m_slipRatio = clampf(m_slipRatio, -4.0f, 4.0f);

    // lateral slip angle (radians), relaxed over the relaxation length so the
    // tire force builds up over travelled distance rather than instantly.
    float slipAngle = std::atan(-vLat / denom);
    float relax = m_info.relaxationLength > 1e-4f
                      ? clamp01(std::fabs(vLong) * dt / m_info.relaxationLength)
                      : 1.0f;
    m_dynamicSlipAngle += (slipAngle - m_dynamicSlipAngle) * relax;
    m_dynamicSlipAngle = clampf(m_dynamicSlipAngle, -PI * 0.5f, PI * 0.5f);

    float mag = magnitude(linearVelocity);
    m_lateralAcceleration = (mag * mag / m_info.wheelRadius) * std::tan(m_dynamicSlipAngle);
}

/* -------------------------------------------------------------------- tire */
void TireWheel::init(const CP_WheelInfo& info) {
    m_info = info;
    m_bLong = magicStiffnessFromPeak(info.pacejkaShapeLong, info.longSlipPeak);
    m_bLat  = magicStiffnessFromPeak(info.pacejkaShapeLat, info.slipAnglePeak * DEG2RAD);
    m_fx = 0.0f;
    m_normalizedMagnitude = 0.0f;
}

Vec3 TireWheel::update(const CP_WheelState& w, float slipRatio, float slipAngleRad,
                       float suspensionForce) {
    Vec3 forwardProj = normalized(projectOnPlane(Vec3(w.forward), Vec3(w.hitNormal)));
    Vec3 sideProj = normalized(projectOnPlane(Vec3(w.right), Vec3(w.hitNormal)));

    // Peak forces scale with normal load (friction-factor * Fz).
    float dLong = m_info.longitudinalCoeff * suspensionForce;
    float dLat  = m_info.lateralCoeff * suspensionForce;

    float fx0 = magicFormula(slipRatio,    m_bLong, m_info.pacejkaShapeLong, dLong, m_info.pacejkaCurveLong);
    float fy0 = magicFormula(slipAngleRad, m_bLat,  m_info.pacejkaShapeLat,  dLat,  m_info.pacejkaCurveLat);

    // Combined slip: keep the resultant inside the friction ellipse so the tire
    // cannot deliver more than its limit when slipping in both directions.
    float gx = dLong > 1e-4f ? fx0 / dLong : 0.0f;
    float gy = dLat  > 1e-4f ? fy0 / dLat  : 0.0f;
    float g = std::sqrt(gx * gx + gy * gy);
    float scale = g > 1.0f ? 1.0f / g : 1.0f;

    m_fx = fx0 * scale;
    float fY = fy0 * scale;

    // Skid intensity: how close the tire is to (or beyond) its friction limit.
    m_normalizedMagnitude = clamp01(g);

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
