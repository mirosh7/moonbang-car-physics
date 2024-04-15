using Car.Controllers.PhysicsControllers.WheelControllers;
using Car.Models.PhysicsModels;
using Car.Models.PhysicsModels.WheelModels;

namespace Car.Controllers.PhysicsControllers
{
    public class SteeringController : ICarController
    {
        private SteeringModel m_steeringModel;
        private SlipForcesSystemModel m_slipForcesSystemModel;
        private InputManager m_inputManager;

        public SteeringController(SteeringModel steeringModel, InputManager inputManager, SlipForcesSystemModel slipForcesSystemModel)
        {
            m_steeringModel = steeringModel;
            m_inputManager = inputManager;
            m_slipForcesSystemModel = slipForcesSystemModel;
        }

        public void OnCarUpdate()
        {
            UpdateSteering();
        }

        private void UpdateSteering()
        {
            m_steeringModel.UpdateAckermann(m_inputManager.steering, m_slipForcesSystemModel.slipAngles);
        }
    }
}
