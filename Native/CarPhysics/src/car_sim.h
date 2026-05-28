/*
 * car_sim.h - top-level orchestrator that wires the ported models together
 * in the same order the original Unity controllers ran them.
 */
#ifndef CARSIM_CAR_SIM_H
#define CARSIM_CAR_SIM_H

#include "models.h"
#include "car_physics.h"

namespace carsim {

class CarSim {
public:
    bool init(const CP_CarConfig& config);

    void updateDrivetrain(const CP_DrivetrainInput& in, CP_DrivetrainOutput& out);
    void updateWheels(const CP_WheelInput& in, CP_WheelOutput& out);

private:
    EngineModel       m_engine;
    GearboxModel      m_gearbox;
    ClutchModel       m_clutch;
    DifferentialModel m_differential;
    BrakesModel       m_brakes;
    SteeringModel     m_steering;

    SuspensionWheel   m_suspension[CARSIM_WHEEL_COUNT];
    AccelerationWheel m_acceleration[CARSIM_WHEEL_COUNT];
    SlipWheel         m_slip[CARSIM_WHEEL_COUNT];
    TireWheel         m_tire[CARSIM_WHEEL_COUNT];
    VisualWheel       m_visual[CARSIM_WHEEL_COUNT];

    /* steer angles produced by the drivetrain phase, consumed by visuals */
    float m_steerAngles[CARSIM_WHEEL_COUNT] = { 0 };
};

} // namespace carsim

#endif // CARSIM_CAR_SIM_H
