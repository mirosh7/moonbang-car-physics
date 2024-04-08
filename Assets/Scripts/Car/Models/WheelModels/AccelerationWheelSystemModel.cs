using System.Collections.Generic;
using Car.Data;
using Car.Models.WheelComponents;
using UnityEngine;

namespace Car.Models.WheelModels
{
    public class AccelerationWheelSystemModel
    {
        private List<AccelerationWheelComponent> m_accelerationWheelComponents = new List<AccelerationWheelComponent>();
        private List<float> m_angularVelocities = new List<float>();

        public List<float> angularVelocities => m_angularVelocities;
        
        public AccelerationWheelSystemModel(List<CarDesc.WheelInfo> wheelInfos)
        {
            foreach (var wheelInfo in wheelInfos)
            {
                m_accelerationWheelComponents.Add(new AccelerationWheelComponent(wheelInfo));
            }

            foreach (var wheelComponent in m_accelerationWheelComponents)
            {
                m_angularVelocities.Add(wheelComponent.angularVelocity);
            }
        }

        public void UpdateWheelsAcceleration(List<bool> wheelHits, List<float> brakeTorques, List<float> outputTorques, List<float> fxs)
        {
            for (int i = 0; i < m_accelerationWheelComponents.Count; i++)
            {
                if (!wheelHits[i] || i > 1)
                {
                    continue;
                }
                
                var driveTorque = outputTorques[i];
                var brakeTorque = brakeTorques[i];
                m_accelerationWheelComponents[i].UpdateWheelAcceleration(fxs[i], driveTorque, brakeTorque);
                m_angularVelocities[i] = m_accelerationWheelComponents[i].angularVelocity;
            }
        }
    }
}
