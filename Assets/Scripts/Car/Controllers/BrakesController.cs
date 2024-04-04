using System.Collections.Generic;
using Car.Models;
using Car.Models.WheelModels;

namespace Car.Controllers
{
    public class BrakesController : ICarController
    {
        private InputManager m_inputManager;
        private BrakesModel m_brakesModel;
        private List<AccelerationWheelModel> m_accelerationWheelModels;
        private List<float> m_accelerationVelocities = new List<float>();

        public BrakesController(BrakesModel brakesModel, List<AccelerationWheelModel> accelerationWheelModels)
        {
            m_inputManager = InputManager.instance;
            m_brakesModel = brakesModel;
            m_accelerationWheelModels = accelerationWheelModels;
            
            foreach (var wheelModel in m_accelerationWheelModels)
            {
                m_accelerationVelocities.Add(wheelModel.angularVelocity);
            }
        }

        public void OnUpdate()
        {
            UpdateBrakes();
        }

        private void UpdateBrakes()
        {
            UpdateAccelerationVelocities();
            m_brakesModel.UpdateBrakes(m_inputManager.brakes, m_accelerationVelocities);
        }
        
        private void UpdateAccelerationVelocities()
        {
            for (int i = 0; i < m_accelerationWheelModels.Count; i++)
            {
                m_accelerationVelocities[i] = m_accelerationWheelModels[i].angularVelocity;
            }
        }
        
    }
}
