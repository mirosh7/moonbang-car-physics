/*
 * api.cpp - the exported C ABI. Thin: validates handles and forwards to the
 * CarSim orchestrator.
 */
/* CARPHYSICS_BUILD_DLL is defined by the build (vcxproj / CMake) so the API
 * is exported with __declspec(dllexport). */
#include <new>
#include "car_physics.h"
#include "car_sim.h"

using carsim::CarSim;

extern "C" {

CARPHYSICS_API CP_Handle CARPHYSICS_CALL carsim_create(const CP_CarConfig* config) {
    if (!config) return nullptr;
    CarSim* sim = new (std::nothrow) CarSim();
    if (!sim) return nullptr;
    if (!sim->init(*config)) {
        delete sim;
        return nullptr;
    }
    return static_cast<CP_Handle>(sim);
}

CARPHYSICS_API void CARPHYSICS_CALL carsim_destroy(CP_Handle handle) {
    delete static_cast<CarSim*>(handle);
}

CARPHYSICS_API void CARPHYSICS_CALL carsim_update_drivetrain(
    CP_Handle handle, const CP_DrivetrainInput* in, CP_DrivetrainOutput* out) {
    if (!handle || !in || !out) return;
    static_cast<CarSim*>(handle)->updateDrivetrain(*in, *out);
}

CARPHYSICS_API void CARPHYSICS_CALL carsim_update_wheels(
    CP_Handle handle, const CP_WheelInput* in, CP_WheelOutput* out) {
    if (!handle || !in || !out) return;
    static_cast<CarSim*>(handle)->updateWheels(*in, *out);
}

CARPHYSICS_API const char* CARPHYSICS_CALL carsim_version(void) {
    return "1.0.0";
}

} // extern "C"
