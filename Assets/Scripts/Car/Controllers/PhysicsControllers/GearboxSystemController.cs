using Car.Models.PhysicsModels;

namespace Car.Controllers.PhysicsControllers
{
    public class GearboxSystemController : ICarController
    {
        private InputManager m_inputManager;
        private GearShiftingModel m_gearShiftingModel;
        private ClutchModel m_clutchModel;
        private DifferentialModel m_differentialModel;
        private EngineModel m_engineModel;
        
        public GearboxSystemController(GearShiftingModel gearShiftingModel, ClutchModel clutchModel, DifferentialModel differentialModel, EngineModel engineModel, InputManager inputManager)
        {
            m_inputManager = inputManager;
            m_gearShiftingModel = gearShiftingModel;
            m_clutchModel = clutchModel;
            m_differentialModel = differentialModel;
            m_engineModel = engineModel;

            m_inputManager.gearUp -= m_gearShiftingModel.ShiftUp;
            m_inputManager.gearDown -= m_gearShiftingModel.ShiftDown;
            m_inputManager.gearUp += m_gearShiftingModel.ShiftUp;
            m_inputManager.gearDown += m_gearShiftingModel.ShiftDown;
        }

        public void OnUpdate()
        {
            UpdateGearboxSystem();
        }

        private void UpdateGearboxSystem()
        {
            var diffInputShaftVelocity = m_differentialModel.GetInputShaftVelocity();
            var gearBoxInputShaftVelocity = m_gearShiftingModel.GetInputShaftVelocity(diffInputShaftVelocity);
            m_clutchModel.UpdateClutchTorque(m_engineModel.engineAngularVelocity, m_gearShiftingModel.currentGearRatio, gearBoxInputShaftVelocity);
        }
    }
}
