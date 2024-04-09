using Car.Models.PhysicsModels;
using Car.Models.PhysicsModels.WheelModels;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class AccelerationWheelSystemController : ICarController
    {
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;
        private BrakesModel m_brakesModel;
        private DifferentialModel m_differentialModel;
        private TireForceSystemModel m_tireForceSystemModel;
        private RaycastWheelSystemModel m_raycastWheelSystemModel;
        
        public AccelerationWheelSystemController(AccelerationWheelSystemModel accelerationWheelSystemModel, BrakesModel brakesModel, DifferentialModel differentialModel, TireForceSystemModel tireForceSystemModel, RaycastWheelSystemModel raycastWheelSystemModel)
        {
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
            m_brakesModel = brakesModel;
            m_differentialModel = differentialModel;
            m_tireForceSystemModel = tireForceSystemModel;
            m_raycastWheelSystemModel = raycastWheelSystemModel;
        }

        public void OnUpdate()
        {
            UpdateWheelsAcceleration();
        }

        private void UpdateWheelsAcceleration()
        {
            m_accelerationWheelSystemModel.UpdateWheelsAcceleration(m_raycastWheelSystemModel.wheelHitStates, m_brakesModel.brakeTorque, m_differentialModel.outputTorque, m_tireForceSystemModel.fxVelocities);
        }
    }
}
