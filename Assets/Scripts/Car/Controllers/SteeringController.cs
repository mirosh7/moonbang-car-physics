using Car.Models;

namespace Car.Controllers
{
    public class SteeringController : ICarController
    {
        private SteeringModel m_steeringModel;
        private InputManager m_inputManager;

        public SteeringController(SteeringModel steeringModel, InputManager inputManager)
        {
            m_steeringModel = steeringModel;
            m_inputManager = inputManager;
        }

        public void OnUpdate()
        {
            UpdateSteering();
        }

        private void UpdateSteering()
        {
            m_steeringModel.UpdateAckermann(m_inputManager.steering);
        }
    }
}
