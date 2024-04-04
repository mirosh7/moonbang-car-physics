using System.Collections.Generic;
using Car.Models;
using Car.Models.WheelModels;

namespace Car.Controllers
{
    public class DifferentialController : ICarController
    {
        private DifferentialModel m_differentialModel;
        private GearShiftingModel m_gearShiftingModel;
        private ClutchModel m_clutchModel;
        private List<AccelerationWheelModel> m_accelerationWheelModels;
        private List<float> m_accelerationVelocities = new List<float>();

        public DifferentialController(DifferentialModel differentialModel, GearShiftingModel gearShiftingModel, ClutchModel clutchModel, List<AccelerationWheelModel> accelerationWheelModels)
        {
            m_differentialModel = differentialModel;
            m_gearShiftingModel = gearShiftingModel;
            m_clutchModel = clutchModel;
            m_accelerationWheelModels = accelerationWheelModels;
            
            foreach (var wheelModel in m_accelerationWheelModels)
            {
                m_accelerationVelocities.Add(wheelModel.angularVelocity);
            }
        }

        public void OnUpdate()
        {
            UpdateDifferential();
        }

        private void UpdateDifferential()
        {
            UpdateAccelerationVelocities();
            var gearBoxTorque = m_gearShiftingModel.GetOutputTorque(m_clutchModel.clutchTorque);
            m_differentialModel.UpdateOutputTorque(gearBoxTorque, m_accelerationVelocities);
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
