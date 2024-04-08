using System.Collections.Generic;
using Car.Models;
using Car.Models.WheelModels;

namespace Car.Controllers
{
    public class BrakesController : ICarController
    {
        private InputManager m_inputManager;
        private BrakesModel m_brakesModel;
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;
        private List<float> m_accelerationVelocities = new List<float>();

        public BrakesController(BrakesModel brakesModel, AccelerationWheelSystemModel accelerationWheelSystemModel, InputManager inputManager)
        {
            m_inputManager = inputManager;
            m_brakesModel = brakesModel;
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
        }

        public void OnUpdate()
        {
            UpdateBrakes();
        }

        private void UpdateBrakes()
        {
            m_brakesModel.UpdateBrakes(m_inputManager.brakes, m_accelerationWheelSystemModel.angularVelocities);
        }
    }
}
