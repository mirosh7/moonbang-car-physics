using Car.Models;

namespace Car.Controllers
{
    public class EngineController : ICarController
    {
        private InputManager m_inputManager;
        private EngineModel m_engineModel;
        private ClutchModel m_clutchModel;
        private GearShiftingModel m_gearShiftingModel;

        public EngineController(EngineModel engineModel, ClutchModel clutchModel, GearShiftingModel gearShiftingModel, InputManager inputManager)
        {
            m_inputManager = inputManager;
            m_engineModel = engineModel;
            m_clutchModel = clutchModel;
            m_gearShiftingModel = gearShiftingModel;
        }

        public void OnUpdate()
        {
            UpdateEngine();
        }

        private void UpdateEngine()
        {
            m_engineModel.UpdateEngine(m_inputManager.acceleration, m_clutchModel.clutchTorque, m_gearShiftingModel.currentGear);
        }
        
    }
}
