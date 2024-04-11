using Car.Models.PhysicsModels.WheelModels;

namespace Car.Controllers.PhysicsControllers.WheelControllers
{
    public class SlipForcesSystemController : ICarController
    {
        private SlipForcesSystemModel m_slipForcesSystemModel;
        private SuspensionForcesSystemModel m_suspensionForcesSystemModel;
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;
        private RaycastWheelSystemModel m_raycastWheelSystemModel;

        public SlipForcesSystemController(SlipForcesSystemModel slipForcesSystemModel, SuspensionForcesSystemModel suspensionForcesSystemModel, AccelerationWheelSystemModel accelerationWheelSystemModel, RaycastWheelSystemModel raycastWheelSystemModel)
        {
            m_slipForcesSystemModel = slipForcesSystemModel;
            m_suspensionForcesSystemModel = suspensionForcesSystemModel;
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
            m_raycastWheelSystemModel = raycastWheelSystemModel;
        }

        public void OnCarUpdate()
        {
            UpdateSlipForces();
        }

        private void UpdateSlipForces()
        {
            m_slipForcesSystemModel.UpdateSlipForces(m_suspensionForcesSystemModel.linearVelocities, m_suspensionForcesSystemModel.suspensionForces, m_accelerationWheelSystemModel.angularVelocities, m_raycastWheelSystemModel.wheelHitStates);
        }
    }
}
