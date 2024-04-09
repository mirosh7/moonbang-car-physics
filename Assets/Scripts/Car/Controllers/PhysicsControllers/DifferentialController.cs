using Car.Models.PhysicsModels;
using Car.Models.PhysicsModels.WheelModels;

namespace Car.Controllers.PhysicsControllers
{
    public class DifferentialController : ICarController
    {
        private DifferentialModel m_differentialModel;
        private GearShiftingModel m_gearShiftingModel;
        private ClutchModel m_clutchModel;
        private AccelerationWheelSystemModel m_accelerationWheelSystemModel;

        public DifferentialController(DifferentialModel differentialModel, GearShiftingModel gearShiftingModel, ClutchModel clutchModel, AccelerationWheelSystemModel accelerationWheelSystemModel)
        {
            m_differentialModel = differentialModel;
            m_gearShiftingModel = gearShiftingModel;
            m_clutchModel = clutchModel;
            m_accelerationWheelSystemModel = accelerationWheelSystemModel;
        }

        public void OnUpdate()
        {
            UpdateDifferential();
        }

        private void UpdateDifferential()
        {
            var gearBoxTorque = m_gearShiftingModel.GetOutputTorque(m_clutchModel.clutchTorque);
            m_differentialModel.UpdateOutputTorque(gearBoxTorque, m_accelerationWheelSystemModel.angularVelocities);
        }
    }
}
