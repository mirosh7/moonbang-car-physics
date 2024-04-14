using System.Collections;
using System.Collections.Generic;
using Car.Models.PhysicsModels;
using Car.Models.PhysicsModels.WheelModels;
using UI.Debug;
using UnityEngine;

public class CarPhysicsInfoController : IController
{
    private UICarPhysicsInfo m_carPhysicsInfo;
    private EngineModel m_engineModel;
    private ClutchModel m_clutchModel;
    private GearShiftingModel m_gearShiftingModel;
    private SlipForcesSystemModel m_slipForcesSystemModel;
    private SuspensionForcesSystemModel m_suspensionForcesSystemModel;
    private AccelerationWheelSystemModel m_accelerationWheelSystemModel;

    public CarPhysicsInfoController(UICarPhysicsInfo carPhysicsInfo, EngineModel engineModel, ClutchModel clutchModel, GearShiftingModel gearShiftingModel, SlipForcesSystemModel slipForcesSystemModel, SuspensionForcesSystemModel suspensionForcesSystemModel, AccelerationWheelSystemModel accelerationWheelSystemModel)
    {
        m_carPhysicsInfo = carPhysicsInfo;
        m_engineModel = engineModel;
        m_clutchModel = clutchModel;
        m_gearShiftingModel = gearShiftingModel;
        m_slipForcesSystemModel = slipForcesSystemModel;
        m_suspensionForcesSystemModel = suspensionForcesSystemModel;
        m_accelerationWheelSystemModel = accelerationWheelSystemModel;
        m_carPhysicsInfo.gameObject.SetActive(true);
    }
    
    public void OnUpdate()
    {
        UpdateCarPhysicsInfo();
    }

    private void UpdateCarPhysicsInfo()
    {
        m_carPhysicsInfo.SetEngineRpm(m_engineModel.engineRpm);
        m_carPhysicsInfo.SetClutchLock(m_clutchModel.clutchLock);
        m_carPhysicsInfo.SetClutchTorque(m_clutchModel.clutchTorque);
        m_carPhysicsInfo.SetGearValue(m_gearShiftingModel.currentGear);
        m_carPhysicsInfo.SetSpeedValue(m_engineModel.carSpeed);
        m_carPhysicsInfo.SetSlipForcesValue(m_slipForcesSystemModel.slipForces);
        m_carPhysicsInfo.SetSuspensionForce(m_suspensionForcesSystemModel.suspensionForces);
        m_carPhysicsInfo.SetAngularVelocities(m_accelerationWheelSystemModel.angularVelocities);
        m_carPhysicsInfo.SetSlipAngles(m_slipForcesSystemModel.slipAngles);
        m_carPhysicsInfo.SetLinearVelocities(m_suspensionForcesSystemModel.linearVelocities);
    }
}
