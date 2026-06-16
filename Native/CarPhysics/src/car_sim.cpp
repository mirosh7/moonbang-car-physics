/*
 * car_sim.cpp - orchestration. The two update phases mirror the order the
 * original Unity controllers executed inside one FixedUpdate:
 *
 *   drivetrain: steering -> gearbox/clutch -> engine -> differential -> brakes
 *   wheels:     visuals -> suspension -> acceleration -> slip -> tire
 *
 * Wheel acceleration deliberately consumes the tire fx from the PREVIOUS tick
 * (one-frame lag), exactly as in the C# version.
 */
#include "car_sim.h"

#include <algorithm>

namespace carsim {

bool CarSim::init(const CP_CarConfig& config) {
    if (config.gearbox.gearCount <= 0 || !config.gearbox.ratios) return false;

    m_engine.init(config.engine);
    m_gearbox.init(config.gearbox);
    m_clutch.init(config.clutch, m_engine.maxEngineTorque());
    m_differential.init(config.differential, config.wheels[0]);
    m_brakes.init(config.brakes);
    m_steering.init(config.steering, config.wheelBase, config.rearTrack);
    m_antiroll = config.antiroll;

    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) {
        m_suspension[i].init(config.wheels[i]);
        m_acceleration[i].init(config.wheels[i]);
        m_slip[i].init(config.wheels[i]);
        m_tire[i].init(config.wheels[i]);
        m_visual[i].init();
        m_steerAngles[i] = 0.0f;
        m_restLength[i] = config.wheels[i].restLength;
    }
    return true;
}

void CarSim::updateDrivetrain(const CP_DrivetrainInput& in, CP_DrivetrainOutput& out) {
    float angularVelocities[CARSIM_WHEEL_COUNT];
    float lateralAccelerations[CARSIM_WHEEL_COUNT];
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) {
        angularVelocities[i] = m_acceleration[i].angularVelocity();
        lateralAccelerations[i] = m_slip[i].lateralAcceleration();
    }

    // gear shift requests (edge-triggered by the host) + timer
    if (in.gearUp)   m_gearbox.requestShiftUp();
    if (in.gearDown) m_gearbox.requestShiftDown();
    m_gearbox.tick(in.dt);

    // 1. steering
    m_steering.update(in.steer, lateralAccelerations, in.dt);
    const float* steer = m_steering.steerAngles();
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) m_steerAngles[i] = steer[i];

    // 2. gearbox + clutch
    float diffShaftVelocity = m_differential.inputShaftVelocity();
    float gearBoxInputShaftVelocity = m_gearbox.inputShaftVelocity(diffShaftVelocity);
    m_clutch.update(m_engine.angularVelocity(), m_gearbox.currentGearRatio(),
                    gearBoxInputShaftVelocity, in.clutch);

    // 3. engine
    Vec3 bodyTorque;
    bool applyTorque = m_engine.update(in.throttle, m_clutch.torque(),
                                       m_gearbox.currentGear(), in.dt, bodyTorque);

    // 4. differential
    float gearBoxTorque = m_gearbox.outputTorque(m_clutch.torque());
    m_differential.update(gearBoxTorque, angularVelocities, in.dt);

    // 5. brakes (+ handbrake on the rear)
    m_brakes.update(in.brake, in.handbrake, angularVelocities);

    // outputs
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) out.steerAngles[i] = m_steerAngles[i];
    out.neutralBodyTorque = bodyTorque.toC();
    out.applyNeutralTorque = applyTorque ? 1 : 0;
    out.engineRpm = m_engine.rpm();
    out.engineAngularVelocity = m_engine.angularVelocity();
    out.currentGear = m_gearbox.currentGear();
    out.clutchTorque = m_clutch.torque();
    out.clutchLock = m_clutch.lock();
}

void CarSim::updateWheels(const CP_WheelInput& in, CP_WheelOutput& out) {
    const float dt = in.dt;

    // 0. visuals - use this tick's transforms but last tick's spin/length,
    //    matching the original controller order (visual ran before suspension).
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) {
        const CP_WheelState& w = in.wheels[i];
        bool isOpposite = (i % 2 == 0);
        Vec3 visualPos = Vec3(w.position) - Vec3(w.up) * m_suspension[i].currentLength();
        out.visualPosition[i] = visualPos.toC();
        m_visual[i].update(m_acceleration[i].angularVelocity(), m_steerAngles[i],
                           isOpposite, dt, out.spinEulerX[i], out.steerEulerY[i]);
    }

    // 1. suspension (only grounded wheels)
    Vec3 wheelForce[CARSIM_WHEEL_COUNT];
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) {
        if (in.wheels[i].hit) {
            wheelForce[i] = m_suspension[i].update(in.wheels[i], dt);
        } else {
            wheelForce[i] = Vec3{ 0.f, 0.f, 0.f };
        }
    }

    // 2. wheel acceleration (uses previous-tick tire fx + this-tick drivetrain)
    const float* driveTorque = m_differential.outputTorque();
    const float* brakeTorque = m_brakes.brakeTorque();
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) {
        if (!in.wheels[i].hit) continue;
        m_acceleration[i].update(m_tire[i].fx(), driveTorque[i], brakeTorque[i],
                                 m_suspension[i].linearVelocity().z,
                                 m_suspension[i].suspensionForce(), dt);
    }

    // 3. slip forces
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) {
        if (!in.wheels[i].hit) continue;
        m_slip[i].update(m_suspension[i].linearVelocity(),
                         m_suspension[i].suspensionForce(),
                         m_acceleration[i].angularVelocity(), dt);
    }

    // 4. tire forces (accumulate onto the suspension force)
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) {
        if (!in.wheels[i].hit) continue;
        Vec3 tireForce = m_tire[i].update(in.wheels[i], m_slip[i].slipRatio(),
                                          m_slip[i].slipAngleRad(),
                                          m_suspension[i].suspensionForce());
        wheelForce[i] = wheelForce[i] + tireForce;
    }

    // 5. anti-roll (stabiliser) bars: couple the L/R wheels of an axle by the
    //    difference in suspension travel. The more-compressed side gets pushed
    //    up, resisting body roll. Needs both wheels of the axle grounded.
    if (m_antiroll.isEnabled) {
        const int axle[2][2] = { {0, 1}, {2, 3} };
        const float k[2] = { m_antiroll.stiffnessFront, m_antiroll.stiffnessRear };
        for (int a = 0; a < 2; ++a) {
            int l = axle[a][0], r = axle[a][1];
            if (!in.wheels[l].hit || !in.wheels[r].hit) continue;
            float travelL = std::max(0.0f, m_restLength[l] - m_suspension[l].currentLength());
            float travelR = std::max(0.0f, m_restLength[r] - m_suspension[r].currentLength());
            float f = k[a] * (travelL - travelR);
            wheelForce[l] = wheelForce[l] + Vec3(in.wheels[l].up) * f;
            wheelForce[r] = wheelForce[r] + Vec3(in.wheels[r].up) * (-f);
        }
    }

    // outputs
    for (int i = 0; i < CARSIM_WHEEL_COUNT; ++i) {
        out.applyForce[i] = wheelForce[i].toC();
        out.applyPoint[i] = in.wheels[i].hitPoint;
        out.angularVelocity[i] = m_acceleration[i].angularVelocity();
        out.suspensionForce[i] = m_suspension[i].suspensionForce();
        out.currentLength[i] = m_suspension[i].currentLength();
        out.linearVelocity[i] = m_suspension[i].linearVelocity().toC();
        out.slipAngle[i] = m_slip[i].slipAngle();
        out.lateralAcceleration[i] = m_slip[i].lateralAcceleration();
        out.slipForceLong[i] = m_slip[i].slipRatio();
        out.slipForceLat[i] = m_slip[i].slipAngle();
        out.normalizedTireMagnitude[i] = m_tire[i].normalizedMagnitude();
        out.fx[i] = m_tire[i].fx();
        out.fy[i] = m_tire[i].fy();
    }
}

} // namespace carsim
