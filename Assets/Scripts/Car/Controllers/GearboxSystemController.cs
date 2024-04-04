using Car.Models;

namespace Car.Controllers
{
    public class GearboxSystemController : ICarController
    {
        private GearShiftingModel m_gearShiftingModel;
        private ClutchModel m_clutchModel;
        private DifferentialModel m_differentialModel;
        private EngineModel m_engineModel;
        
        public GearboxSystemController(GearShiftingModel gearShiftingModel, ClutchModel clutchModel, DifferentialModel differentialModel, EngineModel engineModel)
        {
            m_gearShiftingModel = gearShiftingModel;
            m_clutchModel = clutchModel;
            m_differentialModel = differentialModel;
            m_engineModel = engineModel;
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
