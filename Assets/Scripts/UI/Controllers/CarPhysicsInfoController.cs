using Car;
using UI.Debug;

public class CarPhysicsInfoController : IController
{
    private readonly UICarPhysicsInfo m_carPhysicsInfo;
    private readonly RaceCar m_car;

    public CarPhysicsInfoController(UICarPhysicsInfo carPhysicsInfo, RaceCar car)
    {
        m_carPhysicsInfo = carPhysicsInfo;
        m_car = car;
        m_carPhysicsInfo.gameObject.SetActive(true);
    }

    public void OnUpdate()
    {
        UpdateCarPhysicsInfo();
    }

    private void UpdateCarPhysicsInfo()
    {
        m_carPhysicsInfo.SetEngineRpm(m_car.engineRpm);
        m_carPhysicsInfo.SetClutchLock(m_car.clutchLock);
        m_carPhysicsInfo.SetClutchTorque(m_car.clutchTorque);
        m_carPhysicsInfo.SetGearValue(m_car.currentGear);
        m_carPhysicsInfo.SetSpeedValue(m_car.carSpeed);
        m_carPhysicsInfo.SetSlipForcesValue(m_car.slipForces);
        m_carPhysicsInfo.SetSuspensionForce(m_car.suspensionForces);
        m_carPhysicsInfo.SetAngularVelocities(m_car.angularVelocities);
        m_carPhysicsInfo.SetSlipAngles(m_car.slipAngles);
        m_carPhysicsInfo.SetLinearVelocities(m_car.linearVelocities);
    }
}
