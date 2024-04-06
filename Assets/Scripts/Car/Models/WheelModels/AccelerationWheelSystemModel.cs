using System.Collections.Generic;

namespace Car.Models.WheelModels
{
    public class AccelerationWheelSystemModel
    {
        private List<AccelerationWheelComponent> m_accelerationWheelComponents;
        private DifferentialModel m_differentialModel;
        private TireForceModel m_tireForceModel;
        private BrakesModel m_brakesModel;
        private List<float> m_angularVelocities = new List<float>();

        public List<float> angularVelocities => m_angularVelocities;
        
        public AccelerationWheelSystemModel(List<AccelerationWheelComponent> accelerationWheelComponents, DifferentialModel differentialModel, TireForceModel tireForceModel, BrakesModel brakesModel)
        {
            m_accelerationWheelComponents = accelerationWheelComponents;
            m_differentialModel = differentialModel;
            m_tireForceModel = tireForceModel;
            m_brakesModel = brakesModel;
            
            foreach (var wheelComponent in m_accelerationWheelComponents)
            {
                m_angularVelocities.Add(wheelComponent.angularVelocity);
            }
        }

        public void UpdateWheelsAcceleration()
        {
            for (int i = 0; i < m_accelerationWheelComponents.Count; i++)
            {
                var driveTorque = m_differentialModel.outputTorque[i];
                var brakeTorque = m_brakesModel.brakeTorque[i];
                m_accelerationWheelComponents[i].UpdateWheelAcceleration(m_tireForceModel.fx, driveTorque, brakeTorque);
                m_angularVelocities[i] = m_accelerationWheelComponents[i].angularVelocity;
            }
        }
    }
}
