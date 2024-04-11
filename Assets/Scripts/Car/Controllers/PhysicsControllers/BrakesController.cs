using Car.Models.PhysicsModels;
using Car.Models.PhysicsModels.WheelModels;

namespace Car.Controllers.PhysicsControllers
{
    public class BrakesController : ICarController
    {
        private InputManager m_inputManager;
        private BrakesModel m_brakesModel;
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;

        public BrakesController(BrakesModel brakesModel, AccelerationWheelSystemModel accelerationWheelSystemModel, InputManager inputManager)
        {
            m_inputManager = inputManager;
            m_brakesModel = brakesModel;
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
        }

        public void OnCarUpdate()
        {
            UpdateBrakes();
        }

        private void UpdateBrakes()
        {
            m_brakesModel.UpdateBrakes(m_inputManager.brakes, m_accelerationWheelSystemModel.angularVelocities);
        }
    }
}
