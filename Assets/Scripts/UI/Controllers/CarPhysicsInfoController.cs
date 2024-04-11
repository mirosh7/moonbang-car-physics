using System.Collections;
using System.Collections.Generic;
using Car.Models.PhysicsModels;
using UI.Debug;
using UnityEngine;

public class CarPhysicsInfoController : IController
{
    private UICarPhysicsInfo m_carPhysicsInfo;
    private EngineModel m_engineModel;
    private ClutchModel m_clutchModel;
    private GearShiftingModel m_gearShiftingModel;

    public CarPhysicsInfoController(UICarPhysicsInfo carPhysicsInfo, EngineModel engineModel, ClutchModel clutchModel, GearShiftingModel gearShiftingModel)
    {
        m_carPhysicsInfo = carPhysicsInfo;
        m_engineModel = engineModel;
        m_clutchModel = clutchModel;
        m_gearShiftingModel = gearShiftingModel;
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
    }
}
