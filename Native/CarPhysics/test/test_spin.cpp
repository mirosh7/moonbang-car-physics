/*
 * test_spin.cpp - standalone reproduction of the "wheel does not spin back up
 * after the brake is released" scenario. Drives the real SlipWheel / TireWheel /
 * AccelerationWheel models the same way car_sim.cpp does (one-frame fx lag).
 *
 * Scenario: car rolls forward at a constant ground speed. We brake hard for a
 * while (wheel locks to 0), then release. We print the wheel angular velocity
 * each phase. A correct model should let the wheel spin back up to the rolling
 * speed (omega*R ~= v) after release.
 */
#include "../src/models.h"
#include <cstdio>
#include <cmath>

using namespace carsim;

static CP_WheelInfo makeWheel() {
    CP_WheelInfo w{};
    w.restLength = 0.6f;
    w.suspensionStiffness = 50000.0f;
    w.damperStiffness = 1500.0f;
    w.slipAnglePeak = 15.0f;
    w.longitudinalCoeff = 1.0f;
    w.lateralCoeff = 1.0f;
    w.wheelRadius = 0.33f;
    w.wheelMass = 15.0f;
    w.longFrictionCoeff = 1.0f;
    w.relaxationLength = 0.05f;
    w.longSlipPeak = 0.12f;
    w.pacejkaShapeLong = 1.65f;
    w.pacejkaCurveLong = 0.96f;
    w.pacejkaShapeLat = 1.35f;
    w.pacejkaCurveLat = 0.96f;
    return w;
}

int main() {
    CP_WheelInfo info = makeWheel();
    SlipWheel slip; slip.init(info);
    TireWheel tire; tire.init(info);
    AccelerationWheel acc; acc.init(info);

    const float dt = 0.02f;
    const float v = 10.0f;            // ground forward speed, m/s (constant)
    const float Fz = 3500.0f;         // normal load, N
    const float rollOmega = v / info.wheelRadius;

    // Build a wheel state with forward = +Z, contact straight down.
    CP_WheelState w{};
    w.forward = { 0, 0, 1 };
    w.right   = { 1, 0, 0 };
    w.up      = { 0, 1, 0 };
    w.hitNormal = { 0, 1, 0 };
    Vec3 localVel(0.0f, 0.0f, v);     // pure forward, no lateral

    printf("rolling omega target = %.2f rad/s\n", rollOmega);
    printf("%-6s %-7s %-9s %-9s %-9s\n", "tick", "phase", "omega", "slipR", "fx");

    auto step = [&](int tick, float brake, const char* phase) {
        // order mirrors car_sim.cpp: acceleration consumes last tick's fx
        acc.update(tire.fx(), 0.0f /*drive*/, brake, localVel.z, Fz, dt);
        slip.update(localVel, Fz, acc.angularVelocity(), dt);
        tire.update(w, slip.slipRatio(), slip.slipAngleRad(), Fz);
        printf("%-6d %-7s %-9.3f %-9.3f %-9.1f\n",
               tick, phase, acc.angularVelocity(), slip.slipRatio(), tire.fx());
    };

    int tick = 0;
    for (int i = 0; i < 5; ++i)  step(tick++, 0.0f,    "roll");   // settle rolling
    for (int i = 0; i < 10; ++i) step(tick++, 2000.0f, "BRAKE");  // lock the wheel
    for (int i = 0; i < 20; ++i) step(tick++, 0.0f,    "release");// should spin back up

    printf("\nfinal omega = %.3f rad/s (target %.3f)\n",
           acc.angularVelocity(), rollOmega);
    return 0;
}
