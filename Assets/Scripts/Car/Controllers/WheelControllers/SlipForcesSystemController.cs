using Car.Models.WheelModels;

namespace Car.Controllers.WheelControllers
{
    public class SlipForcesSystemController : ICarController
    {
        private SlipForcesSystemModel m_slipForcesSystemModel;
        private SuspensionForcesSystemModel m_suspensionForcesSystemModel;
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;

        public SlipForcesSystemController(SlipForcesSystemModel slipForcesSystemModel, SuspensionForcesSystemModel suspensionForcesSystemModel, AccelerationWheelSystemModel accelerationWheelSystemModel)
        {
            m_slipForcesSystemModel = slipForcesSystemModel;
            m_suspensionForcesSystemModel = suspensionForcesSystemModel;
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
        }

        public void OnUpdate()
        {
            UpdateSlipForces();
        }

        private void UpdateSlipForces()
        {
            m_slipForcesSystemModel.UpdateSlipForces(m_suspensionForcesSystemModel.linearVelocities, m_suspensionForcesSystemModel.suspensionForces, m_accelerationWheelSystemModel.angularVelocities);
        }
    }
}
