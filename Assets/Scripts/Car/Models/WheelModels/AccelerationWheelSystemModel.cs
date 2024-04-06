using System.Collections.Generic;
using Car.Models.WheelComponents;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class AccelerationWheelSystemModel
    {
        private List<AccelerationWheelComponent> m_accelerationWheelComponents;
        private List<float> m_angularVelocities = new List<float>();

        public List<float> angularVelocities => m_angularVelocities;
        
        public AccelerationWheelSystemModel(List<AccelerationWheelComponent> accelerationWheelComponents)
        {
            m_accelerationWheelComponents = accelerationWheelComponents;
            
            foreach (var wheelComponent in m_accelerationWheelComponents)
            {
                m_angularVelocities.Add(wheelComponent.angularVelocity);
            }
        }

        public void UpdateWheelsAcceleration(List<bool> wheelHits, float[] brakeTorques, float[] outputTorques, List<float> fxs)
        {
            for (int i = 0; i < m_accelerationWheelComponents.Count; i++)
            {
                if (!wheelHits[i])
                {
                    return;
                }
                
                var driveTorque = outputTorques[i];
                var brakeTorque = brakeTorques[i];
                m_accelerationWheelComponents[i].UpdateWheelAcceleration(fxs[i], driveTorque, brakeTorque);
                m_angularVelocities[i] = m_accelerationWheelComponents[i].angularVelocity;
            }
        }
    }
}
